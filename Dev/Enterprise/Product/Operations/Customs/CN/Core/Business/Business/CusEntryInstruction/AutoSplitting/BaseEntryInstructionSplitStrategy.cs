using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CN.Business
{
	public abstract class BaseEntryInstructionSplitStrategy : IEntryInstructionAutoSplitStrategy
	{
		protected BaseEntryInstructionSplitStrategy(CusEntryInstruction entryInstruction)
		{
			EntryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			CusEntryLines = ((CusEntryHeader)entryInstruction.EntryHeader).MergedLines.Cast<CusEntryLine>();
		}

		protected readonly CusEntryInstruction EntryInstruction;

		protected readonly IEnumerable<CusEntryLine> CusEntryLines;

		public abstract int EstimatedSplitCount { get; }

		public void AutoSplit()
		{
			using (EntryInstruction.JobDeclaration.CustomsEntryInstructions.SuspendListChanged())
			{
				var entryLineGroups = GroupEntryLines().ToArray();
				if (entryLineGroups.Length > 1)
				{
					for (var groupSerial = 1; groupSerial < entryLineGroups.Length; groupSerial++)
					{
						var newInstruction = CloneEntryInstruction(EntryInstruction);
						foreach (BaseJobComInvoiceLine invoiceLine in entryLineGroups[groupSerial].SelectMany(entryLine => entryLine.InvoiceLines))
						{
							invoiceLine.JI_CEI = newInstruction.PK;
						}

						OnAfterSplitting(newInstruction, groupSerial);
					}

					OnAfterSplitting(EntryInstruction, 0);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected abstract IEnumerable<IEnumerable<CusEntryLine>> GroupEntryLines();

		protected abstract string GetSuffixBySerial(int serial);

		public ZString CheckBeforeSplit()
		{
			ZString result = ZString.Empty;

			var declaration = EntryInstruction.JobDeclaration;
			if (!declaration.MergeManager.RequiresMerge || declaration.DoMerge())
			{
				var entryHeaderToSplit = EntryInstruction.EntryHeader as CusEntryHeader;
				if (entryHeaderToSplit == null)
				{
					result = Res.GetString("5771AB19-8F8B-41B7-8E19-5B7B07076561", "There is no Entry Header linked to this Entry Instruction.");
				}
			}

			if (result.IsEmpty)
			{
				result = CheckBeforeSplitCore();
			}

			return result;
		}

		protected abstract ZString CheckBeforeSplitCore();

		void OnAfterSplitting(CusEntryInstruction instruction, int serial)
		{
			var descSuffix = GetSuffixBySerial(serial);
			ToggleSplitDescription(instruction, descSuffix);

			OnAfterSplittingCore(instruction);
		}

		protected abstract void OnAfterSplittingCore(CusEntryInstruction instruction);

		#region Private static methods to clone and set values

		static CusEntryInstruction CloneEntryInstruction(CusEntryInstruction instruction, bool includingChild = true)
		{
			var newEntryInstruction = (CusEntryInstruction)instruction.Clone();
			CloneAndClearValues(instruction, newEntryInstruction);
			newEntryInstruction.JobDeclaration.CustomsEntryInstructions.Add(newEntryInstruction);

			if (includingChild)
			{
				var childInstruction = instruction.ChildInstruction;
				if (childInstruction != null)
				{
					var newChildInstruction = CloneEntryInstruction(childInstruction, false);
					newChildInstruction.CEI_CEI_Parent = newEntryInstruction.PK;
				}
			}

			return newEntryInstruction;
		}

		static void CloneAndClearValues(CusEntryInstruction origin, CusEntryInstruction target)
		{
			using (target.GetValidationSuspender())
			using (target.SuspendSettingHasChanges())
			{
				ClearNoOfPackages(origin, target);
				CloneStmNotes(origin, target);
				CloneCusEntryNumbers(origin, target);
				CloneCusCodeDataCollection(origin.OperationMatters, target.OperationMatters);
				CloneCusCodeDataCollection(origin.OtherPackages, target.OtherPackages);
				CloneCusCodeDataCollection(origin.SpecialBusinessIdentifiers, target.SpecialBusinessIdentifiers);
				CloneCusCodeDataCollection(origin.EnterpriseQualifications, target.EnterpriseQualifications);
				CloneCusAddInfoCollection(origin.CIQRequiredDocuments, target.CIQRequiredDocuments);
			}
		}

		static void ClearNoOfPackages(CusEntryInstruction origin, CusEntryInstruction target)
		{
			origin.CEI_Packages = target.CEI_Packages = ZInt.Zero;
		}

		static void CloneStmNotes(CusEntryInstruction origin, CusEntryInstruction target)
		{
			target.CustomsMessageRemarks = origin.CustomsMessageRemarks;
		}

		static void CloneCusEntryNumbers(CusEntryInstruction origin, CusEntryInstruction target)
		{
			target.BillOfLading = origin.BillOfLading;
			target.BillOfLadingDate = origin.BillOfLadingDate;
		}

		static void CloneCusCodeDataCollection<T>(CusCodeDataCollection<T> origin, CusCodeDataCollection<T> target) where T : CusCodeData
		{
			foreach (var cusCodeData in origin)
			{
				target.Add(cusCodeData.Clone());
			}
		}

		static void CloneCusAddInfoCollection<T, MasterT>(DependentCusAddInfoCollection<T, MasterT> origin, DependentCusAddInfoCollection<T, MasterT> target)
			where T : CusAddInfo
			where MasterT : BusinessObject, ILinkable
		{
			foreach (var addInfo in origin)
			{
				target.Add(addInfo.Clone());
			}
		}

		static void ToggleSplitDescription(CusEntryInstruction instruction, ZString suffix, bool includingChild = true)
		{
			instruction.CEI_Description = new ZString(instruction.CEI_Description + "-" + suffix).Left(instruction.CEI_DescriptionInfo.MaxLength);
			if (includingChild)
			{
				var childInstruction = instruction.ChildInstruction;
				if (childInstruction != null)
				{
					ToggleSplitDescription(childInstruction, suffix, false);
				}
			}
		}

		#endregion
	}
}
