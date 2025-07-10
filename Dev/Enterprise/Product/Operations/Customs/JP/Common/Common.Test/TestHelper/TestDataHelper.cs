using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common.Testing
{
	public static class TestDataHelper
	{
		public static string CreateJapanBondedAreaCode(this BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var dataGroupingValue = Core.Constants.CountryCodes.Japan;
			var codeTypeValue = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode;
			var code = "16W66";

			helper.CreateNewOrGetExistingDataGrouping(dataGroupingValue);
			helper.CreateNewOrGetExistingCusCodeType(codeTypeValue, "Testdescription");

			var refCusCodeList = helper.CreateNewOrGetExistingCusCodeList(dataGroupingValue, codeTypeValue, code, "TestDescription", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateTransportModeForCusCodeList(refCusCodeList.PK, Core.Constants.TransportModes.Sea);
			factory.Save();

			return code;
		}

		public static void CreateRefDataForTest(this BusinessObjectFactory factory, string codeTypeValue, params string[] codes)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var dataGroupingValue = Core.Constants.CountryCodes.Japan;

			helper.CreateNewOrGetExistingDataGrouping(dataGroupingValue);
			helper.CreateNewOrGetExistingCusCodeType(codeTypeValue, "Testdescription");
			codes.ForEach(c => helper.CreateNewOrGetExistingCusCodeList(dataGroupingValue, codeTypeValue, c, $"{c} Description", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5)));

			factory.Save();
		}

		public static void CreateTariffData(this BusinessObjectFactory factory, ZString tariffTypeCode, params string[] tariffCodes)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Japan, tariffTypeCode);
			tariffType.ZZI_Description = "Japan Tariff";
			factory.Save();
			tariffCodes.ForEach(tariffCode => helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Japan, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, $"{tariffCode} Description"));
			factory.Save();
		}
		public static string GetResourceStream(string fileName)
		{
			using var stream = typeof(MessageDocumentSupporterTest).Assembly.GetManifestResourceStream($"Enterprise.Customs.JP.Common.Testing.TestFiles.{fileName}");
			return new StreamReader(stream).ReadToEnd();
		}

		public static void CreateUNLOCOData(this BusinessObjectFactory factory)
		{
			var unloco1 = factory.New<RefUNLOCO>();
			unloco1.RL_Code = "JP001";
			unloco1.RL_IATA = ZString.Empty;

			var unloco2 = factory.New<RefUNLOCO>();
			unloco2.RL_Code = "JP002";
			unloco2.RL_IATA = "TKU";

			factory.Save();
		}
	}
}
