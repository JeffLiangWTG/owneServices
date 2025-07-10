using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues
{
	abstract class DefaultTestDataForCountrySpecificRegistryItems
	{
		public virtual bool ExpectedDefaultShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency => false;

		public virtual int EInvoicingRequeueDelayTime => 0;

		public virtual ZString ThirdPartyEInvoiceDocType => Constants.RefDocTypes.Invoice;

		public virtual string PrintWatermarkForTransactionAwaitingApproval => null;

		public virtual bool EnableGovernmentAllocatedNumberBehavior => false;

		public virtual ReadOnlyCodeDescriptionPairList EInvoicingReversalCodesDefault => new ReadOnlyCodeDescriptionPairList();

		public virtual ReadOnlyCodeDescriptionPairList EInvoicingAmendmentCodesDefault => new ReadOnlyCodeDescriptionPairList();

		public virtual string TaxMessageIsMandatoryReceivables => Constants.TaxMessageMandatoryOptionConstants.NotRequired;

		public virtual string TaxMessageIsMandatoryPayables => Constants.TaxMessageMandatoryOptionConstants.NotRequired;

		public virtual string SAFTGroupingCategory => null;
	}
}
