using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCusUnderbondUserControl : CusUnderbondUserControl
	{
		public AUCusUnderbondUserControl()
		{
			InitializeComponent();

			UnderbondsGrid.ControlAdded += UnderbondsGrid_ControlAdded;
		}

		protected override CusUnderbondDetailsUserControl GetCusUnderbondDetailsUserControl()
		{
			return new AUCusUnderbondDetailsUserControl();
		}

		void UnderbondsGrid_ControlAdded(object sender, System.Windows.Forms.ControlEventArgs e)
		{
			if (e.Control is ZGridFindBox findBoxControl && findBoxControl.Name == "C4_UnderbondBySeaVessel")
			{
				findBoxControl.PopupSelected += AUCusUnderbondUserControl_PopupSelected;
			}
		}

		void AUCusUnderbondUserControl_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var bizos = e.SelectedBusinessObjects;
			if (bizos.Length == 1 && bizos[0] is MasterFiles.Business.RefVessel vessel)
			{
				((CusUnderbond)CurrentUnderbond).SetUnderbondBySeaVessel(vessel);
			}
		}
	}
}
