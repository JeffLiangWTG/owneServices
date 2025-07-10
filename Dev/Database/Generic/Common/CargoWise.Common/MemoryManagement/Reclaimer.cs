using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CargoWise.Common.MemoryManagement.Internal
{
	/// <summary>
	/// This is the actual internal memory management class
	/// Typically is a Singleton object stored within the MemoryManager class
	/// Multiple instances may occur in tests
	/// </summary>
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	class Reclaimer
	{
		public Reclaimable Register(string description, FlushCallback instanceType, StaticReclaimWrapper cleanupAction)
		{
			var reclaimable = new Reclaimable(null, (target, action) => cleanupAction(action))
			{
				Description = description
			};
			Register(reclaimable, instanceType);
			return reclaimable;
		}

		public Reclaimable Register<T>(string description, T target, FlushCallback instanceType, ReclaimWrapper<T> cleanupAction)
		{
			Argument.NotNull(cleanupAction, nameof(cleanupAction)); // Suggested By ReviewBot 
			if (cleanupAction.Target == null || IsCompiledGeneratedObject(cleanupAction.Target))
			{
				var result = new Reclaimable<T>(target, cleanupAction)
				{
					Description = description
				};
				Register(result, instanceType);
				return result;
			}
			if (cleanupAction.Method.DeclaringType != null)
			{
				string delegateKey = cleanupAction.Method.DeclaringType.Namespace + "." + cleanupAction.Method.DeclaringType.Name + "." + cleanupAction.Method.Name;
				ErrorReporter.ReportOnce("NonStaticDelegate:" + delegateKey,
					"Cleanup actions must be defined as static methods so that instances are garbage collectable.\r\n" +
						"Change " + delegateKey + " to be a static method.");
			}
			else
			{
				ErrorReporter.ReportOnce("NonStaticDelegate is null");
			}
			return null;
		}

		public IEnumerable<IReclaimable> Examine()
		{
			lock (reclaimables)
			{
				RemoveDeadReferences();
				return reclaimables.ConvertAll(holder => (IReclaimable)holder.Reclaimable);
			}
		}

		public FlushResult Flush(FlushAction action, long targetMemorySizeInBytes)
		{
			if (BelowThreshold(targetMemorySizeInBytes))
			{
				return FlushResult.NotRequired;
			}

			lock (reclaimables)
			{
				RemoveDeadReferences();

				Dictionary<Reclaimable, bool> exhaustedItems = new Dictionary<Reclaimable, bool>();

				DateTime lastCollect = DateTime.Now; // No reference to CargoWise.Types from CargoWise.Common
				FlushResult reclaimResult;
				// Establish a loop that pumps each delegate until desired memory is reached or all delegates are exhaused
				do
				{
					reclaimResult = FlushResult.Exhausted;
					foreach (ReclaimableHolder reclaimableHolder in reclaimables.FindAll(reclaimable => reclaimable.IsEligibleForCurrentThread))
					{
						var reclaimable = reclaimableHolder.Reclaimable;
						if (!exhaustedItems.ContainsKey(reclaimable))
						{
							if (reclaimable.CleanUp(action) == FlushResult.Partial)
							{
								reclaimResult = FlushResult.Partial;
							}
							else
							{
								exhaustedItems[reclaimable] = true;
							}
						}
					}
					if (DateTime.Now - lastCollect > TimeSpan.FromSeconds(5)) // No reference to CargoWise.Types from CargoWise.Common
					{
						lastCollect = DateTime.Now; // No reference to CargoWise.Types from CargoWise.Common
						if (BelowThreshold(targetMemorySizeInBytes))
						{
							// Memory threshold achieved
							return FlushResult.Partial;
						}
					}
				} while (reclaimResult != FlushResult.Exhausted);
			}

			// Memory threshold was not achieved
			return FlushResult.Exhausted;
		}

		#region Implementation

		void Register(Reclaimable inner, FlushCallback instanceType)
		{
			Argument.NotNull(inner, nameof(inner));
			lock (reclaimables)
			{
				reclaimables.Add(new ReclaimableHolder(inner, instanceType));
			}
		}

		static bool BelowThreshold(long targetMemorySizeInBytes)
		{
			return GC.GetTotalMemory(false) < targetMemorySizeInBytes;
		}

		static bool IsCompiledGeneratedObject(object actionTarget)
		{
			Argument.NotNull(actionTarget, nameof(actionTarget));
			return Attribute.GetCustomAttribute(actionTarget.GetType(), typeof(CompilerGeneratedAttribute)) != null;
		}

		void RemoveDeadReferences()
		{
			reclaimables.RemoveAll(reclaimableHolder => !reclaimableHolder.IsAlive);
		}

		readonly List<ReclaimableHolder> reclaimables = new List<ReclaimableHolder>(100);
		#endregion
	}
}