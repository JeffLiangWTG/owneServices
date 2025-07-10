using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business.Duimp;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		#region Overrides

		protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();

			if (Declaration.IsImportExcludingLicense)
			{
				new AdditionalInformationGenerator(Declaration).GenerateAdditionalInformation();
			}

			if (Declaration.IsImportOnly)
			{
				UpdateEntryHeaderCustomsPostedStatus();
				UpdateEntryLinesCustomsPostedStatus();
				foreach (var entryHeader in Declaration.CustomsEntryHeaders)
				{
					entryHeader.SiscomexUsageFees.Load();
				}
			}
		}

		BusinessObjectFactory NewFactory => newFactory ?? (newFactory = new BusinessObjectFactory());
		BusinessObjectFactory newFactory;

		void UpdateEntryHeaderCustomsPostedStatus()
		{
			var entryHeaders = Declaration.ActiveEntryHeaders.FormalEntries.Where(x => x.CH_CustomsPostedStatus.IsAccepted()).ToList();
			if (entryHeaders.Count > 0)
			{
				var entryHeadersInDatabase = NewFactory.Load<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.PK, entryHeaders.Select(x => x.PK)));

				foreach (var entryHeader in entryHeaders)
				{
					var entryHeaderInDatabase = entryHeadersInDatabase.SingleOrDefault(x => x.PK == entryHeader.PK);
					if (entryHeaderInDatabase != null && GetDuimpHeaderMessageText(entryHeader) != GetDuimpHeaderMessageText(entryHeaderInDatabase))
					{
						entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
					}
				}
			}

			string GetDuimpHeaderMessageText(CusEntryHeader entryHeader)
			{
				return new DuimpHeaderMessageBuilder(new DuimpHeaderProvider(new DuimpMessageSendingObject(entryHeader))).GetMessageText();
			}
		}

		void UpdateEntryLinesCustomsPostedStatus()
		{
			var entryLines = Declaration.ActiveEntryHeaders.FormalEntries.SelectMany(h => h.MergedLines.Where(l => l.CL_CustomsPostedStatus.IsAccepted())).ToList();
			if (entryLines.Count > 0)
			{
				var entryLinesInDatabase = NewFactory.Load<CusEntryLine>(new ZQuery(CusEntryLineSchema.PK, entryLines.Select(x => x.PK)));

				foreach (var entryLine in entryLines)
				{
					var entryLineInDatabase = entryLinesInDatabase.SingleOrDefault(x => x.PK == entryLine.PK);
					if (entryLineInDatabase != null && GetDuimpItemMessageText(entryLine) != GetDuimpItemMessageText(entryLineInDatabase))
					{
						entryLine.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
					}
				}
			}

			string GetDuimpItemMessageText(CusEntryLine entryLine)
			{
				return new DuimpLinesMessageBuilder(DuimpLinesProvider.New(new[] { entryLine })).GetMessageText();
			}
		}

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			return new Customs.Business.EntryCreationStrategy[]
			{
				new ExportEntryCreationStrategy(Declaration),
				new ImportEntryCreationStrategy(Declaration),
				new ImportLicenseEntryCreationStrategy(Declaration),
				new ImportSiscomexEntryCreationStrategy(Declaration),
				new LPCOEntryCreationStrategy(Declaration),
				new ImportUsageFeeEntryCreationStrategy(Declaration)
			};
		}

		protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

		protected override void OnMerged()
		{
			base.OnMerged();

			Declaration.ActiveEntryHeaders.AllMergedLinesFees.Where(fee => fee.IsEmpty).ToList().ForEach(x => x.Delete());

			SplitEntryInstructionsIfNeeded();
		}

		void SplitEntryInstructionsIfNeeded()
		{
			if (Declaration.IsImportLicense)
			{
				var anyEntryInstructionHasBeenSplit = false;

				foreach (var entryInstruction in Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().ToArray())
				{
					if (new ImportLicenseEntryInstructionSplitter(entryInstruction).AutoSplit())
					{
						anyEntryInstructionHasBeenSplit = true;
					}
				}

				if (anyEntryInstructionHasBeenSplit)
				{
					DoMerge();
				}
			}
		}

		#endregion
	}
}
