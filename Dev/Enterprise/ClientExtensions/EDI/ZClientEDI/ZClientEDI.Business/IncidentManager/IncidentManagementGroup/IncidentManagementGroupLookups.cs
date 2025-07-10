using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroupConstants;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupLookups : AutoIncidentManagementGroupLookups
	{
		public IncidentManagementGroupLookups(AutoIncidentManagementGroup parent) : base(parent)
		{
		}

		public IncidentManagementGroupLookups(BusinessObjectFactory factory) : base(null)
		{
			this.factory = factory;
		}

		public IncidentManagementGroupLookups(FilterStripBusinessObject businessObject) : base(null)
		{
			factory = businessObject.Factory;
		}

		protected override BusinessObjectFactory Factory => factory ?? base.Factory;
		readonly BusinessObjectFactory factory;

		public new IncidentManagementGroup Parent
		{
			get { return (IncidentManagementGroup)base.Parent; }
		}

		public CodeDescriptionPairList Types
		{
			get
			{
				return Factory.GetCachedValue(
					"IncidentManagementGroupLookups.Types",
				() =>
				{
					var typeArray = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value.Cast<IncidentGroupType>().ToArray();
					var types = new CodeDescriptionPairList();
					typeArray.ForEach(x => types.AddPair(x.GroupType, x.Description));
					return types;
				});
			}
		}

		public CodeDescriptionPairList StageList
		{
			get
			{
				if (Parent.ING_Type.IsEmpty)
				{
					return new CodeDescriptionPairList();
				}

				var stages = Parent.Stages;
				var statuses = new CodeDescriptionPairList();
				stages.Cast<IncidentGroupStatusConfiguration>().ForEach(x => statuses.AddPair(x.Code, x.DescriptionOnGroup));

				return statuses;
			}
		}

		public CodeDescriptionPairList ServiceOutageList
		{
			get
			{
				var outageList = new CodeDescriptionPairList();

				outageList.AddPair(ServiceOutageCodes.Investigating, "INVESTIGATING");
				outageList.AddPair(ServiceOutageCodes.Active, "ACTIVE");
				outageList.AddPair(ServiceOutageCodes.Downgraded, "DOWNGRADED");
				outageList.AddPair(ServiceOutageCodes.Restored, "RESTORED");

				return outageList;
			}
		}

		public CodeDescriptionPairList BusinessImpactList
		{
			get
			{
				var impactList = new CodeDescriptionPairList();

				impactList.AddPair(BusinessImpactCodes.Emergency, "EMERGENCY");
				impactList.AddPair(BusinessImpactCodes.HighImpact, "HIGH IMPACT");
				impactList.AddPair(BusinessImpactCodes.Significant, "SIGNIFICANT");
				impactList.AddPair(BusinessImpactCodes.Moderate, "MODERATE");
				impactList.AddPair(BusinessImpactCodes.MinorLocalised, "MINOR/LOCALISED");

				return impactList;
			}
		}

		public CodeDescriptionPairList UrgencyList
		{
			get
			{
				var urgencyList = new CodeDescriptionPairList();

				urgencyList.AddPair(UrgencyCodes.Critical, "CRITICAL");
				urgencyList.AddPair(UrgencyCodes.VeryHigh, "VERY HIGH");
				urgencyList.AddPair(UrgencyCodes.High, "HIGH");
				urgencyList.AddPair(UrgencyCodes.Medium, "MEDIUM");
				urgencyList.AddPair(UrgencyCodes.Low, "LOW");

				return urgencyList;
			}
		}

		public CodeDescriptionPairList ModuleListAllModules
		{
			get
			{
				if (Parent != null)
				{
					return ModuleList;
				}
				else
				{
					return IncidentDetailsLookupsHelper.GetModuleList(Factory);
				}
			}
		}

		public IBusinessObjectCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		protected virtual IModuleListBuilder GetNewModuleListBuilder()
		{
			return new ModuleListBuilder();
		}

		public SupportIncidentCollection IncidentsNotLinked
		{
			get
			{
				if (incidentsNotLinked == null)
				{
					RefreshIncidentNotLinked();
				}
				return incidentsNotLinked;
			}
		}

		public void RefreshIncidentNotLinked()
		{
			var query = new ZDBOnlyQuery(typeof(SupportIncident));
			var subIncidentManagementLinkQuery = new ZDBOnlySubQuery(typeof(IncidentManagementLink), IncidentManagementLinkSchema.INL_IM_Incident, notIn: true);
			query.AddSubQuery(IncidentMainSchema.PK, subIncidentManagementLinkQuery, JoinCondition.And);

			incidentsNotLinked = new SupportIncidentCollection(Factory, query);
			incidentsNotLinked.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("62f8e677-ca7f-4f6c-9a72-df092307b0f0", "This incident is already linked to a management group. Incidents cannot be linked to multiple management groups."));
		}

		SupportIncidentCollection incidentsNotLinked;

		public WorkItemCollection WorkItemList
		{
			get
			{
				if (workItemList == null)
				{
					workItemList = new WorkItemCollection(Factory);
				}

				return workItemList;
			}
		}
		WorkItemCollection workItemList;

		public CodeDescriptionPairList AutoReplyDescriptionList
		{
			get
			{
				var autoReplyList = new CodeDescriptionPairList();

				autoReplyList.AddPair(AutoReplyDescriptions.Manual, "MANUAL");
				autoReplyList.AddPair(AutoReplyDescriptions.AutoReplyOnce, "AUTO-REPLY ONCE");

				return autoReplyList;
			}
		}

		public CodeDescriptionPairList ProductList => IncidentDetailsLookupsHelper.ProductList;
		public CodeDescriptionPairList ProductAreaList => SupportIncidentLookups.GetFilteredProductAreaList(Factory, Parent.ModuleType, Parent.ING_Product);
		public CodeDescriptionPairList CriticalityList => IncidentDetailsLookupsHelper.CriticalityList;
		public CodeDescriptionPairList ModuleListEnabledModulesOnly => IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(Parent);
		public CodeDescriptionPairList ModuleList => IncidentDetailsLookupsHelper.GetModuleList(Factory, Parent.ModuleType, Parent.ING_Product, Parent.ING_ProductArea);
		public CodeDescriptionPairList ProductAreaIndependentModuleList => IncidentDetailsLookupsHelper.GetModuleList(Factory, Parent.ModuleType, Parent.ING_Product, ZString.Empty);
		public CodeDescriptionPairList ServiceTypeList => IncidentDetailsLookupsHelper.GetServiceTypeList(Parent);

		public IncidentTriageCollection TriageList => Factory.GetCachedValue("IncidentManagementGroupLookups.TriageList", () => new IncidentTriageCollection(Factory));
	}
}
