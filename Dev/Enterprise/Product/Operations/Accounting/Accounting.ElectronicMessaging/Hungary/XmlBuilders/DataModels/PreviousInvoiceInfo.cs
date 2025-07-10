using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class PreviousInvoiceInfo
	{
		public ZString? FirstOriginalInvoiceNumber { get; set; }

		public ZString? WasModifiedWithoutMaster { get; set; }

		public int? ModificationIndex { get; set; }

		public bool WasOriginalInvoiceSubmittedUsingApiVersion2 { get; set; }
	}

	public class PreviousInvoice
	{
		public string AH_TransactionNum { get; set; }
		public string AH_TransactionType { get; set; }
		public ZGuid AH_GB { get; set; }
		public bool WasSubmittedSuccessfully { get; set; }
		public ZDateTime LastResponseReceivedTime { get; set; } = ZDateTime.Empty;
		public int MaximumLineSequence { get; set; }

		public PreviousInvoice()
		{
		}

		internal PreviousInvoice(DynamicBusinessObject bizo)
		{
			AH_TransactionNum = (ZString)bizo[AccTransactionHeaderSchema.AH_TransactionNum];
			AH_TransactionType = (ZString)bizo[AccTransactionHeaderSchema.AH_TransactionType];
			AH_GB = (ZGuid)bizo[AccTransactionHeaderSchema.AH_GB];

			WasSubmittedSuccessfully = (ZString)bizo[AccEInvoicingTransactionPivotSchema.AIP_Status] == EInvoicingPivotState.Succeed;
			LastResponseReceivedTime = ConvertToTransactionBranchLocalTime((ZDateTime)bizo[AccEInvoicingTransactionPivotSchema.AIP_LastResponseReceivedUtc]);

			var sequenceAsObj = bizo[AccTransactionLinesSchema.AL_Sequence];
			MaximumLineSequence = sequenceAsObj == null ? (ZInt)0 : (ZInt)sequenceAsObj;
		}

		ZDateTime ConvertToTransactionBranchLocalTime(ZDateTime utcTime)
		{
			Argument.NotNull(utcTime, nameof(utcTime));

			var result = ZDateTime.Empty;
			if (TransactionBranch != null && utcTime.IsValid)
			{
				var currentBranchZoneSet = TransactionBranch.HomePort?.TimeZoneSet;
				if (currentBranchZoneSet != null)
				{
					ITimeZone calculationTimeZone = currentBranchZoneSet.GetCalculationTimeZone();
					result = calculationTimeZone.ToLocalTime(utcTime.ToDateTime());
				}
			}

			return result;
		}

		GlbBranch TransactionBranch => transactionBranch ?? (transactionBranch = new ReadOnlyBusinessObjectFactory().Load<GlbBranch>(AH_GB));
		GlbBranch transactionBranch;

		public override bool Equals(object obj)
		{
			return obj is PreviousInvoice invoice &&
				   AH_TransactionNum == invoice.AH_TransactionNum &&
				   AH_TransactionType == invoice.AH_TransactionType &&
				   WasSubmittedSuccessfully == invoice.WasSubmittedSuccessfully &&
				   MaximumLineSequence == invoice.MaximumLineSequence;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 1375152542;
				hashCode = hashCode * (-1521134295 + EqualityComparer<string>.Default.GetHashCode(AH_TransactionNum));
				hashCode = hashCode * (-1521134295 + EqualityComparer<string>.Default.GetHashCode(AH_TransactionType));
				hashCode = hashCode * (-1521134295 + WasSubmittedSuccessfully.GetHashCode());
				hashCode = hashCode * (-1521134295 + MaximumLineSequence.GetHashCode());
				return hashCode;
			}
		}

		public override string ToString() => FormattableString.Invariant($"{AH_TransactionNum} {AH_TransactionType} Successful: {WasSubmittedSuccessfully}, MaxSequence: {MaximumLineSequence}");
	}
}
