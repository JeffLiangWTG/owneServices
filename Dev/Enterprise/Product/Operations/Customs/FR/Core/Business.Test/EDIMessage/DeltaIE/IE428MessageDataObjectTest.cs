using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE428MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE428MessageDataObject, CC428BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE428MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE428ResponseMessage.json");
	}
}
