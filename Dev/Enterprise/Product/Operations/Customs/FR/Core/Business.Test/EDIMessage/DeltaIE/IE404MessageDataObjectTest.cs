using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE404;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE404MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE404MessageDataObject, CC404BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE404MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE404ResponseMessage.json");
	}
}
