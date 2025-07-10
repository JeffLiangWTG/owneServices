using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ExportLineSadFieldDescriptionsProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Factory parameter cannot be null when creating ExportLineSadFieldDescriptionsProvider", () => new ExportLineSadFieldDescriptionsProvider(null));
	}

	public void TestGetDescriptionBySequenceNumber()
	{
		var descriptionsProviderForLine = new ExportLineSadFieldDescriptionsProvider(Factory);
		AssertEquals("When Sequence Number is 55, Description should be", "Net Mass", descriptionsProviderForLine.GetDescriptionBySequenceNumber("55"));
		AssertEquals("When Sequence Number is Empty, Description should be", "", descriptionsProviderForLine.GetDescriptionBySequenceNumber(""));
		AssertEquals("When Sequence Number is null, Description should be", "", descriptionsProviderForLine.GetDescriptionBySequenceNumber(null));
	}

	public void TestGetDescriptionByReferenceCode()
	{
		var descriptionsProviderForLine = new ExportLineSadFieldDescriptionsProvider(Factory);
		AssertEquals("When Reference Code is 38, Description should be", "Net Mass", descriptionsProviderForLine.GetDescriptionByReferenceCode("38"));
		AssertEquals("When Reference Code is Empty, Description should be", "", descriptionsProviderForLine.GetDescriptionByReferenceCode(""));
		AssertEquals("When Reference Code is null, Description should be", "", descriptionsProviderForLine.GetDescriptionByReferenceCode(null));
	}

	public void TestGetReferenceCodeBySequenceNumber()
	{
		var descriptionsProviderForLine = new ExportLineSadFieldDescriptionsProvider(Factory);
		AssertEquals("When Sequence Number is 55, reference should be", "38", descriptionsProviderForLine.GetReferenceCodeBySequenceNumber("55"));
		AssertEquals("When Sequence Number is Empty, reference should be", "", descriptionsProviderForLine.GetReferenceCodeBySequenceNumber(""));
		AssertEquals("When Sequence Number is null, reference should be", "", descriptionsProviderForLine.GetReferenceCodeBySequenceNumber(null));
	}

	public void TestGetProgressiveNumberByReferenceCode()
	{
		var descriptionsProviderForLine = new ExportLineSadFieldDescriptionsProvider(Factory);
		AssertEquals("When Reference Code is 38, Description should be", "55", descriptionsProviderForLine.GetSequenceNumberByReferenceCode("38"));
		AssertEquals("When Reference Code is Empty Description should be", "", descriptionsProviderForLine.GetSequenceNumberByReferenceCode(""));
		AssertEquals("When Reference Code is null, Description should be", "", descriptionsProviderForLine.GetSequenceNumberByReferenceCode(null));
	}
}
