using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	#region Cannot Delete Event

	public class OnCannotDeleteEventArgs : EventArgs
	{
		public OnCannotDeleteEventArgs(string error, Charge charge)
		{
			this.Error = error;
			this.Charge = charge;
		}

		public readonly string Error;
		public readonly Charge Charge;
	}

	public delegate void OnCannotDeleteHandler(object sender, OnCannotDeleteEventArgs e);

	#endregion

	#region Pair Struct

	internal struct Pair
	{
		public ZGuid Creditor;
		public ZString InvNumber;

		public Pair(ZGuid creditor, ZString invNumber)
		{
			this.Creditor = creditor;
			this.InvNumber = invNumber;
		}
	}

	#endregion

	public partial class ChargeCollection : BusinessObjectCollection<Charge>
	{
		public ChargeCollection(Job parentJob)
			: base(parentJob.Factory, FilterQuery)
		{
			IsManagedForDataRefresh = true;
			this.ParentJob = parentJob;
			HasChangesChanged += ResetParentScreeningStatus;
		}

		public readonly Job ParentJob;

		#region For Autopopulate

		ZBool fLoadedFromAutopopulate;
		public ZBool LoadedFromAutopopulate
		{
			get { return fLoadedFromAutopopulate; }
			set { fLoadedFromAutopopulate = value; }
		}

		#endregion

		#region Overrides

		protected override bool AllowNewCore
		{
			get
			{
				if (LoadedFromAutopopulate)
				{
					return false;
				}
				else
				{
					return base.AllowNewCore;
				}
			}
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();

			biggestSequenceNumberFromDb = null;
			unpostedSequenceNumbersFromDb = null;

			if (!IsWarehousePeriodicBillingToSkipSequence)
			{
				if (ContainsDuplicateDisplaySequence)
				{
					ReSequenceUnPostedCharges(SortByPostedCostJobNumberAndDisplaySequence);
				}

				foreach (Charge charge in this)
				{
					charge.JR_DisplaySequenceInfo.ValueChanged -= OnDisplaySequenceChanged;
					charge.JR_DisplaySequenceInfo.ValueChanged += OnDisplaySequenceChanged;
				}
			}
			else
			{
				foreach (Charge charge in this)
				{
					charge.JR_DisplaySequenceInfo.ValueChanged -= OnDisplaySequenceChanged;
				}
			}
			if (this.Any())
			{
				var firstCharge = (Charge)this.FirstOrDefault();
				var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
				infoCollector.AddInfoWhenAllowed(ParentJob.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLoadFirstInDatabase,
						() => FormattableString.Invariant($@"JR_GC: {firstCharge.JR_GC}, JR_JH: {firstCharge.JR_JH}, CurrentCompany: {GlbCompany.CurrentCompany.PK}({GlbCompany.CurrentCompany.GC_Code}), job.IsInDatabase: {firstCharge.IsInDatabase}, StackTrace ->\r\n {System.Environment.StackTrace}"),
						useNeverClearedInfo: true);
			}
		}

		public override void Load()
		{
			using (IDisposable suspender = GetCheckDuplicateDisplaySequenceSuspender())
			{
				base.Load();
			}
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			base.Remove(elementToRemove);
			if (Factory.HasContext(BusinessContext.DeletingJobCharge))
			{
				var hashCode = GetHashCode();
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(elementToRemove.PK,
					CriticalValidationInfoCollectorServiceKeyType.ChargeCollectionRemoveMethodInfo, () =>
					{
						if (Contains(elementToRemove))
						{
							return string.Format(CultureInfo.InvariantCulture, (NoResString)"Hash Code = {0}, Contains?InCollection", hashCode);
						}
						return string.Format(CultureInfo.InvariantCulture, (NoResString)"Hash Code = {0}, Contains?NotInCollection", hashCode);
					});
			}
		}

		public FunctionalitySuspender UpdateTotalSuspender => updateTotalSuspender ?? (updateTotalSuspender = GetSuspender());

		FunctionalitySuspender GetSuspender()
		{
			return new FunctionalitySuspender(
						onResumeAction: () =>
						{
							FunctionalitySuspender.ResumeCollection(ChargeTotalSuspenders);

							var totalProviders = this.OfType<BaseCharge>()
													.Select(c => c.TotalProvider)
													.WhereNotNull()
													.Distinct();
							foreach (ITotalProvider tp in totalProviders)
							{
								tp.UpdateTotals();
							}

							ChargeTotalSuspenders.Clear();
						});
		}
		FunctionalitySuspender updateTotalSuspender;

		List<IDisposable> ChargeTotalSuspenders => chargeTotalSuspenders ?? (chargeTotalSuspenders = new List<IDisposable>());
		List<IDisposable> chargeTotalSuspenders;

		#endregion

		#region Additional Job Charges

		public List<Charge> GetMatchingUnapportionedChargesFromThisJobOnly(ZGuid chargeCodePK, ZString invoiceNumToStartWith, ZString additionalInvoiceNumToStartWith)
		{
			List<Charge> result = new List<Charge>();

			foreach (Charge existingCharge in this)
			{
				if (existingCharge.JR_JH == ParentJob.PK &&
					existingCharge.JR_AC == chargeCodePK &&
					!existingCharge.JR_E6.IsValid &&
					(
					existingCharge.JR_APInvoiceNum.IsEmpty || existingCharge.JR_APInvoiceNum.StartsWith(invoiceNumToStartWith, StringComparison.OrdinalIgnoreCase)
					|| (!additionalInvoiceNumToStartWith.IsEmpty && existingCharge.JR_APInvoiceNum.StartsWith(additionalInvoiceNumToStartWith, StringComparison.OrdinalIgnoreCase))
					)
					)
				{
					result.Add(existingCharge);
				}
			}

			return result;
		}

		/// <summary>
		/// Includes all charges from the additional jobs passed in.
		/// </summary>
		/// <param name="Jobs"></param>
		internal bool IncludeChargesFromJobs(params ZGuid[] jobsPKs)
		{
			if (jobsPKs.Length > 0)
			{
				var newDistinctValues = jobsPKs.Distinct().ToArray();
				var currentMinusNewValues = AdditionalJobsToLoadChargesFor.Except(newDistinctValues);
				var newMinusCurrentValues = newDistinctValues.Except(AdditionalJobsToLoadChargesFor);
				var newValuesDifferentToCurrent = currentMinusNewValues.Any() || newMinusCurrentValues.Any();
				if (newValuesDifferentToCurrent)
				{
					AdditionalJobsToLoadChargesFor = newDistinctValues;
					return true;
				}
			}

			return false;
		}

		internal void ResetAdditionalJobs()
		{
			AdditionalJobsToLoadChargesFor = Array.Empty<ZGuid>();
		}

#if DEBUG
		public void IncludeChargesFromJobs_ForTestOnly(params ZGuid[] jobsPKs)
		{
			IncludeChargesFromJobs(jobsPKs);
		}
#endif

		void ResetParentScreeningStatus(object sender, EventArgs e)
		{
			ObjectFactory.Get<IResetParentScreeningStatusHelperForCharges>().ResetJobParentScreeningStatus(HasChanges, ParentJob.Parent, this.Cast<Charge>());
		}

		void ReSequenceUnPostedCharges(Action sortFunction)
		{
			if (sortFunction != null)
			{
				sortFunction();
			}
			short sequence;
			unchecked
			{
				sequence = (short)((short)GetBiggestSequenceNumberForPostedCharges() + 1);
			}

			using (ParentJob.CheckForChargesDisplaySequenceDuplicatesSuspender.GetSuspender())
			{
				foreach (Charge charge in this)
				{
					if (!charge.JR_IsRevenuePosted)
					{
						using (charge.SuspendSettingHasChanges())
						{
							unchecked
							{
								charge.JR_DisplaySequence = (short)(sequence < 0 ? sequence - short.MinValue + 1 : sequence);
								sequence++;
							}
						}
					}
				}
			}
		}

		void SortByPostedCostJobNumberAndDisplaySequence()
		{
			Sort(PostedCostSequenceSort);
		}

		Comparison<Charge> PostedCostSequenceSort = (x, y) =>
		{
			string postedCost_X = Convert.ToInt16(!x.JR_IsCostPosted).ToString();
			string postedCost_Y = Convert.ToInt16(!y.JR_IsCostPosted).ToString();

			string jobNumber_X = x.Job != null ? x.Job.JH_JobNum : new ZString("0");
			string jobNumber_Y = y.Job != null ? y.Job.JH_JobNum : new ZString("0");

			string isinDatabase_X = Convert.ToInt16(!x.IsInDatabase).ToString();
			string isinDatabase_Y = Convert.ToInt16(!y.IsInDatabase).ToString();

			string sortKey_X = isinDatabase_X + postedCost_X + jobNumber_X + x.JR_DisplaySequence.ToString("00000");
			string sortKey_Y = isinDatabase_Y + postedCost_Y + jobNumber_Y + y.JR_DisplaySequence.ToString("00000");

			int result = string.CompareOrdinal(sortKey_X, sortKey_Y);
			return result;
		};

		ZGuid[] AdditionalJobsToLoadChargesFor = Array.Empty<ZGuid>();

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();

			List<ZGuid> jobHeaderPKs = new List<ZGuid>();
			jobHeaderPKs.Add(ParentJob.PK);

			if (AdditionalJobsToLoadChargesFor.Any())
			{
				jobHeaderPKs.AddRange(AdditionalJobsToLoadChargesFor);
			}

			var childJobs = ParentJob.ChildJobPKs;
			if (childJobs.Any())
			{
				jobHeaderPKs.AddRange(childJobs);
			}

			query.AddToFilter(JobChargeSchema.JR_JH, jobHeaderPKs);

			return query;
		}

		protected override bool FetchOnlyFromLocalCache
		{
			get { return !ParentJob.IsInDatabase && !AdditionalJobsToLoadChargesFor.Any(); }
		}

		#endregion

		#region Default Values and Adding

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			Charge childCharge = (Charge)child;
			using (childCharge.GetValidationSuspender())
			{
				childCharge.JR_JH = ParentJob.PK;
				childCharge.JR_GB = ParentJob.JH_GB;
				childCharge.JR_GE = ParentJob.JH_GE;
				if (!IsWarehousePeriodicBillingToSkipSequence)
				{
					unchecked
					{
						childCharge.JR_DisplaySequence = GetUniqueSequenceNumberForUnpostedCharges((short)((short)GetBiggestSequenceNumber() + 1));
					}
				}
			}
		}

		protected bool ContainsChargesFromChildJob
		{
			get
			{
				foreach (Charge charge in this)
				{
					if (charge != null && !charge.IsDeleted && charge.Job != null && ParentJob != null
						&& charge.Job.PK != ParentJob.PK && charge.Job.JH_JH_ParentJob == ParentJob.PK)
					{
						return true;
					}
				}
				return false;
			}
		}

		class CheckDuplicateDisplaySequenceSuspender : IDisposable
		{
			internal CheckDuplicateDisplaySequenceSuspender(ChargeCollection parent)
			{
				this.parent = parent;
				this.parent.checkDuplicateDisplaySequenceSuspendCount++;
			}

			void IDisposable.Dispose()
			{
				parent.checkDuplicateDisplaySequenceSuspendCount--;
			}

			readonly ChargeCollection parent;
		}

		public IDisposable GetCheckDuplicateDisplaySequenceSuspender()
		{
			return new CheckDuplicateDisplaySequenceSuspender(this);
		}

		protected bool IsCheckDuplicateDisplaySequenceSuspended
		{
			get { return checkDuplicateDisplaySequenceSuspendCount > 0; }
		}
		int checkDuplicateDisplaySequenceSuspendCount;

		protected virtual bool ContainsDuplicateDisplaySequence
		{
			get
			{
				var result = false;
				var validDisplaySequences = this.Cast<Charge>().Where(x => !x.IsDeleted && !x.IsDeleting).Select(x => x.JR_DisplaySequence);
				if (validDisplaySequences.Any())
				{
					var numberHash = new HashSet<ZShort>();
					foreach (var number in validDisplaySequences)
					{
						if (!numberHash.Add(number))
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		#region OnAdded

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			ResequenceIfNecessary(bizOAdded);
			if (UpdateTotalSuspender.IsSuspended && bizOAdded is BaseCharge charge)
			{
				ChargeTotalSuspenders.Add(charge.UpdateTotalSuspender.GetSuspender());
			}
		}

		protected override void OnRemoved(BusinessObject bizORemoved)
		{
			base.OnRemoved(bizORemoved);
			Charge childCharge = (Charge)bizORemoved;
			childCharge.JR_DisplaySequenceInfo.ValueChanged -= OnDisplaySequenceChanged;
		}

		protected void ResequenceIfNecessary(BusinessObject bizOAdded)
		{
			if (IsWarehousePeriodicBillingToSkipSequence)
			{
				((Charge)bizOAdded).JR_DisplaySequenceInfo.ValueChanged -= OnDisplaySequenceChanged;
			}
			else if (!IsCheckDuplicateDisplaySequenceSuspended)
			{
				Charge childCharge = (Charge)bizOAdded;
				if (childCharge.IsInDatabase && (ContainsChargesFromChildJob || (ParentJob?.IsWarehousePeriodicBilling ?? false)) && ContainsDuplicateDisplaySequence)
				{
					using (ParentJob.CheckForChargesDisplaySequenceDuplicatesSuspender.GetSuspender())
					{
						ReSequenceUnPostedCharges(SortByPostedCostJobNumberAndDisplaySequence);
					}
				}
				childCharge.JR_DisplaySequenceInfo.ValueChanged -= OnDisplaySequenceChanged;
				childCharge.JR_DisplaySequenceInfo.ValueChanged += OnDisplaySequenceChanged;
				OnDisplaySequenceChanged(this, EventArgs.Empty);
			}
		}

		#region OnDisplaySequenceChanged

		public EventHandler DisplaySequenceChanged;

		void OnDisplaySequenceChanged(object sender, EventArgs args)
		{
			DisplaySequenceChanged?.Invoke(sender, args);
		}

		#endregion

		#endregion

		#endregion

		#region Display sequence

		bool IsWarehousePeriodicBillingToSkipSequence
			=> (ParentJob?.IsWarehousePeriodicBilling ?? false) && AccountingConfigurationRegistry.Instance.DisableDisplaySequenceCalculationForWarehousePeriodicInvoicingJobTypes.Value;

		ZShort BiggestSequenceNumberFromDb
		{
			get
			{
				if (!biggestSequenceNumberFromDb.HasValue)
				{
					biggestSequenceNumberFromDb = (short)GetBiggestSequenceNumberFromDb();
				}
				return biggestSequenceNumberFromDb.Value;
			}
		}

		ZShort? biggestSequenceNumberFromDb;

		List<ZShort> UnpostedSequenceNumbersFromDb
		{
			get
			{
				if (unpostedSequenceNumbersFromDb == null)
				{
					unpostedSequenceNumbersFromDb = GetUnpostedSequenceNumbersFromDb();
					biggestSequenceNumberFromDb = null;
				}
				return unpostedSequenceNumbersFromDb;
			}
		}

		List<ZShort> unpostedSequenceNumbersFromDb;

		static ZQuery FilterQuery
		{
			get
			{
				ZQuery result = new ZQuery();
				result.OrderBy = JobChargeSchema.Constants.JR_DisplaySequence;
				return result;
			}
		}

		ZShort GetBiggestSequenceNumber()
		{
			short result = ZShort.Zero;
			if (this.Count > 0)
			{
				result = GetBiggestSequenceNumberFromCollection();
			}
			if (!this.IsLoaded)
			{
				result = Math.Max(result, BiggestSequenceNumberFromDb);
			}
			return result;
		}

		ZShort GetBiggestSequenceNumberFromCollection()
		{
			short result = 0;
			foreach (Charge charge in this)
			{
				if (charge.JR_DisplaySequence > result)
				{
					result = charge.JR_DisplaySequence;
				}
			}
			return result;
		}

		ZShort GetBiggestSequenceNumberForPostedCharges()
		{
			short result = 0;
			foreach (Charge charge in this)
			{
				if (charge.JR_IsRevenuePosted)
				{
					if (charge.JR_DisplaySequence > result)
					{
						result = charge.JR_DisplaySequence;
					}
				}
			}
			return result;
		}

		internal ZShort GetUniqueSequenceNumberForUnpostedCharges(short proposedSequenceNumber)
		{
			if (proposedSequenceNumber < 0)
			{
				var listOfSequenceNumbers = !this.IsLoaded ? UnpostedSequenceNumbersFromDb : new List<ZShort>();
				unchecked
				{
					proposedSequenceNumber = (short)(proposedSequenceNumber - short.MinValue + 1);
				}
				if (this.Count > 0)
				{
					foreach (Charge charge in this)
					{
						if (!charge.IsRevenuePosted)
						{
							listOfSequenceNumbers.Add(charge.JR_DisplaySequence);
						}
					}
				}
				listOfSequenceNumbers.Sort();
				foreach (ZShort number in listOfSequenceNumbers)
				{
					if (proposedSequenceNumber == short.MaxValue)
					{
						break;
					}
					if (number == proposedSequenceNumber)
					{
						proposedSequenceNumber++;
					}
					if (proposedSequenceNumber < number)
					{
						break;
					}
				}
			}
			return proposedSequenceNumber;
		}

		ZInt GetBiggestSequenceNumberFromDb()
		{
			var sequenceNumber = new DynamicBusinessObjectCollection(Factory);
			var rawQuery = @"
SELECT CONVERT(int, MAX(JR_DisplaySequence)) AS maxSeqNum
FROM dbo.JobCharge
WHERE JR_JH = @ParentJobPK";
			sequenceNumber.Load(rawQuery, new ZSqlParameter[] { ZSqlParameter.New("@ParentJobPK", ParentJob.PK, JobChargeSchema.JR_JH) });
			return (ZInt)sequenceNumber[0]["maxSeqNum"];
		}

		List<ZShort> GetUnpostedSequenceNumbersFromDb()
		{
			var rawQuery = @"
SELECT JR_DisplaySequence AS UnpostedSeqNum
FROM
	dbo.JobCharge
	LEFT JOIN dbo.AccTransactionLines ON AL_PK = JR_AL_ARLine AND AL_LineType = 'REV'
WHERE
	JR_JH = @ParentJobPK
	AND AL_PK IS NULL";
			var queryResult = new DynamicBusinessObjectCollection(Factory);
			queryResult.Load(rawQuery, new ZSqlParameter[] { ZSqlParameter.New("@ParentJobPK", ParentJob.PK, JobChargeSchema.JR_JH) });
			var sequenceNumbers = new List<ZShort>();
			for (var i = 0; i < queryResult.Count; i++)
			{
				sequenceNumbers.Add((ZShort)queryResult[i]["UnpostedSeqNum"]);
			}
			return sequenceNumbers;
		}

		#endregion

		#region Requests

		#region Contains

		public bool ContainsUnPostedAR
		{
			get
			{
				foreach (Charge aCharge in this)
				{
					if (!aCharge.IsRevenuePosted && aCharge.JR_OSSellAmt != 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool ContainsUnPostedAR_AnyAmount
		{
			get
			{
				foreach (Charge charge in this)
				{
					if (!charge.IsRevenuePosted)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool ContainsUnPostedAP
		{
			get
			{
				foreach (Charge aCharge in this)
				{
					if (!aCharge.IsCostPosted && aCharge.JR_OSCostAmt != 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool ContainsPostedRevenue()
		{
			foreach (Charge aCharge in this)
			{
				if (aCharge.IsRevenuePosted)
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsPostedCost()
		{
			foreach (Charge aCharge in this)
			{
				if (aCharge.IsCostPosted)
				{
					return true;
				}
			}
			return false;
		}

		public Charge ContainsChargeCode(AccChargeCode code)
		{
			if (code != null)
			{
				foreach (Charge aCharge in this)
				{
					if (aCharge.ChargeCode != null && aCharge.ChargeCode.PK == code.PK)
					{
						return aCharge;
					}
				}
			}

			return null;
		}

		#endregion

		public bool IsNegativePayment
		{
			get
			{
				for (int i = 0; i < this.Count; i++)
				{
					Charge aCharge = this[i];

					if (!aCharge.JR_PaymentType.IsEmpty)
					{
						ZDecimal totalPaymentAmount = aCharge.JR_OSCostAmt;

						for (int j = 0; j < this.Count; j++)
						{
							Charge nextCharge = this[j];

							if (nextCharge != aCharge)
							{
								bool goesOnTheSameInvoice = nextCharge.JR_OH_CostAccount == aCharge.JR_OH_CostAccount && nextCharge.JR_APInvoiceNum == aCharge.JR_APInvoiceNum;
								bool willBeUsedForPayment = !nextCharge.JR_PaymentType.IsEmpty && goesOnTheSameInvoice;
								if (willBeUsedForPayment)
								{
									totalPaymentAmount += nextCharge.JR_OSCostAmt;
								}
							}
						}
						if (totalPaymentAmount < 0)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public Charge GetChargeByAPInvoiceCreditorCurrency(ChargeWithCost requestor)
		{
			if (!requestor.JR_RX_NKCostCurrency.IsEmpty && requestor.JR_OH_CostAccount.IsValid && !requestor.JR_APInvoiceNum.IsEmpty)
			{
				foreach (Charge charge in this)
				{
					if (charge != requestor && requestor.JR_RX_NKCostCurrency == charge.JR_RX_NKCostCurrency
						&& requestor.JR_OH_CostAccount == charge.JR_OH_CostAccount
						&& requestor.JR_APInvoiceNum == charge.JR_APInvoiceNum)
					{
						return charge;
					}
				}
			}
			return null;
		}

		public bool HasUnpostedARWhenAPPosted(ZGuid[] chargeCodePKs)
		{
			bool hasAPPosted = false;
			bool hasARPosted = false;

			foreach (Charge charge in this)
			{
				foreach (ZGuid chargeCodePK in chargeCodePKs)
				{
					if (charge.JR_AC == chargeCodePK && !charge.JR_IsApportioned)
					{
						hasAPPosted |= charge.IsCostPosted;
						hasARPosted |= charge.IsRevenuePosted;
					}
				}
			}

			return hasAPPosted && !hasARPosted;
		}

		public bool HasPostedAROrAP(ZGuid[] chargeCodePKs)
		{
			foreach (Charge charge in this)
			{
				foreach (ZGuid chargeCodePK in chargeCodePKs)
				{
					if (charge.JR_AC == chargeCodePK && !charge.JR_IsApportioned)
					{
						if (charge.IsCostPosted || charge.IsRevenuePosted)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		#endregion

		#region Deletion

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (elementToDelete.CanDelete)
			{
				base.RemoveAndDelete(elementToDelete);
			}
			else
			{
				OnCannotDelete?.Invoke(this, new OnCannotDeleteEventArgs(elementToDelete.ReasonForNotAbleToDelete, ((Charge)elementToDelete)));
			}
		}

		public event OnCannotDeleteHandler OnCannotDelete;

		#endregion

		#region Validation

		public void Validate()
		{
			SetApportionmentRowWarnings();
			ValidateAPInvoiceDetails();
		}

		protected void SetApportionmentRowWarnings()
		{
			foreach (Charge aCharge in this)
			{
				if (aCharge.JR_IsApportioned && !aCharge.IsCostPosted)
				{
					aCharge.AddRowWarning(Res.GetString("29d5007b-8e38-434d-91c2-7d098b8e690e", "Apportioned cost can be posted only from consol level."));
				}
			}
		}

		protected void ValidateAPInvoiceDetails()
		{
			Hashtable invoiceNumbers = new Hashtable();

			string MakeErrorRow(string input)
			{
				return "  " + input + "\r\n";
			}

			foreach (Charge aCharge in this)
			{
				string key = aCharge.JR_APInvoiceNum + aCharge.JR_OH_CostAccount.ToString();

				if (!aCharge.IsCostPosted && !aCharge.JR_APInvoiceNum.IsEmpty && !invoiceNumbers.Contains(key))
				{
					invoiceNumbers.Add(key, new Pair(aCharge.JR_OH_CostAccount, aCharge.JR_APInvoiceNum));
				}
			}

			foreach (Pair aPair in invoiceNumbers.Values)
			{
				ZQuery filter = new ZQuery();//JobChargeSchema.JR_AL_APLine, SQLComparisonOperator.Equal, null);
				filter.AddToFilter(JobChargeSchema.JR_APInvoiceNum, SQLComparisonOperator.Equal, aPair.InvNumber);
				object creditorValue = aPair.Creditor.IsEmpty ? null : aPair.Creditor;
				filter.AddToFilter(JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.Equal, creditorValue);

				BusinessObject[] lines = Find(filter);

				if (lines.Length > 0)
				{
					Charge previousCharge = lines[0] as Charge;

					foreach (Charge aCharge in lines)
					{
						if (previousCharge != aCharge)
						{
							var errorText = new ZStringBuilder();

							if (aCharge.JR_RX_NKCostCurrency != previousCharge.JR_RX_NKCostCurrency)
							{
								errorText.Append(MakeErrorRow(Res.GetString("ea831e64-75c9-4e76-9bae-23a0b50394dd", "Invoice currency")));
							}

							if (aCharge.JR_APInvoiceDate != previousCharge.JR_APInvoiceDate)
							{
								errorText.Append(MakeErrorRow(Res.GetString("93c32034-6c43-44ad-aef6-cfc2c644b97c", "Invoice Date")));
							}

							if (aCharge.JR_PaymentDate != previousCharge.JR_PaymentDate)
							{
								errorText.Append(MakeErrorRow(Res.GetString("cb563221-7152-4a5c-87b0-4666be93c29f", "Payment Date")));
							}

							if (aCharge.JR_PaymentType != previousCharge.JR_PaymentType)
							{
								errorText.Append(MakeErrorRow(Res.GetString("fe111d62-9d9d-4863-ad3b-72a22bff358f", "Payment Type")));
							}

							if (aCharge.JR_AB != previousCharge.JR_AB)
							{
								errorText.Append(MakeErrorRow(Res.GetString("a202bcf6-26e1-404f-9dc7-b62fcb9f19c7", "Bank Account")));
							}

							if (aCharge.JR_AK != previousCharge.JR_AK)
							{
								errorText.Append(MakeErrorRow(Res.GetString("ffcd674b-63a7-4b1a-81a7-038b5b69aa8d", "Check Book")));
							}

							if (aCharge.JR_ChequeNo != previousCharge.JR_ChequeNo)
							{
								errorText.Append(MakeErrorRow(Res.GetString("0b52221e-ee87-4610-ad37-fcae6443ad37", "Check Number")));
							}

							if (!errorText.IsEmpty)
							{
								errorText.Prepend(System.Environment.NewLine);
								errorText.Prepend(Res.GetString("fa9f8aac-b143-4ed7-b0cc-7a91334f2bdf",
									"The following properties of AP Invoice {0} don't match data in other charges:",
									aPair.InvNumber));

								foreach (Charge theCharge in lines)
								{
									theCharge.AddRowError(errorText.ToString());
								}
								break;
							}
						}
					}
				}
			}
		}
		#endregion

		#region Reload

		static int batchSize
		{
			get
			{
				return
#if DEBUG
 Globals.IsTest ? 5 :
#endif
 500;
			}
		}

		ICollection<ZGuid> GetChargePKsFromDb()
		{
			var chargePKsArray = GetPKs();
			var result = new List<ZGuid>();

			if (chargePKsArray.Any())
			{
				for (int i = 0; i < chargePKsArray.Count; i = i + batchSize)
				{
					ZGuid[] batch = new ZGuid[i <= chargePKsArray.Count - batchSize ? batchSize : chargePKsArray.Count % batchSize];
					chargePKsArray.CopyTo(i, batch, 0, batch.Length);
					string sqlChargePKList = string.Join(",", batch.Select(c => c.ToSqlGuid()).ToArray());

					string sQL = "SELECT " + JobChargeSchema.PK.Name + " FROM dbo.JobCharge WHERE " + JobChargeSchema.PK.Name + " IN (" + sqlChargePKList + ")";
					var dynamicCollection = new DynamicBusinessObjectCollection(Factory);
					dynamicCollection.Load(sQL);

					result.AddRange(dynamicCollection.Select(d => (ZGuid)d[JobChargeSchema.PK.Name]));
				}
			}

			return new HashSet<ZGuid>(result);
		}

		public void Reload()
		{
			var chargePKsFromDb = GetChargePKsFromDb();
			for (int i = 0; i < Count; i++)
			{
				var aCharge = this[i];

				if (aCharge.IsInDatabase)
				{
					if (chargePKsFromDb.Contains(aCharge.PK))
					{
						aCharge.Reload();
						aCharge.RefreshBinding();
					}
					else
					{
						Remove(aCharge);
					}
				}
			}
		}

		#endregion
	}
}
