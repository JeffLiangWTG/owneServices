using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(OrgSystemUsage))]
	internal class OrgSystemUsageTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgSystemUsage(null, null);
		}

		#endregion
	}

	[TestedType(typeof(OrgSystemUsageCollection))]
	internal class OrgSystemUsageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgSystemUsageCollection>
	{
		#region Implementation

		protected override OrgSystemUsageCollection GetCollectionToTest()
		{
			return new OrgSystemUsageCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgSystemUsage(null, null);
		}

		#endregion
	}
}
