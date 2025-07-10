using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public interface IGuidedDecisionMakingTarget
	{
		ZString TariffCode { set; }
		ZString CountryOfOrigin { set; }
		ZString Preference { set; }
		ZString QuotaOrderNumber { set; }
		ZDecimal CustomsFirstQuantity { set; }
		ZString CustomsFirstUnitQty { set; }
		ZDecimal CustomsSecondQuantity { set; }
		ZString CustomsSecondUnitQty { set; }
		ZDecimal CustomsThirdQuantity { set; }
		ZString CustomsThirdUnitQty { set; }
		ZString CountryOfDestination { set; }
		void SetVATCode(ZString vatCode, ZString vatAdditionalCode);
		void SetAdditionalCodes(List<ZString> codesToSet);
		void SetSupportingAndAdditionalDocuments(List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> documentsToSet);
	}
}
