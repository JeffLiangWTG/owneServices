using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC182C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC182CMessageDataObjectTest : NCTSMessageDataObjectTest<CC182CMessageDataObject, Cc182CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC182CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC182CResponseMessage.xml");
	}
}
