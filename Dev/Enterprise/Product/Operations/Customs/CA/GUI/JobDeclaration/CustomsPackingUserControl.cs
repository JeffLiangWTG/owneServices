using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CustomsPackingUserControl : BaseCustomsPackingUserControl
	{
		public CustomsPackingUserControl()
		{
			InitializeComponent();
		}

		public new IInvoicesProvider CurrentDataItem
		{
			get { return (IInvoicesProvider)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			declarationValueChangedAnnouncer_OnValueChanged(this, null);
			declarationValueChangedAnnouncer = CurrentDataItem?.GetValueChangedAnnouncer();
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged += declarationValueChangedAnnouncer_OnValueChanged;
			}
		}
		IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged -= declarationValueChangedAnnouncer_OnValueChanged;
				declarationValueChangedAnnouncer.Dispose();
			}
		}

		void declarationValueChangedAnnouncer_OnValueChanged(object sender, EventArgs e)
		{
			HouseBillsGroupBox.Text = Res.GetString("eb0ba356-89b4-4c93-9b19-3edf0aa3d55e", "Bills");
			PackingDetailsGrid.SetColumnCaption(Package.Schema.CW_HouseBill, Res.GetString("890fe997-57a2-43be-b477-d65e939b0ad1", "Linked Bill(Lowest Bill)"));
			releaseStatusesGrid.SetColumnCaption(ReleaseStatus.Schema.RL_Bill, Res.GetString("6a19d7ac-4367-417c-bd73-ae1b87bbca29", "Bill"));
		}
	}
}
