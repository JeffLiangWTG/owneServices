using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EDIWorkItemCollection))]
	public class EDIWorkItemCollectionTest : ActiveBusinessObjectCollectionTestCase<EDIWorkItemCollection>
	{
		protected override EDIWorkItemCollection GetCollectionToTest()
		{
			return new EDIWorkItemCollection(Factory);
		}
	}
}
