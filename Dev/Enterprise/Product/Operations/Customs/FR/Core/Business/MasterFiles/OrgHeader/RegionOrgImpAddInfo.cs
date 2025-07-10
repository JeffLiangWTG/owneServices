using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class RegionOrgImpAddInfo : EUOrgImpAddInfo, Integration.Customs.FR.IRegionOrgImpAddInfo
	{
		public RegionOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RegionOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo)
		{
		}

		[ResourceStringData("FR.RegionOrgImpAddInfo.ZO_OtherDeferType", Caption = "Method of Payment")]
		public override ZString ZO_OtherDeferType
		{
			get => base.ZO_OtherDeferType;
			set => base.ZO_OtherDeferType = value;
		}

		protected override EUOrgImpAddInfoLookups GetNewLookups() => new RegionOrgImpAddInfoLookups(this);

		public new RegionOrgImpAddInfoLookups Lookups => (RegionOrgImpAddInfoLookups)base.Lookups;
	}
}
