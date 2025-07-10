using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	class Level1LineExtensionMethodTest : TestCaseWithFactory
	{
		public void TestAddOrAppend()
		{
			var dictionary = new Dictionary<ZString, ISet<ZString>>();
			Assert("Precondition: Key should not exist as yet", !dictionary.ContainsKey("ABC"));
			dictionary.AddOrAppend("ABC", "123");
			Assert("Key should exist", dictionary.ContainsKey("ABC"));
			AssertContainsExactElementsInAnyOrder("Value should be stored in set.", new[] { "123" }, dictionary["ABC"]);
			dictionary.AddOrAppend("ABC", "456");
			AssertContainsExactElementsInAnyOrder("Additional value should be appended to set.", new[] { "123", "456" }, dictionary["ABC"]);
		}

		public void TestIsGCCChild()
		{
			var level1Record = new Level1Record();
			AssertEquals(false, Level1LineExtensionMethod.IsGCCChild(null));
			AssertEquals(false, level1Record.IsGCCChild());
			level1Record._200000 = new _200000Line(Level1RecordWithPlaceHolders.Replace(DutyTypePlaceHolder, "D").Replace(ConsolidatedClearanceFlagHolder, "H"));
			AssertEquals(false, level1Record.IsGCCChild());
			level1Record._200000 = new _200000Line(Level1RecordWithPlaceHolders.Replace(DutyTypePlaceHolder, "C").Replace(ConsolidatedClearanceFlagHolder, "N"));
			AssertEquals(false, level1Record.IsGCCChild());
			level1Record._200000 = new _200000Line(Level1RecordWithPlaceHolders.Replace(DutyTypePlaceHolder, "C").Replace(ConsolidatedClearanceFlagHolder, "H"));
			AssertEquals(true, level1Record.IsGCCChild());
		}

		public void TestIsGCCLead()
		{
			var level1Record = new Level1Record();
			AssertEquals(false, Level1LineExtensionMethod.IsGCCLead(null));
			AssertEquals(false, level1Record.IsGCCLead());
			level1Record._200000 = new _200000Line(Level1RecordWithPlaceHolders.Replace(DutyTypePlaceHolder, "D").Replace(ConsolidatedClearanceFlagHolder, "H"));
			AssertEquals(false, level1Record.IsGCCLead());
			level1Record._200000 = new _200000Line(Level1RecordWithPlaceHolders.Replace(DutyTypePlaceHolder, "C").Replace(ConsolidatedClearanceFlagHolder, "N"));
			AssertEquals(false, level1Record.IsGCCLead());
			level1Record._200000 = new _200000Line(Level1RecordWithPlaceHolders.Replace(DutyTypePlaceHolder, "C").Replace(ConsolidatedClearanceFlagHolder, "L"));
			AssertEquals(true, level1Record.IsGCCLead());
			level1Record._200000 = new _200000Line(Level1RecordWithPlaceHolders.Replace(DutyTypePlaceHolder, "C").Replace(ConsolidatedClearanceFlagHolder, "V"));
			AssertEquals(true, level1Record.IsGCCLead());
		}

		const string DutyTypePlaceHolder = "{DutyType}";
		const string ConsolidatedClearanceFlagHolder = "{ConsolidatedClearanceFlag}";
		const string Level1RecordWithPlaceHolders = "                                {DutyType}                                                                                                                                                    {ConsolidatedClearanceFlag}";
	}
}
