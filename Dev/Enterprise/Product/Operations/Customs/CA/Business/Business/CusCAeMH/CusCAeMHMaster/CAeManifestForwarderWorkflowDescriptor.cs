using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAeManifestForwarderWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.CAeManifestWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("D8EB5D0C-BB1E-4D9C-9E5F-062BBFEFEBCE", "CA eManifest Forwarder"); }
		}

		#endregion

		public override Type WorkflowProviderType
		{
			get { return typeof(CusCAeMHMaster); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.CA.CAHouseBilleManifest; }
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = (CodeDescriptionPairList)base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage,
				WorkflowTriggerActionTypeConstants.Descriptions.SendEmanifestCloseMessage);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendAllEmanifestHouseBills,
			WorkflowTriggerActionTypeConstants.Descriptions.SendAllEmanifestHouseBills);
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage)
			{
				var emanifest = (CusCAeMHMaster)source.Job;
				return new SendCAeManifestCloseMessageProcessor(emanifest);
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendAllEmanifestHouseBills)
			{
				var emanifest = (CusCAeMHMaster)source.Job;
				return new SendAlleManifestHouseBillsMessageProcessor(emanifest);
			}

			return base.GetWorkflowTriggerActionCore(source);
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.CAeManifest }; }
		}
	}
}
