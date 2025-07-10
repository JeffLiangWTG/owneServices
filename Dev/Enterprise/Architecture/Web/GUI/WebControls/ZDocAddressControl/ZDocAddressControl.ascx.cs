using System;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZDocAddressControl.
	/// </summary>
	[DefaultProperty("Caption"), ToolboxData("<{0}:ZDocAddressControl runat=server></{0}:ZDocAddressControl>")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class ZDocAddressControl : BaseUserControl, ISelfBindingWebControl
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponents();
		}

		void InitializeComponents()
		{
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.JobDocAddress";
			this.OrgFindBox.ModuleID = OrgModuleID;
			this.DocAddressLabel.Text = Caption;
			this.DocAddressLabel.CssClass = CaptionCssClass;
			this.AddressLabel.Text = AddressCaption;
			this.AddressOverrideLabel.Text = AddressCaption;
			this.ContactLabel.Text = ContactCaption;
			this.ContactOverrideLabel.Text = ContactCaption;
			this.ResidentialCheckBox.Visible = ShowResidentialAddressOnOverride;
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("This is used to distinguish different types of view")]
		enum DocAddressView
		{
			OrgAddressView,
			JobDocAddressView
		}

		#region ISelfBindingWebControl Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo;

		public void Bind(object dataSource)
		{
			if (IsBindable(dataSource))
			{
				if (!string.IsNullOrEmpty(BindTo))
				{
					ParentDataSource = dataSource as BusinessObject;
					OrgFindBox.BindTo = BindTo + ".OrganisationPK";

					OrgFindBox.BindToList = BindToOrgList;
					DocAddress = ZPropertyAccessor.Get(dataSource, BindTo) as JobDocAddress;
					DataBind();
				}
			}
			else
			{
				DocAddress = null;
			}
		}

		public bool IsBindable(object dataSource)
		{
			return (dataSource is BusinessObject) || (!string.IsNullOrEmpty(BindTo) && dataSource != null);
		}

		public void UnBind()
		{
			ParentDataSource = null;
			DocAddress = null;
			OrgFindBox.BindToList = null;

			DataBind();
		}

		#endregion

		#region DataBind

		public override void DataBind()
		{
			base.DataBind();
			if (OrgFindBox.IsBindable(ParentDataSource))
			{
				OrgFindBox.Bind(ParentDataSource);
			}
			BindChildControls(this);

			OverrideCheckBox_CheckedChanged(OverrideCheckBox, EventArgs.Empty);
			SetOrgAddressLabels();
		}

		void BindChildControls(Control parentControl)
		{
			foreach (Control control in parentControl.Controls)
			{
				if (control != OrgFindBox)
				{
					BindChildControls(control);

					ISelfBindingWebControl bindControl = control as ISelfBindingWebControl;
					if (bindControl != null && bindControl.IsBindable(DocAddress))
					{
						bindControl.Bind(DocAddress);
					}
				}
			}
		}

		#endregion

		#region DataBinding Properties

		#region OrgModuleID

		[AttributeProvider(ZGUIConstants.DesignerCategory)]
		public WebModuleID OrgModuleID
		{
			get { return OrgFindBox.ModuleID; }
			set { OrgFindBox.ModuleID = value; }
		}

		#endregion

		#region BindToOrgList

		[AttributeProvider(ZGUIConstants.DesignerCategory)]
		public string BindToOrgList
		{
			get { return OrgFindBox.BindToList; }
			set { OrgFindBox.BindToList = value; }
		}

		#endregion

		#endregion

		#region ParentDataSource

		internal BusinessObject ParentDataSource
		{
			get { return fParentDataSource; }
			set { fParentDataSource = value; }
		}
		BusinessObject fParentDataSource;

		#endregion

		#region DocAddress

		internal JobDocAddress DocAddress
		{
			get { return (fDocAddress == null || fDocAddress.IsDeleted) ? null : fDocAddress; }
			private set { fDocAddress = value; }
		}

		JobDocAddress fDocAddress;

		#endregion

		#region Appearance Properties

		#region Caption

		[Category("Appearance")]
		public string Caption
		{
			get { return DocAddressLabel.Text; }
			set { DocAddressLabel.Text = value; }
		}

		#endregion

		#region Caption CssStyle

		[Category("Appearance")]
		public string CaptionCssClass
		{
			get
			{
				return DocAddressLabel.CssClass;
			}
			set
			{
				DocAddressLabel.CssClass = value;
			}
		}

		#endregion CssStyle

		#region AddressCaption

		[Category("Appearance")]
		public string AddressCaption
		{
			get { return fAddressCaption; }
			set { fAddressCaption = value; }
		}
		string fAddressCaption = Res.GetString("cb1b5d13-5cf2-4cec-a3c4-96f529b8c5c4", "Address");

		#endregion

		#region ContactCaption

		[Category("Appearance")]
		public string ContactCaption
		{
			get { return fContactCaption; }
			set { fContactCaption = value; }
		}
		string fContactCaption = Res.GetString("54290a2b-6bd8-4ed3-b8d4-2cdc65b36771", "Contact");

		#endregion

		#region Details

		public enum OrganisationDetails
		{
			None,
			FullName,
			All
		}

		[Category(ZGUIConstants.DesignerCategory), DefaultValue(OrganisationDetails.All)]
		public OrganisationDetails Details
		{
			get { return fDetails; }
			set
			{
				if (fDetails != value)
				{
					fDetails = value;

					if (fDetails == OrganisationDetails.All)
					{
						OrgFullNameLabel.Visible = true;
						OrgAddressLabel.Visible = true;
						OrgPhoneLabel.Visible = true;
						OrgFaxLabel.Visible = true;
						OrgEmailLabel.Visible = true;
						OrgWebLabel.Visible = true;
						OrgWebLink.Visible = true;
					}
					else if (fDetails == OrganisationDetails.FullName)
					{
						OrgFullNameLabel.Visible = true;
						OrgAddressLabel.Visible = false;
						OrgPhoneLabel.Visible = false;
						OrgFaxLabel.Visible = false;
						OrgEmailLabel.Visible = false;
						OrgWebLabel.Visible = false;
						OrgWebLink.Visible = false;
					}
					else
					{
						OrgFullNameLabel.Visible = false;
						OrgAddressLabel.Visible = false;
						OrgPhoneLabel.Visible = false;
						OrgFaxLabel.Visible = false;
						OrgEmailLabel.Visible = false;
						OrgWebLabel.Visible = false;
						OrgWebLink.Visible = false;
					}
				}
			}
		}
		OrganisationDetails fDetails;

		#endregion

		#region ShowResidentialAddressOnOverride

		[Category("Behaviour"), DefaultValue(false)]
		public bool ShowResidentialAddressOnOverride
		{
			get
			{
				return showResidentialAddressOnOverride;
			}
			set
			{
				showResidentialAddressOnOverride = value;
			}
		}
		bool showResidentialAddressOnOverride;

		#endregion

		#endregion

		#region Set Org Address Labels

		protected void SetOrgAddressLabels()
		{
			bool isValidOrg = (Organisation != null);

			if (Details == OrganisationDetails.All)
			{
				SetOrgFullName(OrgFullNameLabel, isValidOrg);

				if (isValidOrg)
				{
					OrgAddressLabel.Text = OrgAddress;
					OrgPhoneLabel.Text = OrgPhone;
					OrgFaxLabel.Text = OrgFax;
					OrgEmailLabel.Text = OrgEmail;

					string webAddress = OrgWeb.Trim();
					bool isValidWeb = (webAddress != OrganisationMessages.NoWebFoundOnFile);

					if (isValidWeb)
					{
						OrgWebLabel.Text = Res.GetString("031bf622-371c-4265-92b4-217680b778ca", "Web:") + " ";
						OrgWebLink.Text = webAddress;
						OrgWebLink.Visible = true;
					}
					else
					{
						OrgWebLabel.Text = webAddress;
						OrgWebLink.Visible = false;
					}
				}
				else
				{
					OrgPhoneLabel.Text = OrganisationMessages.NoOrgIsSelected;
					OrgWebLink.Visible = false;
				}

				OrgAddressLabel.Visible = isValidOrg;
				OrgFaxLabel.Visible = isValidOrg;
				OrgEmailLabel.Visible = isValidOrg;
				OrgWebLabel.Visible = isValidOrg;
				OrgWebLink.Visible = isValidOrg;
			}
			else if (Details == OrganisationDetails.FullName)
			{
				SetOrgFullName(OrgFullNameLabel, isValidOrg);
			}

			OrgPhoneLabel.Enabled = isValidOrg;
		}

		void SetOrgFullName(Label label, bool isValidOrg)
		{
			label.Text = isValidOrg ? OrgFullName : OrganisationMessages.NoOrgIsSelected;

			label.Enabled = isValidOrg;
			label.Font.Bold = isValidOrg;
		}

		#region Org Details

		protected IOrgHeader Organisation
		{
			get { return DocAddress != null ? DocAddress.Organisation : null; }
		}

		string OrgFullName
		{
			get { return OrgFullNameCore.IsEmpty ? OrganisationMessages.NoFullNameFoundOnFile : (string)OrgFullNameCore; }
		}

		string OrgAddress
		{
			get
			{
				StringBuilder result = new StringBuilder();

				if (!OrgAddress1Core.IsEmpty)
				{
					result.Append(OrgAddress1Core);
				}

				if (!OrgAddress2Core.IsEmpty)
				{
					result.Append("<br />" + OrgAddress2Core);
				}

				if (!OrgUNLOCOCore.IsEmpty)
				{
					result.Append("<br />" + OrgUNLOCOCore);
				}

				return (result.Length == 0) ? OrganisationMessages.NoAddressFoundOnFile : result.ToString();
			}
		}

		string OrgPhone
		{
			get
			{
				StringBuilder result = new StringBuilder();

				if (!OrgPhoneCore.IsEmpty)
				{
					result.Append(Res.GetString("ff72be45-7fb9-4a0b-9723-02e2200ad8bb", "Phone: {0}", OrgPhoneCore));
				}
				if (!OrgMobileCore.IsEmpty)
				{
					if (result.Length > 0)
					{
						result.Append(Res.GetString("8b5fec59-1be4-4a4b-befb-c5a71b5bad5b", ", (Mobile) {0}", OrgMobileCore));
					}
					else
					{
						result.Append(Res.GetString("ff72be45-7fb9-4a0b-9723-02e2200ad8bb", "Phone: {0}", OrgMobileCore));
					}
				}

				return (result.Length == 0) ? OrganisationMessages.NoPhoneFoundOnFile : result.ToString();
			}
		}

		string OrgFax
		{
			get { return OrgFaxCore.IsEmpty ? OrganisationMessages.NoFaxFoundOnFile : Res.GetString("7a0624c0-03d1-4672-bcc2-a44bcdeebbcb", "Fax: {0}", OrgFaxCore); }
		}

		string OrgEmail
		{
			get { return OrgEmailCore.IsEmpty ? OrganisationMessages.NoEmailFoundOnFile : Res.GetString("671de7f6-c69b-47ac-987f-4147cf741212", "Email: {0}", OrgEmailCore); }
		}

		string OrgWeb
		{
			get { return OrgWebCore.IsEmpty ? OrganisationMessages.NoWebFoundOnFile : (string)OrgWebCore; }
		}

		#region Implementation

		#region Organisation Messages

		internal abstract class OrganisationMessages
		{
			public static string NoFullNameFoundOnFile
			{
				get { return Res.GetString("5e4f97cc-8686-457a-80ee-60691b857d83", "*NO NAME FOUND*"); }
			}
			public static string NoAddressFoundOnFile
			{
				get { return Res.GetString("2dbe5291-b271-445b-aa31-7b671df8b0f3", "*NO ADDRESS FOUND*"); }
			}
			public static string NoPhoneFoundOnFile
			{
				get { return Res.GetString("3759b9b4-d4b0-410b-8781-67f2211c0d56", "Phone: *NOT FOUND*"); }
			}
			public static string NoFaxFoundOnFile
			{
				get { return Res.GetString("f4fe8720-db21-4f84-9a61-bea892ac881c", "Fax: *NOT FOUND*"); }
			}
			public static string NoEmailFoundOnFile
			{
				get { return Res.GetString("1584f315-0b6c-4b83-87bc-31ca8c292442", "Email: *NOT FOUND*"); }
			}
			public static string NoWebFoundOnFile
			{
				get { return Res.GetString("28159fed-63d1-4f8c-9aa7-96e389f93d26", "Web: *NOT FOUND*"); }
			}
			public static string NoOrgIsSelected
			{
				get { return Res.GetString("7c23febd-d936-4f00-8b62-c5f6bc40cc09", "* NO ORGANIZATION IS SELECTED"); }
			}
		}

		#endregion

		protected ZString OrgFullNameCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_CompanyName : ZString.Empty; }
		}

		protected ZString OrgAddress1Core
		{
			get { return (DocAddress != null) ? DocAddress.E2_Address1 : ZString.Empty; }
		}

		protected ZString OrgAddress2Core
		{
			get { return (DocAddress != null) ? DocAddress.E2_Address2 : ZString.Empty; }
		}

		protected ZString OrgUNLOCOCore
		{
			get { return (DocAddress != null && DocAddress.Address != null) ? DocAddress.Address.OA_RL_NKRelatedPortCode : ZString.Empty; }
		}

		protected ZString OrgEmailCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Email : ZString.Empty; }
		}

		protected ZString OrgFaxCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Fax : ZString.Empty; }
		}

		protected ZString OrgMobileCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Mobile : ZString.Empty; }
		}

		protected ZString OrgPhoneCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Phone : ZString.Empty; }
		}

		protected ZString OrgWebCore
		{
			get { return (DocAddress != null && Organisation != null) ? Organisation.Web : ZString.Empty; }
		}

		#endregion

		#endregion

		#endregion

		#region OverrideCheckBox checked handler

		protected internal void OverrideCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (((ZCheckBox)sender).Checked)
			{
				DocAddressMultiView.ActiveViewIndex = (int)DocAddressView.JobDocAddressView;
			}
			else
			{
				DocAddressMultiView.ActiveViewIndex = (int)DocAddressView.OrgAddressView;
			}
		}

		#endregion
	}
}
