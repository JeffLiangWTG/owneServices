using System;
using System.Collections.Generic;
using System.Diagnostics;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	public class DummyOperationalActionSectionLog : IOperationalActionSectionLog
	{
		public bool IncludeDebug
		{
			[DebuggerStepThrough]
			get
			{
				return includeDebug;
			}

			[DebuggerStepThrough]
			set
			{
				includeDebug = value;
			}
		}

		public void Verify()
		{
			VerifyCore();
		}

		public string MessagesString()
		{
			return string.Join("\n", messages.ToArray());
		}

		protected virtual void VerifyCore()
		{
			if (lastSectionCount != lastSectionMax)
			{
				throw new InvalidOperationException("Section never reached the end");
			}
		}

		protected void ResetSection()
		{
			sectionSet = false;
			lastSectionMax = 0;
			lastSectionCount = 0;
		}

		protected virtual void NotifyCore(OperationalActionLogErrorLevel errorLevel, string text)
		{
			if (errorLevel > highestErrorLevelEncountered)
			{
				highestErrorLevelEncountered = errorLevel;
			}

			switch (errorLevel)
			{
				case OperationalActionLogErrorLevel.Error:
					messages.Add("ERROR: " + text);
					break;
				case OperationalActionLogErrorLevel.Warning:
					messages.Add("WARNING: " + text);
					break;
				case OperationalActionLogErrorLevel.Success:
					messages.Add("SUCCESS: " + text);
					break;
				case OperationalActionLogErrorLevel.Informational:
					messages.Add("INFO: " + text);
					break;
				case OperationalActionLogErrorLevel.Debug:
					messages.Add("DEBUG: " + text);
					break;
				default:
					messages.Add("OTHER: " + text);
					break;
			}
		}

		public OperationalActionLogErrorLevel HighestErrorLevelEncountered
		{
			[DebuggerStepThrough]
			get
			{
				return highestErrorLevelEncountered;
			}
		}

		#region IOperationalActionSectionLog Members
		public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
		{
			if (includeDebug || errorLevel >= OperationalActionLogErrorLevel.Informational)
			{
				for (int i = 0; i < args.Length; i++)
				{
					// Only add the HL marker from here to help prevent the accidental breakage of links by people using
					// the standard string.Format().
					LogHyperlink link = args[i] as LogHyperlink;
					if (link != null)
					{
						args[i] = "[HL " + link.Text + "]";
					}
				}

				NotifyCore(errorLevel, string.Format(format, args));
			}
		}

		public void Notify(OperationalActionLogErrorLevel errorLevel, string text)
		{
			if (includeDebug || errorLevel >= OperationalActionLogErrorLevel.Informational)
			{
				NotifyCore(errorLevel, text);
			}
		}

		public void SetSectionProgressMax(int max)
		{
			if (sectionSet)
			{
				throw new InvalidOperationException("Section max already set");
			}

			sectionSet = true;
			lastSectionMax = max;
		}

		public void BumpSectionProgress()
		{
			if (!sectionSet)
			{
				throw new InvalidOperationException("Section max not set yet");
			}

			if (lastSectionCount == lastSectionMax)
			{
				throw new InvalidOperationException("Already reached the end");
			}

			lastSectionCount++;
		}

		#endregion
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool includeDebug;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool sectionSet;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int lastSectionMax;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int lastSectionCount;
		[DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		OperationalActionLogErrorLevel highestErrorLevelEncountered;
		readonly public List<string> messages = new List<string>();
	}
}
