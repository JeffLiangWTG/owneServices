using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentPackIdTest : TestCase
	{
		ZGuid commandPK;
		ZGuid supporterPK;

		protected override void SetUp()
		{
			base.SetUp();
			commandPK = ZGuid.NewZGuid();
			supporterPK = ZGuid.NewZGuid();
		}

		public void TestConstruction()
		{
			DocumentPackId id = new DocumentPackId(commandPK);
			AssertEquals("CommandPK", commandPK, id.CommandPK);
			AssertEquals("SupporterPK", ZGuid.Empty, id.SupporterPK);

			id = new DocumentPackId(commandPK, supporterPK);
			AssertEquals("CommandPK", commandPK, id.CommandPK);
			AssertEquals("SupporterPK", supporterPK, id.SupporterPK);
		}

		public void TestEquals()
		{
			DocumentPackId id = new DocumentPackId(commandPK, supporterPK);
			AssertEquals("Equals()", true, id.Equals(new DocumentPackId(commandPK, supporterPK)));
			AssertEquals("Equals()", false, id.Equals(new DocumentPackId(commandPK, ZGuid.Empty)));
			AssertEquals("Equals()", false, id.Equals(new DocumentPackId(ZGuid.Empty, commandPK)));
			AssertEquals("Equals()", false, id.Equals(null));
			AssertEquals("Equals()", false, id.Equals(string.Empty));
			AssertEquals("Equals()", false, id.Equals(0));
		}

		public void TestGetHashCode()
		{
			AssertEquals("GetHashCode()", new DocumentPackId(commandPK, supporterPK).GetHashCode(), new DocumentPackId(commandPK, supporterPK).GetHashCode());
			AssertEquals("GetHashCode()", new DocumentPackId(ZGuid.Empty, ZGuid.Empty).GetHashCode(), new DocumentPackId(ZGuid.Empty, ZGuid.Empty).GetHashCode());
		}
	}
}
