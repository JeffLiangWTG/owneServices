using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIProcessTaskTypeDecider : ProcessTaskTypeDecider
	{
		protected override Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			return GetTypeForLoadCore(parentTablePrefix, parentID, factory);
		}

		Type GetTypeForLoadCore(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			Type result = null;

			switch (parentTablePrefix)
			{
				case IncidentMainSchema.Constants.Prefix:
					result = GetIncidentMainProcessTaskType(factory, parentID);
					break;

				case IncidentManagementGroupSchema.Constants.Prefix:
					result = typeof(IncidentManagementGroupProcessTask);
					break;

				case IncidentTriageSchema.Constants.Prefix:
					result = typeof(IncidentTriageProcessTask);
					break;
			}

			if (result == null)
			{
				result = base.GetTypeForLoad(parentTablePrefix, parentID, factory);
			}

			return result;
		}

		Type GetIncidentMainProcessTaskType(BusinessObjectFactory factory, ZGuid parentID)
		{
			// We're avoiding instantiating the BusinessObjects to prevent OnSaving and OnLoad being called unnecessarily.
			((IBusinessObjectFactoryInternals)factory).RowFactory.ExecuteFetchHintsForTable(IncidentMainSchema.Constants.TableName);

			foreach (IncidentMainTypeInfo typeInfo in IncidentMainTypeInfos)
			{
				if (HasIncidentMainOfType(factory, parentID, typeInfo))
				{
					return typeInfo.ProcessTaskType;
				}
			}

			return null;
		}

		IEnumerable<IncidentMainTypeInfo> IncidentMainTypeInfos
		{
			get
			{
				yield return IncidentMainTypeInfo.New<SupportIncident, SupportIncidentProcessTask>(IncidentConstants.IncidentType.SupportIncident);
				yield return IncidentMainTypeInfo.New<ProfessionalServicesQuote, PSQuoteProcessTask>(IncidentConstants.IncidentType.ProfessionalServicesQuote);
			}
		}

		class IncidentMainTypeInfo
		{
			public static IncidentMainTypeInfo New<T, ProcessTaskT>(string typeCode)
				where T : IncidentMainBase
				where ProcessTaskT : ProcessTask
			{
				return new IncidentMainTypeInfo(typeCode, typeof(ProcessTaskT));
			}

			IncidentMainTypeInfo(string typeCode, Type processTaskType)
			{
				this.TypeCode = typeCode;
				this.ProcessTaskType = processTaskType;
			}

			public readonly string TypeCode;
			public readonly Type ProcessTaskType;
		}

		bool HasIncidentMainOfType(BusinessObjectFactory factory, ZGuid pk, IncidentMainTypeInfo typeInfo)
		{
			ZQuery pkQuery = new ZQuery();
			pkQuery.AddToFilter(IncidentMainSchema.PK, pk);

			ZQuery query = new ZQuery();
			query.AddToFilter(IncidentMainSchema.IM_IncidentType, typeInfo.TypeCode);
			query.AddToFilter(pkQuery, JoinCondition.And);

			return ((IBusinessObjectFactoryInternals)factory).RowFactory.Load(IncidentMainSchema.Constants.TableName, query).Length > 0;
		}

		protected override void AddAdditionalParentFilters(WorkflowDescriptor descriptor, ZDBOnlySubQuery subQuery)
		{
			base.AddAdditionalParentFilters(descriptor, subQuery);

			Type workflowProviderType = descriptor.WorkflowProviderType;
			if (workflowProviderType.IsSubclassOf(typeof(IncidentMainBase)))
			{
				string incidentType;
				if (taskIncidentTypesDictionary.TryGetValue(workflowProviderType, out incidentType))
				{
					subQuery.AddToFilter(IncidentMainSchema.IM_IncidentType, incidentType);
				}
			}
		}

		readonly ImmutableDictionary<Type, string> taskIncidentTypesDictionary = ImmutableDictionary<Type, string>.Empty
			.Add(typeof(SupportIncident), IncidentConstants.IncidentType.SupportIncident)
			.Add(typeof(ProfessionalServicesQuote), IncidentConstants.IncidentType.ProfessionalServicesQuote);
	}
}

