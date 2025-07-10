using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageAmendmentHandler))]
sealed class TemporaryStorageAmendmentHandlerTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		var handler = new TemporaryStorageAmendmentHandler(Factory);
		AssertSame(Factory, handler.Factory);
	}

	public void TestHumanReadableName()
	{
		var handler = (TemporaryStorageAmendmentHandler)GetNewBusinessObject();
		AssertEquals("Temporary Storage Amendment", handler.HumanReadableName);
	}

	public void TestShowTotalEntryLines()
	{
		var handler = (TemporaryStorageAmendmentHandler)GetNewBusinessObject();
		Assert("ShowTotalEntryLines should be false", !handler.ShowTotalEntryLines);
	}

	protected override BusinessObject GetNewBusinessObject() => new TemporaryStorageAmendmentHandler(Factory);
}
