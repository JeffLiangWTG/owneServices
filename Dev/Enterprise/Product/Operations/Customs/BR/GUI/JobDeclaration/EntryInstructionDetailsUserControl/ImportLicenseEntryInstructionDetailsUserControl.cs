using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseEntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public ImportLicenseEntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			InitializeCustomizedGrid();
			InitSplitMenuItem();
		}

		public CusEntryInstruction CurrentEntryInstruction => (CusEntryInstruction)EntryInstructionsGrid.ListManager?.GetCurrent();

		protected void InitializeCustomizedGrid()
		{
			EntryInstructionsGrid.ReOrderColumns(ReorderedColumnsSequence);
		}

		string[] ReorderedColumnsSequence
		{
			get
			{
				if (reorderedColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						CusEntryInstruction.Schema.CEI_Description
					};
					reorderedColumnsSequence = columnList.ToArray();
				}
				return reorderedColumnsSequence;
			}
		}
		string[] reorderedColumnsSequence;

		#region Auto Split

		MenuItem[] fAutoSplitMenuItems;
		MenuItem[] AutoSplitMenuItems => fAutoSplitMenuItems ?? (fAutoSplitMenuItems = new[]
			{
				new ZMenuItem(ResString.GetMultilingualString("3ebd081a-a973-4e83-b2d6-947e4c13e145", "Auto Split"), (s, e) => TrySplitEntryLines()),
				new ZMenuItem("-")
			});

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var selectedInstruction = CurrentEntryInstruction;
			var canSplit = selectedInstruction != null && EntryInstructionsGrid.SelectedElements.Length < 2;

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

		void TrySplitEntryLines()
		{
			var declaration = JobDeclaration;
			if (declaration != null && CurrentEntryInstruction != null)
			{
				var splitMessage = CheckBeforeSplit();

				if (splitMessage.IsEmpty)
				{
					var splitter = new ImportLicenseEntryInstructionSplitter(CurrentEntryInstruction);
					splitMessage = splitter.CheckBeforeSplit();

					var continueSplitMessage = Res.GetString("025903d0-921a-401e-92eb-78853ed1d42c", "Continue to Split?");
					if (splitMessage.IsEmpty)
					{
						if (CurrentEntryInstruction.CEI_Description.Length > CusEntryInstruction.MaximumCharactersForWarningMessageForImportLicense)
						{
							if (Globals.Message.Show(MaximumCharactersNumberExceededMessage,
							continueSplitMessage,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning,
							DialogResult.Yes) == DialogResult.No)
							{
								Globals.Message.Show(Res.GetString("22E498D4-676C-43ED-9AEC-EBAF7A1BDE9B", "Split canceled!"));
								return;
							}
						}

						if (Globals.Message.Show(GetSplitConfirmMessage(splitter.EstimatedSplitCount),
							continueSplitMessage,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning,
							DialogResult.Yes) == DialogResult.Yes)
						{
							if (splitter.AutoSplit())
							{
								declaration.DoMerge();
								splitMessage = Res.GetString("e399c3af-8335-439c-a34b-d8462e2dc3ce", "Split succeeded!");
							}
						}
					}
				}

				if (!splitMessage.IsEmpty)
				{
					Globals.Message.Show(splitMessage);
				}
			}
		}

		ZString CheckBeforeSplit()
		{
			var result = ZString.Empty;

			var declaration = JobDeclaration;
			if (declaration != null)
			{
				if (declaration.HasChanges)
				{
					result = Res.GetString("7126363e-86f4-45da-aad8-ae549b4e691b", "Please save the job first.");
				}
				else if (!declaration.IsMergeDone)
				{
					result = Res.GetString("d472b7f8-efd9-49e4-a219-b1a7a33df8bb", "Please generate entries first by clicking Brokerage > Generate Entries (Merge).");
				}
			}

			return result;
		}

		string GetSplitConfirmMessage(int splitNumber)
		{
			return Res.GetString("80b5d40c-0402-4849-ae76-ed554d84cf67", "This Entry Instruction is going to be split to {0} Entry Instructions, do you want to save the job and continue?", splitNumber);
		}

		internal static string MaximumCharactersNumberExceededMessage => Res.GetString("85300815-D854-4A3E-B065-F2E0B6095F04", "The Entry Instruction Description has more than {0} characters. If split, the last few character(s) may be replaced by the Entry sequence number.",
			CusEntryInstruction.MaximumCharactersForWarningMessageForImportLicense);

		#endregion

	}
}
