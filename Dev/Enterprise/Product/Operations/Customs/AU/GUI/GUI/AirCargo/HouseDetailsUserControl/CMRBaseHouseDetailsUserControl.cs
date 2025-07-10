using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CMRBaseHouseDetailsUserControl : AirCargoHAWBProviderContainerControl
	{
		public CMRBaseHouseDetailsUserControl()
		{
			InitializeComponent();
		}

		#region Binding

		public ZString UserFriendlyStatuses
		{
			get { return HAWB == null ? ZString.Empty : HAWB.CMRCargoStatus.UserFriendlyStatuses; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			CusHAWB parent = CurrentDataItem as CusHAWB;
			if (parent != null)
			{
				HAWB = parent;
			}
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
				HAWB.RefreshBinding();
				conRefTextBox.Visible = HAWB.MAWB.IsAltPartShipModelActive;
				ConRefLabel.Visible = HAWB.MAWB.IsAltPartShipModelActive;
			}
		}

		#endregion

		#region Events

		internal void DetailsButton_Click(object sender, EventArgs e)
		{
			ZString customsInfo = UserFriendlyStatuses;
			if (!customsInfo.IsEmpty)
			{
				Globals.Message.ShowInformation(customsInfo, UserFriendlyStatusMessages.StatusMessageHeader);
			}
			else
			{
				Globals.Message.ShowInformation(UserFriendlyStatusMessages.StatusNotAvailable, UserFriendlyStatusMessages.StatusMessageHeader);
			}
		}

		internal void PopulateABNButton_Click(object sender, EventArgs e)
		{
			if (HAWB != null)
			{
				HAWB.PopulateResponsiblePartyFromConsigneeABNorCCID();
			}
		}

		#endregion

		#region SAC

		void SACCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (SACCheckBox.Focused && SACCheckBox.Checked &&
				SACCheckBox.DataBindings["Checked"].BindingManagerBase.Position >= 0)
			{
				ICusHAWBBase hAWB = SACCheckBox.DataBindings["Checked"].BindingManagerBase.GetCurrent() as ICusHAWBBase;

				if (hAWB != null)
				{
#if DEBUG
					if (Globals.IsTest && !ShowSACForm)
					{
						return;
					}
#endif
					ZFormModaliser.ShowDialogAndDispose(new CMRSACDialogBox(new SACDialogBizo(hAWB)));
				}
			}
		}

#if DEBUG
		internal bool ShowSACForm;
#endif

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
