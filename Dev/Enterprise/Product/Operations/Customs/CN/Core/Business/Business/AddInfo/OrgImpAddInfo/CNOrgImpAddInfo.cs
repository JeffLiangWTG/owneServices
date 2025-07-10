using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.CN;

namespace Enterprise.Customs.CN.Business
{
	public class CNOrgImpAddInfo : AutoCNOrgImpAddInfo, IOrgImpAddInfo
	{
		public CNOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public static CNOrgImpAddInfo Get(OrgHeader orgHeader)
		{
			var countryData = orgHeader.GetCountryData(Core.Constants.CountryCodes.China);
			orgHeader.RegisterEditableChildObject(countryData);
			return (CNOrgImpAddInfo)countryData.ImpAddInfo;
		}

		[List(nameof(Lookups) + "." + nameof(CNOrgImpAddInfoLookups.MessageSubTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CNOrgImpAddInfo|ZO_MessageSubType", Caption = "Declaration Type")]
		public override ZString ZO_MessageSubType { get => base.ZO_MessageSubType; set => base.ZO_MessageSubType = value; }

		[ResourceStringData("Enterprise.Customs.CN.Business.CNOrgImpAddInfo|ZO_IsAssuredInspectClearance", Caption = "Assured Inspect Clearance")]
		public override ZBool ZO_IsAssuredInspectClearance { get => base.ZO_IsAssuredInspectClearance; set => base.ZO_IsAssuredInspectClearance = value; }

		[ResourceStringData("Enterprise.Customs.CN.Business.CNOrgImpAddInfo|ZO_IsConsolidatedDutyCollection", Caption = "Consolidated Duty Collection")]
		public override ZBool ZO_IsConsolidatedDutyCollection { get => base.ZO_IsConsolidatedDutyCollection; set => base.ZO_IsConsolidatedDutyCollection = value; }

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.IntelligentDeclarationTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CNOrgImpAddInfo|ZO_IntelligentDeclarationType", Caption = "Intelligent Declaration Type", MediumCaption = "Intelligent Dec. Type", ShortCaption = "INT Dec. Type", FullDescription = "Intelligent Assisted Declaration Type")]
		public override ZString ZO_IntelligentDeclarationType { get => base.ZO_IntelligentDeclarationType; set => base.ZO_IntelligentDeclarationType = value; }
	}
}
