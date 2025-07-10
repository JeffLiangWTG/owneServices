using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public static class ARMessageProcessorHelper
	{
		public static AsycudaManifestHeader LookForAsycudaManifestHeader(BusinessObjectFactory factory, ZString reference)
		{
			return factory.Load<AsycudaManifestHeader>(new ZQuery()).Where(x => x.RegistrationNumber == reference)?.FirstOrDefault();
		}

		public static ZString GetInterchangeMessageTo(ZString messageType)
		{
			switch (messageType)
			{
				case MessageTypes.Codes.ARA:
				case MessageTypes.Codes.ARB:
					return ARMessageConstants.ARCustomsSeaMode;
				case MessageTypes.Codes.ARD:
					return ARMessageConstants.ARCustomsAirMode;
				default:
					return ZString.Empty;
			}
		}

		#region Sea Response

		public static List<Tuple<string, string>> GetSeaResponseInformation(SeaResponseMessage response)
		{
			var valueList = new List<Tuple<string, string>>();
			valueList.Add(new Tuple<string, string>((NoResString)"Errores", response.Errors.Count.ToString()));
			valueList.Add(new Tuple<string, string>("IdViaje", response.ID));
			return valueList;
		}

		public static ZString MessageInterpretationAcceptedSeaMode(SeaResponseMessage response)
		{
			var note = new StringBuilder();

			note.Append(ARBLMessageConstants.Accepted);
			note.Append(ARBLMessageConstants.NewLine);
			note.Append(ARBLMessageConstants.ID);
			note.Append(response.ID);

			return note.ToString().Trim();
		}

		public static ZString MessageInterpretationRejectedSeaMode(SeaResponseMessage response)
		{
			var note = new StringBuilder();

			note.Append(ARBLMessageConstants.Rejected);
			note.Append(ARBLMessageConstants.NewLine);
			note.Append(ARBLMessageConstants.ID);
			note.Append(response.ID);
			note.Append(ARBLMessageConstants.NewLine);
			note.Append(ARBLMessageConstants.Description);
			note.Append(ARBLMessageConstants.NewLine);
			note.Append(ARBLMessageConstants.NewLine);

			foreach (var error in response.Errors)
			{
				note.Append(ARBLMessageConstants.Reason);
				note.Append(error?.Code);
				note.Append(ARBLMessageConstants.NewLine);
				note.Append(error?.Description);
				note.Append(ARBLMessageConstants.NewLine);
				note.Append(error?.AdditionalDescription);
				note.Append(ARBLMessageConstants.NewLine);
				note.Append(ARBLMessageConstants.NewLine);
			}

			return note.ToString().Trim();
		}

		#endregion

		#region Sea Request

		internal static ZString EnvelopeMessage(ZString xMLMessage)
		{
			xMLMessage = Regex.Replace(xMLMessage, @"<\b", "<ar:");
			xMLMessage = Regex.Replace(xMLMessage, @"<\/", @"</ar:");

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.AR.Manifest.Business.Message.Templates.ARSeaManifestFullRequest.xml"))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd().Replace("{BodyText}", xMLMessage);
			}
		}

		#endregion

		#region Air Response

		internal static ZString MessageInterpretationResponseAirMode(AirResponseMessage response, ZString billNumber, List<IAirResponseError> errors, ZString result)
		{
			var note = new StringBuilder();
			note.Append(ZString.Format("{0} : {1} {2}", ARMessageConstants.HBL, billNumber, result));
			note.AppendLine();
			note.Append(ZString.Format("{0} : {1}", ARAWBMessageConstants.ID, response.ID));
			note.AppendLine();
			note.Append(ZString.Format("{0} : {1}", ARAWBMessageConstants.Name, response.Name));
			note.AppendLine();
			note.Append(ZString.Format("{0} : {1}", ARAWBMessageConstants.Status, response.Status));
			note.AppendLine();

			if (errors.Count > 0)
			{
				foreach (var error in errors)
				{
					note.Append(ZString.Format("{0} : {1} {2} {3}", ARAWBMessageConstants.CodeOfError, error.Code, ARAWBMessageConstants.DescriptionError, error.Value));
					note.AppendLine();
				}
			}
			return note.ToString().Trim();
		}

		#endregion

		public static EDIInterchange GetSentInterchangeWithTrackingId(ZGuid eHubTrackingID, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, eHubTrackingID);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			return factory.LoadTop1<EDIInterchange>(query);
		}

		public static EDIMessage GetOriginalMessage(ZGuid interchangePK, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIMessageSchema.EM_EI, interchangePK);
			return factory.LoadTop1<EDIMessage>(query);
		}
	}
}
