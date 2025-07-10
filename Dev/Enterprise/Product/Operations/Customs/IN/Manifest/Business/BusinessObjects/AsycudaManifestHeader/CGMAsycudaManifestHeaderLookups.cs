using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Manifest.Business;

public class CGMAsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
{
	public CGMAsycudaManifestHeaderLookups(CGMAsycudaManifestHeader parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList WeightUQList => Factory.GetWeightUQList();

	public CodeDescriptionPairList ManifestUQList => Factory.GetPackageTypeList();

	public CodeDescriptionPairList ActionList
	{
		get
		{
			var checkRegistrationStatus = Parent.RegistrationStatus == Business.RegistrationStatusList.Codes.ManifestRegistered;
			return Factory.GetCachedValue($"IN.CGMAsycudaManifestHeaderLookups|ActionList|{checkRegistrationStatus}", () =>
			{
				var list = new ManifestMessageTypeList();
				if (checkRegistrationStatus)
				{
					list.RemoveCode(ManifestMessageTypeList.Codes.Fresh);
				}
				return list;
			});
		}
	}

	public override CodeDescriptionPairList TransportModeList
	{
		get
		{
			var list = base.TransportModeList;
			if (list.Count == 1)
			{
				list.DefaultCode = list[0].Code;
			}
			return list;
		}
	}

	public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<MessageStatusList>();

	public override CodeDescriptionPairList RegistrationStatusList => Factory.GetCachedValue<RegistrationStatusList>();

	protected override ICollection CustomsOfficesCore => UniversalReferenceDataHelper.GetCustomsOfficeCollection(Factory, Parent.IsAir);

	protected override ICollection GetCustomsDischargePortListCore() => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

	protected override RefUNLOCOCollection GetDestinationPortListCore() => new RefUNLOCOCollection(Factory, IndiaFilter(isIndia: true));

	protected override RefUNLOCOCollection GetOriginPortListCore() => new RefUNLOCOCollection(Factory, IndiaFilter(isIndia: false));

	ZDBOnlyQuery IndiaFilter(bool isIndia)
	{
		var result = new ZDBOnlyQuery(typeof(RefUNLOCO));
		var comparisonOperator = isIndia ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
		result.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, comparisonOperator, Core.Constants.CountryCodes.India);
		return result;
	}
}
