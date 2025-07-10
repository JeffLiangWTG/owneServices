using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	internal class ClientCognosGroupingFlagsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIntercompanyCodeLookups()
		{
			AssertEquals("There should be 4 elements in the list", 4, GroupingFlags.Lookups.IntercompanyCodeList.Count);
			AssertEquals("Exclude Intercompany Code", GroupingFlags.Lookups.IntercompanyCodeList.GetDescriptionFromCode("NO"));
			AssertEquals("Include Intercompany Code", GroupingFlags.Lookups.IntercompanyCodeList.GetDescriptionFromCode("I/A"));
			AssertEquals("Include Intercompany Code with Transaction Currency and Amount", GroupingFlags.Lookups.IntercompanyCodeList.GetDescriptionFromCode("J"));
			AssertEquals("Specify ICTOTA as Intercompany Code", GroupingFlags.Lookups.IntercompanyCodeList.GetDescriptionFromCode("ICT"));
		}

		CognosGroupingFlags GroupingFlags
		{
			get
			{
				if (fGroupingFlags == null)
				{
					fGroupingFlags = Factory.New<CognosGroupingFlags>();
				}

				return fGroupingFlags;
			}
		}

		CognosGroupingFlags fGroupingFlags;
	}
}
