using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC035C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC035CMessageDataObjectTest : NCTSMessageDataObjectTest<CC035CMessageDataObject, Cc035CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC035CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC035CResponseMessage.xml");
	}
}
