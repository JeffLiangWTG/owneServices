using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business
{
	[TestedType(typeof(IncidentApprovalCollection))]
	sealed class IncidentApprovalCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IncidentApprovalCollection(Factory);
		}
	}
}
