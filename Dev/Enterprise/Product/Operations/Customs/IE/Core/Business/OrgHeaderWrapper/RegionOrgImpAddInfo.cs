using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business
{
	public class RegionOrgImpAddInfo : EUOrgImpAddInfo, Integration.Customs.IE.IRegionOrgImpAddInfo
	{
		public RegionOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RegionOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo)
		{
		}

		[ResourceStringData("IE.RegionOrgImpAddInfo.ZO_OtherDeferType", Caption = "Payment Method")]
		public override ZString ZO_OtherDeferType
		{
			get => base.ZO_OtherDeferType;
			set => base.ZO_OtherDeferType = value;
		}

		protected override EUOrgImpAddInfoLookups GetNewLookups() => new RegionOrgImpAddInfoLookups(this);

		public new RegionOrgImpAddInfoLookups Lookups => (RegionOrgImpAddInfoLookups)base.Lookups;
	}
}
