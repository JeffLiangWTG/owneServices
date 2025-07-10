using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RequireReasonForCLRItemCollection))]
	sealed class RequireReasonForCLRItemCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RequireReasonForCLRItemCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RequireReasonForCLRItemCollection GetCollectionToTest() => new RequireReasonForCLRItemCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new RequireReasonForCLRItem();

		public void TestAllowNew()
		{
			AssertEquals(true, Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(true, Collection.AllowRemove);
		}

		public new void TestClone()
		{
			var currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new RequireReasonForCLRItemCollection();
			var originalItem = collection.AddNew();
			originalItem.Code = "C01";
			originalItem.Title = "Title1";
			originalItem.ClearingReason = "Description1";
			originalItem.IsMandatory = false;

			var clone = collection.Clone(currentFallbackLevel, Factory);
			AssertNotEquals("Clone should be a different instance.", collection, clone);

			var cloneElement = clone[0] as RequireReasonForCLRItem;

			CombineAssertions("clone element and original item should have identical properties", () =>
			{
				AssertEquals(cloneElement.Code, originalItem.Code);
				AssertEquals(cloneElement.Title, originalItem.Title);
				AssertEquals(cloneElement.ClearingReason, originalItem.ClearingReason);
				AssertEquals(cloneElement.IsMandatory, originalItem.IsMandatory);
			});
		}
	}
}
