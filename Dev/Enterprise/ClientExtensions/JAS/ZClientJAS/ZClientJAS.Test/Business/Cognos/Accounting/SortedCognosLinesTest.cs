using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class SortedCognosLinesTest : CognosLineSorterBaseTestCase
	{
		public void TestAddAndGetLinesAsString()
		{
			SetupGroupingFlagsRegistry();
			AddCognosLinesForTest();
			AssertMultilineEquals("LinesAsString not as expected", ExpectedSortedLinesAsString, SortedCognosLines.GetLinesAsString(), '\n');
		}

		#region Implementation
		void SetupGroupingFlagsRegistry()
		{
			AddNewCognosGroupingFlagsAndSignage(CognosAccountPK1, 1, 2, 3, 4, "NO");
			AddNewCognosGroupingFlagsAndSignage(CognosAccountPK2, 2, 1, 0, 0, "I/A");
			AddNewCognosGroupingFlagsAndSignage(CognosAccountPK3, 0, 0, 1, 2, "J");
			AddNewCognosGroupingFlagsAndSignage(CognosAccountPK4, 4, 1, 2, 3, "ICT");
		}

		void AddCognosLinesForTest()
		{
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeB", "BranchA", "BusinessTypeA", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeB", "BranchA", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeB", "BranchB", "BusinessTypeA", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeA", "BranchA", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeA", "BranchA", "BusinessTypeB", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeB", "BranchB", "BusinessTypeA", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeB", "BranchB", "BusinessTypeB", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeA", "BranchB", "BusinessTypeA", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeB", "BranchA", "BusinessTypeB", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeA", "BranchA", "BusinessTypeA", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeB", "BranchB", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeA", "BranchB", "BusinessTypeA", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeA", "BranchB", "BusinessTypeB", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeA", "BranchB", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK1, "ModeB", "BranchA", "BusinessTypeA", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeC", "BranchB", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeF", "BranchB", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeC", "BranchE", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeB", "BranchF", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeA", "BranchG", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeA", "BranchB", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeC", "BranchA", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeD", "BranchA", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeE", "BranchA", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeB", "BranchB", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeB", "BranchD", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeD", "BranchB", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeD", "BranchC", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeA", "BranchD", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeA", "BranchA", "", "", "CompanyC", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeA", "BranchA", "", "", "CompanyB", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeA", "BranchA", "", "", "CompanyA", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK2, "ModeB", "BranchA", "", ""));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeB", "BranchA", "BusinessTypeA", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeB", "BranchA", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeB", "BranchB", "BusinessTypeA", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchA", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchA", "BusinessTypeB", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeB", "BranchB", "BusinessTypeA", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeB", "BranchB", "BusinessTypeB", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchB", "BusinessTypeA", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeB", "BranchA", "BusinessTypeB", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA", "", "USD"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA", "", "IDR"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchA", "BusinessTypeA", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeB", "BranchB", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchB", "BusinessTypeA", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchB", "BusinessTypeB", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeA", "BranchB", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK4, "ModeB", "BranchA", "BusinessTypeA", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeA", "GeographicalZ"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeA", "GeographicalG"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeA", "GeographicalF"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeA", "GeographicalC"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeA", "GeographicalB", "CompanyA", "USD"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeA", "GeographicalB", "CompanyA", "AUD"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeA", "GeographicalB", "CompanyB", "IDR"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeA", "GeographicalB", "CompanyB", "GBP"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeB", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeB", "GeographicalD"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeC", "GeographicalC"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeC", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeD", "GeographicalA"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeD", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeE", "GeographicalG"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeE", "GeographicalD"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeE", "GeographicalF"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeF", "GeographicalB"));
			SortedCognosLines.Add(GetCognosLine(CognosAccountPK3, "", "", "BusinessTypeF", "GeographicalA"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			SortedCognosLines = new SortedCognosLines(Factory);
			CognosAccountPK1 = Factory.New<CognosAccGLAccountDescriptor>().PK;
			CognosAccountPK2 = Factory.New<CognosAccGLAccountDescriptor>().PK;
			CognosAccountPK3 = Factory.New<CognosAccGLAccountDescriptor>().PK;
			CognosAccountPK4 = Factory.New<CognosAccGLAccountDescriptor>().PK;
		}

		SortedCognosLines SortedCognosLines;
		ZGuid CognosAccountPK1;
		ZGuid CognosAccountPK2;
		ZGuid CognosAccountPK3;
		ZGuid CognosAccountPK4;
		const string ExpectedSortedLinesAsString = @" ,,,ModeA,BranchA,BusinessTypeA,0,,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeA,0,,,GeographicalB
 ,,,ModeA,BranchA,BusinessTypeB,0,,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeB,0,,,GeographicalB
 ,,,ModeA,BranchB,BusinessTypeA,0,,,GeographicalA
 ,,,ModeA,BranchB,BusinessTypeA,0,,,GeographicalB
 ,,,ModeA,BranchB,BusinessTypeB,0,,,GeographicalA
 ,,,ModeA,BranchB,BusinessTypeB,0,,,GeographicalB
 ,,,ModeB,BranchA,BusinessTypeA,0,,,GeographicalA
 ,,,ModeB,BranchA,BusinessTypeA,0,,,GeographicalB
 ,,,ModeB,BranchA,BusinessTypeB,0,,,GeographicalA
 ,,,ModeB,BranchA,BusinessTypeB,0,,,GeographicalB
 ,,,ModeB,BranchB,BusinessTypeA,0,,,GeographicalA
 ,,,ModeB,BranchB,BusinessTypeA,0,,,GeographicalB
 ,,,ModeB,BranchB,BusinessTypeB,0,,,GeographicalA
 ,,,ModeB,BranchB,BusinessTypeB,0,,,GeographicalB
 ,,CompanyA,ModeA,BranchA,,0,,,
 ,,CompanyB,ModeA,BranchA,,0,,,
 ,,CompanyC,ModeA,BranchA,,0,,,
 ,,,ModeB,BranchA,,0,,,
 ,,,ModeC,BranchA,,0,,,
 ,,,ModeD,BranchA,,0,,,
 ,,,ModeE,BranchA,,0,,,
 ,,,ModeA,BranchB,,0,,,
 ,,,ModeB,BranchB,,0,,,
 ,,,ModeC,BranchB,,0,,,
 ,,,ModeD,BranchB,,0,,,
 ,,,ModeF,BranchB,,0,,,
 ,,,ModeD,BranchC,,0,,,
 ,,,ModeA,BranchD,,0,,,
 ,,,ModeB,BranchD,,0,,,
 ,,,ModeC,BranchE,,0,,,
 ,,,ModeB,BranchF,,0,,,
 ,,,ModeA,BranchG,,0,,,
 ,,,ModeA,BranchA,BusinessTypeA,0,,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeA,0,IDR,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeA,0,USD,,GeographicalA
 ,,,ModeB,BranchA,BusinessTypeA,0,,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeA,0,,,GeographicalB
 ,,,ModeB,BranchA,BusinessTypeA,0,,,GeographicalB
 ,,,ModeA,BranchA,BusinessTypeB,0,,,GeographicalA
 ,,,ModeB,BranchA,BusinessTypeB,0,,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeB,0,,,GeographicalB
 ,,,ModeB,BranchA,BusinessTypeB,0,,,GeographicalB
 ,,,ModeA,BranchB,BusinessTypeA,0,,,GeographicalA
 ,,,ModeB,BranchB,BusinessTypeA,0,,,GeographicalA
 ,,,ModeA,BranchB,BusinessTypeA,0,,,GeographicalB
 ,,,ModeB,BranchB,BusinessTypeA,0,,,GeographicalB
 ,,,ModeA,BranchB,BusinessTypeB,0,,,GeographicalA
 ,,,ModeB,BranchB,BusinessTypeB,0,,,GeographicalA
 ,,,ModeA,BranchB,BusinessTypeB,0,,,GeographicalB
 ,,,ModeB,BranchB,BusinessTypeB,0,,,GeographicalB
 ,,CompanyA,,,BusinessTypeA,0,AUD,,GeographicalB
 ,,CompanyA,,,BusinessTypeA,0,USD,,GeographicalB
 ,,CompanyB,,,BusinessTypeA,0,GBP,,GeographicalB
 ,,CompanyB,,,BusinessTypeA,0,IDR,,GeographicalB
 ,,,,,BusinessTypeA,0,,,GeographicalC
 ,,,,,BusinessTypeA,0,,,GeographicalF
 ,,,,,BusinessTypeA,0,,,GeographicalG
 ,,,,,BusinessTypeA,0,,,GeographicalZ
 ,,,,,BusinessTypeB,0,,,GeographicalA
 ,,,,,BusinessTypeB,0,,,GeographicalD
 ,,,,,BusinessTypeC,0,,,GeographicalA
 ,,,,,BusinessTypeC,0,,,GeographicalC
 ,,,,,BusinessTypeD,0,,,GeographicalA
 ,,,,,BusinessTypeD,0,,,GeographicalB
 ,,,,,BusinessTypeE,0,,,GeographicalD
 ,,,,,BusinessTypeE,0,,,GeographicalF
 ,,,,,BusinessTypeE,0,,,GeographicalG
 ,,,,,BusinessTypeF,0,,,GeographicalA
 ,,,,,BusinessTypeF,0,,,GeographicalB";
		#endregion
	}
}
