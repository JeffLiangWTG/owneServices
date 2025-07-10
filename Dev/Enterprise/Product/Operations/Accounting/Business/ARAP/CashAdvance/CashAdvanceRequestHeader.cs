using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public partial class CashAdvanceRequestHeader : AccCashAdvanceRequestHeader, IAccountingNumberFountainDataSource
	{
		public CashAdvanceRequestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[DecimalPlaces("ExchangeRateDecimals")]
		public ZDecimal CAH_ExchangeRate => (Company as ICompany).ExchangeRate.GetRate(CAH_LocalAmount, CAH_OSAmount);

		public int ExchangeRateDecimals => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		ZDateTime IAccountingNumberFountainDataSource.PostDate => CAH_SystemCreateTimeUtc;

		GlbBranch IAccountingNumberFountainDataSource.Branch => GlbBranch.CurrentBranch;

		GlbDepartment IAccountingNumberFountainDataSource.Department => GlbDepartment.CurrentDepartment;

		public override ZString JobBranch => InvoicingJob?.Branch?.GB_Code ?? ZString.Empty;

		public override ZString JobDepartment => InvoicingJob?.Department?.GE_Code ?? ZString.Empty;

		public ZString ShipmentHouseBill => InvoicingJob?.JH_HouseBillNo ?? ZString.Empty;

		public ZString ShipmentMasterBill => InvoicingJob?.JH_MasterBillNo ?? ZString.Empty;

		[List("DebtorAddressList")]
		public ZGuid DebtorAddress
		{
			get
			{
				if (Lines.Any())
				{
					var firstJobCharge = Lines.OfType<AccCashAdvanceRequestLine>()
											.Select(l => l.RelatedJobCharge)
											.FirstOrDefault();
					if (firstJobCharge is BaseCharge firstBaseCharge)
					{
						return firstBaseCharge.DisplaySellInvoiceAddress;
					}
				}
				return ZGuid.Empty;
			}
		}

		Job InvoicingJob => base.Job as Job;

		public OrgAddressDependentCollection DebtorAddressList
		{
			get
			{
				var fAddresses = new OrgAddressDependentCollection(Factory);
				if (Organization != null)
				{
					var filter = new ZQuery(OrgAddressSchema.OA_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					fAddresses = new OrgAddressDependentCollection(Organization, filter);
					fAddresses.Load();
				}
				return fAddresses;
			}
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				CAH_RequestReferenceNumber = AccountingNumberFountainWrapperFactory.Instance.ARCashAdvanceRequestReference.Generate(this);
			}
			base.OnSaving();
		}

		internal bool CanMatchingJournalBeCreated => CanCashAdvanceRelatedJournalBeCreated &&
													CanBeMarkedAsPaid &&
													CAH_LocalOutstandingAmount > 0;

		internal bool CanReverseJournalBeCreated => CanCashAdvanceRelatedJournalBeCreated &&
													CAH_Status == CashAdvanceStatusCodes.RequestHeader.Paid &&
													CAH_LocalOutstandingAmount == 0;

		internal bool CanCashAdvanceRelatedJournalBeCreated
		{
			get
			{
				var result = false;
				var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				if (CAH_Ledger == LedgerTypes.AccountsReceivable)
				{
					result = checker.IsReceivablesCashAdvanceFunctionalityEnabled &&
								!checker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed;
				}
				else if (CAH_Ledger == LedgerTypes.AccountsPayable)
				{
					result = checker.IsPayablesCashAdvanceFunctionalityEnabled &&
								!checker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;
				}
				return result;
			}
		}
	}
}
