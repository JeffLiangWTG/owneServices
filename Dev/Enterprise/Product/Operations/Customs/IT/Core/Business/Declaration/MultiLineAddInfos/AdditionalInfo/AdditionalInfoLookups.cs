using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
{
	public AdditionalInfoLookups(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList GetAdditionalInformationList(ICanBeImportOrExport importExportParent, ZString direction)
	{
		return Factory.GetAdditionalInformationList(importExportParent.DataGroupingCode, importExportParent.Level, direction, ignoreLevel: true);
	}
}
