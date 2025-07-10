using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ImportSadFieldDescriptionsProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("ImportSadFieldDescriptionsProvider should be explode when Factory is null", () => new ImportHeaderSadFieldDescriptionsProvider(null));
	}

	public void TestGetDescriptionBySequenceNumber()
	{
		var descriptionsProviderForHeader = new ImportHeaderSadFieldDescriptionsProvider(Factory);

		AssertEquals("When Progressive Number is empty Description should be", ZString.Empty, descriptionsProviderForHeader.GetDescriptionBySequenceNumber(ZString.Empty));
		AssertEquals("When Progressive Number is invalid, Description should be", ZString.Empty, descriptionsProviderForHeader.GetDescriptionBySequenceNumber("XX"));
		AssertEquals("When Progressive Number is 15, Description should be", "Pre-Clearing", descriptionsProviderForHeader.GetDescriptionBySequenceNumber("15"));
		AssertEquals("When Progressive Number is Empty, Description should be", "", descriptionsProviderForHeader.GetDescriptionBySequenceNumber(""));
		AssertEquals("When Progressive Number is null, Description should be", "", descriptionsProviderForHeader.GetDescriptionBySequenceNumber(null));
	}

	public void TestGetDescriptionByReferenceCode()
	{
		var descriptionsProviderForHeader = new ImportHeaderSadFieldDescriptionsProvider(Factory);

		AssertEquals("When Reference Code is empty Description should be", ZString.Empty, descriptionsProviderForHeader.GetDescriptionByReferenceCode(ZString.Empty));
		AssertEquals("When Reference Code is invalid, Description should be", ZString.Empty, descriptionsProviderForHeader.GetDescriptionByReferenceCode("XX"));
		AssertEquals("When Reference Code is PRE.1, Description should be", "Pre-Clearing", descriptionsProviderForHeader.GetDescriptionByReferenceCode("PRE.1"));
		AssertEquals("When Reference Code is Empty, Description should be", "", descriptionsProviderForHeader.GetDescriptionByReferenceCode(""));
		AssertEquals("When Reference Code is null Description should be", "", descriptionsProviderForHeader.GetDescriptionByReferenceCode(null));
	}

	public void TestGetReferenceCodeBySequenceNumber()
	{
		var descriptionsProviderForHeader = new ImportHeaderSadFieldDescriptionsProvider(Factory);

		AssertEquals("When Progressive Number is empty reference should be", ZString.Empty, descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber(ZString.Empty));
		AssertEquals("When Progressive Number is invalid, reference should be", ZString.Empty, descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber("XX"));
		AssertEquals("When Progressive Number is 15, reference should be", "PRE.1", descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber("15"));
		AssertEquals("When Progressive Number is Empty, reference should be", "", descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber(""));
		AssertEquals("When Progressive Number is null, reference should be", "", descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber(null));
	}

	public void TestGetProgressiveNumberByReferenceCode()
	{
		var descriptionsProviderForHeader = new ImportHeaderSadFieldDescriptionsProvider(Factory);

		AssertEquals("When Reference Code is empty progressive should be", ZString.Empty, descriptionsProviderForHeader.GetSequenceNumberByReferenceCode(ZString.Empty));
		AssertEquals("When Reference Code is invalid, progressive should be", ZString.Empty, descriptionsProviderForHeader.GetSequenceNumberByReferenceCode("XX"));
		AssertEquals("When Reference Code is PRE.1, progressive should be", "15", descriptionsProviderForHeader.GetSequenceNumberByReferenceCode("PRE.1"));
		AssertEquals("When Reference Code is Empty, progressive should be", "", descriptionsProviderForHeader.GetSequenceNumberByReferenceCode(""));
		AssertEquals("When Reference Code is null, progressive should be", "", descriptionsProviderForHeader.GetSequenceNumberByReferenceCode(null));
	}
}
