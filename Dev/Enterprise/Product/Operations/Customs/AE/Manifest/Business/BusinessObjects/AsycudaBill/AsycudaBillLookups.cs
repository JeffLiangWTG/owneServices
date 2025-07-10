using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
{
	public AsycudaBillLookups(ASYCUDA.Business.AsycudaBill parent) : base(parent)
	{
	}

	public new AsycudaBill Parent => (AsycudaBill)base.Parent;

	public CodeDescriptionPairList AECargoTypeList => Factory.GetCachedValue<AECargoTypeList>();

	public CodeDescriptionPairList ServiceRequirementCodeList => Factory.GetCachedValue("AEAsycudaBillLookups|ServiceRequirementCodeList", () => GetServiceRequirementCodeList());

	public override CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue($"AEAsycudaBillLookups|CustomsStatusList_{Parent.ABL_MessageStatus}", () => GetCustomsStatusList());

	public ICollection CustomsOriginPortList => Parent.Header.Lookups.CustomsOriginPortList;

	public CodeDescriptionPairList NegotiableList => Factory.GetCachedValue<NegotiableList>();

	protected override CodeDescriptionPairList PackageTypeListCore => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedArabEmirates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);

	public ICollection SplitBills
	{
		get
		{
			var result = new AsycudaBillFindBoxCollection(Factory);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country/Region", "Property", (ZString)Core.Constants.CountryCodes.UnitedArabEmirates, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("House Bill Number", "Property", Parent.ABL_SplitBillNumber));
			return result;
		}
	}

	CodeDescriptionPairList GetServiceRequirementCodeList()
	{
		var result = new CodeDescriptionPairList();
		result.AddRange(GetCodeListForType(AEConstants.RefCusCodeList.CodeTypes.ServiceRequirement));
		return result;
	}

	CodeDescriptionPairList GetCustomsStatusList()
	{
		var result = new CodeDescriptionPairList();
		var messageStatus = (string)Parent.ABL_MessageStatus;
		switch (messageStatus)
		{
			case AEConstants.Messaging.MessageTypes.CONTRL:
				var errorCodes = GetCodeListForType(RefCusCodeListTypes.ErrorCode);
				result.AddRange(errorCodes);
				result.AddOverwriteIfExists(new CodeDescriptionPair(AEConstants.Messaging.StatusCodes.ERR, Res.GetString("4313CEC2-0ED8-463F-BE75-FD7D424DFF04", "Error occurred in the UNB/UNZ segment.")));
				break;
			case AEConstants.Messaging.MessageTypes.CUSRES:
				result.AddRange(GetCodeListForType(RefCusCodeListTypes.CustomsManifestStatus));
				break;
			default:
				break;
		}
		return result;
	}

	ICodeDescriptionPairList GetCodeListForType(string codeType)
	{
		return new RefCusCodeListTypesListProvider().GetList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest, codeType, ZDateTime.Today, Array.Empty<ZString>());
	}
}
