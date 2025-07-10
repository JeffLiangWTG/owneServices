using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC057C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC057CMessageDataObjectTest : NCTSMessageDataObjectTest<CC057CMessageDataObject, Cc057CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC057CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC057CResponseMessage.xml");
	}
}
