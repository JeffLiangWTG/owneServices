using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentValidation : IncidentMainValidation
	{
		public SupportIncidentValidation(SupportIncident parent)
			: base(parent)
		{
		}

		new SupportIncident Parent
		{
			get { return (SupportIncident)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRelatedProjectPK();
			ValidateDefectCausedByWorkItemPK();
			ValidateDetailNoteText();
			ValidateFeatureRequestClientAddressPK();
			ValidateFeatureRequestContactPK();
			ValidateDatabaseServerCode();
			ValidateClientCompanyCode();
		}

		#region IM_Description

		protected override void CheckIM_Description()
		{
			base.CheckIM_Description();
			MandatoryValidation.CheckEntered(Parent.IM_DescriptionInfo);
		}

		#endregion

		#region IM_GG_Team

		protected override void CheckIM_GG_Team()
		{
			base.CheckIM_GG_Team();
			ListValidation.ErrorIfInvalidPK(Parent.IM_GG_TeamInfo, Parent.Lookups.Teams);
		}

		#endregion

		#region IM_OA_BranchAddress

		protected override void CheckIM_OA_BranchAddress()
		{
			base.CheckIM_OA_BranchAddress();

			MandatoryValidation.CheckEntered(Parent.IM_OA_BranchAddressInfo);
			ListValidation.ErrorIfInvalidPK(Parent.IM_OA_BranchAddressInfo, Parent.Lookups.BranchAddresses);

			OrgHeader client;
			OrgAddress address = Parent.BranchAddress;
			if (address != null && (client = address.Header) != null && !client.OH_IsActive && !Parent.IsInDatabase)
			{
				Parent.IM_OA_BranchAddressInfo.AddError(Res.GetString("751552af-1f9e-4bdf-999c-59028451dacb", "Please do not select Inactive Clients."));
			}
		}

		#endregion

		#region Enterprise

		public void ValidateEnterprisePK()
		{
			ValidateCalculatedProperty(Parent.EnterprisePKInfo);
		}

		protected void CheckEnterprisePK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.EnterprisePKInfo);
		}

		public void ValidateEnterpriseCode()
		{
			ValidateCalculatedProperty(Parent.EnterpriseCodeInfo);
		}

		protected void CheckEnterpriseCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.EnterpriseCodeInfo);
		}

		#endregion

		#region DatabaseServerCode

		public void ValidateDatabaseServerCode()
		{
			ValidateCalculatedProperty(Parent.DatabaseServerCodeInfo);
		}

		protected void CheckDatabaseServerCode()
		{
			if (!Parent.DatabaseServerCode_ReadOnly &&
				(!Parent.IsInDatabase || Parent.IM_LDInfo.HasChanges || Parent.IM_OH_ClientInfo.HasChanges))
			{
				ListValidation.ErrorIfInvalidCode(Parent.DatabaseServerCodeInfo);

				if (!Parent.DatabaseServerCodeInfo.HasErrors())
				{
					var database = Parent.Database;

					if (database == null)
					{
						if (!Parent.IsProjectRelatedIncident
							&& (Parent.IM_Product == ProductTypes.Codes.Enterprise || Parent.IM_Product == ProductTypes.Codes.GLOW)
							&& !Parent.IM_LD_ReadOnly
							&& !Parent.IM_LD.IsValid
							&& Parent.Lookups.DatabaseList.Cast<LicenceDatabase>().Any(x => x.IsEnterpriseFamilyDatabase || x.LD_Product == ProductTypes.Codes.GLOW))
						{
							Parent.DatabaseServerCodeInfo.AddError("Database is mandatory when the Product is ediEnterprise / CargoWiseOne / CargoWiseNext or GLOW.");
						}
					}
					else
					{
						if (!database.LD_IsActive)
						{
							Parent.DatabaseServerCodeInfo.AddError("Please select an active Database.");
						}
					}
				}
			}
		}

		#endregion

		#region ClientCompanyCode

		public void ValidateClientCompanyCode()
		{
			ValidateCalculatedProperty(Parent.ClientCompanyCodeInfo);
		}

		protected void CheckClientCompanyCode()
		{
			bool isNewValue = !Parent.IsInDatabase || Parent.IM_LCCInfo.HasChanges || Parent.IM_LDInfo.HasChanges;
			var info = Parent.ClientCompanyCodeInfo;

			if (isNewValue
				&& !Parent.IsProjectRelatedIncident
				&& Parent.ClientCompanyCode_Visible
				&& !Parent.IM_LCC_ReadOnly
				&& Parent.Lookups.ClientCompanyCodeDescriptionPairList.Count > 0
				&& !Parent.IM_LCC.IsValid)
			{
				info.AddError("Company is mandatory when the Database Product is ediEnterprise / CargoWiseOne / CargoWiseNext.");
			}

			if (isNewValue)
			{
				ListValidation.ErrorIfInvalidCode(info, Parent.Lookups.ActiveClientCompanyCodeDescriptionPairList);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(info, Parent.Lookups.ActiveClientCompanyCodeDescriptionPairList);
			}

			if (!info.HasErrors())
			{
				var clientCompany = Parent.ClientCompany;
				if (clientCompany != null && !clientCompany.LCC_OH.IsEmpty && Parent.IM_OH_Client.IsValid && clientCompany.LCC_OH != Parent.IM_OH_Client)
				{
					if (!Parent.IsInDatabase)
					{
						const string incorrectClientMessage = "Please select a Company that belongs to the selected Client.";
						info.AddError(incorrectClientMessage);
					}
				}
			}
		}

		#endregion

		#region IM_Status

		protected override void CheckIM_Status()
		{
			base.CheckIM_Status();
			MandatoryValidation.CheckEntered(Parent.IM_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IM_StatusInfo, Parent.Lookups.StatusList);
		}

		#endregion

		#region IM_ResolutionCode

		protected override void CheckIM_ResolutionCode()
		{
			base.CheckIM_ResolutionCode();
			MandatoryValidation.CheckEntered(Parent.IM_ResolutionCodeInfo);
		}

		#endregion

		#region IM_Product

		protected override void CheckIM_Product()
		{
			base.CheckIM_Product();

			//Updates to this Validation should also be made in CheckING_Product
			MandatoryValidation.CheckEntered(Parent.IM_ProductInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IM_ProductInfo, Parent.Lookups.ProductList);
		}

		#endregion

		#region IM_Module

		protected override void CheckIM_Module()
		{
			base.CheckIM_Module();

			//Updates to this Validation should also be made in CheckING_Module
			MandatoryValidation.CheckEntered(Parent.IM_ModuleInfo);

			var hasBeenChanged = !Parent.IsInDatabase || Parent.IM_ModuleInfo.HasChanges || Parent.ProductAreaInfo.HasChanges;
			if (hasBeenChanged)
			{
				ListValidation.ErrorIfInvalidCode(Parent.IM_ModuleInfo, Parent.Lookups.ModuleListEnabledModulesOnly);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.IM_ModuleInfo, Parent.Lookups.ProductAreaIndependentModuleList);
			}

			if (Parent.ProductArea.IsEmpty)
			{
				Parent.IM_ModuleInfo.AddWarning("Not linked to any product areas.");
			}
		}

		#endregion

		#region IM_Priority

		protected override void CheckIM_Priority()
		{
			base.CheckIM_Priority();
			MandatoryValidation.CheckEntered(Parent.IM_PriorityInfo);

			var criticalityList = Parent.Lookups.CriticalityList;
			//Updates to this Validation should also be made in CheckING_Priority
			ListValidation.ErrorIfInvalidCode(Parent.IM_PriorityInfo, criticalityList);

			if (criticalityList.ContainsCode(Parent.IM_Priority))
			{
				List<ZString> validCriticalities;
				var validCriticalitiesMap = (!Parent.IsInDatabase || Parent.IM_PriorityInfo.HasChanges) ? Parent.Lookups.ActiveStageCriticalityMapping : Parent.Lookups.AllStageCriticalityMapping;

				var systemProductCollection = new SystemProductCollection();
				if (Parent.IM_Priority == Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement)
				{
					systemProductCollection = EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value;
				}
				else if (Parent.IM_Priority == Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
				{
					systemProductCollection = EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value;
				}
				else
				{
					systemProductCollection = EDIDataRegistry.Instance.SystemProductMappings.Value;
				}

				var product = systemProductCollection.Cast<SystemProduct>().FirstOrDefault(x => x.Code.ToUpper() == Parent.IM_Product);
				if (product != null)
				{
					var isEnabledForProduct = product.Enabled;
					if (!isEnabledForProduct)
					{
						Parent.IM_PriorityInfo.AddError(string.Format(CultureInfo.InvariantCulture, "This product {0} is not enabled for {1} criticality.", Parent.IM_Product, Parent.IM_Priority));
					}
				}

				if (validCriticalitiesMap.TryGetValue(Parent.IM_Category, out validCriticalities) && !validCriticalities.Contains(Parent.IM_Priority))
				{
					var errorMessage = GetCriticalityErrorMessage(criticalityList, validCriticalities);
					Parent.IM_PriorityInfo.AddError(errorMessage);
				}

				if ((Parent.IM_Priority == Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement || Parent.IM_Priority == Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
						&& !Parent.ClientSupportsCr8Cr9)
				{
					Parent.IM_PriorityInfo.AddWarning("The version that the client is currently on does not support CR8/CR9 criticalities. It will appear as CR5 for the client.");
				}
			}
		}

		string GetCriticalityErrorMessage(CodeDescriptionPairList criticalityList, List<ZString> validCriticalities)
		{
			var builder = new ZStringBuilder();
			builder.AppendLine(string.Format(CultureInfo.CurrentCulture, "A {0} can only be in the following criticalities:", Parent.Stage));
			foreach (var criticality in validCriticalities)
			{
				builder.AppendLine(string.Format(CultureInfo.CurrentCulture, " - {0} ({1})", criticality, criticalityList[criticality, StringComparison.Ordinal].Description));
			}
			return builder.ToString().Trim();
		}

		#endregion

		#region IM_ActualHoursWorked

		protected override void CheckIM_ActualHoursWorked()
		{
			base.CheckIM_ActualHoursWorked();
			if (Parent.IM_ChargableWork)
			{
				MandatoryValidation.CheckEntered(Parent.IM_ActualHoursWorkedInfo);
			}
		}

		#endregion

		#region IM_ClientBugSeverity

		protected override void CheckIM_ClientBugSeverity()
		{
			base.CheckIM_ClientBugSeverity();
			ListValidation.ErrorIfInvalidCode(Parent.IM_ClientBugSeverityInfo, Parent.Lookups.BugSeverities);
		}

		#endregion

		#region IM_FeatureRequestIndustryValue

		protected override void CheckIM_FeatureRequestIndustryValue()
		{
			base.CheckIM_FeatureRequestIndustryValue();
			ListValidation.ErrorIfInvalidCode(Parent.IM_FeatureRequestIndustryValueInfo, Parent.Lookups.FeatureRequestIndustryValueList);
		}

		#endregion

		#region IM_Source

		protected override void CheckIM_Source()
		{
			base.CheckIM_Source();
			MandatoryValidation.CheckEntered(Parent.IM_SourceInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IM_SourceInfo, Parent.Lookups.SourceList);
		}

		#endregion

		#region IM_GS_NKSpecifiedBy

		protected override void CheckIM_GS_NKSpecifiedBy()
		{
			base.CheckIM_GS_NKSpecifiedBy();
			if (Parent.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && Parent.IsProjectRelatedIncident)
			{
				MandatoryValidation.CheckEntered(Parent.IM_GS_NKSpecifiedByInfo);
			}
		}

		#endregion

		#region IM_Category

		protected override void CheckIM_Category()
		{
			base.CheckIM_Category();
			MandatoryValidation.CheckEntered(Parent.IM_CategoryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IM_CategoryInfo, Parent.Lookups.StageList);
		}

		#endregion

		#region IM_OC_Contact

		protected override void CheckIM_OC_Contact()
		{
			base.CheckIM_OC_Contact();
			MandatoryValidation.CheckEntered(Parent.IM_OC_ContactInfo);
			ListValidation.ErrorIfInvalidPK(Parent.IM_OC_ContactInfo, Parent.Lookups.Contacts);

			if (Parent.Contact != null)
			{
				if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.Contact.OC_Email))
				{
					var message = "Contact does not have a valid email address";
					if (Parent.ClientSystemSupportsBiDirectionUpdate)
					{
						Parent.IM_OC_ContactInfo.AddWarning(message);
					}
					else
					{
						Parent.IM_OC_ContactInfo.AddError(message);
					}
				}
			}
		}

		#endregion

		#region IM_ProgramArea

		protected override void CheckIM_ProgramArea()
		{
			base.CheckIM_ProgramArea();

			//Updates to this Validation should also be made in CheckING_ProductArea
			ListValidation.ErrorIfInvalidCode(Parent.ProductAreaInfo, Parent.Lookups.FilteredProductAreaList);

			if (!Parent.ProductAreaInfo.HasErrors() && Parent.IncidentTriage != null && !Parent.IM_Module.IsEmpty && !Parent.IM_SourceModuleId.IsEmpty)
			{
				var productAreaModuleMappingsRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(Parent.ModuleType);

				if (productAreaModuleMappingsRegistryItem == null)
				{
					return;
				}

				var systemProduct = productAreaModuleMappingsRegistryItem.Value.GetProductByCode(Parent.IM_Product);
				if (systemProduct == null)
				{
					return;
				}

				var moduleMapping = systemProduct.ModuleMappings.Cast<ProductAreaModuleMapping>().FirstOrDefault(x => x.ModuleCode == Parent.IM_Module);
				var menuItemProductArea = moduleMapping?.SourceModuleMappings.Cast<ProductAreaSourceModuleMapping>().FirstOrDefault(x => x.Code == Parent.IM_SourceModuleId)?.ProductArea ?? string.Empty;

				if (string.IsNullOrEmpty(menuItemProductArea))
				{
					return;
				}

				if (Parent.ProductArea == Parent.IncidentTriage.IMT_ProductArea)
				{
					Parent.ProductAreaInfo.AddWarning(Res.GetString("938f0a5c-fd0a-4471-8126-e7798c075917", "The Product Area matches the attached Triage Node, but conflicts with the expected Product Area {0} for the selected Menu Item.", menuItemProductArea));
				}
				else if (Parent.ProductArea == menuItemProductArea)
				{
					Parent.ProductAreaInfo.AddWarning(Res.GetString("825c9520-c31b-4a12-989e-062a5915604d", "The Product Area matches the attached selected Menu Item, but conflicts with the expected Product Area {0} for the Triage Node.", Parent.IncidentTriage.IMT_ProductArea));
				}
			}
		}

		#endregion

		#region Project Related Incident Validation

		public void ValidateRelatedProjectPK()
		{
			ValidateCalculatedProperty(Parent.RelatedProjectPKInfo);
		}

		protected void CheckRelatedProjectPK()
		{
			if (Parent.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest)
			{
				List<EDIProject> otherRelatedProjects = new List<EDIProject>(Parent.RelatedItems.GetElements<EDIProject>(p => p.PK != Parent.RelatedProjectPK));
				if (otherRelatedProjects.Count > 0)
				{
					string otherRelatedProjectsAsString = string.Join(", ", otherRelatedProjects.ConvertAll<string>(p => p.WKP_ProjectNumber).ToArray());
					string warningMessage = string.Format(CultureInfo.CurrentCulture, "This Feature Request is also related to {0}: {1}",
						otherRelatedProjects.Count > 1 ? "other projects" : "another project",
						otherRelatedProjectsAsString);
					Parent.RelatedProjectPKInfo.AddWarning(warningMessage);
				}

				if (Parent.IM_Source == SupportIncidentLookups.SourceListConstants.CreatedFromProject)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.RelatedProjectPKInfo);
				}
			}
		}

		#endregion

		#region Defect Caused By Work Item Validation

		public void ValidateDefectCausedByWorkItemPK()
		{
			ValidateCalculatedProperty(Parent.DefectCausedByWorkItemPKInfo);
		}

		protected void CheckDefectCausedByWorkItemPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.DefectCausedByWorkItemPKInfo, Parent.Lookups.WorkItems);
			if (Parent.IM_Category == SupportIncidentCategoriesList.Codes.Defect)
			{
				if (Parent.DefectCausedByWorkItemPK.IsEmpty)
				{
					Parent.DefectCausedByWorkItemPKInfo.AddWarning("Please specify a causing work item.");
				}
				else
				{
					if (Parent.RelatedItems.FindByPK(Parent.DefectCausedByWorkItemPK) != null)
					{
						Parent.DefectCausedByWorkItemPKInfo.AddError("Causing WorkItem cannot also be a Related WorkItem. It cannot be both the cause and the fix of an incident.");
					}
				}
			}
		}

		#endregion

		#region DetailNoteText

		public void ValidateDetailNoteText()
		{
			ValidateCalculatedProperty(Parent.DetailNoteTextInfo);
		}

		protected void CheckDetailNoteText()
		{
			MandatoryValidation.CheckEntered(Parent.DetailNoteTextInfo);
		}

		#endregion

		#region FeatureRequestClientAddressPK

		public void ValidateFeatureRequestClientAddressPK()
		{
			ValidateCalculatedProperty(Parent.FeatureRequestClientAddressPKInfo);
		}

		protected void CheckFeatureRequestClientAddressPK()
		{
			if (Parent.IsFeatureRequest)
			{
				MandatoryValidation.CheckEntered(Parent.FeatureRequestClientAddressPKInfo);
			}
			ListValidation.ErrorIfInvalidPK(Parent.FeatureRequestClientAddressPKInfo);
		}

		#endregion

		#region FeatureRequestContactPK

		public void ValidateFeatureRequestContactPK()
		{
			ValidateCalculatedProperty(Parent.FeatureRequestContactPKInfo);
		}

		protected void CheckFeatureRequestContactPK()
		{
			if (Parent.IsFeatureRequest)
			{
				MandatoryValidation.CheckEntered(Parent.FeatureRequestContactPKInfo);
			}

			if (Parent.FeatureRequestContactPK.IsValid && Parent.FeatureRequestContact != null && Parent.FeatureRequestContactPK == Parent.DbFeatureRequestContactPK)
			{
				ListValidation.WarnIfInvalidPK(Parent.FeatureRequestContactPKInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(Parent.FeatureRequestContactPKInfo);
			}
		}

		#endregion

		#region Language

		protected override void CheckIM_Language()
		{
			base.CheckIM_Language();
			MandatoryValidation.CheckEntered(Parent.IM_LanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IM_LanguageInfo);
		}

		#endregion

		#region Country

		protected override void CheckIM_RN_NKCountry()
		{
			base.CheckIM_RN_NKCountry();
			//Updates to this validation should also be made in CheckING_RN_NKCountry
			ListValidation.ErrorIfInvalidCode(Parent.IM_RN_NKCountryInfo);
		}

		#endregion

		#region IM_ServiceType

		protected override void CheckIM_ServiceType()
		{
			base.CheckIM_ServiceType();

			//Updates to this Validation should also be made in CheckING_ServiceType
			ListValidation.ErrorIfInvalidCode(Parent.IM_ServiceTypeInfo, Parent.Lookups.ServiceTypeList);
		}

		#endregion

		#region IM_ServiceStatus

		protected override void CheckIM_ServiceStatus()
		{
			base.CheckIM_ServiceStatus();

			//Updates to this Validation should also be made in CheckING_ServiceStatus
			ListValidation.ErrorIfInvalidCode(Parent.IM_ServiceStatusInfo, Parent.Lookups.ServiceStatusList);
		}

		#endregion

		#region Validate Workflow Related Properties

		public bool ValidateWorkflowTemplateMatchingProperties()
		{
			ValidateIM_Product();
			ValidateIM_ProgramArea();
			ValidateIM_Module();
			ValidateIM_SourceModuleId();
			ValidateIM_Language();
			ValidateIM_RN_NKCountry();

			return !Parent.IM_ProductInfo.HasErrors()
				&& !Parent.IM_ProgramAreaInfo.HasErrors()
				&& !Parent.IM_ModuleInfo.HasErrors()
				&& !Parent.IM_SourceModuleIdInfo.HasErrors()
				&& !Parent.IM_LanguageInfo.HasErrors()
				&& !Parent.IM_RN_NKCountryInfo.HasErrors();
		}

		public bool ValidateWorkflowServiceTypeProperties()
		{
			ValidateIM_Product();
			ValidateIM_ProgramArea();
			ValidateIM_Module();
			ValidateIM_ServiceType();
			MandatoryValidation.CheckEntered(Parent.IM_ServiceTypeInfo);

			return !Parent.IM_ProductInfo.HasErrors()
				&& !Parent.IM_ProgramAreaInfo.HasErrors()
				&& !Parent.IM_ModuleInfo.HasErrors()
				&& !Parent.IM_ServiceTypeInfo.HasErrors();
		}

		#endregion
	}
}
