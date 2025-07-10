using System;

namespace Enterprise.ZArchitecture.Core
{
	public abstract class LogHyperlink : IEquatable<LogHyperlink>
	{
		internal LogHyperlink(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				throw new ArgumentException("text cant be empty", nameof(text));
			}

			this.text = text;
		}

		public string Text
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return text; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "diagnostic information")]
		public sealed override string ToString()
		{
			return "[BROKEN-HL " + text + "]";
		}
		public abstract bool Equals(LogHyperlink other);

		readonly string text;
	}
}
