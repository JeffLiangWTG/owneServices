using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	internal static class PerformanceStatisticCollectorTokenExtensions
	{
		public static IDisposable Exclude(this PerformanceStatisticCollectorToken token)
		{
			return token != null ? token.Suspend() : DisposableAction.NoAction;
		}
	}

	public interface IPerformanceStatisticCollectorToken
	{
		string Name { get; }
		string SubName { get; }
		int ActionCount { get; }
		TimeSpan ElapsedExcludingChildren { get; }
		TimeSpan ElapsedIncludingChildren { get; }
		ZDateTime TimeStartedUtc { get; }
		ZDateTime TimeEndedUtc { get; }
		IEnumerable<PerformanceStatisticCollectorToken> Children { get; }
		string CompanyCode { get; }
		string BranchCode { get; }
		string UserInitials { get; }
		IDisposable Suspend();
		bool CanFold(PerformanceStatisticCollectorToken anotherElement);
		void Incorporate(PerformanceStatisticCollectorToken e2);
	}

	public sealed class PerformanceStatisticCollectorToken : IPerformanceStatisticCollectorToken
	{
		public string Name { get; private set; }
		public string SubName { get; private set; }

		public int ActionCount { get; private set; }
		public TimeSpan ElapsedExcludingChildren { get; private set; }
		public TimeSpan ElapsedIncludingChildren { get; private set; }
		public ZDateTime TimeStartedUtc { get; private set; }
		public ZDateTime TimeEndedUtc { get; private set; }

		public string CompanyCode { get; private set; }
		public string BranchCode { get; private set; }
		public string UserInitials { get; private set; }

		#region Test Constructor, highly convenient
#if DEBUG

		internal PerformanceStatisticCollectorToken(string name, string subname, params PerformanceStatisticCollectorToken[] children)
			: this(name, subname, actionCount: 1, children: children.ToList())
		{
		}

		internal PerformanceStatisticCollectorToken(string action, int actionCount, params PerformanceStatisticCollectorToken[] children)
			: this(action, "This is a sexy test", actionCount, children.ToList())
		{
		}
#endif
		#endregion

		internal PerformanceStatisticCollectorToken(string name, string subname)
			: this(name, subname, actionCount: 1, children: new List<PerformanceStatisticCollectorToken>())
		{
		}

		PerformanceStatisticCollectorToken(string name, string subname, int actionCount, List<PerformanceStatisticCollectorToken> children)
		{
			Name = name;
			SubName = subname;
			ActionCount = actionCount;
			this.children = children;

#if DEBUG
			this.stopwatch = ObjectFactory.Get<IStopwatch>();
#else
			this.stopwatch = new UberWatch(); // Evil but needed for performance
#endif
			stopwatch.Start();
			TimeStartedUtc = UtcNow();

			var env = EnvProxy.Instance;
			using (env.SuspendBranchAccessError())
			{
				CompanyCode = env.CurrentCompany?.Code;
				BranchCode = env.CurrentBranch?.Code;
				UserInitials = env.CurrentUser?.Initials;
			}
		}

		readonly IStopwatch stopwatch;

		#region Suspend Management

		public IDisposable Suspend()
		{
			suspendCount++;
			AdjustStopwatch();
			return new DisposableAction(() =>
			{
				suspendCount--;
				AdjustStopwatch();
			}
			);
		}

		int suspendCount;
		internal bool IsSuspended
		{
			get { return suspendCount > 0; }
		}

		#endregion

		#region Child Token Active management

		bool isChildTokenActive;
		internal bool IsChildTokenActive
		{
			get
			{
				return isChildTokenActive;
			}
			set
			{
				isChildTokenActive = value;
				AdjustStopwatch();
			}
		}

		#endregion

		void AdjustStopwatch()
		{
			if (IsSuspended || IsChildTokenActive)
			{
				if (stopwatch.IsRunning)
				{
					stopwatch.Stop();
					ElapsedExcludingChildren += TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
				}
			}
			else
			{
				if (!stopwatch.IsRunning)
				{
					stopwatch.Restart();
				}
			}
		}

		internal void Stop()
		{
			if (IsSuspended)
			{
				throw new Exception("Tokens may not be stopped whilst suspended - missing Dispose?");
			}
			if (IsChildTokenActive)
			{
				throw new Exception("Tokens may not be stopped whilst a child token is active - missing Dispose?");
			}

			stopwatch.Stop();
			ElapsedExcludingChildren += TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds); // UberWatch has issue losing precisions on ms
			ElapsedIncludingChildren = ElapsedExcludingChildren + TimeSpan.FromMilliseconds(children.Sum(x => x.ElapsedIncludingChildren.TotalMilliseconds));
			var currentTimeUtc = UtcNow();
			if (currentTimeUtc < TimeStartedUtc)
			{
				currentTimeUtc = TimeStartedUtc;
			}
			TimeEndedUtc = currentTimeUtc;
		}

		ZDateTime UtcNow()
		{
#if DEBUG
			return ZDateTime.UtcNow;
#else  // Evil but needed for performance (ZDateTime.UtcNow uses server time, but local is ok for our purposes
			return DateTime.UtcNow;
#endif
		}

		public IEnumerable<PerformanceStatisticCollectorToken> Children
		{
			get { return children; }
		}
		readonly List<PerformanceStatisticCollectorToken> children;

		public PerformanceStatisticCollectorToken PreviousSibling(PerformanceStatisticCollectorToken token)
		{
			var index = children.IndexOf(token);
			return index > 0 ? children[index - 1] : null;
		}

		public bool CanFold(PerformanceStatisticCollectorToken anotherElement)
		{
			return CanFold(this, anotherElement);
		}

		static bool CanFold(PerformanceStatisticCollectorToken element1, PerformanceStatisticCollectorToken element2)
		{
			return element1 != null
					&& element2 != null
					&& element1.Name == element2.Name
					&& element1.SubName == element2.SubName
					&& DatesAreInSameMinute(element1.TimeStartedUtc, element2.TimeStartedUtc)
					&& ElementsHaveEquivalentStructure(element1.children, element2.children);
		}

		static bool ElementsHaveEquivalentStructure(ICollection<PerformanceStatisticCollectorToken> element1, ICollection<PerformanceStatisticCollectorToken> element2)
		{
			return
				element1.Count == element2.Count
				&& element1.Zip(element2, CanFold).All(x => x);
		}

		public static bool DatesAreInSameMinute(ZDateTime d1, ZDateTime d2)
		{
			return Math.Abs((d1 - d2).TotalMinutes) < 1;
		}

		public static bool All(IEnumerable<bool> xs)
		{
			return xs.All(x => x);
		}

		public void Incorporate(PerformanceStatisticCollectorToken e2)
		{
			ZipSecondIntoFirst(this, e2);
		}

		internal static object ZipSecondIntoFirst(PerformanceStatisticCollectorToken e1, PerformanceStatisticCollectorToken e2)
		{
			e1.ElapsedExcludingChildren += e2.ElapsedExcludingChildren;
			e1.ElapsedIncludingChildren += e2.ElapsedIncludingChildren;
			e1.ActionCount += e2.ActionCount;
			if (e2.TimeEndedUtc > e1.TimeEndedUtc)
			{
				e1.TimeEndedUtc = e2.TimeEndedUtc;
			}

			// Count used to force recursion. Removal will break.
			e1.children.Zip(e2.children, ZipSecondIntoFirst).Count();
			return null;
		}

		internal void AddChildToken(PerformanceStatisticCollectorToken childToken)
		{
			children.Add(childToken);
		}

		internal void RemoveChildToken(PerformanceStatisticCollectorToken childToken)
		{
			children.Remove(childToken);
		}
	}
}
