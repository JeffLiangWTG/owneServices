using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionLine : AutoAccCommissionLine, IAccCommissionLine
	{
		#region Constructors

		public AccCommissionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CL0_OverridenDateTimeUtc), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CL0_CancelledDateTimeUtc), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CL0_PaidDateTimeUtc), ConcurrencyPolicy.Strict);
		}

		#endregion

		#region Properties

		#region CL0_CAT

		public OrgCommissionAgreementRecipient CommissionAgreementRecipient
		{
			get
			{
				var rate = CommissionAgreementRecipientRate;
				return rate != null ? rate.CommissionAgreementRecipient : null;
			}
		}

		#endregion

		#region CL0_CancelledDateTimeUtc

		public void Cancel()
		{
			if (!IsCancelled)
			{
				CL0_CancelledDateTimeUtc = ZDateTime.UtcNow;
			}
		}

		public ZBool IsCancelled
		{
			get { return !CL0_CancelledDateTimeUtc.IsEmpty; }
		}

		#endregion

		#region CL0_EntityCommissionAmount

		[DecimalPlaces(nameof(CommissionDecimals))]
		public override ZDecimal CL0_EntityCommissionAmount
		{
			get => base.CL0_EntityCommissionAmount;
			set => base.CL0_EntityCommissionAmount = value;
		}

		#endregion

		#region CL0_EntityPercentage

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal CL0_EntityPercentage
		{
			get => base.CL0_EntityPercentage;
			set => base.CL0_EntityPercentage = value;
		}

		#endregion

		#region CL0_OverridenDateTimeUtc

		public ZBool IsOverriden
		{
			get { return !CL0_OverridenDateTimeUtc.IsEmpty; }
		}

		#endregion

		#region CL0_ShareCommissionAmount

		[DecimalPlaces(nameof(CommissionDecimals))]
		public override ZDecimal CL0_ShareCommissionAmount
		{
			get => base.CL0_ShareCommissionAmount;
			set => base.CL0_ShareCommissionAmount = value;
		}

		#endregion

		#region CL0_PaidDateTimeUtc

		public ZBool IsPaid
		{
			get { return !CL0_PaidDateTimeUtc.IsEmpty; }
		}

		#endregion

		#region CL0_TotalCommissionableAmount

		[DecimalPlaces(nameof(CommissionDecimals))]
		public override ZDecimal CL0_TotalCommissionableAmount
		{
			get => base.CL0_TotalCommissionableAmount;
			set => base.CL0_TotalCommissionableAmount = value;
		}

		#endregion

		#region CL0_TransactionAmount

		[DecimalPlaces(nameof(TransactionDecimals))]
		public override ZDecimal CL0_TransactionAmount
		{
			get => base.CL0_TransactionAmount;
			set => base.CL0_TransactionAmount = value;
		}

		#endregion

		#region Line Group

		public AccCommissionLineGroup LineGroup
		{
			get
			{
				if (CL0_ParentTableCode == AccCommissionLineGroupSchema.Constants.Prefix)
				{
					return Factory.Load<AccCommissionLineGroup>(CL0_ParentID);
				}

				return null;
			}
		}

		#endregion

		#region Commission Header

		public AccCommissionHeader CommissionHeader
		{
			get
			{
				if (CL0_ParentTableCode == AccCommissionHeaderSchema.Constants.Prefix)
				{
					return Factory.Load<AccCommissionHeader>(CL0_ParentID);
				}

				return null;
			}
		}

		#endregion

		public int CommissionDecimals => CommissionCurrency != null ? CommissionCurrency.Decimals : GlbCompany.CurrentCompany.GetLocalDecimals();
		public int TransactionDecimals => TransactionCurrency != null ? TransactionCurrency.Decimals : GlbCompany.CurrentCompany.GetLocalDecimals();
		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		#endregion

		#region Override

		public void MarkAsOverriden(bool isAlreadyReversed = false)
		{
			if (!IsOverriden)
			{
				CL0_OverridenDateTimeUtc = ZDateTime.UtcNow;

				if (CL0_ShouldReinstate && !isAlreadyReversed)
				{
					var reversalLine = Factory.New<AccCommissionLine>();

					PopulateReversalLine(reversalLine, this);

					reversalLine.CL0_ParentID = CL0_ParentID;
					reversalLine.CL0_ParentTableCode = CL0_ParentTableCode;
					reversalLine.CL0_OverridenDateTimeUtc = CL0_OverridenDateTimeUtc;
				}
			}
		}

		public static void PopulateReversalLine(AccCommissionLine reversalLine, IAccCommissionLine originalLine)
		{
			using (reversalLine.SuspendSettingHasChanges())
			using (reversalLine.GetValidationSuspender())
			{
				reversalLine.CL0_BelongsToGroup = originalLine.PK;
				reversalLine.CL0_CAT = originalLine.CL0_CAT;
				reversalLine.CL0_GS_NKStaff = originalLine.CL0_GS_NKStaff;
				reversalLine.CL0_OH_Party = originalLine.CL0_OH_Party;
				reversalLine.CL0_RX_NKTransactionCurrency = originalLine.CL0_RX_NKTransactionCurrency;
				reversalLine.CL0_TransactionAmount = -originalLine.CL0_TransactionAmount;
				reversalLine.CL0_CommissionType = originalLine.CL0_CommissionType;
				reversalLine.CL0_RX_NKCommissionCurrency = originalLine.CL0_RX_NKCommissionCurrency;
				reversalLine.CL0_TotalCommissionableAmount = -originalLine.CL0_TotalCommissionableAmount;
				reversalLine.CL0_SharePortion = originalLine.CL0_SharePortion;
				reversalLine.CL0_ShareTotal = originalLine.CL0_ShareTotal;
				reversalLine.CL0_ShareCommissionAmount = -originalLine.CL0_ShareCommissionAmount;
				reversalLine.CL0_EntityPercentage = originalLine.CL0_EntityPercentage;
				reversalLine.CL0_EntityCommissionAmount = -originalLine.CL0_EntityCommissionAmount;

				reversalLine.CL0_ApprovedDateTimeUtc = ZDateTime.Empty;
				reversalLine.CL0_OverridenDateTimeUtc = ZDateTime.Empty;
				reversalLine.CL0_PaidDateTimeUtc = ZDateTime.Empty;

				if (!originalLine.IsPaid)
				{
					originalLine.Cancel();
					reversalLine.Cancel();
				}
			}
		}

		#endregion

		#region Related Business Objects

		public AccCommissionApprovalRequest CommissionApprovalRequest
		{
			get
			{
				var item = CommissionApprovalRequestItems.FirstOrDefault();
				return item != null ? item.CommissionApprovalRequest : null;
			}
		}

		AccCommissionApprovalRequestItemCollection CommissionApprovalRequestItems
		{
			get
			{
				if (commissionApprovalRequestItems == null)
				{
					commissionApprovalRequestItems = new AccCommissionApprovalRequestItemCollection(this);
				}

				return commissionApprovalRequestItems;
			}
		}
		AccCommissionApprovalRequestItemCollection commissionApprovalRequestItems;

		#endregion

		#region Delete

		public override void Delete()
		{
			CommissionApprovalRequestItems.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (CL0_ParentID.IsEmpty || CL0_ParentTableCode.IsEmpty)
			{
				var commissionHeader = Factory.LoadTop1<AccCommissionHeader>(new ZQuery()) ?? Factory.NewWithValidTestData<AccCommissionHeader>();
				CL0_ParentID = commissionHeader.PK;
				CL0_ParentTableCode = commissionHeader.TablePrefix;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
