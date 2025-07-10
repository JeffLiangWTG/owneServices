using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(MaintenanceBillCollection))]
	internal sealed class MaintenanceBillCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MaintenanceBillCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override MaintenanceBillCollection GetCollectionToTest()
		{
			return new MaintenanceBillCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			LicenceHeader lic = Factory.NewWithValidTestData<LicenceHeader>();
			MaintenanceBillRecipient billRecipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, lic.Company.LC_OH, "AUD", ZDateTime.Now);
			return new MaintenanceBill(lic, billRecipient);
		}

		#endregion
	}
}
