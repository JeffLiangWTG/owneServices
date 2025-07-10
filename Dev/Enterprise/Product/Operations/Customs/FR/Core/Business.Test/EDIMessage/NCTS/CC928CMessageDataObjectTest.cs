using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC928C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC928CMessageDataObjectTest : NCTSMessageDataObjectTest<CC928CMessageDataObject, Cc928CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC928CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC928CResponseMessage.xml");
	}
}
