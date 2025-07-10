using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC029C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC029CMessageDataObjectTest : NCTSMessageDataObjectTest<CC029CMessageDataObject, Cc029CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC029CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC029CResponseMessage.xml");
	}
}
