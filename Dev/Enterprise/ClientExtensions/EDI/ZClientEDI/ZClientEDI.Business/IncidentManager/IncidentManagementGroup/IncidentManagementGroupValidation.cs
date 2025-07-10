using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Business.Res;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupValidation : AutoIncidentManagementGroupValidation
	{
		public IncidentManagementGroupValidation(AutoIncidentManagementGroup parent) : base(parent)
		{
		}

		public new IncidentManagementGroup Parent
		{
			get { return (IncidentManagementGroup)base.Parent; }
		}

		protected override void CheckING_Type()
		{
			base.CheckING_Type();

			MandatoryValidation.CheckEntered(Parent.ING_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ING_TypeInfo);
		}

		protected override void CheckING_Status()
		{
			base.CheckING_Status();
			Parent.RefreshStages();
			if (Parent.ING_Type.IsEmpty)
			{
				Parent.ING_StatusInfo.AddError(Res.GetString("29e53fbc-77a1-4ec2-861b-f66b286231e0", "Type should be set before Status"));
			}

			MandatoryValidation.CheckEntered(Parent.ING_StatusInfo);
			ListValidation.ErrorIfInvalidCode((NoResString)"The current status is invalid. You should reload the form to select a valid status before continuing.", Parent.ING_StatusInfo);
		}

		protected override void CheckING_GS_NKGroupOwner()
		{
			base.CheckING_GS_NKGroupOwner();

			MandatoryValidation.CheckEntered(Parent.ING_GS_NKGroupOwnerInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ING_GS_NKGroupOwnerInfo);
		}

		protected override void CheckING_ServiceOutage()
		{
			base.CheckING_ServiceOutage();

			MandatoryValidation.CheckEntered(Parent.ING_ServiceOutageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ING_ServiceOutageInfo);
		}

		protected override void CheckING_BusinessImpact()
		{
			base.CheckING_BusinessImpact();

			MandatoryValidation.CheckEntered(Parent.ING_BusinessImpactInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ING_BusinessImpactInfo);
		}

		protected override void CheckING_Urgency()
		{
			base.CheckING_Urgency();

			MandatoryValidation.CheckEntered(Parent.ING_UrgencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ING_UrgencyInfo);
		}

		protected override void CheckING_Priority()
		{
			base.CheckING_Priority();
			//Validation should mirror CheckIM_Priority
			//For now it doesn't, we're just keeping it simple
			MandatoryValidation.CheckEntered(Parent.ING_PriorityInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ING_PriorityInfo);
		}

		protected override void CheckING_Product()
		{
			base.CheckING_Product();
			//Validation should mirror CheckIM_Product

			MandatoryValidation.CheckEntered(Parent.ING_ProductInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ING_ProductInfo);
		}

		protected override void CheckING_ProductArea()
		{
			base.CheckING_ProductArea();
			//Validation should mirror CheckIM_ProgramArea

			ListValidation.ErrorIfInvalidCode(Parent.ING_ProductAreaInfo);
		}

		protected override void CheckING_Module()
		{
			base.CheckING_Module();
			//Validation should mirror CheckIM_Module

			MandatoryValidation.CheckEntered(Parent.ING_ModuleInfo);

			var hasBeenChanged = !Parent.IsInDatabase || Parent.ING_ModuleInfo.HasChanges || Parent.ING_ProductAreaInfo.HasChanges;
			if (hasBeenChanged)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ING_ModuleInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.ING_ModuleInfo, Parent.Lookups.ProductAreaIndependentModuleList);
			}

			if (Parent.ING_ProductArea.IsEmpty)
			{
				Parent.ING_ModuleInfo.AddWarning("Not linked to any product areas.");
			}
		}

		protected override void CheckING_RN_NKCountry()
		{
			base.CheckING_RN_NKCountry();
			ListValidation.ErrorIfInvalidCode(Parent.ING_RN_NKCountryInfo);
		}

		protected override void CheckING_ServiceType()
		{
			base.CheckING_ServiceType();
			//Validation should mirror CheckIM_ServiceType
			ListValidation.ErrorIfInvalidCode(Parent.ING_ServiceTypeInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateInitialSymptomsText();
			ValidateBusinessImpactDescriptionText();
			ValidateRootCauseText();
			ValidateAutoCascadeRelatedItems();
		}

		public void ValidateAutoCascadeRelatedItems()
		{
			var workItemErrorMsg = ResString.GetMultilingualString("0f615be1-aebe-4390-9554-970cbcd86529", "Only Defect {0} type work items can be cascaded", NewWorkItemLookups.WorkItemTypeConstants.DefectFix);
			var groupWarningMsg = Res.GetString("4c7462f9-1dc7-4f1b-9ef4-82b14436f028", "A work item has been set to auto-cascade, but the incident management group's criticality is not CR1-CR4");
			Parent.RemoveRowError(workItemErrorMsg);
			Parent.RemoveRowWarning(groupWarningMsg);
			if (Parent.AutoCascadeRelatedItems != null && Parent.AutoCascadeRelatedItems.Count > 0)
			{
				if (!Parent.IsDefect)
				{
					Parent.AddRowWarning(groupWarningMsg);
				}
				var relatedItems = Parent.AutoCascadeRelatedItems;
				foreach (var relatedItem in relatedItems.OfType<WorkItem>())
				{
					if (!relatedItem.WKI_ActivitySubtype.EqualsIgnoringCase(NewWorkItemLookups.WorkItemTypeConstants.DefectFix))
					{
						Parent.AddRowError(workItemErrorMsg);
						break;
					}
				}
				if (Parent.LinkedIncidents != null && Parent.LinkedIncidents.Count > 0)
				{
					var linkedIncidents = Parent.LinkedIncidents;
					foreach (var linkedIncident in linkedIncidents.Cast<IncidentManagementLink>())
					{
						var incident = linkedIncident.SupportIncident;
						var incidentWarningMsg = ResString.GetMultilingualString("5aa806c4-4871-4e46-a243-76d82af89ac4", "A work item has been set to auto-cascade, but could not be cascaded to incident {0} because the incident criticality is not CR1-CR4", incident.IM_IncidentNumber);
						Parent.RemoveRowWarning(incidentWarningMsg);
						if (!incident.IsDefect)
						{
							Parent.AddRowWarning(incidentWarningMsg);
						}
					}
				}
			}
		}

		#region Notes

		public void ValidateInitialSymptomsText()
		{
			ValidateCalculatedProperty(Parent.InitialSymptomsTextInfo);
		}

		public void ValidateBusinessImpactDescriptionText()
		{
			ValidateCalculatedProperty(Parent.BusinessImpactDescriptionTextInfo);
		}

		public void ValidateRootCauseText()
		{
			ValidateCalculatedProperty(Parent.RootCauseTextInfo);
		}

		protected virtual void CheckInitialSymptomsText()
		{
			MandatoryValidation.CheckEntered(Parent.InitialSymptomsTextInfo);
		}

		protected virtual void CheckBusinessImpactDescriptionText()
		{
			MandatoryValidation.CheckEntered(Parent.BusinessImpactDescriptionTextInfo);
		}

		#endregion
	}
}
