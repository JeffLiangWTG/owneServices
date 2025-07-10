using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
    public partial class CPQAControlBase
    {
		private void InitializeComponent()
		{
			this.individualQuestionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.questionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ManditoryQuestionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.individualQuestionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// IndividualQuestionGroupBox
			// 
			this.individualQuestionGroupBox.Controls.Add(this.questionLabel);
			this.individualQuestionGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.individualQuestionGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.individualQuestionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 248, true);
			this.individualQuestionGroupBox.Name = "IndividualQuestionGroupBox";
			this.individualQuestionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 72, true);
			this.individualQuestionGroupBox.TabIndex = 1;
			this.individualQuestionGroupBox.TabStop = false;
			this.individualQuestionGroupBox.Text = "Question";
			// 
			// QuestionLabel
			// 
			this.questionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.questionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.questionLabel.Name = "QuestionLabel";
			this.questionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 53, true);
			this.questionLabel.TabIndex = 0;
			this.questionLabel.Text = "Question";
			// 
			// ManditoryQuestionsGrid
			// 
			this.ManditoryQuestionsGrid.AllowNavigation = false;
			this.ManditoryQuestionsGrid.BindTo = ".";
			this.ManditoryQuestionsGrid.CaptionVisible = false;
			this.ManditoryQuestionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManditoryQuestionsGrid.EnableToolTips = false;
			this.ManditoryQuestionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ManditoryQuestionsGrid.LayoutKey = "zGrid1";
			this.ManditoryQuestionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManditoryQuestionsGrid.Name = "ManditoryQuestionsGrid";
			this.ManditoryQuestionsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ManditoryQuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 248, true);
			this.ManditoryQuestionsGrid.TabIndex = 0;
			// 
			// CPQAControlBase
			// 
			this.Controls.Add(this.ManditoryQuestionsGrid);
			this.Controls.Add(this.individualQuestionGroupBox);
			this.Name = "CPQAControlBase";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 320, true);
			this.individualQuestionGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		private ZGroupBox individualQuestionGroupBox;
		protected ZArchitecture.ZLabel questionLabel;
		public ZArchitecture.ZGrid ManditoryQuestionsGrid;
	}
}
