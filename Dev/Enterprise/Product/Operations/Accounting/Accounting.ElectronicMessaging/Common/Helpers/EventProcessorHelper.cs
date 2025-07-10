using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class EventProcessorHelper
	{
		public static ZString CheckLengthAndTruncIfNeeded(GlbCompany company, ZString message)
		{
			var maxLength = AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength;
			if (message.Length > maxLength)
			{
				var factory = new BusinessObjectFactory();
				var group = factory.Load<GlbGroup>(AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				var informationText = group != null
					? Res.GetString("933af583-977d-4231-863d-9d71acd3355d", "...Details in notification email sent to user group {0}", group.GG_Code)
					: Res.GetString("84b27749-764a-4e18-8fcc-f94b78af607b", "...Notification email with detail error info cannot be sent, as registry '{0}' has not been setup for company {1}", AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.Caption, company.GC_Code);

				message = message.Substring(0, maxLength - informationText.Length) + informationText;
			}

			return message;
		}

		public static string GetBatchInfo(AccEInvoicingBatch batch)
		{
			var batchInfo = new StringBuilder();

			batchInfo.AppendLine((NoResString)"Batch Info:"); // Error Report Message
			batchInfo.AppendLine(batch.GetAllPropertyValues());
			batchInfo.AppendLine();
			batchInfo.AppendLine($"Batch has {batch.TransactionPivots.Count} pivot(s):"); // Error Report Message
			batchInfo.AppendLine(GetPivotsInfo(batch.TransactionPivots.ToArray<AccEInvoicingTransactionPivot>()));

			return batchInfo.ToString();
		}

		public static string GetPivotsInfo(AccEInvoicingTransactionPivot[] pivots)
		{
			var info = new StringBuilder();

			var pivotNumber = 1;
			foreach (var pivot in pivots)
			{
				info.AppendLine($"Pivot #{pivotNumber++}:"); // Error Report Message
				info.AppendLine(pivot.GetAllPropertyValues());
				info.AppendLine();
				info.AppendLine((NoResString)"Pivot Parent transaction Info:"); // Error Report Message
				info.AppendLine(pivot.ParentTransactionHeader?.GetTransactionHeaderInfo());
				info.AppendLine();
			}

			return info.ToString();
		}
	}
}
