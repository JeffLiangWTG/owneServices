using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExportCusEntryInstructionLookupsTest : CusEntryInstructionLookupsAbstractTest<ExportCusEntryInstructionLookups>
	{
		public void TestEntrySubStyleList()
		{
			var expectedData = new (string DeclarationType, string ExpectedCodesAsString)[]
			{
				(ExportDeclarationTypeList.Codes.B1, "A, D, Y"),
				(ExportDeclarationTypeList.Codes.B2, "A, D, Y"),
				(ExportDeclarationTypeList.Codes.B3, "A, D, Y"),
				(ExportDeclarationTypeList.Codes.B4, "A, D, C, F, Y"),
				(ExportDeclarationTypeList.Codes.C1, "C, F"),
				(string.Empty, string.Empty),
			};

			for (var i = 0; i < expectedData.Length; i++)
			{
				var (declarationType, expectedCodesAsString) = expectedData[i];
				AssertEntrySubStyleList(declarationType, expectedCodesAsString);
			}
		}

		public void TestDeclarationTypeListByJE_EntryStyle()
		{
			jobDeclaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			AssertEquals("B3,B4 for CO", "B3, B4", lookups.DeclarationTypeListG2018.CodesAsString);

			jobDeclaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;
			AssertEquals("B1,B2,C1 for EX", "B1, B2, C1", lookups.DeclarationTypeListG2018.CodesAsString);
		}

		public void TestDeclarationType_Caching()
		{
			jobDeclaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			_ = lookups.DeclarationTypeList;

			var newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			newDeclaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			var newInstruction = newDeclaration.CustomsEntryInstructions[0];
			AssertSame("JE_EntryStyle CO, DeclarationTypeList should have been cached.", lookups.DeclarationTypeListG2018, ((ExportCusEntryInstructionLookups)newInstruction.Lookups).DeclarationTypeListG2018);

			jobDeclaration.JE_EntryStyle = newDeclaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;
			AssertSame("JE_EntryStyle EX, DeclarationTypeList should have been cached.", lookups.DeclarationTypeListG2018, ((ExportCusEntryInstructionLookups)newInstruction.Lookups).DeclarationTypeListG2018);
		}

		void AssertEntrySubStyleList(string declarationType, string expectedCodesAsString)
		{
			instruction.CEI_Style = declarationType;
			AssertEquals(expectedCodesAsString, lookups.EntrySubStyleList.CodesAsString);
		}

		protected override (string code, string desc)[] GetExpectedData() => new (string code, string desc)[]
		{
			(ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Descriptions.B1),
			(ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Descriptions.B2),
			(ExportDeclarationTypeList.Codes.B3, ExportDeclarationTypeList.Descriptions.B3),
			(ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Descriptions.B4),
			(ExportDeclarationTypeList.Codes.C1, ExportDeclarationTypeList.Descriptions.C1),
		};

		protected override string MessageType => MessageTypeList.Codes.Export;
	}
}
