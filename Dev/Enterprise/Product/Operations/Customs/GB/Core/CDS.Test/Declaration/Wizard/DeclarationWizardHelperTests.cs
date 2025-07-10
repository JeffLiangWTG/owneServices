using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class DeclarationWizardHelperTests : TestCaseWithFactory
	{
		public void TestCanReadCSVFile()
		{
			var declarationWizardItems = new DeclarationWizardHelper().ReadCsvString(csvData);

			AssertEquals("Should have 30 items", 30, declarationWizardItems.Length);

			CombineAssertions(() =>
			{
				var item = declarationWizardItems.FirstOrDefault();
				AssertEquals("Property RequestedProcedure not set correctly", "01", item.RequestedProcedure);
				AssertEquals("Property ProcedureDefinition not set correctly", "Release for free circulation with simultaneous onward dispatch to another Customs Union Territory", item.ProcedureDefinition);
				AssertEquals("Property Q2DeclarationType not set correctly", "FC", item.Q2DeclarationType);
				AssertEquals("Property Q3 not set correctly", "OD", item.Q3);
				AssertEquals("Property DeclarationType not set correctly", "IM", item.DeclarationType);
				AssertEquals("Property AdditionalDeclarationTypes not set correctly", "D", item.AdditionalDeclarationTypes()[1]);
				AssertEquals("Property Q1 not set correctly", "IM", item.Q1);
				AssertEquals("Property AdditionalDeclarationType not set correctly", "A, D or Y", item.AdditionalDeclarationType);
				AssertEquals("Property ProcedureCategory not set correctly", "H1", item.ProcedureCategory);
				AssertEquals("Property AdditionalDeclarationTypes not set correctly", "A", item.AdditionalDeclarationTypes()[0]);
				AssertEquals("Property AdditionalDeclarationTypes not set correctly", "D", item.AdditionalDeclarationTypes()[1]);
				AssertEquals("Property AdditionalDeclarationTypes not set correctly", "Y", item.AdditionalDeclarationTypes()[2]);
			});

			CombineAssertions(() =>
			{
				var item = declarationWizardItems.LastOrDefault();
				AssertEquals("Property RequestedProcedure not set correctly", "71", item.RequestedProcedure);
				AssertEquals("Property ProcedureDefinition not set correctly", "Entry to a Customs Warehouse(CW)", item.ProcedureDefinition);
				AssertEquals("Property Q2DeclarationType not set correctly", "CW", item.Q2DeclarationType);
				AssertEquals("Property Q3 not set correctly", "", item.Q3);
				AssertEquals("Property DeclarationType not set correctly", "IM or CO", item.DeclarationType);
				AssertEquals("Property Q1 not set correctly", "CO", item.Q1);
				AssertEquals("Property AdditionalDeclarationType not set correctly", "C or F", declarationWizardItems.LastOrDefault().AdditionalDeclarationType);
				AssertEquals("Property ProcedureCategory not set correctly", "I1", declarationWizardItems.LastOrDefault().ProcedureCategory);
				AssertEquals("Property AdditionalDeclarationTypes not set correctly", "C", item.AdditionalDeclarationTypes()[0]);
				AssertEquals("Property AdditionalDeclarationTypes not set correctly", "F", item.AdditionalDeclarationTypes()[1]);
			});
		}

		readonly string[] csvData = new string[] { @"Requested Procedure (D.E. 1/10),Procedure definition,Question 2 - declaration type,Question 3 (only needed for #2='FC'), ""Declaration Type(D.E 1/1)"",Question 1 - IM or CO, Additional Declaration Type (D.E. 1/2),Procedure Category",
@"01,Release for free circulation with simultaneous onward dispatch to another Customs Union Territory,FC,OD,IM,IM,""A, D or Y"",H1",
@"01,Release for free circulation with simultaneous onward dispatch to another Customs Union Territory,FC,OD,IM,IM,C or F ,I1",
@"07,Release for Free Circulation with simultaneous Entry to an Excise Warehouse,FC,EXC,IM ,IM,""A, D or Y"",H1",
@"07,Release for Free Circulation with simultaneous Entry to an Excise Warehouse,FC,EXC,CO,CO,""A, D or Y"",H5",
@"07,Release for Free Circulation with simultaneous Entry to an Excise Warehouse,FC,EXC,IM or CO ,IM,C or F ,I1",
@"07,Release for Free Circulation with simultaneous Entry to an Excise Warehouse,FC,EXC,IM or CO ,CO,C or F ,I1",
@"40,Release to free circulation, FC, None, IM, IM,""A, D, Y or Z"", H1",
@"40, Release to free circulation ,FC,None,CO,CO,""A, D, Y or Z"",H5",
@"40,Release to free circulation, FC, None, IM or CO, IM,""B, C, E or F"", I1",
@"40, Release to free circulation ,FC,None,IM or CO ,CO,""B, C, E or F"",I1",
@"42,Release for free circulation with simultaneous onward supply to another member state, FC, OSR, IM, IM,""A,  D or Y"", H1",
@"42, Release for free circulation with simultaneous onward supply to another member state, FC, OSR, IM, IM, C or F, I1",
@"44, Release to free circulation with duty relief granted under the End Use Special Procedure,FC,End Use, IM, IM,""A, D, Y or Z"", H1",
@"44, Release to free circulation with duty relief granted under the End Use Special Procedure,FC,End Use, IM, IM,""B, C, E or F"", I1",
@"51, Entry to Inward Processing,IP,,IM or CO,IM,""A, D, Y or Z"",H4",
@"51,Entry to Inward Processing, IP,, IM or CO, CO,""A, D, Y or Z"", H4",
@"51, Entry to Inward Processing,IP,,IM or CO,IM,C or F,I1",
@"51,Entry to Inward Processing, IP,, IM or CO, CO, C or F, I1",
@"53, Entry to Temporary Admission,TA,,IM or CO ,IM,""A, D, Y or Z"",H3",
@"53,Entry to Temporary Admission, TA,, IM or CO, CO,""A, D, Y or Z"", H3",
@"53, Entry to Temporary Admission,TA,,IM or CO ,IM,C or F,I1",
@"53,Entry to Temporary Admission, TA,, IM or CO, CO, C or F, I1",
@"61, Re-importation with simultaneous release to Free Circulation,FC,Re-Imp,IM  ,IM,""A, D, Y or Z"",H1",
@"61,Re-importation with simultaneous release to Free Circulation,FC,Re-Imp,CO,CO,""A, D, Y or Z"",H5",
@"61,Re-importation with simultaneous release to Free Circulation,FC,Re-Imp,IM or CO ,IM,C or F,I1",
@"61,Re-importation with simultaneous release to Free Circulation,FC,Re-Imp,IM or CO ,CO,C or F,I1",
@"71,Entry to a Customs Warehouse(CW),CW,,IM or CO ,IM,A or D ,H2",
@"71,Entry to a Customs Warehouse(CW),CW,,IM or CO ,CO,A or D ,H2",
@"71,Entry to a Customs Warehouse(CW),CW,,IM or CO ,IM,C or F,I1",
@"71,Entry to a Customs Warehouse(CW),CW,,IM or CO ,CO,C or F,I1" };
	}
}
