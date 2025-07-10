using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415Provider : EntryHeaderMessageProvider, IRD415Header
	{
		public RD415Provider(DepositRefundApplicationMessageSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
			this.sendingAction = sendingAction;
		}

		readonly DepositRefundApplicationMessageSendingAction sendingAction;

		public IRD415HeaderType Header => CachedValueHelper.GetValue(ref header, () => new RD415HeaderTypeProvider(sendingAction));
		CachedValue<RD415HeaderTypeProvider> header;

		public IRD415ImportDetails ImportDetails => CachedValueHelper.GetValue(ref importDetails, () => new RD415ImportDetailsProvider(sendingAction));
		CachedValue<RD415ImportDetailsProvider> importDetails;

		public IRD415ExportDetails ExportDetails => CachedValueHelper.GetValue(ref exportDetails, () => new RD415ExportDetailsProvider(sendingAction));
		CachedValue<RD415ExportDetailsProvider> exportDetails;

		public IReadOnlyCollection<IRD415OtherDocumentsForDischarge> OtherMethodOfDischarges => Array.Empty<IRD415OtherDocumentsForDischarge>();

		public IDepositRefundDetails DepositRefundDetails => CachedValueHelper.GetValue(ref depositRefundDetails, () => new RD415DepositRefundDetailsProvider(sendingAction));
		CachedValue<RD415DepositRefundDetailsProvider> depositRefundDetails;

		public IFallbackProcedure FallbackProcedure => null;
	}
}
