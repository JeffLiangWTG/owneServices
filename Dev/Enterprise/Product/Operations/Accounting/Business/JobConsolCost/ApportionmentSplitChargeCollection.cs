using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class ApportionmentSplitChargeCollection : DependentBusinessObjectCollection<ApportionSplitCharge, JobConsolCost>
	{
		public ApportionmentSplitChargeCollection(JobConsolCost masterConsolCost)
			: base(masterConsolCost)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			ApportionSplitCharge charge = (ApportionSplitCharge)child;
			charge.IsFinal = Master.IsFinal;
			charge.JR_IsCostTaxAmountOverridden = Master.E6_IsTaxAmountOverridden;
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || (Master != null && Master.IsPosted && !Master.IsApprovingPosting); }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}

		new JobConsolCost Master
		{
			get { return base.Master; }
		}

		public decimal JR_OSCostAmtSum
		{
			get
			{
				decimal sum = 0;
				foreach (ApportionSplitCharge charge in this)
				{
					sum += charge.JR_OSCostAmt;
				}
				return sum;
			}
		}

		public decimal JR_LocalCostAmtSum
		{
			get
			{
				decimal sum = 0;
				foreach (ApportionSplitCharge charge in this)
				{
					sum += charge.JR_LocalCostAmt;
				}
				return sum;
			}
		}

		public decimal JR_OSCostGSTAmtSum
		{
			get
			{
				decimal sum = 0;
				foreach (ApportionSplitCharge charge in this)
				{
					sum += charge.JR_OSCostGSTAmt_Calc;
				}
				return sum;
			}
		}

		public ApportionSplitCharge FindChargeForJob(IJobInvoicingPlugIn shipment)
		{
			if (shipment == null)
			{
				return null;
			}

			return this
				.Cast<ApportionSplitCharge>()
				.FirstOrDefault(x => IsApportionedForShipment(x, shipment));
		}

		bool IsApportionedForShipment(ApportionSplitCharge charge, IJobInvoicingPlugIn shipment) => (charge.ShipmentInfo?.PK ?? charge.Job.JH_ParentID) == shipment.PK;

		public bool ContainsChargeForJob(IJobInvoicingPlugIn shipment)
		{
			if (shipment == null)
			{
				return false;
			}

			return this
				.Cast<ApportionSplitCharge>()
				.Any(x => IsApportionedForShipment(x, shipment));
		}

		public ApportionSplitCharge AddDefaultChargeForJob(Job shipmentJob)
		{
			using (shipmentJob.SuspendSettingHasChanges())
			{
				return AddDefaultChargeForJob(shipmentJob.PK, shipmentJob.JH_GB, shipmentJob.JH_GE);
			}
		}

		internal IEnumerable<ZGuid> GetPostedAPInvoices
		{
			get
			{
				return this.ToArray<ApportionSplitCharge>().Select(x => x.APLine != null ? x.APLine.AL_AH : ZGuid.Empty).Distinct();
			}
		}

		internal IEnumerable<ZGuid> GetPostedTaxRates => this.ToArray<ApportionSplitCharge>().Select(x => x.APLine != null ? x.APLine.AL_AT : ZGuid.Empty).Distinct();

		internal IEnumerable<ZDate> GetPostedTaxDates => this.ToArray<ApportionSplitCharge>().Select(x => x.APLine != null ? x.APLine.AL_TaxDate : ZDate.Empty).Distinct();

		internal IEnumerable<ZGuid> GetPostedTaxClasses
		{
			get
			{
				return this.ToArray<ApportionSplitCharge>().Select(x => x.APLine != null ? x.APLine.AL_A9_VATClass : ZGuid.Empty).Distinct();
			}
		}

		internal IEnumerable<ZGuid> GetPostedTaxBranch => this.ToArray<ApportionSplitCharge>().Select(x => x.APLine != null ? x.APLine.AL_GB_TaxBranch : ZGuid.Empty).Distinct();

		internal bool AreInvoiceDetailsInSyncWithConsolCost
		{
			get
			{
				return this.ToArray<ApportionSplitCharge>().All(x =>
					x.JR_APInvoiceNum == Master.E6_InvoiceNum && x.JR_OH_CostAccount == Master.E6_OH_Creditor && x.JR_CostReference == Master.E6_CostReference
					&& x.JR_APInvoiceDate == Master.E6_InvoiceDate && x.JR_APDocumentReceivedDate == Master.E6_DocumentReceivedDate && x.JR_PaymentDate == Master.E6_PaymentDate
					&& x.JR_AT_CostGSTRate == Master.E6_AT_TaxRate && x.JR_CostTaxDate == Master.E6_TaxDate
					&& x.JR_A9_CostVATClass == Master.E6_A9_VATClass && x.JR_CostPlaceOfSupply == Master.E6_PlaceOfSupply && x.JR_CostPlaceOfSupplyType == Master.E6_PlaceOfSupplyType
					&& x.JR_GB_CostTaxBranch == Master.E6_GB_CostTaxBranch);
			}
		}

		internal bool AreInvoiceDetailsInSyncWithPostedInvoiceLines
		{
			get
			{
				return this.ToArray<ApportionSplitCharge>().All(x =>
					x.APLine != null && x.APLine.AL_LineType == TransactionLineTypes.Cost
					&& x.JR_OH_CostAccount == x.APLine.TransactionHeader.AH_OH
					&& x.JR_APInvoiceNum == x.APLine.TransactionHeader.AH_TransactionNum && x.JR_CostReference == x.APLine.TransactionHeader.AH_TransactionReference
					&& x.JR_APInvoiceDate == x.APLine.TransactionHeader.AH_InvoiceDate && x.JR_APDocumentReceivedDate == x.APLine.TransactionHeader.AH_DocumentReceivedDate && x.JR_PaymentDate == x.APLine.TransactionHeader.AH_DueDate
					&& x.JR_AT_CostGSTRate == x.APLine.AL_AT && x.JR_CostTaxDate == x.APLine.AL_TaxDate
					&& x.JR_A9_CostVATClass == x.APLine.AL_A9_VATClass && x.JR_CostPlaceOfSupply == x.APLine.AL_PlaceOfSupply && x.JR_CostPlaceOfSupplyType == x.APLine.AL_PlaceOfSupplyType
					&& x.JR_GB_CostTaxBranch == x.APLine.AL_GB_TaxBranch);
			}
		}

		internal bool ArePostedToSameAPInvoiceAsConsolCost
		{
			get
			{
				return (GetPostedAPInvoices.Count() == 1) && (GetPostedAPInvoices.First() == Master.E6_AH_APInvoice);
			}
		}

		internal bool ArePostedWithSameTaxRateAndClass
		{
			get
			{
				return (GetPostedTaxRates.Count() == 1 && GetPostedTaxClasses.Count() == 1);
			}
		}

		internal bool ArePostedWithJobRevenueJournal
		{
			get
			{
				return this.ToArray<ApportionSplitCharge>().Any(x => x.IsCostPostedWithJobRevenueJournal);
			}
		}

		internal ApportionSplitCharge AddDefaultChargeForJob(ZGuid shipmentJobPK, ZGuid shipmentJobJH_GB, ZGuid shipmentJobJH_GE)
		{
			ApportionSplitCharge newCharge = this.AddNew();
			using (new DisposableAction(() => newCharge.SetContext(BusinessContext.AddingDefaultApportionmentCharge),
				() => newCharge.RemoveContext(BusinessContext.AddingDefaultApportionmentCharge)))
			using (newCharge.SuspendSettingHasChanges())
			{
				try
				{
					if (newCharge.ParentConsolCost.CalculationStrategy is JobConsolCost.ConsolCostCalculationStrategy)
					{
						newCharge.OrgNotDebtorCheckEnabled = true;
					}
				}
				catch (NullReferenceException ex)
				{
					// Check whether the master has been deleted or there is a problem loading from the factory
					string exMessage = String.Format("Master.IsNull: {0}, Master.IsDeleted: {1}, Master.PK equals NewCharge.JR_E6: {2}", Master.IsNull, Master.IsDeleted, (Master.PK.Equals(newCharge.JR_E6)));
					ErrorReporter.ReportOnce("NullObjectInApportionmentSplitChargeCollection", exMessage, ex);
				}

				newCharge.JR_JH = shipmentJobPK;
				newCharge.JR_E6 = Master.PK;
				newCharge.JR_GB = shipmentJobJH_GB;
				newCharge.JR_GE = shipmentJobJH_GE;
				newCharge.JR_AC = Master.E6_AC_ChargeCode;
				RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Master.E6_RX_NKCurrency);
				if (currency != null)
				{
					newCharge.JR_RX_NKCostCurrency = currency.RX_Code;
					newCharge.JR_OSCostExRate = Master.E6_ExchangeRate;
				}

				if (Master.IsGatewayConsolCost)
				{
					newCharge.JR_E6_GatewaySellHeader = Master.PK;
				}

				newCharge.JR_IsCostTaxAmountOverridden = Master.E6_IsTaxAmountOverridden;
				newCharge.JR_OH_CostAccount = Master.E6_OH_Creditor;
				newCharge.JR_APInvoiceNum = Master.E6_InvoiceNum;
				newCharge.JR_APInvoiceDate = Master.E6_InvoiceDate;
				newCharge.JR_APDocumentReceivedDate = Master.E6_DocumentReceivedDate;
				newCharge.JR_CostReference = Master.E6_CostReference;
				newCharge.JR_CostSupplyType = Master.E6_SupplyType;
				if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
				{
					newCharge.JR_GB_CostTaxBranch = Master.E6_GB_CostTaxBranch;
					newCharge.SetSellTaxBranchDefault();
				}
				newCharge.JR_AT_CostGSTRate = Master.E6_AT_TaxRate;
				newCharge.SetCostTaxDateSafe(Master.E6_TaxDate);
				newCharge.JR_PaymentDate = Master.E6_PaymentDate;
				newCharge.JR_PaymentType = Master.E6_PaymentType;
				newCharge.JR_AB = Master.E6_AB_BankAccount;
				newCharge.JR_AK = Master.E6_AK_ChequeBook;
				newCharge.JR_ChequeNo = Master.E6_ChequeOrReference;
				newCharge.JR_CostPlaceOfSupply = Master.E6_PlaceOfSupply;
				newCharge.JR_CostPlaceOfSupplyType = Master.E6_PlaceOfSupplyType;
				newCharge.JR_A9_CostVATClass = Master.E6_A9_VATClass;

				InitializeGovtChargeCode(newCharge);

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(Master.PK, CriticalValidationInfoCollectorServiceKeyType.DefaultChargeCreationForConsolCost, () => FormattableString.Invariant($"\r\nDefault Charge is created.\r\nDefault Charge PK: {newCharge.PK}\r\nConsol Cost PK: {Master.PK}\r\nShipment Job PK: {shipmentJobPK}\r\nStackTrace: {System.Environment.StackTrace}"));

				return newCharge;
			}
		}

		void InitializeGovtChargeCode(ApportionSplitCharge newCharge)
		{
			if (!Master.IsValidForGovtChargeCode)
			{
				return;
			}

			if (string.IsNullOrEmpty(newCharge.JR_CostGovtChargeCode) || string.IsNullOrEmpty(newCharge.JR_SellGovtChargeCode))
			{
				newCharge.JR_CostGovtChargeCode = Master.GetMatchedGovtChargeCode(CostSell.Cost);
				newCharge.JR_SellGovtChargeCode = Master.GetMatchedGovtChargeCode(CostSell.Revenue);
			}
		}

		internal ApportionmentSplitChargeCollectionValidationSuspender GetApportionmentSplitChargeCollectionValidationSuspender()
		{
			return new ApportionmentSplitChargeCollectionValidationSuspender(this);
		}

		public override void Add(BusinessObject element)
		{
			base.Add(element);

			var apportionSplitCharge = element as ApportionSplitCharge;

			if (apportionSplitCharge != null && apportionSplitCharge.ShipmentInfo != null && apportionSplitCharge.ParentConsolCost != null && !apportionSplitCharge.ParentConsolCost.IsDeleted)
			{
				apportionSplitCharge.SetChargeIsUsedForApportionmentWithoutCalulations(apportionSplitCharge.ShouldIncludeInApportionment);
			}
		}
	}

	internal class ApportionmentSplitChargeCollectionValidationSuspender : IDisposable
	{
		internal ApportionmentSplitChargeCollectionValidationSuspender(ApportionmentSplitChargeCollection collection)
		{
			foreach (ApportionSplitCharge charge in collection)
			{
				Handles.Add(charge.GetValidationSuspender());
			}
		}

		readonly Collection<IDisposable> Handles = new Collection<IDisposable>();

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			foreach (IDisposable handle in Handles)
			{
				handle.Dispose();
			}
		}

		#endregion
	}
}
