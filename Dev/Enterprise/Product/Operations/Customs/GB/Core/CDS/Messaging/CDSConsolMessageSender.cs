using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;
using static Enterprise.Integration.Customs.GB.GBCDS;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public class CDSConsolMessageSender : ConsolMessageSender, ICDSConsolMessageSender
	{
		public CDSConsolMessageSender(CustomsExportConsolIntegrationWrapper consolWrapper)
		{
			wrapper = consolWrapper;
		}
		readonly CustomsExportConsolIntegrationWrapper wrapper;

		protected override ConsolMessageManager GetConsolMessageManager(CustomsExportConsolIntegrationWrapper consolWrapper, GbDes242MessageFunction how, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			return new CDSConsolMessageManager(consolWrapper, how, sendMessagesToCustoms);
		}

		public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
		{
			if (wrapper?.IsProfileForCCSUK ?? false)
			{
				return new[] { ApplicationCodeList.Codes.GbCcsuk, "GCI", CDSServiceTaskConstants.CDSMessageRetrieverServiceTaskCode }; //GCI, EHO and EHI constants not available from here
			}
			else
			{
				return new[] { "EHO", "EHI", CDSServiceTaskConstants.CDSMessageSenderServiceTaskCode, CDSServiceTaskConstants.CDSMessageRetrieverServiceTaskCode };
			}
		}
	}

	internal class CDSConsolMessageManager : ConsolMessageManager
	{
		public CDSConsolMessageManager(CustomsExportConsolIntegrationWrapper consolWrapper, GbDes242MessageFunction how, ISendsMessagesToCustoms sendMessagesToCustoms)
			: base(consolWrapper, how, sendMessagesToCustoms)
		{
		}

		protected override IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return new CDSConsolMessageBuilder(consolWrapper, how);
		}
	}

	internal class CDSConsolMessageBuilder : ConsolMessageBuilder
	{
		public CDSConsolMessageBuilder(CustomsExportConsolIntegrationWrapper consolWrapper, GbDes242MessageFunction how)
			: base(consolWrapper, how)
		{
		}

		protected override Type GetEDIMessageBizOType()
		{
			switch (how)
			{
				case GbDes242MessageFunction.MucrAssociate _:
				case GbDes242MessageFunction.MucrDisAssociate _:
				case GbDes242MessageFunction.MucrClose _:
					return typeof(CDSInventoryLinkingConsolidationRequestEDIMessage);
				case GbInventoryManagementMessageFunction.ArrivalActual _:
				case GbInventoryManagementMessageFunction.ArrivalAnticipated _:
				case GbInventoryManagementMessageFunction.Departure _:
					return typeof(CDSInventoryLinkingMovementRequestEDIMessage);
				case GbDes242MessageFunction.QueryMasterDEC _:
					return typeof(CDSInventoryLinkingQueryRequestEDIMessage);
				default:
					return typeof(CDSEDIMessage);
			}
		}

		protected override ZString GetApplicationCode() => consolWrapper.IsProfileForCCSUK ? ApplicationCodeList.Codes.GbCDSViaCCSUK : ApplicationCodeList.Codes.GbCustomsDeclarationServices;

		protected override void SetMessageType(EDIMessage message)
		{
		}

		protected override ZString GetMessageText()
		{
			return CDSInventoryLinkingRequestMessageBuilder.NewMessageText(new CdsExportConsolIntegrationWrapperToIUkCinvWrapper(consolWrapper), how);
		}

		protected override ZString GetInterpretation(EDIMessage message)
		{
			switch (message.EM_MessageSubType)
			{
				case GbCusDecMessageFunctionsList.Codes.Close:
					return new CDSInventoryLinkingConsolidationRequestEDIMessagePrettier((CDSInventoryLinkingConsolidationRequestEDIMessage)message).MakeHumanReadable();
				default:
					return "Interpretation will be supported soon.";
			}
		}
	}
}
