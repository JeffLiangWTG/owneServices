using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Modules;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupWorkflowDescriptor : WorkflowDescriptor
	{
		readonly BusinessObjectFactory lookupFactory;

		public IncidentManagementGroupWorkflowDescriptor()
		{
			lookupFactory = new BusinessObjectFactory();
		}

		#region ID / Description / ControllerID

		public override string Code => IncidentManagementGroupConstants.WorkflowDescriptorInformation.Code;

		public override IMultilingualString Description => IncidentManagementGroupConstants.WorkflowDescriptorInformation.MultilingualDescription;

		public override ControllerID ControllerID => ClientControllerRegistration.IncidentManagementGroup;

		public override Type WorkflowProviderType => typeof(IncidentManagementGroup);

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
						new ProcessTemplateSubType("Type", Lookups.Types),
						new ProcessTemplateSubType("Product", Lookups.ProductList),
						new ProcessTemplateSubType("Product Area", EDIDataRegistry.Instance.ProductAreas.Value),
					};
				}
				return subTypeInformation;
			}
		}
		ProcessTemplateSubType[] subTypeInformation;

		IncidentManagementGroupLookups Lookups
		{
			get { return lookups ?? (lookups = new IncidentManagementGroupLookups(lookupFactory)); }
		}

		IncidentManagementGroupLookups lookups;

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override bool RequiresClient => false;

		public override bool SupportsBufferManagement => true;

		public override BusinessContext[] DocumentBusinessContext
		{
			get
			{
				return new[] { BusinessContext.IncidentGroup };
			}
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new IncidentManagementGroupFormCustomisationSettingsProvider();
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
		}
	}
}

