using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.EMCS.Testing
{
	[TestedType(typeof(EMCSInvoiceLine))]
	sealed class EMCSInvoiceLineTest : DocumentWrappers.Testing.DocBaseWrapperTest
	{
		public void TestLineNo()
		{
			invoiceLine.JI_LineNo = 1;
			AssertEquals("LineNo", new ZShort(1), wrapper.LineNo);
		}

		public void TestBox18PackagesMarksAndDescription()
		{
			EMCSDeclarationWrapperTestHelper.SetupReferenceTestData(Factory);
			invoiceLine.ZG_AlcoholicStrength = 15.5m;
			invoiceLine.ZG_Density = 10.4m;
			invoiceLine.JI_NDescription = "Line Desc";
			var package1 = new PackageTestData(invoiceLine.JI_NDescription, 100, "BX", $"Shipping Mark {invoiceLine.JI_LineNo}", 15.5m, 10.4m, countable: true);
			var package2 = new PackageTestData(invoiceLine.JI_NDescription, 1, "PL", "Shipping Mark", 15.5m, 10.4m, countable: false);
			EMCSDeclarationWrapperTestHelper.AddPackages(invoiceLine, new[] { package1, package2 });
			AssertEquals("Box18PackagesMarksAndDescription", $"{package1.GetExpected()}{System.Environment.NewLine}{package2.GetExpected()}", wrapper.Box18PackagesMarksAndDescription);
		}

		public void TestBox19CommodityCode()
		{
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("Box19CommodityCode", "1234567890", wrapper.Box19CommodityCode);
		}

		public void TestBox20Quantity()
		{
			invoiceLine.JI_CustomsQuantity = 100;
			invoiceLine.JI_CustomsUnitQty = "1";
			AssertEquals("Box20Quantity", "100 | KG", wrapper.Box20Quantity);
		}

		public void TestBox21GrossMass()
		{
			invoiceLine.JI_Weight = 100;
			invoiceLine.JI_WeightUQ = "T";
			AssertEquals("Box21GrossMass", "100 | T", wrapper.Box21GrossMass);
		}

		public void TestBox22NetMass()
		{
			invoiceLine.JI_NetWeight = 500;
			invoiceLine.JI_NetWeightUQ = "KG";
			AssertEquals("Box22NetMass", "500 | KG", wrapper.Box22NetMass);
		}

		public void TestBox23CustomsStatus()
		{
			AssertEquals("Box23CustomsStatus", ZString.Empty, wrapper.Box23CustomsStatus);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper() => wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			invoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine.ZG_IsMainPack = true;
			wrapper = EMCSInvoiceLine.New(invoiceLine, Factory);
		}
		EMCSJobComInvoiceLine invoiceLine;
		EMCSInvoiceLine wrapper;
	}
}
