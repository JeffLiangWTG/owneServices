using System;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA101;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class FRA101MessageDataObjectTest : DeltaIEMessageDataObjectTest<FRA101MessageDataObject, FRA101AType>
	{
		protected override Type ExpectedPrettierType => typeof(FRA101MessagePrettier);

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA101ResponseMessage.json");
	}
}
