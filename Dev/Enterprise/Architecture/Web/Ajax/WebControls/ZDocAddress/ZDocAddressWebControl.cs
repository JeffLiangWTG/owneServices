using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:ZDocAddressWebControl runat=server></{0}:ZDocAddressWebControl>")]
	public class ZDocAddressWebControl : CompositeControl, ISelfBindingPostbackWebControl, IWebServiceMethodsCaller
	{
		#region Constructors

		public ZDocAddressWebControl()
			: base()
		{ }

		#endregion

		#region Properties

		[Category("Behaviour"), Browsable(true)]
		public bool AutoPostBackOnPostalCodeChanged
		{
			get { return autoPostBackOnPostalCodeChanged; }
			set { autoPostBackOnPostalCodeChanged = value; }
		}
		bool autoPostBackOnPostalCodeChanged;

		public new ZPage Page
		{
			get { return (ZPage)base.Page; }
			set { base.Page = value; }
		}

		public BusinessObjectFactory Factory
		{
			get { return Page.Factory; }
		}

		[Category("Appearance"), DefaultValue("false"), Browsable(true)]
		public bool IsConsignor
		{
			get { return isConsignor; }
			set { isConsignor = value; }
		}
		bool isConsignor;

		[Category("Appearance"), DefaultValue("false"), Browsable(true)]
		public bool IsConsignee
		{
			get { return isConsignee; }
			set { isConsignee = value; }
		}
		bool isConsignee;

		[Category("Appearance"), DefaultValue("false"), Browsable(true)]
		public bool HorizonatalLayout
		{
			get { return horizonatalLayout; }
			set { horizonatalLayout = value; }
		}
		bool horizonatalLayout;

		[Category("Appearance"), DefaultValue("Reg. No"), Browsable(true)]
		public string RegGovNumCaption
		{
			get { return regGovNumCaption; }
			set { regGovNumCaption = value; }
		}
		string regGovNumCaption = Res.GetString("946bf1c0-c7d6-4ad2-bceb-8427a6ffc300", "Reg. No");

		[Category("Appearance"), DefaultValue("City"), Browsable(true)]
		public string CityCaption
		{
			get { return cityCaption; }
			set { cityCaption = value; }
		}
		string cityCaption = Res.GetString("e6e7455b-bb29-4049-b521-7530f5b1b318", "City");

		[Category("Appearance"), DefaultValue("Postal Code"), Browsable(true)]
		public string PostalCodeCaption
		{
			get { return postalCodeCaption; }
			set { postalCodeCaption = value; }
		}
		string postalCodeCaption = Res.GetString("4894ff5d-a39f-4535-a054-b8cec9167bea", "Postal Code");

		[Category("Appearance"), DefaultValue("State"), Browsable(true)]
		public string StateCaption
		{
			get { return stateCaption; }
			set { stateCaption = value; }
		}
		string stateCaption = Res.GetString("4a6fe80a-6aa9-4241-adf4-818754ad8d76", "State");

		[Category("Appearance"), DefaultValue("Country"), Browsable(true)]
		public string CountryCaption
		{
			get { return countryCaption; }
			set { countryCaption = value; }
		}
		string countryCaption = Res.GetString("db5df060-b542-4205-9e1b-8d0609bb4306", "Country/Region");

		[Category("Appearance"), DefaultValue("Residential Address"), Browsable(true)]
		public string ResidentialCheckBoxCaption
		{
			get { return residentialCheckBoxCaption; }
			set { residentialCheckBoxCaption = value; }
		}
		string residentialCheckBoxCaption = Res.GetString("88de5c9d-6f1a-480c-88b3-ac24642e4d55", "Residential Address");

		[Category("Appearance"), DefaultValue("Phone"), Browsable(true)]
		public string PhoneCaption
		{
			get { return phoneCaption; }
			set { phoneCaption = value; }
		}
		string phoneCaption = Res.GetString("74352712-3ae8-489d-a8d1-851d332e1163", "Phone");

		[Category("Appearance"), DefaultValue("Fax"), Browsable(true)]
		public string FaxCaption
		{
			get { return faxCaption; }
			set { faxCaption = value; }
		}
		string faxCaption = Res.GetString("906c3c8e-f2e8-4ee1-8923-918148c8edd7", "Fax");

		[Category("Appearance"), DefaultValue("Email"), Browsable(true)]
		public string EmailCaption
		{
			get { return emailCaption; }
			set { emailCaption = value; }
		}
		string emailCaption = Res.GetString("122f23e2-d398-4b0b-aa16-eab0777f08b9", "Email");

		[Category("Appearance"), DefaultValue("AddressTable"), Browsable(true)]
		public override string CssClass
		{
			get { return cssClass; }
			set { cssClass = value; }
		}
		string cssClass = "AddressTable";

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string Caption
		{
			get { return caption; }
			set { caption = value; }
		}
		string caption = string.Empty;

		public string TryToGetControlCaptionFromAddressType()
		{
			return (WebJobDocAddress != null) ? WebJobDocAddress.AddressTypeDescription : ZString.Empty;
		}

		[Category("Appearance"), DefaultValue("SectionTitle"), Browsable(true)]
		public string CaptionCssClass
		{
			get { return captionCssClass; }
			set { captionCssClass = value; }
		}
		string captionCssClass = "SectionTitle";

		[Category("Appearance"), DefaultValue("Label"), Browsable(true)]
		public string LabelCssClass
		{
			get { return labelCssClass; }
			set { labelCssClass = value; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		string labelCssClass = "Label";

		[Category("Appearance"), DefaultValue("InsideLabel"), Browsable(true)]
		public string InsideLabelCssClass
		{
			get { return insideLabelCssClass; }
			set { insideLabelCssClass = value; }
		}
		string insideLabelCssClass = "InsideLabel";

		[Category("Appearance"), DefaultValue("Company"), Browsable(true)]
		public string OrganisationCaption
		{
			get { return organisationCaption; }
			set { organisationCaption = value; }
		}
		string organisationCaption = Res.GetString("e766de42-629e-4ec9-9f94-2707e82091dc", "Company");

		[Category("Appearance"), DefaultValue("Address"), Browsable(true)]
		public string AddressCaption
		{
			get { return addressCaption; }
			set { addressCaption = value; }
		}
		string addressCaption = Res.GetString("b93ad57d-2892-47f4-8fa7-89101ccac13c", "Address");

		[Category("Appearance"), DefaultValue("Contact"), Browsable(true)]
		public string ContactCaption
		{
			get { return contactCaption; }
			set { contactCaption = value; }
		}
		string contactCaption = Res.GetString("a3157f2e-46e4-431d-8ee1-f3a0038e36e8", "Contact");

		[Browsable(true)]
		public NewOrgRelationTypes NewOrgRelationType
		{
			get { return newOrgRelationType; }
			set { newOrgRelationType = value; }
		}
		NewOrgRelationTypes newOrgRelationType = NewOrgRelationTypes.Unknown;

		[Browsable(true)]
		public WebModuleID OrgModuleID
		{
			get
			{
				if (!moduleIdChanged)
				{
					if (IsConsignee && IsConsignor || !(IsConsignee || IsConsignor))
					{
						orgModuleID = WebModuleIDs.OrganisationTracking;
					}
					else
					{
						if (IsConsignee)
						{
							orgModuleID = WebModuleIDs.OrgConsigneeTracking;
						}
						else if (isConsignor)
						{
							orgModuleID = WebModuleIDs.OrgConsignorTracking;
						}
					}
				}
				return orgModuleID;
			}
			set
			{
				orgModuleID = value;
				moduleIdChanged = true;
			}
		}
		WebModuleID orgModuleID = WebModuleIDs.NotAssigned;
		bool moduleIdChanged;

		[Category("Appearance"), DefaultValue("Save information in database"), Browsable(true)]
		public string SaveCheckboxCaption
		{
			get { return saveCheckboxCaption; }
			set { saveCheckboxCaption = value; }
		}
		string saveCheckboxCaption = Res.GetString("464561cc-fe50-45f5-bede-90355b5b43eb", "Save information in database");

		[Category("Behaviour"), DefaultValue(false), Browsable(true)]
		public bool ResidentialCheckboxVisible
		{
			get { return residentialCheckboxVisible; }
			set { residentialCheckboxVisible = value; }
		}
		bool residentialCheckboxVisible;

		[Category("Behaviour"), DefaultValue(false), Browsable(true)]
		public bool GovermentRegNoVisible
		{
			get { return govermentRegNoVisible; }
			set { govermentRegNoVisible = value; }
		}
		bool govermentRegNoVisible;

		[Category("Behaviour"), DefaultValue(true), Browsable(true)]
		public bool SaveCheckboxVisible
		{
			get { return saveCheckboxVisible; }
			set { saveCheckboxVisible = value; }
		}
		bool saveCheckboxVisible = true;

		[Category("Behaviour"), DefaultValue(false), Browsable(true)]
		public bool IsOptional
		{
			get { return isOptional; }
			set { isOptional = value; }
		}
		bool isOptional;

		[Category("Behaviour"), DefaultValue(""), Browsable(true)]
		public string DependentPortControl
		{
			get { return dependentPortControl; }
			set { dependentPortControl = value; }
		}
		string dependentPortControl = "";

		[Category("Behaviour"), DefaultValue(true), Browsable(true)]
		public bool AllowEdit
		{
			get { return allowEdit; }
			set { allowEdit = value; }
		}
		bool allowEdit = true;

		#endregion

		#region Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		Panel PutInNoWrapPanel(WebControl control)
		{
			Panel result = new Panel();
			result.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
			result.Controls.Add(control);
			return result;
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			UpdatePanel controlUpdatePanel = new UpdatePanel();
			controlUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

			Table controlTable = new Table();
			controlTable.CssClass = CssClass;

			#region Caption

			CaptionLabel.CssClass = CaptionCssClass;
			CaptionLabel.Text = Caption;

			TableCell captionCell = new TableCell();
			captionCell.Controls.Add(PutInNoWrapPanel(CaptionLabel));

			#endregion

			#region Organisation

			OrganisationLabel.Text = OrganisationCaption + ":";

			TableCell orgCaptionCell = new TableCell();
			orgCaptionCell.CssClass = LabelCssClass;
			orgCaptionCell.Controls.Add(PutInNoWrapPanel(OrganisationLabel));

			TableCell orgCell = new TableCell();
			OrganisationTextBox.CssClass = "AddressTableOrg";
			orgCell.Controls.Add(OrganisationTextBox);

			#endregion

			#region Gov. Reg. Num.

			GovRegNumLabel.Text = RegGovNumCaption + ":";

			TableCell govRegCaptionCell = new TableCell();
			govRegCaptionCell.CssClass = LabelCssClass;
			govRegCaptionCell.Controls.Add(PutInNoWrapPanel(GovRegNumLabel));

			GovRegNumTypeDropDownList.CssClass = "AddressTableRegNumList";

			TableCell govRegListCell = new TableCell();
			govRegListCell.Controls.Add(GovRegNumTypeDropDownList);

			GovRegNumTextBox.CssClass = "AddressTableRegNum";
			SSNTextBox.CssClass = "AddressTableRegNum";
			SSNDOBBox.CssClass = "AddressTableRegNum";
			TableCell govRegCell = new TableCell();
			govRegCell.Controls.Add(GovRegNumTextBox);
			govRegCell.Controls.Add(SSNTextBox);

			TableCell ssnDOBCaptionCell = new TableCell();
			ssnDOBCaptionCell.CssClass = LabelCssClass;
			ssnDOBCaptionCell.Controls.Add(PutInNoWrapPanel(SSNDOBLabel));

			TableCell ssnDOBCell = new TableCell();
			ssnDOBCell.Controls.Add(SSNDOBBox);

			#endregion

			#region Address

			AddressLabel.Text = AddressCaption + ":";

			TableCell addrCaptionCell = new TableCell();
			addrCaptionCell.CssClass = LabelCssClass;
			addrCaptionCell.Controls.Add(PutInNoWrapPanel(AddressLabel));

			AddressTextBox.CssClass = "AddressTableAddress";

			TableCell addrCell = new TableCell();
			addrCell.Controls.Add(AddressTextBox);

			Address2TextBox.CssClass = "AddressTableAddress2";

			TableCell addr2CaptionCell = new TableCell();
			TableCell addr2Cell = new TableCell();
			addr2Cell.Controls.Add(Address2TextBox);

			#endregion

			#region Postal Code & City

			PostalCodeLabel.Text = PostalCodeCaption + ":";

			TableCell postalCodeCaptionCell = new TableCell();
			postalCodeCaptionCell.CssClass = LabelCssClass;
			postalCodeCaptionCell.Controls.Add(PutInNoWrapPanel(PostalCodeLabel));

			PostalCodeTextBox.CssClass = "AddressTablePostalCode";

			TableCell postalCodeCell = new TableCell();
			postalCodeCell.Controls.Add(PostalCodeTextBox);

			CityLabel.Text = CityCaption + ":";

			TableCell cityCaptionCell = new TableCell();
			cityCaptionCell.CssClass = InsideLabelCssClass;
			cityCaptionCell.Style.Add(HtmlTextWriterStyle.PaddingLeft, Unit.Pixel(10).ToString());
			cityCaptionCell.Controls.Add(PutInNoWrapPanel(CityLabel));

			CityTextBox.CssClass = "AddressTableCity";

			TableCell cityCell = new TableCell();
			cityCell.Controls.Add(CityTextBox);

			#endregion

			#region State & Country

			StateLabel.Text = StateCaption + ":";

			TableCell stateCaptionCell = new TableCell();
			stateCaptionCell.CssClass = LabelCssClass;
			stateCaptionCell.Controls.Add(PutInNoWrapPanel(StateLabel));

			StateTextBox.CssClass = "AddressTableState";

			TableCell stateCell = new TableCell();
			stateCell.Controls.Add(StateTextBox);

			CountryLabel.Text = CountryCaption + ":";

			TableCell countryCaptionCell = new TableCell();
			countryCaptionCell.CssClass = InsideLabelCssClass;
			countryCaptionCell.Style.Add(HtmlTextWriterStyle.PaddingLeft, Unit.Pixel(10).ToString());
			countryCaptionCell.Controls.Add(PutInNoWrapPanel(CountryLabel));

			CountryTextBox.CssClass = "AddressTableCountry";

			TableCell countryCell = new TableCell();
			countryCell.Controls.Add(CountryTextBox);

			#endregion

			#region Residential

			ResidentialCheckBox.Text = ResidentialCheckBoxCaption;

			TableCell residentialCell = new TableCell();
			residentialCell.Controls.Add(PutInNoWrapPanel(ResidentialCheckBox));

			#endregion

			#region Contact

			ContactLabel.Text = ContactCaption + ":";

			TableCell contactCaptionCell = new TableCell();
			contactCaptionCell.CssClass = LabelCssClass;
			contactCaptionCell.Controls.Add(PutInNoWrapPanel(ContactLabel));

			ContactTextBox.CssClass = "AddressTableContact";

			TableCell contactCell = new TableCell();
			contactCell.Controls.Add(ContactTextBox);

			#endregion

			#region Phone & Fax

			PhoneLabel.Text = PhoneCaption + ":";

			TableCell phoneCaptionCell = new TableCell();
			phoneCaptionCell.CssClass = LabelCssClass;
			phoneCaptionCell.Controls.Add(PutInNoWrapPanel(PhoneLabel));

			PhoneTextBox.CssClass = "AddressTablePhone";

			TableCell phoneCell = new TableCell();
			phoneCell.Controls.Add(PhoneTextBox);

			FaxLabel.Text = FaxCaption + ":";

			TableCell faxCaptionCell = new TableCell();
			faxCaptionCell.CssClass = InsideLabelCssClass;
			faxCaptionCell.Style.Add(HtmlTextWriterStyle.PaddingLeft, Unit.Pixel(10).ToString());
			faxCaptionCell.Controls.Add(PutInNoWrapPanel(FaxLabel));

			FaxTextBox.CssClass = "AddressTableFax";

			TableCell faxCell = new TableCell();
			faxCell.Controls.Add(FaxTextBox);

			#endregion

			#region Email

			EmailLabel.Text = EmailCaption + ":";

			TableCell emailCaptionCell = new TableCell();
			emailCaptionCell.CssClass = LabelCssClass;
			emailCaptionCell.Controls.Add(PutInNoWrapPanel(EmailLabel));

			EmailTextBox.CssClass = "AddressTableEmail";

			TableCell emailCell = new TableCell();
			emailCell.Controls.Add(EmailTextBox);

			#endregion

			#region Save As

			SaveAsNewCheckBox.Text = SaveCheckboxCaption;

			TableCell saveAsNewCell = new TableCell();
			if (ShowSaveCheckBox)
			{
				saveAsNewCell.Controls.Add(PutInNoWrapPanel(SaveAsNewCheckBox));
			}

			#endregion

			#region Layout

			CaptionRow = new TableRow();
			CaptionRow.Cells.Add(captionCell);
			controlTable.Rows.Add(CaptionRow);
			CaptionRow.Visible = !string.IsNullOrEmpty(CaptionLabel.Text);

			if (HorizonatalLayout)
			{
				#region Horizontal Layout

				captionCell.ColumnSpan = 8;

				TableRow rowOne = new TableRow();
				TableRow rowTwo = new TableRow();
				TableRow rowThree = new TableRow();

				SSNDOBRow.Cells.Add(new TableCell() { ColumnSpan = 5 });
				SSNDOBRow.Cells.Add(ssnDOBCaptionCell);
				SSNDOBRow.Cells.Add(ssnDOBCell);

				rowOne.Cells.Add(orgCaptionCell);
				rowTwo.Cells.Add(addrCaptionCell);
				rowThree.Cells.Add(postalCodeCaptionCell);

				orgCell.ColumnSpan = 3;
				addrCell.ColumnSpan = 3;
				rowOne.Cells.Add(orgCell);
				rowTwo.Cells.Add(addrCell);
				rowThree.Cells.Add(postalCodeCell);
				rowThree.Cells.Add(cityCaptionCell);
				rowThree.Cells.Add(cityCell);

				rowOne.Cells.Add(govRegCaptionCell);
				rowTwo.Cells.Add(addr2CaptionCell);
				rowThree.Cells.Add(stateCaptionCell);

				addr2Cell.ColumnSpan = 3;
				govRegCell.ColumnSpan = 2;
				rowOne.Cells.Add(govRegListCell);
				rowOne.Cells.Add(govRegCell);
				rowTwo.Cells.Add(addr2Cell);
				rowThree.Cells.Add(stateCell);
				rowThree.Cells.Add(countryCaptionCell);
				rowThree.Cells.Add(countryCell);

				controlTable.Rows.Add(rowOne);
				controlTable.Rows.Add(SSNDOBRow);
				controlTable.Rows.Add(rowTwo);
				controlTable.Rows.Add(rowThree);

				string suffix = "_H";

				foreach (TableRow row in controlTable.Rows)
				{
					if (!string.IsNullOrEmpty(row.CssClass))
					{
						row.CssClass += suffix;
					}
					foreach (TableCell cell in row.Cells)
					{
						if (!string.IsNullOrEmpty(cell.CssClass))
						{
							cell.CssClass += suffix;
						}
						foreach (WebControl control in cell.Controls)
						{
							if (!string.IsNullOrEmpty(control.CssClass))
							{
								control.CssClass += suffix;
							}
						}
					}
				}

				#endregion
			}
			else
			{
				#region Vertical Layout

				captionCell.ColumnSpan = 4;
				orgCell.ColumnSpan = 3;
				govRegCell.ColumnSpan = 2;
				addrCell.ColumnSpan = 3;
				addr2Cell.ColumnSpan = 3;
				residentialCell.ColumnSpan = 3;
				contactCell.ColumnSpan = 3;
				emailCell.ColumnSpan = 3;
				saveAsNewCell.ColumnSpan = 3;
				ssnDOBCell.ColumnSpan = 2;

				if (AllowEdit)
				{
					OrgRow = new TableRow();
					OrgRow.Cells.Add(orgCaptionCell);
					OrgRow.Cells.Add(orgCell);
					controlTable.Rows.Add(OrgRow);

					GovRegRow = new TableRow();
					GovRegRow.Cells.Add(govRegCaptionCell);
					GovRegRow.Cells.Add(govRegListCell);
					GovRegRow.Cells.Add(govRegCell);
					controlTable.Rows.Add(GovRegRow);

					SSNDOBRow.Cells.Add(new TableCell());
					SSNDOBRow.Cells.Add(ssnDOBCaptionCell);
					SSNDOBRow.Cells.Add(ssnDOBCell);
					controlTable.Rows.Add(SSNDOBRow);

					AddrRow = new TableRow();
					AddrRow.Cells.Add(addrCaptionCell);
					AddrRow.Cells.Add(addrCell);
					controlTable.Rows.Add(AddrRow);

					Addr2Row = new TableRow();
					Addr2Row.Cells.Add(addr2CaptionCell);
					Addr2Row.Cells.Add(addr2Cell);
					controlTable.Rows.Add(Addr2Row);

					PostalCodeCityRow = new TableRow();
					PostalCodeCityRow.Cells.Add(postalCodeCaptionCell);
					PostalCodeCityRow.Cells.Add(postalCodeCell);
					PostalCodeCityRow.Cells.Add(cityCaptionCell);
					PostalCodeCityRow.Cells.Add(cityCell);
					controlTable.Rows.Add(PostalCodeCityRow);

					StateCountryRow = new TableRow();
					StateCountryRow.Cells.Add(stateCaptionCell);
					StateCountryRow.Cells.Add(stateCell);
					StateCountryRow.Cells.Add(countryCaptionCell);
					StateCountryRow.Cells.Add(countryCell);
					controlTable.Rows.Add(StateCountryRow);

					ResidentialRow = new TableRow();
					ResidentialRow.Cells.Add(new TableCell());
					ResidentialRow.Cells.Add(residentialCell);
					controlTable.Rows.Add(ResidentialRow);

					ContactRow = new TableRow();
					ContactRow.Cells.Add(contactCaptionCell);
					ContactRow.Cells.Add(contactCell);
					controlTable.Rows.Add(ContactRow);
				}
				else
				{
					TableRow summaryRow = new TableRow();
					TableCell formattedAddressCell = new TableCell();
					formattedAddressCell.ColumnSpan = 4;
					formattedAddressCell.Controls.Add(PutInNoWrapPanel(FormattedAddressLabel));
					summaryRow.Cells.Add(formattedAddressCell);
					controlTable.Rows.Add(summaryRow);
				}

				PhoneFaxRow = new TableRow();
				PhoneFaxRow.Cells.Add(phoneCaptionCell);
				PhoneFaxRow.Cells.Add(phoneCell);
				PhoneFaxRow.Cells.Add(faxCaptionCell);
				PhoneFaxRow.Cells.Add(faxCell);
				controlTable.Rows.Add(PhoneFaxRow);

				EmailRow = new TableRow();
				EmailRow.Cells.Add(emailCaptionCell);
				EmailRow.Cells.Add(emailCell);
				controlTable.Rows.Add(EmailRow);

				SaveAsNewRow = new TableRow();
				SaveAsNewRow.Cells.Add(new TableCell());
				SaveAsNewRow.Cells.Add(saveAsNewCell);
				controlTable.Rows.Add(SaveAsNewRow);

				#endregion
			}

			#endregion

			controlUpdatePanel.ContentTemplateContainer.Controls.Add(controlTable);
			Controls.Add(controlUpdatePanel);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			EnsureChildControls();
			base.Render(writer);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			SetupInternalControlsVisibility();
			SetupClientScripts();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			#region Control Caption

			CaptionLabel = new Label();

			FormattedAddressLabel = new ZTextLabel();
			FormattedAddressLabel.BindTo = "FormattedAddressSummary";

			#endregion

			#region Organisation

			OrganisationLabel = new Label();
			OrganisationTextBox = new ZAutoCompleteTextBoxWithButton();
			OrganisationTextBox.ModuleID = OrgModuleID;
			OrganisationTextBox.BindTo = "OrganisationPK";
			OrganisationTextBox.BindTextTo = "CompanyName";
			OrganisationTextBox.AutoPostBack = true;
			OrganisationTextBox.ID = "Org";
			OrganisationTextBox.AllowEdit = AllowEdit;
			((OrgHeaderAutoCompleteHelper)OrganisationTextBox.Helper).IsConsignee = IsConsignee;
			((OrgHeaderAutoCompleteHelper)OrganisationTextBox.Helper).IsConsignor = IsConsignor;
			((OrgHeaderAutoCompleteHelper)OrganisationTextBox.Helper).NewOrgRelationType = NewOrgRelationType;

			#endregion

			#region Gov. Reg. Num.

			GovRegNumLabel = new Label();

			GovRegNumTextBox = new ZTextBox();
			GovRegNumTextBox.AutoPostBack = true;
			GovRegNumTextBox.BindTo = "GovRegNum";
			GovRegNumTextBox.ID = "GovReg";

			SSNTextBox = new ZTextBox();
			SSNTextBox.BindTo = "SocialSecurityNumber";
			SSNTextBox.ID = "SocialSecurityNumber";

			SSNDOBLabel = new Label();
			SSNDOBLabel.Text = Res.GetString("12b00e1a-5360-43e8-b629-3ae7d73371a4", "Date Of Birth:");

			SSNDOBBox = new ZDateEditBox();
			SSNDOBBox.BindTo = "SocialSecurityNumberDateOfBirth";
			SSNDOBBox.ID = "SocialSecurityNumberDateOfBirth";

			GovRegNumTypeDropDownList = new ZDropDownList();
			GovRegNumTypeDropDownList.AutoPostBack = true;
			GovRegNumTypeDropDownList.BindTo = "GovRegNumType";
			GovRegNumTypeDropDownList.BindToList = "Lookups.GovRegNumTypes";
			GovRegNumTypeDropDownList.DisplayStyle = Enterprise.ZArchitecture.Core.OComboBoxDropDownStyle.CodeOnly;
			GovRegNumTypeDropDownList.ID = "GovRegType";

			#endregion

			#region Address

			AddressLabel = new Label();
			AddressTextBox = new ZAutoCompleteTextBoxWithButton();
			AddressTextBox.Helper = new OrgAddressAutoCompleteHelper(Factory);
			AddressTextBox.ModuleID = WebModuleIDs.OrgAddress;
			AddressTextBox.BindTo = "AddressPK";
			AddressTextBox.BindTextTo = "Address1";
			AddressTextBox.AutoPostBack = true;
			AddressTextBox.ID = "Addr1";
			AddressTextBox.AllowEdit = AllowEdit;

			Address2TextBox = new ZTextBox();
			Address2TextBox.BindTo = "Address2";
			Address2TextBox.ID = "Addr2";
			Address2TextBox.AllowEdit = AllowEdit;

			#endregion

			#region Postal Code & City

			PostalCodeLabel = new Label();
			PostalCodeTextBox = new ZTextBox();
			PostalCodeTextBox.BindTo = "Postcode";
			PostalCodeTextBox.ID = "Zip";
			PostalCodeTextBox.AllowEdit = AllowEdit;

			CityLabel = new Label();
			CityTextBox = new ZTextBox();
			CityTextBox.BindTo = "City";
			CityTextBox.ID = "City";
			CityTextBox.AllowEdit = AllowEdit;

			#endregion

			#region State & Country

			StateLabel = new Label();
			StateTextBox = new ZTextBox();
			StateTextBox.BindTo = "State";
			StateTextBox.ID = "State";
			StateTextBox.AllowEdit = AllowEdit;

			CountryLabel = new Label();
			CountryTextBox = new ZAutoCompleteTextBox();
			CountryTextBox.Helper = new CountryAutoCompleteHelper(Factory);
			CountryTextBox.BindTo = "CountryCode";
			CountryTextBox.ID = "Country";
			CountryTextBox.AllowEdit = AllowEdit;

			#endregion

			#region Residential

			ResidentialCheckBox = new ZCheckBox();
			ResidentialCheckBox.BindTo = "IsResidentialAddress";
			ResidentialCheckBox.ID = "ResAddr";

			#endregion

			#region Contact

			ContactLabel = new Label();
			ContactTextBox = new ZAutoCompleteTextBoxWithButton();
			ContactTextBox.Helper = new OrgContactAutoCompleteHelper(Factory);
			ContactTextBox.ModuleID = WebModuleIDs.OrgContact;
			ContactTextBox.AutoPostBack = true;
			ContactTextBox.BindTo = "ContactPK";
			ContactTextBox.BindTextTo = "ContactName";
			ContactTextBox.ID = "Cont";
			ContactTextBox.AllowEdit = AllowEdit;

			#endregion

			#region Phone & Fax

			PhoneLabel = new Label();
			PhoneTextBox = new ZTextBox();
			PhoneTextBox.BindTo = "Phone";
			PhoneTextBox.ID = "Phone";
			PhoneTextBox.AllowEdit = AllowEdit;

			FaxLabel = new Label();
			FaxTextBox = new ZTextBox();
			FaxTextBox.BindTo = "Fax";
			FaxTextBox.ID = "Fax";
			FaxTextBox.AllowEdit = AllowEdit;

			#endregion

			#region Email

			EmailLabel = new Label();
			EmailTextBox = new ZTextBox();
			EmailTextBox.BindTo = "Email";
			EmailTextBox.ID = "Email";
			EmailTextBox.AllowEdit = AllowEdit;

			#endregion

			#region Save As

			SaveAsNewCheckBox = new ZCheckBox();
			SaveAsNewCheckBox.BindTo = "SaveAsNew";
			SaveAsNewCheckBox.ID = "SaveAN";

			#endregion

			SetupEvents();
			SetupPostBackEvents();
		}

		#endregion

		#region Events

		protected virtual void SetupEvents()
		{
			OrganisationTextBox.TextChanged += new EventHandler(OnPostDataChanged);
			PostalCodeTextBox.TextChanged += new EventHandler(OnPostDataChanged);
			AddressTextBox.PostDataChanged += new EventHandler(OnPostDataChanged);
			StateTextBox.TextChanged += new EventHandler(OnPostDataChanged);
			CityTextBox.TextChanged += new EventHandler(OnPostDataChanged);
			CountryTextBox.TextChanged += new EventHandler(OnPostDataChanged);
			ContactTextBox.PostDataChanged += new EventHandler(OnPostDataChanged);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded string")]
		protected virtual void SetupClientScripts()
		{
			string onchangeEvent = string.IsNullOrEmpty(dependentPortControl) ? "" : "FindPort('" + dependentPortControl + "', '" + PostalCodeTextBox.ClientID + "', '" + CityTextBox.ClientID + "', '" + StateTextBox.ClientID + "', '" + CountryTextBox.ClientID + "');";
			PostalCodeTextBox.Attributes.Add("onchange", onchangeEvent);
			CityTextBox.Attributes.Add("onchange", onchangeEvent);
			StateTextBox.Attributes.Add("onchange", onchangeEvent);
			CountryTextBox.Attributes.Add("onchange", onchangeEvent);
		}

		public event EventHandler PostalCodeChanged;

		#endregion

		#region Event Handlers

		void RaisePostalCodeChanged()
		{
			if (PostalCodeChanged != null)
			{
				PostalCodeChanged(this, new EventArgs());
			}
		}

		#endregion

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return ((!string.IsNullOrEmpty(BindTo) && dataSource != null && dataSource is BusinessObject) || (dataSource != null && dataSource is JobDocAddress));
		}

		public void Bind(object dataSource)
		{
			BusinessEntity = dataSource;
			EnterpriseBusinessObject source = null;

			source = TryToGetSource<JobDocAddress>(dataSource);

			if (AllowEdit)
			{
				if (source != null)
				{
					BindToJobDocAddress((JobDocAddress)source);
				}
			}
			else
			{
				if (source != null && source is JobDocAddress)
				{
					BindToJobDocAddress((JobDocAddress)source);
				}
				else if (source == null)
				{
					source = TryToGetSource<OrgAddress>(dataSource);
					if (source != null)
					{
						BindToOrgAddress((OrgAddress)source);
					}
				}
			}
		}

		T TryToGetSource<T>(object dataSource) where T : BusinessObject
		{
			T source = null;
			if (dataSource is T)
			{
				source = (T)dataSource;
			}
			else
			{
				if (ZPropertyAccessor.Get(dataSource, BindTo) != null)
				{
					source = ZPropertyAccessor.Get(dataSource, BindTo) as T;
				}
			}
			return source;
		}

		void BindToOrgAddress(OrgAddress source)
		{
			WebOrgAddress webOrgAddress = new WebOrgAddress(source);
			FormattedAddressLabel.Bind(webOrgAddress);
			PhoneTextBox.Bind(webOrgAddress);
			FaxTextBox.Bind(webOrgAddress);
			EmailTextBox.Bind(webOrgAddress);
		}

		void BindToJobDocAddress(JobDocAddress source)
		{
			if (!Page.IsPostBack || WebJobDocAddress == null || WebJobDocAddress.IsDeleted)
			{
				WebJobDocAddress = new WebJobDocAddress(source, ResidentialCheckboxVisible);
				if (BusinessEntity is BusinessObject)
				{
					((BusinessObject)BusinessEntity).RegisterEditableChildObject(WebJobDocAddress);
				}

				WebJobDocAddress.IsOptional = IsOptional;
			}

			WebJobDocAddress.InitBeforeBinding(IsConsignor, IsConsignee, NewOrgRelationType);

			if (string.IsNullOrEmpty(CaptionLabel.Text))
			{
				CaptionLabel.Text = TryToGetControlCaptionFromAddressType();
				CaptionRow.Visible = !string.IsNullOrEmpty(CaptionLabel.Text);
			}

			DataBind();

			if (!ShowGovRegNum)
			{
				GovRegNumTextBox.Text = "";
				SSNTextBox.Text = "";
				SSNDOBBox.Text = "";
			}

			SetupInternalControlsVisibility();

			if (source != null && WebJobDocAddress != null)
			{
				//SaveAsNewRow.Visible = SaveAsNewRow.Visible && (Source.E2_AddressOverride || WebJobDocAddress.SaveAsNew);
			}
			else
			{
				SaveAsNewRow.Visible = false;
			}
			WebJobDocAddress.Inactive = !Visible;
		}

		void SetupInternalControlsVisibility()
		{
			if (HorizonatalLayout)
			{
				GovRegNumLabel.Visible = ShowGovRegNum;
				SSNTextBox.Visible = ShowGovRegNum && ShowSSNDetails;
				SSNDOBLabel.Visible = ShowGovRegNum && ShowSSNDetails;
				SSNDOBBox.Visible = ShowGovRegNum && ShowSSNDetails;
				SSNDOBRow.Visible = ShowGovRegNum && ShowSSNDetails;
				GovRegNumTypeDropDownList.Visible = ShowGovRegNum;
				GovRegNumTextBox.Visible = ShowGovRegNum && !ShowSSNDetails;
			}
			else
			{
				ResidentialRow.Visible = ShowResidentialAddressCheckBox;
				GovRegRow.Visible = ShowGovRegNum;
				GovRegNumTextBox.Visible = ShowGovRegNum && !ShowSSNDetails;
				SSNTextBox.Visible = ShowGovRegNum && ShowSSNDetails;
				SSNDOBLabel.Visible = ShowGovRegNum && ShowSSNDetails;
				SSNDOBBox.Visible = ShowGovRegNum && ShowSSNDetails;
				SSNDOBRow.Visible = ShowGovRegNum && ShowSSNDetails;
				SaveAsNewRow.Visible = ShowSaveCheckBox;
			}
		}

		public void UnBind()
		{
			WebJobDocAddress = null;
			BindTo = "";

			DataBind();
		}

		protected object BusinessEntity
		{
			get { return businessEntity; }
			set { businessEntity = value; }
		}

		object businessEntity;

		#endregion

		#region IBindTo Members

		public string BindTo
		{
			get { return bindTo; }
			set { bindTo = value; }
		}
		string bindTo = "";

		#endregion

		#region Implementation

		protected bool ShowSaveCheckBox
		{
			get { return SaveCheckboxVisible && CanAddNewOrganisations && !HasGovRegNum && HasSaveAsCheckboxCaption; }
		}

		bool HasSaveAsCheckboxCaption
		{
			get { return !string.IsNullOrWhiteSpace(SaveCheckboxCaption); }
		}

		protected bool ShowGovRegNum
		{
			get { return GovermentRegNoVisible && WebJobDocAddress.JobDocAddressWillBeOverriden; }
		}

		protected bool ShowSSNDetails
		{
			get
			{
				return WebJobDocAddress.IsSocialSecurityNumberGovRegNumType;
			}
		}

		protected bool ShowResidentialAddressCheckBox
		{
			get
			{
				return WebJobDocAddress.ResidentialAddressVisible;
			}
		}

		#region Declarations

		TableRow CaptionRow = new TableRow();
		TableRow OrgRow = new TableRow();
		TableRow GovRegRow = new TableRow();
		readonly TableRow SSNDOBRow = new TableRow();
		TableRow AddrRow = new TableRow();
		TableRow Addr2Row = new TableRow();
		TableRow PostalCodeCityRow = new TableRow();
		TableRow StateCountryRow = new TableRow();
		TableRow ResidentialRow = new TableRow();
		TableRow ContactRow = new TableRow();
		TableRow PhoneFaxRow = new TableRow();
		TableRow EmailRow = new TableRow();
		TableRow SaveAsNewRow = new TableRow();

		Label CaptionLabel;
		ZTextLabel FormattedAddressLabel;

		Label OrganisationLabel;
		ZAutoCompleteTextBoxWithButton OrganisationTextBox;

		Label AddressLabel;
		ZAutoCompleteTextBoxWithButton AddressTextBox;
		ZTextBox Address2TextBox;

		Label PostalCodeLabel;
		ZTextBox PostalCodeTextBox;

		Label CityLabel;
		ZTextBox CityTextBox;

		Label StateLabel;
		ZTextBox StateTextBox;

		Label CountryLabel;
		ZAutoCompleteTextBox CountryTextBox;

		Label GovRegNumLabel;
		ZTextBox GovRegNumTextBox;
		ZTextBox SSNTextBox;
		Label SSNDOBLabel;
		ZDateEditBox SSNDOBBox;
		ZDropDownList GovRegNumTypeDropDownList;

		Label ContactLabel;
		ZAutoCompleteTextBoxWithButton ContactTextBox;

		Label PhoneLabel;
		ZTextBox PhoneTextBox;

		Label FaxLabel;
		ZTextBox FaxTextBox;

		Label EmailLabel;
		ZTextBox EmailTextBox;

		ZCheckBox ResidentialCheckBox;
		ZCheckBox SaveAsNewCheckBox;

		#endregion

		#region DataBind

		public override void DataBind()
		{
			base.DataBind();

			BindChildControls();
		}

		void BindChildControls()
		{
			((OrgAddressAutoCompleteHelper)AddressTextBox.Helper).ParentPK = ParentOrgPK;
			AddressTextBox.PopupEnabled = !ParentOrgPK.IsEmpty;
			((OrgContactAutoCompleteHelper)ContactTextBox.Helper).ParentPK = ParentOrgPK;
			ContactTextBox.PopupEnabled = !ParentOrgPK.IsEmpty;

			SaveAsNewCheckBox.Bind(WebJobDocAddress);

			BindChildControlsCore(this);

			ContactTextBox.Bind(WebJobDocAddress);
			AddressTextBox.Bind(WebJobDocAddress);
			OrganisationTextBox.Bind(WebJobDocAddress);
		}

		void BindChildControlsCore(Control parentControl)
		{
			foreach (Control control in parentControl.Controls)
			{
				ISelfBindingWebControl bindControl = control as ISelfBindingWebControl;
				if (bindControl != null && bindControl.IsBindable(WebJobDocAddress))
				{
					if (!(bindControl.Equals(AddressTextBox) || bindControl.Equals(ContactTextBox) || bindControl.Equals(OrganisationTextBox) || bindControl.Equals(SaveAsNewCheckBox)))
					{
						bindControl.Bind(WebJobDocAddress);
					}
				}
				else
				{
					BindChildControlsCore(control);
				}
			}
		}

		#endregion

		protected WebJobDocAddress WebJobDocAddress
		{
			get
			{
				return HttpContext.Current.Session[WebJobDocAddressPK.ToString()] as WebJobDocAddress;
			}
			set
			{
				WebJobDocAddressPK = value.PK;
				HttpContext.Current.Session[WebJobDocAddressPK.ToString()] = value;
			}
		}

		protected ZGuid ParentOrgPK
		{
			get
			{
				return WebJobDocAddress.OrganisationPK.IsEmpty ? (ZGuid)OrganisationTextBox.Helper.GetKey(WebJobDocAddress.CompanyName) : WebJobDocAddress.OrganisationPK;
			}
		}

		protected ZGuid WebJobDocAddressPK
		{
			get { return (ZGuid)(ViewState["WebJobDocAddressPK"] ?? ZGuid.Empty); }
			set { ViewState["WebJobDocAddressPK"] = value; }
		}

		protected bool CanAddNewOrganisations
		{
			get { return ((OrgContactWebUser)WebEnv.AppInstance.SiteUser).CanAddNewOrganisations; }
		}

		protected bool HasGovRegNum
		{
			get { return GovermentRegNoVisible && GovRegNumTextBox.Text.Length > 0; }
		}

		#endregion

		#region ISelfBindingPostbackWebControl Members

		protected void OnPostDataChanged(object sender, EventArgs e)
		{
			RaisePostDataChangedEvent();
			RaisePostalCodeChanged();
		}

		public event EventHandler PostDataChanged;

		protected void SetupPostBackEvents()
		{
			OrganisationTextBox.PostDataChanged += new EventHandler(this.OnPostDataChanged);
			SaveAsNewCheckBox.CheckedChanged += new EventHandler(this.OnPostDataChanged);
		}

		public bool HasChanges
		{
			get { return hasChanges; }
			set { hasChanges = value; }
		}
		bool hasChanges;

		#endregion

		#region IPostBackDataHandler Members

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			bool result = false;
			result = result || OrganisationTextBox.LoadPostData(postDataKey, postCollection);
			result = result || AddressTextBox.LoadPostData(postDataKey, postCollection);
			result = result || CountryTextBox.ProcessPostData(postDataKey, postCollection);
			result = result || ContactTextBox.LoadPostData(postDataKey, postCollection);
			return result;
		}

		public void RaisePostDataChangedEvent()
		{
			if (IsBindable(BusinessEntity))
			{
				Bind(BusinessEntity);
			}
			if (PostDataChanged != null)
			{
				PostDataChanged(this, new EventArgs());
			}
		}

		#endregion

		#region IWebServiceMethodsCaller

		public List<IWebServiceMethod> WebServiceMethods
		{
			get
			{
				return webServiceMethods ?? (webServiceMethods = GetServiceMethods());
			}
		}

		List<IWebServiceMethod> webServiceMethods;

		protected virtual List<IWebServiceMethod> GetServiceMethods()
		{
			List<IWebServiceMethod> result = new List<IWebServiceMethod>();
			result.Add(new PortFinderWebServiceMethod());
			return result;
		}

		#endregion

	}
}
