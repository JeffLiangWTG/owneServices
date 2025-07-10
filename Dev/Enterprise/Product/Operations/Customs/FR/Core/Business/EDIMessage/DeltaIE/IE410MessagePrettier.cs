using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE410;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE410MessagePrettier : DeltaIEMessagePrettier<CC410BType>
	{
		public IE410MessagePrettier(IE410MessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(CC410BType messageObject)
		{
			var importOperation = messageObject.ImportOperation.FirstOrDefault();
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("LRN", importOperation?.LRN ?? ZString.Empty),
				("CRN", importOperation?.CustomsRegistrationNumber ?? ZString.Empty),
				("MRN", importOperation?.MRN ?? ZString.Empty),
				("Operator request reference", messageObject.Request?.OperatorRequestReference ?? ZString.Empty),
				("Customs request reference", messageObject.Request?.CustomsRequestReference ?? ZString.Empty),
				("Invalidation decision date and time", importOperation?.InvalidationDecisionDateAndTime ?? ZString.Empty),
				("Invalidation request date and time", importOperation?.InvalidationRequestDateAndTime ?? ZString.Empty),
				("Invalidation initiated by customs", importOperation == null ? ZString.Empty : (ZString)importOperation.InvalidationInitiatedByCustoms.ToString()),
				("Invalidation motivation", importOperation?.InvalidationMotivation ?? ZString.Empty),
				("Invalidation justification", importOperation?.InvalidationJustification ?? ZString.Empty)
			});
		}
	}
}
