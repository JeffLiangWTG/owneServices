using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(MaintenanceBillRecipientCollection))]
	internal sealed class MaintenanceBillRecipientCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MaintenanceBillRecipientCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override MaintenanceBillRecipientCollection GetCollectionToTest()
		{
			return new MaintenanceBillRecipientCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MaintenanceBillRecipient(Factory, ZGuid.Empty, ZGuid.Empty, "", ZDateTime.Today);
		}

		#endregion
	}
}
