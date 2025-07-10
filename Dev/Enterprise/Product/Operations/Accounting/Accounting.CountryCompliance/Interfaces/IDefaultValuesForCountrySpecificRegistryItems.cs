using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface IDefaultValuesForCountrySpecificRegistryItems
	{
		//Please add new items to the interface alphabetically
		int EInvoicingRequeueDelayTime { get; }

		ReadOnlyCodeDescriptionPairList EInvoicingAmendmentCodes { get; }

		ReadOnlyCodeDescriptionPairList EInvoicingReversalCodes { get; }

		bool EnableGovernmentAllocatedNumberBehavior { get; }

		MultilingualString PrintWatermarkForTransactionAwaitingApproval { get; }

		string SAFTGroupingCategory { get; }

		bool GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency();

		string TaxMessageIsMandatoryPayables { get; }

		string TaxMessageIsMandatoryReceivables { get; }

		string ThirdPartyEInvoiceDocType { get; }
		//Please add new items to the interface alphabetically
	}

	public class DefaultValuesForCountrySpecificRegistryItems : IDefaultValuesForCountrySpecificRegistryItems
	{
		#region IDefaultValuesForCountrySpecificRegistryItems members

		bool IDefaultValuesForCountrySpecificRegistryItems.GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency() => GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency();

		int IDefaultValuesForCountrySpecificRegistryItems.EInvoicingRequeueDelayTime => EInvoicingRequeueDelayTime;

		string IDefaultValuesForCountrySpecificRegistryItems.ThirdPartyEInvoiceDocType => ThirdPartyEInvoiceDocType;

		MultilingualString IDefaultValuesForCountrySpecificRegistryItems.PrintWatermarkForTransactionAwaitingApproval => PrintWatermarkForTransactionAwaitingApproval;

		string IDefaultValuesForCountrySpecificRegistryItems.SAFTGroupingCategory => SAFTGroupingCategory;

		bool IDefaultValuesForCountrySpecificRegistryItems.EnableGovernmentAllocatedNumberBehavior => EnableGovernmentAllocatedNumberBehavior;

		ReadOnlyCodeDescriptionPairList IDefaultValuesForCountrySpecificRegistryItems.EInvoicingAmendmentCodes => EInvoicingAmendmentCodes;

		ReadOnlyCodeDescriptionPairList IDefaultValuesForCountrySpecificRegistryItems.EInvoicingReversalCodes => EInvoicingReversalCodes;

		string IDefaultValuesForCountrySpecificRegistryItems.TaxMessageIsMandatoryReceivables => TaxMessageIsMandatoryReceivables;

		string IDefaultValuesForCountrySpecificRegistryItems.TaxMessageIsMandatoryPayables => TaxMessageIsMandatoryPayables;

		#endregion

		#region Virtual members

		protected virtual bool GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency() => false;

		protected virtual int EInvoicingRequeueDelayTime => 0;

		protected virtual string ThirdPartyEInvoiceDocType => Constants.RefDocTypes.Invoice;

		protected virtual MultilingualString PrintWatermarkForTransactionAwaitingApproval => null;

		protected virtual string SAFTGroupingCategory => null;

		protected virtual bool EnableGovernmentAllocatedNumberBehavior => false;

		protected virtual ReadOnlyCodeDescriptionPairList EInvoicingAmendmentCodes => new ReadOnlyCodeDescriptionPairList();

		protected virtual ReadOnlyCodeDescriptionPairList EInvoicingReversalCodes => new ReadOnlyCodeDescriptionPairList();

		protected virtual string TaxMessageIsMandatoryReceivables => Constants.TaxMessageMandatoryOptionConstants.NotRequired;

		protected virtual string TaxMessageIsMandatoryPayables => Constants.TaxMessageMandatoryOptionConstants.NotRequired;

		#endregion
	}
}

