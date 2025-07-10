using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Enumeration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI.Controls.Internal
{
	class ControlTracker
	{
		readonly List<TrackedControl> controls = new List<TrackedControl>();

		public int ControlCount => controls.Count;

		public void TrackInstanceForLeak(Control control)
		{
			controls.Add(new TrackedControl(control));
		}

		public void StopTrackingInstanceForLeak(Control controlToStopTracking)
		{
			using (var leaks = new DisposableList(controls.Count))
			{
				var noOutOfMemoryThrown = ErrorReporter.LastExceptionsReported()
					.All(exceptionText => exceptionText.IndexOf(OutOfMemoryExceptionTypeName, StringComparison.InvariantCulture) < 0);
				var noSystemShutdownThrown = !ErrorReporter.LastExceptionsReported()
					.Any(exceptionText => exceptionText.IndexOf(SqlExceptionName, StringComparison.InvariantCulture) >= 0 && exceptionText.IndexOf(GCTracker.ShutdownErrorMessageHeader, StringComparison.InvariantCulture) >= 0);
				var noNetworkSQLExceptionThrown = !ErrorReporter.LastExceptionsReported()
					.Any(exceptionText => exceptionText.IndexOf(SqlExceptionName, StringComparison.InvariantCulture) >= 0 && exceptionText.IndexOf(GCTracker.NetworkSQLExceptionMessageHeader, StringComparison.InvariantCulture) >= 0);
				var noTCPConnectionFailureThrown = !ErrorReporter.LastExceptionsReported()
					.Any(exceptionText => exceptionText.IndexOf(SqlExceptionName, StringComparison.InvariantCulture) >= 0 && exceptionText.IndexOf(GCTracker.TCPConnectionFailureMessageHeader, StringComparison.InvariantCulture) >= 0);
				var reportLeaks = noOutOfMemoryThrown && noSystemShutdownThrown && noNetworkSQLExceptionThrown && noTCPConnectionFailureThrown;

				var message = reportLeaks ? new StringBuilder() : null;

				foreach (var item in controls.ToArray())
				{
					var target = item.Control;
					if (target == null || target == controlToStopTracking)
					{
						controls.Remove(item);
					}
					else if (!target.IsDisposed)
					{
						var parents = ZEnumerable.Iterate(target.Parent, c => c.Parent, null);
						var topParent = parents.LastOrDefault();
						var parentNotDisposed = parents.LastOrDefault(x => !x.IsDisposed);
						if (topParent == null || topParent.IsDisposed)
						{
							controls.Remove(item);
							leaks.Add(target);

							if (reportLeaks)
							{
								message.AppendLine((NoResString)"Leak detected. Please check \"Previous Exceptions thrown\" section for the root cause.")
								.Append("Name = ").AppendLine(target.Name)
								.Append("TopParent ").AppendLine(topParent == null ? (NoResString)"is null" : $"({topParent.Name}) is disposed")
								.Append((NoResString)"Parent NOT Disposed is ").AppendLine(parentNotDisposed == null ? "null" : $"{parentNotDisposed.Name} ({ControlDescription.GetControlPath(parentNotDisposed)})")
								.AppendLine((NoResString)"Parent Changes: ").Append(item);

								if (topParent is IDisposeStackProvider disposeStackProvider && disposeStackProvider.DisposeStack != null)
								{
									message.AppendLine((NoResString)"Parent Disposed Stack Trace :").Append(disposeStackProvider.DisposeStack);
								}

								message.AppendLine();
							}
						}
					}
				}

				if (leaks.Any() && reportLeaks)
				{
					ErrorReporter.ReportOnce("ControlTacker - Leaking Control(s):", message.ToString());
				}
			}
		}

		string OutOfMemoryExceptionTypeName => outOfMemoryExceptionTypeName ?? (outOfMemoryExceptionTypeName = typeof(OutOfMemoryException).FullName);
		string outOfMemoryExceptionTypeName;

		string SqlExceptionName => sqlExceptionName ?? (sqlExceptionName = typeof(SqlException).FullName);
		string sqlExceptionName;

		class TrackedControl
		{
			readonly WeakReference<Control> control;
			readonly List<(string path, string stack)> movements = new List<(string, string)>();

			public Control Control
				=> control.TryGetTarget(out var t) ? t : null;

			public TrackedControl(Control c)
			{
				control = new WeakReference<Control>(c);
				RecordPath(false);

				c.ParentChanged += (o, e) => RecordPath();
			}

			public void RecordPath(bool includeStack = true)
				=> movements.Add((ControlDescription.GetControlPath(Control), includeStack ? new StackTrace().ToString() : string.Empty));

			public override string ToString()
			{
				var sb = new StringBuilder();
				foreach (var (path, stack) in movements)
				{
					sb.Append((NoResString)"Path: ").AppendLine(path)
						.Append((NoResString)"Stack: ").Append(string.IsNullOrEmpty(stack) ? (NoResString)"Not collected" : stack)
						.AppendLine();
				}

				return sb.ToString();
			}
		}
	}
}
