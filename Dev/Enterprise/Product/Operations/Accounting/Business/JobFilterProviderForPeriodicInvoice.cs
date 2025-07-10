using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public class JobFilterProviderForPeriodicInvoice : JobFilterProvider
	{
		public JobFilterProviderForPeriodicInvoice(PeriodicInvoiceBaseJobFilterBusinessObject periodicInvoiceFilter)
			: base()
		{
			Argument.NotNull(periodicInvoiceFilter, "periodicInvoiceFilter");
			this.PeriodicInvoiceFilter = periodicInvoiceFilter;
			this.moduleDecider = new PeriodicInvoiceModuleDecider();

			var list = new List<string>() {
												PeriodicInvoiceBaseJobFilterBusinessObject.ETA,
												PeriodicInvoiceBaseJobFilterBusinessObject.ETD,
												PeriodicInvoiceBaseJobFilterBusinessObject.ATA,
												PeriodicInvoiceBaseJobFilterBusinessObject.ATD,
												PeriodicInvoiceBaseJobFilterBusinessObject.PICKUP_DATE,
												PeriodicInvoiceBaseJobFilterBusinessObject.DELIVERY_DATE,
												PeriodicInvoiceBaseJobFilterBusinessObject.AWB_ISSUE_DATE,
												PeriodicInvoiceBaseJobFilterBusinessObject.COMPLETION_DATE,
												PeriodicInvoiceBaseJobFilterBusinessObject.CUSTOMS_CLEARANCE_DATE,
												PeriodicInvoiceBaseJobFilterBusinessObject.TRANSPORT_MODE,
												PeriodicInvoiceBaseJobFilterBusinessObject.CARRIER,
												PeriodicInvoiceBaseJobFilterBusinessObject.PRINCIPAL,
												PeriodicInvoiceBaseJobFilterBusinessObject.VOYAGEVESSEL,
												PeriodicInvoiceBaseJobFilterBusinessObject.SENDING_AGENT,
												PeriodicInvoiceBaseJobFilterBusinessObject.RECEIVING_AGENT,
												PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_DIRECTION,
												PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_LEVEL,
												PeriodicInvoiceBaseJobFilterBusinessObject.Order_Reference,
												PeriodicInvoiceBaseJobFilterBusinessObject.Additional_Reference,
											};
			if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.Canada)
			{
				list.Insert(0, PeriodicInvoiceBaseJobFilterBusinessObject.ACCOUNTING_DATE);
			}
			allfilters = list.ToArray();

			excludeFilter = true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new JobFilterProviderForPeriodicInvoice(PeriodicInvoiceFilter);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public override ZQuery Filter
		{
			get
			{
				excludeFilter = false;
				var jobtypes = GetJobTypesThatAreCompatibleWithFilter();
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(Job));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GenericJob.GenericJob), ViewGenericJobSchema.PK);

				foreach (PeriodicInvoiceModule module in Enum.GetValues(typeof(PeriodicInvoiceModule)))
				{
					subQuery.AddToFilter(GetQuery(jobtypes, module), JoinCondition.Or);
				}

				query.AddSubQuery(JobHeaderSchema.JH_ParentID, subQuery, JoinCondition.And);
				excludeFilter = true;
				return query;
			}
		}

		#region Query related functions

		protected override ZDBOnlyQuery GetEstimatedOrArrivalDatesQuery(DateComparisonOperator comparisonOperator, SailingFilterBuilder.Dates dateType, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(Job));

			if (!excludeFilter)
			{
				query = base.GetEstimatedOrArrivalDatesQuery(comparisonOperator, dateType, date1, date2);
			}

			return query;
		}

		protected override ZQuery GetDeliveryDateQueryCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetDeliveryDateQueryCore(comparisonOperator, date1, date2);
			}

			return query;
		}

		protected override ZQuery GetPickupDateQueryCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetPickupDateQueryCore(comparisonOperator, date1, date2);
			}

			return query;
		}

		protected override ZQuery GetCustomsClearanceDateQueryCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetCustomsClearanceDateQueryCore(comparisonOperator, date1, date2);
			}

			return query;
		}

		protected override ZQuery GetAWBCutOffDateQueryCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetAWBCutOffDateQueryCore(comparisonOperator, date1, date2);
			}

			return query;
		}

		protected override ZQuery GetCompletionDateCore(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetCompletionDateCore(comparisonOperator, date1, date2);
			}

			return query;
		}

		protected override ZQuery GetTransportModeCore(ZString value)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetTransportModeCore(value);
			}

			return query;
		}

		protected override ZQuery GetCarrierQueryCore(ZGuid carrierPK)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetCarrierQueryCore(carrierPK);
			}

			return query;
		}

		protected override ZQuery GetPrincipalQueryCore(ZGuid principalPK)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetPrincipalQueryCore(principalPK);
			}

			return query;
		}

		protected override ZQuery GetVoyageAndVesselQueryCore(SQLComparisonOperator comCperator, ZString voyage, ZString vessel)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				query = base.GetVoyageAndVesselQueryCore(comCperator, voyage, vessel);
			}

			return query;
		}

		protected override ZQuery GetReceivingAgentQueryCore(ZGuid receivingForwarderPK)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				return base.GetReceivingAgentQueryCore(receivingForwarderPK);
			}

			return query;
		}

		protected override ZQuery GetSendingAgentQueryCore(ZGuid sendingForwarderPK)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				return base.GetSendingAgentQueryCore(sendingForwarderPK);
			}

			return query;
		}

		protected override ZQuery GetServiceDirectionCore(ZString direction)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				return base.GetServiceDirectionCore(direction);
			}

			return query;
		}

		protected override ZQuery GetServiceLevelCore(ZString serviceLevelCode)
		{
			var query = new ZQuery();

			if (!excludeFilter)
			{
				return base.GetServiceLevelCore(serviceLevelCode);
			}

			return query;
		}

		ZQuery GetQuery(List<ZString> selectedJobTypes, PeriodicInvoiceModule module)
		{
			IsForwardingModule = IsCFSModule = IsConsolModule = IsCustomsModule = IsConsignmentModule = IsTransportModule = IsAgencyModule = IsWarehouseModule = false;

			IEnumerable<ZString> jobTypes = moduleDecider.GetJobTypesByPeriodicInvoiceModuleFromList(selectedJobTypes, module);

			switch (module)
			{
				case PeriodicInvoiceModule.CFS:
					IsCFSModule = jobTypes.Any();
					break;
				case PeriodicInvoiceModule.Consol:
					IsConsolModule = jobTypes.Any();
					break;
				case PeriodicInvoiceModule.Customs:
					IsCustomsModule = jobTypes.Any();
					break;
				case PeriodicInvoiceModule.Forwarding:
					IsForwardingModule = jobTypes.Any();
					break;
				case PeriodicInvoiceModule.Consignment:
					IsConsignmentModule = jobTypes.Any();
					break;
				case PeriodicInvoiceModule.Transport:
					IsTransportModule = jobTypes.Any();
					break;
				case PeriodicInvoiceModule.Agency:
					IsAgencyModule = jobTypes.Any();
					break;
				case PeriodicInvoiceModule.Warehouse:
					IsWarehouseModule = jobTypes.Any();
					break;
				default:
					break;
			}

			var query = new ZQuery();

			if (jobTypes != null && jobTypes.Any())
			{
				query = new ZDBOnlyQuery(typeof(GenericJob.GenericJob));
				query.AddToFilter(ViewGenericJobSchema.VJ_JobType, jobTypes);
				query.AddToFilter(ViewGenericJobSchema.VJ_IsInactive, false);
				query.AddToFilter(GetExcludingQuery());

				string[] filterDescriptions = ModuleToFilterMapping.ContainsKey(module) ? ModuleToFilterMapping[module] : null;

				if (filterDescriptions != null && PeriodicInvoiceFilter.ActiveModuleFilters.Count > 0)
				{
					var applicableFilters = new List<ModuleFilter>();

					foreach (ZString filterDescription in filterDescriptions)
					{
						var regEx = new System.Text.RegularExpressions.Regex(filterDescription + "\\s*\\(*[0-9]*\\)*");

						foreach (ModuleFilter filter in PeriodicInvoiceFilter.ActiveModuleFilters)
						{
							if (regEx.IsMatch(filter.Description))
							{
								applicableFilters.Add(filter);
							}
						}
					}

					if (applicableFilters.Any())
					{
						var combiner = new ModuleFilterCombiner();
						var fullInsideQuery = combiner.GetCombinedFilter(applicableFilters);
						query.AddToFilter(fullInsideQuery, JoinCondition.And);
					}
				}
			}

			return query;
		}

		#endregion

		#region Module Filter related functions

		public override ZString[] GetJobTypesApplicableToAFilter(string filterName)
		{
			var jobTypes = new List<ZString>();
			var mapping = PeriodicInvoiceModuleDecider.GetJobTypeToCategoryMapping();
			var modules = ModuleToFilterMapping.Where(x => x.Value.Contains(filterName));
			if (modules.Any())
			{
				modules.ForEach(x => jobTypes.AddRange(mapping[x.Key].ToArray()));
			}
			return jobTypes.ToArray();
		}

		List<ZString> GetJobTypesThatAreCompatibleWithFilter()
		{
			List<ZString> result = new List<ZString>();

			var activeDateAndModeFilters = PeriodicInvoiceFilter.ActiveModuleFilters.Where(x => allfilters.Contains(x.Description.ToString()) && !x.IsEmpty);

			if (activeDateAndModeFilters.Any())
			{
				var filterNames = activeDateAndModeFilters.Select(x => x.Description.ToString());

				foreach (ZString jobType in PeriodicInvoiceFilter.SelectedJobTypes)
				{
					var modules = PeriodicInvoiceModuleDecider.GetJobTypeToCategoryMapping().Where(x => x.Value.Contains(jobType));
					if (modules.Any() && activeDateAndModeFilters.Any())
					{
						var moudleName = modules.First().Key;
						bool compatible = ModuleToFilterMapping.ContainsKey(moudleName) && !filterNames.Cast<string>().Except(ModuleToFilterMapping[moudleName]).Any();
						if (compatible)
						{
							result.Add(jobType);
						}
					}
				}

				if (result.Count == 0)
				{
					result.Add("NOJOB");
				}
			}
			else
			{
				result = PeriodicInvoiceFilter.SelectedJobTypes;
			}
			return result;
		}

		Dictionary<PeriodicInvoiceModule, string[]> ModuleToFilterMapping
		{
			get
			{
				if (moduleToFilterMapping == null)
				{
					moduleToFilterMapping = new Dictionary<PeriodicInvoiceModule, string[]>();
					moduleToFilterMapping.Add(PeriodicInvoiceModule.CFS, allfilters.Except(new string[] { PeriodicInvoiceBaseJobFilterBusinessObject.CARRIER, PeriodicInvoiceBaseJobFilterBusinessObject.PRINCIPAL }).ToArray());
					moduleToFilterMapping.Add(PeriodicInvoiceModule.Consol, new string[] {
																							PeriodicInvoiceBaseJobFilterBusinessObject.ETA,
																							PeriodicInvoiceBaseJobFilterBusinessObject.ETD,
																							PeriodicInvoiceBaseJobFilterBusinessObject.ATA,
																							PeriodicInvoiceBaseJobFilterBusinessObject.ATD,
																							PeriodicInvoiceBaseJobFilterBusinessObject.COMPLETION_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.CUSTOMS_CLEARANCE_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.TRANSPORT_MODE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.RECEIVING_AGENT,
																							PeriodicInvoiceBaseJobFilterBusinessObject.SENDING_AGENT
																						});
					var customsFilters = new List<string>() {
																							PeriodicInvoiceBaseJobFilterBusinessObject.ETA,
																							PeriodicInvoiceBaseJobFilterBusinessObject.ETD,
																							PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_DIRECTION,
																							PeriodicInvoiceBaseJobFilterBusinessObject.PICKUP_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.DELIVERY_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.COMPLETION_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.CUSTOMS_CLEARANCE_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.TRANSPORT_MODE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_LEVEL,
																							PeriodicInvoiceBaseJobFilterBusinessObject.Additional_Reference,
																						};
					if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.Canada)
					{
						customsFilters.Insert(0, PeriodicInvoiceBaseJobFilterBusinessObject.ACCOUNTING_DATE);
					}
					moduleToFilterMapping.Add(PeriodicInvoiceModule.Customs, customsFilters.ToArray());
					moduleToFilterMapping.Add(PeriodicInvoiceModule.Forwarding, allfilters.Except(new string[] { PeriodicInvoiceBaseJobFilterBusinessObject.CARRIER, PeriodicInvoiceBaseJobFilterBusinessObject.PRINCIPAL }).ToArray());
					moduleToFilterMapping.Add(PeriodicInvoiceModule.Transport, new string[] {
																								PeriodicInvoiceBaseJobFilterBusinessObject.COMPLETION_DATE,
																								PeriodicInvoiceBaseJobFilterBusinessObject.TRANSPORT_MODE,
																								PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_LEVEL,
																							});
					moduleToFilterMapping.Add(PeriodicInvoiceModule.Agency, new string[] {
																							PeriodicInvoiceBaseJobFilterBusinessObject.ETA,
																							PeriodicInvoiceBaseJobFilterBusinessObject.ETD,
																							PeriodicInvoiceBaseJobFilterBusinessObject.ATA,
																							PeriodicInvoiceBaseJobFilterBusinessObject.ATD,
																							PeriodicInvoiceBaseJobFilterBusinessObject.PICKUP_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.DELIVERY_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.AWB_ISSUE_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.COMPLETION_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.CUSTOMS_CLEARANCE_DATE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.TRANSPORT_MODE,
																							PeriodicInvoiceBaseJobFilterBusinessObject.CARRIER,
																							PeriodicInvoiceBaseJobFilterBusinessObject.PRINCIPAL,
																							PeriodicInvoiceBaseJobFilterBusinessObject.VOYAGEVESSEL,
																							PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_LEVEL
																						});
					moduleToFilterMapping.Add(PeriodicInvoiceModule.Warehouse, new string[] { PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_LEVEL });
					moduleToFilterMapping.Add(PeriodicInvoiceModule.Consignment, new string[] { PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_LEVEL });
				}
				return moduleToFilterMapping;
			}
		}
		Dictionary<PeriodicInvoiceModule, string[]> moduleToFilterMapping;

		#endregion

		#region Implementation

		protected override SchemaColumn JobHeaderQueryLinkColumn
		{
			get
			{
				return ViewGenericJobSchema.PK;
			}
		}

		protected override Type JobHeaderQueryBizObjType
		{
			get
			{
				return typeof(GenericJob.GenericJob);
			}
		}

		#endregion

#if DEBUG
		protected
#endif
		bool excludeFilter;
		protected PeriodicInvoiceBaseJobFilterBusinessObject PeriodicInvoiceFilter;
		readonly PeriodicInvoiceModuleDecider moduleDecider;
		readonly string[] allfilters;
	}
}

#region Test
#if DEBUG

namespace Enterprise.Accounting.Business.Testing
{
	public class DummyJobFilterProviderForPeriodicInvoice : JobFilterProviderForPeriodicInvoice
	{
		public DummyJobFilterProviderForPeriodicInvoice(PeriodicInvoiceBaseJobFilterBusinessObject filterBizO, bool excludeFilter)
			: base(filterBizO)
		{
			base.excludeFilter = excludeFilter;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new DummyJobFilterProviderForPeriodicInvoice(PeriodicInvoiceFilter, excludeFilter);
	}
}
#endif

#endregion
