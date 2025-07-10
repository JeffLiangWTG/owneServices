using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management.Management
{
	public class DepartmentLocator : IDepartmentLocator
	{
		readonly ISimpleLogger logger;

		readonly BusinessObjectFactory factory;

		readonly ZBool requiresCodesMappedToTarget;

		public DepartmentLocator(BusinessObjectFactory factory, ZBool requiresCodesMappedToTarget, ISimpleLogger logger = null)
		{
			this.logger = logger ?? new Integration.DummyLogger();
			this.factory = factory;
			this.requiresCodesMappedToTarget = requiresCodesMappedToTarget;
		}

		public (bool active, IGlbDepartment department) TryGetActiveDepartment(IEDIMessage message, IDataContextDataObject contextDataObject)
		{
			if (TryGetDepartmentFromDataContext(contextDataObject, out var department))
			{
				if (ShouldReturnDepartment(department, out var active))
				{
					return (active, department);
				}

				LogFallbackWarning(department, DepartmentFallbackOption.Default, factory.Load<IGlbDepartment>(message.EM_GE)?.GE_Code ?? string.Empty);
			}

			if (TryGetDepartmentFromEDIMEssage(message, out var departmentFromMessage))
			{
				if (ShouldReturnDepartment(departmentFromMessage, out var active))
				{
					return (active, departmentFromMessage);
				}

				LogFallbackWarning(departmentFromMessage, DepartmentFallbackOption.Environment, factory.Load<IGlbDepartment>(Env.CurrentDepartmentPK)?.GE_Code ?? string.Empty);
			}

			return TryGetActiveEnvironmentDepartment();
		}

		bool TryGetDepartmentFromDataContext(IDataContextDataObject contextDataObject, out IGlbDepartment department)
		{
			if (contextDataObject != null && (contextDataObject.CodesMappedToTarget || !requiresCodesMappedToTarget) && !contextDataObject.EventDepartmentCode.IsEmpty)
			{
				department = factory.LoadFromNaturalKey<IGlbDepartment>(GlbDepartmentSchema.GE_Code, contextDataObject.EventDepartmentCode);
				return department != null;
			}

			department = null;
			return false;
		}

		bool ShouldReturnDepartment(IGlbDepartment department, out bool active)
		{
			active = false;
			if (department is ICancellable cancellable)
			{
				active = !cancellable.IsCancelled;
				if (active || eAdaptorRegistry.Instance.UniversalXMLInactiveDepartmentFailsMessage.Value)
				{
					return true;
				}
			}

			return false;
		}

		bool TryGetDepartmentFromEDIMEssage(IEDIMessage message, out IGlbDepartment department)
		{
			department = factory.Load<IGlbDepartment>(message.EM_GE);
			return department != null;
		}

		(bool active, IGlbDepartment department) TryGetActiveEnvironmentDepartment()
		{
			var envDepartment = factory.Load<IGlbDepartment>(Env.CurrentDepartmentPK);
			if (envDepartment is ICancellable cancellableDepartment)
			{
				return (!cancellableDepartment.IsCancelled, envDepartment);
			}
			return (envDepartment != null, envDepartment);
		}

		void LogFallbackWarning(IGlbDepartment specifiedDepartment, DepartmentFallbackOption fallbackOption, string fallbackDepartmentCode)
		{
			var warningMessage = ResString.GetMultilingualString("C6C92D30-5623-446F-B3D1-2DF14EA204B8", "Department {0} is not active. Falling back to evaluate {1} Department {2}", specifiedDepartment.GE_Code, fallbackOption.ToString().ToLower(), fallbackDepartmentCode);
			if (logger is IXmlImportLogger xmlLogger)
			{
				xmlLogger.LogBoth(LogType.Warning, warningMessage);
			}
			else
			{
				logger.Log(LogType.Warning, warningMessage);
			}
		}

		enum DepartmentFallbackOption
		{
			Default,
			Environment
		}
	}
}
