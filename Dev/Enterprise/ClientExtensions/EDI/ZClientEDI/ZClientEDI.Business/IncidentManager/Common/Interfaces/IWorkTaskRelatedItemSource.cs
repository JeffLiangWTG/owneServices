
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.OpportunityManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

using WorkTaskRelatedItemModuleInfo = Enterprise.ProcessManagement.Business.WorkTaskRelatedItemModuleInfo;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class EDIWorkTaskRelatedItemModuleInfo
	{
		public static WorkTaskRelatedItemModuleInfo Issue(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Issue", delegate
			{
				return new WorkTaskRelatedItemModuleInfo
				{
					Caption = "Issue",
					Type = EDIWorkTaskRelatedItemTypes.Issue,
					AllowNew = false,
					AllowAttach = true,
					ControllerID = ClientControllerRegistration.IssueManager,
					ModuleID = ClientModuleRegistration.IssueManager,
					FindBoxList = new IssueManager.Business.HelpErrorLogCollection(factory)
				};
			});
		}

		public static WorkTaskRelatedItemModuleInfo Quote(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Quote", delegate
			{
				return Quote(factory, true);
			});
		}

		public static WorkTaskRelatedItemModuleInfo Quote(BusinessObjectFactory factory, bool allowNew)
		{
			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = "Professional Services Quote",
				Type = EDIWorkTaskRelatedItemTypes.ProfessionalServiceQuote,
				AllowNew = allowNew,
				AllowAttach = true,
				ControllerID = ClientControllerRegistration.ProfessionalServicesQuote,
				ModuleID = ClientModuleRegistration.ProfessionalServicesQuote,
				FindBoxList = new ProfessionalServicesQuoteCollection(factory)
			};
		}

		public static WorkTaskRelatedItemModuleInfo GenericIncident(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GenericIncident", delegate
			{
				return GenericIncident(factory, true);
			});
		}

		public static WorkTaskRelatedItemModuleInfo GenericIncident(BusinessObjectFactory factory, bool allowAttach, bool allowNew = false)
		{
			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = "Incident",
				AllowAttach = allowAttach,
				AllowNew = allowNew,
				ControllerID = ClientControllerRegistration.SupportIncident,
				ModuleID = ClientModuleRegistration.SupportIncident,
				FindBoxList = new SupportIncidentCollection(factory)
			};
		}

		public static WorkTaskRelatedItemModuleInfo EscalatedIncident(BusinessObjectFactory factory, ZQuery additionalFilter)
		{
			ZQuery query = new ZQuery(IncidentMainSchema.IM_Category, SQLComparisonOperator.NotEqual, SupportIncidentCategoriesList.Codes.Support);

			if (additionalFilter != null)
			{
				query.AddToFilter(additionalFilter);
			}

			SupportIncidentCollection findBoxList = new SupportIncidentCollection(factory);
			findBoxList.SetOverrideNotificationWhenAdditionalFilterNotMet("Incidents linking to this WorkItem as 'Caused By WI' or on Support stage cannot be chosen here. Please select a Defect / Feature Request / Compliance Requirement / Service Request / Content Development and which does not link to this WorkItem as 'Caused By WI'.");

			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = "Defect / Feature Request / Compliance Requirement / Service Request / Content Development",
				AllowNew = false,
				AllowAttach = true,
				ControllerID = ClientControllerRegistration.SupportIncident,
				ModuleID = ClientModuleRegistration.SupportIncident,
				FindBoxList = findBoxList,
				AdditionalFilterForFindBox = query
			};
		}

		public static WorkTaskRelatedItemModuleInfo FeatureRequest(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("FeatureRequest", delegate
			{
				return new WorkTaskRelatedItemModuleInfo
				{
					Caption = "Feature Request",
					Type = EDIWorkTaskRelatedItemTypes.FeatureRequest,
					AllowNew = true,
					AllowAttach = false,
					ControllerID = ClientControllerRegistration.SupportIncident,
					ModuleID = ClientModuleRegistration.SupportIncident
				};
			});
		}

		public static WorkTaskRelatedItemModuleInfo Defect(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Defect", delegate
			{
				return new WorkTaskRelatedItemModuleInfo
				{
					Caption = "Defect",
					Type = EDIWorkTaskRelatedItemTypes.Defect,
					AllowNew = true,
					AllowAttach = false,
					ControllerID = ClientControllerRegistration.SupportIncident,
					ModuleID = ClientModuleRegistration.SupportIncident
				};
			});
		}

		public static WorkTaskRelatedItemModuleInfo NewWorkItem(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("WorkItem", delegate
			{
				return NewWorkItem(factory, true);
			});
		}

		public static WorkTaskRelatedItemModuleInfo NewWorkItem(BusinessObjectFactory factory, bool allowNew)
		{
			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = "Work Item",
				Type = ProcessManagement.Business.WorkTaskRelatedItemTypes.WorkItem,
				AllowNew = allowNew,
				AllowAttach = true,
				ControllerID = ControllerIDs.WorkItem,
				ModuleID = ModuleIDs.WorkItem,
				FindBoxList = new NewWorkItemCollection(factory)
			};
		}

		public static WorkTaskRelatedItemModuleInfo NewWorkItem(BusinessObjectFactory factory, bool allowNew, ZQuery additionalFilter)
		{
			NewWorkItemCollection findBoxList = new NewWorkItemCollection(factory);
			findBoxList.SetOverrideNotificationWhenAdditionalFilterNotMet("Causing WorkItem cannot also be a Related WorkItem. It cannot be both the cause and the fix of an incident.");

			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = "Work Item",
				Type = ProcessManagement.Business.WorkTaskRelatedItemTypes.WorkItem,
				AllowNew = allowNew,
				AllowAttach = true,
				ControllerID = ControllerIDs.WorkItem,
				ModuleID = ModuleIDs.WorkItem,
				FindBoxList = findBoxList,
				AdditionalFilterForFindBox = additionalFilter
			};
		}

		public static WorkTaskRelatedItemModuleInfo EDIProject(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Project", delegate
			{
				return EDIProject(factory, true);
			});
		}

		public static WorkTaskRelatedItemModuleInfo EDIProject(BusinessObjectFactory factory, bool allowNew)
		{
			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = "Project",
				Type = ProcessManagement.Business.WorkTaskRelatedItemTypes.Project,
				AllowNew = allowNew,
				AllowAttach = true,
				ControllerID = ControllerIDs.Project,
				ModuleID = ModuleIDs.Project,
				FindBoxList = new ProjectCollection(factory)
			};
		}

		public static WorkTaskRelatedItemModuleInfo Opportunity(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Opportunity", delegate
			{
				return Opportunity(factory, true);
			});
		}

		public static WorkTaskRelatedItemModuleInfo Opportunity(BusinessObjectFactory factory, bool allowNew, SupportIncident supportIncident = null)
		{
			var list = (supportIncident == null) ? new EDIOrgOpportunityCollection(factory) : new EDIOrgOpportunityCollectionForIncident(factory, supportIncident);
			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = "Opportunity",
				Type = ProcessManagement.Business.WorkTaskRelatedItemTypes.Opportunity,
				AllowNew = allowNew,
				AllowAttach = true,
				ControllerID = ControllerIDs.Opportunity,
				ModuleID = ModuleIDs.Opportunity,
				FindBoxList = list
			};
		}
	}
}
