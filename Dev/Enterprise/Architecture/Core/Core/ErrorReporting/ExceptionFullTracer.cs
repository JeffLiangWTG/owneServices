using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;

namespace Enterprise.ZArchitecture.Core
{
	public abstract class ExceptionFullTracer
	{
		protected string TraceToReporter;
		protected WeakReference CurrentExceptionWeakRef = new WeakReference(null);  // WeakReference is used in case Exception holds references to application objects

		protected void CaptureTraceToReporter(Exception ex)
		{
			if ((ex != null) && (CurrentExceptionWeakRef.Target != ex))
			{
				CurrentExceptionWeakRef.Target = ex;

				TraceToReporter = BuildTrace(new StackTrace(), true);
			}
		}

		protected string BuildTrace(Exception ex)
		{
			Argument.NotNull(ex, "ex");
			string result = "";
			var rootCauseStackTrace = (ex as IWithRootCauseStackTrace)?.RootCauseStackTrace;
			if (rootCauseStackTrace != null)
			{
				result = BuildTrace(rootCauseStackTrace, false);
			}
			else if (IsExceptionWithRemoteStack(ex))
			{
				result = BuildTrace(ex.StackTrace, false);
			}
			else
			{
				result = BuildTrace(new StackTrace(ex), false);
				if (string.IsNullOrWhiteSpace(result))
				{
					result = BuildTrace(ex.StackTrace, false);
				}
			}
			return result;
		}

#if DEBUG
		protected
#endif
 string BuildTrace(StackTrace trace, bool removeReportElements)
		{
			var result = new StringBuilder();
			var frames = trace.GetFrames();

			if (frames != null)
			{
				foreach (StackFrame frame in frames)
				{
					var method = frame.GetMethod();

					AppendTypeAndMethod(result, method);
					AppendParameterList(result, method);
					AppendILOffsetInfo(result, method, frame);

					result.Append("\r\n");
				}
			}

			result.Replace('+', '.'); // some nested class method names have + (plus) instead of . (dot) so replace them

			return removeReportElements ? TracerHelper.RemoveReportElements(result.ToString()) : result.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Must be culture independent")]
		string BuildTrace(string stackTrace, bool removeReportElements)
		{
			var result = new StringBuilder();

			if (string.IsNullOrWhiteSpace(stackTrace))
			{
				return result.ToString();
			}

			string[] callLines = stackTrace.Split('\n');
			for (int i = 0; i < callLines.Length; i++)
			{
				string callLine = callLines[i].Trim();
				if (!string.IsNullOrEmpty(callLine))
				{
					if (callLine.StartsWith("at "))
					{
						int lineNoIndex = callLine.IndexOf(')');
						if (lineNoIndex > 0)
						{
							callLine = callLine.Substring(0, lineNoIndex + 1);
						}

						result.Append("   ").AppendLine(callLine);
					}
					else if (result.Length > 0)
					{
						result.Append("----- ").Append(callLine).AppendLine(" -----");
					}
				}
			}

			result.Replace('+', '.');

			return removeReportElements ? TracerHelper.RemoveReportElements(result.ToString()) : result.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Must be culture independent")]
		void AppendTypeAndMethod(StringBuilder result, MethodBase method)
		{
			result.Append("   at ");

			Type declaringMethod = method.DeclaringType;
			if (declaringMethod != null)
			{
				result.Append(method.DeclaringType.ToString());
				result.Append(".");
				result.Append(method.Name);
			}
		}

		void AppendParameterList(StringBuilder result, MethodBase method)
		{
			result.Append("(");
			bool first = true;

			foreach (var param in method.GetParameters())
			{
				if (!first)
				{
					result.Append(", ");
				}
				else
				{
					first = false;
				}

				result.Append(param.ParameterType.Name);
				result.Append(" ");
				result.Append(param.Name);
			}

			result.Append(")");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Feature")]
		void AppendILOffsetInfo(StringBuilder result, MethodBase method, StackFrame frame)
		{
			string assemblyName = "";
			string typeName = "";
			string methodName = "";
			int ilOffset = 0;
			string parameters = "";
			bool added = true;

			try
			{
				assemblyName = method.Module.Name;
				typeName = method.DeclaringType.ToString();
				methodName = method.Name;
				parameters = GetParameters(method);

				if (frame.GetILOffset() != StackFrame.OFFSET_UNKNOWN)
				{
					ilOffset = frame.GetILOffset();
				}
				else
				{
					added = false;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				added = false;
			}

			if (added)
			{
				result.Append(string.Format(" [Assembly={0}]", assemblyName));
				result.Append(string.Format(" [Type={0}]", typeName));
				result.Append(string.Format(" [Method={0}]", methodName));
				result.Append(string.Format(" [ILOffset={0}]", ilOffset));
				result.Append(string.Format(" [Parameters={0}]", parameters));
			}
		}

		string GetParameters(MethodBase method)
		{
			var parameters = method.GetParameters();

			if (parameters.Length == 0)
			{
				return "NoParameters";
			}

			return string.Join(";", method.GetParameters().Select(p => p.ParameterType.ToString()));
		}

		protected string StripLineNoAndFilename(string frame)
		{
			int index = frame.IndexOf(" in ");
			if (index >= 0)
			{
				frame = frame.Substring(0, index) + "\r\n";
			}

			return frame;
		}

		#region IsExceptionWithRemoreStack

		bool IsExceptionWithRemoteStack(Exception ex)
		{
			if (remoteStackIndexFieldInfo == null)
			{
				remoteStackIndexFieldInfo = typeof(Exception).GetField("_remoteStackIndex", BindingFlags.Instance | BindingFlags.NonPublic);
			}
			return ex != null && remoteStackIndexFieldInfo != null && (int)remoteStackIndexFieldInfo.GetValue(ex) > 0;
		}

		[ThreadStatic]
		static FieldInfo remoteStackIndexFieldInfo;

		#endregion
	}
}
