using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Declaration.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentProcedureValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
		var previousDocument = jobComInvoiceLine.PreviousDocuments.AddNew();
		AssertExceptionThrown<ArgumentNullException>(() => new PreviousDocumentProcedureValidator(null));
		AssertNoExceptionThrown(() => new PreviousDocumentProcedureValidator(previousDocument));
	}

	public void TestMoreThanOnePaAndOneOrMoreRpDocuments()
	{
		PreviousDocumentCollection previousDocuments = (PreviousDocumentCollection)(previousDocument.Parent is EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentsProvider previousDocumentsProvider ? previousDocumentsProvider.PreviousDocuments : null);

		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertNoWarningContaining(previousDocument.CSI_ProcedureInfo, ValidationCaptions.PreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);

		var previousDocumentRp = previousDocuments.AddNew();
		previousDocumentRp.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione;
		AssertNoWarningContaining(previousDocumentRp.CSI_ProcedureInfo, ValidationCaptions.PreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);

		var previousDocumentPa = previousDocuments.AddNew();
		previousDocumentPa.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertHasWarningContaining(previousDocumentPa.CSI_ProcedureInfo, ValidationCaptions.PreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);

		var previousDocumentRp2 = previousDocuments.AddNew();
		previousDocumentRp2.CSI_Procedure = PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea;
		AssertHasWarningContaining(previousDocumentRp2.CSI_ProcedureInfo, ValidationCaptions.PreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		previousDocument = Factory.New<PreviousDocumentForTest>();
		declaration.Invoices.AddNew().PreviousDocuments.Add(previousDocument);
	}

	JobDeclaration declaration;
	PreviousDocumentForTest previousDocument;
}
