using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public interface IGuidedDecisionMakingSource
	{
		ZString DataGrouping { get; }
		ZString ParentDataGrouping { get; }
		ZString UserLanguage { get; }
		ZString DutyRateTypeCode { get; }
		ZBool AllowQuickAdditionalCodeScreen { get; }
		ZBool AllowQuickConditionScreen { get; }
		ZBool IsImport { get; }
		ZBool IsExport { get; }
		ZDate EffectiveDate { get; }
		ZString TariffCode { get; }
		ZString CountryCode { get; }
		ZString CountryOfOrigin { get; }
		ZString Preference { get; }
		ZString QuotaOrderNumber { get; }
		ZDecimal CustomsFirstQuantity { get; }
		ZDecimal CustomsSecondQuantity { get; }
		ZString CustomsSecondUnitQty { get; }
		ZDecimal CustomsThirdQuantity { get; }
		ZString CustomsThirdUnitQty { get; }
		ZString TariffType { get; }
		IEnumerable<ZString> SupplementaryCodes { get; }
		IEnumerable<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> SupportingAndAdditionalDocuments { get; }
		ZString VATCode { get; }
		ZString EffectiveCountryOfDestination { get; }
		IBusiness ParentBusinessObject { get; }
		ZBool IsCustomsFirstQuantityReadOnly { get; }
		ZBool IsCustomsSecondQuantityReadOnly { get; }
		ZBool IsCustomsThirdQuantityReadOnly { get; }
	}
}
