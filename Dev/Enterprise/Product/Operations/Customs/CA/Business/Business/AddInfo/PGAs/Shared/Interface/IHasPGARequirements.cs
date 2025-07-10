using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public interface IHasPGARequirements
	{
		PGARequirementCollection PGARequirements { get; }

		#region OA_Manufacturer

		ZGuid OA_Manufacturer { get; set; }
		ZPropertyInfo OA_ManufacturerInfo { get; }
		ZAddress OA_ManufacturerAddress_ZAddress { get; }
		OrgHeaderCollection ManufacturersLookup { get; }

		#endregion

		#region RN_NKCountryOfOrigin

		ZString RN_NKCountryOfOrigin { get; set; }
		ZPropertyInfo RN_NKCountryOfOriginInfo { get; }
		RefCountryCollection CountryOfOriginsLookup { get; }

		#endregion

		#region RW_NKOriginState

		ZString RW_NKOriginState { get; set; }
		ZPropertyInfo RW_NKOriginStateInfo { get; }
		CodeDescriptionPairList StateCodeListLookup { get; }

		#endregion

		#region RN_NKCountryOfSource

		ZString RN_NKCountryOfSource { get; set; }
		ZPropertyInfo RN_NKCountryOfSourceInfo { get; }
		RefCountryCollection CountryOfSourceLookup { get; }

		#endregion

		#region RW_NKCountryOfSourceState

		ZString RW_NKCountryOfSourceState { get; set; }
		ZPropertyInfo RW_NKCountryOfSourceStateInfo { get; }
		CodeDescriptionPairList CountryOfSourceStateLookup { get; }

		#endregion

		#region JI_BrandName

		ZString JI_BrandName { get; set; }
		ZPropertyInfo JI_BrandNameInfo { get; }

		#endregion

		#region JI_Model

		ZString JI_Model { get; set; }
		ZPropertyInfo JI_ModelInfo { get; }

		#endregion

		ZString Tariff { get; }
		ZPropertyInfo TariffInfo { get; }
	}
}
