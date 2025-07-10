using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC055C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC055CMessageDataObjectTest : NCTSMessageDataObjectTest<CC055CMessageDataObject, Cc055CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC055CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC055CResponseMessage.xml");
	}
}
