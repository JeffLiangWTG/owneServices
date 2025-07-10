using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.IN;

namespace Enterprise.Customs.IN.Business;

public class INOrgImpAddInfo : AutoINOrgImpAddInfo, IOrgImpAddInfo
{
	public INOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
	{
		using (GetValidationSuspender())
		using (SuspendSettingHasChanges())
		{
			Deserialise();
		}
	}

	public INOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
		: base(parentPropertyInfo.BizObj.Factory)
	{
		ParentPropertyInfo = parentPropertyInfo;
		using (GetValidationSuspender())
		using (SuspendSettingHasChanges())
		{
			Deserialise();
		}

		header = ((OrgCountryData)ParentPropertyInfo?.BizObj)?.OrgHeader;
		if (header != null)
		{
			ClearZO_IsDiplomatIfReadOnly();

			header.OH_CategoryInfo.ValueChanged += (o, s) =>
			{
				ClearZO_IsDiplomatIfReadOnly();
				ZO_IsDiplomatInfo.RefreshBinding();
			};

			if (!parentPropertyInfo.Value.Contains(nameof(ZO_TypeOfImporter))
				&& header.MainAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.India
				&& header.OH_Category == OrgConstants.Category.Government)
			{
				ZO_TypeOfImporter = ImporterTypeList.Codes.GovernmentDepartmentsBothCenterState;
			}
		}
	}
	readonly OrgHeader header;

	void ClearZO_IsDiplomatIfReadOnly()
	{
		if (ZO_IsDiplomat_ReadOnly && ZO_IsDiplomat)
		{
			ZO_IsDiplomat = false;
		}
	}

	[MaxLength(1)]
	[List((nameof(Lookups) + "." + nameof(INOrgImpAddInfoLookups.ExporterTypeList)))]
	[ResourceStringData("INOrgImpAddInfo|ZO_TypeOfExporter", ShortCaption = "Ty. Exporter", MediumCaption = "Ty. of Exporter", Caption = "Type of Exporter")]
	public override ZString ZO_TypeOfExporter { get => base.ZO_TypeOfExporter; set => base.ZO_TypeOfExporter = value; }

	[MaxLength(1)]
	[List((nameof(Lookups) + "." + nameof(INOrgImpAddInfoLookups.ImporterTypeList)))]
	[ResourceStringData("INOrgImpAddInfo|ZO_TypeOfExporter", ShortCaption = "Ty. Importer", MediumCaption = "Ty. of Importer", Caption = "Type of Importer")]
	public override ZString ZO_TypeOfImporter { get => base.ZO_TypeOfImporter; set => base.ZO_TypeOfImporter = value; }

	[ReadOnlyMember(nameof(ZO_IsDiplomat_ReadOnly))]
	[ResourceStringData("INOrgImpAddInfo|ZO_IsDiplomat", Caption = "Is Diplomat?")]
	public override ZBool ZO_IsDiplomat { get => base.ZO_IsDiplomat; set => base.ZO_IsDiplomat = value; }

	bool ZO_IsDiplomat_ReadOnly => (header?.OH_Category ?? ZString.Empty) != OrgConstants.Category.Government;

	public static INOrgImpAddInfo Get(OrgHeader organisation) => (INOrgImpAddInfo)organisation?.GetCountryData(Core.Constants.CountryCodes.India).ImpAddInfo;
}
