using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.JP.Business.Testing
{
	public static class TestDataCoreHelper
	{
		public static void CreateRefDataListForTest(this BusinessObjectFactory factory, string codeTypeValue, string description, params string[] codes)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var dataGroupingValue = Core.Constants.CountryCodes.Japan;
			helper.CreateNewOrGetExistingCusCodeType(codeTypeValue, description);
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingValue);
			codes.ForEach(c => helper.CreateNewOrGetExistingCusCodeList(dataGroupingValue, codeTypeValue, c, $"{c} Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1)));
			factory.Save();
		}

		public static void PrepareTradeControlOrderAppendixList(this BusinessObjectFactory factory)
		{
			factory.CreateRefDataListForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix, "Japan Export Trade Control Ordinance Appendix", "12345", "22345");
			factory.CreateRefDataListForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanImportTradeControlOrdinanceAppendix, "Japan Import Trade Control Ordinance Appendix", "1234", "2345");
		}

		public static void SetupECRTestingContext(this CusEntryInstruction entryInstruction)
		{
			var declaration = entryInstruction.JobDeclaration;
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			entryInstruction.ExportControlNumber = ZString.Empty;
		}
	}
}
