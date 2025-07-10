using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC025C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC025CMessageDataObjectTest : NCTSMessageDataObjectTest<CC025CMessageDataObject, Cc025CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC025CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC025C_RI1_ResponseMessage.xml");
	}
}
