using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.ODPL
{
	[TestedType(typeof(OdplModuleUsageCollection))]
	internal class OdplModuleUsageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OdplModuleUsageCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override OdplModuleUsageCollection GetCollectionToTest()
		{
			return new OdplModuleUsageCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OdplModuleUsage(Factory);
		}

		#endregion
	}
}
