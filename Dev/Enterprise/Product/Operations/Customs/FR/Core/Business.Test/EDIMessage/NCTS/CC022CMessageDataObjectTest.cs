using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC022C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC022CMessageDataObjectTest : NCTSMessageDataObjectTest<CC022CMessageDataObject, Cc022CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC022CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC022CResponseMessage.xml");
	}
}
