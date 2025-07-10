using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

namespace Enterprise.Customs.IT.Business;

public class SadEtIrispMessageSubProcessor : SadIrispMessageSubProcessor, ISadImportExportMessageSubProcessor
{
	public SadEtIrispMessageSubProcessor(ISadCustomsLinkedObjectAdapter entryAdapter) : base(entryAdapter)
	{
		if (!entryAdapter.IsExport)
		{
			throw new ArgumentException("Entry must be related to an export job");
		}
	}

	protected override void UpdateOrInsertSpecificEntryNumbers(SadPositiveResponseMessage responseMessage, IEnumerable<CusEntryNumber> entryNumberCollection)
	{
		UpdateOrInsertEntryNumForMRN(responseMessage, entryNumberCollection);
		if (responseMessage.GuaranteeAmount.HasValue)
		{
			UpdateOrInsertEntryNumForGuaranty(responseMessage, entryNumberCollection);
		}
	}

	protected override void PerformActionsForPositiveIrispCore()
	{
		base.PerformActionsForPositiveIrispCore();
		EntryAdapter.UpdatePendingGuaranteeTransactions(PermitTransactionStatusList.Codes.Confirmed);
	}

	protected override void PerformActionsForNegativeIrispCore()
	{
		base.PerformActionsForNegativeIrispCore();
		EntryAdapter.UpdatePendingGuaranteeTransactions(PermitTransactionStatusList.Codes.Deleted);
	}

	protected override void UpdateEntryStatusAfterChildMessagesProcessingCore()
	{
		base.UpdateEntryStatusAfterChildMessagesProcessingCore();
	}

	#region Implementation

	void UpdateOrInsertEntryNumForMRN(SadPositiveResponseMessage responseMessage, IEnumerable<CusEntryNumber> entryNumbers) => UpdateOrInsertEntryNum(entryNumbers, CusEntryNumberConstants.EntryTypes.Mrn, responseMessage.MrnCode, responseMessage.CustomsOffice, responseMessage.RegistrationDate);

	void UpdateOrInsertEntryNumForGuaranty(SadPositiveResponseMessage responseMessage, IEnumerable<CusEntryNumber> entryNumbers) => UpdateOrInsertEntryNum(entryNumbers, CusEntryNumberConstants.EntryTypes.Guaranty, responseMessage.GuaranteeAmount.ToString(), ZString.Empty, responseMessage.RegistrationDate);

	#endregion
}
