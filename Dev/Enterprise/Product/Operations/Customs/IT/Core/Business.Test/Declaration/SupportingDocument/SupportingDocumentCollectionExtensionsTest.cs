using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class SupportingDocumentCollectionExtensionsTest : TestCaseWithFactory
{
	public void TestGetDeclarationOfIntentSupportingDocuments()
	{
		AssertExceptionThrown<ArgumentNullException>(() => (null as IEnumerable<SupportingDocument>).GetDeclarationOfIntentSupportingDocuments());

		var supportingDocumentCollection = new SupportingDocumentCollection(Factory.New<JobDeclaration>());
		var doiSupportingDocument = supportingDocumentCollection.AddNew();
		doiSupportingDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		supportingDocumentCollection.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.C100;
		supportingDocumentCollection.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.U164;
		supportingDocumentCollection.AddNew().CSI_Code = ZString.Empty;
		AssertContainsExactElementsInAnyOrder(new SupportingDocument[] { doiSupportingDocument }, supportingDocumentCollection.Cast<SupportingDocument>().GetDeclarationOfIntentSupportingDocuments());
	}
}
