using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC004C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC004CMessageDataObjectTest : NCTSMessageDataObjectTest<CC004CMessageDataObject, Cc004CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC004CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC004CResponseMessage.xml");
	}
}
