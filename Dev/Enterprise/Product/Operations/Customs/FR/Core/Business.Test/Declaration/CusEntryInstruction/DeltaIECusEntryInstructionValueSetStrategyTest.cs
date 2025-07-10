using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIECusEntryInstructionValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestDefaultCEI_Style()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.CONTI;
			declaration.JE_TransportModeInland = ZString.Empty;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var currentStyle = ZString.Empty;
			var expectedStyle = DeltaIEImportDeclarationTypeList.Codes.H1;
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._01, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._07, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._40, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._42, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._43, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._44, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._45, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._46, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._48, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._61, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._63, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._68, currentStyle, expectedStyle);

			expectedStyle = DeltaIEImportDeclarationTypeList.Codes.H2;
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration, currentStyle, expectedStyle);

			expectedStyle = DeltaIEImportDeclarationTypeList.Codes.H3;
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._53, currentStyle, expectedStyle);

			expectedStyle = DeltaIEImportDeclarationTypeList.Codes.H4;
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._51, currentStyle, expectedStyle);

			expectedStyle = DeltaIEImportDeclarationTypeList.Codes.H5;
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._95, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._96, currentStyle, expectedStyle);

			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.GUADE;
			expectedStyle = DeltaIEImportDeclarationTypeList.Codes.H5;
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._40, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._42, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._61, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._63, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._95, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._96, currentStyle, expectedStyle);

			declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
			expectedStyle = DeltaIEImportDeclarationTypeList.Codes.H6;
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._01, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._07, currentStyle, expectedStyle);
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._40, currentStyle, expectedStyle);

			currentStyle = DeltaIEImportDeclarationTypeList.Codes.I1;
			expectedStyle = DeltaIEImportDeclarationTypeList.Codes.I1;
			AssertCEI_Style(entryInstruction, Core.Constants.Customs.Universal.RefCusProcedure.Codes._01, currentStyle, expectedStyle);
		}

		void AssertCEI_Style(CusEntryInstruction entryInstruction, ZString newProcedure, ZString currentStyle, ZString expectedStyle)
		{
			entryInstruction.CEI_Style = currentStyle;
			entryInstruction.CEI_Procedure = newProcedure;
			AssertEquals(expectedStyle, entryInstruction.CEI_Style);
		}
	}
}
