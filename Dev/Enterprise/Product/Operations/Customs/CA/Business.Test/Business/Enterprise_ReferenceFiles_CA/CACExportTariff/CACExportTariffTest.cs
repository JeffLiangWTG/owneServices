using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACExportTariff))]
	sealed class CACExportTariffTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCodeDescription()
		{
			CACExportTariff tariff = Factory.New<CACExportTariff>();
			tariff.CE_Code = "0000000000";
			tariff.CE_Description = "Short description";

			AssertEquals("Code", "0000000000", CodePropertyAttribute.CodeFromBusinessObject(tariff));
			AssertEquals("Desc", "Short description", DescriptionPropertyAttribute.DescriptionFromBusinessObject(tariff));
		}

		public void TestITariffDataMembers()
		{
			CACExportTariff tariff = Factory.New<CACExportTariff>();
			tariff.CE_Code = "0000000000";
			tariff.CE_Description = "Short description";
			tariff.CE_Unit = "KGM";
			tariff.CE_IsConveyanceIDRequired = ZBool.True;
			AssertEquals("TariffCode", "0000000000", ((ITariffData)tariff).TariffCode);
			AssertEquals("TariffDescription", "Short description", ((ITariffData)tariff).TariffDescription);
			AssertEquals("TariffUnits", "KGM", ((ITariffData)tariff).TariffUnits);
			AssertEquals("ConveyanceIDRequired", ZBool.True, ((ITariffData)tariff).ConveyanceIDRequired);
		}

		public void TestFormattedCode()
		{
			CACExportTariff tariff = Factory.New<CACExportTariff>();
			AssertEquals("CE_FormattedCode", "", tariff.CE_FormattedCode);
			tariff.CE_Code = "1234567890";
			AssertEquals("CE_FormattedCode", "1234.56.78 90", tariff.CE_FormattedCode);
		}

		public void TestITariff()
		{
			var tariff = Factory.New<CACExportTariff>();
			tariff.CE_Code = "1234567890";
			tariff.CE_Description = "Long description";
			tariff.CE_Unit = "KGM";
			var iTariff = (ITariff)tariff;
			AssertEquals("1234567890", iTariff.Code);
			AssertEquals("Long description", iTariff.Description);
			AssertEquals("KGM", iTariff.UQ1);
			AssertEquals("", iTariff.UQ2);
			AssertEquals("", iTariff.UQ3);
			AssertEquals("", iTariff.UQ4);
			AssertEquals("", iTariff.UQ5);
		}
	}
}
