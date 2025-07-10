using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos
{
	public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
		, Integration.Customs.GB.IAdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IValueSetStrategy GetValueSetStrategy() => new AdditionalInfoValueSetStrategy(this);

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return IsImport() ? new ImportAdditionalInfoValidation(this) : new AdditionalInfoValidation(this);
		}

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return Declaration?.ApplicationExtender?.GetNewAdditionalInfoLookups(this) ?? new AdditionalInfoLookups(this);
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override ZZRefCusCodeListCombined RefCusCodeCore => Factory.GetAdditionalInformationCode(ImportExportParent.DataGroupingCode, CSI_Code, includeParentDataGroupingForAdditionalInformation);

		bool includeParentDataGroupingForAdditionalInformation => !Declaration?.IsUCCCompliant ?? true;
		ZBool IsImport() => (Parent as ICanBeImportOrExport)?.IsImport ?? false;
	}
}
