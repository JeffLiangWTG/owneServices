using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportPrevDocN337RefNumberValidationStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new Ucc6ExportPrevDocN337RefNumberValidationStrategy(factory: null));
		AssertNoExceptionThrown(() => new Ucc6ExportPrevDocN337RefNumberValidationStrategy(Factory));
	}

	public void TestCheckReferenceNumber()
	{
		IPreviousDocumentRefNumberValidationStrategy strategy = new Ucc6ExportPrevDocN337RefNumberValidationStrategy(Factory);
		AssertExceptionThrown<ArgumentNullException>(() => strategy.CheckReferenceNumber(null));
	}
}
