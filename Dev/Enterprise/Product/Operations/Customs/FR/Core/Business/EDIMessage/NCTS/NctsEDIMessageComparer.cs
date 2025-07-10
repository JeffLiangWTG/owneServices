using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class NctsEDIMessageComparer : EDIMessageComparer
	{
		public NctsEDIMessageComparer(ListSortDirection sortDirection) : base(sortDirection)
		{
		}

		protected override int CompareInterchangeNums(EDIMessage messageX, EDIMessage messageY)
		{
			var result = base.CompareInterchangeNums(messageX, messageY);
			var interchangeNumberX = messageX.EM_InterchangeNumber.Split('.');
			var interchangeNumberY = messageY.EM_InterchangeNumber.Split('.');

			if (interchangeNumberX.Length == 3 && interchangeNumberY.Length == 3 && interchangeNumberX.First() == interchangeNumberY.First())
			{
				var schemaIdAndFunctionX = GetSchemaIDAndFunction(messageX);
				var schemaIdAndFunctionY = GetSchemaIDAndFunction(messageY);

				if (EntryStatusOrderList.Contains((schemaIdAndFunctionX, schemaIdAndFunctionY)))
				{
					result = -1;
				}
				else if (EntryStatusOrderList.Contains((schemaIdAndFunctionY, schemaIdAndFunctionX)))
				{
					result = 1;
				}
			}

			return result;
		}

		public static readonly List<(string, string)> EntryStatusOrderList = new List<(string, string)>()
		{
			("028", "F02|GARANTIE_SOUS_ENRG"),
			("F02|GARANTIE_SOUS_ENRG", "029"),
			("028", "029"),
			("019", "F02|NOTIF_ARRIVEE_DEST"),
			("F02|NOTIF_ARRIVEE_DEST", "045"),
			("F02|DEMANDE_INVALID", "009"),
			("F02|DEMANDE_RECTIF", "029"),
			("F02|DEMANDE_RECTIF", "004"),
			("F02|DEMANDE_RECTIF", "022"),
			("028", "928"),
			("F02|AVIS_ANT_ARRIV_DEM", "F03"),
			("F03", "025"),
			("F03", "043"),
			("182", "F03"),
			("182", "025"),
			("F02|RECHERCHE_ENGAGEE", "035"),
			("035", "F02|INFO_RECOUVREMENT"),
			("F02|INFO_RECOUVREMENT", "025"),
			("035", "025"),
		};

		static string GetSchemaIDAndFunction(EDIMessage message)
		{
			var schemaId = message.EM_InterchangeNumber.Split('.')[1].SubstringSafe(2, 3);
			var function = schemaId == TP5ResponseMessageSubTypeList.Codes.StatusUpdateNotification ? "|" + GetF02DetailedStatus(message) : string.Empty;
			return schemaId + function;
		}

		public static EDIMessage[] GetSortedMessages(IEnumerable<EDIMessage> messages, ListSortDirection sortDirection)
		{
			return messages.OrderBy((EDIMessage x) => x, new NctsEDIMessageComparer(sortDirection)).ToArray();
		}

		static string GetF02DetailedStatus(EDIMessage message)
		{
			var result = string.Empty;
			if (message is NCTSFREDIMessage nctsFREDIMessage && nctsFREDIMessage.MessageDataObject is CCF02CMessageDataObject messageDataObject)
			{
				result = NCTSFREDIMessage.GetOriginalStatus(messageDataObject.ResponseMessage.TransitOperation?.Statut);
			}
			return result;
		}
	}
}
