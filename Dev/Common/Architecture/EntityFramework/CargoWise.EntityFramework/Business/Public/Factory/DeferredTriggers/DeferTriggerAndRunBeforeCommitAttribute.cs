using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Apply this attribute to a business object to specify that it has triggers that require deferral
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class DeferTriggerAndRunBeforeCommitAttribute : Attribute
	{
		public DeferTriggerAndRunBeforeCommitAttribute(string triggerName, string storedProcName, string columnWithRowPkToRunStoredProcOn, Type deferTriggerConditionStrategyType)
		{
			TriggerName = triggerName;
			StoredProcName = storedProcName;
			ColumnWithRowPkToRunStoredProcOn = columnWithRowPkToRunStoredProcOn;
			DeferTriggerConditionStrategyType = deferTriggerConditionStrategyType;
			ValueToRunStoredProcWith = ValueVersion.Current;
		}

		public DeferTriggerAndRunBeforeCommitAttribute(string triggerName, Type deferTriggerConditionStrategyType)
		{
			TriggerName = triggerName;
			DeferTriggerConditionStrategyType = deferTriggerConditionStrategyType;
			IsSuspendTriggerOnly = true;
		}

		public string TriggerName { get; }
		public string StoredProcName { get; }
		public string ColumnWithRowPkToRunStoredProcOn { get; }
		public string ExtraParamsForStoredProc { get; set; }
		public Type DeferTriggerConditionStrategyType { get; }
		public bool IsSuspendTriggerOnly { get; }

		/// <summary>
		/// This determines whether the current value in memory of 'ColumnWithRowPkToRunStoredProcOn' (that one that's going to be posted), or the original value (the one in the DB),
		/// or both will be passed into the Stored Procedure. Note that for Strategies that run on delete this Parameter is ignored, it will always use the Original Value.
		/// </summary>
		public ValueVersion ValueToRunStoredProcWith { get; set; }

		internal static ImmutableArray<DeferTriggerAndRunBeforeCommitAttribute> GetAttributes(Type type)
		{
			if (!Types.TryGetValue(type, out var attributes))
			{
				var customAttribs = (DeferTriggerAndRunBeforeCommitAttribute[])type.GetCustomAttributes(typeof(DeferTriggerAndRunBeforeCommitAttribute), inherit: true);
				Types[type] = attributes = ImmutableArray.Create(customAttribs);
			}

			return attributes;
		}

		static Dictionary<Type, ImmutableArray<DeferTriggerAndRunBeforeCommitAttribute>> Types
			=> types ??= new Dictionary<Type, ImmutableArray<DeferTriggerAndRunBeforeCommitAttribute>>();

		[ThreadStatic]
		static Dictionary<Type, ImmutableArray<DeferTriggerAndRunBeforeCommitAttribute>> types;
	}
}
