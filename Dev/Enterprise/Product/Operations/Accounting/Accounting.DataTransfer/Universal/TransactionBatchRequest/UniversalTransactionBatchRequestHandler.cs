using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.AccBatchRequest;
using Enterprise.Accounting.DataTransfer.AccBatchRequets;
using Enterprise.Accounting.Export.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using static Enterprise.Accounting.DataTransfer.AccBatchRequest.AccountingTransactionEAdaptorExporter;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class UniversalTransactionBatchRequestHandler : IHttpXmlMessageHandler
	{
		readonly IXmlSessionTracker xmlSessionTracker;

		public IHttpXmlProcessingConfig ProcessingConfig { get; }

		public UniversalTransactionBatchRequestHandler(IXmlSessionTracker xmlSessionTracker) : this(xmlSessionTracker, new DefaultProcessingConfig())
		{
		}

		public UniversalTransactionBatchRequestHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig)
		{
			this.xmlSessionTracker = xmlSessionTracker;
			this.ProcessingConfig = processingConfig;
		}

		public IHttpXmlRequestResponse CreateRequestMessage(DisposableManager manager = null)
		{
			var factory = new BusinessObjectFactory();
			if (manager != null)
			{
				factory.AddDisposableService(manager);
			}
			var result = factory.New<IHttpXmlEDIMessage>();
			result.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataQuery;
			result.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			result.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			result.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatchRequest;
			result.EM_Status = EDIMessageStatusList.Codes.Recognised;
			return result;
		}

		public IHttpXmlRequestResponse CreateResponseMessage(DisposableManager manager = null)
		{
			var factory = new BusinessObjectFactory();
			if (manager != null)
			{
				factory.AddDisposableService(manager);
			}
			var result = factory.New<IHttpXmlEDIMessage>();
			result.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataQuery;
			result.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			result.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			result.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			result.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			return result;
		}

		public IHttpXmlProcessingResult Process(IHttpXmlRequestResponse requestMessage, IHttpXmlMessageSaver messageSaver = null)
		{
			return Process(requestMessage.GetMessageStream(), requestMessage, messageSaver);
		}

		public IHttpXmlProcessingResult Process(SubStreamableStream stream, IHttpXmlRequestResponse requestMessage, IHttpXmlMessageSaver messageSaver = null)
		{
			var result = new HttpXmlProcessingResult();
			result.Status = EDIMessageStatusList.Codes.Error;

			var parseData = stream.ParseAndReturnMoreData<TransactionBatchRequest>();
			var transactionBatchRequest = parseData.DataObject;
			var nameSpace = parseData.NamespaceUsed;
			var batchResponse = GetBatchResponse(transactionBatchRequest, nameSpace);

			if (batchResponse != null && string.IsNullOrEmpty(batchResponse.ErrorMessage))
			{
				result.Status = EDIMessageStatusList.Codes.ProcessedOK;
				if (batchResponse.DataObject != null)
				{
					result.ResponseMessageText = new CargoWise.IO.Shim.SubStreamableStream();
					new XmlWriter().WriteXML(batchResponse.DataObject, result.ResponseMessageText, false);
				}
			}

			messageSaver?.Save(result);

			return result;
		}

		BatchResponse GetBatchResponse(TransactionBatchRequest transactionBatchRequest, string nameSpace)
		{
			BatchResponse batchResponse = null;
			var writerStrategy = new AccountingTransactionDataObjectWriterStrategy(context: "eAdaptor");
			var companyCode = GetCompanyCode(transactionBatchRequest.DataContext);
			if (companyCode.IsEmpty)
			{
				return batchResponse;
			}
			var actionType = transactionBatchRequest.ActionType;
			if (actionType == null)
			{
				NotifyErrorOrInfo(Res.GetString("C0BA0E4C-F919-43df-9333-AD5B9E993AAE", "Action Type is not specified"));
				return batchResponse;
			}
			var exporter = GetExporter();

			switch (actionType.Code)
			{
				case AccBatchRequestTypePairList.Codes.Create:
					batchResponse = exporter.CreateBatch(writerStrategy, companyCode, nameSpace);
					NotifyErrorOrInfo(batchResponse.ErrorMessage, batchResponse.InfoMessage);
					break;
				case AccBatchRequestTypePairList.Codes.Export:
					int batchNumber;
					var dataTargetCollection = transactionBatchRequest.DataContext.DataTargetCollection;
					if (dataTargetCollection == null)
					{
						NotifyErrorOrInfo(Res.GetString("807CBEA2-D442-452F-9B81-EFF995DF95BD", "Data Target Collection is empty"), string.Empty);
					}
					else if (dataTargetCollection.FirstOrDefault() != null && Int32.TryParse(dataTargetCollection.FirstOrDefault().Key, out batchNumber))
					{
						batchResponse = exporter.ExportBatch(writerStrategy, companyCode, batchNumber, nameSpace);
						NotifyErrorOrInfo(batchResponse.ErrorMessage, batchResponse.InfoMessage);
					}
					else
					{
						NotifyErrorOrInfo(Res.GetString("28da0815-2a1b-4f86-838c-95d2ffeae3ce", "Invalid Batch Number"), string.Empty);
					}

					break;
				case AccBatchRequestTypePairList.Codes.CreateAndExport:
					batchResponse = exporter.CreateAndExportBatch(writerStrategy, companyCode, nameSpace);
					NotifyErrorOrInfo(batchResponse.ErrorMessage, batchResponse.InfoMessage);
					break;
				default:
					NotifyErrorOrInfo(Res.GetString("581f9bf7-a545-47dd-ad54-bde61fce9de0", "Not supported Action Type"));
					break;
			}

			return batchResponse;
		}

		ZString GetCompanyCode(IDataContextDataObject dataContext)
		{
			if (dataContext == null)
			{
				NotifyErrorOrInfo(Res.GetString("8D5B6720-3F8A-419A-A55F-5E2E433F6FA8", "Could not get DataContext from Universal Transaction Batch Request"));
				return ZString.Empty;
			}
			var companyCode = dataContext.GetEnterpriseServerAndCompanyIDs().CompanyCode;
			return companyCode;
		}

		protected virtual AccountingTransactionEAdaptorExporter GetExporter()
		{
			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)Db.Connection).ADOConnection, null);
			var exporter = new AccountingTransactionEAdaptorExporter(dataAccess);
			return exporter;
		}

		void NotifyErrorOrInfo(string errorMessage, string infoMessage = null)
		{
			if (!string.IsNullOrEmpty(infoMessage))
			{
				xmlSessionTracker.Log(LogType.Information, infoMessage);
			}
			if (!string.IsNullOrEmpty(errorMessage))
			{
				xmlSessionTracker.Log(LogType.Error, errorMessage);
			}
		}
	}
}
