using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing;

[TestedType(typeof(CC044CSupportingDocumentWrapper))]
sealed class CC044CSupportingDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CSupportingDocumentWrapper>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CSupportingDocumentWrapper(null));

	public void TestReferenceNumber() => CombineAssertions(() =>
	{
		var info = Factory.New<NctsSupportingDocument>();
		info.CSI_ReferenceNumber = "reference";
		info.CSI_Status = NctsUnloadedStateList.Codes.MIS;
		var wrapper = CC044CSupportingDocumentWrapper.New(info);

		AssertNullOrEmpty("ReferenceNumber should be empty hen CSI_status is not NEW", wrapper.ReferenceNumber);
		info.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		wrapper = CC044CSupportingDocumentWrapper.New(info);
		AssertEquals("ReferenceNumber should be equal to CSI_ReferenceNumber when CSI_status is NEW", "reference", wrapper.ReferenceNumber);
	});

	public void TestType() => CombineAssertions(() =>
	{
		var info = Factory.New<NctsSupportingDocument>();
		info.CSI_Code = "Code";
		info.CSI_Status = NctsUnloadedStateList.Codes.MIS;
		var wrapper = CC044CSupportingDocumentWrapper.New(info);

		AssertNullOrEmpty("Type should be empty hen CSI_status is not NEW", wrapper.Type);
		info.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		wrapper = CC044CSupportingDocumentWrapper.New(info);
		AssertEquals("Type should be equal to CSI_Code when CSI_status is NEW", "Code", wrapper.Type);
	});

	public void TestComplementOfInformation()
	{
		var info = Factory.New<NctsSupportingDocument>();
		info.CSI_ReferenceNumber2 = "SupDocRef";
		info.CSI_Status = NctsUnloadedStateList.Codes.MIS;
		var wrapper =  CC044CSupportingDocumentWrapper.New(info);

		info.CSI_ReferenceNumber2 = "SupDocRef";
		AssertNullOrEmpty("ComplementOfInformation should be empty hen CSI_status is not NEW", wrapper.ComplementOfInformation);
		info.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		wrapper = CC044CSupportingDocumentWrapper.New(info);
		AssertEquals("ComplementOfInformation should be equal to CSI_ReferenceNumber2 when CSI_status is NEW", "SupDocRef", wrapper.ComplementOfInformation);
	}

	protected override CC044CSupportingDocumentWrapper GetProvider()
	{
		var info = Factory.New<NctsSupportingDocument>();
		info.CSI_Code = "Code";
		info.CSI_ReferenceNumber = "ReferenceNumber";
		info.CSI_ItemNumber = 99;
		info.CSI_ReferenceNumber2 = "ReferenceNumber2";
		return CC044CSupportingDocumentWrapper.New(info);
	}
}
