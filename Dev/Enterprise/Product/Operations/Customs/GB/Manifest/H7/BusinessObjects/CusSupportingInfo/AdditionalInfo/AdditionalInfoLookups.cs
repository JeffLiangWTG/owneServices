using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AdditionalInfoLookups : BaseAdditionalInfoLookups
	{
		public AdditionalInfoLookups(AdditionalInfo parent)
			: base(parent)
		{
		}

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

		public override CodeDescriptionPairList GetAdditionalInformationList(ICanBeImportOrExport importExportParent, ZString direction)
		{
			return Factory.GetAdditionalInformationList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, importExportParent.Level, direction, true, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
		}
	}
}
