using System.Collections;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.OrgContactCollection))]
	sealed class XsdSequenceIncrementHelperTest : ValueObjectCollectionTestCase
	{
		public void TestSequenceNotGeneratedIfAlreadyExists()
		{
			TestSequencedCollection collection = new TestSequencedCollection();
			TestSequencedItem item = new TestSequencedItem();
			item.Sequence = 99;
			collection.Add(item);
			AssertEquals("Sequence should not be changed if already set", 99, item.Sequence);
		}

		public void TestAssignNewIncrementedSequence()
		{
			TestSequencedCollection collection = new TestSequencedCollection();
			TestSequencedItem sequenced1 = new TestSequencedItem();
			TestSequencedItem sequenced2 = new TestSequencedItem();
			collection.Add(sequenced1);
			collection.Add(sequenced2);
			AssertEquals("First sequence", 1, sequenced1.Sequence);
			AssertEquals("First sequence specified", true, sequenced1.SequenceSpecified);
			AssertEquals("Second sequence", 2, sequenced2.Sequence);
			AssertEquals("Second sequence specified", true, sequenced2.SequenceSpecified);
		}

		class TestSequencedCollection : CollectionBase, ISequencedValueObjectCollection
		{
			public void Add(ISequencedValueObject item)
			{
				List.Add(item);
			}

			protected override void OnInsertComplete(int index, object value)
			{
				base.OnInsertComplete(index, value);
				new XsdSequenceIncrementHelper(this).AssignNewIncrementedSequence((ISequencedValueObject)value);
			}
		}

		class TestSequencedItem : ISequencedValueObject
		{
			public ZInt Sequence
			{
				get { return fSequence; }
				set { fSequence = value; }
			}
			ZInt fSequence;

			public bool SequenceSpecified
			{
				get { return fSequenceSpecified; }
				set { fSequenceSpecified = value; }
			}
			bool fSequenceSpecified;
		}
	}
}
