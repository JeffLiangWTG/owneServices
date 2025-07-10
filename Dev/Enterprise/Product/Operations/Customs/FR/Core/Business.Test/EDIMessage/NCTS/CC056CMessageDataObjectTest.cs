using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC056C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC056CMessageDataObjectTest : NCTSMessageDataObjectTest<CC056CMessageDataObject, Cc056CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC056CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC056CResponseMessage.xml");
	}
}
