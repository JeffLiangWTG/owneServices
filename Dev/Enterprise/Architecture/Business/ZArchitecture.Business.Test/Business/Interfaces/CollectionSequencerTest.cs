using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class CollectionSequencerTest : TestCaseWithFactory
	{
		#region TestMoveUp

		public void TestMoveUp()
		{
			var collection = GetNewCollection();
			var sequencer = GetNewSequencer(collection);

			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = collection.AddNew();
			var dummy4 = collection.AddNew();
			SetSequence(dummy1, 1);
			SetSequence(dummy2, 2);
			SetSequence(dummy3, 3);
			SetSequence(dummy4, 4);
			AssertBusinessObjectSequence(dummy1, dummy2, dummy3, dummy4);

			sequencer.MoveUp(dummy4);
			AssertBusinessObjectSequence(dummy1, dummy2, dummy4, dummy3);

			sequencer.MoveUp(dummy4);
			AssertBusinessObjectSequence(dummy1, dummy4, dummy2, dummy3);

			sequencer.MoveUp(dummy4);
			AssertBusinessObjectSequence(dummy4, dummy1, dummy2, dummy3);

			sequencer.MoveUp(dummy4);
			AssertBusinessObjectSequence(dummy4, dummy1, dummy2, dummy3);
		}

		#endregion

		#region TestMoveDown

		public void TestMoveDown()
		{
			var collection = GetNewCollection();
			var sequencer = GetNewSequencer(collection);

			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = collection.AddNew();
			var dummy4 = collection.AddNew();
			SetSequence(dummy1, 1);
			SetSequence(dummy2, 2);
			SetSequence(dummy3, 3);
			SetSequence(dummy4, 4);
			AssertBusinessObjectSequence(dummy1, dummy2, dummy3, dummy4);

			sequencer.MoveDown(dummy1);
			AssertBusinessObjectSequence(dummy2, dummy1, dummy3, dummy4);

			sequencer.MoveDown(dummy1);
			AssertBusinessObjectSequence(dummy2, dummy3, dummy1, dummy4);

			sequencer.MoveDown(dummy1);
			AssertBusinessObjectSequence(dummy2, dummy3, dummy4, dummy1);

			sequencer.MoveDown(dummy1);
			AssertBusinessObjectSequence(dummy2, dummy3, dummy4, dummy1);
		}

		#endregion

		#region TestSequence

		public void TestSequence()
		{
			var collection = GetNewCollection();
			var sequencer = GetNewSequencer(collection);

			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = collection.AddNew();
			var dummy4 = collection.AddNew();

			SetSequence(dummy1, 0);
			SetSequence(dummy2, 3);
			SetSequence(dummy3, 3);
			SetSequence(dummy4, 10);

			sequencer.Sequence();
			AssertEquals(1, GetSequence(dummy1));
			AssertNotEquals(GetSequence(dummy2), GetSequence(dummy3));
			Assert(GetSequence(dummy2) == 2 || GetSequence(dummy2) == 3);
			Assert(GetSequence(dummy3) == 2 || GetSequence(dummy3) == 3);
			AssertEquals(4, GetSequence(dummy4));
		}

		#endregion

		#region TestSortBySequence

		public void TestSortBySequence()
		{
			var collection = GetNewCollection();
			var sequencer = GetNewSequencer(collection);

			var iActiveCollection = (IActiveBusinessObjectCollection)collection;
			AssertNull("Precondition", iActiveCollection.SortProperty);

			sequencer.SortBySequence();
			AssertEquals(SequenceSchemaColumn.Name, iActiveCollection.SortProperty.Name);
			AssertEquals(ListSortDirection.Ascending, iActiveCollection.SortDirection);
		}

		#endregion

		#region Implementation

		CollectionSequencer<DummyBusinessObject> GetNewSequencer(ActiveBusinessObjectCollection<DummyBusinessObject> collection)
		{
			return new CollectionSequencer<DummyBusinessObject>(collection, SequenceSchemaColumn, b => SequenceValidation(b));
		}

		ActiveBusinessObjectCollection<DummyBusinessObject> GetNewCollection()
		{
			return new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
		}

		void AssertBusinessObjectSequence(DummyBusinessObject dummy1, DummyBusinessObject dummy2, DummyBusinessObject dummy3, DummyBusinessObject dummy4)
		{
			AssertEquals(1, GetSequence(dummy1));
			AssertEquals(2, GetSequence(dummy2));
			AssertEquals(3, GetSequence(dummy3));
			AssertEquals(4, GetSequence(dummy4));
		}

		protected abstract SchemaNumericColumn SequenceSchemaColumn { get; }
		protected abstract Action<DummyBusinessObject> SequenceValidation { get; }
		protected abstract void SetSequence(DummyBusinessObject dummy, int value);

		ZInt GetSequence(DummyBusinessObject dummy)
		{
			return ((INumericZType)dummy[SequenceSchemaColumn]).ToZInt();
		}

		#endregion
	}
}
