using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(EventSubscriptionCollection))]
	public class GlbExternalPasswordCollection_BRTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EventSubscriptionCollection(Factory.NewWithValidTestData<GlbStaff>());
		}

		public void TestSetDefaultsForNewChild()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var eventSubscription = new EventSubscriptionCollection(staff);
			var externalPasswordBr = eventSubscription.AddNew();
			AssertEquals("GP_PasswordType should default to 'BRS'", PasswordTypesList.Codes.BRS, externalPasswordBr.GP_PasswordType);
		}
	}
}
