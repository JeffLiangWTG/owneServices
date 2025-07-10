using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class TaxLookupsCommon : EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon
{
	public TaxLookupsCommon(IEuTax parent) : base(parent)
	{
	}

	protected override void AddCountrySpecificRateCodes(EU.Business.Declaration.MultiLineAddInfos.RateCodeDescriptionPairList rateCodeDescriptionPairList)
	{
		base.AddCountrySpecificRateCodes(rateCodeDescriptionPairList);
		rateCodeDescriptionPairList.AddRange(new VatExemptionTypeCodeDescriptionPairList());
	}

	protected override ZString[] GetCountrySpecificRateTypesToInclude()
	{
		var rateTypes = new List<ZString> { RefCusRateTypes.MiscellaneousNotVatableImportExport, RefCusRateTypes.MiscellaneousImportExport };

		if (parent.ImportExportParent is CusEntryLine entryLine && (entryLine.Declaration?.IsUCC6AndIsExport ?? false))
		{
			rateTypes.Add(Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			rateTypes.Add(Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty);
			rateTypes.Add(Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty);
		}

		return rateTypes.ToArray();
	}
}
