using System;
using System.IO;
using System.Net.Http;
using System.Xml;
using CargoWise.Customs.CA.MessageDefinitions.CAD.Outbound;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class QueryCADMessageManager
	{
		public QueryCADMessageManager(CusEntryHeader entry, HttpClient httpClient = null)
		{
			this.entry = entry;
			this.httpClient = httpClient;
		}
		readonly CusEntryHeader entry;
		readonly HttpClient httpClient;

		public void Validate()
		{
			var carmAPIKey = CACustomsDataRegistry.Instance.CARMAPIKey;
			var carmEndPoint = CACustomsDataRegistry.Instance.CARMEndPoint;
			if (entry == null || !entry.CH_EntrySubmittedDate.IsValid)
			{
				validateFailed = true;
				Globals.Message.ShowError(Res.GetString("CB070D11-8442-4C59-A05E-217A07809BD0", "Entry should have a valid CAD Submitted date."));
			}
			else if (string.IsNullOrEmpty(carmAPIKey.Value) || string.IsNullOrEmpty(carmEndPoint.Value) || !Uri.IsWellFormedUriString(carmEndPoint.Value, UriKind.Absolute))
			{
				validateFailed = true;
				Globals.Message.ShowError(Res.GetString("4BB059FB-D8A4-45A5-92AC-BBD7FD9DC171", "CARM API Key and CARM End Point need to have a valid value, please setup in {0} and {1}.",
					carmAPIKey.GetLocationInEnglish(), carmEndPoint.GetLocationInEnglish()));
			}
		}

		public void Query()
		{
			if (cancelQuery || validateFailed)
			{
				return;
			}
			FireAction(QueryStart, 50, "CAD Query Start");
			var result = QueryCADMessageService.Instance.Value.QueryCADMessage(entry.CH_BGMReference, GetHttpClient);

			if (cancelQuery)
			{
				FireAction(QueryFinished, 100, "User Cancelled");
				return;
			}

			var responseContent = result.ResponseContent;
			if (string.IsNullOrEmpty(responseContent) || !CanDeserialize(responseContent))
			{
				FireAction(QueryFinished, 100, "CAD Query Failed:" + responseContent);
				Globals.Message.ShowError(Res.GetString("34CC25CD-52FD-4B7A-B10C-969BF07FDF26", "CAD Query Failed, Response results '{0}'.", responseContent));
				return;
			}
			else
			{
				CreateMessage(result.QueryContent, responseContent);
			}
		}

		static ZBool CanDeserialize(ZString messageContent)
		{
			using (var stream = new StringReader(messageContent))
			{
				using (var reader = new XmlTextReader(stream))
				{
					var serializer = ZXmlSerializer.New(typeof(DocumentMetaData));
					bool result;
					try
					{
						result = serializer.CanDeserialize(reader);
					}
					catch (XmlException)
					{
						result = false;
					}

					return result;
				}
			}
		}

		protected void FireAction(ProcessStatusEventHandler handler, int percentage, ZString message)
		{
			if (handler != null)
			{
				handler(percentage, message);
			}
		}

		public void CancelQuery()
		{
			cancelQuery = true;
		}
		bool cancelQuery;
		bool validateFailed;

		void CreateMessage(string queryMessageText, string responseMessageText)
		{
			queryMessage = entry.Factory.New<CADMessage>();
			queryMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			queryMessage.EM_MessageSubType = MessageTypeList.Codes.Query;
			queryMessage.EM_Status = EDIMessage.Status.Sent;
			queryMessage.EM_MessageText = queryMessageText;

			responseMessage = entry.Factory.New<CADMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageSubType = MessageTypeList.Codes.Query;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageText = responseMessageText;
			entry.Messages.Add(queryMessage);
			entry.Messages.Add(responseMessage);

			entry.Factory.Saved -= Factory_Saved;
			entry.Factory.Saved += Factory_Saved;
			try
			{
				entry.Factory.Save();
				FireAction(QueryFinished, 100, "CAD Query Complete");
				Globals.Message.ShowInformation(Res.GetString("BDCBA52F-D01B-4E9E-A8E6-36B825ABE4F2", "CARM Query message sent, please reload this declaration after a while waiting for the service task to process the response."));
			}
			catch (ZSaveException e)
			{
				FireAction(QueryFinished, 100, "CAD Query Failed" + e.Message);
				Globals.Message.ShowError(Res.GetString("A5826CC5-8C43-44AE-9E5B-53716B5057E5", "CAD Query Failed, {0}.", e.Message));
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				responseMessage.EM_MessageNum = queryMessage.EM_MessageNum;
			}
		}

		EDIMessage queryMessage;
		EDIMessage responseMessage;

		public virtual HttpClient GetHttpClient => httpClient;

		public delegate void ProcessStatusEventHandler(int recordsToExport, string message);

		public event ProcessStatusEventHandler QueryStart;
		public event ProcessStatusEventHandler QueryFinished;
	}
}
