using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EdiOrgMembershipLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMembershipsSort()
		{
			var autoEdiOrgMembership = Factory.NewWithValidTestData<EdiOrgMembership>();
			var codeDescriptionBoolCollection = autoEdiOrgMembership.Lookups.MembershipTypes;
			if (codeDescriptionBoolCollection.Count > 1)
			{
				for (var i = 0; i < codeDescriptionBoolCollection.Count - 1; i++)
				{
					Assert("The collection should be sorted",codeDescriptionBoolCollection[i].Description.CompareTo(codeDescriptionBoolCollection[i + 1].Description) < 0);
				}
			}
		}
	}
}
