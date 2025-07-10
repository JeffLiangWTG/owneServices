
namespace Enterprise.Registry.Business.Testing
{
	abstract class MilestoneEventUpdatesCollectionTest<T> : RegistryBusinessObjectCollectionTemplateTestCase<T> where T : MilestoneEventUpdatesCollection
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
			var eventUpdate = GetNewElementToAddToTheCollection() as MilestoneEventUpdates;
			AssertNotNull(eventUpdate);
			AssertCollectionNotContains(eventUpdate, Collection);
			AssertNull(eventUpdate.Parent);

			Collection.Add(eventUpdate);
			AssertCollectionContains(eventUpdate, Collection);
			AssertNotNull(eventUpdate.Parent);
			AssertEquals(Collection, eventUpdate.Parent);

			Collection.Remove(eventUpdate);
			AssertCollectionNotContains(eventUpdate, Collection);
			AssertNull(eventUpdate.Parent);
		}

		public void AddNewWithEventType()
		{
			AssertEquals(0, Collection.Count);

			var eventUpdate = Collection.AddNew("TST");
			AssertNotNull(eventUpdate);
			AssertCollectionContains(eventUpdate, Collection);
			AssertEquals("TST", eventUpdate.EventType);
			AssertEquals(0, eventUpdate.WebPartyTypes.Count);
		}

		public void AddNewWithEventTypeAndPartyTypes()
		{
			AssertEquals(0, Collection.Count);

			var eventUpdate = Collection.AddNew("TST", "PY1", "PY2");
			AssertNotNull(eventUpdate);
			AssertCollectionContains(eventUpdate, Collection);
			AssertEquals("TST", eventUpdate.EventType);
			AssertEquals(2, eventUpdate.WebPartyTypes.Count);
			AssertCollectionContains("PY1", eventUpdate.WebPartyTypes);
			AssertCollectionContains("PY2", eventUpdate.WebPartyTypes);
			Assert(eventUpdate.GetValue("PY1"));
			Assert(eventUpdate.GetValue("PY2"));
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

		#endregion
	}
}
