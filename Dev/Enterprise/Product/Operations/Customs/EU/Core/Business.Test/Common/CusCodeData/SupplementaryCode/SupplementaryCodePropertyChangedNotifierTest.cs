using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq.Protected;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class SupplementaryCodePropertyChangedNotifierTest : BaseSupplementaryCodePropertyChangedNotifierTest
	{
		[ExpectNoExceptions]
		public override void TestDefaultUOMsBySupplementaryCode1()
		{
			var (tariff, rate, _, _, _, _, rate5, _)
				= UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "EXP");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			var invoiceLine = invoiceLineMock.Object;
			invoiceLineMock.Protected().Setup<bool>("UseUniversalTariffCore").Returns(true);
			invoiceLineMock.Setup(m => m.UniversalDutyRate).Returns(rate);
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "11111111";

			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsThirdUnitQty = "";
			invoiceLine.JI_SupplementaryCode1 = "Q038";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsUnitQty, NUnit.Framework.Is.EqualTo("CU1").Using(CustomComparers.TypeComparison), "First UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsSecondUnitQty, NUnit.Framework.Is.EqualTo("CU2").Using(CustomComparers.TypeComparison), "Second UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsThirdUnitQty, NUnit.Framework.Is.EqualTo("UM1").Using(CustomComparers.TypeComparison), "Third UQ");
			});

			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsThirdUnitQty = "";
			invoiceLine.JI_SupplementaryCode1 = "";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsUnitQty, NUnit.Framework.Is.EqualTo("CU1").Using(CustomComparers.TypeComparison), "First UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsSecondUnitQty, NUnit.Framework.Is.EqualTo("CU2").Using(CustomComparers.TypeComparison), "Second UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsThirdUnitQty, NUnit.Framework.Is.EqualTo("UM1").Using(CustomComparers.TypeComparison), "Third UQ");
			});
		}

		[ExpectNoExceptions]
		public override void TestDefaultUOMsBySupplementaryCode2()
		{
			var (tariff, rate, _, _, _, _, rate5, _)
				= UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "EXP");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			var invoiceLine = invoiceLineMock.Object;
			invoiceLineMock.Protected().Setup<bool>("UseUniversalTariffCore").Returns(true);
			invoiceLineMock.Setup(m => m.UniversalDutyRate).Returns(rate);
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "11111111";

			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsThirdUnitQty = "";
			invoiceLine.JI_SupplementaryCode2 = "Q038";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsUnitQty, NUnit.Framework.Is.EqualTo("CU1").Using(CustomComparers.TypeComparison), "First UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsSecondUnitQty, NUnit.Framework.Is.EqualTo("CU2").Using(CustomComparers.TypeComparison), "Second UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsThirdUnitQty, NUnit.Framework.Is.EqualTo("UM1").Using(CustomComparers.TypeComparison), "Third UQ");
			});

			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsThirdUnitQty = "";
			invoiceLine.JI_SupplementaryCode2 = "";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsUnitQty, NUnit.Framework.Is.EqualTo("CU1").Using(CustomComparers.TypeComparison), "First UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsSecondUnitQty, NUnit.Framework.Is.EqualTo("CU2").Using(CustomComparers.TypeComparison), "Second UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsThirdUnitQty, NUnit.Framework.Is.EqualTo("UM1").Using(CustomComparers.TypeComparison), "Third UQ");
			});
		}

		[ExpectNoExceptions]
		public override void TestDefaultUOMsByAdditionalSupplementaryCodes()
		{
			var (tariff, rate, _, _, _, _, rate5, _)
				= UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "EXP");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			var invoiceLine = invoiceLineMock.Object;
			invoiceLineMock.Protected().Setup<bool>("UseUniversalTariffCore").Returns(true);
			invoiceLineMock.Setup(m => m.UniversalDutyRate).Returns(rate);
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "11111111";

			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsThirdUnitQty = "";
			var additionalSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode.CY_Code = "Q038";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsUnitQty, NUnit.Framework.Is.EqualTo("CU1").Using(CustomComparers.TypeComparison), "First UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsSecondUnitQty, NUnit.Framework.Is.EqualTo("CU2").Using(CustomComparers.TypeComparison), "Second UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsThirdUnitQty, NUnit.Framework.Is.EqualTo("UM1").Using(CustomComparers.TypeComparison), "Third UQ");
			});

			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsThirdUnitQty = "";
			invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsUnitQty, NUnit.Framework.Is.EqualTo("CU1").Using(CustomComparers.TypeComparison), "First UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsSecondUnitQty, NUnit.Framework.Is.EqualTo("CU2").Using(CustomComparers.TypeComparison), "Second UQ");
				NUnit.Framework.Assert.That(invoiceLine.JI_CustomsThirdUnitQty, NUnit.Framework.Is.EqualTo("UM1").Using(CustomComparers.TypeComparison), "Third UQ");
			});
		}
	}
}
