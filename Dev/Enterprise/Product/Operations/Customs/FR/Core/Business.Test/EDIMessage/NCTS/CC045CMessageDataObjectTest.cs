using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC045C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC045CMessageDataObjectTest : NCTSMessageDataObjectTest<CC045CMessageDataObject, Cc045CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC045CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC045CResponseMessage.xml");
	}
}
