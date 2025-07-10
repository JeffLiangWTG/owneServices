using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSResponseMessageProcessor : ApplicationTypeMessageProcessor
	{
		public GVMSResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "GVMS Response Message";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.GbCustomsGVMSManifest;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is GVMSEDIMessage gvmMessage)
			{
				gvmMessage.EM_Status = ProcessMessageCore(gvmMessage);
			}
		}

		ZString ProcessMessageCore(GVMSEDIMessage gvmsMessage)
		{
			var messageStatus = EDIMessageStatusList.Codes.Discarded;

			var messageDataObject = gvmsMessage.MessageDataObject;
			if (messageDataObject != null)
			{
				var header = gvmsMessage.EM_LinkedObject as AsycudaManifestHeader ?? messageDataObject.GetMainfest(gvmsMessage.Factory);
				if (header == null)
				{
					var gvmsNotificationMessageID = gvmsMessage.Interchange?.GBCustomsBusinessResponse.MessageId;
					if (gvmsNotificationMessageID.HasValue)
					{
						header = GetManifestFromGvmsNotificationMessageID(gvmsNotificationMessageID.Value.Replace("-", ""), gvmsMessage.Factory);
					}
				}

				if (header == null)
				{
					if (gvmsMessage.Interchange == null || gvmsMessage.Interchange.EI_RetryCount > 10)
					{
						messageStatus = EDIMessageStatusList.Codes.Failed;
					}
					else
					{
						gvmsMessage.Interchange.EI_RetryCount++;
						gvmsMessage.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(1);
						ServiceTaskHelper.NudgeServiceTaskDelay(null, Constants.GVMSMessageRetrieverServiceTaskCode, TimeSpan.FromMinutes(1));
						messageStatus = EDIMessageStatusList.Codes.Queued;
					}
				}
				else
				{
					var oldRegistrationStatus = header.RegistrationStatus;
					var gmrId = messageDataObject.gmrId;
					var oldGmrId = header.RegistrationNumber;
					if (oldGmrId.IsEmpty)
					{
						header.RegistrationNumber = gmrId;
					}
					else if (!string.IsNullOrEmpty(gmrId) && oldGmrId != gmrId)
					{
						Logger.Log(string.Format("Updating GMR ID from {0} to {1} on {2}", oldGmrId, gmrId, header.AMA_JobReference));
						header.RegistrationNumber = gmrId;
					}

					header.RegistrationStatus = new GVMSRegistrationStatus().GetCodeFromDescription(messageDataObject.gmrState ?? string.Empty);
					if (!header.RegistrationStatus.IsEmpty && oldRegistrationStatus != header.RegistrationStatus)
					{
						ExecuteGVMSManifestDocument(header);
					}

					header.InspectionRequired = messageDataObject.inspectionRequired;
					if (messageDataObject.reportToLocations?.Count > 0)
					{
						header.InspectionLocations.RemoveAndDeleteAll();
						foreach (var inspection in messageDataObject.reportToLocations)
						{
							foreach (var locationId in inspection.locationIds)
							{
								var inspectionLocation = header.InspectionLocations.AddNew();
								inspectionLocation.CY_Code = inspection.inspectionTypeId;
								inspectionLocation.CY_Data = locationId;
							}
						}
					}

					if (ZDateTime.TryParseExact(messageDataObject.createdDateTime, out var date, "yyyy-MM-ddTHH:mm:ss.fffZ"))
					{
						header.RegistrationDate = date.IsEmpty ? date : new ZDateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond), date.Kind);
					}

					if (gvmsMessage.EM_LinkedObject == null)
					{
						gvmsMessage.EM_LinkedObject = header;
					}
					messageStatus = EDIMessageStatusList.Codes.ProcessedOK;
				}
			}
			return messageStatus;
		}

		void ExecuteGVMSManifestDocument(AsycudaManifestHeader header)
		{
			try
			{
				var documentFactory = header.DocManagerInfo.MasterFactory;
				var storageMain = documentFactory.RetrieveExistingOrCreateStorageMain(header, Core.Constants.DocManagerCodes.AsycudaManifest);
				var template = ExcelTemplateRetriever.GetTemplate("GvmsManifestWithBarcode", AsycudaManifestHeaderDocumentSupporter.AsycudaManifestHeader, header.Factory);
				var docDataProvider = BODocDataProvider.Get(header);
				using (var report = new Report(null, template, docDataProvider, template.TemplateName, null, DocumentDirection.ANY, false))
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					var binaryData = DocumentConverter.ConvertFromExcel(outputStream.ToArray(), OutputFormatType.PDF, ColourDepth.BlackAndWhite);
					storageMain.AddFileOrDocument(binaryData, "GB GVMS Manifest with Barcode - " + header.AMA_JobReference + ".pdf", Core.Constants.RefDocTypes.EntryPrint, false);
					documentFactory.Save();
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Logger.Log("The GVMS Manifest documents have failed to be generated due to the following error : " + e.Message); // Log entries are not localized
			}
		}

		static AsycudaManifestHeader GetManifestFromGvmsNotificationMessageID(ZString gvmsNotificationMessageIDWithoutHyphens, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, gvmsNotificationMessageIDWithoutHyphens);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbCustomsGVMSManifest);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			var message = factory.LoadTop1<GVMSEDIMessage>(query);

			var manifestHeader = message?.EM_LinkedObject as AsycudaManifestHeader;
			return manifestHeader;
		}
	}
}
