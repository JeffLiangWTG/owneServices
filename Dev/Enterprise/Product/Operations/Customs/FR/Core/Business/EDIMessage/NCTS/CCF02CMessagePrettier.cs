using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF02C;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CCF02CMessagePrettier : NCTSMessagePrettier<Ccf02CType>
	{
		public CCF02CMessagePrettier(CCF02CMessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Ccf02CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
					((NoResString)"Object", TP5ResponseMessageSubTypeList.Descriptions.StatusUpdateNotification),
					("LRN", transitOperation?.Lrn ?? ZString.Empty),
					("MRN", transitOperation?.Mrn ?? ZString.Empty),
					((NoResString)"Date and Time", transitOperation?.Date.ToString() ?? ZString.Empty),
					((NoResString)"Detailed status", new TP5InterpretedDetailedStatusList().GetDescriptionFromCode(NCTSFREDIMessage.GetOriginalStatus(transitOperation?.Statut)) ?? ZString.Empty),
					((NoResString)"Comments", transitOperation?.Commentaire ?? ZString.Empty),
					((NoResString)"Status", NCTSFREDIMessage.GetOriginalStatus(transitOperation?.Statut))
			});
		}
	}
}
