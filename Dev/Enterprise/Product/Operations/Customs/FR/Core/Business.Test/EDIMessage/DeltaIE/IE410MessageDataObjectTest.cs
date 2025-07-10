using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE410;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE410MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE410MessageDataObject, CC410BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE410MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE410ResponseMessage.json");
	}
}
