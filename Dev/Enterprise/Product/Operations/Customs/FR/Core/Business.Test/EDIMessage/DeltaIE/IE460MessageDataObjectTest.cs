using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE460;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE460MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE460MessageDataObject, CC460BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE460MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE460ResponseMessage.json");
	}
}
