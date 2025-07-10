using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationSadDocumentSupporterValidationTest : BusinessObjectValidationTestCase
{
	public void TestAutoValidationType()
	{
		var validation = GetNewDeclarationSadDocumentSupporterValidation();

		AssertEquals("AutoValidationType", typeof(JobDeclarationSadDocumentSupporterValidation), validation.AutoValidationType);
	}

	public void TestValidateAll()
	{
		var validation = GetNewDeclarationSadDocumentSupporterValidation();

		declarationSadDocumentSupporter.LayoutStyle = ZString.Empty;
		declarationSadDocumentSupporter.BGMReferenceToPrint = ZString.Empty;
		validation.ValidateAll();
		AssertHasErrorContaining(declarationSadDocumentSupporter.LayoutStyleInfo, MandatoryValidation.MustBeEntered);
		AssertHasErrorContaining(declarationSadDocumentSupporter.BGMReferenceToPrintInfo, MandatoryValidation.MustBeEntered);

		declarationSadDocumentSupporter.LayoutStyle = "1";
		declarationSadDocumentSupporter.BGMReferenceToPrint = "BGM";
		validation.ValidateAll();
		AssertNoErrorContaining(declarationSadDocumentSupporter.LayoutStyleInfo, MandatoryValidation.MustBeEntered);
		AssertNoErrorContaining(declarationSadDocumentSupporter.BGMReferenceToPrintInfo, MandatoryValidation.MustBeEntered);
	}

	public void TestValidateLayoutStyle()
	{
		declarationSadDocumentSupporter.LayoutStyle = ZString.Empty;
		AssertHasErrorContaining(declarationSadDocumentSupporter.LayoutStyleInfo, MandatoryValidation.MustBeEntered);

		declarationSadDocumentSupporter.LayoutStyle = "X";
		AssertNoErrorContaining(declarationSadDocumentSupporter.LayoutStyleInfo, MandatoryValidation.MustBeEntered);
		AssertHasErrorContaining(declarationSadDocumentSupporter.LayoutStyleInfo, ListValidation.InvalidCodeError);

		declarationSadDocumentSupporter.LayoutStyle = "6";
		AssertNoErrorContaining(declarationSadDocumentSupporter.LayoutStyleInfo, ListValidation.InvalidCodeError);
	}

	public void TestValidateBGMReferenceToPrint()
	{
		declarationSadDocumentSupporter.BGMReferenceToPrint = ZString.Empty;
		AssertHasErrorContaining(declarationSadDocumentSupporter.BGMReferenceToPrintInfo, MandatoryValidation.MustBeEntered);

		declarationSadDocumentSupporter.BGMReferenceToPrint = "BGM";
		AssertNoErrorContaining(declarationSadDocumentSupporter.BGMReferenceToPrintInfo, MandatoryValidation.MustBeEntered);
		AssertHasErrorContaining(declarationSadDocumentSupporter.BGMReferenceToPrintInfo, ListValidation.InvalidCodeError);

		declaration.CustomsEntryHeaders.AddNew().CH_BGMReference = "BGM1";

		declarationSadDocumentSupporter.BGMReferenceToPrint = "BGM1";
		AssertNoErrorContaining(declarationSadDocumentSupporter.BGMReferenceToPrintInfo, ListValidation.InvalidCodeError);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declarationSadDocumentSupporter = new JobDeclarationSadDocumentSupporter(declaration);
	}
	JobDeclaration declaration;
	JobDeclarationSadDocumentSupporter declarationSadDocumentSupporter;

	JobDeclarationSadDocumentSupporterValidation GetNewDeclarationSadDocumentSupporterValidation() => new JobDeclarationSadDocumentSupporterValidation(declarationSadDocumentSupporter);
}
