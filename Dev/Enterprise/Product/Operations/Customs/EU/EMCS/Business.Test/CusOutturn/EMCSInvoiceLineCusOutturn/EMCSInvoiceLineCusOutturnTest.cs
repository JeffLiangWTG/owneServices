using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSInvoiceLineCusOutturn))]
	class EMCSInvoiceLineCusOutturnTest : EnterpriseBusinessObjectTestCase
	{
		public void TestC5_OutturnResultReason_Caption()
		{
			AssertEquals("Caption", "Explanation", DataBoundResourceStrings.GetDataForProperty(typeof(EMCSInvoiceLineCusOutturn), "C5_OutturnResultReason").Caption);
		}

		public void TestObservedDifference()
		{
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.ZG_DeclaredValue = 25.0001m;
			AssertEquals(75m, outturn.ObservedDifference);
		}

		public void TestValidation()
		{
			AssertType<EMCSInvoiceLineCusOutturnValidation>(outturn.Validation);
		}

		public void TestUnitQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = "AA";
			AssertEquals("AA", outturn.UnitQuantity);
		}

		public void TestActualQuantity()
		{
			invoiceLine.JI_CustomsQuantity = 100.0001m;
			AssertEquals(100m, outturn.ActualQuantity);
		}

		public void TestActualQuantity_Refused()
		{
			outturn.C5_RejectedQuantity = 20m;
			invoiceLine.JI_CustomsQuantity = 100.0001m;
			AssertEquals(80m, outturn.ActualQuantity);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusOutturnApplicationCodeList.Codes.EMC, outturn.C5_ApplicationCode);
		}

		protected override BusinessObject GetNewBusinessObject() => outturn;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var invoiceLine = GetNewEMCSLineForTest(factory);
			return invoiceLine.Outturn;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => outturn;

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = GetNewEMCSLineForTest(Factory);
			outturn = invoiceLine.Outturn;
		}
		EMCSJobComInvoiceLine invoiceLine;
		EMCSInvoiceLineCusOutturn outturn;

		EMCSJobComInvoiceLine GetNewEMCSLineForTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			_ = line.ZG_AlcoholicStrength; // Force to create AddInfo which leads to TestLightValidation doesn't access every AddInfoProperty and enforces to set CusOutturn.MarkAsNeedingValidation()
			return line;
		}
	}
}
