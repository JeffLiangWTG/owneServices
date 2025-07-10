using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public sealed class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var declarationMoq = Factory.NewMoq<JobDeclaration>();
			declarationMoq.Setup(dm => dm.JE_MessageType).Returns(JobMessageTypeList.Codes.Import);
			var declaration = declarationMoq.Object;

			var aadMessageError = "This code is only applicable to exports";
			var invoice = declaration.Invoices.AddNew();
			var invPreviousDocument = invoice.PreviousDocuments.AddNew();
			invPreviousDocument.CSI_Code = PreviousDocumentCodeListCDS.Codes.AdministrativeAccompanyingDocument;
			var invLinePreviousDocument = invoice.InvoiceLines.AddNew().PreviousDocuments.AddNew();
			invLinePreviousDocument.CSI_Code = PreviousDocumentCodeListCDS.Codes.AdministrativeAccompanyingDocument;

			CombineAssertions(() =>
			{
				AssertHasMessageError(invPreviousDocument.CSI_CodeInfo, aadMessageError);
				AssertHasMessageError(invLinePreviousDocument.CSI_CodeInfo, aadMessageError);
			});

			declarationMoq.Setup(dm => dm.JE_MessageType).Returns(JobMessageTypeList.Codes.Export);
			declaration.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoMessageError(invPreviousDocument.CSI_CodeInfo, aadMessageError);
				AssertNoMessageError(invLinePreviousDocument.CSI_CodeInfo, aadMessageError);
			});

			invLinePreviousDocument.CSI_Code = PreviousDocumentCodeListCDS.Codes.DeclarationUniqueConsignmentReferenceDucr;
			AssertHasMessageError(invLinePreviousDocument.CSI_CodeInfo, "This code is only applicable at header level for exports");
			invLinePreviousDocument.CSI_Code = PreviousDocumentCodeListCDS.Codes.MasterUniqueConsignmentReferenceMucr;
			AssertHasMessageError(invLinePreviousDocument.CSI_CodeInfo, "This code is only applicable at header level");
		}
	}
}
