using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

[TestsSubclassesOf(typeof(ITemporaryStoragePackedItemValidationDecider))]
public abstract class TemporaryStoragePackedItemValidationDeciderAbstractTest<T> : TestCaseWithFactory
	where T : class, ITemporaryStoragePackedItemValidationDecider, new()
{
	public void TestIsAPI_TariffMandatory()
	{
		var validationDecider = new T();
		AssertEquals(ExpectedIsAPI_TariffMandatory, validationDecider.IsAPI_TariffMandatory);
	}

	public void TestIsAPI_GoodsDescriptionMandatory()
	{
		var validationDecider = new T();
		AssertEquals(ExpectedIsAPI_GoodsDescriptionMandatory, validationDecider.IsAPI_GoodsDescriptionMandatory);
	}

	protected abstract bool ExpectedIsAPI_GoodsDescriptionMandatory { get; }

	protected abstract bool ExpectedIsAPI_TariffMandatory { get; }
}
