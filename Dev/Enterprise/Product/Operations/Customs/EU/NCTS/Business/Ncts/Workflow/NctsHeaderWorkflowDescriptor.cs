using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EU.NCTS;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		#region Overrides of WorkflowDescriptor

		public override string Code
		{
			get { return WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("NctsHeaderWorkflowDescriptor", "NCTS - New Computerised Transit System (Europe)"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.EU.NctsMovementController; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(NctsHeader); }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.CusInBondHeader }; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsUniversalTemplates => false;

		public override ZString Port1Name
		{
			get { return (NoResString)"Port of Loading"; }
		}

		public override ZString Port2Name
		{
			get { return (NoResString)"Port of Unloading"; }
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var list = base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);

			var header = parent as NctsHeader;

			var supportedCountry = header != null && AutoMessageSendingHelper.SupportsNctsAutomaticMessageSending(header.CountryCode);
			var isPhase4 = header != null && header.BH_ApplicationCode == CusInBondApplicationCodeList.Codes.NCTS4;
			if (header != null && ((header.IsDepartureMovement && supportedCountry) || isPhase4) || parent is ProcessTaskTemplate)
			{
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendNCTSMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendNCTSMessage));
			}

			if (((supportedCountry || isPhase4) && header.IsArrivalMovement) || parent is ProcessTaskTemplate)
			{
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendNCTSArrivalNotification, WorkflowTriggerActionTypeConstants.Descriptions.SendNCTSArrivalNotification));
			}

			return list;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var action = source.Action;
			IProcessor processor = null;
			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendNCTSMessage)
			{
				var nctsHeader = (NctsHeader)source.Job;
				var supporter = nctsHeader as INCTSAutoSendingMessageSupporter;
				processor = supporter.CreateNCTSMessageProcessor();
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendNCTSArrivalNotification)
			{
				var nctsHeader = (NctsHeader)source.Job;
				var supporter = nctsHeader as INCTSAutoSendingMessageSupporter;
				processor = supporter.CreateNCTSArrivalNotificationMessageProcessor();
			}

			return processor ?? base.GetWorkflowTriggerActionCore(source, queuedLog);
		}

		#endregion

		#region Requires

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return false; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.OrgProxy;
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				CusInBondMoveHeaderSchema.BM_CustomsStatus,
				CusInBondHeaderSchema.BH_MessageStatus,
				CusInBondMoveHeaderSchema.BM_MessageStatus,
			};
		}

		public override string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			switch (fieldColumn.Name)
			{
				case CusInBondMoveHeader.Schema.BM_CustomsStatus:
					return (NoResString)"NCTS Transit Status";
				case CusInBondHeader.Schema.BH_MessageStatus:
				case CusInBondMoveHeader.Schema.BM_MessageStatus:
					return (NoResString)"NCTS Message Status";
				default:
					return base.GetFieldColumnDescription(factory, fieldColumn);
			}
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new NctsHeaderFormCustomisationSettingsProvider();
		}

		public override bool SupportsBufferManagement => true;

		#endregion

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			if (parent is NctsHeader header &&
				AutoMessageSendingHelper.SupportsNctsAutomaticMessageSending(header.CountryCode) || parent is ProcessTaskTemplate)
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			}
		}

#if DEBUG
		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			var header = base.GetBizOForTest(factory);
			((NctsHeader)header).BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			return header;
		}
#endif
	}
}
