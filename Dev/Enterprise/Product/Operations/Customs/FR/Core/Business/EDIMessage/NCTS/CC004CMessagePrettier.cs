using CargoWise.Customs.FR.MessageDefinitions.TP5.CC004C;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC004CMessagePrettier : NCTSMessagePrettier<Cc004CType>
	{
		public CC004CMessagePrettier(CC004CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc004CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("LRN", transitOperation?.Lrn ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Acceptance Date Time", transitOperation?.AmendmentAcceptanceDateAndTime.ToString() ?? ZString.Empty)
			});
		}
	}
}
