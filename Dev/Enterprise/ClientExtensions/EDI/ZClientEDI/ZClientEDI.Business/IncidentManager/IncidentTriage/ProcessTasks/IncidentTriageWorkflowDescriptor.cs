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
	public class IncidentTriageWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => IncidentTriageConstants.WorkflowDescriptorInformation.Code;

		public override IMultilingualString Description => IncidentTriageConstants.WorkflowDescriptorInformation.MultilingualDescription;

		public override ControllerID ControllerID => ClientControllerRegistration.IncidentTriage;

		public override Type WorkflowProviderType => typeof(IncidentTriage);

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
						new ProcessTemplateSubType("Product Area", Lookups.ProductAreaList),
						new ProcessTemplateSubType("Sec./Svc./Req.", IncidentTriageLookups.ModuleListForTemplate),
					};
				}
				return subTypeInformation;
			}
		}
		ProcessTemplateSubType[] subTypeInformation;

		IncidentTriageLookups Lookups
		{
			get { return lookups ?? (lookups = new IncidentTriageLookups(Factory ?? new BusinessObjectFactory())); }
		}

		IncidentTriageLookups lookups;

		#endregion

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new IncidentTriageFormCustomisationSettingsProvider();
		}

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
				return new[] { BusinessContext.INVALID };
			}
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email;
		}
	}
}
