using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CRM.Common
{
	public class CrmOpportunityWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code => WorkflowDescriptors.CrmOpportunityWorkflowDescriptorCode;

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("CRM|CrmOpportunityWorkflowDescriptor|Description", "GLOW Sales Opportunity (DO NOT USE)"); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			// Must match CrmOpportunity.GetTemplateSelectionCriteria
			// and CrmOpportunityFormCustomisationSettingProvider.GetPropertiesThatAffectWorkflow
			get
			{
				subTypesList ??= new[]
					{
						new ProcessTemplateSubType(Res.GetString("04c3fcae-48cf-40d4-a17b-2f0a8dabd2bc", "Sales Type"), OrganisationsDataRegistry.Instance.GlowOpportunitySalesTypes.Value.GetActiveCodeDescriptionPairList()),
						new ProcessTemplateSubType(Res.GetString("6710a629-54bd-4512-babb-bf9b30596506", "Source Type"), OrganisationsDataRegistry.Instance.GlowOpportunityLeadSource.Value.GetActiveCodeDescriptionPairList()),
						new ProcessTemplateSubType(Res.GetString("a462d981-6dab-4a56-9a39-e1ad80a2420e", "Product Type"), OrganisationsDataRegistry.Instance.GlowOpportunityProductType.Value.GetActiveCodeDescriptionPairList()),
					};
				return subTypesList;
			}
		}
		ProcessTemplateSubType[] subTypesList;

		#endregion

		public override bool RequiresClient => false;

		public override bool SupportsEventTracking => false;

		public override bool SupportsBufferManagement => false;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Client |
				MessageRecipientPartyType.Email;
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CrmOpportunity); }
		}

		public override ControllerID ControllerID => null;

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				var opportunity = (CrmOpportunity)bizObj;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(
					new MessageRecipientParty(
						opportunity.Organization,
						opportunity.ReferringContact != null ? opportunity.ReferringContact.OC_Email : ZString.Empty,
						opportunity.Organization.MainAddress != null ? opportunity.Organization.MainAddress.OA_Email : ZString.Empty
						)
					);
			}
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new CrmOpportunityFormCustomisationSettingProvider();
		}
	}
}
