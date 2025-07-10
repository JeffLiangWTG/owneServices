using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// Loads and manages all ArchiveSystem objects provided throughout the entire system
	/// </summary>
	public sealed class ArchiveManager : IArchiveManager
	{
		public ArchiveManager(IArchiveSystemDescriptorLoader loader)
		{
			var loadResult = loader.Load();
			ArchiveSystemDescriptors = loadResult.ToList();

			archiveDescriptorLookup = new Dictionary<string, IArchiveSystemDescriptor>();
			foreach (var descriptor in ArchiveSystemDescriptors)
			{
				archiveDescriptorLookup.Add(descriptor.Code, descriptor);
			}
		}

		public List<IArchiveSystemDescriptor> ArchiveSystemDescriptors { get; private set; }

		/// <summary>
		/// Runs archiving for the system that matches the given systemCode
		/// </summary>
		/// <param name="systemCode">3 letter code for the system</param>
		/// <param name="config">User configuration parameters</param>
		/// <param name="logger">Object to log messages</param>
		/// <param name="schedule">schedule object to be updated</param>
		public void Run(string systemCode, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token)
		{
			_ = Argument.NotNullOrEmpty(systemCode, "systemCode");
			_ = Argument.NotNull(config, "config");
			_ = Argument.NotNull(logger, "logger");
			_ = Argument.NotNull(schedule, "schedule");

			if (!archiveDescriptorLookup.ContainsKey(systemCode))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "SystemCode '{0}' not found in registered ArchiveSystemDescriptors list", systemCode));
			}

			var system = new ArchiveSystem(archiveDescriptorLookup[systemCode]);
			Run(system, config, logger, schedule, token);
		}

		void Run(IArchiveSystem system, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token)
		{
			Helpers.ToLog((NoResString)"Registry Settings:", system.Descriptor.GetRegistryLogs(), logger, system);
			Helpers.ToLog((NoResString)"Configuration Parameters:", config.GetConfigurationLogs(system), logger, system);

			try
			{
				var archiveStageList = system.GetArchiveStages(config).ToList();

				if (archiveStageList.Count > 0)
				{
					foreach (var stage in archiveStageList)
					{
						if (schedule.GetWatermark(stage.Name) != null)
						{
							logger.LogInfo(system.Descriptor.Code, $"Watermark date is '{schedule.GetWatermark(stage.Name).WatermarkDate}'");
						}
						logger.LogInfo(system.Descriptor.Code, $"Executing: {stage.Name}");
						var completedStageSuccessfully = stage.ExecuteStage(system, stage, config, logger, schedule, token);

						if (!completedStageSuccessfully)
						{
							break;
						}
					}

					logger.LogInfo(system.Descriptor.Code, $"Completed {system.Descriptor.PresentTenseVerb.GetUnresolvedString().ToLower()} records");
				}
				else
				{
					logger.LogInfo(system.Descriptor.Code, $"There are no archive stages to execute with the current configuration.");
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var message = $"Error encountered during {system.Descriptor.PresentTenseVerb.GetUnresolvedString().ToLower()}. Run aborted.";
				logger.LogAndReportError("RunException", system.Descriptor.Code, message, e);
			}
		}

		readonly Dictionary<string, IArchiveSystemDescriptor> archiveDescriptorLookup;

		public static readonly string SystemSpecificSuffix = $"{ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode}{ObjectFactory.Get<IProductRegistration>().Key.ServerCode}";
	}
}
