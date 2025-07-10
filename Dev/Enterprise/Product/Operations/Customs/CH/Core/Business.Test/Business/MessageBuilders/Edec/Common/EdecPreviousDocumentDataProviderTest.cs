using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecPreviousDocumentDataProviderTest : TestCaseWithFactory
{
	public void TestConstructorNullArgument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNull("Null", EdecPreviousDocumentDataProvider.New(null));
			AssertNotNull("Not Null", EdecPreviousDocumentDataProvider.New(previousDocument));
		});
	}

	public void TestProvider()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
		previousDocument.CSI_Code = "1";
		previousDocument.CSI_ReferenceNumber = "123456789";
		previousDocument.CSI_Description = "Description";

		var messageBuilder = EdecPreviousDocumentDataProvider.New(previousDocument);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(messageBuilder.PreviousDocumentType), "1", messageBuilder.PreviousDocumentType);
			AssertEquals(nameof(messageBuilder.PreviousDocumentReference), "123456789", messageBuilder.PreviousDocumentReference);
			AssertEquals(nameof(messageBuilder.AdditionalInformation), "Description", messageBuilder.AdditionalInformation);
		});
	}
}
