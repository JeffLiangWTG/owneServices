using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSPackageCollection))]
	class EMCSPackageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTooManyPackages()
		{
			packages.RemoveAndDeleteAll();
			for (var i = 0; i < 99; i++)
			{
				packages.AddNew();
			}

			CombineAssertions(() =>
			{
				Assert(!packages.HasErrors());

				var extraInvalidPackage = packages.AddNew();
				AssertHasRowError(extraInvalidPackage, "There are too many Packages. Maximum of 99.");
			});
		}

		public void TestAllowNewCore()
		{
			declaration.JE_MessageStatus = string.Empty;
			Factory.InvalidateCachedProperties();

			CombineAssertions(() =>
			{
				Assert("Should default to true.", packages.AllowNew);

				declaration.JE_MessageStatus = EDIMessage.Status.Sent;
				Factory.InvalidateCachedProperties();

				Assert("Should not allow new when the message status of parent is SNT.", !packages.AllowNew);

				declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
				Factory.InvalidateCachedProperties();

				Assert("Should not allow new when the message status of parent is ACK.", !packages.AllowNew);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			packages = declaration.EMCSPackages;
		}
		EMCSJobDeclaration declaration;
		EMCSPackageCollection packages;

		protected override BusinessObjectCollection GetCollectionToTest() => packages;

		protected override System.Type GetExpectedCollectionType() => typeof(EMCSPackageCollection);
	}
}
