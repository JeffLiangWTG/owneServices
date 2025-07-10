using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentEscalateAction : SupportIncidentAction
	{
		public SupportIncidentEscalateAction(SupportIncident incident, ZString originalCriticality)
			: base(incident)
		{
			this.originalCriticality = originalCriticality;
		}

		protected override void PerformAction()
		{
			if (!EscalationStage.IsEmpty)
			{
				Incident.SetCriticalityWithoutLoggingReason(EscalationCriticality);

				if (!MenuItem.IsEmpty)
				{
					Incident.IM_SourceModuleId = MenuItem;
				}

				if (!SectionRequirementService.IsEmpty)
				{
					if (Incident.IM_Module == SectionRequirementService)
					{
						Incident.IM_Module = ZString.Empty;
					}

					Incident.IM_Module = SectionRequirementService;
				}

				Incident.Escalate(EscalationStage, Comment);

				if (Incident.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && Incident.IM_Priority == Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest)
				{
					IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.FeatureAccepted, Incident);
					if (Incident.WorkflowItems.AllTasksClosedOrCancelled)
					{
						Incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.FeatureAccepted;
					}
				}
			}
		}

		#region Properties

		#region Escalation Stage

		[List("EscalationStageList")]
		[MaxLength(SupportIncident.Schema.IM_Category)]
		public ZString EscalationStage
		{
			get { return escalationStage; }
			set
			{
				if (escalationStage != value)
				{
					SetNonPersistentPropertyValue(EscalationStageInfo, ref escalationStage, value);

					if (!IsValidationSuspended)
					{
						ValidateEscalationStage();
					}

					SectionRequirementService = ZString.Empty;
					ProductArea = ZString.Empty;
					MenuItem = ZString.Empty;

					var criticalityList = EscalationCriticalityList;
					if (criticalityList.Count == 1)
					{
						EscalationCriticality = criticalityList[0].Code;
					}
					else if (criticalityList.ContainsCode(originalCriticality))
					{
						EscalationCriticality = originalCriticality;
					}
					else
					{
						EscalationCriticality = ZString.Empty;
					}
				}
			}
		}
		ZString escalationStage;

		public ZPropertyInfo EscalationStageInfo
		{
			get { return GetZPropertyInfo(nameof(EscalationStage)); }
		}

		public CodeDescriptionPairList EscalationStageList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (ICodeDescription stage in new SupportIncidentCategoriesList())
				{
					if (Incident == null || Incident.IM_Category != stage.Code)
					{
						if (Incident != null && Incident.RelatedWorkItems.Count != 0 && stage.Code == SupportIncidentCategoriesList.Codes.Support)
						{
							continue;
						}
						result.Add(stage);
					}
				}

				return result;
			}
		}

		#endregion

		#region Criticality

		[List("EscalationCriticalityList")]
		[MaxLength(SupportIncident.Schema.IM_PriorityMaxLength)]
		public ZString EscalationCriticality
		{
			get { return escalationCriticality; }
			set
			{
				if (escalationCriticality != value)
				{
					SetNonPersistentPropertyValue(EscalationCriticalityInfo, ref escalationCriticality, value);

					SectionRequirementService = ZString.Empty;
					ProductArea = ZString.Empty;
					MenuItem = ZString.Empty;

					if (!IsValidationSuspended)
					{
						ValidateEscalationCriticality();
					}
				}
			}
		}
		ZString escalationCriticality;

		public ZPropertyInfo EscalationCriticalityInfo
		{
			get { return GetZPropertyInfo(nameof(EscalationCriticality)); }
		}

		public CodeDescriptionPairList EscalationCriticalityList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				var fullCriticalityList = Incident.Lookups.CriticalityList;
				List<ZString> validCriticalityList;
				if (Incident.Lookups.ActiveStageCriticalityMapping.TryGetValue(EscalationStage, out validCriticalityList))
				{
					foreach (var criticalityCode in validCriticalityList)
					{
						result.AddPair(criticalityCode, fullCriticalityList.GetDescriptionFromCode(criticalityCode));
					}
				}

				return result;
			}
		}

		public bool EscalationCriticality_ReadOnly
		{
			get { return EscalationStage.IsEmpty; }
		}

		readonly ZString originalCriticality;

		#endregion

		#region Menu Section / Compliance Requirement / Requested Service

		[List("SectionRequirementServiceList")]
		[MaxLength(SupportIncident.Schema.IM_Module)]
		public ZString SectionRequirementService
		{
			get { return sectionRequirementService; }
			set
			{
				if (sectionRequirementService != value)
				{
					SetNonPersistentPropertyValue(SectionRequirementServiceInfo, ref sectionRequirementService, value);
				}

				//Validates value to update error message with correct human readable name
				if (!IsValidationSuspended)
				{
					ValidateSectionRequirementService();
				}

				if (!SectionRequirementServiceInfo.HasErrors())
				{
					ProductArea = RecalculateProductArea();
				}
			}
		}
		ZString sectionRequirementService;

		public ZPropertyInfo SectionRequirementServiceInfo
		{
			get { return GetZPropertyInfo(nameof(SectionRequirementService), GetSectionRequirementServiceLabelText()); }
		}

		public ZString GetSectionRequirementServiceLabelText()
		{
			ZString result;
			var moduleListType = IncidentApprovalLookups.GetModuleListType(EscalationCriticality);
			if (moduleListType == ModuleListType.Cr8)
			{
				result = "Requirement";
			}
			else if (moduleListType == ModuleListType.Cr9)
			{
				result = "Service";
			}
			else
			{
				result = "Menu Section";
			}

			return result;
		}

		public CodeDescriptionPairList SectionRequirementServiceList
		{
			get
			{
				var moduleType = IncidentApprovalLookups.GetModuleListType(EscalationCriticality);
				return Incident.Lookups.GetEnabledModuleList(moduleType, Incident.IM_Product, ProductArea);
			}
		}

		public ModuleListType ModuleListType
		{
			get { return IncidentApprovalLookups.GetModuleListType(EscalationCriticality); }
		}

		public bool HasModuleListTypeChanged
		{
			get
			{
				var originalModuleListType = IncidentApprovalLookups.GetModuleListType(originalCriticality);
				var newModuleListType = IncidentApprovalLookups.GetModuleListType(EscalationCriticality);
				return originalModuleListType != newModuleListType;
			}
		}

		#endregion

		#region Menu Item

		[MaxLength(SupportIncident.Schema.IM_SourceModuleId)]
		public ZString MenuItem
		{
			get { return menuItem; }
			set
			{
				if (menuItem != value)
				{
					SetNonPersistentPropertyValue(MenuItemInfo, ref menuItem, value);
				}
			}
		}

		ZString menuItem;

		public ZPropertyInfo MenuItemInfo
		{
			get { return GetZPropertyInfo(nameof(MenuItem)); }
		}

		public ZString MenuItemWithPath
		{
			get
			{
				var sourceModule = EDIDataRegistry.Instance.SourceModules.Value.GetSourceModule(MenuItem, Incident.IM_Product);
				if (sourceModule != null)
				{
					return sourceModule.Path + " " + sourceModule.Description;
				}

				return MenuItem;
			}
		}

		#endregion

		#region Product Area

		[List("ProductAreaList")]
		[MaxLength(SupportIncident.Schema.IM_ProgramArea)]
		public ZString ProductArea
		{
			get { return productArea; }
			set
			{
				if (productArea != value)
				{
					SetNonPersistentPropertyValue(ProductAreaInfo, ref productArea, value);
					if (!IsValidationSuspended)
					{
						ValidateProductArea();
					}
				}
			}
		}
		ZString productArea;

		public ZPropertyInfo ProductAreaInfo
		{
			get { return GetZPropertyInfo(nameof(ProductArea)); }
		}

		public ZString ProductAreaDescription
		{
			get { return !ProductArea.IsEmpty ? ProductAreaList.GetDescriptionFromCode(ProductArea) : string.Empty; }
		}

		public CodeDescriptionPairList ProductAreaList
		{
			get
			{
				var moduleType = IncidentApprovalLookups.GetModuleListType(EscalationCriticality);
				return SupportIncidentLookups.GetFilteredProductAreaList(Factory, moduleType, Incident.IM_Product, Incident.IncidentTriage);
			}
		}

		public ZString RecalculateProductArea()
		{
			return IncidentDetailsHelper.FindProductArea(Incident.IM_Product, EscalationCriticality, SectionRequirementService, MenuItem);
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEscalationStage();
			ValidateEscalationCriticality();
			ValidateProductArea();
			ValidateSectionRequirementService();
		}

		void ValidateEscalationStage()
		{
			EscalationStageInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EscalationStageInfo);
			ListValidation.ErrorIfInvalidCode(EscalationStageInfo);
		}

		void ValidateEscalationCriticality()
		{
			EscalationCriticalityInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(EscalationCriticalityInfo);
			ListValidation.ErrorIfInvalidCode(EscalationCriticalityInfo);

			if ((EscalationCriticality == Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement || EscalationCriticality == Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
					&& !Incident.ClientSupportsCr8Cr9)
			{
				EscalationCriticalityInfo.AddWarning("The version that the client is currently on does not support CR8/CR9 criticalities. It will appear as CR5 for the client.");
			}
		}

		void ValidateProductArea()
		{
			ProductAreaInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ProductAreaInfo);
		}

		public void ValidateSectionRequirementService()
		{
			SectionRequirementServiceInfo.ClearAllNotifications();
			if (HasModuleListTypeChanged)
			{
				MandatoryValidation.CheckEntered(SectionRequirementServiceInfo);
			}
			ListValidation.ErrorIfInvalidCode(SectionRequirementServiceInfo);
		}

		#endregion
	}
}

