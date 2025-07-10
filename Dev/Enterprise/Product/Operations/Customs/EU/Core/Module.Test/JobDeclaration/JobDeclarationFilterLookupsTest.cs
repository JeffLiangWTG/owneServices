using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Module.Testing
{
	class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestTransportTypeList()
		{
			var bizO = new JobDeclarationFilterBusinessObject();
			var moduleFilter = new JobDeclarationFilterLookups(bizO);
			AssertEquals("AIR, FIX, IWT, OWN, MAI, RAI, ROA, SEA", moduleFilter.TransportTypeList.CodesAsString);
			AssertEquals(moduleFilter.TransportTypeList, Factory.GetCachedValue<TransportTypeList>());
		}

		public void TestEntryStatusList()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var customsWareRegistry = ObjectFactory.Get<Integration.Customs.CustomsWare.ICustomsWareRegistry>();
				customsWareRegistry.Password.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TSTPW");

				var newFactory = new BusinessObjectFactory();
				var cusCodeHelper = new UniversalReferenceTestDataHelper(newFactory);
				var cusCodeType = cusCodeHelper.CreateNewOrGetExistingCusCodeType("CSTA", "Customs Status");
				cusCodeHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, "CSTA", "SIT", "TST DESC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
				cusCodeHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, "CSTA", "ACC", "BELGIUM SPECIAL ACC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
				newFactory.Save();

				CombineAssertions(() =>
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
					{
						var bizO = new JobDeclarationFilterBusinessObject();
						var moduleFilter = new JobDeclarationFilterLookups(bizO);
						var exp = new List<string>();
						exp.AddRange(new string[] { "SIT", "SUB", "ACK" });
						AssertContainsExactElementsInAnyOrder("IT List", exp, moduleFilter.EntryStatusList().GetAllCodes());
					}

					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Belgium))
					{
						var bizO = new JobDeclarationFilterBusinessObject();
						var moduleFilter = new JobDeclarationFilterLookups(bizO);
						var exp = new List<string>();
						exp.AddRange((new Common.EU.CustomsWareEntryStatusList()).GetAllCodes());
						AssertContainsExactElementsInAnyOrder("BE List", exp, moduleFilter.EntryStatusList().GetAllCodes());
						AssertEquals("BE Code in ZZ not overriden", "BELGIUM SPECIAL ACC", moduleFilter.EntryStatusList().GetDescriptionFromCode("ACC"));
					}

					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
					{
						var bizO = new JobDeclarationFilterBusinessObject();
						var moduleFilter = new JobDeclarationFilterLookups(bizO);
						var exp = new List<string>();
						exp.AddRange(new string[] { "SUB", "ACK" });
						exp.AddRange((new Common.EU.EntryStatusList()).GetAllCodes());
						AssertContainsExactElementsInAnyOrder("ES", exp, moduleFilter.EntryStatusList().GetAllCodes());
					}
				});
			}
		}

		public void TestMessageStatusList()
		{
			var newFactory = new BusinessObjectFactory();
			var expected = new List<string>(new string[] { "AWR", "FFT", "MLT", "NOT", "OK", "ERR", "UNK" });

			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Lithuania))
				{
					var bizO = new JobDeclarationFilterBusinessObject();
					var moduleFilter = new JobDeclarationFilterLookups(bizO);
					AssertContainsExactElementsInAnyOrder("Lithuania", expected, moduleFilter.MessageStatusList().GetAllCodes());
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
				{
					var bizO = new JobDeclarationFilterBusinessObject();
					var moduleFilter = new JobDeclarationFilterLookups(bizO);
					AssertContainsExactElementsInAnyOrder("Spain", expected, moduleFilter.MessageStatusList().GetAllCodes());
				}
			});
		}

		public void TestContainerModeList()
		{
			var bizO = new JobDeclarationFilterBusinessObject();
			var moduleFilter = new JobDeclarationFilterLookups(bizO);
			AssertEquals("LCL, FCL, LSE, ULD, BBK, BLK, LQD, ROR, LTL, FTL, OBC, UNA, CNT, NCT", moduleFilter.ContainerModeList.CodesAsString);
		}

		public void TestMessageTypeList()
		{
			var bizO = new JobDeclarationFilterBusinessObject();
			var moduleFilter = new JobDeclarationFilterLookups(bizO);
			AssertEquals("EXP, IMP, MSC", moduleFilter.MessageTypeList.CodesAsString);
		}

		public void TestPackTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("UNPKG", "Packagings", "UNE");
			helper.CreateCusCodeList("UNE", "UNPKG", "ABC", "abc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("UNE", "UNPKG", "DEF", "def", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var bizO = new JobDeclarationFilterBusinessObject();
			var moduleFilter = new JobDeclarationFilterLookups(bizO);
			AssertEquals("ABC, DEF", moduleFilter.PackTypeList.CodesAsString);
		}

		public void TestIATALoadPorts()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "Belgium - with EU as Parent, no local list", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France - with EU as Parent, with local list", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Malta, "Malta - without EU as Parent");

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Airline Codes and Percentages for EU AIR freight calculation");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "BEIP1", "BE IATAPort 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "BEIP2", "BE IATAPort 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "FRIP1", "FR IATAPort 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "FRIP2", "FR IATAPort 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "GBIP1", "GB IATAPort 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "GBIP2", "GB IATAPort 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, codeType, "FRIP1", "FR IATAPort 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, codeType, "FRIP2", "FR IATAPort 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, codeType, "FRIP3", "FR IATAPort 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Belgium))
				{
					var declaration = factory.New<BaseJobDeclaration>();
					AssertType<ZZRefCusCodeListCombinedCollection>("BE should load EU list", declaration.Lookups.IATALoadPorts);
					factory = new BusinessObjectFactory();
					var query = declaration.Lookups.IATALoadPorts.CompleteFilter;
					var data = factory.Load<ZZRefCusCodeListCombined>(query);
					AssertEquals(6, data.Length);
					Assert(data.Any(x => x.ZZD_CountryOrGrouping == "EUN"));
					Assert(!data.Any(x => x.ZZD_CountryOrGrouping == "FR"));
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
				{
					var declaration = factory.New<BaseJobDeclaration>();
					AssertType<ZZRefCusCodeListCombinedCollection>("FR should load FR List", declaration.Lookups.IATALoadPorts);
					factory = new BusinessObjectFactory();
					var query = declaration.Lookups.IATALoadPorts.CompleteFilter;
					var data = factory.Load<ZZRefCusCodeListCombined>(query);
					AssertEquals(3, data.Length);
					Assert(!data.Any(x => x.ZZD_CountryOrGrouping == "EUN"));
					Assert(data.Any(x => x.ZZD_CountryOrGrouping == "FR"));
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malta))
				{
					var declaration = factory.New<BaseJobDeclaration>();
					AssertType<AirportCollection>("MT should load UNLOCO list", declaration.Lookups.IATALoadPorts);
				}
			});
		}

		public void TestSupportingDocumentsType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs1 = new Dictionary<string, string[]>();
			attributeNameValuePairs1.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs1.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType }, "9001", "9001 DES", attributeNameValuePairs1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var attributeNameValuePairs2 = new Dictionary<string, string[]>();
			attributeNameValuePairs2.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs2.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { exportCodeType }, "9002", "9002 DES", attributeNameValuePairs2, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var attributeNameValuePairs3 = new Dictionary<string, string[]>();
			attributeNameValuePairs3.Add("Level", new string[] { "HEADER" });
			attributeNameValuePairs3.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.Italy,
				new string[] { importCodeType }, "9003", "9003 DES", attributeNameValuePairs3, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var attributeNameValuePairs4 = new Dictionary<string, string[]>();
			attributeNameValuePairs4.Add("Level", new string[] { "HEADER" });
			attributeNameValuePairs4.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.Germany,
				new string[] { exportCodeType }, "9004", "9004 DES", attributeNameValuePairs4, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var attributeNameValuePairs5 = new Dictionary<string, string[]>();
			attributeNameValuePairs5.Add("Level", new string[] { "HEADER" });
			attributeNameValuePairs5.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.Belgium,
				new string[] { importCodeType }, "9005", "9005 DES", attributeNameValuePairs5, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var attributeNameValuePairs6 = new Dictionary<string, string[]>();
			attributeNameValuePairs6.Add("Level", new string[] { "HEADER" });
			attributeNameValuePairs6.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.Spain,
				new string[] { exportCodeType }, "9006", "9006 DES", attributeNameValuePairs6, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
				{
					var bizO = new JobDeclarationFilterBusinessObject();
					var moduleFilter = new JobDeclarationFilterLookups(bizO);
					var collection = moduleFilter.SupportingDocumentsType;
					collection.Load();
					AssertContainsExactElementsInAnyOrder("IT List", new ZString[] { "9001", "9002", "9003" }, collection.Select(x => x.ZZD_Code).ToArray());
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
				{
					var bizO = new JobDeclarationFilterBusinessObject();
					var moduleFilter = new JobDeclarationFilterLookups(bizO);
					var collection = moduleFilter.SupportingDocumentsType;
					collection.Load();
					AssertContainsExactElementsInAnyOrder("IT List", new ZString[] { "9001", "9002", "9004" }, collection.Select(x => x.ZZD_Code).ToArray());
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Belgium))
				{
					var bizO = new JobDeclarationFilterBusinessObject();
					var moduleFilter = new JobDeclarationFilterLookups(bizO);
					var collection = moduleFilter.SupportingDocumentsType;
					collection.Load();
					AssertContainsExactElementsInAnyOrder("IT List", new ZString[] { "9001", "9002", "9005" }, collection.Select(x => x.ZZD_Code).ToArray());
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
				{
					var bizO = new JobDeclarationFilterBusinessObject();
					var moduleFilter = new JobDeclarationFilterLookups(bizO);
					var collection = moduleFilter.SupportingDocumentsType;
					collection.Load();
					AssertContainsExactElementsInAnyOrder("IT List", new ZString[] { "9001", "9002", "9006" }, collection.Select(x => x.ZZD_Code).ToArray());
				}
			});
		}

		public void TestCustomsOfficePurposeList()
		{
			var bizO = new JobDeclarationFilterBusinessObject();
			var moduleFilter = new JobDeclarationFilterLookups(bizO);
			AssertEquals("List is empty for EU", 0, moduleFilter.CustomsOfficePurposeList.Count);
		}

		public void TestExitPresentationStatusesList()
		{
			var bizO = new JobDeclarationFilterBusinessObject();
			var moduleFilter = new JobDeclarationFilterLookups(bizO);
			AssertEquals("EXR, COX, MLT, EXJ", moduleFilter.ExitPresentationStatuses.CodesAsString);
		}

		public void TestExportExitStatusList()
		{
			var bizO = new JobDeclarationFilterBusinessObject();
			var moduleFilter = new JobDeclarationFilterLookups(bizO);
			AssertNotNull("Export Exit Status List", moduleFilter.ExportExitStatusList);
			AssertEquals("CodesAsString", "EXT, ERR, REM, PRD, EXR, COX, REJ", moduleFilter.ExportExitStatusList.CodesAsString);
		}

		public void TestRequestedProcedureList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC2", "22", "222", "CPC2 Desc", "EXP", group: "H2,H7");
			helper.CreateRefCusProcedure(currentCountry, "B", "CPC3", "33", "333", "CPC3 Desc", "XXX", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC2", "CPC2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC3", "CPC3 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var parent = new JobDeclarationFilterBusinessObject();
			var lookups = parent.Lookups;
			var list = lookups.RequestedProcedureList;
			AssertEquals("CodesAsString", "CPC1, CPC2", list.CodesAsString);
			AssertSame(list, lookups.RequestedProcedureList);
		}

		public void TestPreviousProcedureCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "01", "P1", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "02", "P2", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "04", "P3", "333", "CPC3 Desc", "XXX", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P1", "Pre1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P2", "Pre2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P2", "Pre2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P3", "Pre3 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var parent = new JobDeclarationFilterBusinessObject();
			var lookups = parent.Lookups;
			var list = lookups.PreviousProcedureCodeList;
			AssertEquals("CodesAsString", "P1, P2", list.CodesAsString);
			AssertSame(list, lookups.PreviousProcedureCodeList);
		}

		public void TestAdditionalProcedureCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "01", "P1", "C01", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "01", "P2", "C02", "CPC1 Desc", "EXP", group: "H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "04", "P4", "C03", "CPC3 Desc", "XXX", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C01", "C01 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C02", "C02 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C03", "C03 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var parent = new JobDeclarationFilterBusinessObject();
			var lookups = parent.Lookups;
			var list = lookups.AdditionalProcedureCodeList;
			AssertEquals("CodesAsString", "C01, C02", list.CodesAsString);
			AssertSame(list, lookups.AdditionalProcedureCodeList);
		}

		public void TestGetDataGroupingCodesForSupportingDocumentList()
		{
			var lookups = new JobDeclarationFilterLookups(null);
			var result = (ZString[])lookups.GetType().GetProperty("GetDataGroupingCodesForSupportingDocumentList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(lookups);
			CombineAssertions(() =>
			{
				AssertEquals("Length", 1, result.Length);
				AssertContainsExactElementsInAnyOrder("Values", new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode }, result);
			});
		}
	}
}
