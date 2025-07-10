using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

sealed class MrnAndIutMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader and messageType is null", () => new MrnAndIutMessageWrapper(null, null));
	}

	public void TestMrn()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.MovementReferenceNumberSetter("22CH00000294926586");

		var messageWrapper = (IMrnAndIutProvider)new MrnAndIutMessageWrapper(entryHeader, EDIMessageTypeList.Codes.AccountingSummaryRequest);
		AssertEquals(nameof(messageWrapper.Mrn), "22CH00000294926586", messageWrapper.Mrn);
	}

	public void TestIut()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		var message = entryHeader.Messages.AddNew();
		message.EM_MessageType = "PRR";
		message.IsTransmitMessage = false;
		message.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.TestFiles.TestAccountingSummaryRequestReceived.xml");

		var messageWrapper = (IMrnAndIutProvider)new MrnAndIutMessageWrapper(entryHeader, EDIMessageTypeList.Codes.AccountingSummaryRequest);
		AssertEquals(nameof(messageWrapper.Iut), "20240725D12050138112", messageWrapper.Iut);
	}
}
