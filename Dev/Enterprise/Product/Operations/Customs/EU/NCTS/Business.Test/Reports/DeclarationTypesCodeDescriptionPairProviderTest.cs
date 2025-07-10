using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class DeclarationTypesCodeDescriptionPairProviderTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				var declarationTypeCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType;
				var europeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunId);
				helper.CreateNewOrGetExistingCusCodeType(declarationTypeCode, "NCTS Declaration Type (Box 1)");
				helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, declarationTypeCode, "T1", "Goods moving under external community transit procedure", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, declarationTypeCode, "T2", "Goods moving under internal community transit procedure", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				var declarationTypeCodeCH = "N0231";
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, declarationTypeCodeCH, "T1CH", "Mixed consignments comprising both goods to be placed under external Union transit procedure", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, declarationTypeCodeCH, "T2CH", "Goods not having the customs status of Union goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var provider = new DeclarationTypesCodeDescriptionPairProvider();
				var codeDescriptionPairList = provider.GetCodeDescriptionPairList();
				AssertEquals("EU CodesAsString", "T1, T2", codeDescriptionPairList.CodesAsString);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
				{
					codeDescriptionPairList = provider.GetCodeDescriptionPairList();
					AssertEquals("CH CodesAsString", "T1CH, T2CH", codeDescriptionPairList.CodesAsString);
				}
			}
		}
	}
}
