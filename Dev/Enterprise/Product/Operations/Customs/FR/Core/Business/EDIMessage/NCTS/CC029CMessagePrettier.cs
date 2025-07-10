using CargoWise.Customs.FR.MessageDefinitions.TP5.CC029C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC029CMessagePrettier : NCTSMessagePrettier<Cc029CType>
	{
		public CC029CMessagePrettier(CC029CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc029CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", NCTS5DepartureCustomsStatusList.Descriptions.ReleasedForTransit ?? ZString.Empty),
				("LRN", transitOperation?.Lrn ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Release Date", transitOperation?.ReleaseDate.ToString("dd/MM/yyyy") ?? ZString.Empty)
			});
		}
	}
}
