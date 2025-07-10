using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.JP.Business.DeclarationCargoTypeList;
using static Enterprise.Customs.JP.Business.JPExportDeclarationTypeList;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionLookups))]
	sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestValueTypeList()
		{
			AssertType<ValueTypeList>(CusEntryInstruction.Lookups.ValueTypeList);
		}

		public void TestTradeTypeFirstChar_List()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertTradeTypeCharExist(true, TradeTypeFirstChar.Codes.A);
				AssertTradeTypeCharExist(false, TradeTypeFirstChar.Codes.E);

				Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				AssertTradeTypeCharExist(false, TradeTypeFirstChar.Codes.A);
				AssertTradeTypeCharExist(true, TradeTypeFirstChar.Codes.E);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertTradeTypeCharExist(true, TradeTypeFirstChar.Codes.C);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertTradeTypeCharExist(false, TradeTypeFirstChar.Codes.C);

				CusEntryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.E;
				AssertTradeTypeCharExist(true, TradeTypeFirstChar.Codes.B);
				CusEntryInstruction.CEI_DeclarationCargoType = "";
				AssertTradeTypeCharExist(true, TradeTypeFirstChar.Codes.B);
				CusEntryInstruction.CEI_DeclarationCargoType = "!";
				AssertTradeTypeCharExist(false, TradeTypeFirstChar.Codes.B);

				AssertTradeTypeCharExist(true, TradeTypeFirstChar.Codes.D);
			});

			void AssertTradeTypeCharExist(bool shouldExist, string tradeTypeChar)
			{
				if (shouldExist)
				{
					Assert($"{tradeTypeChar} should be exist.", Lookups.TradeTypeFirstCharList.ContainsCode(tradeTypeChar));
				}
				else
				{
					Assert($"{tradeTypeChar} should not be exist.", !Lookups.TradeTypeFirstCharList.ContainsCode(tradeTypeChar));
				}
			}
		}

		public void TestTradeTypeSecondChar_List()
		{
			var charList = Lookups.TradeTypeSecondCharList;
			var tradeTypeSecondChars = new TradeTypeSecondChar();

			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.A,
					new JPExportDeclarationTypeList().GetAllCodes().Where(x => !x.Equals(JPExportDeclarationTypeList.Codes.R)).ToArray());
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.B, JPExportDeclarationTypeList.Codes.R);
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.C, JPExportDeclarationTypeList.Codes.R);
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.D, JPExportDeclarationTypeList.Codes.R);
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.I, JPExportDeclarationTypeList.Codes.R);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.A,
											JPImportDeclarationTypeList.Codes.C,
											JPImportDeclarationTypeList.Codes.F,
											JPImportDeclarationTypeList.Codes.H,
											JPImportDeclarationTypeList.Codes.N,
											JPImportDeclarationTypeList.Codes.J,
											JPImportDeclarationTypeList.Codes.P);
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.B, JPImportDeclarationTypeList.Codes.S);
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.C, JPImportDeclarationTypeList.Codes.M);

				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.E,
											JPImportDeclarationTypeList.Codes.K,
											JPImportDeclarationTypeList.Codes.D,
											JPImportDeclarationTypeList.Codes.R);

				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.F,
											JPImportDeclarationTypeList.Codes.U,
											JPImportDeclarationTypeList.Codes.L);

				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.H, new JPImportDeclarationTypeList().GetAllCodes().ToArray());

				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.I, JPImportDeclarationTypeList.Codes.A);
				AssertTradeTypeCharExist(TradeTypeSecondChar.Codes.J,
											JPImportDeclarationTypeList.Codes.B,
											JPImportDeclarationTypeList.Codes.E);

				Assert($"{TradeTypeSecondChar.Codes.D} should not be exist.", !Lookups.TradeTypeSecondCharList.ContainsCode(TradeTypeSecondChar.Codes.D));
				Assert($"{TradeTypeSecondChar.Codes.G} should not be exist.", !Lookups.TradeTypeSecondCharList.ContainsCode(TradeTypeSecondChar.Codes.G));

				CusEntryInstruction.CEI_BeforePermitApplicationReason = "1";
				Assert($"{TradeTypeSecondChar.Codes.D} should be exist.", Lookups.TradeTypeSecondCharList.ContainsCode(TradeTypeSecondChar.Codes.D));
				Assert($"{TradeTypeSecondChar.Codes.G} should be exist.", Lookups.TradeTypeSecondCharList.ContainsCode(TradeTypeSecondChar.Codes.G));
			});

			void AssertTradeTypeCharExist(string tradeTypeChar, params string[] declarationTypesAllowExist)
			{
				foreach (var declarationType in declarationTypesAllowExist)
				{
					CusEntryInstruction.CEI_Style = declarationType;
					Assert($"{tradeTypeChar} should be exist.", Lookups.TradeTypeSecondCharList.ContainsCode(tradeTypeChar));
				}

				CodeDescriptionPairList declarationTypeList = Declaration.IsImport ? new JPImportDeclarationTypeList() : new JPExportDeclarationTypeList();
				var declarationTypeNotAllowExist = declarationTypeList.GetAllCodes().FirstOrDefault(x => !declarationTypesAllowExist.Contains(x));
				if (declarationTypeNotAllowExist != null)
				{
					CusEntryInstruction.CEI_Style = declarationTypeNotAllowExist;
					Assert($"{tradeTypeChar} should not be exist.", !Lookups.TradeTypeSecondCharList.ContainsCode(tradeTypeChar));
				}
			}
		}

		public void TestTradeTypeThirdChar_List()
		{
			CombineAssertions(() =>
			{
				AssertTradeTypeCharExist(true, TradeTypeThirdChar.Codes.E);

				CusEntryInstruction.CEI_CommercialValueType = CommercialValueTypeList.Codes.ImportApprovalHasCommercialOrCombination;
				AssertTradeTypeCharExist(true, TradeTypeThirdChar.Codes.E);

				CusEntryInstruction.CEI_CommercialValueType = CommercialValueTypeList.Codes.ImportApprovalNoCommercialValue;
				AssertTradeTypeCharExist(false, TradeTypeThirdChar.Codes.E);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				CusEntryInstruction.CEI_CommercialValueType = CommercialValueTypeList.Codes.ImportApprovalHasCommercialOrCombination;
				AssertTradeTypeCharExist(false, TradeTypeThirdChar.Codes.G);

				CusEntryInstruction.CEI_CommercialValueType = CommercialValueTypeList.Codes.ImportApprovalNoCommercialValue;
				AssertTradeTypeCharExist(true, TradeTypeThirdChar.Codes.G);

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertTradeTypeCharExist(false, TradeTypeThirdChar.Codes.G);

				AssertTradeTypeCharExist(true, TradeTypeThirdChar.Codes.A);
				AssertTradeTypeCharExist(true, TradeTypeThirdChar.Codes.B);
				AssertTradeTypeCharExist(true, TradeTypeThirdChar.Codes.C);
				AssertTradeTypeCharExist(true, TradeTypeThirdChar.Codes.D);
			});

			void AssertTradeTypeCharExist(bool shouldExist, string tradeTypeChar)
			{
				if (shouldExist)
				{
					Assert($"{tradeTypeChar} should be exist.", Lookups.TradeTypeThirdCharList.ContainsCode(tradeTypeChar));
				}
				else
				{
					Assert($"{tradeTypeChar} should not be exist.", !Lookups.TradeTypeThirdCharList.ContainsCode(tradeTypeChar));
				}
			}
		}

		public void TestApprovalCertificateCategoryList()
		{
			var list = Lookups.ApprovalCertificateCategoryList;

			CombineAssertions(() =>
			{
				AssertEquals("list.GetType()", typeof(ApprovalCertificateCategoryCodeList), list.GetType());
				AssertNotNull("List should not be null", list);
				AssertEquals("list.Count", 8, list.Count);
				AssertEquals(list.GetDescriptionFromCode(ApprovalCertificateCategoryCodeList.Codes.N1), "「ＦＥ」該当だが特例扱いで許可証等が不要な場合");
				AssertEquals(list.GetDescriptionFromCode(ApprovalCertificateCategoryCodeList.Codes.E1), "輸出令第２条第１項第１号または第１号の２に該当するもの");
				AssertEquals(list.GetDescriptionFromCode(ApprovalCertificateCategoryCodeList.Codes.N2), "「Ｅ１」該当だが特例扱いで許可証等が不要な場合");
				AssertEquals(list.GetDescriptionFromCode(ApprovalCertificateCategoryCodeList.Codes.E2), "輸出令第２条第１項第２号に該当するもの");
				AssertEquals(list.GetDescriptionFromCode(ApprovalCertificateCategoryCodeList.Codes.N3), "「Ｅ２」該当だが特例扱いで許可証等が不要な場合");
				AssertEquals(list.GetDescriptionFromCode(ApprovalCertificateCategoryCodeList.Codes.FT), "外国為替令第６条、第８条または第１７条第２項に該当するもの");
				AssertEquals(list.GetDescriptionFromCode(ApprovalCertificateCategoryCodeList.Codes.FE), "外国為替及び外国貿易法第４８条第１項に該当するものの（輸出貿易管理令第４条第１項第３号イから二該当の有無 を含む。）");
				AssertEquals(list.GetDescriptionFromCode(ApprovalCertificateCategoryCodeList.Codes.NO), "上記に該当しないもの（輸出承認等不要の場合）（輸出令第４条第１項第３号イから二非該当を含む。）");
			});
		}

		public void TestCommercialValueTypes()
		{
			var list = Lookups.CommercialValueTypes;

			AssertEquals("2 items in total for AttachedOfImportApprovalCertificateList", 2, list.Count);
			AssertSame("Is cached", list, Lookups.CommercialValueTypes);
		}

		public void TestResultsOfContentsInspectionList()
		{
			var list = Lookups.ContentInspectionResultList;
			CombineAssertions(() =>
			{
				Assert("Code A", list.ContainsCode("A"));
				AssertEquals("Description for code A", "No Problem", list.GetDescriptionFromCode("A"));
				Assert("Code B", list.ContainsCode("B"));
				AssertEquals("Description for code B", "Problem", list.GetDescriptionFromCode("B"));
				Assert("Code C", list.ContainsCode("C"));
				AssertEquals("Description for code C", "Confirmation Request", list.GetDescriptionFromCode("C"));

				AssertEquals("3 items in total for ResultsOfContentsInspectionList", 3, list.Count);
				AssertSame("Is cached", list, Lookups.ContentInspectionResultList);
			});
		}

		public void TestIDACertificationIdentificationList()
		{
			var list = Lookups.IDACertificateIdList;
			CombineAssertions(() =>
			{
				AssertType<IDACertificateIdList>("Type: IDACertificationIdentificationList", list);
				AssertEquals("Item count should be 10", 10, list.Count);
				AssertSame("Is cached", list, Lookups.IDACertificateIdList);
			});
		}

		public void TestBPApplicationReasonList()
		{
			var list = Lookups.BeforePermitApplicationReasonList;

			CombineAssertions(() =>
			{
				AssertType<BeforePermitApplicationReasonList>("Type: BPApplicationReasonList", list);
				AssertContainsExactElementsInAnyOrder(new[] { "1A", "1B", "1C", "2A", "2B", "2C", "2D", "2E", "2F", "2G", "3A", "3B", "3C", "3D", "3E", "3F", "3G", "3H", "3J", "4A", "4B", "4C", "9X" }, list.GetAllCodes());
				AssertSame("Is cached", list, Lookups.BeforePermitApplicationReasonList);
			});
		}

		public void TestCustomsOfficeForSpecialDeclarationsList()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1Y");
			var list = Lookups.CustomsOfficeList;

			CombineAssertions(() =>
			{
				AssertEquals(1, list.Count);
				Assert(list.ContainsCode("1Y"));
			});
		}

		public void TestCustomsOfficeDepartmentForSpecialDeclarationsList()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1Y");
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCustomsOfficeDepartment, "1Y01");
			var instruction = CusEntryInstruction;

			CombineAssertions(() =>
			{
				instruction.CEI_CustomsOfficeForSpecialDeclarations = "1Z";
				AssertEquals(0, instruction.Lookups.CustomsOfficeDepartmentForSpecialDeclarationsList.Count);

				instruction.CEI_CustomsOfficeForSpecialDeclarations = "1Y";
				AssertEquals(1, instruction.Lookups.CustomsOfficeDepartmentForSpecialDeclarationsList.Count);
				Assert(instruction.Lookups.CustomsOfficeDepartmentForSpecialDeclarationsList.ContainsCode("01"));
			});
		}

		public void TestGrossWeightUnitList()
		{
			var list = Lookups.GrossWeightUnitList;
			AssertContainsExactElementsInAnyOrder(new[] { "DT", "G", "HG", "KG", "KT", "LB", "LT", "MC", "MG", "OT", "OZ", "T", "TL", "TN" }, list.GetAllCodes());
		}

		public void TestVolumeUnitList()
		{
			var list = Factory.New<CusEntryInstruction>().Lookups.VolumeUnitList;
			AssertEquals("BF, CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE", list.CodesAsString);
		}

		public void TestBondedLocationList()
		{
			var zzd1 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			zzd1.ZZD_Code = "JPBLC";
			zzd1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Japan;
			zzd1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode;
			zzd1.ZZD_StartDate = ZDate.Today.AddDays(-1);

			var zzd2 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			zzd2.ZZD_Code = "USBLC";
			zzd2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.UnitedStates;
			zzd2.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode;
			zzd2.ZZD_StartDate = ZDate.Today.AddDays(-1);

			var zzd3 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			zzd3.ZZD_Code = "JPXXX";
			zzd3.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Japan;
			zzd3.ZZD_CodeType = "XXX";
			zzd3.ZZD_StartDate = ZDate.Today.AddDays(-1);

			var zzd4 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			zzd4.ZZD_Code = "JPBLC-Outdated";
			zzd4.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Japan;
			zzd4.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode;
			zzd4.ZZD_EndDate = ZDate.Today.AddDays(-1);

			var zzd5 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			zzd5.ZZD_Code = "JPBLC-NotYetValid";
			zzd5.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Japan;
			zzd5.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode;
			zzd5.ZZD_StartDate = ZDate.Today.AddDays(1);

			var list = Factory.New<CusEntryInstruction>().Lookups.BondedLocationList as ZZRefCusCodeListCombinedCollection;
			list.Load();

			AssertContainsExactElementsInExactOrder(new[] { zzd1 }, list);
		}

		public void TestSpecialCargoList()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AOG");
			var list = Lookups.SpecialCargoCodeList;
			var collection = (ZZRefCusCodeListCombinedCollection)Lookups.SpecialCargoCodeList;
			collection.Load();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, collection.Count);
				AssertContainsExactElementsInExactOrder(JPRefCusCodeListTypes.GetSpecialCargoCodes(Factory), collection);
			});
		}

		public void TestCargoTypeList()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var cargoTypeList = instruction.Lookups.CargoTypeList;

			CombineAssertions(() =>
			{
				AssertSame("Cached", cargoTypeList, instruction.Lookups.CargoTypeList);
				AssertArrayEqualsByElements(new[] { "R", "T" }, cargoTypeList.GetAllCodes());
			});
		}

		public void TestBillNumberTypeList()
		{
			var list = Lookups.BillNumberTypeList;

			CombineAssertions(() =>
			{
				AssertType<BillNumberTypeList>("Type: BillNumberTypeList", list);
				AssertContainsExactElementsInAnyOrder(new[] { "A", "H", "L" }, list.GetAllCodes());
				AssertSame("Is cached", list, Lookups.BillNumberTypeList);
			});
		}

		public void TestCDB01CargoTypeList()
		{
			var list = Lookups.CDB01CargoTypeList;

			CombineAssertions(() =>
			{
				AssertType<CDB01CargoTypeList>("Type: CDB01CargoTypeList", list);
				AssertContainsExactElementsInAnyOrder(new[] { "N", "R", "T", "O", "K" }, list.GetAllCodes());
				AssertSame("Is cached", list, Lookups.CDB01CargoTypeList);
			});
		}

		public void TestMoveInDestinationCodeList()
		{
			Declaration.JE_CustomsOffice = "1B";
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var collection = Lookups.MoveInDestinationCodeList as ZZRefCusCodeListCombinedCollection;

			CombineAssertions(() =>
			{
				collection.AssertFilterBusinessObjectDefault("Transport Mode:Property", ModuleTextFilter.ComparisonConstants.Exact, Core.Constants.TransportModes.Sea, true);
				collection.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "1", true);
				collection.AssertFilterBusinessObjectDefault("List Type:Property", string.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, false);
				collection.AssertFilterBusinessObjectDefault("Country/Region or Grouping:Property", string.Empty, Core.Constants.CountryCodes.Japan, false);
			});
		}

		public void TestDeclarationCargoTypes()
		{
			var declaration = Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var instruction = CusEntryInstruction;
			var entryInstructionLookups = Lookups;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Import, Air, CEI_Style not euqal 'C'", new[] { "S", "B", "L", "E", "H", "U" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPImportDeclarationTypeList.Codes.C;
				AssertContainsExactElementsInAnyOrder("Import, Air, CEI_Style euqals 'C'", new[] { "S", "B", "L", "X", "E", "H", "U" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertContainsExactElementsInAnyOrder("Import, Sea, CEI_Style euqals 'C'", new[] { "L", "X", "M" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPImportDeclarationTypeList.Codes.A;
				AssertContainsExactElementsInAnyOrder("Import, Sea, CEI_Style not euqal 'C'", new[] { "L", "M" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertContainsExactElementsInAnyOrder("Export, Sea", new[] { "E", "G", "H", "L", "M", "P", "U", "X" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertContainsExactElementsInAnyOrder("Export, Sea", new[] { "S", "B", "L", "X", "G", "K", "E", "H", "U" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());
			});
		}

		public void TestExportDeclarationCargoTypes()
		{
			var declaration = Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var instruction = CusEntryInstruction;
			var entryInstructionLookups = Lookups;

			CombineAssertions(() =>
			{
				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.E;
				AssertContainsExactElementsInAnyOrder("Export, Air, CEI_Style euqal 'E'", new[] { "S", "B", "L", "X", "G", "K", "E", "H", "U" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.N;
				AssertContainsExactElementsInAnyOrder("Export, Air, CEI_Style euqal 'N'", new[] { "S", "B", "L", "X", "G", "K" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.M;
				AssertContainsExactElementsInAnyOrder("Export, Air, CEI_Style euqal 'M'", new[] { "S", "B", "L", "X", "G", "K" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.R;
				AssertContainsExactElementsInAnyOrder("Export, Air, CEI_Style euqal 'R'", new[] { "S", "B", "L", "X", "G", "K", "E", "H", "U" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.T;
				AssertContainsExactElementsInAnyOrder("Export, Air, CEI_Style euqal 'T'", new[] { "S", "B", "K" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
				AssertContainsExactElementsInAnyOrder("Export, Air, CEI_Style euqal 'G'", new[] { "S", "B" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.E;
				AssertContainsExactElementsInAnyOrder("Export, Sea, CEI_Style euqal 'E'", new[] { "L", "X", "G", "P", "E", "H", "M", "U" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.N;
				AssertContainsExactElementsInAnyOrder("Export, Sea, CEI_Style euqal 'N'", new[] { "L", "X", "G", "P" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.M;
				AssertContainsExactElementsInAnyOrder("Export, Sea, CEI_Style euqal 'M'", new[] { "L", "X", "G", "P" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.R;
				AssertContainsExactElementsInAnyOrder("Export, Sea, CEI_Style euqal 'R'", new[] { "L", "X", "G", "P", "E", "H", "M", "U" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.T;
				AssertContainsExactElementsInAnyOrder("Export, Sea, CEI_Style euqal 'T'", new[] { "L", "X", "G", "P" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());

				instruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
				AssertContainsExactElementsInAnyOrder("Export, Sea, CEI_Style euqal 'G'", new[] { "L", "X", "G", "P" }, entryInstructionLookups.DeclarationCargoTypes.GetAllCodes());
			});
		}

		public void TestStyleList()
		{
			var entryInstructionLookups = Lookups;
	
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertType<JPImportDeclarationTypeList>(entryInstructionLookups.StyleList);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = CusEntryInstruction.PK;
				invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.Y;
				AssertType<JPExportDeclarationTypeList>(entryInstructionLookups.StyleList);

				var otherLaw = invoiceLine.OtherLaws.AddNew();
				otherLaw.CFR_Reference = "MS";
				AssertType<BaseExportDeclarationTypeList>(entryInstructionLookups.StyleList);

				invoiceLine.JI_NACCSCode = ExportNACCSCodeList.Codes.T;
				AssertType<ReturnedGoodsDeclarationTypeList>(entryInstructionLookups.StyleList);
			});
		}

		public void TestDeclarationConditionList_Export()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			instruction.CEI_Style = JPExportDeclarationTypeList.Codes.E;
			AssertType<ExportDeclarationConditionListWhenDeclarationTypeIsEorR>(instruction.Lookups.DeclarationConditionList);

			instruction.CEI_Style = JPExportDeclarationTypeList.Codes.R;
			AssertType<ExportDeclarationConditionListWhenDeclarationTypeIsEorR>(instruction.Lookups.DeclarationConditionList);

			instruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
			AssertType<ExportDeclarationConditionListWhenDeclarationTypeIsNotEorR>(instruction.Lookups.DeclarationConditionList);
		}

		public void TestDeclarationConditionList_Import()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var instructionLookups = instruction.Lookups;
			var allCodesForDeclarationType = new JPImportDeclarationTypeList().GetAllCodes();
			var normalCodes = (ZString[])ReflectionUtil.GetPropertyValue(instructionLookups, "NormalDeclarationTypes");

			AsertDeclarationConditionList_WithCertainDeclartionTypes(ImportDeclarationConditionList.Codes.H, normalCodes);
			AsertDeclarationConditionList_WithCertainDeclartionTypes(ImportDeclarationConditionList.Codes.T, normalCodes);

			var codesForOpeningAndNormalReg = (IEnumerable<string>)ReflectionUtil.GetPropertyValue(instructionLookups, "OpeningAndNormalRegDeclarationTypes");

			AsertDeclarationConditionList_WithCertainDeclartionTypes(ImportDeclarationConditionList.Codes.Empty, normalCodes.ToArray().Concat(codesForOpeningAndNormalReg.Select(x => new ZString(x))));
			AsertDeclarationConditionList_WithCertainDeclartionTypes(ImportDeclarationConditionList.Codes.K, normalCodes.Concat(codesForOpeningAndNormalReg.Select(x => new ZString(x))));

			void AsertDeclarationConditionList_WithCertainDeclartionTypes(string condition, IEnumerable<ZString> declarationTypes)
			{
				CombineAssertions(() =>
				{
					foreach (var code in allCodesForDeclarationType)
					{
						instruction.CEI_Style = code;

						if (declarationTypes.Contains(code))
						{
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", instructionLookups.DeclarationConditionList.ContainsCode(condition));
						}
						else
						{
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));
						}
					}
				});
			}

			var codesForAutomaticStart = (IEnumerable<string>)ReflectionUtil.GetPropertyValue(instructionLookups, "AutomaticStartDeclarationTypes");

			AsertDeclarationConditionList_ZUS(ImportDeclarationConditionList.Codes.Z, normalCodes);
			AsertDeclarationConditionList_ZUS(ImportDeclarationConditionList.Codes.U, normalCodes);
			AsertDeclarationConditionList_ZUS(ImportDeclarationConditionList.Codes.S, normalCodes);
			AsertDeclarationConditionList_ZUS(ImportDeclarationConditionList.Codes.J, codesForAutomaticStart.Select(x => new ZString(x)));

			void AsertDeclarationConditionList_ZUS(string condition, IEnumerable<ZString> declarationTypes)
			{
				CombineAssertions(() =>
				{
					foreach (var code in allCodesForDeclarationType)
					{
						instruction.CEI_Style = code;

						if (declarationTypes.Contains(code))
						{
							invoiceLine.JI_BondedDate = ZDateTime.Empty;
							instruction.CEI_DeclarationCargoType = string.Empty;
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", instructionLookups.DeclarationConditionList.ContainsCode(condition));

							invoiceLine.JI_BondedDate = DateTime.Today;
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));

							invoiceLine.JI_BondedDate = ZDateTime.Empty;
							instruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.H;
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));

							instruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.M;
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));
						}
						else
						{
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));
						}
					}
				});
			}

			var codesForMaritimeImports = normalCodes.ToList();
			codesForMaritimeImports.Remove(JPImportDeclarationTypeList.Codes.Y);
			codesForMaritimeImports.Add(JPImportDeclarationTypeList.Codes.G);

			AsertDeclarationConditionList_I(ImportDeclarationConditionList.Codes.I, codesForMaritimeImports);

			void AsertDeclarationConditionList_I(string condition, IEnumerable<ZString> declarationTypes)
			{
				CombineAssertions(() =>
				{
					foreach (var code in allCodesForDeclarationType)
					{
						instruction.CEI_Style = code;

						if (declarationTypes.Contains(code))
						{
							invoiceLine.JI_BondedDate = ZDateTime.Empty;
							instruction.CEI_DeclarationCargoType = string.Empty;
							declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", instructionLookups.DeclarationConditionList.ContainsCode(condition));

							invoiceLine.JI_BondedDate = DateTime.Today;
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));

							invoiceLine.JI_BondedDate = ZDateTime.Empty;
							foreach (var cargoType in new MailedCargoList().GetAllCodes())
							{
								instruction.CEI_DeclarationCargoType = MailedCargoList.Codes.E;
								Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));
							}

							instruction.CEI_DeclarationCargoType = string.Empty;
							declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
							Assert($"JE_TransportMode:{declaration.JE_TransportMode} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));
						}
						else
						{
							Assert($"DeclarationType:{code} , expected DeclarationCondition:{condition}", !instructionLookups.DeclarationConditionList.ContainsCode(condition));
						}
					}
				});
			}
		}

		public void TestCustomsWeightUnitConditionList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var instructionLookups = instruction.Lookups;

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			AssertContainsExactElementsInAnyOrder("HDF01", new[] { "KGM" }, instructionLookups.CustomsWeightUnitConditionList.GetAllCodes());

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("HCH01", new[] { "KGM", "LBR" }, instructionLookups.CustomsWeightUnitConditionList.GetAllCodes());

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertContainsExactElementsInAnyOrder("others", new[] { "KGM", "LBR", "TNE" }, instructionLookups.CustomsWeightUnitConditionList.GetAllCodes());
		}

		public void TestViaList()
		{
			Declaration.JE_CustomsOffice = "1B";
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var collection = CusEntryInstruction.Lookups.ViaList;

			CombineAssertions(() =>
			{
				collection.AssertFilterBusinessObjectDefault("Transport Mode:Property", ModuleTextFilter.ComparisonConstants.Exact, Core.Constants.TransportModes.Sea, true);

				collection.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "1", true);

				collection.AssertFilterBusinessObjectDefault("List Type:Property", string.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, true);

				collection.AssertFilterBusinessObjectDefault("Country/Region or Grouping:Property", string.Empty, Core.Constants.CountryCodes.Japan, true);
			});
		}

		public void TestFinalDestinations()
		{
			var dec = Factory.New<JobDeclaration>();
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, dec.CountryCode));
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, dec.CountryCode));
			dec.JE_RL_NKOrigin = foreignPort.RL_Code;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(false, foreignPort.MatchesFilter(filter));

			dec.JE_RL_NKOrigin = ZString.Empty;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(false, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));

			dec.JE_MessageType = "Z!Z";
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));
		}

		JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
		JobDeclaration declaration;

		CusEntryInstruction CusEntryInstruction => cusEntryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
		CusEntryInstruction cusEntryInstruction;

		CusEntryInstructionLookups Lookups => CusEntryInstruction.Lookups;
	}
}
