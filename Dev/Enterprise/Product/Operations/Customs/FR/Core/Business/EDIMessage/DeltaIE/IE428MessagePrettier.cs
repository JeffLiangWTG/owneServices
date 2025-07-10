using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE428MessagePrettier : DeltaIEMessagePrettier<CC428BType>
	{
		public IE428MessagePrettier(IE428MessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(CC428BType messageObject)
		{
			var importOperation = messageObject.ImportOperation;
			var declarationStatus = messageObject.DeclarationStatus;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", declarationStatus?.State ?? ZString.Empty),
				("Status Date", declarationStatus?.StateDateTime ?? ZString.Empty),
				("Acceptance Date Time", importOperation?.DeclarationAcceptanceDateAndTime ?? ZString.Empty),
				("LRN", importOperation?.LRN ?? ZString.Empty),
				("CRN", importOperation?.CustomsRegistrationNumber ?? ZString.Empty),
				("MRN", importOperation?.MRN ?? ZString.Empty)
			});
		}
	}
}
