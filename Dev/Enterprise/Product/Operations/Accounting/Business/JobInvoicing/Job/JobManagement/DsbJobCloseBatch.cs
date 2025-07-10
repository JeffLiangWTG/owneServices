using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class DsbJobCloseBatch : AutoDsbJobCloseBatch
	{
		public DsbJobCloseBatch(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal JBB_LargestAmount
		{
			get => base.JBB_LargestAmount;
			set => base.JBB_LargestAmount = value;
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal JBB_SmallestAmount
		{
			get => base.JBB_SmallestAmount;
			set => base.JBB_SmallestAmount = value;
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal JBB_TotalAmount
		{
			get => base.JBB_TotalAmount;
			set => base.JBB_TotalAmount = value;
		}

		[List("Lookups.StatusList")]
		public override ZString JBB_BatchStatus
		{
			get => base.JBB_BatchStatus;
			set => base.JBB_BatchStatus = value;
		}

		public ZDateTime JBB_ApprovalTime => (JBB_ApprovalTimeUtc.IsValid) ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(JBB_ApprovalTimeUtc.ToDateTime()) : ZDateTime.Empty;
		public ZDateTime JBB_SystemCreateTime => (JBB_SystemCreateTimeUtc.IsValid) ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(JBB_SystemCreateTimeUtc.ToDateTime()) : ZDateTime.Empty;

		public ZPropertyInfo JBB_ApprovalTimeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(JBB_ApprovalTime)); }
		}

		public ZPropertyInfo JBB_SystemCreateTimeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(JBB_SystemCreateTime)); }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("fcde8bff-0ff6-4d1c-8784-5b1c10908363", @"{0} - Disbursement Job Batch", JBB_BatchNumber);

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property) => true;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Local Currency

		public int LocalCurrencyDecimals => GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		public ZGuid LocalCurrencyPK => GlbCompany.CurrentCompany.LocalCurrency.PK;

		public ZString LocalCurrencyCode => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		public RefCurrencyCollection LocalCurrencies => new RefCurrencyCollection(Factory);

		#endregion

		#region BalanceAmount
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal BalanceAmount {
			get
			{
				return fBalanceAmount;
			}
			private set
			{
				SetNonPersistentPropertyValue(BalanceAmountInfo, ref fBalanceAmount, value);
				BalanceAmountInfo.RefreshBinding();
			}
		}
		ZDecimal fBalanceAmount;

		public ZPropertyInfo BalanceAmountInfo
		{
			get { return GetZPropertyInfo(nameof(BalanceAmount)); }
		}

		#endregion

		#region TransactionLines

		[ChildEditable()]
		public DsbJobCloseBatchTransactionLineCollection TransactionLines
		{
			get
			{
				if (fTransactionLines == null)
				{
					fTransactionLines = new DsbJobCloseBatchTransactionLineCollection(this);
					fTransactionLines.Load();
					RegisterEditableChildObject(fTransactionLines);
				}
				return fTransactionLines;
			}
		}
		DsbJobCloseBatchTransactionLineCollection fTransactionLines;

		#endregion

		#region Jobs

		[ChildEditable()]
		public DSBJobManagementCollection Jobs
		{
			get
			{
				if (fJobs == null)
				{
					var lines = TransactionLines.Cast<AccTransactionLines>().Where(x => !x.AL_JH.IsEmpty);
					var filter = new ZQuery().AddToFilter(JobHeaderSchema.PK, lines.Select(x => x.AL_JH));
					fJobs = new DSBJobManagementCollection(Factory, filter);
					RegisterEditableChildObject(fJobs);
					fJobs.ForEach(job => job.DSBSurplusAndShortfallAmount = lines.Where(line => line.AL_JH == job.PK).Sum(line => line.AL_LineAmount));
					BalanceAmount = fJobs.Sum(job => job.DSBSurplusAndShortfallAmount);
				}
				return fJobs;
			}
		}
		DSBJobManagementCollection fJobs;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JBB_GC = GlbCompany.CurrentCompany.PK;
			JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
		}
	}
}
