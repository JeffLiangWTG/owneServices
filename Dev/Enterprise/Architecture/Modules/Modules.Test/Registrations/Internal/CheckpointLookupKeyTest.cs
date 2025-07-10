using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class CheckpointLookupKeyTest : TestCase
	{
		public void TestCodeEquals()
		{
			CheckpointLookupKey key = new CheckpointLookupKey("xxx");
			AssertEquals("CodeEquals", true, key.CodeEquals("xxx"));
			AssertEquals("CodeEquals", true, key.CodeEquals("XXX"));
			AssertEquals("CodeEquals", false, key.CodeEquals("XXy"));
			AssertEquals("CodeEquals", true, CheckpointLookupKey.Empty.CodeEquals(null));
			AssertEquals("CodeEquals", true, CheckpointLookupKey.Empty.CodeEquals(""));
			AssertEquals("CodeEquals", false, CheckpointLookupKey.Empty.CodeEquals(" "));
		}

		public void TestConstructors()
		{
			CheckpointLookupKey key = new CheckpointLookupKey("Test");
			AssertEquals("Code", "Test", key.Code);
			AssertEquals("ItemGuid", Guid.Empty, key.ItemGuid);

			Guid itemGuid = Guid.NewGuid();
			key = new CheckpointLookupKey("TEST2", itemGuid);
			AssertEquals("Code", "TEST2", key.Code);
			AssertEquals("ItemGuid", itemGuid, key.ItemGuid);
		}

		public void TestEmptyKey()
		{
			AssertEquals("Code", "", CheckpointLookupKey.Empty.Code);
			AssertEquals("ItemGuid", Guid.Empty, CheckpointLookupKey.Empty.ItemGuid);
		}

		public void TestEquality_Empty()
		{
			CheckpointLookupKey key1 = new CheckpointLookupKey(null, Guid.Empty);
			CheckpointLookupKey key2 = new CheckpointLookupKey("", Guid.Empty);
			AssertEquals("Equals(CheckpointLookupKey)", true, ((IEquatable<CheckpointLookupKey>)key1).Equals(key2));
			AssertEquals("Equals(object)", true, ((object)key1).Equals(key2));
			AssertEquals("== operator", true, key1 == key2);
			AssertEquals("!= operator", false, key1 != key2);
			AssertEquals("GetHashCode()", key1.GetHashCode(), key2.GetHashCode());
		}

		public void TestEquality_NotEmpty()
		{
			CheckpointLookupKey key1 = new CheckpointLookupKey("alpha", new Guid("1234567890abcdef1234567890abcdef"));
			CheckpointLookupKey key2 = new CheckpointLookupKey("ALPHA", new Guid("1234567890abcdef1234567890abcdef"));
			CheckpointLookupKey key3 = new CheckpointLookupKey("alpha", new Guid("00000000000000000000000000000000"));
			CheckpointLookupKey key4 = new CheckpointLookupKey("bravo", new Guid("1234567890abcdef1234567890abcdef"));
			CheckpointLookupKey key5 = new CheckpointLookupKey("charlie", new Guid("11111111111111111111111111111111"));

			AssertEquals("Code and ItemGuid are the same.", true, ((IEquatable<CheckpointLookupKey>)key1).Equals(key2));
			AssertEquals("Code and ItemGuid are the same.", true, ((object)key1).Equals(key2));
			AssertEquals("Code and ItemGuid are the same.", true, key1 == key2);
			AssertEquals("Code and ItemGuid are the same.", false, key1 != key2);
			AssertEquals("Objects are equal - hashcodes should be equal.", key1.GetHashCode(), key2.GetHashCode());

			AssertEquals("ItemGuids differ.", false, ((IEquatable<CheckpointLookupKey>)key1).Equals(key3));
			AssertEquals("ItemGuids differ.", false, ((object)key1).Equals(key3));
			AssertEquals("ItemGuids differ.", true, key1 != key3);
			AssertEquals("ItemGuids differ.", false, key1 == key3);

			AssertEquals("Codes differ.", false, ((IEquatable<CheckpointLookupKey>)key1).Equals(key4));
			AssertEquals("Codes differ.", false, ((object)key1).Equals(key4));
			AssertEquals("Codes differ.", true, key1 != key4);
			AssertEquals("Codes differ.", false, key1 == key4);

			AssertEquals("Codes and ItemGuids differ.", false, ((IEquatable<CheckpointLookupKey>)key1).Equals(key5));
			AssertEquals("Codes and ItemGuids differ.", false, ((object)key1).Equals(key5));
			AssertEquals("Codes and ItemGuids differ.", true, key1 != key5);
			AssertEquals("Codes and ItemGuids differ.", false, key1 == key5);

			AssertEquals("Types differ.", false, key1.Equals(""));
		}

		public void TestIsEmpty()
		{
			AssertEquals("IsEmpty", false, new CheckpointLookupKey("x", Guid.Empty).IsEmpty);
			AssertEquals("IsEmpty", false, new CheckpointLookupKey(null, Guid.NewGuid()).IsEmpty);
			AssertEquals("IsEmpty", false, new CheckpointLookupKey("x", Guid.NewGuid()).IsEmpty);
			AssertEquals("IsEmpty", true, new CheckpointLookupKey(null, Guid.Empty).IsEmpty);
			AssertEquals("IsEmpty", true, new CheckpointLookupKey("", Guid.Empty).IsEmpty);
		}
	}
}
