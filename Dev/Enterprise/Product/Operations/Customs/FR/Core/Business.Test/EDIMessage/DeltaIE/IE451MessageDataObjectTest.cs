using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE451;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class IE451MessageDataObjectTest : DeltaIEMessageDataObjectTest<IE451MessageDataObject, CC451BType>
	{
		protected override Type ExpectedPrettierType => typeof(IE451MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE451ResponseMessage.json");
	}
}
