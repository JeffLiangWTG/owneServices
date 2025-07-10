using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportPrevDocNMRNRefNumberValidationStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new Ucc6ExportPrevDocNMRNRefNumberValidationStrategy(null));
	}

	public void TestCheckReferenceNumber()
	{
		IPreviousDocumentRefNumberValidationStrategy strategy = new Ucc6ExportPrevDocNMRNRefNumberValidationStrategy(Factory);
		AssertExceptionThrown<ArgumentNullException>(() => strategy.CheckReferenceNumber(null));
	}
}
