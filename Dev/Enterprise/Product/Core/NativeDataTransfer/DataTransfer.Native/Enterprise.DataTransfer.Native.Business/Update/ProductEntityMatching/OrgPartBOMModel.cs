using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching
{
	class OrgPartBOMModel
	{
		public OrgPartBOMModel(OrgPartBOM bom)
			: this(bom.PK.ToGuid(), bom.Component, bom.PackType, bom.OE_ComponentQty)
		{
		}

		public OrgPartBOMModel(Guid pk, OrgSupplierPart component, RefPackType packType, decimal componentQuantity)
		{
			PK = pk;
			Component = component;
			PackType = packType;
			ComponentQuantity = componentQuantity;

			TotalComponentsInUse = 0m;
		}

		public Guid PK { get; }
		public OrgSupplierPart Component { get; }
		public RefPackType PackType { get; }
		public decimal ComponentQuantity { get; }

		public decimal TotalComponentsInUse { get; set; }
		public decimal TotalComponentStockQuantity =>
			(Component != null && PackType != null)
				? (decimal)Component.UnitConverter.Convert(ComponentQuantity, PackType.F3_Code, Component.OP_StockKeepingUnit)
				: 0m;

		public string ToKey() => ToKey(Component, PackType);
		public static string ToKey(OrgSupplierPart component, RefPackType packType) => string.Format("{0} - {1}", component?.OP_PartNum, packType?.F3_Code);
	}
}
