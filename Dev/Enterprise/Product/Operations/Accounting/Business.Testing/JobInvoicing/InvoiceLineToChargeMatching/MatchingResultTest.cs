using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing.InvoiceLineToChargeMatching
{
	class MatchingResultTest : TestCaseWithFactory
	{
		public void TestMatchingOutcomeEnum()
		{
			string[] matchingOutcomePriorityList = { "MatchingNotPerformed", "NoMatchFound", "PartiallyMatched", "FullyMatched" };

			var enumStrings = Enum.GetNames(typeof(MatchingOutcome));

			AssertEquals(4, enumStrings.Length);

			for (int i = 0; i < 4; i++)
			{
				AssertEquals("The order of items in MatchingOutcome should match the order in the MatchingOutcomePriorityList", matchingOutcomePriorityList[i], enumStrings[i]);
			}
		}
	}
}
