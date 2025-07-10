using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using WTG.Glow.Data.Annotations;

namespace GlowIndexQueryService.Business
{
	public static class GlowModuleToCW1ModuleConverter
	{
		static readonly ImmutableDictionary<string, string> entityToTablePrefixMapping = CreateMapping();

		static ImmutableDictionary<string, string> CreateMapping()
		{
			var types = LoadTypesFromAssembly();
			var entityTypeToTableCodeDic = new Dictionary<string, string>();

			foreach (var type in types)
			{
				var tableCode = TableCodeAttribute.GetTableCode(type);
				if (!string.IsNullOrEmpty(tableCode))
				{
					entityTypeToTableCodeDic.Add(type.Name, tableCode);
				}
				else
				{
					if (type.GetCustomAttribute<BaseEntityInterfaceAttribute>()?.InterfaceType is Type interfaceType)
					{
						tableCode = TableCodeAttribute.GetTableCode(interfaceType);
						if (!string.IsNullOrEmpty(tableCode))
						{
							entityTypeToTableCodeDic.Add(type.Name, tableCode);
						}
					}
				}
			}

#if DEBUG
			entityTypeToTableCodeDic.Add("IDummyBusinessObject", "Z0");
#endif
			return entityTypeToTableCodeDic.ToImmutableDictionary();
		}

		static Type[] LoadTypesFromAssembly()
			=> (GetAssembliesFromCurrentDomain() ?? GetAsmFromCurrentBinDir())?.GetExportedTypes() ?? Array.Empty<Type>();

#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
		static Assembly GetAssembliesFromCurrentDomain()
			=> AppDomain.CurrentDomain
				.GetAssemblies()
				.FirstOrDefault(assembly => assembly.FullName.Contains(CWGlowModelInterfaceAssemblyName));
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Need to attempt to load assembly")]
		static Assembly GetAsmFromCurrentBinDir()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if NET
			if (path.EndsWith(CommonAssemblyInfo.CWNetCoreSubfolder))
			{
				path = Directory.GetParent(path).FullName;
			}
#endif
			var assemblyName = Path.Combine(path, $"{CWGlowModelInterfaceAssemblyName}.dll"); // its a file path

			if (string.IsNullOrEmpty(assemblyName))
			{
				return null;
			}

			Assembly assembly;
			try
			{
				assembly = Assembly.LoadFile(assemblyName);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return null;
			}

			return assembly;
		}

		public static string ConvertEntityTypeToTableCode(string entityName)
		{
			if (string.IsNullOrEmpty(entityName))
			{
				return null;
			}
			var entityNameWithoutOf = entityName.Split([".Of.", "[["], StringSplitOptions.None).FirstOrDefault();
			return entityToTablePrefixMapping.TryGetValue(entityNameWithoutOf, out var code) ? code : null;
		}

		public static BusinessObject ConvertEntityToBizo(string strPK, string entityName, BusinessObjectFactory factory = null)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (string.IsNullOrEmpty(entityName) || !Guid.TryParse(strPK, out var pk))
				{
					return null;
				}

				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				var tableCode = ConvertEntityTypeToTableCode(entityName);
				return !string.IsNullOrEmpty(tableCode) ?
					factory.Load(tableCode, pk) :
					null;
			}
		}

		const string CWGlowModelInterfaceAssemblyName = "CargoWise.Glow.Model.Interfaces"; // not translatable

		public static ModuleIdentifier[] GetAllModuleIDs() => EntityTypeModuleIdentifierMap.Values.SelectMany(v => v).ToArray();

		static readonly ImmutableHashSet<ModuleId> verifiedModuleList =
			new ModuleId[]
		{
			ModuleId.GlbBranch,
			ModuleId.Project,
			ModuleId.WorkItem,
			ModuleId.GlbCapability,
			ModuleId.ReportStatistics,
			ModuleId.BMControlCustomisation,
			ModuleId.AcceptabilityBand,
			ModuleId.GlbDepartment,
			ModuleId.SalesEnquiry,
			ModuleId.ReportManagement,
			ModuleId.ProcessHeader,
			ModuleId.BMSystems,
			ModuleId.ProcessTasks,
			ModuleId.GlbCompany,
			ModuleId.GlbStaff,
			ModuleId.ScheduledReports,
			ModuleId.UniversalCopySchedule,
		}.ToImmutableHashSet();

		static public bool CheckIsVerifiedModule(ModuleIdentifier moduleIdentifier)
			=> moduleIdentifier?.ID is ModuleId moduleId && verifiedModuleList.Contains(moduleId);

		public static string ConvertModuleIdentifierToEntityType(ModuleIdentifier moduleIdentifier)
		{
			if (moduleIdentifier == null)
			{
				return null;
			}

			foreach (var map in EntityTypeModuleIdentifierMap)
			{
				if (map.Value.Contains(moduleIdentifier))
				{
					return ConvertTypeName(map.Key);
				}
			}

			return null;
		}

		static string ConvertTypeName(string entityType)
		{
			if (!string.IsNullOrWhiteSpace(entityType) && entityType.Contains(".Of."))
			{
				return entityType.Replace(".Of.", "[[") + "]]";
			}
			return entityType;
		}

		internal static string NormalizeTypeName(string entityType)
		{
			if (!string.IsNullOrWhiteSpace(entityType))
			{
				return entityType.Replace("[[", ".Of.").Replace("]]", string.Empty);
			}

			return entityType;
		}

		[ThreadStatic]
		static Dictionary<string, List<ModuleIdentifier>> entityTypeModuleIdentifierMap;

		static Dictionary<string, List<ModuleIdentifier>> EntityTypeModuleIdentifierMap => GetEntityTypeModuleIdentifierMap();

		static Dictionary<string, List<ModuleIdentifier>> GetEntityTypeModuleIdentifierMap()
		{
			if (entityTypeModuleIdentifierMap != null)
			{
				return entityTypeModuleIdentifierMap;
			}

			var moduleList = ObjectFactory.Get<IModuleFactory>();
			if (EntityTypes == null || EntityTypes.Count == 0)
			{
				entityTypes = defaultEntityTypesIfGlowServiceNotAvailable;
			}

			entityTypeModuleIdentifierMap = new Dictionary<string, List<ModuleIdentifier>>();

			foreach (var entityType in EntityTypes)
			{
				if (MultiLayerEntityTypeToModuleIdentifier.ContainsKey(entityType))
				{
					entityTypeModuleIdentifierMap[entityType] = new List<ModuleIdentifier>(MultiLayerEntityTypeToModuleIdentifier[entityType]);
					continue;
				}

				var prefix = GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode(entityType);
				if (string.IsNullOrEmpty(prefix))
				{
					continue;
				}
				var moduleIdentifiers = moduleList.GetRegisteredIdentifiersByColumnNamePrefix(prefix)?.ToList();
				if (moduleIdentifiers?.Count > 0)
				{
					entityTypeModuleIdentifierMap[entityType] = new List<ModuleIdentifier>(moduleIdentifiers);
				}
			}

			return entityTypeModuleIdentifierMap;
		}

		static readonly Dictionary<string, List<ModuleIdentifier>> MultiLayerEntityTypeToModuleIdentifier = new Dictionary<string, List<ModuleIdentifier>> { { "IStmScheduleTask.Of.IStmMenuItem", [ModuleIDs.ReportManagement, ModuleIDs.ScheduledReports] } };

		static readonly HashSet<string> defaultEntityTypesIfGlowServiceNotAvailable = new HashSet<string>() {
			"IWhsCycleCountLocationVariance", "INettingForeignCurrencyOffer", "IWhsItemCycleCountLocation", "IWhsVolCam", "IWhsPickLine", "IWhsItemDispatchLoadList",
			"INettingSystem", "IWhsReceive", "IHVLVConsignment", "IWhsWorkOrder", "INettingForeignCurrencyRequest", "IGlbStaff", "IWhsPickShortLine", "IJobShipment",
			"IRefMaterial", "IDtbConsignment", "IJobOrderHeader", "ICYContainerLoadList", "ICFSContainerLoadList", "IOrgHeader", "IWhsItemDispatchConsignment", "ICYDDelivery",
			"IHRJobApplicationDocument", "ICYDPickupHeader", "IDtbConsignmentRunSheet", "INettingFXDeal", "INettingReceivableTransaction", "IWhsItemCycleCountLocationVariance",
			"IJobSupplierBooking", "IWhsItemReceiveASN", "ICarrierShipmentHeader", "IWorkProject", "IOrgSupplierPart", "IWhsAdjustment", "IOrgCustomerAddress", "IPkgPackageItemDivot",
			"IWhsOrder", "ICYDPickup", "IWorkRequest", "ICarrierShipmentRoRo", "IWhsVASOrderLine", "ICYDTransportationUnit", "INettingPayableTransaction", "INettingSystemPeriod",
			"IBMNCNShape", "IPackageJob.Of.IWhsDocket", "ICYDReceiveAdvice", "INettingOrganisation", "IWhsDocketLine", "ICusClassPartPivot", "IIncidentRequest",
			"INettingMatchPivot", "IWhsItemTransferHeader", "IWhsItemPackageState", "ICarrierShipmentContainer", "IJobConsol", "IBMBoard", "IWhsItemReceiveTransportationUnit",
			"ICYDDeliveryHeader", "IJobContainer", "ICYDYardUnitState", "IJobDeclaration", "IWhsItemDispatchTransportationUnit", "IWhsTransfer", "IJobOrderLine",
			"IWhsCycleCountLocation", "IWorkItem", "ICusContainer", "ICYDMovementHeader", "ICYDReleaseAdvice", "IJobSailing", "IHelpErrorLog", "IWhsItemReceiveConsignment",
			"IWhsDynamicWorkOrder", "IWhsVASOrder", "IGlbCompany", "IGlbDepartment", "IGlbBranch", "IGlbCapability", "IProcessHeader", "IBMControlCustomisation", "IStmReportRun",
			"IBMSystem", "IBMComponentAcceptabilityBand", "IOrgColdCallRegister", "IStmScheduleTask.Of.IStmMenuItem","IProcessTask", "IStmUniversalCopy"
		};

		static ISet<string> EntityTypes => entityTypes ?? ObjectFactory.Get<IGlowIndexQueryEngine>().GetGlowEntityTypes();

#if DEBUG
		public static void ClearEntityTypesCache()
		{
			entityTypeModuleIdentifierMap = null;
			entityTypes = null;
		}
#endif

		[ThreadStatic]
		static ISet<string> entityTypes;
	}
}
