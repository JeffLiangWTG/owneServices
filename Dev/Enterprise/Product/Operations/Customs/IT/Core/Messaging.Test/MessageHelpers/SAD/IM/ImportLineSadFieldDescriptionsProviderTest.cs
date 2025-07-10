using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ImportLineSadFieldDescriptionsProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("ImportLineSadFieldDescriptionsProvider should be explode when Factory is null", () => new ImportLineSadFieldDescriptionsProvider(null));
	}

	public void TestGetDescriptionBySequenceNumber()
	{
		var descriptionsProviderForLine = new ImportLineSadFieldDescriptionsProvider(Factory);
		AssertEquals("When Sequence Number is 81, Description should be", "Total Item Taxed Amount", descriptionsProviderForLine.GetDescriptionBySequenceNumber("81"));
		AssertEquals("When Sequence Number is Empty, Description should be", "", descriptionsProviderForLine.GetDescriptionBySequenceNumber(""));
		AssertEquals("When Sequence Number is null, Description should be", "", descriptionsProviderForLine.GetDescriptionBySequenceNumber(null));
	}

	public void TestGetDescriptionByReferenceCode()
	{
		var descriptionsProviderForLine = new ImportLineSadFieldDescriptionsProvider(Factory);
		AssertEquals("When Reference Code is T, Description should be", "Total Item Taxed Amount", descriptionsProviderForLine.GetDescriptionByReferenceCode("T"));
		AssertEquals("When Reference Code is Empty, Description should be", "", descriptionsProviderForLine.GetDescriptionByReferenceCode(""));
		AssertEquals("When Reference Code is null, Description should be", "", descriptionsProviderForLine.GetDescriptionByReferenceCode(null));
	}

	public void TestGetReferenceCodeBySequenceNumber()
	{
		var descriptionsProviderForLine = new ImportLineSadFieldDescriptionsProvider(Factory);
		AssertEquals("When Sequence Number is 81, reference should be", "T", descriptionsProviderForLine.GetReferenceCodeBySequenceNumber("81"));
		AssertEquals("When Sequence Number is Empty, reference should be", "", descriptionsProviderForLine.GetReferenceCodeBySequenceNumber(""));
		AssertEquals("When Sequence Number is null, reference should be", "", descriptionsProviderForLine.GetReferenceCodeBySequenceNumber(null));
	}

	public void TestGetProgressiveNumberByReferenceCode()
	{
		var descriptionsProviderForLine = new ImportLineSadFieldDescriptionsProvider(Factory);
		AssertEquals("When Reference Code is T, Description should be", "81", descriptionsProviderForLine.GetSequenceNumberByReferenceCode("T"));
		AssertEquals("When Reference Code is Empty Description should be", "", descriptionsProviderForLine.GetSequenceNumberByReferenceCode(""));
		AssertEquals("When Reference Code is null, Description should be", "", descriptionsProviderForLine.GetSequenceNumberByReferenceCode(null));
	}
}
