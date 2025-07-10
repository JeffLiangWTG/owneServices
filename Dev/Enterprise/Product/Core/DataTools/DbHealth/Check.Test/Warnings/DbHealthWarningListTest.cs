using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	sealed class DbHealthWarningListTest : TestCase
	{
		public void TestTableHeader()
		{
			DbHealthWarningList testWarningList = new DbHealthWarningList();
			testWarningList.Add(new DummyWarning("S0", "W0", "D0", "A0"));

			string html = testWarningList.ToHtmlMessage();
			Assert(html.Contains("<th><b>Type</b></th>"));
			Assert(html.Contains("<th><b>Source</b></th>"));
			Assert(html.Contains("<th><b>Description</b></th>"));
			Assert(html.Contains("<th><b>Recommended Action</b></th>"));
		}

		public void TestAdd()
		{
			DbHealthWarningList testWarningList = new DbHealthWarningList();

			AssertEquals("Count 0", 0, testWarningList.Count);

			testWarningList.Add(new DummyWarning("S0", "W0", "D0", "A0"));
			AssertEquals("Count 1", 1, testWarningList.Count);
			AssertEquals("Source Type Dummy", DummyWarning.StaticSourceType, testWarningList[0].SourceType);
			AssertEquals("Source", "S0", testWarningList[0].Source);
			AssertEquals("Warning Type", "W0", testWarningList[0].WarningType);
			AssertEquals("Description", "D0", testWarningList[0].Description);
			AssertEquals("Action", "A0", testWarningList[0].Action);

			testWarningList.Add(new VeryDummyWarning("S1", "W1", "D1", "A1"));
			AssertEquals("Count 2", 2, testWarningList.Count);
			AssertEquals("Source Type Disk", VeryDummyWarning.StaticSourceType, testWarningList[1].SourceType);
		}

		public void TestAddRange()
		{
			DbHealthWarningList testWarningList1 = new DbHealthWarningList();
			testWarningList1.Add(new VeryDummyWarning("S0", "W0", "D0", "A0"));

			AssertEquals("Count 1", 1, testWarningList1.Count);

			DbHealthWarningList testWarningList2 = new DbHealthWarningList();
			testWarningList2.Add(new DummyWarning("S1", "W1", "D1", "A1"));
			testWarningList2.Add(new DummyWarning("S2", "W2", "D2", "A2"));
			testWarningList2.Add(new VeryDummyWarning("S3", "W3", "D3", "A3"));

			testWarningList1.AddRange(testWarningList2);
			AssertEquals("Count 4", 4, testWarningList1.Count);

			AssertEquals("Description 0", "D0", testWarningList1[0].Description);
			AssertEquals("Description 1", "D1", testWarningList1[1].Description);
			AssertEquals("Description 2", "D2", testWarningList1[2].Description);
			AssertEquals("Description 3", "D3", testWarningList1[3].Description);

			AssertEquals("Source Type 0", VeryDummyWarning.StaticSourceType, testWarningList1[0].SourceType);
			AssertEquals("Source Type 1", DummyWarning.StaticSourceType, testWarningList1[1].SourceType);
			AssertEquals("Source Type 2", DummyWarning.StaticSourceType, testWarningList1[2].SourceType);
			AssertEquals("Source Type 3", VeryDummyWarning.StaticSourceType, testWarningList1[3].SourceType);
		}

		public void TestListOrder()
		{
			DbHealthWarningList testWarningList = new DbHealthWarningList();
			VeryDummyWarning testWarning0 = new VeryDummyWarning("S0", "W0", "D0", "A0");
			DummyWarning testWarning1 = new DummyWarning("S1", "W1", "D1", "A1");
			VeryDummyWarning testWarning2 = new VeryDummyWarning("S2", "W2", "D2", "A2");
			testWarningList.Add(testWarning0);
			testWarningList.Add(testWarning1);
			testWarningList.Add(testWarning2);

			AssertEquals("Source 0", "S0", testWarningList[0].Source);
			AssertEquals("Source 1", "S1", testWarningList[1].Source);
			AssertEquals("Source 2", "S2", testWarningList[2].Source);

			AssertEquals("Source Type 0", VeryDummyWarning.StaticSourceType, testWarningList[0].SourceType);
			AssertEquals("Source Type 1", DummyWarning.StaticSourceType, testWarningList[1].SourceType);
			AssertEquals("Source Type 2", VeryDummyWarning.StaticSourceType, testWarningList[2].SourceType);

			AssertEquals("Warning Type 0", "W0", testWarningList[0].WarningType);
			AssertEquals("Warning Type 1", "W1", testWarningList[1].WarningType);
			AssertEquals("Warning Type 2", "W2", testWarningList[2].WarningType);

			AssertEquals("Description 0", "D0", testWarningList[0].Description);
			AssertEquals("Description 1", "D1", testWarningList[1].Description);
			AssertEquals("Description 2", "D2", testWarningList[2].Description);

			AssertEquals("Action 0", "A0", testWarningList[0].Action);
			AssertEquals("Action 1", "A1", testWarningList[1].Action);
			AssertEquals("Action 2", "A2", testWarningList[2].Action);

			string allDescriptions = "";

			foreach (DbHealthWarning waring in testWarningList)
			{
				allDescriptions += waring.Description + "_";
			}

			AssertEquals("Concatenated Descriptions", "D0_D1_D2_", allDescriptions);
		}
	}
}
