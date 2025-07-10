using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.Accounting.ElectronicMessaging.Brazil
{
	public class BrazilPayloadWriter : TransactionBatchToPayloadWriterBase
	{
		protected override void WritePayloadToStream(TransactionBatch transactionBatch, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			Argument.NotNull(transactionBatch, nameof(transactionBatch));
			Argument.NotNull(stream, nameof(stream));
			Argument.NotNull(accBatch, nameof(accBatch));
			Argument.NotNull(notifications, nameof(notifications));
			Argument.NotNull(warnings, nameof(warnings));

			switch (messageType)
			{
				case BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest:
					WriteCancellationPayloadToStream(accBatch, stream, notifications, warnings);
					break;
				case BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest:
					WriteGenerateInvoicePayloadToStream(accBatch, stream, notifications, warnings);
					break;
				default:
					throw new ArgumentException("Unknown message type: " + messageType);
			}
		}

		void WriteGenerateInvoicePayloadToStream(AccEInvoicingBatch accBatch, Stream stream, INotifications notifications, INotifications warnings)
		{
			var transaction = accBatch.TransactionPivots[0]?.ParentTransactionHeader;
			if (transaction == null)
			{
				return;
			}
			if (transaction.GetComplianceAllocationMethodARThisTransaction() != AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate)
			{
				return;
			}

			var complianceBook = GetComplianceSequenceSafe(transaction);
			if (complianceBook == null)
			{
				notifications.AddError(Res.GetString("375997b7-9139-43c7-89e2-099749858c6d", "No Compliance Sequence was found for the AR INV transaction using GVT Compliance Document Number Allocation Method."));
				return;
			}

			var prefix = complianceBook.XD_Prefix;
			var payload = new GenerateInvoiceRequestData
			{
				ComplianceBookPrefix = prefix,
			};
			WritePayloadAsJson(payload, stream);
		}

		void WriteCancellationPayloadToStream(AccEInvoicingBatch accBatch, Stream stream, INotifications notifications, INotifications warnings)
		{
			if (accBatch.AIB_GovernmentAllocatedNumber.IsEmpty)
			{
				notifications.AddError(Res.GetString("76065ce5-a80e-487f-98cf-32d6a65fa803", "No government allocated number (NF-e number) was found for original AR INV transaction."));
				return;
			}

			var payload = new CancellationRequestData()
			{
				GovernmentAllocatedNumber = accBatch.AIB_GovernmentAllocatedNumber,
				InvoiceVerificationCode = GetOriginalInvoiceVerificationCode(accBatch),
			};
			WritePayloadAsJson(payload, stream);
		}

		AccComplianceSequence GetComplianceSequenceSafe(AccTransactionHeader transaction)
		{
			try
			{
				return transaction.ComplianceSequence;
			}
			catch (ComplianceSequenceRelatedException)
			{
				return null;
			}
		}

		string GetOriginalInvoiceVerificationCode(AccEInvoicingBatch accBatch)
		{
			var sql = @"SELECT AHF_Number
FROM dbo.AccEInvoicingTransactionPivot
	JOIN dbo.AccTransactionHeader ON AH_PK = AIP_ParentID
	JOIN dbo.AccTransactionHeaderAuthorisationRecord ON AHF_ParentId = AH_TransactionBelongsToGroup
WHERE AIP_AIB = @BatchPK
	AND AHF_RecordType = 'BRZ'";
			var parameters = new ZSqlParameter[] { ZSqlParameter.New("@BatchPK", accBatch.PK, AccEInvoicingTransactionPivotSchema.AIP_AIB) };
			var result = new DynamicBusinessObjectCollection(accBatch.Factory);
			result.Load(sql, parameters);

			return result.Count > 0 ? result[0]["AHF_Number"].ToString() : null;
		}

		static void WritePayloadAsJson(object payload, Stream stream)
		{
			var payloadAsJson = JsonConvert.SerializeObject(payload);
			var payloadAsJsonBytes = MessageEncoding.UTF8WithoutBOM.GetBytes(payloadAsJson);
			stream.Write(payloadAsJsonBytes, 0, payloadAsJsonBytes.Length);
		}

		public class GenerateInvoiceRequestData
		{
			public string ComplianceBookPrefix { get; set; }
		}

		public class CancellationRequestData
		{
			public string GovernmentAllocatedNumber { get; set; }
			public string InvoiceVerificationCode { get; set; }
		}
	}
}
