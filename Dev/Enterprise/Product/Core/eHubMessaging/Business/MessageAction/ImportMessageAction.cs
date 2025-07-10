using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business
{
	public abstract class ImportMessageAction : MessageAction, IOnlySaveDataWhenNoRecordsHaveErrors
	{
		protected ImportMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected internal override bool ExecuteActionCore(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant)
		{
			notifications.Notify(new InfoNotification(Res.GetString("b12a8162-b67e-4bf3-bbe9-10cc22638b13", "Combining message text")));
			var messageText = GetMessageText(message);

			if (!String.IsNullOrEmpty(messageText))
			{
				byte[] messageBytes = MessageEncoding.UTF8WithoutBOM.GetBytes(messageText);
				using (var messageStream = new MemoryStream(messageBytes))
				{
					var importer = GetDataImporter();

					notifications.Notify(new InfoNotification(Res.GetString("4d1ef6f6-6d3d-4c89-adbf-d8a1ce9fcc68", "Running import")));

					importer.OnlySaveDataWhenNoRecordsHaveErrors = this.OnlySaveDataWhenNoRecordsHaveErrors;

					ITransactionParticipant[] forSave;
					var result = ImportData(importer, new StreamReader(messageStream, MessageEncoding.UTF8WithoutBOM), notifications, message, out forSave);

					participant = new List<ITransactionParticipant>();
					participant.AddRange(forSave);

					if (result)
					{
						notifications.Notify(new InfoNotification(Res.GetString("50a5426b-f805-44a6-ad64-76c88dcf973b", "Link message to Job")));
						LinkMessageToImportedBusinessObjects(message, importer.ImportedBusinessObjects);
					}

					notifications.Notify(new InfoNotification(Res.GetString("cd6bf8ba-acfc-46c0-bde2-c73938704989", "Import finished")));

					return result;
				}
			}
			else
			{
				participant = new List<ITransactionParticipant>();
				notifications.Notify(new WarningNotification(WarningType.Warning, Res.GetString("8b0c32c4-7b49-4912-b767-0821b5013dde", "Message is empty.")));
				return false;
			}
		}

		string GetMessageText(EDIMessage message)
		{
			var messageText = String.Empty;
			var interchange = message.Interchange;

			if (interchange != null)
			{
				var subTypeList = new EDIMessageSubTypeList();

				if (interchange.EI_InterchangeType == EDIInterchangeTypeList.Codes.XMS
					&& !String.IsNullOrEmpty(message.EM_MessageSubType)
					&& subTypeList.ContainsCode(message.EM_MessageSubType)
					&& message.EM_MessageSubType != EDIMessageSubTypeList.Codes.Unknown)
				{
					messageText = CreateInterchangeText(message);
				}
				else
				{
					messageText = interchange.EI_HeaderText + message.EM_MessageText + interchange.EI_FooterText;
				}
			}
			else
			{
				messageText = message.EM_MessageText;
			}

			return messageText;
		}

		protected virtual bool ImportData(IDataImporterControllingSave dataImporter, TextReader reader, INotifications notifications, EDIMessage message, out ITransactionParticipant[] forSave)
		{
			return dataImporter.ImportDataToFactory(reader, string.Empty, notifications, message, out forSave);
		}

		void LinkMessageToImportedBusinessObjects(EDIMessage message, BusinessObject[] importedBusinessObjects)
		{
			if (importedBusinessObjects != null && importedBusinessObjects.Length > 0)
			{
				foreach (var bizObj in importedBusinessObjects.Where(bizO => !(bizO is StmALog)))
				{
					new MessageDataExportImportLogLinker(Events.DataImport, FactoryProvider.Current).LinkMessageToParentBOLogs(message, bizObj as IStmALogParent);
				}
			}
		}

		protected internal abstract IDataImporterControllingSave GetDataImporter();

		string CreateInterchangeText(EDIMessage message)
		{
			message.Interchange.UpdateInterchangeHeaderText();
			var outputString = new StringBuilder();

			using (var writer = new XmlTextWriter(new StringWriter(outputString, CultureInfo.InvariantCulture)))
			{
				var interchangeHeader = message.Interchange.EI_HeaderText;
				writer.WriteStartElement("XmlInterchange");
				writer.WriteRaw(interchangeHeader.ToString());
				writer.WriteStartElement("Payload");
				writer.WriteStartElement(message.GetLocalTagName());
				writer.WriteRaw(message.EM_MessageText);
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndElement();
			}

			return outputString.ToString();
		}

		#region IOnlySaveDataWhenNoRecordsHaveErrors Members

		public bool OnlySaveDataWhenNoRecordsHaveErrors { get; set; }

		#endregion
	}

	static class EDIInterchangeExtensions
	{
		public static void UpdateInterchangeHeaderText(this EDIInterchange interchange)
		{
			if (interchange != null && interchange.EI_InterchangeType == EDIInterchangeTypeList.Codes.XMS && interchange.EI_HeaderText.IsEmpty && !interchange.HeaderTextIsUpdated)
			{
				interchange.HeaderTextIsUpdated = true;
				using (var bodyStream = interchange.GetEI_BodyTextReader().CopyAndDispose())
				{
					interchange.UpdateInterchangeHeaderTextUsingXPathReaderAndDisposeStream(bodyStream);
				}
			}
		}

		public static void UpdateInterchangeHeaderTextUsingXPathReaderAndDisposeStream(this EDIInterchange interchange, Stream bodyStream)
		{
			using (var reader = new XPathReader(new XmlTextReader(bodyStream), XmlInterchangeInfoXPathCollection))
			{
				if (reader.ReadUntilMatch())
				{
					using (var streamReader = new StreamReader(LargeMessageHelper.GetStreamFromNode(reader)))
					{
						interchange.EI_HeaderText = streamReader.ReadToEnd();
					}
				}
			}
		}

		static XPathCollection XmlInterchangeInfoXPathCollection
		{
			get
			{
				if (xmlInterchangeInfoXPathCollection == null)
				{
					xmlInterchangeInfoXPathCollection = new XPathCollection();
					xmlInterchangeInfoXPathCollection.Add((NoResString)"/*[local-name()='XmlInterchange']/*[local-name()='InterchangeInfo']");
				}
				return xmlInterchangeInfoXPathCollection;
			}
		}

		[ThreadStatic]
		static XPathCollection xmlInterchangeInfoXPathCollection;
	}
}
