using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using JobDeclarationLookups = Enterprise.Customs.GB.Business.Declaration.JobDeclarationLookups;
using ZZCustomsFunctionalityEffectiveDate = Enterprise.Customs.Universal.ZZCustomsFunctionalityEffectiveDate;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApplicationCodeList_BLT_IsCdsFunctionalityEnabled_CDSAvailable()
		{
			var customsInterface = new LocalCountryCustomsInterface
			{
				SubmissionType = DeclarationApplicationCodeList.Codes.Builtin
			};
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
				{
					var declaration = Factory.New<JobDeclaration>();
					var lookups = declaration.Lookups;
					var list = lookups.ApplicationCodeList;
					CombineAssertions(() =>
					{
						AssertEquals("CodesAsString", "CDS", list.CodesAsString);
						AssertSame("Cached", list, lookups.ApplicationCodeList);
					});

					declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
					Factory.Save();
					CombineAssertions("Current is CHF", () =>
					{
						var list2 = lookups.ApplicationCodeList;
						AssertNotSame(list, list2);
						AssertEquals("CodesAsString", "CDS, CHF", list2.CodesAsString);
						AssertSame("Cached", list2, lookups.ApplicationCodeList);
					});
				}
			}
		}

		public void TestApplicationCodeList_NotBLT_IsCdsFunctionalityEnabled_CDSAvailable()
		{
			var customsInterface = new LocalCountryCustomsInterface
			{
				RecipientID = "RecipientID",
				SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced
			};
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
				{
					var declaration = Factory.New<JobDeclaration>();
					var lookups = declaration.Lookups;
					var list = lookups.ApplicationCodeList;
					CombineAssertions(() =>
					{
						AssertEquals("CodesAsString", "CDS, ITF", list.CodesAsString);
						AssertSame("Cached", list, lookups.ApplicationCodeList);
					});

					declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
					Factory.Save();
					CombineAssertions("Current is CHF", () =>
					{
						var list2 = lookups.ApplicationCodeList;
						AssertNotSame(list, list2);
						AssertEquals("CodesAsString", "CDS, CHF, ITF", list2.CodesAsString);
						AssertSame("Cached", list2, lookups.ApplicationCodeList);
					});
				}
			}
		}

		public void TestApplicationCodeList_EmptySubmissionType_IsDeveloper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = declaration.Lookups;
			var list = lookups.ApplicationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "CDS", list.CodesAsString);
				AssertSame("Cached", list, lookups.ApplicationCodeList);
			});
		}

		public void TestApplicationCodeList_EmptySubmissionType_NotCDSAvailable_IsDeveloper()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var lookups = declaration.Lookups;
				var list = lookups.ApplicationCodeList;
				CombineAssertions(() =>
				{
					AssertEquals("CodesAsString", "CDS", list.CodesAsString);
					AssertSame("Cached", list, lookups.ApplicationCodeList);
				});
			}
		}

		public void TestApplicationCodeList_EmptySubmissionType_IsCdsFunctionalityEnabled_CDSAvailable()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCodeExports, Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var lookups = declaration.Lookups;
				var list = lookups.ApplicationCodeList;
				CombineAssertions(() =>
				{
					AssertEquals("CodesAsString", "CDS", list.CodesAsString);
					AssertSame("Cached", list, lookups.ApplicationCodeList);
				});
			}
		}

		public void TestEidrTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = declaration.Lookups;
			AssertType<EidrTypeList>(lookups.EidrTypes);
			AssertEquals("CFS, DEL, SCD", lookups.EidrTypes.CodesAsString);
		}

		public void TestEntryStatusListForDefaultFallBack()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(!declaration.Lookups.EntryStatusList.ContainsCode(Customs.Common.EU.EntryStatusList.Codes.NotSent));
		}

		public void TestDeclarationTypeList_ChangeAsApplicationCodeChanges()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "A", "11", "11", "111", "One", "IMP", group: "SFD,ISD");
			helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "A", "22", "22", "222", "Two", "IMP", group: "SFD,ICR");
			helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "B", "33", "33", "333", "Three", "IMP", group: "ICR,ISF");
			helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "A", "44", "44", "444", "Four", "EXP", group: "EFD");
			helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "A", "55", "55", "555", "Five", "EXP", group: "EXS;EFD,ECR");
			helper.CreateRefCusProcedure("ZA", "B", "33", "33", "333", "Three", "IMP", group: "IFW");
			helper.CreateRefCusProcedure("ZA", "A", "55", "55", "555", "Five", "EXP", group: "ESD");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var declarationTypeListForCHIEF = declaration.CusEntryInstruction.Lookups.DeclarationTypeList;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var declarationTypeListForCDS = declaration.CusEntryInstruction.Lookups.DeclarationTypeList;

			AssertNotEquals(declarationTypeListForCHIEF.CodesAsString, declarationTypeListForCDS.CodesAsString);
		}

		public void TestModeOfTransportList()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var eunGroup = refHelper.CreateNewOrGetExistingDataGrouping("EUN");
			var gbGroup = refHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunGroup);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			AssertEquals(typeof(ModeOfTransportList), lookups.ModeOfTransportList.GetType());
			Assert(lookups.ModeOfTransportList.ContainsCode(GBModeOfTransportList.Codes._6_RoRoFreight));
		}

		public void TestTransportTypeList()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var eunGroup = refHelper.CreateNewOrGetExistingDataGrouping("EUN");
			var gbGroup = refHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunGroup);

			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			AssertEquals(typeof(GBCombinedTransportTypeList), lookups.TransportTypeList.GetType());
			Assert(lookups.TransportTypeList.ContainsCode(GBTransportTypeList.Codes.ROR));
		}

		public void TestDeclarationIncoTermList_ChangeAsTransportModeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var incoTermList = declaration.Lookups.IncoTermList;
			var seaItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeAlongsideShip);
			var roadItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeCarrier);
			var cdsItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.Other);
			AssertEquals("Default List", 8, incoTermList.Count);
			AssertNull("FAS does not exist in the IncoTermList", seaItem);
			AssertNotNull("FCA exists in the IncoTermList", roadItem);
			AssertNotNull("XXX exists in the IncoTermList", cdsItem);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			incoTermList = declaration.Lookups.IncoTermList;
			seaItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeAlongsideShip);
			roadItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeCarrier);
			cdsItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.Other);
			AssertEquals("IncoTermList for SEA", 12, incoTermList.Count);
			AssertNotNull("FAS exists in the IncoTermList", seaItem);
			AssertNotNull("FCA exists in the IncoTermList", roadItem);
			AssertNotNull("XXX exists in the IncoTermList", cdsItem);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			incoTermList = declaration.Lookups.IncoTermList;
			seaItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeAlongsideShip);
			roadItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeCarrier);
			cdsItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.Other);
			AssertEquals("IncoTermList for ALL", 8, incoTermList.Count);
			AssertNull("FAS does not exist in the IncoTermList", seaItem);
			AssertNotNull("FCA exists in the IncoTermList", roadItem);
			AssertNotNull("XXX exists in the IncoTermList", cdsItem);
		}

		public void TestLocationOfGoodsLookup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.Port, "Port");

			var code1 = helper.CreateCusCodeList(Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, RefCusCodeListTypes.Codes.Port, "AUAIRPORT1", "AU AIRPORT1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var attribute1 = helper.CreateCusCodeListAttribute(code1.PK, RefCusCodeList.Attributes.Facility, "AU");
			var attTransportMode1 = helper.CreateTransportModeForCusCodeList(code1.PK, TransportTypeList.Codes.Air);

			var code2 = helper.CreateCusCodeList(Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, RefCusCodeListTypes.Codes.Port, "AUAIRPORT2", "AU AIRPORT2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var attribute2 = helper.CreateCusCodeListAttribute(code2.PK, RefCusCodeList.Attributes.Facility, "AU");
			var attTransportMode2 = helper.CreateTransportModeForCusCodeList(code2.PK, TransportTypeList.Codes.Air);

			var code3 = helper.CreateCusCodeList(Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, RefCusCodeListTypes.Codes.Port, "AUPORT1", "AU PORT1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var attribute3 = helper.CreateCusCodeListAttribute(code3.PK, RefCusCodeList.Attributes.Facility, "AU");
			var attTransportMode3 = helper.CreateTransportModeForCusCodeList(code3.PK, TransportTypeList.Codes.Sea);

			var code4 = helper.CreateCusCodeList(Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, RefCusCodeListTypes.Codes.Port, "BUPORT1", "BU PORT1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var attribute4 = helper.CreateCusCodeListAttribute(code4.PK, RefCusCodeList.Attributes.Facility, "BU");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_Calc_LocationOtherInformationType = "AU";

			var locationList = declaration.Lookups.LocationOfGoods;
			AssertEquals("LocationList for AU AIR", 2, locationList.Count);
			var auAir = locationList.GetDescriptionFromCode("AUAIRPORT1");
			var auSea = locationList.GetDescriptionFromCode("AUPORT1");
			var buSea = locationList.GetDescriptionFromCode("BUPORT1");
			AssertNotNull("AU AIRPORT1 exists in the LocationOfGoodsList", auAir);
			AssertNull("AU AIRPORT2 does not exist in the LocationOfGoodsList", auSea);
			AssertNull("BU PORT1 does not exist in the LocationOfGoodsList", buSea);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			locationList = declaration.Lookups.LocationOfGoods;
			AssertEquals("LocationList for AU SEA", 1, locationList.Count);
			auAir = locationList.GetDescriptionFromCode("AUAIRPORT1");
			auSea = locationList.GetDescriptionFromCode("AUPORT1");
			buSea = locationList.GetDescriptionFromCode("BUPORT1");
			AssertNull("AU AIRPORT1 does not exist in the LocationOfGoodsList", auAir);
			AssertNotNull("AU AIRPORT2 exists in the LocationOfGoodsList", auSea);
			AssertNull("BU PORT1 does not exist in the LocationOfGoodsList", buSea);

			declaration.JE_Calc_LocationOtherInformationType = "BU";
			locationList = declaration.Lookups.LocationOfGoods;
			AssertEquals("LocationList for BU SEA", 1, locationList.Count);
			auAir = locationList.GetDescriptionFromCode("AUAIRPORT1");
			auSea = locationList.GetDescriptionFromCode("AUPORT1");
			buSea = locationList.GetDescriptionFromCode("BUPORT1");
			AssertNull("AU AIRPORT1 does not exist in the LocationOfGoodsList", auAir);
			AssertNull("AU AIRPORT2 does not exist in the LocationOfGoodsList", auSea);
			AssertNotNull("BU PORT1 exists in the LocationOfGoodsList", buSea);
		}

		public void TestPortFilters()
		{
			var dec = Factory.New<JobDeclaration>();
			var loader = new RefUNLOCO.Loader(Factory);

			var sydney = loader.Load("AUSYD");
			var belfast = loader.Load("GBBEL");
			var rotterdam = loader.Load("NLRTM");

			RefUNLOCOCollection filter;

			filter = dec.Lookups.Origins;
			Assert(filter.Contains(sydney));
			Assert(filter.Contains(rotterdam));
			Assert(filter.Contains(belfast));

			filter = dec.Lookups.PortOfLoadings;
			Assert(filter.Contains(sydney));
			Assert(filter.Contains(rotterdam));
			Assert(filter.Contains(belfast));

			filter = dec.Lookups.PortOfArrivals;
			Assert(filter.Contains(sydney));
			Assert(filter.Contains(rotterdam));
			Assert(filter.Contains(belfast));

			filter = dec.Lookups.FinalDestinations;
			Assert(filter.Contains(sydney));
			Assert(filter.Contains(rotterdam));
			Assert(filter.Contains(belfast));

			filter = dec.Lookups.PortOfFirstArrivals;
			Assert(!filter.Contains(sydney));
			Assert(filter.Contains(rotterdam));
			Assert(filter.Contains(belfast));
		}

		public void TestCustomsOffices_IsRelevantToIsLocalCountryOnlyProperty()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, "Northern Ireland");
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeType(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Germany, RefCusCodeListTypes.Codes.CustomsOffice, "DE EXT", "DE EXT", yesterday, tomorrow, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeList(Constants.CountryCodes.Italy, RefCusCodeListTypes.Codes.PackageTypes, "BOX", yesterday, tomorrow);
			helper.CreateCusCodeList(Constants.CountryCodes.Italy, RefCusCodeListTypes.Codes.CustomsOffice, "IT", yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Italy, RefCusCodeListTypes.Codes.CustomsOffice, "IT EXT", "IT EXT", yesterday, tomorrow, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.CustomsOffice, "GB EXT", "GB EXT", yesterday, tomorrow, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.CustomsOffice, "XI Belfast EXT for GB", "GB EXT", yesterday, tomorrow, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, RefCusCodeListTypes.Codes.CustomsOffice, "XI EXT", "XI EXT", yesterday, tomorrow, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var mainOfficeRequirement = declaration.CustomsOfficeRequirementHelper.MainOffice;
				mainOfficeRequirement.OfficeRole = EuOfficeCodesTypes.Codes.OfficeOfExit;

				declaration.ZG_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
				mainOfficeRequirement.IsLocalCountryOnly = true;
				var customsOffice = declaration.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "GB EXT", "XI EXT", "XI Belfast EXT for GB" }, customsOffice.Select(x => x.ZZD_Code));

				mainOfficeRequirement.IsLocalCountryOnly = false;
				customsOffice = declaration.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "IT EXT", "DE EXT", "XI EXT" }, customsOffice.Select(x => x.ZZD_Code));

				declaration.ZG_NorthernIrelandMode = NIModeList.Codes.NotToOrFromNi;
				customsOffice = declaration.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "GB EXT" }, customsOffice.Select(x => x.ZZD_Code));

				declaration.ZG_NorthernIrelandMode = NIModeList.Codes.ExportFromNiToRestOfWorld;
				customsOffice = declaration.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "IT EXT", "DE EXT", "GB EXT", "XI EXT", "XI Belfast EXT for GB" }, customsOffice.Select(x => x.ZZD_Code));
			}
		}

		public void TestCDSRouteOfEntryList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("CDSRouteOfEntryList", "1, 2, 3, 6, 0, H1, H2, H3, H6, H0, H", declaration.AddInfoLookups.RouteOfEntryList.CodesAsString);
		}

		public void TestCDSDeclarationStatusICSList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("CDSDeclarationStatusICSList", "1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23", declaration.AddInfoLookups.ImportClearanceStatusICSList.CodesAsString);
		}
	}
}
