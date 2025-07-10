using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RecipientDefTest : TestCase
	{
		public void TestIgnoreSystemEmailDestinationOverride()
		{
			var recipient = new RecipientDef("aaa");
			Assert("should be false.", !recipient.IsForSystemCommunication);
			Assert("should be false.", !recipient.IgnoreSystemEmailDestinationOverride);

			recipient.IgnoreSystemEmailDestinationOverride = true;
			Assert("Should return false as IsForSystemCommunication is false. ", !recipient.IgnoreSystemEmailDestinationOverride);

			recipient.IsForSystemCommunication = true;
			Assert("Should return true as IsForSystemCommunication is true. ", recipient.IgnoreSystemEmailDestinationOverride);
		}

		public void TestConstructor()
		{
			RecipientDef def1 = new RecipientDef("aaa");
			AssertNotNull(def1);
			AssertEquals(def1.Email, "aaa");
			Assert(!def1.IsForSystemCommunication);

			RecipientDef def2 = new RecipientDef("bbb", true);
			AssertNotNull(def2);
			AssertEquals(def2.Email, "bbb");
			Assert(def2.IsForSystemCommunication);

			RecipientDef def3 = new RecipientDef("ccc");
			AssertNotNull(def3);
			AssertEquals(def3.Email, "ccc");
			Assert(!def3.IsForSystemCommunication);
		}

		public void TestEquals()
		{
			RecipientDef def = new RecipientDef("aaa");
			AssertEquals("aaa", def);
			AssertEquals(new string(new char[] { 'a', 'a', 'a' }), def);
			AssertEquals(new ZString("aaa"), def);
		}
	}
}
