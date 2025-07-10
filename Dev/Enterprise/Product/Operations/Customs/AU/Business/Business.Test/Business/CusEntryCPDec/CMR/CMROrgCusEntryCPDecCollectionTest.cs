using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMROrgCusEntryCPDecCollection))]
	public class CMROrgCusEntryCPDecCollectionTest : CMRCusEntryCPDecCollectionTest
	{
		public new void TestAllowNew()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrganisationCPQA orgCPQA = new OrganisationCPQA(org);
			CMRCusEntryCPDecCollection cPCollection = orgCPQA.Questions;
			AssertEquals("Collection's 'AllowNew' property", true, cPCollection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrganisationCPQA orgCPQA = new OrganisationCPQA(org);
			return new CMROrgCusEntryCPDecCollection(orgCPQA);
		}
	}
}
