using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public class NettingClearingJournalExporter : INettingClearingJournalExporter
	{
		public NettingClearingJournalExporter(BusinessObjectFactory factory, INotifications notifications, NettingStatement statement)
		{
			Factory = factory;
			Notification = notifications;
			Statement = statement;
		}

		readonly BusinessObjectFactory Factory;
		readonly INotifications Notification;
		readonly NettingStatement Statement;

		public bool ExportClearingJournal(OrgHeader nettingSystemOrgHeader, ZString participantEHubID, ZString csvContent)
		{
			try
			{
				var csvFileConverter = new PaymentReceiptRemittanceFileConverter(Notification, Factory);
				var xsd = csvFileConverter.ConvertCSVContentToXsd(csvContent);

				using (var stream = (SubStreamableStream)new MemoryStream())
				using (XmlTextWriter writer = new XmlTextWriter(stream, System.Text.Encoding.UTF8))
				{
					writer.Formatting = Formatting.Indented;
					WriteXml(stream, writer, xsd);

					var context = new DeliveryContext(Factory)
					{
						ParentInfo = EntityInfo.New(Statement),
						ApplicationCode = ApplicationCodeList.Codes.XMS,
						MessageTypeCode = EDIMessageTypeList.Codes.XMS,
						MessageSubTypeCode = EDIMessageSubTypeList.Codes.NettingClearingJournals,
						Notifications = Notification,
					};

					var delivery = new EDIMessageDelivery();
					var communicationModes = nettingSystemOrgHeader.EDICommunicationsModes.FindByModuleAndFileFormat(EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction);

					var recipientCommunicationMode = communicationModes.FirstOrDefault(x => x.EK_Destination == participantEHubID);
					if (recipientCommunicationMode != null)
					{
						delivery.Deliver(context, recipientCommunicationMode, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
					}
					else
					{
						throw new IncorrectDataSetupException(Res.GetString("836a253c-9e06-43a2-a01a-594597467743", "No EDI Communications setup found for E Hub ID: '{0}' in Netting Center Organization.", participantEHubID));
					}
				}
			}
			catch (IncorrectDataSetupException ex)
			{
				Notification.AddError(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notification.AddError(Res.GetString("973b76c9-d60e-44f5-92cf-86ead23b5d6b",
					"Could not send Netting Journal Clearing file to '{0}'.\r\nError Details: {1}", participantEHubID, ex.Message));

				throw;
			}

			return true;
		}

		Xsd.XmlInterchange CreateInterchange()
		{
			Xsd.XmlInterchange result = new Xsd.XmlInterchange();
			result.InterchangeInfo = new Xsd.InterchangeInfo();
			result.Version = "1";

			var currentCompany = Env.CurrentCompany;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			OrgHeader organisation = Factory.Load<OrgHeader>(currentCompany.OrganisationPK);
			result.InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(organisation, new ValueObjectExportContext(Notification));
			result.InterchangeInfo.Source = new Xsd.InterchangeInfoSource();
			result.InterchangeInfo.Source.CompanyCode = currentCompany.Code;
			result.InterchangeInfo.Source.EnterpriseCode = registrationKey.EnterpriseCode;
			result.InterchangeInfo.Source.OriginServer = registrationKey.ServerCode;
			result.InterchangeInfo.Source.LoginName = GlbStaff.CurrentUser.GS_LoginName;
			result.InterchangeInfo.Date = ZDateTime.Now.ToDateTime();

			result.PayloadSpecified = false;

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void WriteXml(Stream stream, XmlTextWriter xmlWriter, IValueObject xsd)
		{
			Xsd.XmlInterchange interchange = CreateInterchange();
			XmlValueObjectSerializer interchangeSerializer = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
			interchangeSerializer.Serialize(xmlWriter, interchange);
			xmlWriter.Flush();

			stream.Position = 0;

			StreamReader reader = new StreamReader(stream);
			string interchangeString = reader.ReadToEnd();
			int index = interchangeString.LastIndexOf(InterchangeInfoCloseTagRaw);

			ToWriteWhenDone = interchangeString.Substring(index + InterchangeInfoCloseTagRaw.Length);

			byte[] toWriteWhenDoneAsBytes = reader.CurrentEncoding.GetBytes(ToWriteWhenDone);

			stream.Position -= toWriteWhenDoneAsBytes.Length;

			xmlWriter.WriteRaw("<Payload><NettingClearingJournals xmlns=\"http://www.edi.com.au/EnterpriseService/\">");
			xmlWriter.Flush();

			using (var tempStream = new MemoryStream())
			{
				XmlTextWriter writer = new XmlTextWriter(new StreamWriter(tempStream));
				writer.Formatting = Formatting.Indented;

				var serializer = new XmlValueObjectSerializer(typeof(Xsd.FinancialInvoices));
				serializer.Serialize(tempStream, xsd);

				tempStream.Position = 0;

				StreamReader invoiceReader = new StreamReader(tempStream);
				string data = invoiceReader.ReadToEnd();
				data = data.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", string.Empty);
				data = data.Replace("xmlns=\"http://www.edi.com.au/EnterpriseService/\"", string.Empty);
				data = data.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", string.Empty);
				data = data.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", string.Empty);

				xmlWriter.WriteRaw(data);
				xmlWriter.Flush();
			}

			xmlWriter.WriteRaw("</NettingClearingJournals></Payload>" + ToWriteWhenDone);
			xmlWriter.Flush();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		const string InterchangeInfoCloseTagRaw = "</InterchangeInfo>";
		string ToWriteWhenDone = string.Empty;
	}
}
