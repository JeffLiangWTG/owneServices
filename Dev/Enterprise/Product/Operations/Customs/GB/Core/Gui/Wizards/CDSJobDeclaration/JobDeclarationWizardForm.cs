using System;
using System.Linq;
using Enterprise.Customs.GB.CDS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Wizards
{
	public partial class JobDeclarationWizardForm : ZChildForm
	{
		public JobDeclarationWizardForm()
		{
			InitializeComponent();
#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(LabelQuestion, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		public JobDeclarationWizardForm(IDeclarationWizardManager manager)
		{
			InitializeComponent();
			GroupBoxOptions.Text = string.Empty;
			this.manager = manager;
			RefreshQuestion();
		}

		void RefreshQuestion()
		{
			var options = manager.DeclarationWizard.GetOptions();

			if (options.Any())
			{
				LabelQuestion.Text = manager.DeclarationWizard.CurrentQuestion.QuestionText;
				CreateRadioButtonsForQuestion(options);
			}

			RefreshButtons();
		}

		void RefreshButtons()
		{
			ButtonPrevious.Enabled = !manager.DeclarationWizard.IsFirstQuestion();
			ButtonNext.Visible = !manager.DeclarationWizard.IsLastQuestion();
			ButtonFinish.Visible = manager.DeclarationWizard.IsLastQuestion();
		}

		void CreateRadioButtonsForQuestion(System.Collections.Generic.List<DeclarationWizardOption> options)
		{
			GroupBoxOptions.Controls.Clear();
			int x = 10;
			int y = 10;
			int count = 0;

			foreach (var option in options)
			{
				var radioButton = new ZRadioButton();
				radioButton.Text = option.Description;
				radioButton.Tag = option;
				radioButton.Checked = option.Selected;
				radioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(x, y + (count * 30), true);
				radioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 20, true);
				radioButton.CheckedChanged += RadioButton_CheckedChanged;

				GroupBoxOptions.Controls.Add(radioButton);
				count++;
			}
		}

		void RadioButton_CheckedChanged(object sender, EventArgs e)
		{
			ZRadioButton radioButton = sender as ZRadioButton;
			DeclarationWizardOption option = radioButton?.Tag as DeclarationWizardOption;

			if (option != null)
			{
				option.Selected = radioButton.Checked;
			}
		}

		readonly IDeclarationWizardManager manager;

		void ButtonNext_Click(object sender, EventArgs e)
		{
			manager.NextQuestion();
			RefreshQuestion();
		}

		void ButtonPrevious_Click(object sender, EventArgs e)
		{
			manager.PreviousQuestion();
			RefreshQuestion();
		}

		void ButtonFinish_Click(object sender, EventArgs e)
		{
			manager.FinishWizard();
			manager.PopulateDeclaration();
			this.Close();
		}
	}
}
