using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseEntryInstructionUnsplitter
	{
		public ImportLicenseEntryInstructionUnsplitter(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			entryInstructionsCanUnsplit = declaration.CustomsEntryInstructions.Where(x => CanUnsplit(x)).ToArray();
		}

		readonly JobDeclaration declaration;
		readonly CusEntryInstruction[] entryInstructionsCanUnsplit;

		#region Unsplit

		public void UnsplitEntryInstructions()
		{
			foreach (var entryInstruction in entryInstructionsCanUnsplit)
			{
				var parentEntryPK = entryInstruction.ParentEntryInstruction.PK;
				entryInstruction.InvoiceLines.ForEach(x => x.JI_CEI = parentEntryPK);

				if (entryInstruction.EntryHeader?.Messages.Count > 0)
				{
					entryInstruction.RemoveParentInstruction();
				}
				else
				{
					entryInstruction.Delete();
				}
			}
		}

		bool CanUnsplit(CusEntryInstruction entryInstruction)
		{
			return entryInstruction.HasParentEntryInstruction && (entryInstruction.EntryHeader?.EntryNumber.IsEmpty ?? true);
		}

		bool NeedsToUnsplit(ImportLicenseEntryCreationStrategy strategy, CusEntryInstruction entryInstruction)
		{
			return entryInstruction.InvoiceLines.Any(x => x.LastMergeKeyForImportLicenseEntry != strategy.GetKeyForLine(x));
		}

		public bool HasEntryInstructionsNeedToUnsplit()
		{
			var strategy = new ImportLicenseEntryCreationStrategy(declaration, true);
			return entryInstructionsCanUnsplit.Any(x => NeedsToUnsplit(strategy, x));
		}

		#endregion
	}
}
