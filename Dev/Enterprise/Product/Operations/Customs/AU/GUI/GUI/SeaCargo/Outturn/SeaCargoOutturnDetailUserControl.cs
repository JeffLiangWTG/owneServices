using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoOutturnDetailUserControl : ZUserControl
	{
		public SeaCargoOutturnDetailUserControl()
		{
			InitializeComponent();
		}

		private ZArchitecture.ZLabel containerNumberLabel;
		private ZArchitecture.ZLabel sealNumberLabel;
		private ZArchitecture.ZLabel outternResultLabel;
		private ZArchitecture.ZLabel houseBillLabel;
		private ZArchitecture.ZLabel oceanBillLabel;
		private ZCheckBox sealIntactCheckBox;
		private ZCheckBox damagedCheckBox;
		private ZCheckBox pillagedCheckBox;
		private ZArchitecture.ZLabel receiptLabel;
		private ZArchitecture.ZLabel unpackLabel;
		private ZArchitecture.ZLabel packagesLabel;
		private ZArchitecture.ZLabel cargoTypeLabel;
		private ZArchitecture.ZLabel goodsDescriptionLabel;
		private ZArchitecture.ZLabel marksNumsLabel;
		private ZDateEdit receiptDateEdit;
		private ZDateEdit unpackDateEdit;
		private ZDropEdit cargoTypeDropEdit;
		private ZArchitecture.ZTextBox containerNumberTextBox;
		private ZCalcDropEdit packagesCalcDropEdit;
		private ZArchitecture.ZTextBox oceanBillTextBox;
		private ZArchitecture.ZTextBox houseBillTextBox;
		private ZArchitecture.ZTextBox goodsDescriptionTextBox;
		private ZArchitecture.ZTextBox marksNumsTextBox;
		private ZArchitecture.ZTextBox sealNumberTextBox;
		private ZDropEdit outturnResultDropEdit;
		private ZCalcDropEdit outerPacksCalcDropEdit;
		private ZArchitecture.ZLabel outerPacksLabel;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel customsStatusLabel;
		private ZArchitecture.ZTextBox customsStatusTextBox;
		private ZDropEdit commercialStatusDropEdit;
		private ZButton detailsButton;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZTextBox underbondStatusTextBox;
		private ZButton seaCargoInfoButton1;

		#region User Friendly Status

		public CusOutturn CurrentOutturn
		{
			get
			{
				CusOutturn result = currentOutturn;
				if (result == null)
				{
					BindingManagerBase bm = GetBindingManager("Outturns");
					result = bm == null || bm.Position == -1 ? null : (CusOutturn)bm.GetCurrent();
				}
				return result;
			}
			set { currentOutturn = value; }
		}
		CusOutturn currentOutturn;

		internal void DetailsButton_Click(object sender, System.EventArgs e)
		{
			if (CurrentOutturn != null && !((ICalculatedCusStatusCalculator)CurrentOutturn.StatusCalculator).UserFriendlyStatusText.IsEmpty)
			{
				Globals.Message.ShowInformation(((ICalculatedCusStatusCalculator)CurrentOutturn.StatusCalculator).UserFriendlyStatusText, UserFriendlyStatusMessages.StatusMessageHeader);
			}
			else
			{
				Globals.Message.ShowInformation(UserFriendlyStatusMessages.StatusNotAvailable, UserFriendlyStatusMessages.StatusMessageHeader);
			}
		}

		void SeaCargoInfoButton_Click(object sender, System.EventArgs e)
		{
			DepotCusOutturn depotOutturn = CurrentOutturn as DepotCusOutturn;
			ZString infoText = depotOutturn != null ? depotOutturn.SEIDetails : ZString.Empty;
			Globals.Message.ShowInformation(infoText.IsEmpty ? "No additional information is available" : infoText.ToString());
		}

		#endregion
	}
}
