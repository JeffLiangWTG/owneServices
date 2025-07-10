using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE404;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE404MessagePrettier : DeltaIEMessagePrettier<CC404BType>
	{
		public IE404MessagePrettier(IE404MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(CC404BType messageObject)
		{
			var remarks = messageObject.Remarks;

			var result = ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Customs request reference", messageObject.Request?.CustomsRequestReference ?? ZString.Empty),
				("Operator request reference", messageObject.Request?.OperatorRequestReference ?? ZString.Empty),
				("MRN", messageObject.ImportOperation?.MRN ?? ZString.Empty),
				("Amendment date and time", messageObject.ImportOperation?.AmendmentDateAndTime ?? ZString.Empty),
				("Amendment acceptance date and time", messageObject.ImportOperation?.AmendmentAcceptanceDateAndTime ?? ZString.Empty),
				("Amendment justification", messageObject.ImportOperation?.AmendmentJustification ?? ZString.Empty),
			});

			if (!remarks.IsNullOrEmpty())
			{
				result += ToKeyValuePairSection(remarks.Select(r => ((ZString)"Remarks code and Reason", (ZString)(r.Code + r.Reason))), title: "Remarks");
			}

			return result;
		}
	}
}
