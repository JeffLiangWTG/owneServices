using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA103;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class FRA103MessageDataObjectTest : DeltaIEMessageDataObjectTest<FRA103MessageDataObject, FRA103AType>
	{
		protected override Type ExpectedPrettierType => typeof(FRA103MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA103ResponseMessage.json");
	}
}
