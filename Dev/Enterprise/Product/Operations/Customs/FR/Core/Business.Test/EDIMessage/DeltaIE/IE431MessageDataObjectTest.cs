using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE431;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE431MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE431MessageDataObject, CC431BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE431MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE431ResponseMessage.json");
	}
}
