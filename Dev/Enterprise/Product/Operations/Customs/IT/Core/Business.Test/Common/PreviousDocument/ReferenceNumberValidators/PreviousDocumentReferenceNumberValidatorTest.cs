using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PreviousDocumentReferenceNumberValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		AssertExceptionThrown<ArgumentNullException>("previousDocument cannot be null", () => new PreviousDocumentReferenceNumberValidator(previousDocument: null, previousDocumentSettings: null));
		AssertExceptionThrown<ArgumentNullException>("previousDocumentSettings cannot be null", () => new PreviousDocumentReferenceNumberValidator(previousDocument, previousDocumentSettings: null));
		AssertNoExceptionThrown(() => new PreviousDocumentReferenceNumberValidator(previousDocument, new PreviousDocumentFieldsInfo()));
	}
}
