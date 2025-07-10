using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ExportHeaderSadFieldDescriptionsProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Factory parameter cannot be null when creating ImportSadFieldDescriptionsProvider", () => new ExportHeaderSadFieldDescriptionsProvider(null));
	}

	public void TestGetDescriptionBySequenceNumber()
	{
		var descriptionsProviderForHeader = new ExportHeaderSadFieldDescriptionsProvider(Factory);

		AssertEquals("When Progressive Number is empty Description should be", ZString.Empty, descriptionsProviderForHeader.GetDescriptionBySequenceNumber(ZString.Empty));
		AssertEquals("When Progressive Number is invalid, Description should be", ZString.Empty, descriptionsProviderForHeader.GetDescriptionBySequenceNumber("XX"));
		AssertEquals("When Progressive Number is 13, Description should be", "Security", descriptionsProviderForHeader.GetDescriptionBySequenceNumber("13"));
		AssertEquals("When Progressive Number is Empty, Description should be", "", descriptionsProviderForHeader.GetDescriptionBySequenceNumber(""));
		AssertEquals("When Progressive Number is null, Description should be", "", descriptionsProviderForHeader.GetDescriptionBySequenceNumber(null));
	}

	public void TestGetDescriptionByReferenceCode()
	{
		var descriptionsProviderForHeader = new ExportHeaderSadFieldDescriptionsProvider(Factory);

		AssertEquals("When Reference Code is empty Description should be", ZString.Empty, descriptionsProviderForHeader.GetDescriptionByReferenceCode(ZString.Empty));
		AssertEquals("When Reference Code is invalid, Description should be", ZString.Empty, descriptionsProviderForHeader.GetDescriptionByReferenceCode("XX"));
		AssertEquals("When Reference Code is S00, Description should be", "Security", descriptionsProviderForHeader.GetDescriptionByReferenceCode("S00"));
		AssertEquals("When Reference Code is Empty, Description should be", "", descriptionsProviderForHeader.GetDescriptionByReferenceCode(""));
		AssertEquals("When Reference Code is null Description should be", "", descriptionsProviderForHeader.GetDescriptionByReferenceCode(null));
	}

	public void TestGetReferenceCodeBySequenceNumber()
	{
		var descriptionsProviderForHeader = new ExportHeaderSadFieldDescriptionsProvider(Factory);

		AssertEquals("When Progressive Number is empty reference should be", ZString.Empty, descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber(ZString.Empty));
		AssertEquals("When Progressive Number is invalid, reference should be", ZString.Empty, descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber("XX"));
		AssertEquals("When Progressive Number is 13, reference should be", "S00", descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber("13"));
		AssertEquals("When Progressive Number is Empty, reference should be", "", descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber(""));
		AssertEquals("When Progressive Number is null, reference should be", "", descriptionsProviderForHeader.GetReferenceCodeBySequenceNumber(null));
	}

	public void TestGetProgressiveNumberByReferenceCode()
	{
		var descriptionsProviderForHeader = new ExportHeaderSadFieldDescriptionsProvider(Factory);

		AssertEquals("When Reference Code is empty progressive should be", ZString.Empty, descriptionsProviderForHeader.GetSequenceNumberByReferenceCode(ZString.Empty));
		AssertEquals("When Reference Code is invalid, progressive should be", ZString.Empty, descriptionsProviderForHeader.GetSequenceNumberByReferenceCode("XX"));
		AssertEquals("When Reference Code is S00, progressive should be", "13", descriptionsProviderForHeader.GetSequenceNumberByReferenceCode("S00"));
		AssertEquals("When Reference Code is Empty, progressive should be", "", descriptionsProviderForHeader.GetSequenceNumberByReferenceCode(""));
		AssertEquals("When Reference Code is null, progressive should be", "", descriptionsProviderForHeader.GetSequenceNumberByReferenceCode(null));
	}
}
