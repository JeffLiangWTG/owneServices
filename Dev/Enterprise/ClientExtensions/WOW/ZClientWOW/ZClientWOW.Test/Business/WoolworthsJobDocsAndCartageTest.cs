using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobDocsAndCartage))]
	public class WoolworthsJobDocsAndCartageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOnChangeOfJP_DeliveryCartageAdvisedDoNotPopulateToJC_ArrivalCartageAdvised()
		{
			WoolworthsJobDeclaration declaration = Factory.New<WoolworthsJobDeclaration>();
			WoolworthsCusContainer cusContainer = (WoolworthsCusContainer)declaration.CusContainers.AddNew();
			ZDateTime currentDateTime = ZDateTime.Now;
			declaration.DocsAndCartage.JP_DeliveryCartageAdvised = currentDateTime;
			AssertEquals(true, cusContainer.JobContainer.JC_ArrivalCartageAdvised.IsEmpty && cusContainer.JobContainer.JC_ArrivalCartageAdvised != currentDateTime);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return JobDocsAndCartage.New(new WoolworthsMockJobDocsAndCartageParent(Factory));
		}
		#endregion
	}
}
