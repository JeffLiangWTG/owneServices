using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosLineSorterTest : CognosLineSorterBaseTestCase
	{
		#region Scenario 1
		public void TestGetSortedLinesAsString_Scenario1()
		{
			ZGuid cognosAccountPK = Factory.New<CognosAccGLAccountDescriptor>().PK;
			AddNewCognosGroupingFlagsAndSignage(cognosAccountPK, 1, 2, 3, 4, "J");
			CognosLineSorter cognosLineSorter = new CognosLineSorter(Factory, cognosAccountPK);
			AddCognosLinesForScenario1(cognosLineSorter, cognosAccountPK);
			AssertMultilineEquals("SortedLinesAsString not as expected", ExpectedSortedLinesAsStringForScenario1, cognosLineSorter.GetSortedLinesAsString(), '\n');
		}

		void AddCognosLinesForScenario1(CognosLineSorter cognosLineSorter, ZGuid cognosAccountPK)
		{
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "BusinessTypeA", "GeographicalB", "", "USD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeB", "GeographicalB", "CompanyA", "USD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "BusinessTypeB", "GeographicalA", "", "USD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeA", "GeographicalA", "CompanyB", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeB", "GeographicalA", "CompanyB", "USD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeB", "GeographicalB", "CompanyB", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeA", "GeographicalB", "CompanyA", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeB", "GeographicalB", "CompanyA", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "BusinessTypeA", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "BusinessTypeB", "GeographicalB", "", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA", "CompanyA", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalB", "CompanyA", "USD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeB", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeB", "GeographicalB", "CompanyB", "USD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "BusinessTypeA", "GeographicalA", "CompanyB", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "BusinessTypeB", "GeographicalB", "CompanyB", "USD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "BusinessTypeB", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "BusinessTypeA", "GeographicalA", "", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeB", "GeographicalB", "CompanyB", "AUD"));
		}

		const string ExpectedSortedLinesAsStringForScenario1 = @" ,,CompanyA,ModeA,BranchA,BusinessTypeA,0,AUD,,GeographicalA
 ,,CompanyA,ModeA,BranchA,BusinessTypeA,0,USD,,GeographicalB
 ,,CompanyB,ModeA,BranchA,BusinessTypeB,0,USD,,GeographicalA
 ,,CompanyB,ModeA,BranchA,BusinessTypeB,0,AUD,,GeographicalB
 ,,CompanyB,ModeA,BranchB,BusinessTypeA,0,AUD,,GeographicalA
 ,,,ModeA,BranchB,BusinessTypeA,0,,,GeographicalB
 ,,,ModeA,BranchB,BusinessTypeB,0,,,GeographicalA
 ,,CompanyB,ModeA,BranchB,BusinessTypeB,0,USD,,GeographicalB
 ,,,ModeB,BranchA,BusinessTypeA,0,AUD,,GeographicalA
 ,,,ModeB,BranchA,BusinessTypeA,0,USD,,GeographicalB
 ,,,ModeB,BranchA,BusinessTypeB,0,USD,,GeographicalA
 ,,,ModeB,BranchA,BusinessTypeB,0,AUD,,GeographicalB
 ,,CompanyB,ModeB,BranchB,BusinessTypeA,0,AUD,,GeographicalA
 ,,CompanyA,ModeB,BranchB,BusinessTypeA,0,AUD,,GeographicalB
 ,,,ModeB,BranchB,BusinessTypeB,0,,,GeographicalA
 ,,CompanyA,ModeB,BranchB,BusinessTypeB,0,AUD,,GeographicalB
 ,,CompanyA,ModeB,BranchB,BusinessTypeB,0,USD,,GeographicalB
 ,,CompanyB,ModeB,BranchB,BusinessTypeB,0,AUD,,GeographicalB
 ,,CompanyB,ModeB,BranchB,BusinessTypeB,0,USD,,GeographicalB";
		#endregion
		#region Scenario 2
		public void TestGetSortedLinesAsString_Scenario2()
		{
			ZGuid cognosAccountPK = Factory.New<CognosAccGLAccountDescriptor>().PK;
			AddNewCognosGroupingFlagsAndSignage(cognosAccountPK, 2, 1, 0, 0, "NO");
			CognosLineSorter cognosLineSorter = new CognosLineSorter(Factory, cognosAccountPK);
			AddCognosLinesForScenario2(cognosLineSorter, cognosAccountPK);
			AssertMultilineEquals("SortedLinesAsString not as expected", ExpectedSortedLinesAsStringForScenario2, cognosLineSorter.GetSortedLinesAsString(), '\n');
		}

		void AddCognosLinesForScenario2(CognosLineSorter cognosLineSorter, ZGuid cognosAccountPK)
		{
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeC", "BranchB", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeF", "BranchB", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeC", "BranchE", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchF", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchG", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeC", "BranchA", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeD", "BranchA", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeE", "BranchA", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchD", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeD", "BranchB", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeD", "BranchC", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchD", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "", ""));
		}

		const string ExpectedSortedLinesAsStringForScenario2 = @" ,,,ModeA,BranchA,,0,,,
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
 ,,,ModeA,BranchG,,0,,,";
		#endregion
		#region Scenario 3
		public void TestGetSortedLinesAsString_Scenario3()
		{
			ZGuid cognosAccountPK = Factory.New<CognosAccGLAccountDescriptor>().PK;
			AddNewCognosGroupingFlagsAndSignage(cognosAccountPK, 0, 0, 1, 2, "I/A");
			CognosLineSorter cognosLineSorter = new CognosLineSorter(Factory, cognosAccountPK);
			AddCognosLinesForScenario3(cognosLineSorter, cognosAccountPK);
			AssertMultilineEquals("SortedLinesAsString not as expected", ExpectedSortedLinesAsStringForScenario3, cognosLineSorter.GetSortedLinesAsString(), '\n');
		}

		void AddCognosLinesForScenario3(CognosLineSorter cognosLineSorter, ZGuid cognosAccountPK)
		{
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeF", "GeographicalA", "CompanyG", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeA", "GeographicalZ"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeA", "GeographicalG"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeA", "GeographicalF"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeA", "GeographicalC"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeA", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeB", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeB", "GeographicalD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeC", "GeographicalC"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeC", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeD", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeF", "GeographicalA", "CompanyD", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeD", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeE", "GeographicalG"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeE", "GeographicalD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeE", "GeographicalF"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeF", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeF", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeF", "GeographicalA", "CompanyC", ""));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "", "", "BusinessTypeF", "GeographicalA", "CompanyB", ""));
		}

		const string ExpectedSortedLinesAsStringForScenario3 = @" ,,,,,BusinessTypeA,0,,,GeographicalB
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
 ,,CompanyB,,,BusinessTypeF,0,,,GeographicalA
 ,,CompanyC,,,BusinessTypeF,0,,,GeographicalA
 ,,CompanyD,,,BusinessTypeF,0,,,GeographicalA
 ,,CompanyG,,,BusinessTypeF,0,,,GeographicalA
 ,,,,,BusinessTypeF,0,,,GeographicalB";
		#endregion
		#region Scenario 4
		public void TestGetSortedLinesAsString_Scenario4()
		{
			ZGuid cognosAccountPK = Factory.New<CognosAccGLAccountDescriptor>().PK;
			AddNewCognosGroupingFlagsAndSignage(cognosAccountPK, 4, 1, 2, 3, "ICT");
			CognosLineSorter cognosLineSorter = new CognosLineSorter(Factory, cognosAccountPK);
			AddCognosLinesForScenario4(cognosLineSorter, cognosAccountPK);
			AssertMultilineEquals("SortedLinesAsString not as expected", ExpectedSortedLinesAsStringForScenario4, cognosLineSorter.GetSortedLinesAsString(), '\n');
		}

		void AddCognosLinesForScenario4(CognosLineSorter cognosLineSorter, ZGuid cognosAccountPK)
		{
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "BusinessTypeA", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "BusinessTypeB", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA", "", "AUD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeA", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeB", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeB", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeA", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeB", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA", "", "USD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "BusinessTypeA", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "BusinessTypeB", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA", "", "HKD"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA", "", "GBP"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchB", "BusinessTypeB", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "BusinessTypeA", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchA", "BusinessTypeA", "GeographicalA", "", "IDR"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "BusinessTypeB", "GeographicalB"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeA", "BranchB", "BusinessTypeB", "GeographicalA"));
			cognosLineSorter.Add(GetCognosLine(cognosAccountPK, "ModeB", "BranchA", "BusinessTypeA", "GeographicalA"));
		}

		const string ExpectedSortedLinesAsStringForScenario4 = @" ,,,ModeA,BranchA,BusinessTypeA,0,,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeA,0,AUD,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeA,0,GBP,,GeographicalA
 ,,,ModeA,BranchA,BusinessTypeA,0,HKD,,GeographicalA
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
 ,,,ModeB,BranchB,BusinessTypeB,0,,,GeographicalB";
		#endregion
	}
}
