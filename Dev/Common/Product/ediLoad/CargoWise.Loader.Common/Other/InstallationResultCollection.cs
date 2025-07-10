using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public sealed class InstallationResultCollection : IEnumerable<InstallationResult>
	{
		readonly List<InstallationResult> results;

		public InstallationResultCollection()
		{
			results = new List<InstallationResult>();
		}

		public int Count
		{
			get { return results.Count; }
		}

		public int ErrorCount { get; private set; }
		public int OKCount { get; private set; }
		public int WarningCount { get; private set; }

		public InstallationResult this[int index]
		{
			get
			{
				if (index < 0)
				{
					throw new ArgumentException("Invalid argument.", nameof(index));
				}

				if (index >= this.Count)
				{
					throw new ArgumentException("Invalid argument.", nameof(index));
				}

				return results[index];
			}
		}

		public void Add(InstallationResult result)
		{
			Argument.NotNull(result, nameof(result));
			switch (result.Status)
			{
				case InstallationResultStatus.OK:
					OKCount++;
					break;
				case InstallationResultStatus.Warning:
					WarningCount++;
					break;
				case InstallationResultStatus.Error:
					ErrorCount++;
					break;
			}
			results.Add(result);
		}

		public string GetErrorMessages()
		{
			return GetMessages(delegate(InstallationResult result)
			{
				return result.IsError;
			});
		}

		public string GetWarningMessages()
		{
			return GetMessages(delegate(InstallationResult result)
			{
				return result.IsWarning;
			});
		}

		string GetMessages(Predicate<InstallationResult> predicate)
		{
			Argument.NotNull(predicate, nameof(predicate));
			StringBuilder messages = new StringBuilder();
			foreach (InstallationResult result in this)
			{
				if (result != null && predicate.Invoke(result) && !string.IsNullOrEmpty(result.Message))
				{
					if (messages.Length > 0)
					{
						messages.AppendLine();
						messages.AppendLine();
					}
					messages.Append(result.Message);
				}
			}
			return messages.ToString();
		}

		#region IEnumerable<InstallationResult> Members

		IEnumerator<InstallationResult> IEnumerable<InstallationResult>.GetEnumerator()
		{
			return results.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return results.GetEnumerator();
		}

		#endregion
	}
}
