using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF03C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing;

sealed class CCF03CMessageDataObjectTest : NCTSMessageDataObjectTest<CCF03CMessageDataObject, Ccf03CType>
{
	protected override Type ExpectedPrettierType => typeof(CCF03CMessagePrettier);

	protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF03CResponseMessage.xml");
}
