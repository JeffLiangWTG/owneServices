using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CD906C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CD906CMessageDataObjectTest : NCTSMessageDataObjectTest<CD906CMessageDataObject, Cd906CType>
	{
		protected override Type ExpectedPrettierType => typeof(CD906CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CD906CResponseMessage.xml");
	}
}
