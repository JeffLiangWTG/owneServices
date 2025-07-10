using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

class Ucc6ExportAdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
{
	public Ucc6ExportAdditionalInfoLookups(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo parent) : base(parent)
	{
	}

	protected override ZBool OmitLevelAttribute => true;
}
