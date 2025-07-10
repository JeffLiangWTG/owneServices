using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC055C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC055CMessagePrettier : NCTSMessagePrettier<Cc055CType>
	{
		public CC055CMessagePrettier(CC055CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc055CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;

			var messageInterpretations = new List<(ZString key, ZString value)>
			{
				((NoResString)"Status", NCTS5DepartureCustomsStatusList.Descriptions.GuaranteeInvalid),
				("MRN", transitOperation?.Mrn ?? ZString.Empty)
			};

			return ToKeyValuePairSection(messageInterpretations.ToArray());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Cc055CType messageObject)
		{
			var invalidGuaranteeReasons = messageObject.GuaranteeReference.Where(r => r.InvalidGuaranteeReason != null).OrderBy(r => r.SequenceNumber);
			if (invalidGuaranteeReasons.Any())
			{
				var tableCreator = GetHtmlTableCreator();
				var invalidGuaranteeReasonTitle = (NoResString)"Invalid Guarantee Reason";
				tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[]
				{
					new CellWithFormatting("GRN", "width", "75px"),
					new CellWithFormatting(invalidGuaranteeReasonTitle, "width", "200px"),
				});

				foreach( var invalidGuaranteeReason in invalidGuaranteeReasons)
				{
					tableCreator.WriteRow(invalidGuaranteeReason.Grn, GetNCTS5InvalidGuaranteeReasonDescription(invalidGuaranteeReason.InvalidGuaranteeReason.Code));
				}
				return ToTableSection("Invalid Guarantee Reason", tableCreator);
			}
			return ZString.Empty;
		}
	}
}
