using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class EDIMessageTypeDeciderTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestGetTypeForBinding()
	{
		NUnit.Framework.Assert.That(typeDecider.GetTypeForBinding(), Is.Null);
	}

	[ExpectNoExceptions]
	public void TestGetTypeForNew()
	{
		NUnit.Framework.Assert.That(typeDecider.GetTypeForNew(), Is.Null);
	}

	public void TestGetTypeForLoad()
	{
		var message = Factory.New<EDIMessage>();
		var row = ((INeedRow)message).Row;
		var typeDecider = new EDIMessageTypeDecider();

		AssertEquals(typeof(AEEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
	}

	protected override void SetUp()
	{
		base.SetUp();
		typeDecider = new EDIMessageTypeDecider();
	}

	EDIMessageTypeDecider typeDecider;
}
