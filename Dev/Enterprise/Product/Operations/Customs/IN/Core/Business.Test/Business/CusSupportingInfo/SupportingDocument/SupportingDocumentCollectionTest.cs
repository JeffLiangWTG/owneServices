using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SupportingDocumentCollection))]
sealed class SupportingDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SupportingDocument>
{
	public void TestMaxCountValidation()
	{
		const int maxAllowed = 9999;
		const string expectedErrorMessage = "The maximum number of 9999 Supporting Documents has been exceeded.";
		var collection = Factory.New<CusEntryInstruction>().SupportingDocuments;

		for (var i = 0; i < maxAllowed; i++)
		{
			var newItem = collection.AddNew();
		}

		CombineAssertions(() =>
		{
			AssertEquals("Maximum allowed", false, collection.HasErrors());
			AssertNoRowMessageError(collection.Last(), expectedErrorMessage);

			var nextItem = collection.AddNew();
			AssertHasRowMessageError(nextItem, expectedErrorMessage);
		});
	}

	public void TestDefaultCSI_Type()
	{
		var supportingDocument = SupportingDocuments.AddNew();
		AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.SupportingDocument, supportingDocument.CSI_Type);
	}

	protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection() => SupportingDocuments;

	SupportingDocumentCollection SupportingDocuments => supportingDocuments ??= Factory.New<CusEntryInstruction>().SupportingDocuments;
	SupportingDocumentCollection supportingDocuments;
}
