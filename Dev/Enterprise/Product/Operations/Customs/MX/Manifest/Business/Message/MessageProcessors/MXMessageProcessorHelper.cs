using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public static class MXMessageProcessorHelper
	{
		public static AsycudaBill LookForAsycudaBill(BusinessObjectFactory factory, ZString messageReference)
		{
			AsycudaBill bill = null;

			if (!messageReference.IsEmpty)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, MXMessage.ApplicationCodes.MXCustoms) { OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " DESC", };
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, MXMessage.Direction.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageReference);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypes.Codes.MXA);

				var originalMessage = factory.LoadTop1<MXMessage>(query);
				if (originalMessage != null)
				{
					bill = factory.Load<AsycudaBill>(originalMessage.EM_LinkUniqueID);
				}
			}

			return bill;
		}

		#region First Response Sea Mode

		public static List<Tuple<string, string>> GetFirstResponseInformation(BLFirstResponseMessage response)
		{
			var valueList = new List<Tuple<string, string>>();
			valueList.Add(new Tuple<string, string>((NoResString)"Resp", response.Resp));
			valueList.Add(new Tuple<string, string>("IdPet", response.IdPet));
			return valueList;
		}

		#endregion

		#region Final Response Sea Mode

		public static List<Tuple<string, string>> GetFinalResponseInformation(string errorCount)
		{
			var resp = errorCount == ZInt.Zero.ToString() ? MXBLMessageConstants.Accepted : MXBLMessageConstants.Rejected;

			var valueList = new List<Tuple<string, string>>();
			valueList.Add(new Tuple<string, string>((NoResString)"Response", resp));
			valueList.Add(new Tuple<string, string>((NoResString)"No. Errors", errorCount));
			return valueList;
		}

		public static ZString MessageInterpretationAcceptedFinalResponse(ZString billNumber)
		{
			var note = new StringBuilder();

			note.Append(ZString.Format("{0} : {1} {2}", MXMessageConstants.HBL, billNumber, MXMessageConstants.AcceptedByCustoms));
			note.AppendLine();

			return note.ToString().Trim();
		}

		public static ZString MessageInterpretationRejectedFinalResponse(ZString numberErrors, ZString billNumber, List<(string, string)> errors)
		{
			var note = new StringBuilder();

			note.Append(ZString.Format("{0} : {1} {2}", MXBLMessageConstants.SendingBill, billNumber, MXMessageConstants.RejectedMessage));
			note.AppendLine();

			note.Append(ZString.Format("{0} : {1}", MXBLMessageConstants.NumberErrors, numberErrors));
			note.AppendLine();

			if (Convert.ToInt32(numberErrors) > 0)
			{
				foreach (var error in errors)
				{
					note.Append(ZString.Format("{0} : {1} {2} {3}", MXBLMessageConstants.CodeOfError, error.Item1, MXMessageConstants.DescriptionError, error.Item2));
					note.AppendLine();
				}
			}
			return note.ToString().Trim();
		}

		internal static List<(string, string)> GetErrorsFromXml(ZString messageText)
		{
			var errors = new List<(string, string)>();

			var doc = new XmlDocument();
			doc.LoadXml(messageText);

			GetErrorsFromXmlElements(doc, new List<string>() { "K1", "K1M10", "K1M11", "K1N1" }, errors);

			return errors.Distinct().ToList();
		}

		internal static void GetErrorsFromXmlElements(XmlDocument doc, List<string> nodesName, List<(string, string)> errors)
		{
			foreach (var nodeName in nodesName)
			{
				var nodes = doc.GetElementsByTagName(nodeName);
				for (var i = 0; i < nodes.Count; i++)
				{
					var node = nodes.Item(i);
					errors.Add((node.FirstChild?.InnerText, node.LastChild?.InnerText));
				}
			}
		}

		#endregion

		#region Error Response Sea Mode

		public static ZString MessageInterpretationForCustomsErrorMessage(Dictionary<string, string> headerTextDictionary)
		{
			var messageText = new ZStringBuilder();

			var errorType = GetValueFromHeaderTextDictionary(headerTextDictionary, "custom.ErrorType");
			var notificationType = GetValueFromHeaderTextDictionary(headerTextDictionary, "custom.NotificationType");
			var errorDescription = GetValueFromHeaderTextDictionary(headerTextDictionary, "custom.ErrorDescription");

			messageText.Append(ZString.Format("{0}{1}", MXMessageConstants.ErrorType, errorType));
			messageText.AppendLine();
			messageText.Append(ZString.Format("{0}{1}", MXMessageConstants.NotificationType, notificationType));
			messageText.AppendLine();
			messageText.Append(ZString.Format("{0}{1}", MXMessageConstants.ErrorDescription, errorDescription));
			messageText.AppendLine();

			return messageText.ToString();
		}

		public static ZString GetValueFromHeaderTextDictionary(Dictionary<string, string> headerTextDictionary, ZString key)
		{
			return headerTextDictionary.TryGetValue(key, out var res) ? res : string.Empty;
		}

		#endregion

		#region First Response Air Mode

		internal static ZString MessageInterpretationFirstResponseAirMode(ZString status, ZString billNumber, List<string> errors, ZString result)
		{
			var note = new StringBuilder();
			note.Append(ZString.Format("{0} : {1} {2}", MXMessageConstants.HBL, billNumber, result));
			note.AppendLine();
			note.Append(ZString.Format("{0} : {1}", MXAWBMessageConstants.Status, status));

			if (errors.Count > 0)
			{
				note.AppendLine();
				note.Append(MXMessageConstants.DescriptionError.Trim());
				note.AppendLine();

				foreach (var error in errors)
				{
					note.Append(error);
					note.AppendLine();
				}
			}
			return note.ToString().Trim();
		}

		#endregion

		#region Final Response Air Mode

		internal static ZString MessageInterpretationFinalResponseAirMode(AWBFinalResponseMessage response, ZString billNumber, ZString result)
		{
			var note = new StringBuilder();
			note.Append(ZString.Format("{0} : {1} {2}", MXMessageConstants.HBL, billNumber, result));
			note.AppendLine();
			note.Append(ZString.Format("{0} : {1}", MXAWBMessageConstants.ID, response.ID));
			note.AppendLine();
			note.Append(ZString.Format("{0} : {1}", MXAWBMessageConstants.Name, response.Name));
			note.AppendLine();
			note.Append(ZString.Format("{0} : {1}", MXAWBMessageConstants.Status, response.Status));
			note.AppendLine();

			if (response.Errors.Count > 0)
			{
				foreach (var error in response.Errors)
				{
					note.Append(ZString.Format("{0} : {1}", MXMessageConstants.DescriptionError, error));
					note.AppendLine();
				}
			}
			return note.ToString().Trim();
		}

		#endregion
	}
}
