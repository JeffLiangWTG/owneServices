using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgOpportunityValueCollection))]
	public class EDIOrgOpportunityValueCollectionTest : OrgOpportunityValueCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			EDIOrgOpportunity opp = Factory.New<EDIOrgOpportunity>();
			return opp.ValueItems;
		}
	}
}
