using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC043C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC043CMessageDataObjectTest : NCTSMessageDataObjectTest<CC043CMessageDataObject, Cc043CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC043CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC043CResponseMessage.xml");
	}
}
