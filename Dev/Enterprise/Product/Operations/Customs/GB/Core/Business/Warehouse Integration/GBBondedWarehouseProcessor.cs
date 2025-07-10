using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Business
{
	public class GBBondedWarehouseProcessor
	{
		public GBBondedWarehouseProcessor(CusEntryHeader entry, EDIMessage outgoingMessage, ZString responseFunction, ILoggingInformation logger)
		{
			this.logger = logger;
			this.entry = entry;
			this.responseFunction = responseFunction;
			messagePK = outgoingMessage.PK;
		}

		public void SetupForBondedWarehousing()
		{
			if (entry.SupportsBondedWarehousing && (CustomsStatusAttributeHelper.ShouldUpdateBondedWhs(entry.Factory, responseFunction, entry.Declaration.ApplicationExtender.GetDefaultDataGroupingCode(entry.Declaration), ZDateTime.Today)))
			{
				entry.Factory.Saved -= BondedWarehouseMessageProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
				entry.Factory.Saved += BondedWarehouseMessageProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
			}
		}

		readonly ILoggingInformation logger;
		readonly ZGuid messagePK;
		readonly EmailDef emailReportThatHasBeenDelayed;
		readonly ZString responseFunction;

		BondedWarehouseMessageProcessorCreator BondedWarehouseMessageProcessorCreator => bondedWarehouseMessageProcessorCreator ?? (bondedWarehouseMessageProcessorCreator = new BondedWarehouseMessageProcessorCreator(logger, GetNewBondedWarehouseMessageProcessor, GetFallbackNotificationGroupPK));
		BondedWarehouseMessageProcessorCreator bondedWarehouseMessageProcessorCreator;

		BondedWarehouseMessageProcessor GetNewBondedWarehouseMessageProcessor(Action<EmailDef, EDIMessage> sendEmail)
		{
			return entry?.Declaration?.ApplicationExtender.GetBondedWarehouseMessageProcessor(messagePK, emailReportThatHasBeenDelayed, sendEmail);
		}

		static Guid GetFallbackNotificationGroupPK(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		readonly CusEntryHeader entry;
	}
}
