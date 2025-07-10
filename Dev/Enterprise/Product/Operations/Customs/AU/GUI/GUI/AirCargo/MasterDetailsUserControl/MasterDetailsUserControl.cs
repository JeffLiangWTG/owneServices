using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class MasterDetailsUserControl : ZUserControl
	{
		public MasterDetailsUserControl()
		{
			InitializeComponent();
		}

		protected CusHAWBBase HAWB
		{
			get { return CurrentDataItem as CusHAWBBase; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			ChangeControlsVisibility();
		}

		void ChangeControlsVisibility()
		{
			if (HAWB != null && HAWB.MAWB != null)
			{
				HAWB.MAWB.RefreshBinding();
				AltPartShipModelCheckBox.Visible = HAWB.MAWB.IsAltPartShipModelActive;
			}
		}
	}
}
