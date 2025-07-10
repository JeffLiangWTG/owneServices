using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE429MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE429MessageDataObject, CC429BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE429MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE429ResponseMessage.json");
	}
}
