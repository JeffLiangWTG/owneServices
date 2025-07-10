using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.PlugIns
{
	public partial class RNSUserControl : ZUserControl
	{
		public RNSUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			RNSMessaging.RN_EntryDateInfo.ValueChanged -= EntryDataChanged;
			base.OnAfterFirstBinding(e);
			RNSMessaging.RN_EntryDateInfo.ValueChanged += EntryDataChanged;
			EntryDateChanged();
		}

		public RNSUserControl(RNSMessagingBO rnsMessaging) : this()
		{
			RNSMessaging = rnsMessaging;
			this.LatestNoticeProcessingDateEdit.SetReadOnly(true);
		}

		#region Layout

		void EntryDataChanged(object sender, EventArgs e)
		{
			EntryDateChanged();
		}

		void EntryDateChanged()
		{
			this.EntryDateEdit.Visible = !RNSMessaging.RN_EntryDate.IsEmpty;
			this.LoginUserTextBox.Visible = !RNSMessaging.RN_LoginUser.IsEmpty;
		}

		#endregion

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(RNSMessaging, "");
		}

		public RNSMessagingBO RNSMessaging { get; set; }
	}
}
