using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC019C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC019CMessageDataObjectTest : NCTSMessageDataObjectTest<CC019CMessageDataObject, Cc019CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC019CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC019CResponseMessage.xml");
	}
}
