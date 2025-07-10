using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseEntryInstructionSplitter
	{
		public ImportLicenseEntryInstructionSplitter(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			invoiceLineGroups = entryInstruction.InvoiceLines.GroupBy(x => x.JI_CL).SelectMany(x => x.Batch(maximumInvoiceLinesAllowed)).ToArray();
		}

		readonly CusEntryInstruction entryInstruction;

		readonly IEnumerable<JobComInvoiceLine>[] invoiceLineGroups;
		readonly int maximumInvoiceLinesAllowed = CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense;

		public int EstimatedSplitCount => invoiceLineGroups.Length;

		#region AutoSplit

		public bool AutoSplit()
		{
			var succeeded = false;

			using (entryInstruction.JobDeclaration.CustomsEntryInstructions.SuspendListChanged())
			{
				var count = EstimatedSplitCount;
				if (count > 1)
				{
					for (var groupSerial = 1; groupSerial < count; groupSerial++)
					{
						var newInstruction = CloneEntryInstruction(entryInstruction);
						newInstruction.LinkToParentInstruction(entryInstruction);
						foreach (BaseJobComInvoiceLine invoiceLine in invoiceLineGroups[groupSerial])
						{
							invoiceLine.JI_CEI = newInstruction.PK;
						}

						OnAfterSplitting(newInstruction, groupSerial);
					}

					OnAfterSplitting(entryInstruction, 0);
					succeeded = true;
				}
			}

			return succeeded;
		}

		string GetSuffixBySerial(int serial)
		{
			return (serial + 1).ToString(CultureInfo.InvariantCulture);
		}

		public ZString CheckBeforeSplit() => EstimatedSplitCount > 1 ? string.Empty : NoNeedToSplitMessage;

		public static string NoNeedToSplitMessage => Res.GetString("529abdbd-1b56-4ac4-8779-ba83515f2069", "The count of Invoice Lines linked to this Entry Instruction does not exceed the limit and all Invoice Lines have been merged to one Entry Line. No need to split this Entry Instruction.");

		void OnAfterSplitting(CusEntryInstruction instruction, int serial)
		{
			if (serial > 0)
			{
				var descSuffix = GetSuffixBySerial(serial);
				ToggleSplitDescription(instruction, descSuffix);
			}
		}

		#endregion

		#region Private static methods to clone and set values

		CusEntryInstruction CloneEntryInstruction(CusEntryInstruction instruction)
		{
			var newEntryInstruction = instruction.Clone();
			newEntryInstruction.JobDeclaration.CustomsEntryInstructions.Add(newEntryInstruction);
			return newEntryInstruction;
		}

		void ToggleSplitDescription(CusEntryInstruction instruction, ZString suffix)
		{
			var description = instruction.CEI_Description + "-" + suffix;
			if (description.Length > instruction.CEI_DescriptionInfo.MaxLength)
			{
				var difference = description.Length - instruction.CEI_DescriptionInfo.MaxLength;
				description = instruction.CEI_Description.Substring(0, description.Length - difference - suffix.Length - 1) + "-" + suffix;
			}
			instruction.CEI_Description = description;
		}

		#endregion
	}
}
