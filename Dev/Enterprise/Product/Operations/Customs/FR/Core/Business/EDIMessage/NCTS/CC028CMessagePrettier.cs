using CargoWise.Customs.FR.MessageDefinitions.TP5.CC028C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC028CMessagePrettier : NCTSMessagePrettier<Cc028CType>
	{
		public CC028CMessagePrettier(NCTSMessageDataObject<Cc028CType> messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc028CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", NCTS5DepartureCustomsStatusList.Descriptions.MrnAllocated ?? ZString.Empty),
				("LRN", transitOperation?.Lrn ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Acceptance Date Time", transitOperation?.DeclarationAcceptanceDate.ToString() ?? ZString.Empty)
			});
		}
	}
}
