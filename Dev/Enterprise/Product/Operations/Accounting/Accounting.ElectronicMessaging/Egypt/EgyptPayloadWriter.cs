using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt
{
	public class EgyptPayloadWriter : TransactionBatchToPayloadWriterBase
	{
		protected override void WritePayloadToStream(TransactionBatch transactionBatch, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			Argument.NotNull(transactionBatch, nameof(transactionBatch));
			Argument.NotNull(stream, nameof(stream));
			Argument.NotNull(accBatch, nameof(accBatch));
			Argument.NotNull(notifications, nameof(notifications));
			Argument.NotNull(warnings, nameof(warnings));

			var query = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, AccTransactionHeaderAuthorisationRecordTypes.Egypt);
			var parentPks = accBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().Select(x => x.AIP_ParentID).ToArray();
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, parentPks);
			var authRecords = accBatch.Factory.Load<AccTransactionHeaderAuthorisationRecord>(query);

			if (authRecords.Length == 0)
			{
				warnings.AddError(Res.GetString("729b6319-fd0f-4397-8178-0860f54b98f0", "E-Invoicing batch contains no signed E-Invoice document(s)."));
			}
			else if (authRecords.Length != parentPks.Length)
			{
				warnings.AddError(Res.GetString("8925e5a9-476d-4fd2-a544-6c8da4befe7e", "Unexpected number of signed E-Invoice documents ({0:N0}) compared to transactions in batch ({1:N0}).", authRecords.Length, parentPks.Length));
			}

			if (authRecords.Any(x => x.AHF_AuthorisationData.IsEmpty))
			{
				warnings.AddError(Res.GetString("24a31acc-55b5-4a6b-af82-dc1e8f0ddf34", "One or more transactions are missing expected signed E-Invoice document."));
			}

			WriteStringToStream(stream, (NoResString)"<submission><documents>"); // XML element names.
			foreach (var authRecord in authRecords)
			{
				if (!authRecord.AHF_AuthorisationData.IsEmpty)
				{
					var bytes = authRecord.AHF_AuthorisationData;
					stream.Write(bytes, 0, bytes.Length);
				}
			}
			WriteStringToStream(stream, (NoResString)"</documents></submission>"); // XML element names.
		}

		static void WriteStringToStream(Stream stream, string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return;
			}
			var bytes = MessageEncoding.UTF8WithoutBOM.GetBytes(s);
			stream.Write(bytes, 0, bytes.Length);
		}
	}
}
