using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC917C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC917CMessageDataObjectTest : NCTSMessageDataObjectTest<CC917CMessageDataObject, Cc917CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC917CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC917CResponseMessage.xml");
	}
}
