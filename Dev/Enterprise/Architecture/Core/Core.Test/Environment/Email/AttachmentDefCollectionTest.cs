using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AttachmentDefCollectionTest : TestCase
	{
		class SubAttachmentDef : AttachmentDef
		{
			public SubAttachmentDef() : base("", Array.Empty<byte>())
			{ }
		}

		AttachmentDefCollectionForTest Collection;
		AttachmentDef AttachmentDef;
		SubAttachmentDef TestSubAttachmentDef;
		AttachmentDef NullAttachmentDef;

		protected override void SetUp()
		{
			base.SetUp();
			Collection = new AttachmentDefCollectionForTest();
			AttachmentDef = new AttachmentDef("", Array.Empty<byte>());
			TestSubAttachmentDef = new SubAttachmentDef();
			NullAttachmentDef = null;
		}

		public void TestIndexer()
		{
			Collection.Add(null);
			Collection.Add(null);
			Collection.Add(null);

			Collection[0] = AttachmentDef;
			Collection[1] = TestSubAttachmentDef;
			Collection[2] = NullAttachmentDef;

			AssertEquals(AttachmentDef, Collection[0]);
			AssertEquals(TestSubAttachmentDef, Collection[1]);
			AssertEquals(NullAttachmentDef, Collection[2]);
		}

		public void TestAdd()
		{
			Collection.Add(AttachmentDef);
			Collection.Add(TestSubAttachmentDef);
			Collection.Add(NullAttachmentDef);

			AssertEquals(AttachmentDef, Collection[0]);
			AssertEquals(TestSubAttachmentDef, Collection[1]);
			AssertEquals(NullAttachmentDef, Collection[2]);
		}

		public void TestIndexOf()
		{
			Collection.Add(AttachmentDef);
			Collection.Add(TestSubAttachmentDef);
			Collection.Add(NullAttachmentDef);

			AssertEquals(0, Collection.IndexOf(AttachmentDef));
			AssertEquals(1, Collection.IndexOf(TestSubAttachmentDef));
			AssertEquals(2, Collection.IndexOf(NullAttachmentDef));
		}

		public void TestInsert()
		{
			Collection.Add(NullAttachmentDef);
			Collection.Insert(0, TestSubAttachmentDef);
			Collection.Insert(0, AttachmentDef);

			AssertEquals(AttachmentDef, Collection[0]);
			AssertEquals(TestSubAttachmentDef, Collection[1]);
			AssertEquals(NullAttachmentDef, Collection[2]);
		}

		public void TestRemove()
		{
			Collection.Add(AttachmentDef);
			Collection.Add(TestSubAttachmentDef);
			Collection.Add(NullAttachmentDef);

			AssertEquals(3, Collection.Count);

			Collection.Remove(AttachmentDef);
			Collection.Remove(TestSubAttachmentDef);
			Collection.Remove(NullAttachmentDef);

			AssertEquals(0, Collection.Count);
		}

		public void TestContains()
		{
			AssertEquals(false, Collection.Contains(AttachmentDef));
			AssertEquals(false, Collection.Contains(TestSubAttachmentDef));
			AssertEquals(false, Collection.Contains(NullAttachmentDef));

			Collection.Add(AttachmentDef);
			Collection.Add(TestSubAttachmentDef);
			Collection.Add(NullAttachmentDef);

			AssertEquals(true, Collection.Contains(AttachmentDef));
			AssertEquals(true, Collection.Contains(TestSubAttachmentDef));
			AssertEquals(true, Collection.Contains(NullAttachmentDef));
		}

		[ExpectNoExceptions]
		public void TestOnInsert()
		{
			Collection.OnInsertForTesting(0, AttachmentDef);
			Collection.OnInsertForTesting(0, TestSubAttachmentDef);
			Collection.OnInsertForTesting(0, NullAttachmentDef);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOnInsertInvalidType()
		{
			Collection.OnInsertForTesting(0, "");
		}

		[ExpectNoExceptions]
		public void TestOnRemove()
		{
			Collection.OnRemoveForTesting(0, AttachmentDef);
			Collection.OnRemoveForTesting(0, TestSubAttachmentDef);
			Collection.OnRemoveForTesting(0, NullAttachmentDef);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOnRemoveInvalidType()
		{
			Collection.OnRemoveForTesting(0, "");
		}

		[ExpectNoExceptions]
		public void TestOnSet()
		{
			Collection.OnSetForTesting(0, null, AttachmentDef);
			Collection.OnSetForTesting(0, null, TestSubAttachmentDef);
			Collection.OnSetForTesting(0, null, NullAttachmentDef);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOnSetInvalidType()
		{
			Collection.OnSetForTesting(0, null, "");
		}

		[ExpectNoExceptions]
		public void TestOnValidate()
		{
			Collection.OnValidateForTesting(AttachmentDef);
			Collection.OnValidateForTesting(TestSubAttachmentDef);
			Collection.OnValidateForTesting(NullAttachmentDef);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOnValidateInvalidType()
		{
			Collection.OnValidateForTesting("");
		}

		class AttachmentDefCollectionForTest : AttachmentDefCollection
		{
			public void OnInsertForTesting(int index, Object value) => OnInsert(index, value);
			public void OnRemoveForTesting(int index, Object value) => OnRemove(index, value);
			public void OnValidateForTesting(Object value) => OnValidate(value);
			public void OnSetForTesting(int index, Object oldValue, Object newValue) => OnSet(index, oldValue, newValue);
		}
	}
}
