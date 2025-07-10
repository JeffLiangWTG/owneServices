using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordAuthorisationCollection))]
	public class GlbExternalPasswordAuthorisationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var externalPassword = Factory.NewWithValidTestData<GlbExternalPassword>();
			externalPassword.GP_GS = ZGuid.Empty;
			var collection = new GlbExternalPasswordAuthorisationCollection(externalPassword);

			CombineAssertions(() =>
			{
				AssertNull("Staff on the collection is null", collection.Master.Staff);
				AssertEquals("Valid Staff required", false, collection.AllowNew);
				externalPassword.GP_GS = Factory.NewWithValidTestData<GlbStaff>().PK;
				AssertEquals("Staff is not current user", false, collection.AllowNew);
				externalPassword.GP_GS = GlbStaff.CurrentUser.PK;
				AssertEquals("Staff is current user", true, collection.AllowNew);
			});
		}

		public void TestAllowRemove()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var externalPassword = Factory.NewWithValidTestData<GlbExternalPassword>();
			externalPassword.GP_GS = ZGuid.Empty;
			var collection = new GlbExternalPasswordAuthorisationCollection(externalPassword);

			CombineAssertions(() =>
			{
				AssertNull("Staff on the collection is null", collection.Master.Staff);
				AssertEquals("Valid Staff required", false, collection.AllowRemove);
				externalPassword.GP_GS = staff.PK;
				AssertEquals("Staff is not current user", false, collection.AllowRemove);
				externalPassword.GP_GS = GlbStaff.CurrentUser.PK;
				AssertEquals("Staff is current user and not controller", true, collection.AllowRemove);
				externalPassword.GP_GS = staff.PK;
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals("Staff is different user and current user is controller controller", true, collection.AllowRemove);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
			=> new GlbExternalPasswordAuthorisationCollection(Factory.NewWithValidTestData<GlbExternalPassword>());
	}
}
