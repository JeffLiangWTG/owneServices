using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.Business.Testing;

class PlausiValidationTest : TestCaseWithFactory
{
	public void TestPlausiValidationNew()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = ZString.Empty;
			AssertType<PlausiValidation>($"MessageType={declaration.JE_MessageType}", PlausiValidation.New(declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportPlausiValidation>($"MessageType={declaration.JE_MessageType}", PlausiValidation.New(declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportPlausiValidation>($"MessageType={declaration.JE_MessageType}", PlausiValidation.New(declaration));

			declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertType<ExportDeclarationActivationPlausiValidation>($"MessageType={declaration.JE_MessageType}", PlausiValidation.New(declaration));
		});
	}

	public void TestPlausiValidationNew_SupportingDocument()
	{
		var supportingDocumentParent = new SupportingDocumentParentForTesting();
		var declaration = Factory.New<JobDeclaration>();
		supportingDocumentParent.JobDeclaration = declaration;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = ZString.Empty;
			AssertType<PlausiValidation>(PlausiValidation.New(supportingDocumentParent));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportPlausiValidation>(PlausiValidation.New(supportingDocumentParent));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportPlausiValidation>(PlausiValidation.New(supportingDocumentParent));
			declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertType<ExportDeclarationActivationPlausiValidation>(PlausiValidation.New(supportingDocumentParent));
		});
	}

	class SupportingDocumentParentForTesting : ICusSupportingInfoParent
	{
		public ZDateTime DateOfValuation { get; set; }

		public SupportingDocumentCollection SupportingDocuments { get; set; }

		public BusinessObjectFactory Factory { get; set; }

		public bool IsGSPCertificateRequired { get; set; }

		public IEnumerable<SupportingDocument> SupportingDocumentsIncludingInherited { get; set; }

		public JobDeclaration JobDeclaration { get; set; }

		public ZGuid PK => ZGuid.NewZGuid();

		public ZDateTime EffectiveAssessmentDate => DateOfValuation;

		public HugeSequenceNumberGenerator AdditionalInformationLineNumberGenerator => null;

		public void MarkAsNeedingValidation() { }

		public void ValidateNonTradingGoods() { }
	}
}
