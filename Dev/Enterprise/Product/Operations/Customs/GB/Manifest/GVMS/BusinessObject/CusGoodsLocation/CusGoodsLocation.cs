using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.GB.GBGVMS.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new Customs.Business.CusGoodsLocationValidation Validation => GetNewValidation();

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new EU.Business.CusGoodsLocationValidation(this);

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		[ResourceStringData("Enterprise.Customs.GB.GVMS.InspectionType", Caption = "Inspection Type")]
		public ZString InspectionType => Lookups.TypeList.GetWithDescription(CGL_Type);

		[ResourceStringData("Enterprise.Customs.GB.GVMS.InspectionLocation", Caption = "Inspection Location")]
		public ZString InspectionLocation => Lookups.InspectionLocationList.GetWithDescription(CGL_AdditionalIdentifier);
	}
}
