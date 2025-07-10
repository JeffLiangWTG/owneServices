using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE917;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class IE917MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE917MessageDataObject, CC917BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE917MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ResponseMessage.json");
	}
}
