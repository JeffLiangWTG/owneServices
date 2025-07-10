using System;
using System.Collections.Immutable;
using CargoWise.Common;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.TimeEngineScheduler.ServiceTask
{
	class SchedulerActionFactory : ISchedulerActionFactory
	{
		readonly Lazy<IImmutableDictionary<string, SchedulerActionAttribute>> allActions;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
		public SchedulerActionFactory()
		{
			allActions = new Lazy<IImmutableDictionary<string, SchedulerActionAttribute>>(() =>
			{
				var builder = ImmutableDictionary.CreateBuilder<string, SchedulerActionAttribute>();
				AssemblyMetaDataReader.GetAttributes<SchedulerActionAttribute>().ForEach(a =>
				{
					try
					{
						builder.Add(a.Code, a);
					}
					catch (ArgumentException ex) when (ex.Message.StartsWith("An element with the same key but a different value already exists", StringComparison.OrdinalIgnoreCase))
					{
						var existingData = builder[a.Code];
						throw new ArgumentException($"Existing type {existingData.Type.FullName}, this type {a.TypeName}", ex);
					}
				});
				return builder.ToImmutable();
			});
		}

		public ISchedulerAction GetSchedulerAction(string code)
		{
			if (allActions.Value.TryGetValue(code, out var config))
			{
				return (ISchedulerAction)Activator.CreateInstance(Type.GetType($"{config.TypeName}, {config.TypeAssemblyName}", throwOnError: true));
			}
			else
			{
				return null;
			}
		}
	}
}
