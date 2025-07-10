using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	// CARGO  HANDLER  - REMOVAL  NOTE  - also called GATE PASS
	class CnsChildProcessor_CmiRel4 : CnsChildProcessor
	{
		internal override void Process(EDIMessage receivedEdiMessage, ILogger serviceLogger, IEDocsDelayedSaver eDocsSaver)
		{
			// This message could refer to LOTS of entries.  Hence not the standard processing. 
			// Attached the message to one entry, but show it as a text eDoc on all the entries.
			this.receivedEdiMessage = receivedEdiMessage;
			var firstLineOfBody = GetFirstLineOfMessageText(receivedEdiMessage);
			var firstOrMainEntryHeader = FindMainEntryAndAllOtherEntriesAndUpdateAllEntries(eDocsSaver, firstLineOfBody);

			if (firstOrMainEntryHeader != null)
			{
				InterpretMessage(receivedEdiMessage);
				firstOrMainEntryHeader.Messages.Add(receivedEdiMessage);
				receivedEdiMessage.EM_Status = "RCV";
				SendNotificationEmail(firstOrMainEntryHeader, firstLineOfBody);
			}
			else
			{
				receivedEdiMessage.EM_Status = "FAL";  // Failed to find job.

				if (receivedEdiMessage.EM_ReceiveTransmit == EDIMessage.Direction.Receive && receivedEdiMessage.Interchange != null)
				{
					receivedEdiMessage.Interchange.EI_Status = "FAL";
				}

				SendFailureNotificationEmail("Reference or job could not be found - " + firstLineOfBody, receivedEdiMessage);
			}
		}

		ZString GetListOfEntries()
		{
			var startOfListOfLines = "CC Date";
			var endOfListOfLines = "Certify that all goods have cleared customs";
			var lotsOfDashes = "-------------------------------------------------------------------------------";
			var body = receivedEdiMessage.EM_MessageText;
			body = body.SubstringSafe(0, body.IndexOf(endOfListOfLines, StringComparison.OrdinalIgnoreCase));
			body = body.Substring(body.IndexOf(startOfListOfLines, StringComparison.OrdinalIgnoreCase) + startOfListOfLines.Length);
			return body.Replace(lotsOfDashes, "").Trim();
		}

		CusEntryHeader FindMainEntryAndAllOtherEntriesAndUpdateAllEntries(IEDocsDelayedSaver eDocsSaver, string firstLineOfBody)
		{
			CusEntryHeader firstEntryFound = null;
			var listOfEntries = GetListOfEntries();
			foreach (var line in listOfEntries.Split(new[] { '\r', '\n' }))
			{
				var cleanLine = line.Trim();

				var fieldCount = cleanLine.Split(" ").Length;

				if (cleanLine.IsEmpty || fieldCount < 6)
				{
					continue;
				}

				var oneUcn = cleanLine.Substring(0, cleanLine.IndexOf(" ", StringComparison.OrdinalIgnoreCase));
				var entryNumberWithoutEpu = Regex.Match(cleanLine, @"\d{6}[A-Z]");
				var clearanceDate = Regex.Match(cleanLine, @"\d{1,2}-\d{1,2}-\d{2}");
				if (entryNumberWithoutEpu.Captures.Count > 0 && clearanceDate.Captures.Count > 0)
				{
					var entry = FindEntry(entryNumberWithoutEpu.Captures[0].Value, oneUcn);
					if (entry != null)
					{
						if (firstEntryFound == null)
						{
							firstEntryFound = entry;
						}
					}
					UpdateEntryToClearAndAddFileToEdocs(entry, eDocsSaver, firstLineOfBody, clearanceDate.Value);
				}
			}
			return firstEntryFound;
		}

		void UpdateEntryToClearAndAddFileToEdocs(CusEntryHeader entry, IEDocsDelayedSaver eDocsSaver, string firstLineOfBody, ZString clearanceDateString)
		{
			ZDateTime clearanceDate;
			if (ZDateTime.TryParseExact(clearanceDateString, out clearanceDate, "dd-MM-yy"))
			{
				SaveToEDocs(entry, eDocsSaver, firstLineOfBody);
				entry.CH_EntryStatus = EntryStatusList.Codes.Clear;
				entry.CH_EntryReleaseDate = clearanceDate;
			}
		}

		CusEntryHeader FindEntry(string entryNumWithoutEpu, string oneUcn)
		{
			ZDBOnlySubQuery entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.MasterUCR);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, oneUcn);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddSubQuery(entryNumberQuery, JoinCondition.And);
			var decsWithThisUcn = receivedEdiMessage.Factory.Load<JobDeclaration>(query);
			foreach (var dec in decsWithThisUcn)
			{
				foreach (CusEntryHeader entry in dec.CustomsEntryHeaders)
				{
					if (entry.EntryNumber.EndsWith(entryNumWithoutEpu, StringComparison.OrdinalIgnoreCase))
					{
						return entry;
					}
				}
			}

			return null;
		}
	}
}



