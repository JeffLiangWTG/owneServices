using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournal : TransactionHeaderWithLines, IJobCosting, IDocManagerSupport, ICommissionableTransaction, IEDocsParsingSupport
	{
		public new abstract class Schema : TransactionHeader.Schema
		{
			public const string BranchPKFrom = "BranchPKFrom";
			public const string BranchPKTo = "BranchPKTo";
			public const string DepartmentPKFrom = "DepartmentPKFrom";
			public const string DepartmentPKTo = "DepartmentPKTo";
			public const string CostRevenueTypeFrom = "CostRevenueTypeFrom";
			public const string CostRevenueTypeTo = "CostRevenueTypeTo";
			public const string DefaultSharing = "DefaultSharing";
		}

		public JobRevenueJournal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void SetupDependencies(IClosedJobReopener closedJobReopener)
		{
			ClosedJobReopener = closedJobReopener;
		}

		public IClosedJobReopener ClosedJobReopener { get; private set; }

		#region Simplified Entry

		public bool IsInSimpleEntryMode
		{
			get;
			private set;
		}

		[ChildEditable(true)]
		public JobRevenueJournalLineCollection JournalLines
		{
			get { return (JobRevenueJournalLineCollection)Lines; }
		}

		[ChildEditable(true)]
		public JobRevenueJournalChargeCollection JournalCharges
		{
			get
			{
				if (JournalCharges_cached == null)
				{
					JournalCharges_cached = new JobRevenueJournalChargeCollection(this);
					RegisterEditableChildObject(JournalCharges_cached);
				}
				return JournalCharges_cached;
			}
		}
		JobRevenueJournalChargeCollection JournalCharges_cached;

		public void ActivateSimpleEntry(Job job)
		{
			Factory.SetContext(BusinessContext.CreateJobRevenueJournalNotInRevenueJournalModule);
			JournalCharges.Job = job;

			if (job != null)
			{
				BranchPKFrom = job.JH_GB;
				DepartmentPKFrom = job.JH_GE;
				if (!job.JobDescription.IsEmpty)
				{
					AH_Desc = job.JobDescription;
				}
			}
			BranchPKTo = GlbBranch.CurrentBranch.PK;
			DepartmentPKTo = GlbDepartment.CurrentDepartment.PK;
			SetDefaultCostRevenueTypeToAndFrom();

			IsInSimpleEntryMode = true;
		}

		void SetDefaultCostRevenueTypeToAndFrom()
		{
			using (new DisposableAction(() => Factory.SetContext(BusinessContext.SetDefaultCostRevenueType), () => Factory.RemoveContext(BusinessContext.SetDefaultCostRevenueType)))
			{
				var registrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code)
				{
					CostRevenueTypeFrom = TransactionLineTypes.Revenue;
					CostRevenueTypeTo = TransactionLineTypes.Revenue;
				}
				else if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code)
				{
					CostRevenueTypeFrom = TransactionLineTypes.Cost;
					CostRevenueTypeTo = TransactionLineTypes.Cost;
				}
				else if (registrySetting == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code)
				{
					CostRevenueTypeFrom = TransactionLineTypes.Cost;
					CostRevenueTypeTo = TransactionLineTypes.Revenue;
				}
			}
		}

		public void DeactivateSimpleEntry()
		{
			JournalCharges.DetachCharges();
			if (IsInSimpleEntryMode)
			{
				IsInSimpleEntryMode = false;
				RunPreSaveValidation();
			}
		}

		[List("Lookups.Branches")]
		public ZGuid BranchPKFrom
		{
			get { return BranchPKFrom_cached; }
			set
			{
				BranchPKFrom_cached = value;
				BranchPKFromInfo.RefreshBinding();
				foreach (JobRevenueJournalCharge journalCharge in JournalCharges)
				{
					journalCharge.BranchPKFrom = BranchPKFrom;
				}

				if (!IsValidationSuspended && JournalValidation != null)
				{
					JournalValidation.ValidateBranchPKFrom();
				}
			}
		}
		ZGuid BranchPKFrom_cached;

		public ZPropertyInfo BranchPKFromInfo
		{
			get { return GetZPropertyInfo(Schema.BranchPKFrom); }
		}

		[List("Lookups.Branches")]
		public ZGuid BranchPKTo
		{
			get { return BranchPKTo_cached; }
			set
			{
				BranchPKTo_cached = value;
				BranchPKToInfo.RefreshBinding();
				foreach (JobRevenueJournalCharge journalCharge in JournalCharges)
				{
					journalCharge.BranchPKTo = BranchPKTo;
				}

				if (!IsValidationSuspended && JournalValidation != null)
				{
					JournalValidation.ValidateBranchPKTo();
				}
			}
		}
		ZGuid BranchPKTo_cached;

		public ZPropertyInfo BranchPKToInfo
		{
			get { return GetZPropertyInfo(Schema.BranchPKTo); }
		}

		[List("Lookups.Departments")]
		public ZGuid DepartmentPKFrom
		{
			get { return DepartmentPKFrom_cached; }
			set
			{
				DepartmentPKFrom_cached = value;
				DepartmentPKFromInfo.RefreshBinding();
				foreach (JobRevenueJournalCharge journalCharge in JournalCharges)
				{
					journalCharge.DepartmentPKFrom = DepartmentPKFrom;
				}

				if (!IsValidationSuspended && JournalValidation != null)
				{
					JournalValidation.ValidateDepartmentPKFrom();
				}
			}
		}
		ZGuid DepartmentPKFrom_cached;

		public ZPropertyInfo DepartmentPKFromInfo
		{
			get { return GetZPropertyInfo(Schema.DepartmentPKFrom); }
		}

		[List("Lookups.Departments")]
		public ZGuid DepartmentPKTo
		{
			get { return DepartmentPKTo_cached; }
			set
			{
				DepartmentPKTo_cached = value;
				DepartmentPKToInfo.RefreshBinding();
				foreach (JobRevenueJournalCharge journalCharge in JournalCharges)
				{
					journalCharge.DepartmentPKTo = DepartmentPKTo;
				}

				if (!IsValidationSuspended && JournalValidation != null)
				{
					JournalValidation.ValidateDepartmentPKTo();
				}
			}
		}
		ZGuid DepartmentPKTo_cached;

		public ZPropertyInfo DepartmentPKToInfo
		{
			get { return GetZPropertyInfo(Schema.DepartmentPKTo); }
		}

		[DecimalPlaces(2)]
		public ZDecimal DefaultSharing
		{
			get { return DefaultSharing_cached; }
			set
			{
				DefaultSharing_cached = value;
				DefaultSharingInfo.RefreshBinding();
				foreach (JobRevenueJournalCharge journalCharge in JournalCharges)
				{
					if (journalCharge.Share == ZDecimal.Zero)
					{
						journalCharge.Share = DefaultSharing;
					}
				}

				if (!IsValidationSuspended && JournalValidation != null)
				{
					JournalValidation.ValidateDefaultSharing();
				}
			}
		}
		ZDecimal DefaultSharing_cached;

		public ZPropertyInfo DefaultSharingInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultSharing); }
		}

		[MaxLength(3)]
		[List("CostRevenueTypeList")]
		public ZString CostRevenueTypeTo
		{
			get
			{
				return fCostRevenueTypeTo;
			}
			set
			{
				CheckMaximumLength(CostRevenueTypeToInfo, value);
				if (Factory.HasContext(BusinessContext.SetDefaultCostRevenueType) || SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideCostRevenueType))
				{
					fCostRevenueTypeTo = value;
					CostRevenueTypeToInfo.RefreshBinding();
					foreach (JobRevenueJournalCharge journalCharge in JournalCharges)
					{
						journalCharge.CostRevenueTypeTo = fCostRevenueTypeTo;
					}
					if (!IsValidationSuspended)
					{
						JournalValidation?.ValidateCostRevenueTypeTo();
					}
				}
				else
				{
					SecurityHelper.ShowError(SecurityCore.AllowOverrideCostRevenueType);
				}
			}
		}
		ZString fCostRevenueTypeTo;

		public ZPropertyInfo CostRevenueTypeToInfo
		{
			get { return GetZPropertyInfo(Schema.CostRevenueTypeTo); }
		}

		protected bool CostRevenueTypeTo_ReadOnly => CostRevenueTypeIsReadOnly;

		[MaxLength(3)]
		[List("CostRevenueTypeList")]
		public ZString CostRevenueTypeFrom
		{
			get
			{
				return fCostRevenueTypeFrom;
			}
			set
			{
				CheckMaximumLength(CostRevenueTypeFromInfo, value);
				if (Factory.HasContext(BusinessContext.SetDefaultCostRevenueType) || SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideCostRevenueType))
				{
					fCostRevenueTypeFrom = value;
					CostRevenueTypeFromInfo.RefreshBinding();
					foreach (JobRevenueJournalCharge journalCharge in JournalCharges)
					{
						journalCharge.CostRevenueTypeFrom = fCostRevenueTypeFrom;
					}
					if (!IsValidationSuspended)
					{
						JournalValidation?.ValidateCostRevenueTypeFrom();
					}
				}
				else
				{
					SecurityHelper.ShowError(SecurityCore.AllowOverrideCostRevenueType);
				}
			}
		}
		ZString fCostRevenueTypeFrom;

		public ZPropertyInfo CostRevenueTypeFromInfo
		{
			get { return GetZPropertyInfo(Schema.CostRevenueTypeFrom); }
		}

		protected bool CostRevenueTypeFrom_ReadOnly => CostRevenueTypeIsReadOnly;

		public CodeDescriptionPairList CostRevenueTypeList
		{
			get
			{
				if (fCostRevenueTypeList == null)
				{
					fCostRevenueTypeList = new CodeDescriptionPairList();
					fCostRevenueTypeList.AddPair(TransactionLineTypes.Cost, Res.GetString("7e3bebea-da0d-46aa-ba67-f53ab0a0148a", "Cost"));
					fCostRevenueTypeList.AddPair(TransactionLineTypes.Revenue, Res.GetString("5389648b-aa12-4c4a-a766-a316124884d2", "Revenue"));
				}
				return fCostRevenueTypeList;
			}
		}
		CodeDescriptionPairList fCostRevenueTypeList;

		bool CostRevenueTypeIsReadOnly
		{
			get
			{
				var registrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				return registrySetting != AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code;
			}
		}

		#endregion

		#region Security Checkpoint Helper

		SecurityCheckpoint PluginSecurity
		{
			get
			{
				var plugInSecurity = Env.Security.None;
				if (JournalCharges.Job != null)
				{
					var plugInParent = JournalCharges.Job.PlugInData;
					if (plugInParent != null && plugInParent.InvoicingSupporter != null)
					{
						plugInSecurity = plugInParent.InvoicingSupporter.JobInvoicingSecurity;
					}
				}
				return plugInSecurity;
			}
		}

		public JobInvoicingSecurityHelper SecurityHelper
		{
			get { return fSecurityHelper ?? (fSecurityHelper = new JobInvoicingSecurityHelper(() => PluginSecurity)); }
		}
		JobInvoicingSecurityHelper fSecurityHelper;

		#endregion

		#region Implementation

		bool HasJobChargeTransformerRun;

		void SetJobRevenueRecognitionDate(JobRevenueJournalLine invoicingLine)
		{
			Job job = Factory.Load<Job>(invoicingLine.AL_JH);
			if (job != null)
			{
				job.ApplyRevenueRecognitionDate(invoicingLine);
			}
			else
			{
				invoicingLine.UpdateAL_ReverseDate();
			}
		}

		internal void RecalculateAH_JH_AH_GB_AH_GE(out ZGuid newAH_JH, out ZGuid newAH_GB, out ZGuid newAH_GE)
		{
			newAH_JH = ZGuid.Empty;
			newAH_GB = AH_GB;
			newAH_GE = AH_GE;

			if (Lines.Count > 0)
			{
				newAH_JH = Lines[0].AL_JH;
				foreach (JobRevenueJournalLine line in Lines)
				{
					if (line.AL_JH != newAH_JH)
					{
						newAH_JH = ZGuid.Empty;
						break;
					}
				}
			}

			var job = Factory.Load<JobHeader>(newAH_JH);
			if (job != null)
			{
				if (!AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.Value)
				{
					newAH_GB = job.JH_GB;
				}
				newAH_GE = job.JH_GE;
			}
		}

		Charge[] ClearedJobCharges;

		void SetJobChargesWIPAccrualCreationDate()
		{
			if (ClearedJobCharges != null)
			{
				foreach (Charge aCharge in ClearedJobCharges)
				{
					aCharge.WIPAccrualCreationDate = AH_PostDate;
				}
			}
		}

		JobRevenueJournal ReverseJournal
		{
			get { return fReverseTransaction as JobRevenueJournal; }
		}

		JobRevenueJournalValidation JournalValidation
		{
			get { return Validation as JobRevenueJournalValidation; }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("FD399748-D5AF-429F-9539-0E2761C292F8", "Job Revenue Journal"); }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.JobCosting; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.JRJournal; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.JobRevenueJournal; }
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(JobRevenueJournalLine); }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new JobRevenueJournalValidation(this);
		}

		protected override TransactionHeader CopyTransaction()
		{
			JobRevenueJournal copiedJournal = (JobRevenueJournal)base.CopyTransaction();

			copiedJournal.AH_Desc = AH_Desc;
			copiedJournal.AH_InvoiceDate = AH_InvoiceDate;
			copiedJournal.AH_PostDate = AH_PostDate;
			copiedJournal.AH_GB = AH_GB;
			copiedJournal.AH_GE = AH_GE;
			copiedJournal.AH_RX_NKTransactionCurrency = AH_RX_NKTransactionCurrency;
			copiedJournal.AH_ExchangeRate = AH_ExchangeRate;
			copiedJournal.AH_OSExTaxAmount = AH_OSExTaxAmount;

			foreach (JobRevenueJournalLine line in JournalLines)
			{
				copiedJournal.JournalLines.AddNew().CopyValuesFrom(line);
			}

			return copiedJournal;
		}

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);
			using (GetValidationSuspender())
			using (ReverseJournal.GetValidationSuspender())
			{
				ReverseJournal.AH_DueDate = ZDateTime.Empty;

				foreach (JobRevenueJournalLine line in Lines)
				{
					using (line.GetValidationSuspender())
					{
						JobRevenueJournalLine reverseInvoiceLine = (JobRevenueJournalLine)ReverseJournal.Lines.AddNew();
						using (reverseInvoiceLine.GetValidationSuspender())
						{
							reverseInvoiceLine.CopyValuesFrom(line);
							reverseInvoiceLine.ReverseDebitCreditSign();
						}
					}
				}

				ReverseInvoicingTransformer transformer = new ReverseInvoicingTransformer(Factory);
				if (mustTransform)
				{
					transformer.Transform(this);
				}
				ReverseJournal.ClearedJobCharges = transformer.Charges;
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			using (GetValidationSuspender())
			{
				base.OnFactorySavingBeforeTransactionCore();

				if (!IsInDatabase)
				{
					foreach (JobRevenueJournalLine line in Lines)
					{
						line.AL_PostDate = AH_PostDate;
						line.CalculateHighPrecisionExchangeRate();
						SetJobRevenueRecognitionDate(line);
					}
					if (!AH_IsCancelled && !HasJobChargeTransformerRun)
					{
						new JobRevenueJournalJobChargeTransformer(Factory).Transform(this);
						HasJobChargeTransformerRun = true;
					}
					SetJobChargesWIPAccrualCreationDate();

					ZGuid newAH_JH;
					ZGuid newAH_GB;
					ZGuid newAH_GE;
					RecalculateAH_JH_AH_GB_AH_GE(out newAH_JH, out newAH_GB, out newAH_GE);

					AH_JH = newAH_JH;
					AH_GB = newAH_GB;
					AH_GE = newAH_GE;
				}
			}
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSExTaxAmount_ReadOnly
		{
			get { return true; }
			set { }
		}

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new JobRevenueJournalLineCollection(this);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && IsTransactionInDatabaseReadOnly)
			{
				JournalCharges.SetReadOnlyIncludingChildren(true);
				RefreshBindingIncludingChildren();
			}
		}

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return new JobRevenueJournalDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.JobRevenueJournal)); }
		}
		AccountingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		internal void MarkAsAlreadyTransformed()
		{
			HasJobChargeTransformerRun = true;
		}

		public bool IsAutoJRJ
		{
			get { return AH_TransactionCategory == Constants.TransactionCategory.Codes.AutoJobRevenueJournal; }
		}

		ZString ICommissionableTransaction.AH_Calc_LocalRXCode
		{
			get
			{
				return AH_Calc_LocalRXCode;
			}
		}

		bool ICommissionableTransaction.IsJobRelated
		{
			get
			{
				return AH_JH.IsValid || Lines.OfType<AccTransactionLines>().Any(x => x.AL_JH.IsValid);
			}
		}

		BusinessObjectCollection ICommissionableTransaction.Lines
		{
			get
			{
				return Lines;
			}
		}

		public override ZBool CanApplyTaxBranch => false;
	}

	public class JobRevenueJournalDocumentSupporter : TransactionHeader.TransactionHeaderDocumentSupporter
	{
		public JobRevenueJournalDocumentSupporter(JobRevenueJournal journal)
			: base(journal)
		{
		}

		protected JobRevenueJournal Journal
		{
			get { return (JobRevenueJournal)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.JobRevenueJournalCustomiseDocuments; }
		}

		public override CargoWise.Definitions.BusinessContext BusinessContext
		{
			get { return CargoWise.Definitions.BusinessContext.JobRevenueJournal; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.JobRevenueJournal, Constants.DataContext.GenericFreightJob };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Constants.DataContext.JobRevenueJournal)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.JobRevenueJournal, Journal) };
			}
			else
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
		}

		#endregion
	}
}
