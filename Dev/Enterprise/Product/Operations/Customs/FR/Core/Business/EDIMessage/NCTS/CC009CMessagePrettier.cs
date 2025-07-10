using CargoWise.Customs.FR.MessageDefinitions.TP5.CC009C;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC009CMessagePrettier : NCTSMessagePrettier<Cc009CType>
	{
		public CC009CMessagePrettier(CC009CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(Cc009CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			var invalidation = messageObject.Invalidation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", "Invalidation Decision"),
				("LRN", transitOperation?.Lrn ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Decision Date Time", invalidation?.DecisionDateAndTime.ToString() ?? ZString.Empty),
				("Initiated By Customs", invalidation?.InitiatedByCustoms == CargoWise.Customs.FR.MessageDefinitions.TP5.Flag.Item0 ? "0" : "1" ?? ZString.Empty)
			});
		}
	}
}
