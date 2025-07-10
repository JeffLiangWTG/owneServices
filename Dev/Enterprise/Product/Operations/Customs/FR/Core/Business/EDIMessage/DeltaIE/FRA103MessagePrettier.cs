using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA103;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FRA103MessagePrettier : DeltaIEMessagePrettier<FRA103AType>
	{
		public FRA103MessagePrettier(FRA103MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetErrorsTableIfNeeded(FRA103AType messageObject) => ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(FRA103AType messageObject)
		{
			var operation = messageObject.Operation.FirstOrDefault();
			var request = messageObject.Request;
			var result = ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("LRN", operation?.LRN ?? ZString.Empty),
				("CRN", operation?.CustomsRegistrationNumber ?? ZString.Empty),
				("MRN", operation?.MRN ?? ZString.Empty),
				("Customs request reference", request?.CustomsRequestReference ?? ZString.Empty),
				("Timer request instruction start date", messageObject.ExtendedTimerForRequestInstruction?.TimerRequestInstructionStartDate ?? ZString.Empty),
				("Initial timer request instruction expiry date", messageObject.ExtendedTimerForRequestInstruction?.InitialTimerRequestInstructionExpiryDate ?? ZString.Empty),
				("New timer request instruction expiry date", messageObject.ExtendedTimerForRequestInstruction?.NewTimerRequestInstructionExpiryDate ?? ZString.Empty),
				("Extension Information", messageObject.ExtendedTimerForRequestInstruction?.ExtensionInformation ?? ZString.Empty),
			});
			return result;
		}
	}
}
