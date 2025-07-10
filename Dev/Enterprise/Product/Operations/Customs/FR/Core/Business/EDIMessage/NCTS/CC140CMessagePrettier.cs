using CargoWise.Customs.FR.MessageDefinitions.TP5.CC140C;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC140CMessagePrettier : NCTSMessagePrettier<Cc140CType>
	{
		public CC140CMessagePrettier(CC140CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc140CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", transitOperation != null ? TP5ResponseMessageSubTypeList.Descriptions.RequestOnNonArrivedMovement : ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Request Date", transitOperation?.RequestOnNonArrivedMovementDate.ToString("dd/MM/yyyy") ?? ZString.Empty),
				((NoResString)"Limit Response Date", transitOperation?.LimitForResponseDate.ToString("dd/MM/yyyy") ?? ZString.Empty),
			});
		}
	}
}
