using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override ICollection Locations => JPRefCusCodeListTypes.GetJapanBondedAreaCodes(Factory, Parent.Header?.AMA_TransportMode ?? ZString.Empty);

		protected override CodeDescriptionPairList PackageTypeListCore => JPRefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes);

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<JPMessageStatusList>();

		public IBusinessObjectCollection SpecialCargoCodes => JPRefCusCodeListTypes.GetSpecialCargoCodes(Factory);

		public CodeDescriptionPairList CargoTypeList => Factory.GetCachedValue<ManifestCargoTypeList>();

		public CodeDescriptionPairList JPCustomsWeightUnitList => Factory.GetCachedValue<CustomsWeightUnitList>();

		public CodeDescriptionPairList JPCustomsVolumeUnitList => Factory.GetCachedValue<CustomsVolumeUnitList>();

		public override CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<JPCustomsStatusList>();

		public new ConsigneeCollection Consignees => new AsycudaBillConsigneeCollection(Factory, Parent);

		public new ConsignorCollection Consignors => new AsycudaBillConsignorCollection(Factory, Parent);

		public new OrgHeaderCollection Organisations => new AsycudaBillNotifyPartyCollection(Factory, Parent);

		public CodeDescriptionPairList ReasonList => TemporaryLandingInfo.GetReasonList(Factory);

		public CodeDescriptionPairList BondedTransportList => Factory.GetCachedValue("JP.Manifest.Business.BondedTransportList", () =>
		{
			var result = TemporaryLandingInfo.GetBondedTransportList(Factory);
			result.Sort();
			return result;
		});

		public RefCountryCollection CountryCollection => countryCollection ??= new RefCountryCollection(Factory);
		public RefCountryCollection countryCollection;

		public ChildTariffViewCollection TariffCollection => ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Japan, Parent.TariffType, ZDateTime.Today, null);

		public override RefUNLOCOCollection FinalDestinations => LookupsHelper.GetPortCollectionCore(Factory, Parent.FinalDestinationIATACodeInfo, Parent.IsAir);
	}
}
