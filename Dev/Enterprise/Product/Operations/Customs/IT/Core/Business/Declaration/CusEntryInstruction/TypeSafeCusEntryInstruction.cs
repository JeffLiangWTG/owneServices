using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class CusEntryInstruction
{
	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
	{
		var result = base.GetCusSupportingInfoTypesCore();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}
}
