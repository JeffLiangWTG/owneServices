using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentCloseAction : SupportIncidentAction
	{
		public SupportIncidentCloseAction(SupportIncident incident)
			: base(incident)
		{
			originalCriticality = Incident.IM_Priority;
			this.Criticality = Incident.IM_Priority;
		}

		protected override void PerformAction()
		{
			if (!ResolutionMethod.IsEmpty)
			{
				Incident.SetCriticalityWithoutLoggingReason(Criticality);

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

				if (ResolutionMethod == SupportIncidentLookups.DispositionList.Constants.FeatureAccepted)
				{
					Incident.CloseAsAcceptedFeatureRequest(Comment);
				}
				else
				{
					if (PostIRSEvent && IRSEventTimeUtcOverride.IsValid)
					{
						Incident.CloseIncident(ResolutionMethod, Comment, IRSEventTimeUtcOverride);
					}
					else
					{
						Incident.CloseIncident(ResolutionMethod, Comment);
					}
					
					if (SendDevelopmentEstimate || SendSoftwareQuote)
					{
						if (SendDevelopmentEstimate)
						{
							Incident.Estimate.CIE_EstimateSentDateUTC = ZDateTime.UtcNow;
						}
						else if (SendSoftwareQuote)
						{
							Incident.Quote.CIQ_QuoteSentDateUTC = ZDateTime.UtcNow;
						}

						Incident.EstimateOrQuoteAdded(Comment, eDoc);
					}
				}
			}
		}

		#region Properties

		#region Resolution Method

		[MaxLength(SupportIncident.Schema.IM_ResolutionCodeMaxLength)]
		public ZString ResolutionMethod
		{
			get { return resolutionMethod; }
			set
			{
				if (resolutionMethod != value)
				{
					CheckMaximumLength(ResolutionMethodInfo, value);
					resolutionMethod = value;
					ResolutionMethodInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						ValidateResolutionMethod();
					}
				}
			}
		}
		ZString resolutionMethod;

		public ZPropertyInfo ResolutionMethodInfo
		{
			get { return GetZPropertyInfo(nameof(ResolutionMethod)); }
		}

		public CodeDescriptionPairList ActiveCloseStatusDispositionList
		{
			get
			{
				return GetCloseStatusDispositionList();
			}
		}

		public CodeDescriptionPairList GetCloseStatusDispositionList(bool activeOnly = true)
		{
			return Factory.GetCachedValue(
				"SupportIncidentCloseAction.GetCloseStatusDispositionList:" + Incident.IM_Category + ":" + Criticality + ":" + Incident.IM_Product + ";" + activeOnly.ToString(),
				() =>
				{
					var tree = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
					var result = tree.GetChildrenExactMatchOnly(activeOnly, Incident.IM_Category, Criticality, Incident.IM_Product);

					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, Incident.IM_Category, Criticality, CodeDescriptionBoolTreeNode.AllCode);
					}
					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, Incident.IM_Category, CodeDescriptionBoolTreeNode.AllCode, Incident.IM_Product);
					}
					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, Incident.IM_Category, CodeDescriptionBoolTreeNode.AllCode, CodeDescriptionBoolTreeNode.AllCode);
					}

					if (Incident.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest)
					{
						result.AddPairIfNotExist(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, "Feature Request Accepted");
						if (Incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
						{
							result.AddPairIfNotExist(SupportIncidentLookups.DispositionList.Constants.FormalQuotationAccepted, "Formal Quotation Accepted");
							result.AddPairIfNotExist(SupportIncidentLookups.DispositionList.Constants.FormalQuotationDeclined, "Formal Quotation Declined");
						}
					}

					return result;
				});
		}

		public CodeDescriptionPairList CloseStatusDispositionListToValidate
		{
			get
			{
				return Factory.GetCachedValue(
					"SupportIncidentCloseAction.CloseStatusDispositionListToValidate:" + Incident.IM_Category + ":" + Criticality + ":" + Incident.IM_Product,
					() =>
					{
						var result = new CodeDescriptionPairList(ActiveCloseStatusDispositionList);

						if (Incident.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && Incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
						{
								result.AddPairIfNotExist(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided, "Development Estimate Provided - Awaiting Customer");
								result.AddPairIfNotExist(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, "Formal Quotation Provided - Awaiting Customer");
						}

						return result;
					});
			}
		}

		#endregion

		public ZString PrePopulateResolutionMethod { get; set; }

		public ZDateTime IRSEventTimeUtcOverride { get; set; }

		public bool PostIRSEvent { get; set; }

		#region Criticality

		[List("CriticalityList")]
		[MaxLength(SupportIncident.Schema.IM_PriorityMaxLength)]
		public ZString Criticality
		{
			get { return criticality; }
			set
			{
				if (criticality != value)
				{
					CheckMaximumLength(CriticalityInfo, value);
					criticality = value;
					CriticalityInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						ValidateCriticality();
					}
				}
			}
		}
		ZString criticality;

		public ZPropertyInfo CriticalityInfo
		{
			get { return GetZPropertyInfo(nameof(Criticality)); }
		}

		public CodeDescriptionPairList CriticalityList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				var fullCriticalityList = Incident.Lookups.CriticalityList;
				List<ZString> validCriticalityList;
				if (Incident.Lookups.ActiveStageCriticalityMapping.TryGetValue(Incident.IM_Category, out validCriticalityList))
				{
					foreach (var criticalityCode in validCriticalityList)
					{
						result.AddPair(criticalityCode, fullCriticalityList.GetDescriptionFromCode(criticalityCode));
					}
				}

				return result;
			}
		}

		readonly ZString originalCriticality;

		#endregion

		#region eDoc

		public IeDoc eDoc { get; set; }

		#endregion

		#region Flag Properties

		public ZBool SendDevelopmentEstimate { get; set; }
		public ZBool SendSoftwareQuote { get; set; }

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
			var moduleListType = IncidentApprovalLookups.GetModuleListType(Criticality);
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
				var moduleType = IncidentApprovalLookups.GetModuleListType(Criticality);

				return Factory.GetCachedValue(
					"SupportIncidentCloseAction.SectionRequirementServiceList:" + moduleType + ":" + Incident.IM_Product + ":" + ProductArea,
					() =>
					{
						return Incident.Lookups.GetEnabledModuleList(moduleType, Incident.IM_Product, ProductArea);
					});
			}
		}

		public ModuleListType ModuleListType
		{
			get { return IncidentApprovalLookups.GetModuleListType(Criticality); }
		}

		public bool HasModuleListTypeChanged
		{
			get
			{
				var originalModuleListType = IncidentApprovalLookups.GetModuleListType(originalCriticality);
				var newModuleListType = IncidentApprovalLookups.GetModuleListType(Criticality);
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
				var moduleType = IncidentApprovalLookups.GetModuleListType(Criticality);
				return SupportIncidentLookups.GetFilteredProductAreaList(Factory, moduleType, Incident.IM_Product, Incident.IncidentTriage);
			}
		}

		public ZString RecalculateProductArea()
		{
			return IncidentDetailsHelper.FindProductArea(Incident.IM_Product, Criticality, SectionRequirementService, MenuItem);
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateResolutionMethod();
			ValidateCriticality();
			ValidateProductArea();
			ValidateSectionRequirementService();
		}

		public void ValidateResolutionMethod()
		{
			ResolutionMethodInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ResolutionMethodInfo, CloseStatusDispositionListToValidate);
			MandatoryValidation.CheckEntered(ResolutionMethodInfo);
			ValidateAwaitingAutoUpgradeResolutionMethod();
		}

		void ValidateAwaitingAutoUpgradeResolutionMethod()
		{
			if (ResolutionMethod == SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade)
			{
				if (Incident.RelatedWorkItems.Count == 0)
				{
					const string errorForNoWIs = "Cannot be marked as Awaiting Auto Upgrade if no work item attached";
					ResolutionMethodInfo.AddError(errorForNoWIs);
				}
				else if (Incident.Database == null)
				{
					const string errorForNoDatabase = "Cannot be marked as Awaiting Auto Upgrade if the incident is not linked to a database";
					ResolutionMethodInfo.AddError(errorForNoDatabase);
				}
				else
				{
					const string errorForNonClosedWIs = "Cannot be marked as Awaiting Auto Upgrade if work item(s) are still in progress";
					bool hasNonClosedWIs = false;
					foreach (NewWorkItem workItem in Incident.RelatedWorkItems)
					{
						if (workItem.WKI_Status != ProcessTaskStatusCodeList.Codes.Closed &&
							workItem.WKI_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
						{
							hasNonClosedWIs = true;
							break;
						}
					}

					if (hasNonClosedWIs)
					{
						ResolutionMethodInfo.AddError(errorForNonClosedWIs);
					}
				}
			}
		}

		protected override void ValidateComment()
		{
			CommentInfo.ClearAllNotifications();

			if (ResolutionMethod == SupportIncidentLookups.DispositionList.Constants.Closed.Other && Comment.Length < 20)
			{
				CommentInfo.AddError("The resolution comment must be atleast 20 characters when the Resolution Method is Other.");
			}
		}

		public void ValidateCriticality()
		{
			CriticalityInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CriticalityInfo);

			if (resolutionMethod == SupportIncidentLookups.DispositionList.Constants.FeatureAccepted)
			{
				List<ZString> validCriticalities;
				if (Incident.Lookups.ActiveStageCriticalityMapping.TryGetValue(SupportIncidentCategoriesList.Codes.FeatureRequest, out validCriticalities)
					&& !validCriticalities.Contains(Criticality))
				{
					var error = new ZStringBuilder();
					error.AppendLine(string.Format(CultureInfo.CurrentCulture, "A {0} can only be in the following criticalities:", SupportIncidentCategoriesList.Descriptions.FeatureRequest));
					foreach (var crit in validCriticalities)
					{
						error.AppendLine(string.Format(CultureInfo.CurrentCulture, " - {0} ({1})", crit, Incident.Lookups.CriticalityList[crit, StringComparison.Ordinal].Description));
					}
					CriticalityInfo.AddError(error.ToString().Trim());
				}
			}
			else
			{
				var criticalityList = CriticalityList;
				ListValidation.ErrorIfInvalidCode(CriticalityInfo, criticalityList);
				if (criticalityList.ContainsCode(Criticality)
					&& (Criticality == Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement || Criticality == Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
					&& !Incident.ClientSupportsCr8Cr9)
				{
					CriticalityInfo.AddWarning("The version that the client is currently on does not support CR8/CR9 criticalities. It will appear as CR5 for the client.");
				}
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

