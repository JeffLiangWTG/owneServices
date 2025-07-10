using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

abstract class ClickableContextAbstractTest : TestCaseWithFactory
{
	public void TestCaption() => AssertEquals("Caption text", GetExpectedCaption(), GetClickableContext().Caption.EnglishText);

	public void TestName() => AssertEquals("Name text", GetExpectedName(), GetClickableContext().Name);

	protected abstract IClickableContext GetClickableContext();
	protected abstract string GetExpectedName();
	protected abstract string GetExpectedCaption();
}
