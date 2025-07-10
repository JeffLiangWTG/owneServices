using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.OpportunityManager.Business.Test
{
	[TestedType(typeof(EDIOrgOpportunityCollection))]
	public class EDIOrgOpportunityCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionMembers()
		{
			EDIOrgOpportunity opportunity = Factory.New<EDIOrgOpportunity>();

			Collection.Load();

			AssertEquals("Collection should contain Opportunity.", true, Collection.Contains(opportunity));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDIOrgOpportunityCollection(Factory);
		}

		#endregion
	}
}
