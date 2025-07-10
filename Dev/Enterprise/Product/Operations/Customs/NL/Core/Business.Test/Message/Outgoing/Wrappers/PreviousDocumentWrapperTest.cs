using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class PreviousDocumentWrapperTest : DataProviderTestCase<PreviousDocumentWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new PreviousDocumentWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestId()
	{
		previousDocument.CSI_ReferenceNumber = "PRV-321";
		AssertEquals("PRV-321", wrapper.Id);
	}

	public void TestTypeCode()
	{
		previousDocument.CSI_Code = "IMA1";
		AssertEquals("IMA1", wrapper.TypeCode);
	}

	public void TestLineNumeric()
	{
		previousDocument.CSI_LineNo = 1;
		AssertEquals(1, wrapper.LineNumeric);
	}

	public void TestLineNumeric_NotMapped()
	{
		previousDocument.Declaration.JE_MessageType = "EXP";
		previousDocument.CSI_LineNo = 0;
		previousDocument.CSI_Code = "XXX";
		AssertNull(wrapper.LineNumeric);
	}

	public void TestCcQualifierCode()
	{
		AssertNull(wrapper.CcQualifierCode);
	}

	public void TestWriteOff()
	{
		AssertNull(wrapper.WriteOff);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		previousDocument = declaration.PreviousDocuments.AddNew();
		wrapper = new PreviousDocumentWrapper(previousDocument, 1);
	}
	PreviousDocumentWrapper wrapper;
	PreviousDocument previousDocument;

	protected override PreviousDocumentWrapper GetProvider() => wrapper;
}
