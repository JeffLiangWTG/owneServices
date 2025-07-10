using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Netting
{
	public partial class ParticipantStatementForm : ZChildForm
	{
		public ParticipantStatementForm()
		{
			InitializeComponent();
		}

		public ParticipantStatementForm(NettingDocumentPrinter nettingDocumentPrinter)
			: base(nettingDocumentPrinter)
		{
			InitializeComponent();

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			ZFormMenuStrategy.AddAdornments(this);
		}

		public override ODisplayMode DisplayMode => ODisplayMode.Browse;

		public override string FormVerb => string.Empty;

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void finalizePeriodButton_Click(object sender, EventArgs e)
		{
			if (DocumentPrinter.NettingPeriod.IsEmpty || !DocumentPrinter.NettingPeriod.IsValid)
			{
				Globals.Message.ShowError(GetEnterNettingPeriodMessage());
			}
			else
			{
				try
				{
					this.DisplayMode = ODisplayMode.ReadOnly;

					if (Globals.Message.Show(Res.GetString("0f77b3b8-81d3-4419-a053-73dd6702ef5a", @"Are you sure you want to finalize '{0}'?", DocumentPrinter.Period.NSP_Period), Res.GetString("872b9d89-186a-49a6-bce8-ef4b8ad247dd", "Confirm finalization"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						ZString errorMessage;
						using (var progressForm = new ProgressForm())
						{
							progressForm.Status = Res.GetString("C5419580-2EDD-4EE3-AD84-2C2BFEE56EEC", "Finalizing Netting Cycle...");
							progressForm.Show();
							errorMessage = DocumentPrinter.CallFinaliseNettingCycle();
						}

						if (errorMessage.IsEmpty)
						{
							var result = Globals.Message.Show(Res.GetString("9578FA40-AD11-4E20-9DFE-4BD1C32CAD31", @"Netting cycle finalization completed.
Do you want to Generate/Deliver Participant Statements? Generating Participant Statements might take a while.
Participant Statements can be generated at any time from the 'Netting' module."), Res.GetString("520F0A99-3C8C-41BC-B1A0-53BFF6005BB5", "Finalize Cycle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

							if (result == DialogResult.Yes)
							{
								using (var progressForm = new ProgressForm())
								{
									progressForm.Status = Res.GetString("706DA86F-8469-49E0-99A4-334294433654", @"Printing Participant Statements and Clearing Journals...");
									progressForm.Show();
									errorMessage = DocumentPrinter.PrintParticipantStatementsAndClearJournals();
								}
							}
						}
						else
						{
							Globals.Message.Show(errorMessage);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.Message);
				}
				finally
				{
					this.DisplayMode = ODisplayMode.Browse;
				}
			}
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			if (DocumentPrinter.NettingPeriod.IsEmpty || !DocumentPrinter.NettingPeriod.IsValid)
			{
				Globals.Message.ShowError(GetEnterNettingPeriodMessage());
			}
			else
			{
				try
				{
					ZString errorMessage;
					using (var progressForm = new ProgressForm())
					{
						progressForm.Status = Res.GetString("EB08DE8E-FDAD-4A71-90EC-EC3E4637587E", "Printing Statements...");
						progressForm.Show();
						errorMessage = DocumentPrinter.PrintAllFinalParticipantStatements();
					}

					if (!errorMessage.IsEmpty)
					{
						Globals.Message.Show(errorMessage);
					}
				}
				catch (IncorrectDataSetupException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void nettingCyclePrintButton_Click(object sender, EventArgs e)
		{
			if (DocumentPrinter.NettingPeriod.IsEmpty || !DocumentPrinter.NettingPeriod.IsValid)
			{
				Globals.Message.ShowError(GetEnterNettingPeriodMessage());
			}
			else
			{
				try
				{
					ZString errorMessage;
					using (var progressForm = new ProgressForm())
					{
						progressForm.Status = Res.GetString("30974A8E-0AAC-423D-9776-931279E9BF68", "Printing Clearing Bank Payment Document...");
						progressForm.Show();
						errorMessage = DocumentPrinter.PrintClearingBankPaymentDocument();
					}

					if (!errorMessage.IsEmpty)
					{
						Globals.Message.Show(errorMessage);
					}
				}
				catch (IncorrectDataSetupException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void allTransactionParticipantStatementButton_Click(object sender, EventArgs e)
		{
			if (DocumentPrinter.NettingPeriod.IsEmpty || !DocumentPrinter.NettingPeriod.IsValid)
			{
				Globals.Message.ShowError(GetEnterNettingPeriodMessage());
			}
			else
			{
				try
				{
					using (var progressForm = new ProgressForm())
					{
						progressForm.Status = Res.GetString("06168B50-220F-4808-A316-75124F22217B", "Printing Statements (Trial)...");
						progressForm.Show();
						DocumentPrinter.PrintAllTrialParticipantStatements();
					}
				}
				catch (IncorrectDataSetupException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		NettingDocumentPrinter DocumentPrinter => (NettingDocumentPrinter)BusinessEntity;

		static string GetEnterNettingPeriodMessage()
		{
			return Res.GetString("b8e09293-efc9-490c-993f-c978b572de21", "Please enter a valid Netting Cycle.");
		}

		void detailsStatementButton_Click(object sender, EventArgs e)
		{
			if (DocumentPrinter.NettingPeriod.IsEmpty || !DocumentPrinter.NettingPeriod.IsValid)
			{
				Globals.Message.ShowError(GetEnterNettingPeriodMessage());
			}
			else
			{
				try
				{
					ZString errorMessage;
					using (var progressForm = new ProgressForm())
					{
						progressForm.Status = Res.GetString("9AF9387E-594A-421E-9471-D02670CD338F", "Printing Detailed Statements...");
						progressForm.Show();
						errorMessage = DocumentPrinter.PrintDetailedParticipantStatements();
					}

					if (!errorMessage.IsEmpty)
					{
						Globals.Message.Show(errorMessage);
					}
				}
				catch (IncorrectDataSetupException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void zButton1_Click(object sender, EventArgs e)
		{
			if (DocumentPrinter.NettingPeriod.IsEmpty || !DocumentPrinter.NettingPeriod.IsValid)
			{
				Globals.Message.ShowError(GetEnterNettingPeriodMessage());
			}
			else
			{
				try
				{
					ZString errorMessage;
					using (var progressForm = new ProgressForm())
					{
						progressForm.Status = Res.GetString("E341B70C-E285-4A15-A00C-9E1ED793774F", "Printing Clearing Journals...");
						progressForm.Show();
						errorMessage = DocumentPrinter.PrintClearingJournals();
					}

					if (!errorMessage.IsEmpty)
					{
						Globals.Message.Show(errorMessage);
					}
				}
				catch (IncorrectDataSetupException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void trialDetailedParticipantStatementButton_Click(object sender, EventArgs e)
		{
			if (DocumentPrinter.NettingPeriod.IsEmpty || !DocumentPrinter.NettingPeriod.IsValid)
			{
				Globals.Message.ShowError(GetEnterNettingPeriodMessage());
			}
			else
			{
				try
				{
					using (var progressForm = new ProgressForm())
					{
						progressForm.Status = Res.GetString("B79867D1-5723-4278-B69B-3D1A8B10FBD0", "Printing Detailed Statements (Trial)...");
						progressForm.Show();
						DocumentPrinter.PrintAllTrialDetailedParticipantStatements();
					}
				}
				catch (IncorrectDataSetupException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}
	}
}
