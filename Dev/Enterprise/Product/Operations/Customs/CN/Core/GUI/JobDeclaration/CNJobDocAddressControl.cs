using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CNJobDocAddressControl : ZUserControl
	{
		public CNJobDocAddressControl()
		{
			InitializeComponent();
			UpdateControlsLayout();

			OrganisationPanel.AllowOverlap(OverrideAddressCheckbox);
			DetailsTabControl.AllowOverlap(OverrideAddressCheckbox);
		}

		protected CNJobDocAddress DocAddress
		{
			get { return (CNJobDocAddress)base.CurrentDataItem; }
		}

		public override ResourceStringData CaptionResourceString
		{
			get { return MainGroupBox.CaptionResourceString; }
			set { MainGroupBox.CaptionResourceString = value; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToOrganisations))]
		[ZArchitecture.GUI.Testing.ExcludeFromBindToAttributesTest]
		public string BindToOrganisations
		{
			get { return OrganisationFindBox.BindToList; }
			set { OrganisationFindBox.BindToList = value; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			string orgBindingMember = (DataSource == null) ? "" : new KBindingMemberInfo(dataMember, "OrganisationPK");
			OrganisationFindBox.SetDataBinding(DataSource, orgBindingMember);

			ContactDropEdit.BindToList = "Organisation+ContactsActive";
			AddressDropEdit.BindToList = "Organisation+Address_List";
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (DocAddress != null)
			{
				DocAddress.E2_AddressOverrideInfo.ValueChanged -= UpdateControlsLayout;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (DocAddress != null)
			{
				DocAddress.E2_AddressOverrideInfo.ValueChanged += UpdateControlsLayout;
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				UpdateControlsLayout();
			}
		}

		void UpdateControlsLayout(object sender, EventArgs e)
		{
			UpdateControlsLayout();
		}

		public void UpdateControlsLayout()
		{
			var docAddress = DocAddress;
			if (docAddress != null && !docAddress.IsDeleted)
			{
				AddressTabPage.TabVisible = !docAddress.E2_AddressOverride;
				OverrideAddressTabPage.TabVisible = docAddress.E2_AddressOverride;
				OrganisationPanel.Visible = !docAddress.E2_AddressOverride;

				SocialCreditCodeTextBox.Visible = docAddress.RequiresDomesticOrg;
				CustomsCodeTextBox.Visible = docAddress.RequiresDomesticOrg;
				CIQCodeTextBox.Visible = docAddress.RequiresDomesticOrg;

				OverseasPartyCodeTypeDropEdit.Visible = OverseasPartyCodeTextBox.Visible = docAddress.RequiresOverseasOrg;
			}
		}
	}
}
