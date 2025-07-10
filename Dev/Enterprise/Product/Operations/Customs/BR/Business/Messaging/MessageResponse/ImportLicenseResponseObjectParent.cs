using System;
using System.IO;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.BatchProcessor.LoggingInformation;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public interface IImportLicenseResponseObjectParent : IXmlLoadingObjectParent<ImportLicenseResponseObjectCollection>
	{
		bool ResponseHasDiagnosis { get; }
		bool ResponseHasReferenceNumber { get; }
		bool ResponseHasStatus { get; }
	}

	public abstract class ImportLicenseResponseObjectParent<T> : XmlLoadingObjectParent<T, ImportLicenseResponseObjectCollection>, IImportLicenseResponseObjectParent
	{
		public ImportLicenseResponseObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		protected ZString responseText;

		public override ImportLicenseResponseObjectCollection Collection
		{
			get
			{
				if (importLicenseResponseObjects == null)
				{
					importLicenseResponseObjects = new ImportLicenseResponseObjectCollection(Factory);
					RegisterEditableChildObject(importLicenseResponseObjects);
				}
				return importLicenseResponseObjects;
			}
		}

		ImportLicenseResponseObjectCollection importLicenseResponseObjects;

		protected CusEntryHeader[] LoadRelatedEntries(string applicationReference)
		{
			return Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(x => x.FindImportLicenceMessage(applicationReference) != null).ToArray();
		}

		protected abstract ZString InterchangeType { get; }

		protected abstract IInboundMessageCreator GetMessageCreator(LoggingInformation logger);

		protected abstract BRCResponseMessageProcessor GetMessageProcessor(LoggingInformation logger);

		public virtual bool ResponseHasDiagnosis => false;

		public virtual bool ResponseHasReferenceNumber => false;

		public virtual bool ResponseHasStatus => false;

		protected override void LoadObjects(T responseData) { }

		public override bool CreateDataFromXml()
		{
			var result = false;

			if (hasValidResponse)
			{
				var factory = new BusinessObjectFactory();
				var interchange = CreateInterchangeAndProcessMessages(factory);

				try
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, attempts: 0);
				}
				catch (ZSaveException)
				{
					AddLog(100, 100, Res.GetString("F8F75648-DA32-473C-BF1A-E74CE6856810", "Cannot save the response successfully."));
				}

				result = interchange.IsInDatabase;
			}
			else
			{
				AddLog(100, 100, Res.GetString("41183B8C-E244-4678-BA5F-0C6C02D0417C", "Valid response file has not been loaded."));
			}

			return result;
		}

		protected override T ReadFile(Stream stream)
		{
			T loteli = default;
			using (var reader = new StreamReader(stream))
			{
				try
				{
					responseText = reader.ReadToEnd();
					loteli = XmlObjectSerializer.Deserialize<T>(responseText);
				}
				catch (Exception)
				{
					responseText = ZString.Empty;
				}
			}

			return loteli;
		}

		BREDIInterchange CreateInterchangeAndProcessMessages(BusinessObjectFactory factory)
		{
			var completedCount = 0;
			var totalCount = 0;

			var logger = new LoggingInformation();
			logger.OnLogInfoAdded += new LogInfoAdded((string log, LogType logType) => AddLog(completedCount, totalCount, $"{logType}: {log}"));

			var interchange = factory.New<BREDIInterchange>();
			interchange.EI_InterchangeType = InterchangeType;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = BREDIInterchange.BRCustoms;
			interchange.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			interchange.EI_Status = EDIMessageStatusList.Codes.Queued;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_BodyText = responseText;

			var messageCreator = GetMessageCreator(logger);

			if (messageCreator != null)
			{
				messageCreator.CreateMessagesForInterchange(interchange);
				totalCount = interchange.ContainedMessages.Count;

				if (totalCount > 0)
				{
					var messageProcessor = GetMessageProcessor(logger);
					if (messageProcessor != null)
					{
						foreach (var message in interchange.ContainedMessages.Cast<EDIMessage>())
						{
							messageProcessor.PreProcessMessage(message);
							messageProcessor.ProcessMessage(message);
							AddLog(completedCount++, totalCount, Res.GetString("f37bd333-b9a3-4659-9b18-7b6dce6f5763", "Response Message {0} processed.", message.EM_MessageNum));
						}
					}

					interchange.EI_Status = EDIInterchange.Status.Received;
				}
				else
				{
					interchange.EI_Status = EDIInterchange.Status.Failed;
				}
			}

			return interchange;
		}
	}
}
