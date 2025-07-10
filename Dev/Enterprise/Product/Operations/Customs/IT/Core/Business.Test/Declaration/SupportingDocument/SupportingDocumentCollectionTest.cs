using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocumentCollection))]
sealed class SupportingDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.SupportingDocumentCollectionTest
{
	protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetCusSupportingInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new SupportingDocumentCollection(declaration);
	}

	protected override void AssertDateOfIssue(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument)
	{
		var itSupporintDocument = (SupportingDocument)supportingDocument;
		AssertEquals("CSI_DateOfIssue", new ZDateTime(ZDateTime.Today.Year, 01, 01), itSupporintDocument.CSI_DateOfIssue);
		AssertEquals("CSI_YearOfIssue", ZDateTime.Today.Year.ToString(), itSupporintDocument.CSI_YearOfIssue);
	}

	public void TestGetDeclarationOfIntentSupportingDocuments()
	{
		var supportingDocumentCollection = (SupportingDocumentCollection)GetCusSupportingInfoCollection();
		var doiSupportingDocument = supportingDocumentCollection.AddNew();
		doiSupportingDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		supportingDocumentCollection.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.C100;
		supportingDocumentCollection.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.U164;
		supportingDocumentCollection.AddNew().CSI_Code = ZString.Empty;
		AssertContainsExactElementsInAnyOrder(new SupportingDocument[] { doiSupportingDocument }, supportingDocumentCollection.GetDeclarationOfIntentSupportingDocuments());
	}

	public void TestGetFirstSupportingDocumentOrNewIfNotExists()
	{
		var supportingDocumentCollection = (SupportingDocumentCollection)GetCusSupportingInfoCollection();

		AssertEquals("[PRE-CONDITION] Collection count", 0, supportingDocumentCollection.Count);

		var supportingDocument = supportingDocumentCollection.GetFirstSupportingDocumentOrAddNewIfNotExists("XXX");

		AssertEquals("Collection count", 1, supportingDocumentCollection.Count);
		AssertSame("Should be the same object", supportingDocument, supportingDocumentCollection.Cast<SupportingDocument>().Single());

		var anotherSupportingDocument = supportingDocumentCollection.GetFirstSupportingDocumentOrAddNewIfNotExists("XXX");
		AssertEquals("Collection count", 1, supportingDocumentCollection.Count);
		AssertSame("Should be the same object", anotherSupportingDocument, supportingDocumentCollection.Cast<SupportingDocument>().Single());
	}
}
