using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibilityCollection))]
	sealed class EventVisibilityCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EventVisibilityCollection>
	{
		public void TestAllowNew()
		{
			Assert(Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			Assert(Collection.AllowRemove);
		}

		public void TestItemParentOnAddedAndRemoved()
		{
			var eventCode = GetNewElementToAddToTheCollection() as EventVisibility;
			AssertNotNull(eventCode);
			AssertCollectionNotContains(eventCode, Collection);
			AssertNull(eventCode.Parent);

			Collection.Add(eventCode);
			AssertCollectionContains(eventCode, Collection);
			AssertNotNull(eventCode.Parent);
			AssertEquals(Collection, eventCode.Parent);

			Collection.Remove(eventCode);
			AssertCollectionNotContains(eventCode, Collection);
			AssertNull(eventCode.Parent);
		}

		public void AddNewWithEventCode()
		{
			AssertEquals(0, Collection.Count);

			var eventCode = Collection.AddNew("TST");
			AssertNotNull(eventCode);
			AssertCollectionContains(eventCode, Collection);
			AssertEquals("TST", eventCode.EventCode);
		}

		#region overrides

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override EventVisibilityCollection GetCollectionToTest()
		{
			return new EventVisibilityCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EventVisibility("TST");
		}

		#endregion
	}
}
