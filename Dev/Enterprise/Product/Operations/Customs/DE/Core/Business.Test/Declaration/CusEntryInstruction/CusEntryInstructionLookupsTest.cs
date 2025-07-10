using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMainAccountingOrganisations()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			var collection = instruction.Lookups.MainAccountingOrganisations;
			CombineAssertions(() =>
			{
				AssertType<OrgHeaderCollection>("Type", collection);
				AssertSame("Cached", collection, instruction.Lookups.MainAccountingOrganisations);
			});
		}

		public void TestSimplifiedGrantAuthorizationList()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			CombineAssertions(() =>
			{
				AssertEquals("List values", "J, N", instruction.Lookups.SimplifiedGrantAuthorizationList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<SimplifiedGrantAuthorizationList>(), instruction.Lookups.SimplifiedGrantAuthorizationList);
			});
		}

		public void TestCriteriaTypeList()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			CombineAssertions(() =>
			{
				AssertEquals("List Codes", "0, 1", instruction.Lookups.CriteriaTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<CriteriaTypeList>(), instruction.Lookups.CriteriaTypeList);
			});
		}

		public void TestVariantList_Import()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var validImportStyleSubStyleCombinations = new Dictionary<string, string[]>
			{
				{ ImportDeclarationTypeList.Codes.AAV, new[] { EntrySubStyleList.Codes.SimplifiedDeclaration } },
				{ ImportDeclarationTypeList.Codes.AVABR, Array.Empty<string>() },
				{ ImportDeclarationTypeList.Codes.AZ,  new[] { EntrySubStyleList.Codes.SimplifiedDeclaration  } },
				{ ImportDeclarationTypeList.Codes.AZL, new[] { EntrySubStyleList.Codes.SimplifiedDeclaration } },
				{ ImportDeclarationTypeList.Codes.BA,  new[] { EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic } },
				{ ImportDeclarationTypeList.Codes.EAV, new[] { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA } },
				{ ImportDeclarationTypeList.Codes.EGN, new[] { EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic } },
				{ ImportDeclarationTypeList.Codes.EGZ, new[] { EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic } },
				{ ImportDeclarationTypeList.Codes.EZA, new[] { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.IncompleteDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB } },//
				{ ImportDeclarationTypeList.Codes.EZL, new[] { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA } },
				{ ImportDeclarationTypeList.Codes.LUZ, Array.Empty<string>() },
				{ ImportDeclarationTypeList.Codes.VAV, new[] { EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
				{ ImportDeclarationTypeList.Codes.VZA, new[] { EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
				{ ImportDeclarationTypeList.Codes.VZL, new[] { EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC } },
		};

			foreach (var importStyleSubStyleArrayKeyValuePair in validImportStyleSubStyleCombinations)
			{
				instruction.CEI_Style = importStyleSubStyleArrayKeyValuePair.Key;
				AssertContainsExactElementsInAnyOrder(importStyleSubStyleArrayKeyValuePair.Value, lookups.VariantList.GetAllCodes());
			}
		}

		public void TestCEI_SubStyle_ReadOnly()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;

			AssertEquals(true, instruction.CEI_SubStyle_ReadOnly);
		}

		public void TestDeclarationTypeList_IMP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "20", "20", "200", "test2", "IMP", group: "AAV,AZ,AZL,BA,EAV,EGN,EGZ,EZA,EZL,L\u00dcZ,VAV,VZA,VZL");
			Factory.Save();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				var list = lookups.DeclarationTypeList;
				AssertEquals("CodesAsString", "AAV, AZ, AZL, BA, EAV, EGN, EGZ, EZA, EZL, LÜZ, VAV, VZA, VZL", list.CodesAsString);
				AssertSame("Cached", list, lookups.DeclarationTypeList);
			});
		}

		public void TestDeclarationTypeList_IMP_EnableInwardProcessing()
		{
			using (CustomsDataRegistry.Instance.EnableInwardProcessing.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "20", "20", "200", "test2", "IMP", group: "AAV,AVABR, AZ,AZL,BA,EAV,EGN,EGZ,EZA,EZL,L\u00dcZ,VAV,VZA,VZL");
				Factory.Save();

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				CombineAssertions(() =>
				{
					var list = lookups.DeclarationTypeList;
					AssertEquals("CodesAsString", "AAV, AVABR, AZ, AZL, BA, EAV, EGN, EGZ, EZA, EZL, LÜZ, VAV, VZA, VZL", list.CodesAsString);
					AssertSame("Cached", list, lookups.DeclarationTypeList);
				});
			}
		}

		public void TestDeclarationTypeList_IMP_CO()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "20", "20", "200", "test2", "IMP", group: "AAV,AZ,AZL,BA,EAV,EGN,EGZ,EZA,EZL,L\u00dcZ,VAV,VZA,VZL");
			Factory.Save();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			CombineAssertions(() =>
			{
				var list = lookups.DeclarationTypeList;
				AssertEquals("CodesAsString", "AZ, EZA, VZA", list.CodesAsString);
				AssertSame("Cached", list, lookups.DeclarationTypeList);
			});
		}

		public void TestDeclarationTypeList_EXP()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				AssertTypeProcedureList(ExportDeclarationTypeTimeList.Codes._00, "000100, 000110, 000200, 000210, 000400, 000901, 000902, 001300, 001310, 001410, 110100, 110110, 110200, 110210, 110400, 111300, 111310, 111410, 120100, 120110, 120200, 120210, 200100, 200110, 200200, 200210, 200400, 201300, 201310, 201410");
				AssertTypeProcedureList(ExportDeclarationTypeTimeList.Codes._10, "000000, 000400, 110000, 110400, 200000, 200400");
				AssertTypeProcedureList(ExportDeclarationTypeTimeList.Codes._11, "000000, 110000, 120000, 200000");
				AssertTypeProcedureList(ExportDeclarationTypeTimeList.Codes._12, "000000, 000400, 110000, 110400, 120000, 200000, 200400");
				AssertTypeProcedureList(ExportDeclarationTypeTimeList.Codes._13, "000000");
				AssertTypeProcedureList(ExportDeclarationTypeTimeList.Codes._20, "000000");
				AssertSame("Cached", lookups.DeclarationTypeList, lookups.DeclarationTypeList);
			});

			void AssertTypeProcedureList(string subStyle, string expected)
			{
				instruction.CEI_SubStyle = subStyle;
				AssertEquals($"CEI_SubStyle '{subStyle}'", expected, lookups.DeclarationTypeList.CodesAsString);
			}
		}

		public void TestDeclarationTypeList_WAD()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			var list = lookups.DeclarationTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("List", WarehouseAdjustmentDeclarationTypeList.Codes.SER, list.CodesAsString);
				AssertSame("Cached", list, lookups.DeclarationTypeList);
			});
		}

		public void TestEntrySubStyleList_AVABR()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			var list = lookups.EntrySubStyleList;
			CombineAssertions(() =>
			{
				AssertEquals("List", ZString.Empty, list.CodesAsString);
				AssertSame("Cached", list, lookups.EntrySubStyleList);
			});
		}

		public void TestEarlyClearanceFlags()
		{
			var list = lookups.EarlyClearanceFlags;
			CombineAssertions(() =>
			{
				AssertEquals("List", "J, N", list.CodesAsString);
				AssertSame("Cached", list, lookups.EarlyClearanceFlags);
			});
		}

		public void TestVariantList_Export()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertEquals("List", "00, 10, 11, 12, 13, 20", lookups.VariantList.CodesAsString);
		}

		public void TestVariantList_MiscellaneousCustoms()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("List", string.Empty, lookups.VariantList.CodesAsString);
		}

		public void TestVariantListCached()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertSame(lookups.VariantList, lookups.VariantList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertSame(lookups.VariantList, lookups.VariantList);
		}

		public void TestCPCList_CEI_Style_Empty()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = string.Empty;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, new ImportMainProcedureCodeList().CodesAsString);
		}

		public void TestCPCList_CEI_Style_AAV()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "51");
		}

		public void TestCPCList_CEI_Style_AZ()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "01, 40, 42, 45, 49, 61, 63");
		}

		public void TestCPCList_CEI_Style_AZL()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "71");
		}

		public void TestCPCList_CEI_Style_BA()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.BA;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "");
		}

		public void TestCPCList_CEI_Style_EAV()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "51");
		}

		public void TestCPCList_CEI_Style_EGN()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EGN;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "");
		}

		public void TestCPCList_CEI_Style_EGZ()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EGZ;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "");
		}

		public void TestCPCList_CEI_Style_EZA()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "01, 40, 42, 45, 49, 61, 63");
		}

		public void TestCPCList_CEI_Style_EZL()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "71");
		}

		public void TestCPCList_CEI_Style_LUZ()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "71");
		}

		public void TestCPCList_CEI_Style_VAV()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "51");
		}

		public void TestCPCList_CEI_Style_VZA()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "01, 40, 42, 45, 49, 61, 63");
		}

		public void TestCPCList_CEI_Style_VZL()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "71");
		}

		public void TestCPCList_WhenCEI_StyleIsUnknown_ShouldUseRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.Germany, "", "71", "00", "", "DES1", "IMP", group: "UNK1");
			helper.CreateRefCusProcedure(CountryCodes.Germany, "", "71", "78", "", "DES2", "IMP", group: "UNK1");
			helper.CreateRefCusProcedure(CountryCodes.Germany, "", "42", "00", "C22", "DES3", "IMP", group: "UNK2");
			helper.CreateRefCusProcedure(CountryCodes.Germany, "", "49", "71", "C18", "DES4", "IMP", group: "UNK2");
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			instruction.CEI_Style = "UNK2";
			AssertListHasCorrectValuesAndIsCached(x => x.CPCList, "42, 49");
		}

		void AssertListHasCorrectValuesAndIsCached(Func<CusEntryInstructionLookups, CodeDescriptionPairList> getList, string expectedValues)
		{
			CombineAssertions(() =>
			{
				var list = getList.Invoke(lookups);
				AssertEquals("Values", expectedValues, list.CodesAsString);
				AssertSame("Cached", list, getList.Invoke(lookups));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			lookups = instruction.Lookups;
		}
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryInstructionLookups lookups;
	}
}
