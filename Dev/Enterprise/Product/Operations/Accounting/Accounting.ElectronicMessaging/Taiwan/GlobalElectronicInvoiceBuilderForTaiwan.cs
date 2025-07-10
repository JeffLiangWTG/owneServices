using System;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan
{
	public class GlobalElectronicInvoiceBuilderForTaiwan : IGlobalElectronicInvoiceBuilder
	{
		public GlobalElectronicInvoiceBuilderForTaiwan(string batchNumber, ComplianceDocumentBatch complianceDocumentBatch)
		{
			Argument.NotNull(complianceDocumentBatch, "complianceDocumentHeaders");
			Argument.NotNullOrEmpty(batchNumber, "batchNumber");

			BatchNumber = batchNumber;
			ComplianceDocumentBatch = complianceDocumentBatch;
		}

		ComplianceDocumentBatch ComplianceDocumentBatch { get; }

		string BatchNumber { get; }

		public (GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) Create()
		{
			var errorsNotifications = new Logger();
			var warningsNotifications = new Logger();

			var eInvoice = new GlobalElectronicInvoicing()
			{
				Header = new GlobalElectronicInvoicingHeader()
				{
					ElectronicInvoiceBatchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest()
					{
						MessagingSystem = TAIWAN_ELECTRONIC_INVOICING,
						BatchNumber = BatchNumber,
						MessageType = WS_REQUEST,
						FileName = GetFileName(ComplianceDocumentBatch),
						IsProductionSystem = GEIMessageHelper.GetIsProductionSystem(GEIMessageCompany),
						IsProductionSystemSpecified = true,
					}
				},
				Payload = GetPayloadFlatFile(errorsNotifications),
				TransactionBatchSpecified = false,
			};

			return (eInvoice, errorsNotifications, warningsNotifications);
		}

		const string WTGVATNo = "52889317";
		const string eInvoicingINVTypeName = "InvoiceMD";
		const string eInvoicingVoidingINVTypeName = "Invoice-PV-MD";
		const string eInvoicingCRDTypeName = "AllowanceMD";
		const string eInvoicingCRDTVoidingTypeName = "Allowance-PV-MD";

		string GetFileName(ComplianceDocumentBatch complianceDocumentBatch)
		{
			string eInvoicingName = null;
			if (complianceDocumentBatch.ComplianceDocumentHeaderDetails.Length > 0)
			{
				var transactionType = complianceDocumentBatch.ComplianceDocumentHeaderDetails[0].TransactionType;
				if (transactionType == TransactionTypes.Invoice)
				{
					eInvoicingName = ComplianceDocumentBatch.ComplianceDocumentHeaderDetails[0].IsSpecialVoiding
						? eInvoicingVoidingINVTypeName
						: eInvoicingINVTypeName;
				}
				else
				{
					eInvoicingName = ComplianceDocumentBatch.ComplianceDocumentHeaderDetails[0].IsSpecialVoiding
						? eInvoicingCRDTVoidingTypeName
						: eInvoicingCRDTypeName;
				}
			}
			var companyVATNo = complianceDocumentBatch.ComplianceDocumentHeaderDetails[0].SystemVATRegistrationNum;
			return FormattableString.Invariant($"{WTGVATNo}-{eInvoicingName}-{companyVATNo}-Paper-{ZDateTime.Now.ToString("yyyyMMdd-HHmmssfff", CultureInfo.InvariantCulture)}.txt"); // Constant string used in XML file. Not a user visible text.
		}

		ZString GetPayloadFlatFile(INotifications notifications)
		{
			var encodedText = string.Empty;
			using (var memoryStream = new MemoryStream())
			{
				new FlatFileWriterForTaiwan().WriteFlatFileToStream(ComplianceDocumentBatch.ComplianceDocumentHeaderDetails, memoryStream, notifications);
				encodedText = Encoding.UTF8.GetString(memoryStream.ToArray());
			}
			return encodedText;
		}

		public GlbCompany GEIMessageCompany => _geiMessageCompany ?? (_geiMessageCompany = new ReadOnlyBusinessObjectFactory().Load<GlbCompany>(ComplianceDocumentBatch.CompanyPK));
		GlbCompany _geiMessageCompany;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in XML file. Not a user visible text.")]
		const string TAIWAN_ELECTRONIC_INVOICING = "Taiwan electronic invoicing system";
		[EInvoiceMessageSubType]
		public const string WS_REQUEST = EInvoiceAPICommandList.Codes.Request; // Constant string used in XML file. Not a user visible text.
	}
}
