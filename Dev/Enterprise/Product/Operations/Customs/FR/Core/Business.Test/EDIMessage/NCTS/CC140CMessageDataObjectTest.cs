using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC140C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class CC140CMessageDataObjectTest : NCTSMessageDataObjectTest<CC140CMessageDataObject, Cc140CType>
	{
		protected override Type ExpectedPrettierType => typeof(CC140CMessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC140CResponseMessage.xml");
	}
}
