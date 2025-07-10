using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryManualReleaseHandlerValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckReleaseCode()
	{
		var manuallRelease = new EntryManualReleaseHandler(Factory.New<CusEntryHeader>());
		manuallRelease.ReleaseCode = "123";
		AssertNoErrorContaining(manuallRelease.ReleaseCodeInfo, MandatoryValidation.MustBeEntered);

		manuallRelease.ReleaseCode = "";
		AssertHasErrorContaining(manuallRelease.ReleaseCodeInfo, MandatoryValidation.MustBeEntered);
	}

	public void TestCheckReleaseDate()
	{
		var manuallRelease = new EntryManualReleaseHandler(Factory.New<CusEntryHeader>());
		manuallRelease.ReleaseDate = ZDate.Today;
		AssertNoErrorContaining(manuallRelease.ReleaseDateInfo, MandatoryValidation.MustBeEntered);

		manuallRelease.ReleaseDate = ZDate.Empty;
		AssertHasErrorContaining(manuallRelease.ReleaseDateInfo, MandatoryValidation.MustBeEntered);
	}
}
