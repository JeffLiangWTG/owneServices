using System;
using System.Text;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class KeyAppendStrategy : IAppendStrategy
	{
		public void AppendMatch(string text, StringBuilder builder)
		{
			string line = TidyStackFrame(text);

			bool shouldAppend = true;
			foreach (IMatcher pattern in linesToRemove)
			{
				if (pattern.IsMatch(line) && (builder.Length > 0 || pattern.MatchTop))
				{
					shouldAppend = false;
					break;
				}
			}

			if (shouldAppend && lastLine != line)
			{
				builder.Append(line);
				builder.Append(System.Environment.NewLine);
			}

			lastLine = line;
		}

		#region Implementation

		string lastLine = "";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Manipulating stack traces, not setting file paths")]
		string TidyStackFrame(string stackFrame)
		{
			string result = stackFrame;

			if (result != null)
			{
				result = result.Trim();

				int index = result.IndexOf(@"in C:\", StringComparison.Ordinal);
				if (index > -1)
				{
					result = result.Substring(0, index);
				}

				index = result.IndexOf("(", StringComparison.Ordinal);
				if (index > -1)
				{
					result = result.Substring(0, index + 1);
				}
			}

			return result;
		}

		#region Matching

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static readonly IMatcher[] linesToRemove = new IMatcher[]
		{
			new StringMatcher("at System.Environment.GetStackTrace(", true),
			new StringMatcher("at System.Environment.get_StackTrace(", true),
			new StringMatcher("at Enterprise.ZArchitecture.Core.BaseExceptionReporter.", true),
			new StringMatcher("at Enterprise.ZArchitecture.Core.", true),
			new StringMatcher("at Enterprise.ZArchitecture.Environment.UserNotification.", true),
			new StringMatcher("at Enterprise.ZArchitecture.Environment.UserNotificationBase.", true),
			new StringMatcher("at Enterprise.ZArchitecture.Core.ExceptionReportingForm.", true),
			new StringMatcher("at WTG.ErrorReporting.NLog.ErrorReportingNLogTarget.Write(", true),
			new StringMatcher("at CargoWise.Data.DbCommand.Execute("),
			new StringMatcher("at Enterprise.ZArchitecture.ErrorReporter.", true),
			new StringMatcher("at Enterprise.ZArchitecture.BusinessObjectFactory.Enterprise.Core.ZIntegration.IPersistentFactory.Save("),
			new StringMatcher("at Enterprise.ZArchitecture.DataAccess.RowFactory.SaveTogetherUnsafe("),
			new StringMatcher("at Enterprise.ZArchitecture.DataAccess.RowFactory.SaveTogether("),
			new StringMatcher("at Enterprise.ZArchitecture.ZWinForm.HandleSaveButtonUnsafe("),
			new StringMatcher("at Enterprise.ZArchitecture.ZWinForm.SaveInternal("),
			new StringMatcher("at Enterprise.ZArchitecture.BusinessObjectFactory.SaveTogether("),
			new StringMatcher("at Enterprise.ZArchitecture.ComponentModel."),
			new StringMatcher("at System.Reflection.RuntimeMethodInfo.InternalInvoke("),
			new StringMatcher("at System.Reflection.RuntimeMethodInfo.Invoke("),
			new WildcardStringMatcher("at System.Windows.Forms.*.WndProc("),
			new WildcardStringMatcher("at System.Windows.Forms.*.DefWndProc("),
			new WildcardStringMatcher("at System.Windows.Forms.*.OnMessage("),
			new WildcardStringMatcher("at System.Windows.Forms.*.OnClick("),
			new WildcardStringMatcher("at System.Windows.Forms.*.WmActivate("),
			new WildcardStringMatcher("at System.Windows.Forms.*.WmCommand("),
			new WildcardStringMatcher("at System.Windows.Forms.*.SendMessage("),
			new StringMatcher("at System.Windows.Forms.Command.Invoke("),
			new StringMatcher("at System.Windows.Forms.MenuItemData.Execute("),
			new StringMatcher("at System.Windows.Forms.Control.ReflectMessageInternal("),
			new StringMatcher("at System.Windows.Forms.UnsafeNativeMethods.CallWindowProc("),
			new StringMatcher("at System.Windows.Forms.NativeWindow.Callback("),
			new StringMatcher("at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW("),
			new StringMatcher("at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageA("),
			new StringMatcher("at System.Windows.Forms.ComponentManager.System.Windows.Forms.UnsafeNativeMethods+IMsoComponentManager.FPushMessageLoop("),
			new StringMatcher("at System.Windows.Forms.ThreadContext.RunMessageLoopInner("),
			new StringMatcher("at System.Windows.Forms.ThreadContext.RunMessageLoop("),
			new StringMatcher("at System.Windows.Forms.Application.Run("),
			new StringMatcher("at Enterprise.Startup.ApplicationStartupDirector.RunEnterprise("),
			new StringMatcher("at Enterprise.Startup.ApplicationStartupDirector.StartEnterprise("),
			new StringMatcher("at Enterprise.Startup.ApplicationStartupDirector.Main("),
			new StringMatcher("----- Exception caught and reported here -----", true)
		};

		interface IMatcher
		{
			bool IsMatch(string value);
			bool MatchTop { get; }
		}

		abstract class BaseMatcher : IMatcher
		{
			protected BaseMatcher(bool matchTop)
			{
				this.matchTop = matchTop;
			}

			public bool MatchTop
			{
				get { return matchTop; }
			}

			public abstract bool IsMatch(string value);

			readonly bool matchTop;
		}

		class StringMatcher : BaseMatcher
		{
			public StringMatcher(string pattern)
				: this(pattern, false)
			{
			}

			public StringMatcher(string pattern, bool matchTop)
				: base(matchTop)
			{
				this.pattern = pattern;
			}

			public override bool IsMatch(string value)
			{
				return value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) > -1;
			}

			readonly string pattern;
		}

		class WildcardStringMatcher : BaseMatcher
		{
			public WildcardStringMatcher(string pattern)
				: this(pattern, false)
			{
			}

			public WildcardStringMatcher(string pattern, bool matchTop)
				: base(matchTop)
			{
				startPattern = pattern.Substring(0, pattern.IndexOf("*", StringComparison.Ordinal));
				endPattern = pattern.Substring(pattern.IndexOf("*", StringComparison.Ordinal) + 1);
			}

			readonly string startPattern;
			readonly string endPattern;

			public override bool IsMatch(string value)
			{
				int startPatternIndex = value.IndexOf(startPattern, StringComparison.Ordinal);
				return startPatternIndex > -1 && value.IndexOf(endPattern, StringComparison.Ordinal) > startPatternIndex;
			}
		}

		#endregion

		#endregion
	}
}

