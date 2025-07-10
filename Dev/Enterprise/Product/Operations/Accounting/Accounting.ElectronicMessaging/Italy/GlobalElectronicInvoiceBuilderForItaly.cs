using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal;
using Enterprise.Messaging.Integration;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class GlobalElectronicInvoiceBuilderForItaly : IGlobalElectronicInvoiceBuilder
	{
		public GlobalElectronicInvoiceBuilderForItaly(string batchNumber, UniversalTransactionBatch universalTransactionBatch)
		{
			Argument.NotNull(universalTransactionBatch, "universalTransactionBatch");
			Argument.NotNullOrEmpty(batchNumber, "batchNumber");

			BatchNumber = batchNumber;
			Batch = universalTransactionBatch;
		}

		UniversalTransactionBatch Batch { get; }

		string BatchNumber { get; }

		public (GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) Create()
		{
			var errorNotifications = new Logger();
			var warningNotifications = new Logger();

			var (branch, loaderError) = new TransactionBatchDataLoader().LoadBranchAndCompany(Batch);
			if (!string.IsNullOrEmpty(loaderError))
			{
				errorNotifications.AddError(loaderError);
				return (null, errorNotifications, warningNotifications);
			}

			var eInvoice = new GlobalElectronicInvoicing()
			{
				Header = new GlobalElectronicInvoicingHeader()
				{
					ElectronicInvoiceBatchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest()
					{
						MessagingSystem = ITALY_ELECTRONIC_INVOICING,
						BatchNumber = BatchNumber,
						MessageType = WS_REQUEST,
						IsProductionSystem = GEIMessageHelper.GetIsProductionSystem(branch.Company),
						IsProductionSystemSpecified = true,
						AdditionalDataItems = GetAdditionalHeaderDataItems()
					}
				},
				Payload = GetPayloadXML(errorNotifications, warningNotifications),
				TransactionBatchSpecified = false,
			};

			Validate(eInvoice, errorNotifications, warningNotifications);

			return (eInvoice, errorNotifications, warningNotifications);
		}

		GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems()
		{
			var result = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();

			var issuerRegistrationNumber = FatturaElettronicaDataHelper.getIssuerRegistrationNumber(Batch.TransactionCollection.First());
			AddItem(FatturaElettronicaDataHelper.IssuerRegistrationNumber, issuerRegistrationNumber);

			return result;

			void AddItem(ZString key, ZString value)
			{
				var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = key, Value = value };
				result.Add(item);
			}
		}

		ZString GetPayloadXML(INotifications errorNotifications, INotifications warningNotifications)
		{
			using (var memoryStream = new MemoryStream())
			{
				new FatturaElettronicaXmlWriter().WriteXmlToStream(Batch, memoryStream, errorNotifications, warningNotifications);
				memoryStream.Position = 0;
				using (var reader = new StreamReader(memoryStream, true))
				{
					return reader.ReadToEnd();
				}
			}
		}

		void Validate(GlobalElectronicInvoicing invoice, INotifications errorNotifications, INotifications warningNotifications)
		{
			using (var ms = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(invoice.Payload)))
			{
				var xsdValidaiton = new FatturaElettronicaXsdValidation(errorNotifications);
				var xmlDoc = xsdValidaiton.ValidateXml(ms);
				new FatturaElettronicaPolicyValidation(errorNotifications, warningNotifications, xmlDoc, Batch).ValidateXml();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in XML file. Not a user visible text.")]
		const string ITALY_ELECTRONIC_INVOICING = "Italy electronic invoicing system";
		[EInvoiceMessageSubType]
		public const string WS_REQUEST = EInvoiceAPICommandList.Codes.Request; // Constant string used in XML file. Not a user visible text.
	}
}
