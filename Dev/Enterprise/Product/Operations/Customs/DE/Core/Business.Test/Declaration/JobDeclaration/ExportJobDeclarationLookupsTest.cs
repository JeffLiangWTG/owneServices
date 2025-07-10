using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using EURefCusCodeListTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ExportJobDeclarationLookupsTest : EU.Business.Declaration.Testing.JobDeclarationLookupsTest<ExportJobDeclarationLookups, JobDeclaration>
	{
		public void TestGoodsOrigin()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EX15, "Origin country/territory for entry style EX");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_CO15, "Origin country/territory for entry style CO");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EU15, "Origin country/territory for entry style EU");

			helper.CreateCusCodeList("DE", EURefCusCodeListTypes.Code_EX15, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_CO15, "BB", "Test BB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_CO15, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_EU15, "DD", "Test DD", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_EU15, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

			Factory.Save();

			CombineAssertions(() =>
			{
				jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
				AssertEquals("EX Entry Style", "AA", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);

				jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
				AssertEquals("CO Entry Style", "BB", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);

				jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
				AssertEquals("EU Entry Style", "DD", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);
			});
		}

		public void TestGoodsDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EX17, "Destination country/territory for entry style EX");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_CO17, "Destination country/territory for entry style CO");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EU17, "Destination country/territory for entry style EU");

			helper.CreateCusCodeList("DE", EURefCusCodeListTypes.Code_EX17, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_CO17, "BB", "Test BB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_CO17, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_EU17, "DD", "Test DD", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_EU17, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

			Factory.Save();

			CombineAssertions(() =>
			{
				jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
				AssertEquals("EX Entry Style", "AA", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);

				jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
				AssertEquals("CO Entry Style", "BB", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);

				jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
				AssertEquals("EU Entry Style", "DD", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);
			});
		}

		public void TestCustomsOffices()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			var cusOffice = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE EXP", "DE EXP", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			helper.CreateCusCodeListAttribute(cusOffice.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var mainOfficeRequirement = declaration.CustomsOfficeRequirementHelper.MainOffice;
				mainOfficeRequirement.OfficeRole = "EXT";

				mainOfficeRequirement.IsLocalCountryOnly = true;
				var customsOffices = declaration.Lookups.CustomsOffices;
				customsOffices.Load();

				var cusCodeList = customsOffices.Cast<ZZRefCusCodeListCombined>().Single();

				AssertEquals("Attributes Count", 2, cusCodeList.Attributes.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "EXT", "EXP" }, cusCodeList.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Select(x => x.ZZE_Value));
			}
		}

		public void TestFinalDestinations()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var loader = new RefUNLOCO.Loader(Factory);
			var sydney = loader.Load("AUSYD");
			var aglona = loader.Load("LVAGL");
			var helgoland = loader.Load("DEHGL");
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.FinalDestinations;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Contains eu ports", true, filter.Contains(aglona));
				AssertEquals("Contains local ports", true, filter.Contains(helgoland));
			});
		}

		protected override ZString EntryStatusListCodeType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus;
	}
}
