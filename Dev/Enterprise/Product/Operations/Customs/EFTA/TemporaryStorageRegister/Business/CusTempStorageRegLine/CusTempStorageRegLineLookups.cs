using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineLookups : AutoCusTempStorageRegLineLookups
{
	public CusTempStorageRegLineLookups(AutoCusTempStorageRegLine parent) : base(parent)
	{
	}

	public virtual CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue<CodeDescriptionPairList>();

	public virtual CodeDescriptionPairList PackageTypeList => GetPackingUnitTypesList();

	public virtual CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<CodeDescriptionPairList>();

	public virtual CodeDescriptionPairList UnionStatusList => Factory.GetCachedValue<CodeDescriptionPairList>();

	public virtual CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

	public virtual CodeDescriptionPairList BulkPackageUnitTypeList
		=> GetPackingUnitTypesList(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);

	protected virtual ZDateTime GetUnPackTypeStartDate() => new (2012, 1, 1);

	CodeDescriptionPairList GetPackingUnitTypesList(params ZString[] attributeNamesToMatch)
	{
		return Universal.RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			GetUnPackTypeStartDate(),
			attributeNamesToMatch.Any()
				? attributeNamesToMatch.Select(attributeName => new KeyValuePair<ZString, ZString>(attributeName, ZString.Empty)).ToArray()
				: null);
	}
}
