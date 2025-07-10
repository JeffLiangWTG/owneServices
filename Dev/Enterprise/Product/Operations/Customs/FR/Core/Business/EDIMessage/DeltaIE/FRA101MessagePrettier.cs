using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA101;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FRA101MessagePrettier : DeltaIEMessagePrettier<FRA101AType>
	{
		public FRA101MessagePrettier(FRA101MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(FRA101AType messageObject)
		{
			var operation = messageObject.ImportOrExportOperation;
			var status = messageObject.DeclarationStatus;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("LRN", operation?.LRN ?? ZString.Empty),
				("CRN", operation?.CustomsRegistrationNumber ?? ZString.Empty),
				("MRN", operation?.MRN ?? ZString.Empty),
				((NoResString)"Declaration Type", operation?.DeclarationType ?? ZString.Empty),
				((NoResString)"Additional Declaration Type", operation?.AdditionalDeclarationType ?? ZString.Empty),
				((NoResString)"State", status?.State ?? ZString.Empty),
				((NoResString)"State Date Time", status?.StateDateTime ?? ZString.Empty),
				((NoResString)"Previous State", status?.PreviousState ?? ZString.Empty),
				((NoResString)"Event", status?.Event ?? ZString.Empty)
			});
		}
	}
}
