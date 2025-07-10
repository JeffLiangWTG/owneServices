using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public AdditionalInfoLookups(AdditionalInfo parent)
			: base(parent)
		{
		}

		protected override ZBool OmitLevelAttribute => true;

		public override CodeDescriptionPairList GetAdditionalInformationList(ICanBeImportOrExport importExportParent, ZString direction)
		{
			return Factory.GetAdditionalInformationList(importExportParent.DataGroupingCode, Parent.ImportExportParent.Level, direction, true);
		}

		protected override string GetDataGroupingForUCCAdditionalInformation(ICanBeImportOrExport importExportParent) => Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE;
	}
}
