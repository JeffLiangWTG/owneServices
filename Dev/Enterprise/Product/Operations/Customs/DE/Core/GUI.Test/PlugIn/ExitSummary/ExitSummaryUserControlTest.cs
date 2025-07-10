using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using ExitSummaryUserControl = Enterprise.Customs.DE.GUI.PlugIn.ExitSummaryUserControl;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExitSummaryUserControlTest : ExitSummaryUserControlForVirtualPropertiesTest<ExitSummaryUserControl>
	{
		public void TestControls()
		{
			using (var control = new ExitSummaryUserControl())
			{
				CombineAssertions(() =>
				{
					var splitContainer = control.FindSingle<KSplitContainer>(c => c.Name == "SplitContainer");
					AssertNoExceptionThrown("TopPanel", () => splitContainer.Panel1.FindSingle<ZPanel>(c => c.Name == "TopPanel"));
					AssertNoExceptionThrown("MovementGroupBox", () => splitContainer.Panel1.FindSingle<ZGroupBox>(c => c.Name == "MovementGroupBox"));

					var tabControl = splitContainer.Panel2.FindSingle<ZTabControl>(c => c.Name == "TabControl");
					AssertArrayEqualsByElements("AllTabPages", new[] { "Details", "Additional Documents", "Messages" }, tabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.CaptionResourceString.Caption).ToArray());

					var addDocsTabPage = tabControl.FindSingle<ZTabPage>("AdditionalDocumentsTabPage");
					var addInfosGrid = addDocsTabPage.FindSingle<ZGrid>("AdditionalInfosGrid");
					AssertEquals("AddInfoControl Binding member", "CusExitDetails.AdditionalInfos", addInfosGrid.GetBindingMember());
				});
			}
		}

		public void TestControls_Excluded()
		{
			using (var control = new ExitSummaryUserControl())
			{
				CombineAssertions(() =>
				{
					var topPanel = control.FindSingle<ZPanel>(c => c.Name == "TopPanel");
					AssertNull("HeaderArrivalNotificationPlaceTextBox", topPanel.FindSingleOrDefault<ZTextBox>(c => c.Name == "HeaderArrivalNotificationPlaceTextBox"));

					var movementDetailsPanel = control.FindSingle<ZPanel>(c => c.Name == "MovementDetailsPanel");
					AssertNull("ArrivalNotificationPlaceTextBox", movementDetailsPanel.FindSingleOrDefault<ZTextBox>(c => c.Name == "ArrivalNotificationPlaceTextBox"));
				});
			}
		}

		public void TestAdditionalDocumentsUserControl()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertNotNull("Additional Documents", control.FindSingleOrDefault<AdditionalInfosUserControlWithGrid>("AdditionalDocumentsUserControl"));
			}
		}

		public void TestHeaderArrivalNotificationDateDateEdit()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertEquals("HeaderArrivalNotificationDateDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, control.FindSingle<ZDateEdit>("HeaderArrivalNotificationDateDateEdit").DateTimeFormat);
			}
		}

		public void TestHeaderExitDateDateEdit()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertEquals("HeaderExitDateDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, control.FindSingle<ZDateEdit>("HeaderExitDateDateEdit").DateTimeFormat);
			}
		}

		public void TestHeaderLoadingPlaceTextBox()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertEquals("HeaderLoadingPlaceTextBox CharacterCasing", CharacterCasing.Normal, control.FindSingle<ZTextBox>("HeaderLoadingPlaceTextBox").CharacterCasing);
			}
		}

		public void TestArrivalNotificationDateDateEdit()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertEquals("ArrivalNotificationDateDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, control.FindSingle<ZDateEdit>("ArrivalNotificationDateDateEdit").DateTimeFormat);
			}
		}

		public void TestExitDateDateEdit()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertEquals("ExitDateDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, control.FindSingle<ZDateEdit>("ExitDateDateEdit").DateTimeFormat);
			}
		}

		public void TestLoadingPlaceTextBox()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertEquals("LoadingPlaceTextBox CharacterCasing", CharacterCasing.Normal, control.FindSingle<ZTextBox>("LoadingPlaceTextBox").CharacterCasing);
			}
		}

		public void TestDeclarantDocAddressControl()
		{
			using (var control = new ExitSummaryUserControl())
			{
				var declarantDocAddressControl = control.FindSingle<ZDocAddressControl>("DeclarantDocAddressControl");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Declarant", declarantDocAddressControl.CaptionResourceString.Caption);
					AssertEquals("Display Mode", ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox, declarantDocAddressControl.DisplayMode);
					AssertEquals("Organisations List", "CusExitDetails.Lookups.OrgHeaderCollection", declarantDocAddressControl.BindToOrganisations);
				});
			}
		}

		public void TestRepresentativeDocAddressControl()
		{
			using (var control = new ExitSummaryUserControl())
			{
				var representativeDocAddressControl = control.FindSingle<ZDocAddressControl>("RepresentativeDocAddressControl");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Representative", representativeDocAddressControl.CaptionResourceString.Caption);
					AssertEquals("Display Mode", ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox, representativeDocAddressControl.DisplayMode);
					AssertEquals("Organisations List", "CusExitDetails.Lookups.OrgHeaderCollection", representativeDocAddressControl.BindToOrganisations);
				});
			}
		}

		public void TestMovementsGrid()
		{
			using (var control = new ExitSummaryUserControl())
			{
				CombineAssertions(() =>
				{
					var movementsGrid = control.FindSingle<ZGrid>("MovementsGrid");

					var arrivalNotificationPlaceColumnStyleInfo = movementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_ArrivalNotificationPlace);
					AssertNull("ArrivalNotificationPlaceColumnStyleInfo", arrivalNotificationPlaceColumnStyleInfo);

					var arrivalNotificationDateColumnStyleInfo = movementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_ArrivalNotificationDate) as ZDateEditColumnStyleInfo;
					AssertEquals("ArrivalNotificationDateColumnStyleInfo DateTimeFormat", ZDateTimePickerFormat.Long, arrivalNotificationDateColumnStyleInfo.DateTimeFormat);
					AssertEquals("ArrivalNotificationDateColumnStyleInfo Width", 140, arrivalNotificationDateColumnStyleInfo.Width);

					var exitDateColumnStyleInfo = movementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_ExitDate) as ZDateEditColumnStyleInfo;
					AssertEquals("ExitDateColumnStyleInfo DateTimeFormat", ZDateTimePickerFormat.Long, exitDateColumnStyleInfo.DateTimeFormat);
					AssertEquals("ExitDateColumnStyleInfo Width", 109, exitDateColumnStyleInfo.Width);

					var movementReferenceNumberColumnStyle = movementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_MovementReferenceNumber);
					AssertEquals("MovementReferenceNumberColumnStyle Width", 130, movementReferenceNumberColumnStyle.Width);

					var transportIDColumnStyle = movementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_TransportID);
					AssertEquals("TransportIDColumnStyle Width", 120, transportIDColumnStyle.Width);

					var locationOfGoodsColumnStyleInfo = movementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_LocationOfGoods) as ZTextBoxColumnStyleInfo;
					AssertEquals("LocationOfGoodsColumnStyleInfo CharacterCasing", CharacterCasing.Normal, locationOfGoodsColumnStyleInfo.CharacterCasing);
					AssertEquals("LocationOfGoodsColumnStyleInfo Width", 150, locationOfGoodsColumnStyleInfo.Width);

					var referenceNumberUCRColumnStyleInfo = movementsGrid.GetColumnStyle(CusExitDetail.Schema.ReferenceNumberUCR) as ZTextBoxColumnStyleInfo;
					AssertEquals("ReferenceNumberUCRColumnStyleInfo CharacterCasing", CharacterCasing.Upper, referenceNumberUCRColumnStyleInfo.CharacterCasing);
					AssertEquals("ReferenceNumberUCRColumnStyleInfo Width", 130, referenceNumberUCRColumnStyleInfo.Width);

					var registrationNumberAWBColumnStyleInfo = movementsGrid.GetColumnStyle(CusExitDetail.Schema.RegistrationNumberAWB) as ZTextBoxColumnStyleInfo;
					AssertEquals("RegistrationNumberAWBColumnStyleInfo CharacterCasing", CharacterCasing.Upper, registrationNumberAWBColumnStyleInfo.CharacterCasing);
					AssertEquals("RegistrationNumberAWBColumnStyleInfo Width", 150, registrationNumberAWBColumnStyleInfo.Width);

					var statusDescriptionStyleInfo = movementsGrid.GetColumnStyle(CusExitDetail.Schema.StatusDescription) as ZTextBoxColumnStyleInfo;
					AssertNotNull("Status Description Column", statusDescriptionStyleInfo);
					AssertEquals("Status Description Character Casing", CharacterCasing.Normal, statusDescriptionStyleInfo.CharacterCasing);
					AssertEquals("Status Description Width", 160, statusDescriptionStyleInfo.Width);

					AssertArrayEqualsByElements("Movements Grid Column Order",
						new[]
						{
							CusExitDetail.Schema.CED_MovementReferenceNumber,
							CusExitDetail.Schema.ReferenceNumberUCR,
							CusExitDetail.Schema.RegistrationNumberAWB,
							CusExitDetail.Schema.CED_CustomsOffice,
							CusExitDetail.Schema.CED_ArrivalNotificationDate,
							CusExitDetail.Schema.CED_ExitDate,
							CusExitDetail.Schema.CED_TransportID,
							CusExitDetail.Schema.CED_LocationOfGoods,
							CusExitDetail.Schema.CED_Status,
							CusExitDetail.Schema.StatusDescription
						},
						movementsGrid.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().Select(x => x.ColumnName).ToArray());
				});
			}
		}

		public void TestItemsGrid()
		{
			using (var control = new ExitSummaryUserControl())
			{
				CombineAssertions(() =>
				{
					var itemsGrid = control.FindSingle<ZGrid>("ItemsGrid");

					var lineNumberColumnStyle = itemsGrid.GetColumnStyle(CusExitItemSchema.Constants.CXI_LineNumber);
					AssertEquals("LineNumberColumnStyle Width", 63, lineNumberColumnStyle.Width);

					var netMassColumnStyle = itemsGrid.GetColumnStyle(CusExitItemSchema.Constants.CXI_NetMass);
					AssertEquals("NetMassColumnStyle Width", 71, netMassColumnStyle.Width);

					var grossMassColumnStyle = itemsGrid.GetColumnStyle(CusExitItemSchema.Constants.CXI_GrossMass);
					AssertEquals("GrossMassColumnStyle Width", 81, grossMassColumnStyle.Width);

					var statusColumnStyle = itemsGrid.GetColumnStyle(CusExitItemSchema.Constants.CXI_Status);
					AssertEquals("StatusColumnStyle Width", 53, statusColumnStyle.Width);

					var referenceNumberUCRColumnStyleInfo = itemsGrid.GetColumnStyle(CusExitItem.Schema.ReferenceNumberUCR) as ZTextBoxColumnStyleInfo;
					AssertEquals("ReferenceNumberUCRColumnStyleInfo CharacterCasing", CharacterCasing.Upper, referenceNumberUCRColumnStyleInfo.CharacterCasing);
					AssertEquals("ReferenceNumberUCRColumnStyleInfo Width", 138, referenceNumberUCRColumnStyleInfo.Width);

					var registrationNumberAWBColumnStyleInfo = itemsGrid.GetColumnStyle(CusExitItem.Schema.RegistrationNumberAWB) as ZTextBoxColumnStyleInfo;
					AssertEquals("RegistrationNumberAWBColumnStyleInfo CharacterCasing", CharacterCasing.Upper, registrationNumberAWBColumnStyleInfo.CharacterCasing);
					AssertEquals("RegistrationNumberAWBColumnStyleInfo Width", 153, registrationNumberAWBColumnStyleInfo.Width);
				});
			}
		}

		public void TestReferenceNumberUCRTextBox()
		{
			using (var control = new ExitSummaryUserControl())
			{
				var textBox = control.FindSingle<ZTextBox>("ReferenceNumberUCRTextBox");
				CombineAssertions(() =>
				{
					AssertEquals("BindTo", "CusExitDetails.ReferenceNumberUCR", textBox.BindTo);
					AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, textBox.CharacterCasing);
				});
			}
		}

		public void TestRegistrationNumberAWBTextBox()
		{
			using (var control = new ExitSummaryUserControl())
			{
				var textBox = control.FindSingle<ZTextBox>("RegistrationNumberAWBTextBox");
				CombineAssertions(() =>
				{
					AssertEquals("BindTo", "CusExitDetails.RegistrationNumberAWB", textBox.BindTo);
					AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, textBox.CharacterCasing);
				});
			}
		}
	}
}
