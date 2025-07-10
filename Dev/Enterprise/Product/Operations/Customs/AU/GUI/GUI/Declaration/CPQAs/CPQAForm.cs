using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	/// <summary>
	/// Summary description for CPQAForm.
	/// </summary>
	public partial class CPQAForm : ZChildForm
	{
		public CPQAForm(CusEntryHeaderMessageStatusFilteredCollection entryHeaders) : base(entryHeaders)
		{
			this.entryHeaders = entryHeaders;
			ControlVisibility();
		}

		readonly CusEntryHeaderMessageStatusFilteredCollection entryHeaders;

		void ControlVisibility()
		{
			var declaration = (entryHeaders.FirstOrDefault() as CusEntryHeader)?.Declaration;
			var showLodgementQuestionsControl = declaration == null
												|| declaration.IsAggregateDeclaration
												|| !ConsolidatedDeclaration.IsConsolidated(declaration);

			if (!showLodgementQuestionsControl)
			{
				cpqAsForHeaderControl2.Visible = false;
				var messageLabel = new ZArchitecture.ZLabel
				{
					Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true),
					Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 50, true),
					ForeColor = System.Drawing.Color.Red,
					IsFontBold = true,
					Name = "LodgementQuestionsMessageLabel",
					Text = "General Lodgement Questions are located in the Consolidated Entry."
				};

				declarationQuestionsGroupBox.Controls.Add(messageLabel);
			}
		}

		public override string FormCaption
		{
			get { return "Lodgement Questions"; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ValidateEntries();
		}

		protected virtual void ValidateEntries()
		{
			foreach (CusEntryHeader entryHeader in entryHeaders)
			{
				entryHeader.RunPreSaveValidation();
			}
		}

		public bool IsOKToProceed;
		void OKButton_Click(object sender, EventArgs e)
		{
			if (!entryHeaders.AreAllCPDecQuestionsAnswered())
			{
				var hasErrors = entryHeaders.Cast<CusEntryHeader>().Any(x => x.Questions.Cast<CMRCusEntryCPDec>().Any(y => y.ON_AnswerCodeInfo.HasErrors())
				|| x.AllEntryLines.Cast<CusEntryLine>().Any(y => y.Questions.Cast<CMRCusEntryCPDec>().Any(z => z.ON_AnswerCodeInfo.HasErrors())));
				if (hasErrors)
				{
					Globals.Message.Show("There are questions that are not answered correctly. Please fix the errors and try again!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else if (Globals.Message.Show("There are questions that are not answered yet, or have warnings. Are you sure you wish to continue?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
				{
					IsOKToProceed = true;
					Close();
				}
			}
			else
			{
				IsOKToProceed = true;
				Close();
			}
		}

		void CPQACancelButton_Click(object sender, EventArgs e)
		{
			IsOKToProceed = false;
			Close();
		}
	}
}
