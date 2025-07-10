using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.Common.CusEntryNumberTypes;

namespace Enterprise.Customs.Common.Testing
{
	class CusEntryNumberTypesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCountrySpecificDefaultEntryNumberTypeReturnsBlankOrCodeFromList()
		{
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.Australia);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.Brazil);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.Singapore);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.HongKong);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.Iceland);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.UnitedStates);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.PuertoRico);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.VirginIslands);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.Guam);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.NorthernMarianaIslands);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.AmericanSamoa);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.NewZealand);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.Nigeria);
			AssertEntryNumberListIncludesDefaultValueForCountry(Constants.CountryCodes.UnitedKingdom);  // this test is flawed - AssertEntryNumberListIncludesDefaultValueForCountry() can easily make no assertions, like for UK
		}

		[ExpectNoExceptions]
		void AssertEntryNumberListIncludesDefaultValueForCountry(string country)
		{
			ZString defaultValueIsImport = CountrySpecificDefaultEntryNumberType(country, true);

			if (!defaultValueIsImport.IsEmpty)
			{
				NUnit.Framework.Assert.That(CountrySpecificCustomsEntryNumberTypeList(Factory, country, true).ContainsCode(defaultValueIsImport), Is.EqualTo(true), "default code should be in the list for all countries");
			}

			ZString defaultValueIsExport = CountrySpecificDefaultEntryNumberType(country, false);

			if (!defaultValueIsExport.IsEmpty)
			{
				NUnit.Framework.Assert.That(CountrySpecificCustomsEntryNumberTypeList(Factory, country, false).ContainsCode(defaultValueIsExport), Is.EqualTo(true), "default code should be in the list for all countries");
			}
		}

		[ExpectNoExceptions]
		public void TestExemptionCodes()
		{
			NUnit.Framework.Assert.That(IsExemptionCode("EXP"), Is.False);
			NUnit.Framework.Assert.That(IsExemptionCode(CMRExportExemptionCodes.EXDC.Code), Is.True);
			NUnit.Framework.Assert.That(IsExemptionCode(CMRExportExemptionCodes.EXDD.Code), Is.True);
			NUnit.Framework.Assert.That(IsExemptionCode(CMRExportExemptionCodes.EXLV.Code), Is.True);
			NUnit.Framework.Assert.That(IsExemptionCode(CMRExportExemptionCodes.EXML.Code), Is.True);
			NUnit.Framework.Assert.That(IsExemptionCode(CMRExportExemptionCodes.EXPE.Code), Is.True);
			NUnit.Framework.Assert.That(IsExemptionCode(CMRExportExemptionCodes.EXSP.Code), Is.True);
			NUnit.Framework.Assert.That(IsExemptionCode(CMRExportExemptionCodes.EXTI.Code), Is.True);
		}

		[ExpectNoExceptions]
		public void TestCountrySpecificCustomsEntryNumberTypeList()
		{
			var singaporeList = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Singapore, false);
			AssertSingaporeCustomsEntryListContents(singaporeList);

			var australianCMRList = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Australia, false);
			NUnit.Framework.Assert.That(australianCMRList.Count, Is.EqualTo(9));
			var australianImportsList = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Australia, true);
			NUnit.Framework.Assert.That(australianImportsList.Count, Is.EqualTo(2));

			var auCompany = (BusinessObject)Factory.New<IGlbCompany>();
			auCompany[GlbCompanySchema.GC_Code.Name] = "AUC";
			auCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "AU";
			var auBranch = (BusinessObject)Factory.New<IGlbBranch>();
			auBranch[GlbBranchSchema.GB_Code.Name] = "AUC";
			auBranch[GlbBranchSchema.GB_GC.Name] = auCompany.PK;

			var brazilCompany = (BusinessObject)Factory.New<IGlbCompany>();
			brazilCompany[GlbCompanySchema.GC_Code.Name] = "BRC";
			brazilCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "BR";
			var brazilBranch = (BusinessObject)Factory.New<IGlbBranch>();
			brazilBranch[GlbBranchSchema.GB_Code.Name] = "BRC";
			brazilBranch[GlbBranchSchema.GB_GC.Name] = brazilCompany.PK;
			Factory.Save();

			CodeDescriptionPairList brazilianExportsList;
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, auBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				brazilianExportsList = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Brazil, false);
				NUnit.Framework.Assert.That(brazilianExportsList.Count, Is.EqualTo(2));
			}
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, brazilBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				Factory.ClearCachedValue<CodeDescriptionPairList>("CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList|" + Constants.CountryCodes.Brazil + "|" + false);
				brazilianExportsList = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Brazil, false);
				NUnit.Framework.Assert.That(brazilianExportsList.Count, Is.EqualTo(2));
				NUnit.Framework.Assert.That(brazilianExportsList.ContainsCode(Brazil.DUE), Is.True);
				NUnit.Framework.Assert.That(brazilianExportsList.ContainsCode(Brazil.EAK), Is.True);
			}

			var brazilianImportsList = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Brazil, true);
			NUnit.Framework.Assert.That(brazilianImportsList.Count, Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestIsUSCustomsCountryNeededEXPEntryType()
		{
			var result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.UnitedStates, true);
			NUnit.Framework.Assert.That(result, Is.EqualTo(UnitedStates.CRN).Using(CustomComparers.TypeComparison));
			result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.UnitedStates, false);
			NUnit.Framework.Assert.That(result, Is.EqualTo(CusEntryNumberTypeList.Codes.ITN).Using(CustomComparers.TypeComparison));

			var entryTypeList = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.UnitedStates, true);
			NUnit.Framework.Assert.That(entryTypeList.CodesAsString, Is.EqualTo("CRN, ENS, INB"));
			entryTypeList = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.UnitedStates, false);
			NUnit.Framework.Assert.That(entryTypeList.CodesAsString, Is.EqualTo("ITN, NVC"));

			NUnit.Framework.Assert.That(IsUSCustomsCountryNeededEXPEntryType(Constants.CountryCodes.VirginIslands), Is.EqualTo(true));
			NUnit.Framework.Assert.That(IsUSCustomsCountryNeededEXPEntryType(Constants.CountryCodes.PuertoRico), Is.EqualTo(true));
			NUnit.Framework.Assert.That(IsUSCustomsCountryNeededEXPEntryType(Constants.CountryCodes.UnitedStates), Is.EqualTo(true));
			NUnit.Framework.Assert.That(IsUSCustomsCountryNeededEXPEntryType(Constants.CountryCodes.Guam), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestCountrySpecificDefaultEntryNumberType()
		{
			var result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.Australia, false);
			NUnit.Framework.Assert.That(result, Is.EqualTo(CANType.CustomsAuthorityNumber.Code).Using(CustomComparers.TypeComparison));
			result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.Australia, true);
			NUnit.Framework.Assert.That(result, Is.EqualTo(ZString.Empty));

			result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.Brazil, true);
			NUnit.Framework.Assert.That(result, Is.EqualTo(ZString.Empty));
			result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.Brazil, false);
			NUnit.Framework.Assert.That(result, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestHongKongEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.HongKong, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(1), "There is only one type for export");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(HongKong.ExportLicense));
			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.HongKong, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(1), "There is only one type for import");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(HongKong.ImportLicense));
		}

		[ExpectNoExceptions]
		public void TestHongKongDefaultEntryNumberType()
		{
			ZString result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.HongKong, false);
			NUnit.Framework.Assert.That(result, Is.EqualTo(HongKong.ExportLicense).Using(CustomComparers.TypeComparison));
			result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.HongKong, true);
			NUnit.Framework.Assert.That(result, Is.EqualTo(HongKong.ImportLicense).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUnitedKingdomDefaultEntryNumberType()
		{
			ZString result = CountrySpecificDefaultEntryNumberType(Constants.CountryCodes.UnitedKingdom, false);
			NUnit.Framework.Assert.That(result, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		void AssertSingaporeCustomsEntryListContents(CodeDescriptionPairList entryList)
		{
			NUnit.Framework.Assert.That(entryList.Count, Is.EqualTo(17));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.Permit), Is.EqualTo("Permit"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.Certificate), Is.EqualTo("Certificate"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.TS), Is.EqualTo("Transhipment (includes re-documentation cargo)"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.UA), Is.EqualTo("Unaccompanied, non-controlled goods with total value not exceeding $400"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.PP), Is.EqualTo("Not prohibited under regulation 6 of the Imports and Exports Regulations 1995"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.DP), Is.EqualTo("Diplomatic correspondence"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.MD), Is.EqualTo("By joint defense force, excluding civilian motor vehicles"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.MF), Is.EqualTo("By the MFA, excluding motor vehicles"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.CA), Is.EqualTo("Used motor vehicles covered by Carnet de Passage endorsed by the Automobile Association of Singapore"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.AT), Is.EqualTo("Goods covered with an ATA Carnet"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.SP), Is.EqualTo("Bona fide trade samples not exceeding $400"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.CD), Is.EqualTo("Commercial, shipping or airline documents"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.PM), Is.EqualTo("Press photographs or negatives, news write-ups, news clippings, news films or news transcription tapes"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.HC), Is.EqualTo("Human corpses, human remains, human bones or cremated ashes"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.HT), Is.EqualTo("Human transplant materials"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.PT), Is.EqualTo("Pets"));
			NUnit.Framework.Assert.That(entryList.GetDescriptionFromCode(SG.CustomsEntryTypeList.Singapore.SGExemption.Codes.ZZ), Is.EqualTo("Others"));
		}

		[ExpectNoExceptions]
		public void TestUnitedStatesEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.UnitedStates, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(2), "There are two types for export");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.ITN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.NVOCC));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.UnitedStates, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(3), "There are three types for import");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(UnitedStates.CRN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(UnitedStates.EntrySummary));
			NUnit.Framework.Assert.That(result[2].Code, Is.EqualTo(UnitedStates.InBond));
		}

		[ExpectNoExceptions]
		public void TestPuertoRicoEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.PuertoRico, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(2), "There are two types for export");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.ITN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.NVOCC));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.PuertoRico, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(3), "There are three types for import");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(UnitedStates.CRN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(UnitedStates.EntrySummary));
			NUnit.Framework.Assert.That(result[2].Code, Is.EqualTo(UnitedStates.InBond));
		}

		[ExpectNoExceptions]
		public void TestVirginIslandsEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.VirginIslands, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(2), "There are two types for export");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.ITN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.NVOCC));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.VirginIslands, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(3), "There are three types for import");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(UnitedStates.CRN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(UnitedStates.EntrySummary));
			NUnit.Framework.Assert.That(result[2].Code, Is.EqualTo(UnitedStates.InBond));
		}

		[ExpectNoExceptions]
		public void TestGuamEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Guam, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(2), "There are two types for export");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.ITN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.NVOCC));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Guam, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(3), "There are three types for import");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(UnitedStates.CRN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(UnitedStates.EntrySummary));
			NUnit.Framework.Assert.That(result[2].Code, Is.EqualTo(UnitedStates.InBond));
		}

		[ExpectNoExceptions]
		public void TestNorthenMarianaIslandsEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.NorthernMarianaIslands, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(2), "There are two types for export");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.ITN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.NVOCC));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.NorthernMarianaIslands, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(3), "There are three types for import");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(UnitedStates.CRN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(UnitedStates.EntrySummary));
			NUnit.Framework.Assert.That(result[2].Code, Is.EqualTo(UnitedStates.InBond));
		}

		[ExpectNoExceptions]
		public void TestAmericanSamoaEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.AmericanSamoa, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(2), "There are two types for export");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.ITN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(CusEntryNumberTypeList.Codes.NVOCC));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.AmericanSamoa, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(3), "There are three types for import");
			NUnit.Framework.Assert.That(result[0].Code, Is.EqualTo(UnitedStates.CRN));
			NUnit.Framework.Assert.That(result[1].Code, Is.EqualTo(UnitedStates.EntrySummary));
			NUnit.Framework.Assert.That(result[2].Code, Is.EqualTo(UnitedStates.InBond));
		}

		[ExpectNoExceptions]
		public void TestEuropeanUnionEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Germany, false);
			NUnit.Framework.Assert.That(result.Count > CusEntryNumberTypes.EU.EUCustomsEntryTypeList.Count, Is.EqualTo(true), "There are multiple types for export");
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain(Standard.ClearancePermitNumber));
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain(FreightDataRegistry.Instance.CustomsPermitClearanceNumbers.Value.CodesAsString));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Germany, true);
			NUnit.Framework.Assert.That(result.Count > CusEntryNumberTypes.EU.EUCustomsEntryTypeList.Count, Is.EqualTo(true), "There are multiple types for import");
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain(Standard.ClearancePermitNumber));
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain(FreightDataRegistry.Instance.CustomsPermitClearanceNumbers.Value.CodesAsString));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.France, true);
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain(Standard.MovementReferenceNumber));
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain(CusEntryNumberTypes.EU.T1));
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain(CusEntryNumberTypes.EU.MasterUCR));
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain("UCR"));
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain("MUC"));
			NUnit.Framework.Assert.That(result.CodesAsString, Does.Contain("IMP"));
			NUnit.Framework.Assert.That(result.GetDescriptionFromCode(Standard.MovementReferenceNumber), Is.EqualTo("Movement Reference Number"));

			NUnit.Framework.Assert.That(CusEntryNumberTypes.EU.CommunityTransitStatusCodesList_Export.CodesAsString, Does.Contain("X"));
			NUnit.Framework.Assert.That(CusEntryNumberTypes.EU.CommunityTransitStatusCodesList_Import.CodesAsString, Does.Contain("T2LSM"));
			NUnit.Framework.Assert.That(CusEntryNumberTypes.EU.CommunityTransitStatusCodesList_Import.CodesAsString, Does.Not.Contain("X"));
			NUnit.Framework.Assert.That(CusEntryNumberTypes.EU.CommunityTransitStatusCodesList_Export.CodesAsString, Does.Contain("T2LSM"));
			NUnit.Framework.Assert.That(CusEntryNumberTypes.EU.CommunityTransitStatusCodesList.CodesAsString, Does.Contain("X"));
			NUnit.Framework.Assert.That(CusEntryNumberTypes.EU.CommunityTransitStatusCodesList.CodesAsString, Does.Contain("T2LSM"));
		}

		[ExpectNoExceptions]
		public void TestSpainEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Spain, false);

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(Spain.ClearanceCSV, Does.Contain("CLR"));
				NUnit.Framework.Assert.That(Spain.T2CMovementReferenceNumber, Does.Contain("TMR"));
				NUnit.Framework.Assert.That(Spain.CircuitCan, Does.Contain("CIS"));
			});
		}

		[ExpectNoExceptions]
		public void TestFranceEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.France, false);
			foreach (CodeDescriptionPair pair in CusEntryNumberTypes.EU.EUCustomsEntryTypeList)
			{
				NUnit.Framework.Assert.That(result.ContainsCode(pair.Code), Is.EqualTo(true));
			}
		}

		[ExpectNoExceptions]
		public void TestNotClearedByAgentNumberType()
		{
			ZString result = NotClearedByAgentNumberType(Constants.CountryCodes.Australia);
			NUnit.Framework.Assert.That(result, Is.EqualTo("").Using(CustomComparers.TypeComparison));
			result = NotClearedByAgentNumberType(Constants.CountryCodes.Germany);
			NUnit.Framework.Assert.That(result, Is.EqualTo(CusEntryNumberTypes.EU.T1).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSouthAfricaEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.SouthAfrica, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(4), "There are four types for export");
			NUnit.Framework.Assert.That(result[3].Code, Is.EqualTo(Standard.MovementReferenceNumber));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.SouthAfrica, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(4), "There are four types for import");
			NUnit.Framework.Assert.That(result[3].Code, Is.EqualTo(Standard.MovementReferenceNumber));
		}

		[ExpectNoExceptions]
		public void TestSwitzerlandEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Switzerland, false);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(4), "There are four types for export");
			NUnit.Framework.Assert.That(result[3].Code, Is.EqualTo(Switzerland.GoodsDeclarationReferenceNumber));

			result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Switzerland, true);
			NUnit.Framework.Assert.That(result.Count, Is.EqualTo(4), "There are four types for import");
			NUnit.Framework.Assert.That(result[3].Code, Is.EqualTo(Switzerland.GoodsDeclarationReferenceNumber));
		}

		[ExpectNoExceptions]
		public void TestGermanyEntryNumberTypes()
		{
			CodeDescriptionPairList result = CountrySpecificCustomsEntryNumberTypeList(Factory, Constants.CountryCodes.Germany, false);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(Standard.LocalReferenceNumber, Does.Contain("LRN"));
			});
		}

		[ExpectNoExceptions]
		public void TestCountrySpecificCustomsEntryNumberTypeListIsCached()
		{
			var countries = new[]
			{
				Constants.CountryCodes.Australia,
				Constants.CountryCodes.Brazil,
				Constants.CountryCodes.Singapore,
				Constants.CountryCodes.HongKong,
				Constants.CountryCodes.NewZealand,
				Constants.CountryCodes.Iceland,
				Constants.CountryCodes.UnitedStates,
				Constants.CountryCodes.PuertoRico,
				Constants.CountryCodes.VirginIslands,
				Constants.CountryCodes.Guam,
				Constants.CountryCodes.NorthernMarianaIslands,
				Constants.CountryCodes.AmericanSamoa,
				Constants.CountryCodes.SouthAfrica,
				Constants.CountryCodes.Germany
			};

			var isImportSettings = new[]
			{
				true,
				false
			};

			var entryTypeListArgs = countries.SelectMany(country => isImportSettings,
				(country, isImport) => new
				{
					Country = country,
					IsImport = isImport
				});

			foreach (var entryTypeListArg in entryTypeListArgs)
			{
				var numberTypeList1 = CountrySpecificCustomsEntryNumberTypeList(Factory, entryTypeListArg.Country, entryTypeListArg.IsImport);
				var numberTypeList2 = CountrySpecificCustomsEntryNumberTypeList(Factory, entryTypeListArg.Country, entryTypeListArg.IsImport);

				NUnit.Framework.Assert.That(numberTypeList2, Is.EqualTo(numberTypeList1), string.Format("list for country:{0} isImport:{1} should be cached", entryTypeListArg.Country, entryTypeListArg.IsImport));
			}
		}
	}
}
