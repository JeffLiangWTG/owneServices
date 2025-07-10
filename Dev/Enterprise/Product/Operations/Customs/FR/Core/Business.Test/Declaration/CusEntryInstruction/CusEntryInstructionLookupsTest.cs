using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusEntryInstructionLookupsTest : EU.Business.Declaration.Testing.CusEntryInstructionLookupsTest
	{
		protected override Type TypeOfDeclarationOfInstruction => typeof(JobDeclaration);

		protected override Type GetExpectedEntrySubStyleListType() => typeof(EntrySubstyleCodePairList);

		protected override ZString GetExpectedEntrySubStyleListCodesAsStringWithoutDeclaration() => new EntrySubstyleCodePairList().CodesAsString;
		protected override ZString GetExpectedEntrySubStyleListCodesAsString() => new EntrySubstyleCodePairList().CodesAsString;

		public void TestGetDefinedDeclarationTypeList_DependingOnMessageType()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedureLanguage");
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.France, "IM", "40", "00", "000", "One", "IMP", group: "40");
			helper.CreateRefCusProcedure(CountryCodes.France, "EX", "10", "00", "000", "Four", "EXP", group: "10");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("List should filter on ZZ6_ShipmentType matching declaration message type (EXP).", "10", instruction.Lookups.DeclarationTypeList[0].Code);

				declaration.JE_MessageType = "IMP";
				AssertEquals("List should filter on ZZ6_ShipmentType matching declaration message type (IMP).", "40", instruction.Lookups.DeclarationTypeList[0].Code);
			});
		}

		[TestDate(2024,10,17)]
		public void TestDeclarationTypeList_DependingOnDateForDuty()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedureLanguage");
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure");
			var date = new ZDateTime(2024, 10, 12, 10, 22, 00);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedureWithDefaultDate(CountryCodes.France, "IM", "10", "00", "000", "One", "IMP", date.AddDays(-5), date.AddDays(-1), group: "10");
			helper.CreateRefCusProcedureWithDefaultDate(CountryCodes.France, "IM", "40", "00", "000", "two", "IMP", date.AddDays(-5), date.AddDays(-2), group: "40");
			helper.CreateRefCusProcedureWithDefaultDate(CountryCodes.France, "IM", "20", "00", "000", "three", "IMP", date.AddDays(1), date.AddDays(+10), group: "20");
			helper.CreateRefCusProcedureWithDefaultDate(CountryCodes.France, "IM", "50", "00", "000", "four", "IMP", date, date.AddDays(+10), group: "50");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				instruction.CEI_DateForDuty = ZDateTime.Empty;
				AssertContainsExactElementsInAnyOrder("List should filter on CEI_DateForDuty - date is empty then should take date = today.", new ZString[] { "20", "50" }, instruction.Lookups.DeclarationTypeList.GetAllCodes());

				instruction.CEI_DateForDuty = ZDateTime.Invalid;
				AssertContainsExactElementsInAnyOrder("List should filter on CEI_DateForDuty - date is invalid then should take date = today.", new ZString[] { "20", "50" }, instruction.Lookups.DeclarationTypeList.GetAllCodes());

				instruction.CEI_DateForDuty = date;
				AssertContainsExactElementsInAnyOrder("List should filter on CEI_DateForDuty - date is equal start date.", new ZString[] { "50" }, instruction.Lookups.DeclarationTypeList.GetAllCodes());

				instruction.CEI_DateForDuty = date.AddDays(-1);
				AssertContainsExactElementsInAnyOrder("List should filter on CEI_DateForDuty - date is equal end date.", new ZString[] { "10" }, instruction.Lookups.DeclarationTypeList.GetAllCodes());

				instruction.CEI_DateForDuty = date.AddDays(2);
				AssertContainsExactElementsInAnyOrder("List should filter on CEI_DateForDuty - date is between start date and end date.", new ZString[] { "20", "50" }, instruction.Lookups.DeclarationTypeList.GetAllCodes());

				instruction.CEI_DateForDuty = date.AddDays(-10);
				AssertEquals("List should filter on CEI_DateForDuty - date too old.", 0, instruction.Lookups.DeclarationTypeList.Count);

				instruction.CEI_DateForDuty = date.AddDays(11);
				AssertEquals("List should filter on CEI_DateForDuty - date is too far in the future.", 0, instruction.Lookups.DeclarationTypeList.Count);
			});
		}

		public void TestDeclarationTypeList_DependingOnApplicationCode()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedureLanguage");
			Db.Connection.ExecuteNonQuery("DELETE FROM RefDatabase_RefCusProcedure");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "EX", "01", "00", "000", "Export DeltaIE", "EXP", group: "H1");
			helper.CreateRefCusProcedure(CountryCodes.France, "EX", "01", "00", "000", "Export DeltaG", "EXP", group: "10");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			AssertEquals("List should be based on DB records having FR as data grouping.", "10", instruction.Lookups.DeclarationTypeList[0].Code);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var instruction2 = Factory.New<CusEntryInstruction>();
			instruction2.CEI_JE = declaration2.PK;
			AssertEquals("List should be based on DB records having DIE as data grouping.", "H1", instruction2.Lookups.DeclarationTypeList[0].Code);
		}

		public void TestCPCList()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedureLanguage");
			Db.Connection.ExecuteNonQuery("DELETE FROM RefDatabase_RefCusProcedure");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.France, "", "10", "00", "000", "Export DeltaG Procedure Group 10", EU.Business.MessageTypeList.Codes.Export, group: "B1");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "21", "00", "000", "Export DeltaG Procedure Group 21", EU.Business.MessageTypeList.Codes.Export, group: "B2");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "22", "00", "000", "Export DeltaIE Procedure Group B1", EU.Business.MessageTypeList.Codes.Export, group: "B1");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "31", "00", "000", "Export DeltaIE Procedure Group B2", EU.Business.MessageTypeList.Codes.Export, group: "B2");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "01", "00", "000", "Import DeltaG Procedure Group 01", EU.Business.MessageTypeList.Codes.Import, group: "B1");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "02", "00", "000", "Import DeltaG Procedure Group 02", EU.Business.MessageTypeList.Codes.Import, group: "B2");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "07", "00", "000", "Import DeltaIE Procedure Group B1", EU.Business.MessageTypeList.Codes.Import, group: "B1");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "40", "00", "000", "Import DeltaIE Procedure Group B2", EU.Business.MessageTypeList.Codes.Import, group: "B2");
			Factory.Save();

			CombineAssertions("List should filter on FR Data Grouping, Message Type, Declaration Application Code and Declaration Type", () =>
			{
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B1", "10");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B2", "21");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B1", "01");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B2", "02");
			});
		}

		public void TestEntrySubstyleCodePairList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("For DeltaG declarations, Lookups.EntrySubStyleList should be same as EntrySubstyleCodePairList.", new EntrySubstyleCodePairList().CodesAsString, instruction.Lookups.EntrySubStyleList.CodesAsString);
		}

		void AssertProcedureList(string messageType, string declarationType, string expectedResult)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = declarationType;
			AssertEquals(expectedResult, entryInstruction.Lookups.CPCList.CodesAsString);
		}
	}
}
