using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415ImportDetailsProvider : IRD415ImportDetails
	{
		public RD415ImportDetailsProvider(DepositRefundApplicationMessageSendingAction sendingAction)
		{
			this.sendingAction = sendingAction;
			invoiceLine = (JobComInvoiceLine)sendingAction.EntryHeader.RandomEntryLine.RandomLine;
		}
		readonly DepositRefundApplicationMessageSendingAction sendingAction;
		readonly JobComInvoiceLine invoiceLine;

		public string Procedure => invoiceLine.JI_Calc_RequestedProcedure;

		public IReadOnlyCollection<IRD415GoodsInformationImport> GoodsInformations => goodsInformations ??= sendingAction.EntryHeader.MergedLines.Select(x => new RD415GoodsInformationImportProvider(x)).ToArray();
		IReadOnlyCollection<RD415GoodsInformationImportProvider> goodsInformations;

		public IAmountsHeldDeposit AmountsHeldDeposit => CachedValueHelper.GetValue(ref amountsHeldDeposit, () => new RD415AmountsHeldDepositProvider(sendingAction));
		CachedValue<RD415AmountsHeldDepositProvider> amountsHeldDeposit;

		public IRD415AdditionalInformation AdditionalInformation => CachedValueHelper.GetValue(ref additionalInformation, () => new RD415AdditionalInformationProvider(sendingAction));
		CachedValue<RD415AdditionalInformationProvider> additionalInformation;
	}
}
