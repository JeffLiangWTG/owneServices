using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoDeclarationUserControl : ZUserControl
	{
		public SeaCargoDeclarationUserControl()
		{
			InitializeComponent();
			SeaCargoBoundTabControl.SelectedIndexChanged += new EventHandler(SeaCargoBoundTabControl_SelectedIndexChanged);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is CusSCAHouse houseBill && houseBill != null)
			{
				houseBill.MakeOceanBillAnEditableChild();
				SeaCargoBoundTabControl.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.AU.CusSCAHouseCusUnderbondPluginController)?.BusinessEntity?.Add(houseBill);
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupPlugins();
		}

		void SetupPlugins()
		{
			SeaCargoBoundTabControl.PlugIns.Add(ZArchitecture.Modules.ControllerIDs.Customs.AU.CusSCAHouseCusUnderbondPluginController);
		}

		void SeaCargoBoundTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			ZTabPage tabPage = SeaCargoBoundTabControl.SelectedTab;
			if (tabPage != null)
			{
				foreach (Control control in tabPage.Controls)
				{
					CusUnderbondUserControl underbondControl = control as CusUnderbondUserControl;
					if (underbondControl != null)
					{
						underbondControl.OutturnDisabled = true;
					}
				}
			}
		}
	}
}
