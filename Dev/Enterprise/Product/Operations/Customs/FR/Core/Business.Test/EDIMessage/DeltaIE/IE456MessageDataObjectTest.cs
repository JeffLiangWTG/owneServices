using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE456;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE456MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE456MessageDataObject, CC456BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE456MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE456ResponseMessage.json");
	}
}
