using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
	public delegate void ProgressEvent(string progressText);

	public class InternalLogProcessorBase
	{
		#region Filter

		public string StartGuidSubString
		{
			get { return startGuidSubString; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
			set { startGuidSubString = value.ToLower(CultureInfo.InvariantCulture).Trim(); }
		}

		public string EndGuidSubString
		{
			get { return endGuidSubString; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
			set { endGuidSubString = value.ToLower(CultureInfo.InvariantCulture).Trim(); }
		}

		string startGuidSubString;
		string endGuidSubString;

		public virtual bool EnableFilter
		{
			get { return false; }
		}

		#endregion

		#region Process

		public bool Process()
		{
			try
			{
				UpdateProgress("Processing...");
				Factory = new BusinessObjectFactory();
				ProcessCore();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorMessage = "An Exception occurred while processing:" + System.Environment.NewLine + ex.ToString();
			}

			return (ErrorMessage == null);
		}

		protected virtual void ProcessCore()
		{
		}

		#endregion

		#region Message

		protected virtual string ProgressText
		{
			get { return "Processing: " + occurrenceCounter.ToString(CultureInfo.InvariantCulture); }
		}

		public virtual string CompletedStatusMessage
		{
			get { return "Processing Complete"; }
		}

		public string ErrorMessage
		{
			get;
			protected set;
		}

		#endregion

		#region Implementation

		protected virtual bool IsLogOkToProcess(EdiHelpErrorLog log)
		{
			return IsGuidInRange(log.PK);
		}

		protected bool IsGuidInRange(ZGuid guid)
		{
			if (StartGuidSubString == null || StartGuidSubString.Length == 0 || EndGuidSubString == null || EndGuidSubString.Length == 0)
			{
				return true;
			}

			string guidStr = guid.ToString();
			string from = guidStr.Substring(0, StartGuidSubString.Length);

			int compare = string.Compare(from, StartGuidSubString, StringComparison.Ordinal);
			if (compare >= 0)
			{
				string to = guidStr.Substring(0, EndGuidSubString.Length);

				compare = string.Compare(to, EndGuidSubString, StringComparison.Ordinal);
				if (compare <= 0)
				{
					return true;
				}
			}

			return false;
		}

		protected void UpdateProgress(string progressText)
		{
			if (OnUpdateProgress != null)
			{
				OnUpdateProgress(progressText);
			}
		}

		protected int occurrenceCounter;
		protected BusinessObjectFactory Factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event ProgressEvent OnUpdateProgress;

		#endregion
	}
}
