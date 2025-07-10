using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading;
using CargoWise.Async;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;
using ResString = CargoWise.Main.ResString;

namespace Enterprise.Startup.Tools
{
	public class ThreadMonitor : NonPersistentBusinessObject<ThreadMonitorValidation>
	{
		public ThreadMonitor(Thread defaultThread)
		{
			this.defaultThread = defaultThread;
			this.SelectedThreadId = defaultThread.ManagedThreadId;

			IntervalNumber = 1;
			IntervalType = IntervalTypeConstant.Second;
		}

		readonly Thread defaultThread;

		#region Property

		#region IntervalNumber

		public ZInt IntervalNumber
		{
			get { return intervalNumber; }
			set
			{
				SetNonPersistentPropertyValue(IntervalNumberInfo, ref intervalNumber, value);

				Validation.ValidateIntervalNumber();
			}
		}

		public ZPropertyInfo IntervalNumberInfo
		{
			get { return GetZPropertyInfo(nameof(IntervalNumber), "Interval Number"); }
		}

		ZInt intervalNumber;

		#endregion

		#region IntervalType

		[List("IntervalTypes")]
		public ZString IntervalType
		{
			get { return intervalType; }
			set
			{
				SetNonPersistentPropertyValue(IntervalTypeInfo, ref intervalType, value);

				Validation.ValidateIntervalType();
				Validation.ValidateIntervalNumber();
			}
		}

		public ZPropertyInfo IntervalTypeInfo
		{
			get { return GetZPropertyInfo(nameof(IntervalType), "Interval Type"); }
		}

		public CodeDescriptionPairList IntervalTypes
		{
			get
			{
				if (intervalTypes == null)
				{
					intervalTypes = new CodeDescriptionPairList();
					intervalTypes.AddPair(IntervalTypeConstant.Millisecond, ResString.GetMultilingualString("8d456d89-8fbe-4d03-9d73-9ddecb9c728a9", "Millisecond"));
					intervalTypes.AddPair(IntervalTypeConstant.Second, ResString.GetMultilingualString("3d7f33ec-51ed-4013-81e8-9b977856ee99", "Second"));
					intervalTypes.AddPair(IntervalTypeConstant.Minute, ResString.GetMultilingualString("e274bbe3-53b1-4617-845a-936dbd63def4", "Minute"));
				}

				return intervalTypes;
			}
		}
		CodeDescriptionPairList intervalTypes;

		ZString intervalType;

		#endregion

		#region CallStack

		public ZString CallStack
		{
			get; private set;
		}

		#endregion

		#region IntervalMillisecondSafe

		public int IntervalMillisecondSafe
		{
			get
			{
				switch (IntervalType)
				{
					case IntervalTypeConstant.Millisecond:
					{
						return IntervalNumber > MaxMilliseconds
							? MaxMilliseconds
							: (int)TimeSpan.FromMilliseconds(IntervalNumber).TotalMilliseconds;
					}

					case IntervalTypeConstant.Second:
					{
						return IntervalNumber > MaxSeconds
							? MaxMilliseconds
							: (int)TimeSpan.FromSeconds(IntervalNumber).TotalMilliseconds;
					}

					case IntervalTypeConstant.Minute:
					{
						return IntervalNumber > MaxMinutes
							? MaxMilliseconds
							: (int)TimeSpan.FromMinutes(IntervalNumber).TotalMilliseconds;
					}

					default:

						return MaxMilliseconds;
				}
			}
		}

		#endregion

		public int MaxSeconds => (int)maxTimeSpan.TotalSeconds;

		public int MaxMinutes => (int)maxTimeSpan.TotalMinutes;

		public int MaxMilliseconds => (int)maxTimeSpan.TotalMilliseconds;

		readonly TimeSpan maxTimeSpan = TimeSpan.FromHours(1);

		#region Selected Thread

		[List(nameof(Threads))]
		public ZInt SelectedThreadId
		{
			get
			{
				return selectedThreadId;
			}
			set
			{
				SetNonPersistentPropertyValue(SelectedThreadIdInfo, ref selectedThreadId, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSelectedThread();
				}

				SelectedThreadIdInfo.RefreshBinding();
			}
		}

		ZInt selectedThreadId;

		public ZPropertyInfo SelectedThreadIdInfo => GetZPropertyInfo(nameof(SelectedThreadId));

		public CodeDescriptionPairList Threads
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(defaultThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture), string.IsNullOrEmpty(defaultThread.Name) ? Res.GetString("5eaaedb8-a028-4ae9-bc02-f0ce29a6243d", "Main Thread") : defaultThread.Name);

				foreach (var thread in DefaultAsyncStrategy.GetActiveSTAThreads().OrderBy(x => x.ManagedThreadId))
				{
					list.AddPairIfNotExist(thread.ManagedThreadId.ToString(CultureInfo.InvariantCulture), thread.Name);
				}

				return list;
			}
		}

		Thread SelectedThread
		{
			get
			{
				return SelectedThreadId == defaultThread.ManagedThreadId
					? defaultThread
					: DefaultAsyncStrategy.GetActiveSTAThreads().SingleOrDefault(x => x.ManagedThreadId == SelectedThreadId);
			}
		}

		#endregion

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
		public void LogCallStack()
		{
			var thread = SelectedThread;

			if (thread == null)
			{
				CallStack = Res.GetString("cf8ca950-df82-423b-a8c5-bb29772d0b44", "The selected thread is no longer active.");
			}
			else
			{
				try
				{
#pragma warning disable 0618
					try
					{
						if (thread != Thread.CurrentThread)
						{
							thread.Suspend();
						}

#if !WINZOR && !NET8_0_OR_GREATER
						CallStack = new StackTrace(thread, true).ToString();
#else
						CallStack = new StackTrace(true).ToString();
#endif
					}
					finally
					{
						if (thread != Thread.CurrentThread)
						{
							thread.Resume();
						}
					}
#pragma warning restore 0618
				}
				catch (ThreadStateException ex)
				{
					CallStack = ex.ToString();
				}
				catch (SecurityException ex)
				{
					CallStack = ex.ToString();
				}
			}
		}

		#region Implementations

		public override ThreadMonitorValidation GetNewValidation()
		{
			return new ThreadMonitorValidation(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return ResString.GetMultilingualString("6e1c71c8-10eb-4602-84c4-00255b42c683", "Thread Monitor"); }
		}

		#endregion
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "For Development")]
	public static class IntervalTypeConstant
	{
		public const string Millisecond = "Millisecond";
		public const string Second = "Second";
		public const string Minute = "Minute";
	}
}
