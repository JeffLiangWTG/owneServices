using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class EUAddInfoEuOfficeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOfficeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeGB000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeGB000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExport;
			var officeCodeList = officeCode.Lookups.OfficeCodeList;
			officeCodeList.Load();
			NUnit.Framework.Assert.That(officeCodeList.Count, NUnit.Framework.Is.EqualTo(1));
			AssertContainsExactElementsInAnyOrder(new[] { "GB000001" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

			officeCode.CY_Code = ZString.Empty;
			officeCodeList = officeCode.Lookups.OfficeCodeList;
			officeCodeList.Load();
			NUnit.Framework.Assert.That(officeCodeList.Count, NUnit.Framework.Is.EqualTo(2));
			AssertContainsExactElementsInAnyOrder(new[] { "GB000001", "IEDUB100" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		[ExpectNoExceptions]
		public void TestPurposeList()
		{
			var dec = Factory.New<Integration.Customs.EUEMCS.IJobDeclaration>();
			var officeCode = ((IEuOfficeCodeCollectionSupporter)dec).CustomsOffices.AddNew();
			var officeCodeList = new OfficeCodes_EMCS();
			officeCodeList.RemoveCode(OfficeCodes_EMCS.Codes.OfficeOfDestination);
			NUnit.Framework.Assert.That(officeCode.Lookups.CY_CodeList, NUnit.Framework.Is.EquivalentTo(officeCodeList).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPurposeList_UseDesInsteadOfCaa()
		{
			var dec = Factory.New<Integration.Customs.IEEMCS.IEMCSJobDeclaration>();
			var officeCode = ((IEuOfficeCodeCollectionSupporter)dec).CustomsOffices.AddNew();
			var officeCodeList = new OfficeCodes_EMCS();
			officeCodeList.RemoveCode(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);
			NUnit.Framework.Assert.That(officeCode.Lookups.CY_CodeList, NUnit.Framework.Is.EquivalentTo(officeCodeList).Using(CustomComparers.TypeComparison));
		}

		public void TestOfficeCodeList_IsRelevantToIsLocalCountryOnlyProperty()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE EXT", "DE EXT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT", yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT EXT", "IT EXT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT ENT", "IT ENT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "ENT");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var extReq = declaration.CustomsOfficeRequirementHelper.OtherRequirements.First(x => x.OfficeRole == "EXT");
				var extOffice = declaration.CustomsOffices.Cast<EuOfficeCode>().First(x => x.CY_Code == "EXT");

				extReq.IsLocalCountryOnly = true;
				var officeCodeList = extOffice.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "IT EXT" }, officeCodeList.Select(x => x.ZZD_Code));

				extReq.IsLocalCountryOnly = false;
				officeCodeList = extOffice.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "IT EXT", "DE EXT" }, officeCodeList.Select(x => x.ZZD_Code));
			}
		}

		[ExpectNoExceptions]
		public void TestJobDeclarationOfficeCodeList_ComesFromCustomsOfficeRequirements()
		{
			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
			{
				new CustomsOfficeRequirement
				{
					OfficeRole = "CAU",
				},
				new CustomsOfficeRequirement
				{
					OfficeRole = "DEP",
				},
				new CustomsOfficeRequirement
				{
					OfficeRole = "DES",
				}
			});

			var office = declaration.CustomsOffices.AddNew();
			NUnit.Framework.Assert.That(office.Lookups.CY_CodeList.GetAllCodes(), NUnit.Framework.Is.EquivalentTo(new[] { "CAU", "DEP", "DES" }));
		}

		public void TestOfficeCodeList_IsRelevantToIsForeignCountryOnlyProperty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT EXT", "IT EXT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LV EXT", "LV EXT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE EXT", "DE EXT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE ENT", "DE ENT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var extReq = declaration.CustomsOfficeRequirementHelper.OtherRequirements.First(x => x.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfExit);
				var extOffice = declaration.CustomsOffices.Cast<EuOfficeCode>().First(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit);

				CombineAssertions(() =>
				{
					extReq.IsForeignCountryOnly = true;
					var officeCodeList = extOffice.Lookups.OfficeCodeList;
					officeCodeList.Load();
					AssertContainsExactElementsInAnyOrder("IsForeignCountryOnly = True", new[] { "DE EXT", "LV EXT" }, officeCodeList.Select(x => x.ZZD_Code));

					extReq.IsForeignCountryOnly = false;
					officeCodeList = extOffice.Lookups.OfficeCodeList;
					officeCodeList.Load();
					AssertContainsExactElementsInAnyOrder("IsForeignCountryOnly = False", new[] { "IT EXT", "DE EXT", "LV EXT" }, officeCodeList.Select(x => x.ZZD_Code));
				});
			}
		}
	}
}
