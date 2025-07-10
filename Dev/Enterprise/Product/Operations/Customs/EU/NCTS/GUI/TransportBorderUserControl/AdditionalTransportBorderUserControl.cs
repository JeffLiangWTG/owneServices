using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class AdditionalTransportBorderUserControl : ZUserControl
	{
		public AdditionalTransportBorderUserControl()
		{
			InitializeComponent();
		}

		void MoreButton_OnClick(object sender, EventArgs e)
		{
			if (CurrentDataItem is NctsDepartureMovementHeader moveHeader)
			{
				ZFormModaliser.Show(new AdditionalTransportBorderForm(moveHeader), ParentForm);
			}
		}
	}
}
