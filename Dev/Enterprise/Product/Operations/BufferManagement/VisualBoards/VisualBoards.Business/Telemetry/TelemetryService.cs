using System;
using System.Diagnostics;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.VisualBoards.Business.Telemetry;

public sealed class TelemetryService
{
	public const string SourceName = "Enterprise.VisualBoards";
	[ThreadSafe]
	public static readonly ActivitySource ActivitySource = new ActivitySource(SourceName);
}

public class RootActivity : IDisposable
{
	public Activity Activity { get; }

	readonly Activity parent;
	bool disposed;
	public RootActivity(Activity activity, Activity parent) 
	{
		this.parent = parent;
		Activity = activity;
	}

	public void Dispose()
	{
		if (Activity != null && !disposed)
		{
			Activity.Dispose();
			Activity.Current = parent;
			disposed = true;
		}
	}
}

public static class ActivitySourceExtensions
{
	public static RootActivity StartRootActivity(this ActivitySource activitySource, string name)
	{
		var parent = Activity.Current;
		Activity.Current = null;
		var activity = activitySource.CreateActivity(name, ActivityKind.Internal);
		if (activity is null)
		{
			Activity.Current = parent;
			return null;
		}
		else
		{
			activity.Start();
			return new RootActivity(activity, parent);
		}
	}
}
