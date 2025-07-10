using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAImportInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		readonly PGATabCollection pgaTabCollection;

		public CAImportInvoiceLineUserControl()
		{
			InitializeComponent();

			pgaTabCollection = new PGATabCollection(LineDetailTabControl, CustomsInvoiceLinesBoundGrid, true);

			if (!DesignModeFinder.IsDesigning)
			{
				var countryOfOriginColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin);
				countryOfOriginColumnStyle.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|409f6b89-db86-409f-91c9-2b02f0bf0e5a", "ORG", "Origin", "Goods Origin", "Country/Region of Origin of the goods.");
				countryOfOriginColumnStyle.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|d8c4d98e-28e0-49f8-af66-f633bfc79357", "Origin");
				CustomsInvoiceLinesBoundGrid.ColourDeciding += CustomsInvoiceLinesBoundGrid_ColourDeciding;
			}

			ClassificationTariffUserControlHelper.UpdateTariffColumnStyleInfoAndTariffFindBoxToGetTariffFromSRDb(
				CustomsInvoiceLinesBoundGrid,
				JobComInvoiceLine.Schema.JI_FormattedTariff,
				ClassificationNumberFindBox,
				"ClassificationNumberFromRefDbFindBox",
				() => { return CurrentInvoiceLine?.EffectiveDateForDutyRate ?? ZDateTime.Today; },
				(x) =>
				{
					this.BindingSource.SetBindingMember(x, "FilteredInvoiceLines.JI_FormattedTariff");
					CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff);
				}
				);
		}

		#region Overrides

		protected override bool UseUniversalTariff
		{
			get { return false; }
		}

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		new JobComInvoiceLine CurrentInvoiceLine
		{
			get { return (JobComInvoiceLine)base.CurrentInvoiceLine; }
		}

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl()
		{
			return new InvoiceLineChargesUserControl();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			JobDeclaration.CA_OGDCFIAInfo.ValueChanged += CA_OGDCFIAInfo_ValueChanged;
			JobDeclaration.CA_OGDICInfo.ValueChanged += CA_OGDICInfo_ValueChanged;
			JobDeclaration.CA_OGDNRInfo.ValueChanged += CA_OGDNRInfo_ValueChanged;
			JobDeclaration.CA_OGDTCInfo.ValueChanged += CA_OGDTCInfo_ValueChanged;
			JobDeclaration.OnApportionmentDirtyChanged += Declaration_OnApportionmentDirtyChanged;
			SetCFIAVisibility();
			SetSITTVisibility();
			SetNRCANVisibility();
			SetTiresVisibility();
			Declaration_OnApportionmentDirtyChanged();
			USStateOfExportDropEdit.Visible = CountryOfExportCodeFindBox.Visible = JobDeclaration.IsLVS;
			if (JobDeclaration.IsIM2)
			{
				CustomsInvoiceLinesBoundGrid.AllowCopyToNewRowMenuItem = true;
				this.CustomsInvoiceLinesBoundGrid.SelectedRowsChangedInMouseDown += CustomsInvoiceLinesBoundGrid_SelectIndexChanged;
				CustomsInvoiceLinesBoundGrid.DisableImportDataMenuItem = true;
			}
			else
			{
				CustomsInvoiceLinesBoundGrid.AllowCopyToNewRowMenuItem = false;
				CustomsInvoiceLinesBoundGrid.DisableImportDataMenuItem = false;
			}
		}

		protected override ZArchitecture.Modules.ModuleIdentifier ClassificationModuleID
		{
			get { return ZArchitecture.Modules.ModuleIDs.Customs.CA.HTSClassification; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			declarationValueChangedAnnouncer_OnValueChanged(this, null);
			declarationValueChangedAnnouncer = CurrentDataItem?.GetValueChangedAnnouncer();
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged += new EventHandler(declarationValueChangedAnnouncer_OnValueChanged);
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
			SetVisibility();
		}

		protected override Core.Forms.ZGridColumnInfo CreatePartAttrib1Column()
		{
			ZDropEditColumnStyleInfo partAttrib1DropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib1DropEditColumnStyleInfo.ColumnName = "JI_PartAttrib1";
			partAttrib1DropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			partAttrib1DropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			return partAttrib1DropEditColumnStyleInfo;
		}

		protected override Core.Forms.ZGridColumnInfo CreatePartAttrib2Column()
		{
			ZDropEditColumnStyleInfo partAttrib2DropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib2DropEditColumnStyleInfo.ColumnName = "JI_PartAttrib2";
			partAttrib2DropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			partAttrib2DropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			return partAttrib2DropEditColumnStyleInfo;
		}

		protected override Core.Forms.ZGridColumnInfo CreatePartAttrib3Column()
		{
			ZDropEditColumnStyleInfo partAttrib3DropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib3DropEditColumnStyleInfo.ColumnName = "JI_PartAttrib3";
			partAttrib3DropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			partAttrib3DropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			return partAttrib3DropEditColumnStyleInfo;
		}

		protected override Core.Forms.ZGridColumnInfo CreateSerialNumberColumn()
		{
			ZDropEditColumnStyleInfo serialNumberDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			serialNumberDropEditColumnStyleInfo.ColumnName = "JI_SerialNumber";
			serialNumberDropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			serialNumberDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			return serialNumberDropEditColumnStyleInfo;
		}

		void SetPGATabPageVisible()
		{
			if (JobDeclaration?.IsIID ?? false)
			{
				PGATabPage.TabVisible = true;
			}
		}

		void SetupIIDColumns(bool isIID)
		{
			if (!isIID)
			{
				CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(IIDColumns);
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLine.Schema.CA_OGDStatus, Res.GetString("46290d21-442c-4bc0-b890-892e02924c1a", "OGD Status"));
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLine.Schema.CA_OGDStatusDescription, Res.GetString("b6d59ffb-9355-4538-8c09-2167a74d3783", "OGD Status Description"));
			}
			else
			{
				CustomsInvoiceLinesBoundGrid.AddToAvailableColumns(IIDColumns);
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLine.Schema.CA_OGDStatus, Res.GetString("6663c846-4392-48d0-9f79-7b09c0d56aff", "PGA Status"));
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLine.Schema.CA_OGDStatusDescription, Res.GetString("46d45e91-671f-4058-8c65-fc7220b3b8dc", "PGA Status Description"));
			}
		}

		void SetVisibility()
		{
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				var isIID = declaration.IsIID;
				PGATabPage.TabVisible = isIID;
				pgaTabCollection.Visible = isIID;
				CFIATabPage.TabVisible = !isIID;
				SITTTabPage.TabVisible = !isIID;
				NRCANTabPage.TabVisible = !isIID;
				TiresTabPage.TabVisible = !isIID;

				CustomsInvoiceLinesBoundGrid.SetAvailability(declaration.IsWHSUniversalXMLActive && declaration.IsExWarehouseEntry, [JobComInvoiceLine.Schema.JI_PreviousEntryNumber, JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber]);
				CustomsInvoiceLinesBoundGrid.SetAvailability(declaration.IsOGD || isIID, [JobComInvoiceLine.Schema.CA_OGDStatus, JobComInvoiceLine.Schema.CA_OGDStatusDescription]);

				PackagesPivotTabPage.TabVisible = declaration.SupportsChcPivotBetweenInvoiceLineAndPacking;
				SetupIIDColumns(isIID);
			}
		}

		#region InitializeGridLayout

		protected override void InitializeGridLayoutCore()
		{
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = Core.Constants.CountryCodes.Canada + JobDeclaration.JE_MessageType;
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				AddColumnsForIM2Job();
				AddColumnsForIMPAndIIDJob();
				CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.CA_RN_NKExport, JobComInvoiceLine.Schema.CA_USStateOfExport);
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, InvoiceLineDefaultColumnsSequence);
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(InvoiceLineDefaultColumnsSequence);
			}
		}

		void AddColumnsForIM2Job()
		{
			if (JobDeclaration.IsIM2)
			{
				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9F974179-7A20-40C8-8434-2411E8058AD2", "Previous Entry Line");
				zTextBoxColumnStyleInfo1.ColumnName = "CA_PreviousB3LineNo";
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo1, 70, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|3B9F5D32-761A-4175-A6CE-36BF448080B2", "Previous Sub-Header");
				zTextBoxColumnStyleInfo2.ColumnName = "CA_PreviousB3SubHeaderNo";
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo2, 50, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|886B72BA-917D-42B2-A443-5A7CC61BDB2B", "New Sub-header");
				zTextBoxColumnStyleInfo3.ColumnName = "CusEntryLine.CA_B2SubHeader";
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo3, 50, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			}
		}

		void AddColumnsForIMPAndIIDJob()
		{
			if (JobDeclaration.IsImport && JobDeclaration.IsIID)
			{
				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C48910A1-0460-46C2-9E59-2B5013DC706A", "CFIA Indicator");
				zCheckBoxColumnStyleInfo3.ColumnName = "CA_CFIAAllProgramInd";
				zCheckBoxColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|99D89FBF-4293-46E4-A12C-FA073A520502", "PGA CFIA");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo3, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);

				ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo11 = new ZCodeFindBoxColumnStyleInfo();
				zCodeFindBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|7C748258-D8A1-4D4E-8FE3-D4C102F968C4", "CFIA Country/Region Of Source");
				zCodeFindBoxColumnStyleInfo11.ColumnName = "CA_RN_NKCountryOfSourceCFIA";
				zCodeFindBoxColumnStyleInfo11.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|99D89FBF-4293-46E4-A12C-FA073A520502", "PGA CFIA");
				ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo11, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo11);

				ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo9 = new ZCodeFindBoxColumnStyleInfo();
				zCodeFindBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|651DCCE8-A2B9-451B-8F72-1DED5DFDECCC", "CFIA AIRS End Use");
				zCodeFindBoxColumnStyleInfo9.ColumnName = "CA_AIRSEndUseCFIA";
				zCodeFindBoxColumnStyleInfo9.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|99D89FBF-4293-46E4-A12C-FA073A520502", "PGA CFIA");
				ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo9, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo9);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo33 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo33.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4B13C681-9251-4186-A952-D6662D3DAA79", "CFIA AIRS Extension Code");
				zTextBoxColumnStyleInfo33.ColumnName = "CA_AIRSExtensionCodeCFIA";
				zTextBoxColumnStyleInfo33.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|99D89FBF-4293-46E4-A12C-FA073A520502", "PGA CFIA");
				zTextBoxColumnStyleInfo33.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo33, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo33);

				ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
				zGuidFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|04A41987-247E-48AD-A316-ACD5149CFDE4", "CFIA Delivery Location");
				zGuidFindBoxColumnStyleInfo.ColumnName = "CA_DeliveryLocationCFIA";
				zGuidFindBoxColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|99D89FBF-4293-46E4-A12C-FA073A520502", "PGA CFIA");
				ControlDpiScalingHelper.SetWidth(ref zGuidFindBoxColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo);

				ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo4 = new ZGuidDropEditColumnStyleInfo();
				zGuidDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F6E9186A-F5A5-46E6-B424-8D97283D1A73", "CFIA Delivery Address");
				zGuidDropEditColumnStyleInfo4.ColumnName = "CA_OA_ConsigneeAddressCFIA";
				zGuidDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|99D89FBF-4293-46E4-A12C-FA073A520502", "PGA CFIA");
				ControlDpiScalingHelper.SetWidth(ref zGuidDropEditColumnStyleInfo4, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo4);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo34 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo34.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|E5DC0789-8553-4A52-B453-208D969BCC8E", "CFIA State Of Source");
				zDropEditColumnStyleInfo34.ColumnName = "CA_RW_NKSourceStateCFIA";
				zDropEditColumnStyleInfo34.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|99D89FBF-4293-46E4-A12C-FA073A520502", "PGA CFIA");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo34, 200, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo34);

				ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo10 = new ZCodeFindBoxColumnStyleInfo();
				zCodeFindBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|1D9568DF-7D71-4F49-A055-25AA7A116D7F", "CFIA AIRS Miscellaneous");
				zCodeFindBoxColumnStyleInfo10.ColumnName = "CA_AIRSMiscellaneousCFIA";
				zCodeFindBoxColumnStyleInfo10.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|99D89FBF-4293-46E4-A12C-FA073A520502", "PGA CFIA");
				ControlDpiScalingHelper.SetWidth(ref zCodeFindBoxColumnStyleInfo10, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo10);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|AA615D5C-E568-4826-B490-8D918AD17FFC", "HC API Intended Use Code");
				zDropEditColumnStyleInfo11.ColumnName = "CA_IntendedUseCodeAPI";
				zDropEditColumnStyleInfo11.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D20BB4DF-DEF3-48CD-9C72-E07AACB862CC", "PGA HC API");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo11, 200, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|41E88897-647F-451D-AEFB-A898EF05FFA0", "HC API Indicator");
				zCheckBoxColumnStyleInfo4.ColumnName = "CA_APIProgramInd";
				zCheckBoxColumnStyleInfo4.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D20BB4DF-DEF3-48CD-9C72-E07AACB862CC", "PGA HC API");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo4, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|2D59FD74-507B-4653-86D4-0E39AC1B0E4D", "HC API Commodity Code");
				zDropEditColumnStyleInfo12.ColumnName = "CA_CategoryAPI";
				zDropEditColumnStyleInfo12.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D20BB4DF-DEF3-48CD-9C72-E07AACB862CC", "PGA HC API");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo12, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|CC731FD4-88AF-443D-9A3A-D601D9028432", "HC API Manufacture Date");
				zDateEditColumnStyleInfo1.ColumnName = "CA_ProductionDateAPI";
				zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D20BB4DF-DEF3-48CD-9C72-E07AACB862CC", "PGA HC API");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo1, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo34 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo34.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|408D1AE6-984B-429C-95F4-89BB531FA604", "HC API GITN No");
				zTextBoxColumnStyleInfo34.ColumnName = "CA_GTINNumberAPI";
				zTextBoxColumnStyleInfo34.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D20BB4DF-DEF3-48CD-9C72-E07AACB862CC", "PGA HC API");
				zTextBoxColumnStyleInfo34.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo34, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo34);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo54 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo54.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|1A6B16AD-6FE7-4F0B-A7E2-8E1EAED6FD43", "HC API Brand Name");
				zTextBoxColumnStyleInfo54.ColumnName = "CA_BrandNameAPI";
				zTextBoxColumnStyleInfo54.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D20BB4DF-DEF3-48CD-9C72-E07AACB862CC", "PGA HC API");
				zTextBoxColumnStyleInfo54.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo54, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo54);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo35 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo35.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|E2DBFA1C-3511-4373-B1CD-E0961DD6982D", "HC API Batch/Lot Number");
				zTextBoxColumnStyleInfo35.ColumnName = "CA_BatchLotNumberAPI";
				zTextBoxColumnStyleInfo35.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D20BB4DF-DEF3-48CD-9C72-E07AACB862CC", "PGA HC API");
				zTextBoxColumnStyleInfo35.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo35, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo35);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|A9E3746B-3DB0-4469-88D3-2B8DBA71D78C", "HC BBC Indicator");
				zCheckBoxColumnStyleInfo5.ColumnName = "CA_BBCProgramInd";
				zCheckBoxColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|67A2F12F-6A1F-459A-8C2E-EDD4BEC34517", "PGA HC BBC");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo5, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B81F3EBA-5603-4C75-AD3B-5551C09428E5", "HC BBC Intended Use Code");
				zDropEditColumnStyleInfo13.ColumnName = "CA_IntendedUseCodeBBC";
				zDropEditColumnStyleInfo13.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|67A2F12F-6A1F-459A-8C2E-EDD4BEC34517", "PGA HC BBC");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo13, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|049E2AF4-A431-4FF7-B620-91047E99F545", "HC BBC Commodity Code");
				zDropEditColumnStyleInfo14.ColumnName = "CA_CategoryBBC";
				zDropEditColumnStyleInfo14.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|67A2F12F-6A1F-459A-8C2E-EDD4BEC34517", "PGA HC BBC");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo14, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo39 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo39.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|74B487A1-D245-40EB-B52B-AB0C02E38F72", "HC BBC GITN No");
				zTextBoxColumnStyleInfo39.ColumnName = "CA_GTINNumberBBC";
				zTextBoxColumnStyleInfo39.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|67A2F12F-6A1F-459A-8C2E-EDD4BEC34517", "PGA HC BBC");
				zTextBoxColumnStyleInfo39.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo39, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo39);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F83C38DC-4AF3-43BF-97B2-F2DD74243E05", "HC BBC Expiry Date");
				zDateEditColumnStyleInfo2.ColumnName = "CA_ExpiryDateBBC";
				zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|67A2F12F-6A1F-459A-8C2E-EDD4BEC34517", "PGA HC BBC");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo2, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B0D326D6-8445-4F3E-B777-D1CED04F9B90", "HC CTO Indicator");
				zCheckBoxColumnStyleInfo6.ColumnName = "CA_CTOProgramInd";
				zCheckBoxColumnStyleInfo6.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F22D0EA5-D331-4D4B-A3DC-8F615DD8EF89", "PGA HC CTO");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo6, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|74FE2C0A-7D01-44BA-95A5-036973F92358", "HC CTO Intended Use Code");
				zDropEditColumnStyleInfo15.ColumnName = "CA_IntendedUseCodeCTO";
				zDropEditColumnStyleInfo15.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F22D0EA5-D331-4D4B-A3DC-8F615DD8EF89", "PGA HC CTO");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo15, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo15);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo16 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|645A202B-B45A-4526-B636-DBEFA9D9E5FD", "HC CTO Commodity Code");
				zDropEditColumnStyleInfo16.ColumnName = "CA_CategoryCTO";
				zDropEditColumnStyleInfo16.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F22D0EA5-D331-4D4B-A3DC-8F615DD8EF89", "PGA HC CTO");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo16, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo16);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B7C32CED-2770-4061-9577-7CF70D9B2BA0", "HC CTO Expiry Date");
				zDateEditColumnStyleInfo9.ColumnName = "CA_ExpiryDateCTO";
				zDateEditColumnStyleInfo9.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo9.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F22D0EA5-D331-4D4B-A3DC-8F615DD8EF89", "PGA HC CTO");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo9, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo40 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo40.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|84E6FE96-67E6-49FB-8E75-BC39854AF6E6", "HC CTO GITN No");
				zTextBoxColumnStyleInfo40.ColumnName = "CA_GTINNumberCTO";
				zTextBoxColumnStyleInfo40.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F22D0EA5-D331-4D4B-A3DC-8F615DD8EF89", "PGA HC CTO");
				zTextBoxColumnStyleInfo40.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo40, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo40);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|01D13556-4794-4C09-B585-21987EC0D177", "HC CTO Lymphohematopoietic Cells and Organs");
				zCheckBoxColumnStyleInfo7.ColumnName = "CA_CTO_LCO";
				zCheckBoxColumnStyleInfo7.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F22D0EA5-D331-4D4B-A3DC-8F615DD8EF89", "PGA HC CTO");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo7, 230, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|BA26F546-B420-4FCD-820A-63FEEB918E5B", "HC CPR Indicator");
				zCheckBoxColumnStyleInfo8.ColumnName = "CA_CPRProgramInd";
				zCheckBoxColumnStyleInfo8.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo8, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo17 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|FE106E55-FCB3-43E9-917E-A5DE9327BA99", "HC CPR Intended Use Code");
				zDropEditColumnStyleInfo17.ColumnName = "CA_IntendedUseCodeCPR";
				zDropEditColumnStyleInfo17.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo17, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo17);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo18 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|7886D5E7-B980-4ED5-88BE-275EE75E6177", "HC CPR Commodity Code");
				zDropEditColumnStyleInfo18.ColumnName = "CA_CategoryCPR";
				zDropEditColumnStyleInfo18.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo18, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo18);

				ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
				zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|409ACE59-581F-48A3-8C02-98EC7B8D1416", "HC CPR Manufacturer");
				zGuidFindBoxColumnStyleInfo2.ColumnName = "CA_ManufacturerOrgPKCPR";
				zGuidFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				ControlDpiScalingHelper.SetWidth(ref zGuidFindBoxColumnStyleInfo2, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);

				ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo = new ZGuidDropEditColumnStyleInfo();
				zGuidDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|6BB944FA-DF04-4B05-A782-7B321A987764", "HC CPR Address");
				zGuidDropEditColumnStyleInfo.ColumnName = "CA_OA_ManufacturerAddressCPR";
				zGuidDropEditColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				ControlDpiScalingHelper.SetWidth(ref zGuidDropEditColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|370CE73F-4A16-431B-B5EB-26F686EA9026", "HC CPR Manufacture Date");
				zDateEditColumnStyleInfo3.ColumnName = "CA_ProductionDateCPR";
				zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo3, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo41 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo41.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|CAE68A21-BAC8-4667-8553-17244B9B6770", "HC CPR GITN No");
				zTextBoxColumnStyleInfo41.ColumnName = "CA_GTINNumberCPR";
				zTextBoxColumnStyleInfo41.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				zTextBoxColumnStyleInfo41.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo41, 100, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo41);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo55 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo55.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|CBC6D00A-FEAD-4EE0-9861-B615E588AA0E", "HC CPR Brand Name");
				zTextBoxColumnStyleInfo55.ColumnName = "CA_BrandNameCPR";
				zTextBoxColumnStyleInfo55.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				zTextBoxColumnStyleInfo55.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo55, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo55);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo47 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo47.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B85D6837-3CEF-4712-9E84-8246C43F6B4C", "HC CPR Batch/Lot Number");
				zTextBoxColumnStyleInfo47.ColumnName = "CA_BatchLotNumberCPR";
				zTextBoxColumnStyleInfo47.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				zTextBoxColumnStyleInfo47.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo47, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo47);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo61 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo61.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5A8A3284-EF91-422C-AA4B-94E8B065CF60", "HC CPR Trade Name");
				zTextBoxColumnStyleInfo61.ColumnName = "CA_TradeNameCPR";
				zTextBoxColumnStyleInfo61.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|621AADE5-83B7-4096-8063-4E2CCD93B4F9", "PGA HC CPR");
				zTextBoxColumnStyleInfo61.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo61, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo61);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F88283F4-FF36-4682-93EE-465051D3ACCA", "HC DSE Indicator");
				zCheckBoxColumnStyleInfo9.ColumnName = "CA_DSEProgramInd";
				zCheckBoxColumnStyleInfo9.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|A8F855D7-D55A-4F0B-9C59-789FDFFDEC5C", "PGA HC DSE");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo9, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo19 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|2329A4A6-B805-4CD2-82A5-DCA345CE73F1", "HC DSE Intended Use Code");
				zDropEditColumnStyleInfo19.ColumnName = "CA_IntendedUseCodeDSE";
				zDropEditColumnStyleInfo19.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|A8F855D7-D55A-4F0B-9C59-789FDFFDEC5C", "PGA HC DSE");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo19, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo19);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo20 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|E09B668B-B42C-44BE-9786-C1196873A73D", "HC DSE Commodity Code");
				zDropEditColumnStyleInfo20.ColumnName = "CA_CategoryDSE";
				zDropEditColumnStyleInfo20.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|A8F855D7-D55A-4F0B-9C59-789FDFFDEC5C", "PGA HC DSE");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo20, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo20);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D65DD9CF-D32D-4B97-ACE4-92451F524894", "HC DSE Certify");
				zCheckBoxColumnStyleInfo10.ColumnName = "CA_ComplianceStatement";
				zCheckBoxColumnStyleInfo10.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|A8F855D7-D55A-4F0B-9C59-789FDFFDEC5C", "PGA HC DSE");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo10, 90, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo11 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|74AE0327-6BB2-4662-8265-43750A168751", "HC HDR Indicator");
				zCheckBoxColumnStyleInfo11.ColumnName = "CA_HDRProgramInd";
				zCheckBoxColumnStyleInfo11.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4C3E6E60-4C12-46A4-8FE7-D420F098DDBE", "PGA HC HDR");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo11, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo11);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo21 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo21.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|988809DB-68A5-4CCE-84D8-FBB8530A2A46", "HC HDR Intended Use Code");
				zDropEditColumnStyleInfo21.ColumnName = "CA_IntendedUseCodeHDR";
				zDropEditColumnStyleInfo21.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4C3E6E60-4C12-46A4-8FE7-D420F098DDBE", "PGA HC HDR");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo21, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo21);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo22 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo22.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B93A47CE-3266-445B-951B-B2997D32847E", "HC HDR Commodity Code");
				zDropEditColumnStyleInfo22.ColumnName = "CA_CategoryHDR";
				zDropEditColumnStyleInfo22.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4C3E6E60-4C12-46A4-8FE7-D420F098DDBE", "PGA HC HDR");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo22, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo22);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|EB762057-B28C-49E0-80C1-777765B40150", "HC HDR Manufacture Date");
				zDateEditColumnStyleInfo4.ColumnName = "CA_ProductionDateHDR";
				zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo4.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4C3E6E60-4C12-46A4-8FE7-D420F098DDBE", "PGA HC HDR");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo4, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo42 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo42.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|33560A16-2B53-49E2-929E-C4BB27935562", "HC HDR GITN No");
				zTextBoxColumnStyleInfo42.ColumnName = "CA_GTINNumberHDR";
				zTextBoxColumnStyleInfo42.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4C3E6E60-4C12-46A4-8FE7-D420F098DDBE", "PGA HC HDR");
				zTextBoxColumnStyleInfo42.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo42, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo42);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo56 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo56.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D4A2DFD5-4383-45CB-A34A-5A2F0466164A", "HC HDR Brand Name");
				zTextBoxColumnStyleInfo56.ColumnName = "CA_BrandNameHDR";
				zTextBoxColumnStyleInfo56.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4C3E6E60-4C12-46A4-8FE7-D420F098DDBE", "PGA HC HDR");
				zTextBoxColumnStyleInfo56.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo56, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo56);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo48 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo48.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5D93F96D-FF22-4F08-A26F-BC14605A23D4", "HC HDR Batch/Lot Number");
				zTextBoxColumnStyleInfo48.ColumnName = "CA_BatchLotNumberHDR";
				zTextBoxColumnStyleInfo48.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4C3E6E60-4C12-46A4-8FE7-D420F098DDBE", "PGA HC HDR");
				zTextBoxColumnStyleInfo48.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo48, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo48);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo12 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4AB8397E-C8B9-4710-BF32-44772F159DDA", "HC OCS Indicator");
				zCheckBoxColumnStyleInfo12.ColumnName = "CA_OCSProgramInd";
				zCheckBoxColumnStyleInfo12.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5FACB22A-A9D2-4C1C-8B07-709ACD863214", "PGA HC OCS");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo12, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo12);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo23 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo23.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|806040CA-D411-4FAA-81FD-81238C7AB1C3", "HC OCS Intended Use Code");
				zDropEditColumnStyleInfo23.ColumnName = "CA_IntendedUseCodeOCS";
				zDropEditColumnStyleInfo23.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5FACB22A-A9D2-4C1C-8B07-709ACD863214", "PGA HC OCS");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo23, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo23);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo24 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo24.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|0210842D-9D14-48D4-80B6-70C5F954FF7C", "HC OCS Commodity Code");
				zDropEditColumnStyleInfo24.ColumnName = "CA_CategoryOCS";
				zDropEditColumnStyleInfo24.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5FACB22A-A9D2-4C1C-8B07-709ACD863214", "PGA HC OCS");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo24, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo24);

				ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
				zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|F4854766-185B-4CF8-8D66-9FD175B7342D", "HC OCS Manufacturer");
				zGuidFindBoxColumnStyleInfo4.ColumnName = "CA_ManufacturerOrgPKOCS";
				zGuidFindBoxColumnStyleInfo4.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5FACB22A-A9D2-4C1C-8B07-709ACD863214", "PGA HC OCS");
				ControlDpiScalingHelper.SetWidth(ref zGuidFindBoxColumnStyleInfo4, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);

				ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new ZGuidDropEditColumnStyleInfo();
				zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B8497E66-79A8-4FD6-86D6-40A2EFDA6720", "HC OCS Address");
				zGuidDropEditColumnStyleInfo2.ColumnName = "CA_OA_ManufacturerAddressOCS";
				zGuidDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5FACB22A-A9D2-4C1C-8B07-709ACD863214", "PGA HC OCS");
				ControlDpiScalingHelper.SetWidth(ref zGuidDropEditColumnStyleInfo2, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo57 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo57.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|065EBA83-5D81-4F40-B3F6-73153B20FE4F", "HC OCS Brand Name");
				zTextBoxColumnStyleInfo57.ColumnName = "CA_BrandNameOCS";
				zTextBoxColumnStyleInfo57.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5FACB22A-A9D2-4C1C-8B07-709ACD863214", "PGA HC OCS");
				zTextBoxColumnStyleInfo57.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo57, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo57);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo49 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo49.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|E6C8EDD7-3014-4381-9F7C-88054EB56174", "HC OCS Batch/Lot Number");
				zTextBoxColumnStyleInfo49.ColumnName = "CA_BatchLotNumberOCS";
				zTextBoxColumnStyleInfo49.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5FACB22A-A9D2-4C1C-8B07-709ACD863214", "PGA HC OCS");
				zTextBoxColumnStyleInfo49.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo49, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo49);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo13 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|7C456347-30DA-4A8A-9712-F638D4D06FEA", "HC MDE Indicator");
				zCheckBoxColumnStyleInfo13.ColumnName = "CA_MDEProgramInd";
				zCheckBoxColumnStyleInfo13.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo13, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo13);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo25 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo25.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D754DECB-FF6D-4C36-A6BE-D716952D47EA", "HC MDE Intended Use Code");
				zDropEditColumnStyleInfo25.ColumnName = "CA_IntendedUseCodeMDE";
				zDropEditColumnStyleInfo25.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo25, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo25);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo26 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo26.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4B89D212-67D7-4444-A458-2582724C451B", "HC MDE Commodity Code");
				zDropEditColumnStyleInfo26.ColumnName = "CA_CategoryMDE";
				zDropEditColumnStyleInfo26.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo26, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo26);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B7DF76D6-46F8-45FC-A385-125F11B4D7E8", "HC MDE Manufacture Date");
				zDateEditColumnStyleInfo5.ColumnName = "CA_ProductionDateMDE";
				zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo5, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo36 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo36.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5159F0FA-1388-4D24-A36C-D001E4EFD38E", "HC MDE Unique Device ID");
				zTextBoxColumnStyleInfo36.ColumnName = "CA_UniqueDeviceIDNumber";
				zTextBoxColumnStyleInfo36.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				zTextBoxColumnStyleInfo36.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo36, 90, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo36);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo14 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D73D2605-FF84-4AE7-81F7-3107768BFDCE", "HC Medical Device Establishment License Exemption");
				zCheckBoxColumnStyleInfo14.ColumnName = "CA_MDE_LEX";
				zCheckBoxColumnStyleInfo14.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo14, 230, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo14);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo44 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo44.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B1AE7D10-2F80-4C8F-8A9B-0DA6101ED8F2", "HC MDE GITN No");
				zTextBoxColumnStyleInfo44.ColumnName = "CA_GTINNumberMDE";
				zTextBoxColumnStyleInfo44.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				zTextBoxColumnStyleInfo44.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo44, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo44);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo58 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo58.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|44CE89C0-6357-4DBA-B978-3CC98ECE2032", "HC MDE Brand Name");
				zTextBoxColumnStyleInfo58.ColumnName = "CA_BrandNameMDE";
				zTextBoxColumnStyleInfo58.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				zTextBoxColumnStyleInfo58.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo58, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo58);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo50 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo50.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|59B64B2B-A2F7-471E-9D6A-A9F4ABECDA3C", "HC MDE Batch/Lot Number");
				zTextBoxColumnStyleInfo50.ColumnName = "CA_BatchLotNumberMDE";
				zTextBoxColumnStyleInfo50.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				zTextBoxColumnStyleInfo50.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo50, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo50);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo63 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo63.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|49524187-2B68-405E-8680-EB9F775BD407", "HC MDE Model Name");
				zTextBoxColumnStyleInfo63.ColumnName = "CA_ModelNameMDE";
				zTextBoxColumnStyleInfo63.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|06B4B2B5-5CCA-4528-B7FA-0C99B49F0749", "PGA HC MDE");
				zTextBoxColumnStyleInfo63.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo63, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo63);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo15 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C2A3F7F1-D86F-459F-BAD9-FD9B03721C25", "HC NHP Indicator");
				zCheckBoxColumnStyleInfo15.ColumnName = "CA_NHPProgramInd";
				zCheckBoxColumnStyleInfo15.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|842A4E44-D64F-4B55-B67D-1E574792A4D1", "PGA HC NHP");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo15, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo15);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo27 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo27.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|7789DD09-3455-4036-B4BA-50FDDE5A324E", "HC NHP Intended Use Code");
				zDropEditColumnStyleInfo27.ColumnName = "CA_IntendedUseCodeNHP";
				zDropEditColumnStyleInfo27.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|842A4E44-D64F-4B55-B67D-1E574792A4D1", "PGA HC NHP");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo27, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo27);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo28 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo28.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|1B7557BE-C70C-44D0-B856-5D42521D6BA8", "HC NHP Commodity Code");
				zDropEditColumnStyleInfo28.ColumnName = "CA_CategoryNHP";
				zDropEditColumnStyleInfo28.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|842A4E44-D64F-4B55-B67D-1E574792A4D1", "PGA HC NHP");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo28, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo28);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|080ABC47-D204-495C-8130-DDCFFE2C11AE", "HC NHP Manufacture Date");
				zDateEditColumnStyleInfo6.ColumnName = "CA_ProductionDateNHP";
				zDateEditColumnStyleInfo6.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo6.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|842A4E44-D64F-4B55-B67D-1E574792A4D1", "PGA HC NHP");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo6, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo45 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo45.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|6FAF5CA3-E59B-4728-B238-618BBD4E2C50", "HC NHP GITN No");
				zTextBoxColumnStyleInfo45.ColumnName = "CA_GTINNumberNHP";
				zTextBoxColumnStyleInfo45.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|842A4E44-D64F-4B55-B67D-1E574792A4D1", "PGA HC NHP");
				zTextBoxColumnStyleInfo45.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo45, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo45);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo51 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo51.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|59199314-B513-44CC-833E-95E96E66B680", "HC NHP Batch/Lot Number");
				zTextBoxColumnStyleInfo51.ColumnName = "CA_BatchLotNumberNHP";
				zTextBoxColumnStyleInfo51.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|842A4E44-D64F-4B55-B67D-1E574792A4D1", "PGA HC NHP");
				zTextBoxColumnStyleInfo51.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo51, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo51);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo16 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|5F1DF1B7-5CE1-489C-A32F-8B5FAC5FE096", "HC PES Indicator");
				zCheckBoxColumnStyleInfo16.ColumnName = "CA_PESProgramInd";
				zCheckBoxColumnStyleInfo16.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo16, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo16);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo29 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo29.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4FDBFAAC-FCCE-4AE6-911F-1CF80AF6051E", "HC PES Intended Use Code");
				zDropEditColumnStyleInfo29.ColumnName = "CA_IntendedUseCodePES";
				zDropEditColumnStyleInfo29.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo29, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo29);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo30 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo30.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|95FE720D-36CE-4221-957C-06CF75324E3F", "HC PES Commodity Code");
				zDropEditColumnStyleInfo30.ColumnName = "CA_CategoryPES";
				zDropEditColumnStyleInfo30.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo30, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo30);

				ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
				zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|37DA4808-CE86-428B-BD52-3FB724E98B19", "HC PES Manufacturer");
				zGuidFindBoxColumnStyleInfo5.ColumnName = "CA_ManufacturerOrgPKPES";
				zGuidFindBoxColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zGuidFindBoxColumnStyleInfo5, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);

				ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new ZGuidDropEditColumnStyleInfo();
				zGuidDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|723D1EDC-7A75-4ED7-8889-F25B70CF5666", "HC PES Address");
				zGuidDropEditColumnStyleInfo3.ColumnName = "CA_OA_ManufacturerAddressPES";
				zGuidDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zGuidDropEditColumnStyleInfo3, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|3B895C93-B02A-42CE-B2C4-12056232BFAB", "HC PES Manufacture Date");
				zDateEditColumnStyleInfo7.ColumnName = "CA_ProductionDatePES";
				zDateEditColumnStyleInfo7.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo7.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo7, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo37 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo37.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|681EF53C-C554-4133-B686-1157E80B746C", "HC PES CAS No");
				zTextBoxColumnStyleInfo37.ColumnName = "CA_CASNumber";
				zTextBoxColumnStyleInfo37.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				zTextBoxColumnStyleInfo37.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo37, 90, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo37);

				ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
				zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|2981F95A-2094-4EB9-9EA7-797A8766AA01", "HC PES UNDG Code");
				zGuidFindBoxColumnStyleInfo3.ColumnName = "CA_DangerousGoodsDGSubsPES";
				zGuidFindBoxColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zGuidFindBoxColumnStyleInfo3, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo59 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo59.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|CE44EEBF-8337-4716-B51A-BF1DE2297DD8", "HC PES Brand Name");
				zTextBoxColumnStyleInfo59.ColumnName = "CA_BrandNamePES";
				zTextBoxColumnStyleInfo59.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				zTextBoxColumnStyleInfo59.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo59, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo59);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo52 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo52.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|36FA8C29-8EB0-4A7E-B630-5C86D6B08FFC", "HC PES Batch/Lot Number");
				zTextBoxColumnStyleInfo52.ColumnName = "CA_BatchLotNumberPES";
				zTextBoxColumnStyleInfo52.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				zTextBoxColumnStyleInfo52.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo52, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo52);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo62 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo62.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|07BA54F9-6055-4ABD-86FE-7F3F87A79813", "HC PES Trade Name");
				zTextBoxColumnStyleInfo62.ColumnName = "CA_TradeNamePES";
				zTextBoxColumnStyleInfo62.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				zTextBoxColumnStyleInfo62.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo62, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo62);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo17 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|016337FC-5012-434D-854F-95006B4D7673", "HC PMRA Scheduled Pest Control Products");
				zCheckBoxColumnStyleInfo17.ColumnName = "CA_PES_SPCP";
				zCheckBoxColumnStyleInfo17.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo17, 160, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo17);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo18 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9574DD99-E2BF-49E9-B6F1-6911DE65BACE", "HC PMRA Exempt Control Products");
				zCheckBoxColumnStyleInfo18.ColumnName = "CA_PES_EPCP";
				zCheckBoxColumnStyleInfo18.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|73156C67-193E-4DE9-A4FF-C7C02D12E6A5", "PGA HC PES");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo18, 90, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo18);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo19 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|0701A757-2640-4E40-9449-C233117200DE", "HC RED Indicator");
				zCheckBoxColumnStyleInfo19.ColumnName = "CA_REDProgramInd";
				zCheckBoxColumnStyleInfo19.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|65AF6C8B-C40A-468C-AD13-962BDED47FD5", "PGA HC RED");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo19, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo19);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo31 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo31.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|439B5739-2127-4112-997D-E474F134692F", "HC RED Commodity Code");
				zDropEditColumnStyleInfo31.ColumnName = "CA_CategoryRED";
				zDropEditColumnStyleInfo31.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|65AF6C8B-C40A-468C-AD13-962BDED47FD5", "PGA HC RED");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo31, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo31);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo38 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo38.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|23EB51CB-2DDE-4FEB-89C3-6DF697608AC0", "HC FDA Number");
				zTextBoxColumnStyleInfo38.ColumnName = "CA_FDANumber";
				zTextBoxColumnStyleInfo38.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|65AF6C8B-C40A-468C-AD13-962BDED47FD5", "PGA HC RED");
				zTextBoxColumnStyleInfo38.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo38, 90, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo38);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo64 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo64.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|A9EF8631-D048-40F5-B1E8-1804F4C22182", "HC RED Model Name");
				zTextBoxColumnStyleInfo64.ColumnName = "CA_ModelNameRED";
				zTextBoxColumnStyleInfo64.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|65AF6C8B-C40A-468C-AD13-962BDED47FD5", "PGA HC RED");
				zTextBoxColumnStyleInfo64.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo64, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo64);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo20 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCheckBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|254FE556-451B-4F64-8DA8-5186DB7D0B61", "HC VET Indicator");
				zCheckBoxColumnStyleInfo20.ColumnName = "CA_VETProgramInd";
				zCheckBoxColumnStyleInfo20.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|796EC4E3-1D12-406E-9192-00280D48A6EE", "PGA HC VET");
				ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo20, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo20);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo32 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo32.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|2605972D-D30E-4241-821B-E7A94FAE45B8", "HC VET Intended Use Code");
				zDropEditColumnStyleInfo32.ColumnName = "CA_IntendedUseCodeVET";
				zDropEditColumnStyleInfo32.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|796EC4E3-1D12-406E-9192-00280D48A6EE", "PGA HC VET");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo32, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo32);

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo33 = new ZDropEditColumnStyleInfo();
				zDropEditColumnStyleInfo33.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|A75D33B9-B4BF-4D4A-8740-070DF2823ED7", "HC VET Commodity Code");
				zDropEditColumnStyleInfo33.ColumnName = "CA_CategoryVET";
				zDropEditColumnStyleInfo33.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|796EC4E3-1D12-406E-9192-00280D48A6EE", "PGA HC VET");
				ControlDpiScalingHelper.SetWidth(ref zDropEditColumnStyleInfo33, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo33);

				ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZArchitecture.ZDateEditColumnStyleInfo();
				zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|6D82670B-F0CC-4709-9EF2-8FEB5A79C719", "HC VET Manufacture Date");
				zDateEditColumnStyleInfo8.ColumnName = "CA_ProductionDateVET";
				zDateEditColumnStyleInfo8.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
				zDateEditColumnStyleInfo8.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|796EC4E3-1D12-406E-9192-00280D48A6EE", "PGA HC VET");
				ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo8, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo46 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo46.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|DA42C91E-FE85-4744-8CC0-F4C27FEE79B9", "HC VET GITN No");
				zTextBoxColumnStyleInfo46.ColumnName = "CA_GTINNumberVET";
				zTextBoxColumnStyleInfo46.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|796EC4E3-1D12-406E-9192-00280D48A6EE", "PGA HC VET");
				zTextBoxColumnStyleInfo46.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo46, 110, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo46);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo60 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo60.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|0CAD19E0-21B9-44B4-8240-4E07CCA4F7B3", "HC VET Brand Name");
				zTextBoxColumnStyleInfo60.ColumnName = "CA_BrandNameVET";
				zTextBoxColumnStyleInfo60.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|796EC4E3-1D12-406E-9192-00280D48A6EE", "PGA HC VET");
				zTextBoxColumnStyleInfo60.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo60, 120, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo60);

				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo53 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo53.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|69C112E2-040F-43AE-8C2B-0B59144D1C0B", "HC VET Batch/Lot Number");
				zTextBoxColumnStyleInfo53.ColumnName = "CA_BatchLotNumberVET";
				zTextBoxColumnStyleInfo53.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|796EC4E3-1D12-406E-9192-00280D48A6EE", "PGA HC VET");
				zTextBoxColumnStyleInfo53.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
				ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo53, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo53);

				ZArchitecture.ZTextBoxColumnStyleInfo zADDDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zADDDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_ADD_Description";
				zADDDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|EB353699-A294-4E07-AD2A-50DAB36CD93C", "Duty & Tax ADD");
				ControlDpiScalingHelper.SetWidth(ref zADDDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zADDDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zADDDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zADDDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_ADD_ExemptCode";
				zADDDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|EB353699-A294-4E07-AD2A-50DAB36CD93C", "Duty & Tax ADD");
				ControlDpiScalingHelper.SetWidth(ref zADDDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zADDDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zADDDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zADDDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_ADD_Code";
				zADDDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|EB353699-A294-4E07-AD2A-50DAB36CD93C", "Duty & Tax ADD");
				ControlDpiScalingHelper.SetWidth(ref zADDDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zADDDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zADDDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zADDDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_ADD_Override";
				zADDDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|EB353699-A294-4E07-AD2A-50DAB36CD93C", "Duty & Tax ADD");
				ControlDpiScalingHelper.SetWidth(ref zADDDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zADDDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zADDDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zADDDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_ADD_Amount";
				zADDDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|EB353699-A294-4E07-AD2A-50DAB36CD93C", "Duty & Tax ADD");
				ControlDpiScalingHelper.SetWidth(ref zADDDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zADDDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zADDDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zADDDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_ADD_Rate";
				zADDDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|EB353699-A294-4E07-AD2A-50DAB36CD93C", "Duty & Tax ADD");
				ControlDpiScalingHelper.SetWidth(ref zADDDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zADDDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zADDDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zADDDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_ADD_RateType";
				zADDDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|EB353699-A294-4E07-AD2A-50DAB36CD93C", "Duty & Tax ADD");
				ControlDpiScalingHelper.SetWidth(ref zADDDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zADDDutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zCPTDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zCPTDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_CPT_Description";
				zCPTDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B28247FD-3D35-4E3D-A53E-45C5CAC34338", "Duty & Tax CPT");
				ControlDpiScalingHelper.SetWidth(ref zCPTDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCPTDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zCPTDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCPTDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_CPT_ExemptCode";
				zCPTDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B28247FD-3D35-4E3D-A53E-45C5CAC34338", "Duty & Tax CPT");
				ControlDpiScalingHelper.SetWidth(ref zCPTDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCPTDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zCPTDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCPTDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_CPT_Code";
				zCPTDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B28247FD-3D35-4E3D-A53E-45C5CAC34338", "Duty & Tax CPT");
				ControlDpiScalingHelper.SetWidth(ref zCPTDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCPTDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCPTDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCPTDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_CPT_Override";
				zCPTDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B28247FD-3D35-4E3D-A53E-45C5CAC34338", "Duty & Tax CPT");
				ControlDpiScalingHelper.SetWidth(ref zCPTDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCPTDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zCPTDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCPTDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_CPT_Amount";
				zCPTDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B28247FD-3D35-4E3D-A53E-45C5CAC34338", "Duty & Tax CPT");
				ControlDpiScalingHelper.SetWidth(ref zCPTDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCPTDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zCPTDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCPTDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_CPT_Rate";
				zCPTDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B28247FD-3D35-4E3D-A53E-45C5CAC34338", "Duty & Tax CPT");
				ControlDpiScalingHelper.SetWidth(ref zCPTDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCPTDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zCPTDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCPTDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_CPT_RateType";
				zCPTDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|B28247FD-3D35-4E3D-A53E-45C5CAC34338", "Duty & Tax CPT");
				ControlDpiScalingHelper.SetWidth(ref zCPTDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCPTDutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zCTADutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zCTADutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_CTA_Description";
				zCTADutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C2D36C07-2DC9-4FA9-A219-F2C342C1F74E", "Duty & Tax CTA");
				ControlDpiScalingHelper.SetWidth(ref zCTADutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCTADutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zCTADutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCTADutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_CTA_ExemptCode";
				zCTADutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C2D36C07-2DC9-4FA9-A219-F2C342C1F74E", "Duty & Tax CTA");
				ControlDpiScalingHelper.SetWidth(ref zCTADutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCTADutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zCTADutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCTADutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_CTA_Code";
				zCTADutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C2D36C07-2DC9-4FA9-A219-F2C342C1F74E", "Duty & Tax CTA");
				ControlDpiScalingHelper.SetWidth(ref zCTADutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCTADutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCTADutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCTADutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_CTA_Override";
				zCTADutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C2D36C07-2DC9-4FA9-A219-F2C342C1F74E", "Duty & Tax CTA");
				ControlDpiScalingHelper.SetWidth(ref zCTADutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCTADutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zCTADutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCTADutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_CTA_Amount";
				zCTADutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C2D36C07-2DC9-4FA9-A219-F2C342C1F74E", "Duty & Tax CTA");
				ControlDpiScalingHelper.SetWidth(ref zCTADutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCTADutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zCTADutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCTADutyAndTaxRateColumnStyleInfo.ColumnName = "CA_CTA_Rate";
				zCTADutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C2D36C07-2DC9-4FA9-A219-F2C342C1F74E", "Duty & Tax CTA");
				ControlDpiScalingHelper.SetWidth(ref zCTADutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCTADutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zCTADutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCTADutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_CTA_RateType";
				zCTADutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|C2D36C07-2DC9-4FA9-A219-F2C342C1F74E", "Duty & Tax CTA");
				ControlDpiScalingHelper.SetWidth(ref zCTADutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCTADutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zEXCDTYDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zEXCDTYDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_EXCDTY_Description";
				zEXCDTYDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|473D6F36-55B4-465E-987B-CF88EF79B0C9", "Duty & Tax Excise DTY");
				ControlDpiScalingHelper.SetWidth(ref zEXCDTYDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXCDTYDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zEXCDTYDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zEXCDTYDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_EXCDTY_ExemptCode";
				zEXCDTYDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|473D6F36-55B4-465E-987B-CF88EF79B0C9", "Duty & Tax Excise DTY");
				ControlDpiScalingHelper.SetWidth(ref zEXCDTYDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXCDTYDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zEXCDTYDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zEXCDTYDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_EXCDTY_Code";
				zEXCDTYDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|473D6F36-55B4-465E-987B-CF88EF79B0C9", "Duty & Tax Excise DTY");
				ControlDpiScalingHelper.SetWidth(ref zEXCDTYDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXCDTYDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zEXCDTYDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zEXCDTYDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_EXCDTY_Override";
				zEXCDTYDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|473D6F36-55B4-465E-987B-CF88EF79B0C9", "Duty & Tax Excise DTY");
				ControlDpiScalingHelper.SetWidth(ref zEXCDTYDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXCDTYDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zEXCDTYDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zEXCDTYDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_EXCDTY_Amount";
				zEXCDTYDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|473D6F36-55B4-465E-987B-CF88EF79B0C9", "Duty & Tax Excise DTY");
				ControlDpiScalingHelper.SetWidth(ref zEXCDTYDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXCDTYDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zEXCDTYDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zEXCDTYDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_EXCDTY_Rate";
				zEXCDTYDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|473D6F36-55B4-465E-987B-CF88EF79B0C9", "Duty & Tax Excise DTY");
				ControlDpiScalingHelper.SetWidth(ref zEXCDTYDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXCDTYDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zEXCDTYDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zEXCDTYDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_EXCDTY_RateType";
				zEXCDTYDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|473D6F36-55B4-465E-987B-CF88EF79B0C9", "Duty & Tax Excise DTY");
				ControlDpiScalingHelper.SetWidth(ref zEXCDTYDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXCDTYDutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zCLSDTYDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zCLSDTYDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_CLSDTY_Description";
				zCLSDTYDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D0BACE20-0513-4DC6-8F31-8FDDCCC2E9A7", "Duty & Tax Classification DTY");
				ControlDpiScalingHelper.SetWidth(ref zCLSDTYDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCLSDTYDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zCLSDTYDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCLSDTYDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_CLSDTY_ExemptCode";
				zCLSDTYDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D0BACE20-0513-4DC6-8F31-8FDDCCC2E9A7", "Duty & Tax Classification DTY");
				ControlDpiScalingHelper.SetWidth(ref zCLSDTYDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCLSDTYDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zCLSDTYDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCLSDTYDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_CLSDTY_Code";
				zCLSDTYDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D0BACE20-0513-4DC6-8F31-8FDDCCC2E9A7", "Duty & Tax Classification DTY");
				ControlDpiScalingHelper.SetWidth(ref zCLSDTYDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCLSDTYDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCLSDTYDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCLSDTYDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_CLSDTY_Override";
				zCLSDTYDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D0BACE20-0513-4DC6-8F31-8FDDCCC2E9A7", "Duty & Tax Classification DTY");
				ControlDpiScalingHelper.SetWidth(ref zCLSDTYDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCLSDTYDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zCLSDTYDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCLSDTYDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_CLSDTY_Amount";
				zCLSDTYDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D0BACE20-0513-4DC6-8F31-8FDDCCC2E9A7", "Duty & Tax Classification DTY");
				ControlDpiScalingHelper.SetWidth(ref zCLSDTYDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCLSDTYDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zCLSDTYDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCLSDTYDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_CLSDTY_Rate";
				zCLSDTYDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D0BACE20-0513-4DC6-8F31-8FDDCCC2E9A7", "Duty & Tax Classification DTY");
				ControlDpiScalingHelper.SetWidth(ref zCLSDTYDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCLSDTYDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zCLSDTYDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCLSDTYDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_CLSDTY_RateType";
				zCLSDTYDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D0BACE20-0513-4DC6-8F31-8FDDCCC2E9A7", "Duty & Tax Classification DTY");
				ControlDpiScalingHelper.SetWidth(ref zCLSDTYDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCLSDTYDutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zCVDDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zCVDDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_CVD_Description";
				zCVDDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|875AA1AD-40D5-4219-8267-B703215C0A77", "Duty & Tax CVD");
				ControlDpiScalingHelper.SetWidth(ref zCVDDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCVDDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zCVDDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCVDDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_CVD_ExemptCode";
				zCVDDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|875AA1AD-40D5-4219-8267-B703215C0A77", "Duty & Tax CVD");
				ControlDpiScalingHelper.SetWidth(ref zCVDDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCVDDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zCVDDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCVDDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_CVD_Code";
				zCVDDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|875AA1AD-40D5-4219-8267-B703215C0A77", "Duty & Tax CVD");
				ControlDpiScalingHelper.SetWidth(ref zCVDDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCVDDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zCVDDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zCVDDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_CVD_Override";
				zCVDDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|875AA1AD-40D5-4219-8267-B703215C0A77", "Duty & Tax CVD");
				ControlDpiScalingHelper.SetWidth(ref zCVDDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCVDDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zCVDDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCVDDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_CVD_Amount";
				zCVDDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|875AA1AD-40D5-4219-8267-B703215C0A77", "Duty & Tax CVD");
				ControlDpiScalingHelper.SetWidth(ref zCVDDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCVDDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zCVDDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCVDDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_CVD_Rate";
				zCVDDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|875AA1AD-40D5-4219-8267-B703215C0A77", "Duty & Tax CVD");
				ControlDpiScalingHelper.SetWidth(ref zCVDDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCVDDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zCVDDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zCVDDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_CVD_RateType";
				zCVDDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|875AA1AD-40D5-4219-8267-B703215C0A77", "Duty & Tax CVD");
				ControlDpiScalingHelper.SetWidth(ref zCVDDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCVDDutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zEXSDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zEXSDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_EXS_Description";
				zEXSDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D48DF921-2979-4CC3-83EF-93B256EFC1CB", "Duty & Tax EXS");
				ControlDpiScalingHelper.SetWidth(ref zEXSDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXSDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zEXSDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zEXSDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_EXS_ExemptCode";
				zEXSDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D48DF921-2979-4CC3-83EF-93B256EFC1CB", "Duty & Tax EXS");
				ControlDpiScalingHelper.SetWidth(ref zEXSDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXSDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zEXSDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zEXSDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_EXS_Code";
				zEXSDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D48DF921-2979-4CC3-83EF-93B256EFC1CB", "Duty & Tax EXS");
				ControlDpiScalingHelper.SetWidth(ref zEXSDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXSDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zEXSDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zEXSDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_EXS_Override";
				zEXSDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D48DF921-2979-4CC3-83EF-93B256EFC1CB", "Duty & Tax EXS");
				ControlDpiScalingHelper.SetWidth(ref zEXSDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXSDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zEXSDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zEXSDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_EXS_Amount";
				zEXSDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D48DF921-2979-4CC3-83EF-93B256EFC1CB", "Duty & Tax EXS");
				ControlDpiScalingHelper.SetWidth(ref zEXSDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXSDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zEXSDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zEXSDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_EXS_Rate";
				zEXSDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D48DF921-2979-4CC3-83EF-93B256EFC1CB", "Duty & Tax EXS");
				ControlDpiScalingHelper.SetWidth(ref zEXSDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXSDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zEXSDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zEXSDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_EXS_RateType";
				zEXSDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|D48DF921-2979-4CC3-83EF-93B256EFC1CB", "Duty & Tax EXS");
				ControlDpiScalingHelper.SetWidth(ref zEXSDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zEXSDutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zGSTDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zGSTDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_GST_Description";
				zGSTDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4A769391-8496-468D-AB36-01F9863D34E0", "Duty & Tax GST");
				ControlDpiScalingHelper.SetWidth(ref zGSTDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGSTDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zGSTDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zGSTDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_GST_ExemptCode";
				zGSTDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4A769391-8496-468D-AB36-01F9863D34E0", "Duty & Tax GST");
				ControlDpiScalingHelper.SetWidth(ref zGSTDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGSTDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zGSTDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zGSTDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_GST_Code";
				zGSTDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4A769391-8496-468D-AB36-01F9863D34E0", "Duty & Tax GST");
				ControlDpiScalingHelper.SetWidth(ref zGSTDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGSTDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zGSTDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zGSTDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_GST_Override";
				zGSTDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4A769391-8496-468D-AB36-01F9863D34E0", "Duty & Tax GST");
				ControlDpiScalingHelper.SetWidth(ref zGSTDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGSTDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zGSTDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zGSTDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_GST_Amount";
				zGSTDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4A769391-8496-468D-AB36-01F9863D34E0", "Duty & Tax GST");
				ControlDpiScalingHelper.SetWidth(ref zGSTDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGSTDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zGSTDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zGSTDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_GST_Rate";
				zGSTDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4A769391-8496-468D-AB36-01F9863D34E0", "Duty & Tax GST");
				ControlDpiScalingHelper.SetWidth(ref zGSTDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGSTDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zGSTDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zGSTDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_GST_RateType";
				zGSTDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|4A769391-8496-468D-AB36-01F9863D34E0", "Duty & Tax GST");
				ControlDpiScalingHelper.SetWidth(ref zGSTDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGSTDutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zSAFDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zSAFDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_SAF_Description";
				zSAFDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|25F6A4F6-BD6A-441E-9E5D-30B1EF816A07", "Duty & Tax SAF");
				ControlDpiScalingHelper.SetWidth(ref zSAFDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSAFDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zSAFDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zSAFDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_SAF_ExemptCode";
				zSAFDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|25F6A4F6-BD6A-441E-9E5D-30B1EF816A07", "Duty & Tax SAF");
				ControlDpiScalingHelper.SetWidth(ref zSAFDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSAFDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zSAFDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zSAFDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_SAF_Code";
				zSAFDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|25F6A4F6-BD6A-441E-9E5D-30B1EF816A07", "Duty & Tax SAF");
				ControlDpiScalingHelper.SetWidth(ref zSAFDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSAFDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zSAFDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zSAFDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_SAF_Override";
				zSAFDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|25F6A4F6-BD6A-441E-9E5D-30B1EF816A07", "Duty & Tax SAF");
				ControlDpiScalingHelper.SetWidth(ref zSAFDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSAFDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zSAFDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zSAFDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_SAF_Amount";
				zSAFDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|25F6A4F6-BD6A-441E-9E5D-30B1EF816A07", "Duty & Tax SAF");
				ControlDpiScalingHelper.SetWidth(ref zSAFDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSAFDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zSAFDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zSAFDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_SAF_Rate";
				zSAFDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|25F6A4F6-BD6A-441E-9E5D-30B1EF816A07", "Duty & Tax SAF");
				ControlDpiScalingHelper.SetWidth(ref zSAFDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSAFDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zSAFDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zSAFDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_SAF_RateType";
				zSAFDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|25F6A4F6-BD6A-441E-9E5D-30B1EF816A07", "Duty & Tax SAF");
				ControlDpiScalingHelper.SetWidth(ref zSAFDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSAFDutyAndTaxRateTypeColumnStyleInfo);

				ZArchitecture.ZTextBoxColumnStyleInfo zSURDutyAndTaxDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zSURDutyAndTaxDescriptionColumnStyleInfo.ColumnName = "CA_SUR_Description";
				zSURDutyAndTaxDescriptionColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9DBA24C7-6F22-41B8-9427-86AB1A01F644", "Duty & Tax SUR");
				ControlDpiScalingHelper.SetWidth(ref zSURDutyAndTaxDescriptionColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSURDutyAndTaxDescriptionColumnStyleInfo);

				ZDropEditColumnStyleInfo zSURDutyAndTaxExemptCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zSURDutyAndTaxExemptCodeColumnStyleInfo.ColumnName = "CA_SUR_ExemptCode";
				zSURDutyAndTaxExemptCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9DBA24C7-6F22-41B8-9427-86AB1A01F644", "Duty & Tax SUR");
				ControlDpiScalingHelper.SetWidth(ref zSURDutyAndTaxExemptCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSURDutyAndTaxExemptCodeColumnStyleInfo);

				ZDropEditColumnStyleInfo zSURDutyAndTaxCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zSURDutyAndTaxCodeColumnStyleInfo.ColumnName = "CA_SUR_Code";
				zSURDutyAndTaxCodeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9DBA24C7-6F22-41B8-9427-86AB1A01F644", "Duty & Tax SUR");
				ControlDpiScalingHelper.SetWidth(ref zSURDutyAndTaxCodeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSURDutyAndTaxCodeColumnStyleInfo);

				ZArchitecture.ZCheckBoxColumnStyleInfo zSURDutyAndTaxOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				zSURDutyAndTaxOverrideColumnStyleInfo.ColumnName = "CA_SUR_Override";
				zSURDutyAndTaxOverrideColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9DBA24C7-6F22-41B8-9427-86AB1A01F644", "Duty & Tax SUR");
				ControlDpiScalingHelper.SetWidth(ref zSURDutyAndTaxOverrideColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSURDutyAndTaxOverrideColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zSURDutyAndTaxAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zSURDutyAndTaxAmountColumnStyleInfo.ColumnName = "CA_SUR_Amount";
				zSURDutyAndTaxAmountColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9DBA24C7-6F22-41B8-9427-86AB1A01F644", "Duty & Tax SUR");
				ControlDpiScalingHelper.SetWidth(ref zSURDutyAndTaxAmountColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSURDutyAndTaxAmountColumnStyleInfo);

				ZArchitecture.ZCalcEditColumnStyleInfo zSURDutyAndTaxRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zSURDutyAndTaxRateColumnStyleInfo.ColumnName = "CA_SUR_Rate";
				zSURDutyAndTaxRateColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9DBA24C7-6F22-41B8-9427-86AB1A01F644", "Duty & Tax SUR");
				ControlDpiScalingHelper.SetWidth(ref zSURDutyAndTaxRateColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSURDutyAndTaxRateColumnStyleInfo);

				ZDropEditColumnStyleInfo zSURDutyAndTaxRateTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
				zSURDutyAndTaxRateTypeColumnStyleInfo.ColumnName = "CA_SUR_RateType";
				zSURDutyAndTaxRateTypeColumnStyleInfo.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("CAImportInvoiceLineUserControl|9DBA24C7-6F22-41B8-9427-86AB1A01F644", "Duty & Tax SUR");
				ControlDpiScalingHelper.SetWidth(ref zSURDutyAndTaxRateTypeColumnStyleInfo, 150, true);
				this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zSURDutyAndTaxRateTypeColumnStyleInfo);
			}
		}

		string[] InvoiceLineDefaultColumnsSequence
		{
			get
			{
				if (invoiceLineDefaultColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						JobComInvoiceLine.Schema.JI_LineNo,
						JobComInvoiceLine.Schema.JI_Calc_Invoice,
						JobComInvoiceLine.Schema.CA_PageNumber,
						JobComInvoiceLine.Schema.CA_PageRelativeLineNumber,
						JobComInvoiceLine.Schema.JI_B3LineNumber,
						JobComInvoiceLine.Schema.JI_PartNo,
						JobComInvoiceLine.Schema.JI_CC,
						JobComInvoiceLine.Schema.JI_FormattedTariff,
						JobComInvoiceLine.Schema.CA_99TariffCode,
						JobComInvoiceLine.Schema.JI_InvoiceQuantity,
						JobComInvoiceLine.Schema.JI_InvoiceUQ,
						JobComInvoiceLine.Schema.JI_CustomsQuantity,
						JobComInvoiceLine.Schema.JI_CustomsUnitQty,
						JobComInvoiceLine.Schema.JI_LinePrice,
						JobComInvoiceLine.Schema.JI_Description,
						JobComInvoiceLine.Schema.CA_TreatmentCode,
						JobComInvoiceLine.Schema.CA_ValueForDutyCode,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin,
						JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin,
						JobComInvoiceLine.Schema.JI_PreviousEntryNumber,
						JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,
						JobComInvoiceLine.Schema.DangerousGoodsDGSubs,
						JobComInvoiceLine.Schema.JI_MatchingKey,
					};
					SetupIIDColumns(JobDeclaration.IsIID);
					invoiceLineDefaultColumnsSequence = columnList.ToArray();
				}
				return invoiceLineDefaultColumnsSequence;
			}
		}
		string[] invoiceLineDefaultColumnsSequence;

		string[] IIDColumns
		{
			get
			{
				if (iidColumns == null)
				{
					iidColumns = new[]
					{
						JobComInvoiceLine.Schema.ConsigneePK,
						JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress,
						JobComInvoiceLine.Schema.CA_CFIAAllProgramInd,
						JobComInvoiceLine.Schema.CA_RN_NKCountryOfSourceCFIA,
						JobComInvoiceLine.Schema.CA_AIRSEndUseCFIA,
						JobComInvoiceLine.Schema.CA_AIRSExtensionCodeCFIA,
						JobComInvoiceLine.Schema.CA_DeliveryLocationCFIA,
						JobComInvoiceLine.Schema.CA_OA_ConsigneeAddressCFIA,
						JobComInvoiceLine.Schema.CA_RW_NKSourceStateCFIA,
						JobComInvoiceLine.Schema.CA_AIRSMiscellaneousCFIA,
						JobComInvoiceLine.Schema.CA_APIProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeAPI,
						JobComInvoiceLine.Schema.CA_CategoryAPI,
						JobComInvoiceLine.Schema.CA_BrandNameAPI,
						JobComInvoiceLine.Schema.CA_ProductionDate,
						JobComInvoiceLine.Schema.CA_GTINNumber,
						JobComInvoiceLine.Schema.CA_BatchLotNumber,
						JobComInvoiceLine.Schema.CA_BBCProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeBBC,
						JobComInvoiceLine.Schema.CA_CategoryBBC,
						JobComInvoiceLine.Schema.CA_ExpiryDate,
						JobComInvoiceLine.Schema.CA_CTOProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeCTO,
						JobComInvoiceLine.Schema.CA_CategoryCTO,
						JobComInvoiceLine.Schema.CA_CTO_LCO,
						JobComInvoiceLine.Schema.CA_CPRProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeCPR,
						JobComInvoiceLine.Schema.CA_CategoryCPR,
						JobComInvoiceLine.Schema.CA_ManufacturerOrgPKCPR,
						JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressCPR,
						JobComInvoiceLine.Schema.CA_CategoryCPR,
						JobComInvoiceLine.Schema.CA_BrandNameCPR,
						JobComInvoiceLine.Schema.CA_TradeNameCPR,
						JobComInvoiceLine.Schema.CA_DSEProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeDSE,
						JobComInvoiceLine.Schema.CA_CategoryDSE,
						JobComInvoiceLine.Schema.CA_ComplianceStatement,
						JobComInvoiceLine.Schema.CA_HDRProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeHDR,
						JobComInvoiceLine.Schema.CA_CategoryHDR,
						JobComInvoiceLine.Schema.CA_BrandNameHDR,
						JobComInvoiceLine.Schema.CA_OCSProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeOCS,
						JobComInvoiceLine.Schema.CA_CategoryOCS,
						JobComInvoiceLine.Schema.CA_ManufacturerOrgPKOCS,
						JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressOCS,
						JobComInvoiceLine.Schema.CA_BrandNameOCS,
						JobComInvoiceLine.Schema.CA_MDEProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeMDE,
						JobComInvoiceLine.Schema.CA_CategoryMDE,
						JobComInvoiceLine.Schema.CA_BrandNameMDE,
						JobComInvoiceLine.Schema.CA_ModelNameMDE,
						JobComInvoiceLine.Schema.CA_UniqueDeviceIDNumber,
						JobComInvoiceLine.Schema.CA_MDE_LEX,
						JobComInvoiceLine.Schema.CA_NHPProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeNHP,
						JobComInvoiceLine.Schema.CA_CategoryNHP,
						JobComInvoiceLine.Schema.CA_PESProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodePES,
						JobComInvoiceLine.Schema.CA_CategoryPES,
						JobComInvoiceLine.Schema.CA_ManufacturerOrgPKPES,
						JobComInvoiceLine.Schema.CA_OA_ManufacturerAddressPES,
						JobComInvoiceLine.Schema.CA_CASNumber,
						JobComInvoiceLine.Schema.CA_DangerousGoodsDGSubsPES,
						JobComInvoiceLine.Schema.CA_BrandNamePES,
						JobComInvoiceLine.Schema.CA_TradeNamePES,
						JobComInvoiceLine.Schema.CA_PES_SPCP,
						JobComInvoiceLine.Schema.CA_PES_EPCP,
						JobComInvoiceLine.Schema.CA_REDProgramInd,
						JobComInvoiceLine.Schema.CA_CategoryRED,
						JobComInvoiceLine.Schema.CA_FDANumber,
						JobComInvoiceLine.Schema.CA_ModelNameRED,
						JobComInvoiceLine.Schema.CA_VETProgramInd,
						JobComInvoiceLine.Schema.CA_IntendedUseCodeVET,
						JobComInvoiceLine.Schema.CA_CategoryVET,
						JobComInvoiceLine.Schema.CA_BrandNameVET,
					};
				}

				return iidColumns;
			}
		}
		string[] iidColumns;

		#endregion

		#endregion

		#region Events

		protected override void CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			base.CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged(sender, e);
			SetPGATabPageVisible();
			pgaTabCollection.Update(CurrentInvoiceLine?.PGARequirements);
		}

		protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.HookInvoiceLineEvents(baseInvoiceLine);
			if (baseInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.OnRefreshSIMAMeasureEvent += delegate
				{
					var simaMeasuresForm = new SIMADumpingNumberForm(invoiceLine.SIMAMeasures, invoiceLine.JI_Tariff);
					var parentForm = FindForm();
					simaMeasuresForm.Icon = parentForm.Icon;
					if (simaMeasuresForm.ShowDialog(parentForm) == DialogResult.OK)
					{
						return simaMeasuresForm.SelectedDumpingNumber;
					}

					return null;
				};

				invoiceLine.ShouldDeleteLuxuryTaxInvoiceLine += delegate
				{
					var confirmationMessage = Res.GetString("75E5FDD6-40FA-4291-B7A9-072C0BC48CA5", "Making this change will delete the corresponding luxury tax invoice line. Do you want to continue?");
					return Globals.Message.Show(confirmationMessage, "Do you want to proceed?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
				};
			}
		}

		protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.UnHookInvoiceLineEvents(baseInvoiceLine);

			if (baseInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.OnRefreshSIMAMeasureEvent = null;
				invoiceLine.ShouldDeleteLuxuryTaxInvoiceLine = null;
			}
		}

		void CA_OGDTCInfo_ValueChanged(object sender, EventArgs e)
		{
			SetTiresVisibility();
		}

		void CA_OGDNRInfo_ValueChanged(object sender, EventArgs e)
		{
			SetNRCANVisibility();
		}

		void CA_OGDICInfo_ValueChanged(object sender, EventArgs e)
		{
			SetSITTVisibility();
		}

		void CA_OGDCFIAInfo_ValueChanged(object sender, EventArgs e)
		{
			SetCFIAVisibility();
		}

		void Declaration_OnApportionmentDirtyChanged()
		{
			DutyAndTaxGridGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = JobDeclaration.ApportionmentDirty
				? Res.GetString("03a675fe-084f-4f72-8115-826b3810ed93", "Apportionment of Line Duties and Taxes is pending. All not overridden duties and taxes will be recalculated on SAVE.")
				: Res.GetString("626f49f6-67de-48db-a203-1b2140391b64", "Line Duties and Taxes");
		}

		void CustomsInvoiceLinesBoundGrid_SelectIndexChanged(object sender, EventArgs e)
		{
			if (JobDeclaration.IsIM2 && CurrentInvoiceLine != null && CurrentInvoiceLine.JI_LineNo > JobDeclaration.InvoiceLines.Count)
			{
				Globals.Message.ShowError(Res.GetString("0890094B-AA18-4F01-95E2-8173291C8D0D", "Add invoice line is not allowed on IM2 declaration, please right click to select 'Copy to New Row' instead."), Res.GetString("A93B2185-D44E-42A4-AC79-9E22A150C0CA", "Error in Declaration."));
				CurrentInvoiceLine.Delete();
			}
		}

		void CustomsInvoiceLinesBoundGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			var invoiceLine = (JobComInvoiceLine)e.ObjectAtRow;
			if (invoiceLine != null)
			{
				switch (invoiceLine.CA_OGDStatus)
				{
					case AVSStatusList.Codes.NotImport:
						e.Colour = Color.Red;
						break;
					case AVSStatusList.Codes.NotValidated:
					case AVSStatusList.Codes.Error:
					case AVSStatusList.Codes.Rejected:
					case AVSStatusList.Codes.Unknown:
						e.Colour = Color.LightBlue;
						break;
					case AVSStatusList.Codes.InspectionRequired:
					case AVSStatusList.Codes.ReviewRequired:
						e.Colour = Color.Yellow;
						break;
				}
			}
		}

		#endregion

		#region Set Tab Page Visibility

		void SetTiresVisibility()
		{
			TiresPanel.Visible = JobDeclaration.CA_OGDTC;
			TiresUnavailableLabel.Visible = !TiresPanel.Visible;
		}

		void SetNRCANVisibility()
		{
			NRCANPanel.Visible = JobDeclaration.CA_OGDNR;
			NRCANUnavailableLabel.Visible = !NRCANPanel.Visible;
		}

		void SetSITTVisibility()
		{
			SITTPanel.Visible = JobDeclaration.CA_OGDIC;
			SITTUnavailableLabel.Visible = !SITTPanel.Visible;
		}

		void SetCFIAVisibility()
		{
			CFIAPanel.Visible = JobDeclaration.CA_OGDCFIA;
			CFIAUnavailableLabel.Visible = !CFIAPanel.Visible;
		}

		#endregion

		#region Disposing

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CustomsInvoiceLinesBoundGrid != null)
				{
					CustomsInvoiceLinesBoundGrid.ColourDeciding -= CustomsInvoiceLinesBoundGrid_ColourDeciding;
				}

				var jobDeclaration = JobDeclaration;
				if (jobDeclaration != null)
				{
					jobDeclaration.CA_OGDCFIAInfo.ValueChanged -= CA_OGDCFIAInfo_ValueChanged;
					jobDeclaration.CA_OGDICInfo.ValueChanged -= CA_OGDICInfo_ValueChanged;
					jobDeclaration.CA_OGDNRInfo.ValueChanged -= CA_OGDNRInfo_ValueChanged;
					jobDeclaration.CA_OGDTCInfo.ValueChanged -= CA_OGDTCInfo_ValueChanged;
					jobDeclaration.OnApportionmentDirtyChanged -= Declaration_OnApportionmentDirtyChanged;
					if (jobDeclaration.IsIM2 && CustomsInvoiceLinesBoundGrid != null)
					{
						CustomsInvoiceLinesBoundGrid.SelectedRowsChangedInMouseDown -= CustomsInvoiceLinesBoundGrid_SelectIndexChanged;
					}
				}
				if (pgaTabCollection != null)
				{
					pgaTabCollection.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
