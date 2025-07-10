using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibilityOverrideCollection))]
	class EventVisibilityOverrideCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EventVisibilityOverrideCollection>
	{
		public void TestAllowNew()
		{
			Assert(Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			Assert(Collection.AllowRemove);
		}

		public void AddNewWithWorkflowCode()
		{
			AssertEquals(0, Collection.Count);

			var eventInfo = Collection.AddNew("TST");
			AssertNotNull(eventInfo);
			AssertCollectionContains(eventInfo, Collection);
			AssertEquals("TST", eventInfo.Code);
		}

		public void TestAddNew_DuplicateWorkflowCode_ValidationError()
		{
			var element0 = Collection.AddNew("HVC");
			var element1 = Collection.AddNew("TST");
			Collection.RunPreSaveValidation();

			var errorMessage = "The Code has been duplicated and must be unique.";
			AssertNoError("Prerequesite", element0.CodeInfo, errorMessage);
			AssertNoError("Prerequesite", element1.CodeInfo, errorMessage);

			var element2 = Collection.AddNew("HVC");
			Collection.RunPreSaveValidation();

			AssertHasError("Element 0 has error", element0.CodeInfo, errorMessage);
			AssertNoError("Element 1 does not have error", element1.CodeInfo, errorMessage);
			AssertHasError("Element 2 has error", element2.CodeInfo, errorMessage);
		}

		#region Overrides

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override EventVisibilityOverrideCollection GetCollectionToTest()
		{
			return new EventVisibilityOverrideCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EventVisibilityOverride("TST");
		}

		#endregion
	}
}
