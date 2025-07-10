using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class SupportingDocumentWrapperTest : DataProviderTestCase<SupportingDocumentWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new SupportingDocumentWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(1, wrapper.SequenceNumeric);
	}

	public void TestExpirationDateTime()
	{
		supportingDocument.CSI_DateOfExpiry = ZDateTime.BrettsBirthday;
		AssertEquals(CargoWise.Types.ZDateTime.BrettsBirthday.ToString("yyyyMMdd"), wrapper.ExpirationDateTime);
	}

	public void TestCCQualifierCode()
	{
		AssertNull(wrapper.CCQualifierCode);
	}

	public void TestLineNumericValue()
	{
		supportingDocument.CSI_ItemNumber = 1;
		AssertEquals(1, wrapper.LineNumericValue);
		supportingDocument.CSI_ItemNumber = 0;
		AssertNull(wrapper.LineNumericValue);
	}

	public void TestSubmitter()
	{
		supportingDocument.CSI_AdditionalDescription = "SUPREF21";
		AssertEquals("SUPREF21", wrapper.Submitter);
	}

	public void TestId()
	{
		supportingDocument.CSI_ReferenceNumber = "SUPREF11";
		AssertEquals("SUPREF11", wrapper.Id);
	}

	public void TestCode()
	{
		supportingDocument.CSI_Code = "SPCD1";
		AssertEquals("SPCD1", wrapper.Code);
	}

	public void TestWriteOff()
	{
		CombineAssertions(() =>
		{
			AssertNull("When quantity and value are both empty, we shouldn't map this section", wrapper.WriteOff);
			supportingDocument.CSI_Quantity = 5;
			AssertNotNull(wrapper.WriteOff);
			AssertType<WriteOffWrapper>(wrapper.WriteOff);
		});
	}

	protected override void SetUp()
	{
		supportingDocument = Factory.New<SupportingDocument>();
		wrapper = new SupportingDocumentWrapper(supportingDocument, 1);
	}
	SupportingDocument supportingDocument;
	SupportingDocumentWrapper wrapper;

	protected override SupportingDocumentWrapper GetProvider() => wrapper;
}
