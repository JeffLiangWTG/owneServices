using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing;

[TestedType(typeof(CC044CDocumentWrapper))]
sealed class CC044CDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CDocumentWrapper>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CDocumentWrapper(null));

	public void TestReferenceNumber() => CombineAssertions(() =>
	{
		var info = Factory.New<CusSupportingInfo>();
		info.CSI_ReferenceNumber = "reference";
		var wrapper = CC044CDocumentWrapper.New(info);
		AssertNullOrEmpty(wrapper.ReferenceNumber);
		info.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("reference", wrapper.ReferenceNumber);
	});

	public void TestType() => CombineAssertions(() =>
	{	var info = Factory.New<CusSupportingInfo>();
		info.CSI_Code = "CODE";
		var wrapper = CC044CDocumentWrapper.New(info);
		AssertNullOrEmpty(wrapper.Type);
		info.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("CODE", wrapper.Type);
	});

	protected override CC044CDocumentWrapper GetProvider()
	{
		var info = Factory.New<CusSupportingInfo>();
		return CC044CDocumentWrapper.New(info);
	}
}
