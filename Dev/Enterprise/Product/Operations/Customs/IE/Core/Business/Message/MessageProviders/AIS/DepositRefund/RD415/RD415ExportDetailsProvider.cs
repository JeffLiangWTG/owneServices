using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415ExportDetailsProvider : IRD415ExportDetails
	{
		public RD415ExportDetailsProvider(DepositRefundApplicationMessageSendingAction sendingAction)
		{
			this.sendingAction = sendingAction;
		}
		readonly DepositRefundApplicationMessageSendingAction sendingAction;

		public string MRN => sendingAction.ExportMovementReferenceNumber;

		public DateTime Date => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(sendingAction.ExportDate, true);

		public IReadOnlyCollection<IRD415GoodsInformationExport> GoodsInformations => goodsInformations ??= sendingAction.EntryHeader.MergedLines.Select(x => new RD415GoodsInformationExportProvider(x)).ToArray();
		IReadOnlyCollection<RD415GoodsInformationExportProvider> goodsInformations;
	}
}
