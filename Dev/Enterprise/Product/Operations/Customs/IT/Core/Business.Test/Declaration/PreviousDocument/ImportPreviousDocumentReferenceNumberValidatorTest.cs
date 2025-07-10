using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ImportPreviousDocumentReferenceNumberValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		AssertExceptionThrown<ArgumentNullException>("previousDocument cannot be null", () => new ImportPreviousDocumentReferenceNumberValidator(previousDocument: null, previousDocumentSettings: null));
		AssertExceptionThrown<ArgumentNullException>("previousDocumentSettings cannot be null", () => new ImportPreviousDocumentReferenceNumberValidator(previousDocument, previousDocumentSettings: null));
		AssertNoExceptionThrown(() => new ImportPreviousDocumentReferenceNumberValidator(previousDocument, new PreviousDocumentFieldsInfo()));
	}
}
