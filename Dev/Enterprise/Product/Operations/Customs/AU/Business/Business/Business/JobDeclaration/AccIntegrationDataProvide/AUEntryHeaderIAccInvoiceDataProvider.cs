using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.InterfaceImplementations;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AUEntryHeaderIAccInvoiceDataProvider : IAccInvoiceDataProvider, ICustomsChargeEntry
	{
		public AUEntryHeaderIAccInvoiceDataProvider(CusEntryHeader entryHeader)
		{
			invoiceDataProvider = Argument.NotNull(entryHeader, nameof(entryHeader));
			customsChargeEntry = entryHeader;
		}
		readonly IAccInvoiceDataProvider invoiceDataProvider;
		readonly ICustomsChargeEntry customsChargeEntry;

		#region IAccInvoiceDataProvider

		public bool IsBillable => invoiceDataProvider.IsBillable;

		public string ReasonForUnbillability => invoiceDataProvider.ReasonForUnbillability;

		public bool HasBeenWithdrawn => invoiceDataProvider.HasBeenWithdrawn;

		public string EntryWithdrawnStatusTerm => invoiceDataProvider.EntryWithdrawnStatusTerm;

		public ICustomsCharges[] CustomsCharges
		{
			get
			{
				ICustomsCharges customsCharges = new CusEntryHeaderCustomsCharges(this);
				return new ICustomsCharges[] { new CusEntryHeaderCustomsCharges.CustomsChargeCache(customsCharges) };
			}
		}

		public virtual ZString UniqueNumber => invoiceDataProvider.UniqueNumber;

		public ZString PreviousUniqueNumber => invoiceDataProvider.PreviousUniqueNumber;

		public ZDateTime InvoiceDate => invoiceDataProvider.InvoiceDate;

		public ZDateTime APDueDate => invoiceDataProvider.APDueDate;

		public AutoPostingNotification AutoPostingNotification => invoiceDataProvider.AutoPostingNotification;

		public ICustomsJobInfo CustomsJob => invoiceDataProvider.CustomsJob;

		public BusinessObjectFactory Factory => invoiceDataProvider.Factory;

		public bool IsEligibleForIntegration => invoiceDataProvider.IsEligibleForIntegration;

		public virtual bool APInvoiceNumberAlwaysIncludeChargeCode => false;

		public bool MatchCustomsChargesToClear(ZString apInvoiceNumber, ZString description) => invoiceDataProvider.MatchCustomsChargesToClear(apInvoiceNumber, description);

		bool IAccInvoiceDataProvider.IsAutoBillingDueDateFromPaymentTerms => CustomsDataRegistry.Instance.AutoBillingDueDateFromPaymentTerms.Value;

		#endregion

		#region ICustomsChargeEntry

		public JobHeader Job => customsChargeEntry.Job;

		public abstract EntryChargeTypeList EntryChargeTypeList { get; }

		public ZString ReferenceNumber => customsChargeEntry.ReferenceNumber;

		public ZGuid CreditorPK => customsChargeEntry.CreditorPK;

		public bool EntryReferenceInChargeDescSupported => customsChargeEntry.EntryReferenceInChargeDescSupported;

		public ZString LocalCurrencyCode => customsChargeEntry.LocalCurrencyCode;

		public ZDecimal GetTotalChargeValueFor(EntryChargeType chargeType, ZString methodOfPaymentCode)
		{
			return customsChargeEntry.GetTotalChargeValueFor(chargeType, methodOfPaymentCode);
		}

		public bool IsFeePaidByBroker(string chargeType, ZString methodOfPaymentCode, ILogger logger)
		{
			return customsChargeEntry.IsFeePaidByBroker(chargeType, methodOfPaymentCode, logger);
		}

		public CustomsCharge[] GetNonFeeCountrySpecificCharges()
		{
			return customsChargeEntry.GetNonFeeCountrySpecificCharges();
		}

		public ZString[] GetMethodsOfPaymentThatCanInfluenceAutoRating()
		{
			return customsChargeEntry.GetMethodsOfPaymentThatCanInfluenceAutoRating();
		}

		#endregion
	}
}
