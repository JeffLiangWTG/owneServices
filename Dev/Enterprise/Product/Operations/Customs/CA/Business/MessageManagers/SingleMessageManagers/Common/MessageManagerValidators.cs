using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture.Data.Mutex;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	#region CanSendDeclarationChecker

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class CanSendDeclarationChecker
	{
		public CanSendDeclarationChecker(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public static void EntryNotNullAndAttachedToDeclaration(CusEntryHeader entryHeader)
		{
			Argument.NotNull(entryHeader, "entryHeader");
			Argument.NotNull(entryHeader, "entryHeader.Declaration", "No Declaration attached to Entry Header");
		}

		public bool DeclarationNotNull()
		{
			Argument.NotNull(declaration, "Declaration");
			return true;
		}

		public bool AtLeastOneEntryExists()
		{
			var entriesExist = declaration.CustomsEntryHeaders.Count > 0;
			if (!entriesExist)
			{
				LastErrorMessage = Res.GetString("9cb9cbbc-123e-4b5f-b6d5-94f62bb0c610", "No Entries exist for this declaration, please ensure at least one Invoice Header and Line have been entered and select 'Generate Entries' from the brokerage menu.");
			}
			return entriesExist;
		}

		public bool CheckEntryLineQuantities()
		{
			var result = declaration.CustomsEntryHeaders.Count > 0;
			foreach (CusEntryHeader entry in declaration.CustomsEntryHeaders)
			{
				var maxNumberOfEntryLines = entry.Validation.MaxNumberOfEntryLines;
				if (declaration != null && declaration.IsImport && !declaration.IsB3NotRequired && entry.IsB3C && entry.MergedLines.Count > maxNumberOfEntryLines)
				{
					if (declaration.CA_MergeBy.IsEmpty || declaration.CA_MergeBy == B3MergeByList.Codes.NotMerge || declaration.CA_MergeBy == B3MergeByList.Codes.NotMergeUsingProductNumberInDescription)
					{
						entry.AddRowMessageError(Res.GetString("14C7E24A-CD57-4A81-BC0F-2410B6CD2F71", "The number of entry lines exceeds {0}, system may not be able to generate a B3 document for this job. Please review the “B3 Merge By” option on the MISC tab and consider merging multiple invoice lines into one B3 line.", maxNumberOfEntryLines));
					}
					else
					{
						entry.AddRowMessageError(Res.GetString("5B08F807-C9CE-4B73-B746-F5C10B8043D0", "The number of entry lines exceeds {0}, system may not be able to generate a B3 document for this job. Please reduce the number of entry lines and create an additional entry if necessary.", maxNumberOfEntryLines));
					}
				}
			}
			return result;
		}

		public bool IsItNecessaryToReset()
		{
			if (!declaration.IsMergeDone)
			{
				LastErrorMessage = Res.GetString("19948229-0741-4b8d-9c79-337faf5f64a6", "You have no merged lines to throw away.");
			}
			return declaration.IsMergeDone;
		}

		public bool MultipleEntriesNotExist()
		{
			if (declaration.CustomsEntryHeaders.Count > 1)
			{
				throw new ArgumentException("Multiple entries exist on " + declaration.JE_DeclarationReference);
			}
			return true;
		}

		public bool RequiredEntryExist(string entryType)
		{
			if (declaration.CA_RequiresMerge || declaration.CustomsEntryHeaders.All(header => header.CH_MessageType != entryType))
			{
				LastErrorMessage = Res.GetString("4ce9c123-d60e-40f0-8042-fac65d8a9951",
					"Required entry doesn't exist, or it requires merge. Please run the option to Generate Entries from the brokerage menu, then save and try again. Entry type: {0}, Declaration job reference: {1}",
					entryType, declaration.JE_DeclarationReference);
				return false;
			}
			return true;
		}

		public bool LockEntryHeaderWithMutexWhenSendingMessage(string entryType)
		{
			var entryHeader = !string.IsNullOrEmpty(entryType) ? declaration.GetEntryHeaderFor(entryType) : (declaration.CustomsEntryHeaders.Count > 0 ? declaration.CustomsEntryHeaders[0] : null);
			if (entryHeader != null)
			{
				if (entryHeaderMutex == null)
				{
					entryHeaderMutex = new ZGlobalMutex(Enterprise.ZArchitecture.Modules.MutexIDs.SendCustomsMessage, entryHeader.PK.ToString());
				}

				if (!entryHeaderMutex.Lock())
				{
					var entryHeaderLockedByInfo = entryHeaderMutex.GetMutexLockByInfo();
					LastErrorMessage = Res.GetString("C350BE37-8082-4138-975A-754359504F74", "This entry has been locked, as {0} is trying to send the same message for this entry. Please wait until the lock has been released or close and re-open the form then try again.", entryHeaderLockedByInfo);
					return false;
				}
			}

			return true;
		}
		ZGlobalMutex entryHeaderMutex;

		public void DisposeEntryHeaderMutexAfterSendingMessage()
		{
			if (entryHeaderMutex != null && entryHeaderMutex.HasLock)
			{
				entryHeaderMutex.Unlock();
			}

			entryHeaderMutex = null;
		}

		public string LastErrorMessage { get; private set; }
		readonly JobDeclaration declaration;
	}

	#endregion

	#region EDIReleaseValidator

	#endregion
}
