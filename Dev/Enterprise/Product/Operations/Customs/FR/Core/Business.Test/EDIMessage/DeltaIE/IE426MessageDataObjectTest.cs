using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE426MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE426MessageDataObject, CC426BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE426MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE426ResponseMessage.json");
	}
}
