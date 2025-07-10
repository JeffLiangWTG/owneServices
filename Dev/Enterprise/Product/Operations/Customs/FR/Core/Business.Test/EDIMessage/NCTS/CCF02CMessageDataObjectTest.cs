using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF02C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CCF02CMessageDataObjectTest : NCTSMessageDataObjectTest<CCF02CMessageDataObject, Ccf02CType>
	{
		protected override Type ExpectedPrettierType => typeof(CCF02CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF02CResponseMessage_ANTICIPEE.xml");
	}
}
