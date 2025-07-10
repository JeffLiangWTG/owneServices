using System;
using System.Windows.Forms;

namespace Enterprise.StlAnalysis.Load
{
	public partial class STLAnalysisForm : Form // Cannot use ZArchitecture because this is a standalone tool
	{
		public STLAnalysisForm()
		{
			InitializeComponent();
		}

		void RefreshCollectionControls()
		{
			uscAdHocConsultationControl.RefreshControls();
			uscEditFeatureControl.RefreshControls();
		}

		public const string ListViewCodeColumnKey = "Code";
		public const string ListViewModuleColumnKey = "Module";
		public const string ListViewStlBasisColumnKey = "StlBasis";
		public const string ListViewStlEntityColumnKey = "StlEntity";
		public const string ListViewUnitsColumnKey = "UnitCount";

		void STLAnalysisForm_Load(object sender, EventArgs e)
		{
			using (new UserInterfaceSuspender(this))
			{
				RefreshCollectionControls();
			}
		}

		void STLAnalysisForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (uscLoadUserControl.IsEtlRunning)
			{
				var dialogResult = MessageBox.Show( // Cannot use Enterprise Environment because this is a standalone tool
					this,
					"Do you want to cancel the load process?",
					"Cancel ETL?",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button2);

				if (dialogResult == DialogResult.Yes)
				{
					uscLoadUserControl.AbortEtl();
				}
				else
				{
					e.Cancel = true;
				}
			}
		}
	}
}
