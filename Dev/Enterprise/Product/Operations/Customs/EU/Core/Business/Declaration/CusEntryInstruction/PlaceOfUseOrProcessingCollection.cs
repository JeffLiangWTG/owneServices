using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class PlaceOfUseOrProcessingCollection : DependentBusinessObjectCollection<PlaceOfUseOrProcessing, CusEntryInstruction>
	{
		public PlaceOfUseOrProcessingCollection(CusEntryInstruction master) : base(master, new ZQuery(CusGoodsLocationSchema.CGL_LocationUse, PlaceOfUseOrProcessingLocationUseList.Codes.PlacesOfUseOrProcessing)) { }

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(PlaceOfUseOrProcessing);

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusGoodsLocationSchema.CGL_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var placeOfUseOrProcessing = (PlaceOfUseOrProcessing)child;
			placeOfUseOrProcessing.CGL_LocationUse = PlaceOfUseOrProcessingLocationUseList.Codes.PlacesOfUseOrProcessing;
		}
	}
}
