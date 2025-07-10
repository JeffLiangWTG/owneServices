using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPackageLabel))]
	sealed class DocWhsPackageLabelTest : DocWhsLabelTest
	{
		#region Properties

		public void TestSelectedOrderLine()
		{
			AssertEquals(DocOrderLine, labelWrapperOrderLine.SelectedOrderLine);
		}

		public void TestProductCodeBarcode()
		{
			AssertEquals(ZString.Empty, labelWrapper.ProductCode);
			AssertEquals(ZString.Empty, labelWrapperOrderLine.ProductCode);
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "ProductCode";
			OrderLine.WE_OP = part.PK;
			TextBarcode barcode = new TextBarcode(labelWrapperOrderLine.ProductCode);
			AssertEquals(barcode.TextAs128sFontString, labelWrapperOrderLine.ProductCodeBarcode);

			WhsPickableDocket order = Factory.New<WhsPickableDocket>();
			order.Lines.Add(OrderLine);
			OrgHeader consignee = Factory.New<OrgHeader>();
			order.ConsigneePK = consignee.PK;
			OrgPartRelation orgPart = part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = consignee.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			orgPart.OU_LocalPartNumber = "LOCALCODE";
			barcode = new TextBarcode(labelWrapperOrderLine.ProductCode);
			AssertEquals(barcode.TextAs128sFontString, labelWrapperOrderLine.ProductCodeBarcode);
		}

		public void TestProductCode()
		{
			AssertEquals(ZString.Empty, labelWrapper.ProductCode);
			AssertEquals(ZString.Empty, labelWrapperOrderLine.ProductCode);
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "ProductCode";
			OrderLine.WE_OP = part.PK;
			AssertEquals("PRODUCTCODE", labelWrapperOrderLine.ProductCode);

			WhsPickableDocket order = Factory.New<WhsPickableDocket>();
			order.Lines.Add(OrderLine);
			OrgHeader consignee = Factory.New<OrgHeader>();
			order.ConsigneePK = consignee.PK;
			OrgPartRelation orgPart = part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = consignee.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			orgPart.OU_LocalPartNumber = "LOCALCODE";
			AssertEquals("Product Code", "LOCALCODE", labelWrapperOrderLine.ProductCode);
		}

		public void TestProductDesc()
		{
			AssertEquals(ZString.Empty, labelWrapperOrderLine.ProductDesc);
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_Desc = "ProductDescription";
			OrderLine.WE_OP = part.PK;
			AssertEquals("ProductDescription", labelWrapperOrderLine.ProductDesc);

			WhsPickableDocket order = Factory.New<WhsPickableDocket>();
			order.Lines.Add(OrderLine);
			OrgHeader consignee = Factory.New<OrgHeader>();
			order.ConsigneePK = consignee.PK;
			OrgPartRelation orgPart = part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = consignee.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			orgPart.OU_LocalPartDescription = "LOCAL DESCRIPTION";
			AssertEquals("Product Description", "LOCAL DESCRIPTION", labelWrapperOrderLine.ProductDesc);
		}

		public void TestComponentFlag()
		{
			AssertEquals(ZString.Empty, labelWrapperOrderLine.ComponentFlag);
			WhsWorkOrderLine workOrderLine = Factory.NewWithValidTestData<WhsWorkOrderLine>();
			DocOrderLine = DocWhsPickableDocketLine.New(workOrderLine, Factory);
			labelWrapperOrderLine = DocWhsPackageLabel.New(labelObj, DocOrderLine, Factory);
			AssertEquals("BOM Component Part", labelWrapperOrderLine.ComponentFlag);

			WhsOrderLine orderLine = Factory.NewWithValidTestData<WhsOrderLine>();
			DocOrderLine = DocWhsPickableDocketLine.New(orderLine, Factory);
			labelWrapperOrderLine = DocWhsPackageLabel.New(labelObj, DocOrderLine, Factory);

			AssertEquals(ZString.Empty, labelWrapperOrderLine.ComponentFlag);
		}

		public void TestFirstDate()
		{
			AssertEquals(ZString.Empty, labelWrapperOrderLine.FirstDate);
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			OrderLine.WE_OP = part.PK;
			OrderLine.WE_ExpiryDate = ZDate.Today;

			AssertEquals("Expiry Date:", labelWrapperOrderLine.FirstDate);
			OrderLine.WE_PackingDate = ZDate.Today;
			AssertEquals("Expiry Date:", labelWrapperOrderLine.FirstDate);

			OrderLine.WE_ExpiryDate = ZDate.Empty;
			AssertEquals("Packing Date:", labelWrapperOrderLine.FirstDate);
		}

		public void TestSecondDate()
		{
			AssertEquals(ZString.Empty, labelWrapperOrderLine.SecondDate);
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			OrderLine.WE_OP = part.PK;
			AssertEquals(ZString.Empty, labelWrapperOrderLine.SecondDate);

			OrderLine.WE_ExpiryDate = ZDate.Today;
			AssertEquals(ZString.Empty, labelWrapperOrderLine.SecondDate);

			OrderLine.WE_PackingDate = ZDate.Today;
			AssertEquals("Packing Date:", labelWrapperOrderLine.SecondDate);
		}

		public void TestExpiryDate()
		{
			AssertEquals(ZDateTime.Empty, labelWrapperOrderLine.ExpiryDate);
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			OrderLine.WE_OP = part.PK;
			AssertEquals(ZDateTime.Empty, labelWrapperOrderLine.ExpiryDate);

			OrderLine.WE_PackingDate = ZDate.Today;
			AssertEquals(OrderLine.WE_PackingDate, labelWrapperOrderLine.ExpiryDate);

			OrderLine.WE_ExpiryDate = ZDate.Today;
			AssertEquals(OrderLine.WE_ExpiryDate, labelWrapperOrderLine.ExpiryDate);
		}

		public void TestPackingDateTemporary()
		{
			AssertEquals(ZDateTime.Empty, labelWrapperOrderLine.PackingDateTemporary);
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			OrderLine.WE_OP = part.PK;
			AssertEquals(ZDateTime.Empty, labelWrapperOrderLine.PackingDateTemporary);

			OrderLine.WE_PackingDate = ZDate.Today;
			AssertEquals(ZDateTime.Empty, labelWrapperOrderLine.PackingDateTemporary);

			OrderLine.WE_ExpiryDate = ZDate.Today;
			AssertEquals(OrderLine.WE_PackingDate, labelWrapperOrderLine.PackingDateTemporary);
		}

		#endregion

		#region Implementation

		WhsOrderLine OrderLine;
		DocWhsPickableDocketLine DocOrderLine;
		new DocWhsPackageLabel labelWrapper;
		DocWhsPackageLabel labelWrapperOrderLine;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocWhsPackageLabel.New(labelObj, Factory),
				};
		}

		protected override void SetUp()
		{
			labelObj = new WhsLabel();
			labelWrapper = DocWhsPackageLabel.New(labelObj, Factory);
			OrderLine = Factory.New<WhsOrderLine>();
			DocOrderLine = DocWhsPickableDocketLine.New(OrderLine, Factory);
			labelWrapperOrderLine = DocWhsPackageLabel.New(labelObj, DocOrderLine, Factory);
			base.SetUp();
		}

		#endregion
	}
}
