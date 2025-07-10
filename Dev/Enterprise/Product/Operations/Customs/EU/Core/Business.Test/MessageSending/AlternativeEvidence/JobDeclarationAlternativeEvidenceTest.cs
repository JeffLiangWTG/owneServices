using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing;

[TestedType(typeof(JobDeclarationAlternativeEvidence))]
sealed class JobDeclarationAlternativeEvidenceTest : NonPersistentBusinessObjectTestCase
{
	public void TestEvidenceType() => CombineAssertions(() =>
	{
		var alternativeEvidence = CreateJobDeclarationAlternativeEvidence();
		AssertEquals("EvidenceType is empty", ZString.Empty, alternativeEvidence.EvidenceType);

		alternativeEvidence.EvidenceType = "A";
		AssertEquals("EvidenceType is A", "A", alternativeEvidence.EvidenceType);
	});

	public void TestEvidenceTypeMaxLength() => AssertEquals(2, CreateJobDeclarationAlternativeEvidence().EvidenceTypeInfo.MaxLength);

	public void TestEvidenceTypeCaption() => AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(CreateJobDeclarationAlternativeEvidence().EvidenceTypeInfo).Caption);

	public void TestDocumentType() => CombineAssertions(() =>
	{
		var alternativeEvidence = CreateJobDeclarationAlternativeEvidence();
		AssertEquals("DocumentType is empty", ZString.Empty, alternativeEvidence.DocType);

		alternativeEvidence.DocType = "A";
		AssertEquals("DocumentType is A", "A", alternativeEvidence.DocType);
	});

	public void TestDocumentTypeMaxLen() => AssertEquals(7, CreateJobDeclarationAlternativeEvidence().DocTypeInfo.MaxLength);

	public void TestDocumentTypeCaption() => AssertEquals("Doc. Type", DataBoundResourceStrings.GetDataForProperty(CreateJobDeclarationAlternativeEvidence().DocTypeInfo).Caption);

	public void TestDocumentTypeReadOnly() => AssertEquals(false, CreateJobDeclarationAlternativeEvidence().DocTypeInfo.ReadOnly);

	public void TestReference() => CombineAssertions(() =>
	{
		var alternativeEvidence = CreateJobDeclarationAlternativeEvidence();
		AssertEquals("Reference is empty", ZString.Empty, alternativeEvidence.Reference);

		alternativeEvidence.Reference = "A";
		AssertEquals("Reference is A", "A", alternativeEvidence.Reference);
	});

	public void TestReferenceMaxLength() => AssertEquals(70, CreateJobDeclarationAlternativeEvidence().ReferenceInfo.MaxLength);

	public void TestReferenceCaption() => AssertEquals("Reference Number", DataBoundResourceStrings.GetDataForProperty(CreateJobDeclarationAlternativeEvidence().ReferenceInfo).Caption);

	public void TestReferenceReadOnly() => AssertEquals(false, CreateJobDeclarationAlternativeEvidence().ReferenceInfo.ReadOnly);

	public void TestValidation() => AssertType<JobDeclarationAlternativeEvidenceValidation>(CreateJobDeclarationAlternativeEvidence().Validation);

	protected override BusinessObject GetNewBusinessObject() => CreateJobDeclarationAlternativeEvidence();

	JobDeclarationAlternativeEvidence CreateJobDeclarationAlternativeEvidence() => new(Factory);
}
