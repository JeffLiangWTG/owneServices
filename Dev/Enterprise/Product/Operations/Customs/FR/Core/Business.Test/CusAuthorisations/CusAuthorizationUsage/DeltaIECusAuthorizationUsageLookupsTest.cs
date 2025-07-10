using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	sealed class DeltaIECusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_DeltaIE()
		{
			var config = new RefDataConfig(
				dataGroupings:
				[
					new(code: "EUN", description: "European Union", parent: ""),
					new(code: "DIE", description: "France IE", parent: "EUN")
				],
				cusCodeTypes:
				[
					new(typeCode: "AUTH", description: "Authorization", attributeTypes: [new(name: "IsImport", dataGrouping: "DIE")],
						cusCodes:
						[
							new(code: "C600", dataGrouping: "DIE", attributes: [new(name: "IsImport", value: "1")])
						]
					)
				]
			);
			EUUniversalTestDataHelper.SetUpTestRefData(Factory, config);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			cusAuthorizationUsage.AGC_Number = "12345";
			cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

			var authorizationUsageCodeList = cusAuthorizationUsage.Lookups.CodeList;
			var authorizationUsageCodeDescriptionPairList = (ReadOnlyCodeDescriptionPairList)authorizationUsageCodeList;
			AssertEquals("Code C600 is retrieved", expected: true, authorizationUsageCodeDescriptionPairList.ContainsCode("C600"));
			AssertEquals("Code AUL is not retrieved", expected: false, authorizationUsageCodeDescriptionPairList.ContainsCode("AUL"));
		}
	}
}
