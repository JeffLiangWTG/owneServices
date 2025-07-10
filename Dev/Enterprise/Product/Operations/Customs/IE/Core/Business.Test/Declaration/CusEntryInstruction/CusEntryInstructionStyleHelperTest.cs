using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class CusEntryInstructionStyleHelperTest : TestCaseWithFactory
	{
		public void TestGetDefaultDeclarationType()
		{
			CombineAssertions("GetDefaultDeclarationType() should return correct results per JE_MessageType.", () =>
			{
				AssertEquals("REX -> A3.", ReExportDeclarationTypeList.Codes.A3, CusEntryInstructionStyleHelper.GetDefaultDeclarationType(IEJobMessageTypeList.Codes.ReExport));
				AssertEquals("EXS -> A1.", ExitSummaryDeclarationTypeList.Codes.A1, CusEntryInstructionStyleHelper.GetDefaultDeclarationType(IEJobMessageTypeList.Codes.ExitSummary));
				AssertEquals("EXP -> B1.", ExportDeclarationTypeList.Codes.B1, CusEntryInstructionStyleHelper.GetDefaultDeclarationType(IEJobMessageTypeList.Codes.Export));
				AssertEquals("IMP -> H1.", ImportDeclarationTypeList.Codes.H1, CusEntryInstructionStyleHelper.GetDefaultDeclarationType(IEJobMessageTypeList.Codes.Import));
				AssertEquals("Empty for unrecognized MessageType.", ZString.Empty, CusEntryInstructionStyleHelper.GetDefaultDeclarationType("XXX"));
				AssertEquals("Empty for empty input.", ZString.Empty, CusEntryInstructionStyleHelper.GetDefaultDeclarationType(ZString.Empty));
			});
		}

		public void TestSecurityRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("SecurityRequired returns false for empty Instruction", false, CusEntryInstructionStyleHelper.SecurityRequired(instruction));

			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			AssertEquals("CEI_Style B1, JE_EntryStyle CO, SecurityRequired returns false.", false, CusEntryInstructionStyleHelper.SecurityRequired(instruction));

			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;
			AssertEquals("CEI_Style B1, JE_EntryStyle NOT CO, SecurityRequired returns true.", true, CusEntryInstructionStyleHelper.SecurityRequired(instruction));

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			AssertEquals("CEI_Style B2, SecurityRequired returns true.", true, CusEntryInstructionStyleHelper.SecurityRequired(instruction));
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.C1;
			AssertEquals("CEI_Style C1, SecurityRequired returns true.", true, CusEntryInstructionStyleHelper.SecurityRequired(instruction));

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			AssertEquals("CEI_Style B3, SecurityRequired returns false.", false, CusEntryInstructionStyleHelper.SecurityRequired(instruction));
		}

		public void TestTransportChargesMoPRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("TransportChargesMoPRequired returns false for empty Instruction", false, CusEntryInstructionStyleHelper.TransportChargesMoPRequired(instruction));

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;
			AssertEquals("EXP, B1, non-CO TransportChargesMoPRequired returns true.", true, CusEntryInstructionStyleHelper.TransportChargesMoPRequired(instruction));

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			AssertEquals("EXS, TransportChargesMoPRequired returns true.", true, CusEntryInstructionStyleHelper.TransportChargesMoPRequired(instruction));
		}
	}
}
