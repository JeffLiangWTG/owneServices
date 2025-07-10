using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

sealed class MrnMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new MrnMessageWrapper(null));
	}

	public void TestMrn()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.MovementReferenceNumberSetter("22CH00000294926586");

		var messageWrapper = (IMrnProvider)new MrnMessageWrapper(entryHeader);
		AssertEquals(nameof(messageWrapper.Mrn), "22CH00000294926586", messageWrapper.Mrn);
	}
}
