using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaIEJobComInvoiceLineLookupsTest : TestCaseWithFactory
	{
		public void TestCPCList()
		{
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

			CombineAssertions("List should filter on DIE Data Grouping,  Message Type, Declaration Application Code and DeclarationType", () =>
			{
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B1", "2200000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B2", "3100000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B1", "0700000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B2", "4000000");
			});
		}

		void AssertProcedureList(string messageType, string declarationType, string expectedResult)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = declarationType;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			AssertContainsExactElementsInAnyOrder(new string[] { expectedResult }, invoiceLine.Lookups.CPCList.Select(x => x.ZZ6_ProcedureCode + x.ZZ6_PreviousProcedureCode + x.ZZ6_Concession).ToArray());
		}

		public void TestCPCListProcedureCodeFilterIsPrepopulatedFromEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_Procedure = "45";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.JI_CEI = ZGuid.Empty;
			var lookups = invoiceLine.Lookups.CPCList;

			AssertNull("Procedure code filter in CPCList should not be prepopulated when no Entry Instruction is linked.", lookups.FilterBusinessObjectDefaults["Procedure Code" + ":Property"].Value);

			invoiceLine.JI_CEI = entryInstruction.PK;
			lookups = invoiceLine.Lookups.CPCList;

			AssertEquals("Procedure code filter in CPCList should be prepopulated with the value from Entry Instruction CEI_Procedure.", "45", lookups.FilterBusinessObjectDefaults["Procedure Code" + ":Property"].Value);
		}
	}
}
