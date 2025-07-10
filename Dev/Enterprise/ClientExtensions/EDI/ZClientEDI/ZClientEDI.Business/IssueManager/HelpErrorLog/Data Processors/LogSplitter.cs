using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class LogSplitter : InternalLogProcessorBase
	{
		public LogSplitter(HelpErrorLogOccurrence occurrenceToSplit)
		{
			this.occurrenceToSplit = occurrenceToSplit;
			parentLog = occurrenceToSplit.Issue;
			occurrencesToDelete = new List<HelpErrorLogOccurrence>();
			newLogs = new List<EdiHelpErrorLog>();
		}

		#region Process

		protected override void ProcessCore()
		{
			Factory = parentLog.Factory;
			occurrenceCounter = 0;
			occurrencesToDelete.Clear();
			newLogs.Clear();
			updateModulus = 5;

			if (parentLog.KeyCount > 1)
			{
				ProcessSplit();
			}
			else
			{
				ErrorMessage = CannotSplitIfOnlyOneKey;
			}
		}

		void ProcessSplit()
		{
			ExceptionXml xml = new ExceptionXml(occurrenceToSplit.HO_XMLData);
			if (xml.IsValid)
			{
				xml.PopulateFromXML();

				SplitAllOccurrencesWithKey(xml.KeyFields.LogKey);

				LogThisSplitAction();
				Factory.Save();
			}
			else
			{
				ErrorMessage = CannotProcessInvalidXml;
			}
		}

		void SplitAllOccurrencesWithKey(string key)
		{
			// need to do this before splitting the occurrences so that when the occurrences are processed they don't match back to parentLog
			RemoveKeyFromParentLog(key);

			HelpErrorLogCollection allLogs = new HelpErrorLogCollection(Factory);
			allLogs.Load();

			// use a filtered reader otherwise we run out of memory on logs with 1000s of occurrences
			FilteredBusinessObjectReader reader = GetNewFilteredBusinessObjectReader(occurrenceToSplit.Issue.PK);
			BusinessObject[] occurrenceList = reader.LoadNextBatchInANewFactory(null);

			while (occurrenceList.Length > 0)
			{
				foreach (BusinessObject bizo in occurrenceList)
				{
					UpdateOccurrenceCounters();
					SplitOccurrenceIfItMatchesKey(allLogs, bizo as HelpErrorLogOccurrence, key);
				}

				occurrenceList = reader.LoadNextBatchInANewFactory(occurrenceList[occurrenceList.Length - 1]);
			}

			reader.Factory.Save();

			DeleteSplitOccurrences();
		}

		void SplitOccurrenceIfItMatchesKey(HelpErrorLogCollection allLogs, HelpErrorLogOccurrence occurrence, string key)
		{
			ExceptionXml xml = new ExceptionXml(occurrence.HO_XMLData);
			if (xml.IsValid)
			{
				xml.PopulateFromXML();
				if (xml.KeyFields.LogKey == key)
				{
					EdiHelpErrorLog newLog = xml.Process(allLogs, true);

					// save new logs so later we can add a system split log
					if (newLog != null && !newLogs.Contains(newLog))
					{
						newLogs.Add(newLog);
					}

					// add this occurrence into a list for later deletion
					occurrencesToDelete.Add(Factory.Load<HelpErrorLogOccurrence>(occurrence.PK));
				}
			}
		}

		void RemoveKeyFromParentLog(string keyString)
		{
			var keysToDelete = new List<HelpErrorLogKey>();

			foreach (HelpErrorLogKey key in parentLog.Keys)
			{
				if (key.HK_Key == keyString)
				{
					keysToDelete.Add(key);
				}
			}

			foreach (HelpErrorLogKey key in keysToDelete)
			{
				parentLog.Keys.RemoveAndDelete(key);
			}
		}

		void DeleteSplitOccurrences()
		{
			foreach (HelpErrorLogOccurrence o in occurrencesToDelete)
			{
				parentLog.Occurrences.RemoveAndDelete(o);
				parentLog.HE_FailCount--;
			}
		}

		void LogThisSplitAction()
		{
			foreach (EdiHelpErrorLog newLog in newLogs)
			{
				parentLog.Logs.AddNew(Events.DocumentSplit, "Occurrence(s) split to Issue: " + newLog.HE_IssueNumber);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				newLog.Logs.AddNew(Events.AddedARecordToTheSystem, "Occurrence(s) added by split from Issue: " + newLog.HE_IssueNumber);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		FilteredBusinessObjectReader GetNewFilteredBusinessObjectReader(ZGuid logPK)
		{
			ZQuery query = new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, logPK);
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(query, typeof(HelpErrorLogOccurrence));
			reader.BatchSize = 500;
			reader.SaveBeforeLoadNextEnabled = true;
			return reader;
		}

		#endregion

		#region Update Progress Messages

		protected override string ProgressText
		{
			get { return "Scanning Occurrences: " + occurrenceCounter.ToString(CultureInfo.InvariantCulture) + "\r\n"; }
		}

		public override string CompletedStatusMessage
		{
			get { return "Split Complete: " + occurrencesToDelete.Count.ToString(CultureInfo.InvariantCulture) + " Occurrence(s) have been split from this Log"; }
		}

		#endregion

		#region Implementation

		void UpdateOccurrenceCounters()
		{
			occurrenceCounter++;

			if (occurrenceCounter % updateModulus == 0)
			{
				UpdateProgress(ProgressText);
			}
		}

		int updateModulus;
		readonly List<EdiHelpErrorLog> newLogs;
		readonly EdiHelpErrorLog parentLog;
		readonly HelpErrorLogOccurrence occurrenceToSplit;
		readonly List<HelpErrorLogOccurrence> occurrencesToDelete;

		internal const string CannotSplitIfOnlyOneKey = "This Issue has only one key so there is nothing to split";
		internal const string CannotProcessInvalidXml = "This occurrence contains invalid xml and could not be processed";

		#endregion
	}
}

