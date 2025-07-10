using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC009C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC009CMessageDataObjectTest : NCTSMessageDataObjectTest<CC009CMessageDataObject, Cc009CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC009CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC009CResponseMessage.xml");
	}
}
