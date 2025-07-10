using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class StaticCustomisationLineCache
	{
		public StaticCustomisationLineCache(StaticControlProperty property, PropertyCache cache, IEnumerable<ProcessTask> tasks, IEnumerable<ProcessHeader> headers, bool showJobWorkflow, bool showWorkflow)
		{
			Key = property;
			propertyCache = cache;
			PopulateCache(property, propertyCache, tasks, headers, showJobWorkflow, showWorkflow);
		}

		public StaticControlProperty Key { get; private set; }
		readonly PropertyCache propertyCache;

		#region API

		public TValue GetCachedValue<TValue>(StaticControlProperty key, ICardContent card)
		{
			if (GetWorkflowValueGetter(key) != null)
			{
				return propertyCache.GetCachedValue<TValue>(card.WorkflowIdentifier, key.ToString());
			}
			else
			{
				return propertyCache.GetCachedValue<TValue>(card.TaskIdentifier, key.ToString());
			}
		}

		internal static TValue GetValue<TValue>(StaticControlProperty key, ProcessTask task, ProcessHeader workflow, bool showJobWorkflow, bool showWorkflow)
		{
			var workflowValueGetter = GetWorkflowValueGetter(key);
			if (workflowValueGetter != null)
			{
				return (TValue)workflowValueGetter(workflow);
			}
			else
			{
				return (TValue)GetTaskValueGetter(key, showJobWorkflow, showWorkflow)(task);
			}
		}

		static readonly ImmutableHashSet<string> staticControlTypeCodes = ImmutableHashSet.Create(new StaticControlTypeList().Cast<CodeDescriptionPair>().Select(c => c.Code).ToArray());

		internal static IEnumerable<StaticControlProperty> GetRequiredProperties(StaticControlCustomisation line)
		{
			if (!line.IsReadOnlyControlType)
			{
				if (staticControlTypeCodes.Contains(line.ControlType))
				{
					var message = string.Format(CultureInfo.InvariantCulture, $"The control type [{line.ControlType}] is not readonly but it is being used in a readonly context. Layout: {line.Parent}"); // This is a developer error message.
					Globals.Message.ShowDeveloperErrorOnce("4a862574-f881-456e-8d47-10a21a7bad47", message, message);
				}

				return Enumerable.Empty<StaticControlProperty>();
			}
			else
			{
				switch (line.ControlType)
				{
					case StaticControlTypeList.Codes.TaskStatusIndicator:
						return new[] { StaticControlProperty.ConstraintStatus, StaticControlProperty.RequiresResourceWithCapability, StaticControlProperty.TaskStatus };

					case StaticControlTypeList.Codes.DateAcceptabilityPicture:
						return new[] { StaticControlProperty.ApplicableDateAcceptability };

					case StaticControlTypeList.Codes.AttachedTagsIndicator:
					case StaticControlTypeList.Codes.Label:
						return Enumerable.Empty<StaticControlProperty>();

					default:
						throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Unrecognised control type [{0}].", line.ControlType));
				}
			}
		}

		#endregion

		#region Value Getters

		static Func<ProcessTask, object> GetTaskValueGetter(StaticControlProperty key, bool showJobWorkflow, bool showWorkflow)
		{
			switch (key)
			{
				case StaticControlProperty.ConstraintStatus:
					return (task) => ConstrainedModeHelper.GetConstraintStatus(task);

				case StaticControlProperty.RequiresResourceWithCapability:
					return (task) => task.RequiresResourceWithCapability;

				case StaticControlProperty.TaskStatus:
					return (task) => GetConsolidatedStatus(task, showJobWorkflow, showWorkflow);

				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognised source [{0}]", key));
			}
		}

		static ZString GetConsolidatedStatus(ProcessTask task, bool showJobWorkflow, bool showWorkflow)
		{
			var result = ZString.Empty;
			if (showJobWorkflow || showWorkflow)
			{
				var workflow = showJobWorkflow ? task.GetProcessHeaderForCardType(showJobWorkflow) : task.GetProcessHeader();
				if (workflow.IsWorking)
				{
					result = ProcessTaskStatusCodeList.Codes.Working;
				}
				else if (workflow.IsSuspended)
				{
					result = ProcessTaskStatusCodeList.Codes.Suspended;
				}
			}
			else
			{
				result = task.P9_Status;
			}
			return result;
		}

		static Func<ProcessHeader, object> GetWorkflowValueGetter(StaticControlProperty key)
		{
			if (key == StaticControlProperty.ApplicableDateAcceptability)
			{
				return (workflow) => workflow.ApplicableDateAcceptability;
			}
			else
			{
				return null;
			}
		}

		static void PopulateCache(StaticControlProperty key, PropertyCache cache, IEnumerable<ProcessTask> tasks, IEnumerable<ProcessHeader> headers, bool showJobWorkflow, bool showWorkflow)
		{
			var workflowValueGetter = GetWorkflowValueGetter(key);
			if (workflowValueGetter != null)
			{
				headers.ForEach(w => cache.GetCachedValue(w.PK, key.ToString(), () => workflowValueGetter(w)));
			}
			else
			{
				var taskValueGetter = GetTaskValueGetter(key, showJobWorkflow, showWorkflow);
				tasks.ForEach(t => cache.GetCachedValue(t.PK, key.ToString(), () => taskValueGetter(t)));
			}
		}

		#endregion
	}
}
