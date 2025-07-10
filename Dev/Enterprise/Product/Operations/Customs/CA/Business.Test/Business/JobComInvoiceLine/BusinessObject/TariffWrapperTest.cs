using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TariffWrapperTest : TestCaseWithFactory
	{
		public void TestTariffWrapper()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariffView1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "2709000030", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, "Global Tariff 2709000030");
			universalHelper.CreateTariffUOM(tariffView1, "CU1", "MTQ");
			universalHelper.CreateTariffAttribute(Constants.RefCusTariffAttribute.AttributeName.ConveyanceRequired, Constants.RefCusTariffAttribute.AttributeValue.Y, tariffView1);

			var tariffView2 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "2709000031", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, "Global Tariff 2709000031");
			universalHelper.CreateTariffUOM(tariffView2, "CU2", "MTQ");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invHeader.JobComInvoiceLines.AddNew();
			AssertTariffWrapper(invoiceLine1, ZString.Empty, ZString.Empty, ZString.Empty, false);

			invoiceLine1.JI_Tariff = "2709000030";
			AssertTariffWrapper(invoiceLine1, "2709000030", "Global Tariff 2709000030", "MTQ", true);

			var invoiceLine2 = invHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2709000031";
			AssertTariffWrapper(invoiceLine2, "2709000031", "Global Tariff 2709000031",  ZString.Empty, false);
		}

		void AssertTariffWrapper(JobComInvoiceLine invoiceLine, ZString tariffCode, ZString description, ZString unit, ZBool conveyanceIDRequired)
		{
			var wrapper = new TariffWrapper(invoiceLine);
			var tariffData = (ITariffData)wrapper;
			AssertEquals(tariffData.TariffCode, tariffCode);
			AssertEquals(tariffData.TariffDescription, description);
			AssertEquals(tariffData.TariffUnits, unit);
			AssertEquals(tariffData.ConveyanceIDRequired, conveyanceIDRequired);
			var tariff = (ITariff)wrapper;
			AssertEquals(tariff.Code, tariffCode);
			AssertEquals(tariff.Description, description);
			AssertEquals(tariff.UQ1, unit);
			AssertEquals(tariff.UQ2, ZString.Empty);
			AssertEquals(tariff.UQ3, ZString.Empty);
			AssertEquals(tariff.UQ4, ZString.Empty);
			AssertEquals(tariff.UQ5, ZString.Empty);
		}
	}
}
