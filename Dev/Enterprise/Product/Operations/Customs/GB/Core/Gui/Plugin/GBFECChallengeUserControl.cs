using System;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin
{
	public partial class GBFECChallengeUserControl : ZUserControl
	{
		public GBFECChallengeUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var listManager = FECChallengesGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged += ListManager_CurrentChanged;
			}
			ListManager_CurrentChanged(null, null);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var listManager = FECChallengesGrid.ListManager;
				if (listManager != null)
				{
					listManager.CurrentChanged -= ListManager_CurrentChanged;
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			switch (CurrentFECChallengeItem?.CY_Code)
			{
				case Business.FECChallengeFields.Codes.JI_NettMass:
				case Business.FECChallengeFields.Codes.JI_Price:
				case Business.FECChallengeFields.Codes.JI_Supp:
					NewValueCodeFindBox.Visible = false;
					NewValueCalcEdit.Visible = true;
					NewValueDropEdit.Visible = false;
					OriginalValueCodeFindBox.Visible = false;
					OriginalValueCalcEdit.Visible = true;
					OriginalValueDropEdit.Visible = false;
					break;
				case Business.FECChallengeFields.Codes.JI_NettMassUQ:
				case Business.FECChallengeFields.Codes.JI_SuppUQ:
					NewValueCodeFindBox.Visible = false;
					NewValueCalcEdit.Visible = false;
					NewValueDropEdit.Visible = true;
					OriginalValueCodeFindBox.Visible = false;
					OriginalValueCalcEdit.Visible = false;
					OriginalValueDropEdit.Visible = true;
					break;
				default:
					NewValueCodeFindBox.Visible = true;
					NewValueCalcEdit.Visible = false;
					NewValueDropEdit.Visible = false;
					OriginalValueCodeFindBox.Visible = true;
					OriginalValueCalcEdit.Visible = false;
					OriginalValueDropEdit.Visible = false;
					break;
			}
		}

		FECChallenge CurrentFECChallengeItem => FECChallengesGrid.ListManager.GetCurrent() as FECChallenge;
	}
}
