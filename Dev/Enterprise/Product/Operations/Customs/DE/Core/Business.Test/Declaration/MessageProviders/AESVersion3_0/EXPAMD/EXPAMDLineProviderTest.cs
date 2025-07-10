using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPAMDLineProvider))]
	class EXPAMDLineProviderTest : AESLineProviderAbstractTest<EXPAMDLineProvider>
	{
		public void TestCommercialReferenceNumber()
		{
			_header.Reset();
			_header.Setup(h => h.CommercialReferenceNumber).Returns("ABC");
			invoice.JZ_UCR = "ABC";
			AssertEquals(ZString.Empty, Provider.CommercialReferenceNumber);
		}

		public void TestCommercialReferenceNumber_EmptyInHeader()
		{
			_header.Reset();
			_header.Setup(h => h.CommercialReferenceNumber).Returns(ZString.Empty);
			invoice.JZ_UCR = "ABC";
			AssertEquals("ABC", Provider.CommercialReferenceNumber);
		}

		public void TestCommoditySpecified()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
				AssertEquals("Style4thDigitIsNot2", ZBool.False, Provider.CommoditySpecified);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				AssertEquals("Style4thDigitIs2", ZBool.True, Provider.CommoditySpecified);
			});
		}

		public void TestStatisticalValueSpecified()
		{
			CombineAssertions(() =>
			{
				InvoiceLineCharge invoiceLineCharge = invoiceLine.Charges.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
				invoiceLineCharge.J7_ChargeType = ZString.Empty;
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
				AssertEquals("No currency, chargecode or commodity", false, Provider.StatisticalValueSpecified);

				invoiceLineCharge.J7_ChargeType = ChargeTypeList.Codes.StatisticalValue;
				AssertEquals("only charges", false, Provider.StatisticalValueSpecified);

				invoice.JZ_RX_NKInvoice_Currency = "EUR";
				AssertEquals("Only charges and currency", false, Provider.StatisticalValueSpecified);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				AssertEquals("Commodity, charges currency", true, Provider.StatisticalValueSpecified);
			});
		}

		public void TestNetMass()
		{
			invoiceLine.JI_CustomsQuantity = 2.25m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CustomsQuantity = 1100m;
			AssertEquals("Value in KGM", 1102.25m, Provider.NetMass);
		}

		public void TestNetMass_Round3()
		{
			invoiceLine.JI_CustomsQuantity = 2.2514m;
			AssertEquals(2.251m, Provider.NetMass);
		}

		public void TestNetMass_Normalize()
		{
			invoiceLine.JI_CustomsQuantity = 2.2000m;
			AssertEquals(2.2m, Provider.NetMass);
		}

		public void TestGrossMass()
		{
			invoiceLine.JI_Weight = 3.25m;
			invoiceLine.JI_WeightUQ = Constants.Weight.Kilograms;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Weight = 2000000m;
			invoiceLine2.JI_WeightUQ = Constants.Weight.Milligrams;

			AssertEquals("Value", 5.25m, Provider.GrossMass);
		}

		public void TestGrossMass_Round3()
		{
			invoiceLine.JI_Weight = 3.2513m;
			invoiceLine.JI_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals(3.251m, Provider.GrossMass);
		}

		public void TestGrossMass_Normalize()
		{
			invoiceLine.JI_Weight = 3.2000m;
			invoiceLine.JI_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals(3.2m, Provider.GrossMass);
		}

		public void TestGrossMass_Invalid()
		{
			invoiceLine.JI_Weight = 3.2000m;
			invoiceLine.JI_WeightUQ = ZString.Empty;
			AssertEquals(ZDecimal.Zero, Provider.GrossMass);
		}

		public void TestPackages()
		{
			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			var package1Pivot = package1.InvoiceLinePivotCollection.AddNew();
			package1Pivot.CHC_JI = invoiceLine.PK;
			var package2 = declaration.Packages.AddNew();
			var package2Pivot = package2.InvoiceLinePivotCollection.AddNew();
			package2Pivot.CHC_JI = invoiceLine.PK;

			CombineAssertions(() =>
			{
				var packages = Provider.Packages;
				AssertEquals("Count", 2, packages.Count);
				AssertEquals("Cached", packages, Provider.Packages);
			});
		}

		readonly Mock<IEXPAMDHeader> _header = new Mock<IEXPAMDHeader>();

		protected override EXPAMDLineProvider GetProvider() => new EXPAMDLineProvider(entryLine, _header.Object);

		new IEXPAMDLine Provider => base.Provider;
	}
}
