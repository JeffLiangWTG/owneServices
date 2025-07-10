using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentWorkflowDescriptor : WorkflowDescriptor
	{
		readonly BusinessObjectFactory lookupFactory;

		public SupportIncidentWorkflowDescriptor()
		{
			lookupFactory = lookupFactory ?? new BusinessObjectFactory();
		}

		#region ID / Description / ControllerID

		public override string Code
		{
			get { return EDIJobInvoicingConsumerTypes.Incident.Code; }
		}

		public override IMultilingualString Description
		{
			get { return EDIJobInvoicingConsumerTypes.Incident.MultilingualDescription; }
		}

		public override ControllerID ControllerID
		{
			get { return ClientControllerRegistration.SupportIncident; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(SupportIncident); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				if (subTypeInformation == null)
				{
					subTypeInformation = new[]
					{
						new ProcessTemplateSubType("Product", Lookups.ProductList),
						new ProcessTemplateSubType("Stage", Lookups.StageList),
						new ProcessTemplateSubType("Reported By", Lookups.SourceList),
						new ProcessTemplateSubType("Product Area", EDIDataRegistry.Instance.ProductAreas.Value),
						new ProcessTemplateSubType("Language", Lookups.Languages)
					};
				}
				return subTypeInformation;
			}
		}
		ProcessTemplateSubType[] subTypeInformation;

		SupportIncidentLookups Lookups
		{
			get { return lookups ?? (lookups = new SupportIncidentLookups(lookupFactory)); }
		}

		SupportIncidentLookups lookups;

		#endregion

		#region Default Date

		protected override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			if (dateTimeSourceType == SupportIncidentEstimateDefaultedFromList.Codes.TimeOfTaskCopiedFromTemplate)
			{
				var addedLog = defaultedFromDateProvider.Logs.AddedLog;
				if (addedLog != null && !addedLog.IsDeleted)
				{
					var branch = addedLog.Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, addedLog.SL_GB_NKBranch));
					return (addedLog.SL_EventTime, branch?.HomePort);
				}

				var date = ZDateTime.UtcNow.ToLocalBranchTime();
				if (defaultedFromDateProvider is IAuditDetails auditDetails && auditDetails.SystemCreateTimeUtc.IsValid)
				{
					date = auditDetails.SystemCreateTimeUtc.ToLocalBranchTime();
				}
				return (date, ((GlbBranch)Env.CurrentBranch).HomePort);
			}

			return (ZDateTime.Empty, null);
		}

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get
			{
				return new[] { BusinessContext.SupportIncident };
			}
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				SupportIncident incident = (SupportIncident)bizObj;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(incident.Client, ZString.Empty));
			}
			else
			{
				base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			}
		}

		public override CodeDescriptionPairList EstimateDefaultedFromList
		{
			get { return new SupportIncidentEstimateDefaultedFromList(); }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new SupportIncidentFormCustomisationSettingsProvider();
		}
	}
}

