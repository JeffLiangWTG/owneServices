using System;
using System.IO;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(Enterprise.CustomerService.Business.ServiceRequestResponsesProcessor))]

namespace Enterprise.CustomerService.Business
{
	public class ServiceRequestResponsesProcessor
	{
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^" + ServiceRequestResponsesSubject)]
		public bool ProcessServiceRequestResponse(MailItem item, ILogger logger)
		{
			return ProcessMailItem(item, message => logger.Log(LogType.Information, message));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute argument")]
		public const string ServiceRequestResponsesSubject = "Customer Service Incident Raised - Your Ref";

		static bool ProcessMailItem(MailItem item, Action<string> logAction)
		{
			if (item.MailAttachments.Count > 0)
			{
				ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));
				Xsd.CustomerServiceResponse response = null;
				using (MemoryStream ms = new MemoryStream(item.MailAttachments[0].MA_Data))
				{
					try
					{
						response = (Xsd.CustomerServiceResponse)serializer.Deserialize(ms);
					}
					catch (InvalidOperationException ex)
					{
						throw new InvalidOperationException("Deseralize xml data failed, the xml data: " + item.MailAttachments[0].MA_Data.ToAscii() + System.Environment.NewLine + ex.Message, ex);
					}
				}

				if (null != response && CustomerServiceResponseMessageAction.ProcessAndSave(response))
				{
					logAction(Res.GetString("59fcd625-1bc8-4d04-8f84-c2e8dcab1d1c", "Processed Incident Approval {0} - Incident {1}", response.ClientReferenceNumber, response.IncidentNumber));
					return true;
				}
			}

			return false;
		}
	}
}
