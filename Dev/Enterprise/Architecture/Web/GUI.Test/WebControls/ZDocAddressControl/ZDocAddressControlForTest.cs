using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDocAddressControlForTest : ZDocAddressControl
	{
		public ZDocAddressControlForTest() : base()
		{
			// Instantiate Controls
			this.DocAddressLabel = new Label();
			this.OverrideCheckBox = new ZCheckBox();
			this.DocAddressMultiView = new MultiView();
			this.OrgFindBox = new ZGuidFindBox();
			this.Controls.Add(OrgFindBox);
			this.AddressLabel = new Label();
			this.AddressDropEdit = new ZGuidDropDownList();
			this.OrgFullNameLabel = new Label();
			this.OrgAddressLabel = new Label();
			this.ContactLabel = new Label();
			this.OrgPhoneLabel = new Label();
			this.OrgFaxLabel = new Label();
			this.OrgEmailLabel = new Label();
			this.OrgWebLabel = new Label();
			this.OrgWebLink = new HyperLink();
			this.AddressOverrideLabel = new Label();
			this.ContactOverrideLabel = new Label();
			this.ResidentialCheckBox = new ZCheckBox();
		}

		#region Test Properties

		public void OnInitForTesting(EventArgs e) => OnInit(e);
		public ZCheckBox ResidentialCheckBoxForTesting => this.ResidentialCheckBox;
		public ZCheckBox OverrideCheckBoxForTesting => this.OverrideCheckBox;
		public Label DocAddressLabelForTesting => DocAddressLabel;
		public Label AddressLabelForTesting => AddressLabel;
		public Label AddressOverrideLabelForTesting => AddressOverrideLabel;
		public Label ContactLabelForTesting => ContactLabel;
		public Label ContactOverrideLabelForTesting => ContactOverrideLabel;
		public ZGuidFindBox OrgFindBoxForTesting => OrgFindBox;
		public Label OrgFullNameLabelForTesting => OrgFullNameLabel;
		public Label OrgAddressLabelForTesting => OrgAddressLabel;
		public Label OrgPhoneLabelForTesting => OrgPhoneLabel;
		public Label OrgFaxLabelForTesting => OrgFaxLabel;
		public Label OrgEmailLabelForTesting => OrgEmailLabel;
		public Label OrgWebLabelForTesting => OrgWebLabel;
		public HyperLink OrgWebLinkForTesting => OrgWebLink;

		#endregion
	}
}
