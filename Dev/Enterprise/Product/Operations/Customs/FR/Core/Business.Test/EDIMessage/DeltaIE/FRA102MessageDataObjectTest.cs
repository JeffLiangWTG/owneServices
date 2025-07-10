using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA102;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class FRA102MessageDataObjectTest : DeltaIEMessageDataObjectTest<FRA102MessageDataObject, FRA102AType>
	{
		protected override Type ExpectedPrettierType => typeof(FRA102MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA102ResponseMessage.json");
	}
}
