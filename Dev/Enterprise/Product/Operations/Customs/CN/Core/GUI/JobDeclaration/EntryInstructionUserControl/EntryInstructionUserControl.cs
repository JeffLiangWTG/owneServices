using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using SplitStrategyProvider = Enterprise.Customs.CN.Business.EntryInstructionAutoSplitStrategyProvider;

namespace Enterprise.Customs.CN.GUI
{
	public partial class EntryInstructionUserControl : BaseCustomsEntryUserControl
	{
		public EntryInstructionUserControl()
		{
			InitializeComponent();
			InitSplitMenuItem();
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)CurrentDataItem;

		public CusEntryInstruction CurrentEntryInstruction => (CusEntryInstruction)EntryInstructionsGrid.ListManager?.GetCurrent();

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			DetailsUserControl.UserControlType = typeof(LayoutEntryInstructionDetailUserControl);
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			var willGenerateBothEntries = JobDeclaration?.WillGenerateBothEntries ?? false;
			EntryInstructionsGrid.SetAvailability(willGenerateBothEntries, CusEntryInstruction.Schema.CEI_CEI_Parent);
			EntryInstructionsGrid.SetAvailability(willGenerateBothEntries, "ParentInstruction+CEI_Description");
		}

		#region Auto Split

		MenuItem[] fAutoSplitMenuItems;
		MenuItem[] AutoSplitMenuItems => fAutoSplitMenuItems ?? (fAutoSplitMenuItems = new[]
			{
				new ZMenuItem(
					ResString.GetMultilingualString("BC0BE2A4-9FDA-40F1-BB07-9F29FE4DA9E0", "Auto Split"),
					new[]
					{
						new ZMenuItem(ResString.GetMultilingualString("B3C0CEE9-4979-400A-9014-AD40D9E8E7FE", "Up to 50 Entry Lines"), (s, e) => TrySplitEntryLines(SplitStrategyProvider.SplitBy.LineCount50)),
						new ZMenuItem(ResString.GetMultilingualString("3E75B1F6-43AB-40E3-815D-EA3188B03988", "Up to 20 Entry Lines"), (s, e) => TrySplitEntryLines(SplitStrategyProvider.SplitBy.LineCount20)),
						new ZMenuItem(ResString.GetMultilingualString("2306CDA6-556E-4F48-BCAA-0882382B8CB1", "Based on Legal Inspection required or not"), (s, e) => TrySplitEntryLines(SplitStrategyProvider.SplitBy.LegalInspection)),
					}
				),
				new ZMenuItem("-")
			});

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var selectedInstruction = CurrentEntryInstruction;
			var canSplit = selectedInstruction != null && !selectedInstruction.IsChild && EntryInstructionsGrid.SelectedElements.Length < 2;

			foreach (var menuItem in AutoSplitMenuItems)
			{
				menuItem.Enabled = canSplit;
			}
		}

		void InitSplitMenuItem()
		{
			EntryInstructionsGrid.ContextMenu.MenuItems.InsertRange(0, AutoSplitMenuItems);
			EntryInstructionsGrid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void TrySplitEntryLines(SplitStrategyProvider.SplitBy splitBy)
		{
			var declaration = JobDeclaration;

			IEntryInstructionAutoSplitStrategy splitStrategy = null;
			var errorMsg = CheckUIOperations();
			if (errorMsg.IsEmpty)
			{
				splitStrategy = SplitStrategyProvider.GetStrategy(CurrentEntryInstruction, splitBy);
				errorMsg = splitStrategy.CheckBeforeSplit();
			}

			if (errorMsg.IsEmpty && splitStrategy != null)
			{
				if ((ParentForm as ZForm).PreSaveDeclaration(JobDeclaration, GetSplitConfirmMessage(splitStrategy.EstimatedSplitCount), SplitConfirmPopupCaption))
				{
					splitStrategy.AutoSplit();
					declaration.DoMerge();
				}
			}
			else
			{
				Globals.Message.Show(errorMsg);
			}
		}

		ZString CheckUIOperations()
		{
			var result = new ZString();

			var declaration = CurrentEntryInstruction.JobDeclaration;
			if (!declaration.IsMergeDone)
			{
				result = Res.GetString("AF8ACA3D-1167-4DD5-BD1E-052BEE9CD4DA", "Please generate entries first by clicking Brokerage > Generate Entries (Merge).");
			}

			return result;
		}

		string SplitConfirmPopupCaption => Res.GetString("f472101d-2680-45c8-8513-f488c8992626", "Auto Split");

		string GetSplitConfirmMessage(int splitNumber)
		{
			return Res.GetString(
				"c7e91e9a-93de-4a60-be07-37884ff9fab1",
				"This Entry Instruction is going to be split to {0} Entry Instructions, do you want to save the job and continue?",
				splitNumber
			);
		}

		#endregion
	}
}
