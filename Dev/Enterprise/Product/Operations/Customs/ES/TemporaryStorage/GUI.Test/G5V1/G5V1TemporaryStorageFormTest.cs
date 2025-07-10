using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStorageForm))]
	sealed class G5V1TemporaryStorageFormTest : ZTemplateFormTest
	{
		public void TestMainControlTabs()
		{
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				CombineAssertions(() =>
				{
					AssertNotNull(mainTabControl);

					AssertEquals("Tab Pages count", 9, mainTabControl.TabCount);

					AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
					{
						"MainTabPage",
						"BillPartiesTabPage",
						"ContainerTabPage",
						"PacksTabPage",
						"PackedItemsTabPage",
						"MessagesTabPage",
						"WorkflowTabPage",
						"NotesTabPage",
						"LogsTabPage"
					}, mainTabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
				});
			}
		}

		public void TestBillsTab()
		{
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertExceptionThrown<InvalidOperationException>("Bills tab is not in the form by default", () => mainTabControl.FindSingle<ZTabPage>("BillsTabPage"));
			}
		}

		public void TestBillsTabVisibility()
		{
			tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertExceptionThrown<InvalidOperationException>("Bills tab is not in the form when AMA_MessageType = G5X", () => mainTabControl.FindSingle<ZTabPage>("BillsTabPage"));
			}

			tempStorage.AMA_MessageType = "G5E";
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var billsTabPage = mainTabControl.FindSingle<ZTabPage>("BillsTabPage");
				var billsLayoutPanel = billsTabPage.FindSingle<DynamicLayoutPanel>("UCC6TemporaryStorageBillsLayoutPanel");
				CombineAssertions(() =>
				{
					AssertEquals("Bills tab label should be Bills", "Bills", billsTabPage.CaptionResourceString.Caption);
					AssertEquals("Bills tab should be visible for G5E", true, billsTabPage.TabVisible);
				});
			}

			tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertExceptionThrown<InvalidOperationException>("Bills tab is not in the form when AMA_MessageType = LAM", () => mainTabControl.FindSingle<ZTabPage>("BillsTabPage"));
			}

			tempStorage.AMA_MessageType = "G5R";
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var billsTabPage = mainTabControl.FindSingle<ZTabPage>("BillsTabPage");
				var billsLayoutPanel = billsTabPage.FindSingle<DynamicLayoutPanel>("UCC6TemporaryStorageBillsLayoutPanel");
				CombineAssertions(() =>
				{
					AssertEquals("Bills tab label should be Bills", "Bills", billsTabPage.CaptionResourceString.Caption);
					AssertEquals("Bills tab should be visible for G5R", true, billsTabPage.TabVisible);
				});
			}

			tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertExceptionThrown<InvalidOperationException>("Bills tab is not in the form when AMA_MessageType = TSM", () => mainTabControl.FindSingle<ZTabPage>("BillsTabPage"));
			}
		}

		public void TestMessagesControls()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var messagesTabPage = mainTabControl.FindSingle<ZTabPage>("MessagesTabPage");
				CombineAssertions("MessagesTabPage properties", () =>
				{
					AssertEquals("MessagesTabPage should be visible.", true, messagesTabPage.TabVisible);

					tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
					AssertEquals("MessagesTabPage should not be visible when AMA_MessageType = LAM.", false, messagesTabPage.TabVisible);

					tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
					AssertEquals("MessagesTabPage should be visible when AMA_MessageType = G5X.", true, messagesTabPage.TabVisible);

					tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
					AssertEquals("MessagesTabPage should not be visible when AMA_MessageType = TSM.", true, messagesTabPage.TabVisible);
				});
			}
		}

		public void TestContainerControls()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var containerTabPage = mainTabControl.FindSingle<ZTabPage>("ContainerTabPage");
				CombineAssertions("ContainerTabPage properties", () =>
				{
					AssertEquals("ContainerTabPage should be visible.", true, containerTabPage.TabVisible);

					tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
					AssertEquals("ContainerTabPage should not be visible when AMA_MessageType = LAM.", false, containerTabPage.TabVisible);

					tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
					AssertEquals("ContainerTabPage should be visible when AMA_MessageType = G5X.", true, containerTabPage.TabVisible);

					tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
					AssertEquals("ContainerTabPage should not be visible when AMA_MessageType = TSM.", false, containerTabPage.TabVisible);
				});
			}
		}

		[RequiresSTA]
		public void TestBillPartiesTab()
		{
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var billPartiesTabPage = mainTabControl.FindSingle<ZTabPage>("BillPartiesTabPage");
				var billPartiesLayoutPanel = mainTabControl.FindSingle<DynamicLayoutPanel>("BillPartiesLayoutPanel");
				CombineAssertions(() =>
				{
					billPartiesTabPage.AssertThisControl(x => x.WithCaption("Organizations")
															   .WithTabVisible(true));
					AssertNotNull("BillPartiesTabPage should contain BillPartiesLayoutPanel", billPartiesLayoutPanel);
				});
			}
		}

		public void TestPacksTab()
		{
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var packsTabPage = mainTabControl.FindSingle<ZTabPage>("PacksTabPage");
				var packsLayoutPanel = packsTabPage.FindSingle<DynamicLayoutPanel>("PacksLayoutPanel");
				CombineAssertions(() =>
				{
					packsTabPage.AssertThisControl(x => x.WithCaption("Packs")
														 .WithTabVisible(true));
					AssertNotNull("PacksTabPage should contain PacksLayoutPanel", packsLayoutPanel);

					var ucc6TemporaryStoragePackagesControl = packsLayoutPanel.FindSingle<UCC6TemporaryStoragePackagesControl>("UCC6TemporaryStoragePackagesControl");
					AssertNotNull("PacksLayoutPanel should contain UCC6TemporaryStoragePackagesControl", ucc6TemporaryStoragePackagesControl);
				});
			}
		}

		[RequiresSTA]
		public void TestPackedItemsTab()
		{
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var packedItemsTabPage = mainTabControl.FindSingle<ZTabPage>("PackedItemsTabPage");
				var packedItemsLayoutPanel = packedItemsTabPage.FindSingle<DynamicLayoutPanel>("PackedItemsLayoutPanel");
				CombineAssertions(() =>
				{
					packedItemsTabPage.AssertThisControl(x => x.WithCaption("Items")
															   .WithTabVisible(true));
					AssertNotNull("packedItemsTabPage should contain PackedItemsLayoutPanel", packedItemsLayoutPanel);

					var g5v1TemporaryStoragePackedItemControl = packedItemsLayoutPanel.FindSingle<G5V1TemporaryStoragePackedItemControl>("G5V1TemporaryStoragePackedItemControl");
					AssertNotNull("PackedItemsLayoutPanel should contain G5V1TemporaryStoragePackedItemControl", g5v1TemporaryStoragePackedItemControl);
				});
			}
		}

		public void TestGetNewMessagingMenu()
		{
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				AssertType<G5V1TemporaryStorageMessagesMenu>("GetNewMessagingMenu should have been overridden", ((IFileMenuItemsProvider)form).MainMenu.MenuItems.FindByName(nameof(G5V1TemporaryStorageMessagesMenu)));
			}
		}

		public void TestControlsReadOnlyWhenMessageSent()
		{
			tempStorage.AMA_MessageStatus = "SNT";
			var expectedValue = true;

			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>("MainTabPage");
				var mainTabPanel = mainTabPage.FindSingle<DynamicLayoutPanel>("MainDynamicLayoutPanel");
				
				var countryCodeFindBox = mainTabPanel.FindSingle<ZCodeFindBox>("CountryCodeFindBox");
				var countryDescriptionBox = countryCodeFindBox.DescriptionBox;
				var countryButton = countryCodeFindBox.PopupButton;
				var countryCodeBox = countryCodeFindBox.CodeBox;

				var messageTypeDropEdit = mainTabPanel.FindSingle<ZDropEdit>("MessageTypeDropEdit");
				var messageTypeDescriptionBox = messageTypeDropEdit.DescriptionBox;
				var messageTypeCodeBox = messageTypeDropEdit.CodeBox;

				var transportModeDropEdit = mainTabPanel.FindSingle<ZDropEdit>("TransportModeDropEdit");
				var transportModeDescriptionBox = transportModeDropEdit.DescriptionBox;
				var transportModeCodeBox = transportModeDropEdit.CodeBox;

				var transportTypeDropEdit = mainTabPanel.FindSingle<ZDropEdit>("TransportTypeDropEdit");
				var transportTypeDescriptionBox = transportTypeDropEdit.DescriptionBox;
				var transportTypeCodeBox = transportTypeDropEdit.CodeBox;

				var declarantAddressControl = mainTabPanel.FindSingle<ZAddressControl>("DeclarantAddressControl");
				var declarantAddressTextBox = declarantAddressControl.FindSingle<ZTextBox>("AddressTextBox");
				var declarantAddressDropEdit = declarantAddressControl.FindSingle<ZDropEdit>("AddressDropEdit");
				var declarantAddressDescriptionBox = declarantAddressDropEdit.DescriptionBox;
				var declarantAddressCodeBox = declarantAddressDropEdit.CodeBox;
				var declarantOrganizationFindBox = declarantAddressControl.FindSingle<ZAddressFindBox>("OrganisationFindBox");
				var declarantOrganizationDescriptionBox = declarantOrganizationFindBox.DescriptionBox;
				var declarantOrganizationButton = declarantOrganizationFindBox.PopupButton;
				var declarantOrganizationCodeBox = declarantOrganizationFindBox.CodeBox;

				var representativeAddressControl = mainTabPanel.FindSingle<ZAddressControl>("RepresentativeAddressControl");
				var representativeAddressTextBox = representativeAddressControl.FindSingle<ZTextBox>("AddressTextBox");
				var representativeAddressDropEdit = representativeAddressControl.FindSingle<ZDropEdit>("AddressDropEdit");
				var representativeAddressDescriptionBox = representativeAddressDropEdit.DescriptionBox;
				var representativeAddressCodeBox = representativeAddressDropEdit.CodeBox;
				var representativeOrganizationFindBox = representativeAddressControl.FindSingle<ZAddressFindBox>("OrganisationFindBox");
				var representativeOrganizationDescriptionBox = representativeOrganizationFindBox.DescriptionBox;
				var representativeOrganizationButton = representativeOrganizationFindBox.PopupButton;
				var representativeOrganizationCodeBox = representativeOrganizationFindBox.CodeBox;

				var departureCustomsOffice = mainTabPanel.FindSingle<ZCodeFindBox>("SupervisingCustomsOfficeCodeFindBox");
				var departureCustomsOfficeDescriptionBox = departureCustomsOffice.DescriptionBox;
				var departureCustomsOfficeButton = departureCustomsOffice.PopupButton;
				var departureCustomsOfficeCodeBox = departureCustomsOffice.CodeBox;

				var locationOfGoodsUserControl = mainTabPanel.FindSingle<LocationOfGoodsUserControl>("LocationOfGoodsUserControl");
				var locationofGoodsButton = locationOfGoodsUserControl.FindSingle<ZButton>("MoreButton");
				var locationOfGoodsDescription = locationOfGoodsUserControl.FindSingle<ZTextBox>("LocationOfGoodsDescription");

				var destinationCustomsOffice = mainTabPanel.FindSingle<ZCodeFindBox>("DestinationCustomsOfficeCodeFindBox");
				var destinationCustomsOfficeDescriptionBox = destinationCustomsOffice.DescriptionBox;
				var destinationCustomsOfficeButton = destinationCustomsOffice.PopupButton;
				var destinationCustomsOfficeCodeBox = destinationCustomsOffice.CodeBox;
				
				var departureGoodsLocation = mainTabPanel.FindSingle<DestinationLocationOfGoodsUserControl>("DestinationLocationOfGoodsUserControl");
				var departureGoodsLocationMoreButton = departureGoodsLocation.MoreButton;
				var departureGoodsLocationOfGoodsDescription = departureGoodsLocation.FindSingle<ZTextBox>("LocationOfGoodsDescription");

				var transportDocumentTypeDropEdit = mainTabPanel.FindSingle<ZDropEdit>("TransportDocumentTypeDropEdit");
				var transportDocumentTypeDescriptionBox = transportDocumentTypeDropEdit.DescriptionBox;
				var transportDocumentTypeCodeBox = transportDocumentTypeDropEdit.CodeBox;

				var transportDocumentTextBox = mainTabPanel.FindSingle<ZTextBox>("TransportDocumentTextBox");

				var authorizationTypeDropEdit = mainTabPanel.FindSingle<ZDropEdit>("AuthorizationTypeDropEdit");
				var authorizationTypeDescriptionBox = authorizationTypeDropEdit.DescriptionBox;
				var authorizationTypeCodeBox = authorizationTypeDropEdit.CodeBox;

				var authorizationOwnerGuidFindBox = mainTabPanel.FindSingle<ZGuidFindBox>("AuthorizationOwnerGuidFindBox");
				var authorizationOwnerDescriptionBox = authorizationOwnerGuidFindBox.DescriptionBox;
				var authorizationOwnerButton = authorizationOwnerGuidFindBox.PopupButton;
				var authorizationOwnerCodeBox = authorizationOwnerGuidFindBox.CodeBox;

				var authorizationNumberCodeFindBox = mainTabPanel.FindSingle<ZCodeFindBox>("AuthorizationNumberCodeFindBox");
				var authorizationNumberDescriptionBox = authorizationNumberCodeFindBox.DescriptionBox;
				var authorizationNumberPopupButton = authorizationNumberCodeFindBox.PopupButton;
				var authorizationNumberCodeBox = authorizationNumberCodeFindBox.CodeBox;

				var cusAgentCodeFindBox = mainTabPanel.FindSingle<ZCodeFindBox>("CusAgentCodeFindBox");
				var cusAgentDescriptionBox = cusAgentCodeFindBox.DescriptionBox;
				var cusAgentButton = cusAgentCodeFindBox.PopupButton;
				var cusAgentCodeBox = cusAgentCodeFindBox.CodeBox;

				var certificateDropEdit = mainTabPanel.FindSingle<ZDropEdit>("CertificateDropEdit");
				var certificateCodeBox = certificateDropEdit.CodeBox;

				var trainingCheckBox = mainTabPanel.FindSingle<ZCheckBox>("TrainingCheckBox");

				var mainTabDeclarationDetails = mainTabPanel.FindSingle<ZGroupBox>("DeclarationDetailsGroupBox");
				var mainTabDeclarationDetailsPanel = mainTabDeclarationDetails.FindSingle<DynamicLayoutPanel>("DynamicDetailsUserControlLayoutPanel");
				var lrnTextBox = mainTabDeclarationDetailsPanel.FindSingle<ZTextBox>("LRNTextBox");
				var mrnTextBox = mainTabDeclarationDetailsPanel.FindSingle<ZTextBox>("MRNTextBox");
				var customsStatusDropEdit = mainTabDeclarationDetailsPanel.FindSingle<ZDropEdit>("CustomsStatusDropEdit");
				var messageStatusDropEdit = mainTabDeclarationDetailsPanel.FindSingle<ZDropEdit>("MessageStatusDropEdit");
				var circuitTextBox = mainTabDeclarationDetailsPanel.FindSingle<ZTextBox>("CircuitTextBox");
				var acceptanceDateDateEdit = mainTabDeclarationDetailsPanel.FindSingle<ZDateEdit>("AcceptanceDateDateEdit");
				var clearanceNumberTextBox = mainTabDeclarationDetailsPanel.FindSingle<ZTextBox>("ClearanceNumberTextBox");

				var dsdtSdFormatNumberTextBox = mainTabDeclarationDetailsPanel.FindSingle<ZUserControl>("DsdtSdFormatNoUrlUserControl").FindSingle<ZTextBox>("DsdtSdFormatNumberTextBox");
				var dsdtMrnNumberTextBox = mainTabDeclarationDetailsPanel.FindSingle<ZTextBox>("DsdtMrnNumberTextBox");

				var guaranteeGroupBox = mainTabPanel.FindSingle<ZGroupBox>("GuaranteeGroupBox");
				var guaranteePanel = guaranteeGroupBox.FindSingle<DynamicLayoutPanel>("DynamicGuaranteePanel");

				var bondNumberCodeFindBox = guaranteePanel.FindSingle<ZCodeFindBox>("BondNumberCodeFindBox");
				var bondNumberDescriptionBox = bondNumberCodeFindBox.DescriptionBox;
				var bondNumberPopupButton = bondNumberCodeFindBox.PopupButton;
				var bondNumberCodeBox = bondNumberCodeFindBox.CodeBox;

				var liabilityAmountCalcDropEdit = guaranteePanel.FindSingle<ZCalcDropEdit>("AmountCalcDropEdit");
				var liabilityAmountCalcEdit = liabilityAmountCalcDropEdit.FindSingle<ZCalcEdit>("AmountCalcEdit");

				var liabilityAmountUnitDropEdit = liabilityAmountCalcDropEdit.FindSingle<ZDropEdit>("UnitDropEdit");
				var liabilityAmountUnitDescriptionBox = liabilityAmountUnitDropEdit.DescriptionBox;
				var liabilityAmountUnitCodeBox = liabilityAmountUnitDropEdit.CodeBox;

				var overrideCheckBox = guaranteePanel.FindSingle<ZCheckBox>("OverrideCheckBox");

				var mainTabDocumentsTabControl = mainTabPanel.FindSingle<ZTabControl>("DocumentsTabControl");

				var supportingDocumentsTabPage = mainTabDocumentsTabControl.FindSingle<ZTabPage>("SupportingDocumentUserControlTabPage");
				var supportingDocumentsPanel = supportingDocumentsTabPage.FindSingle<DynamicLayoutPanel>("SupportingDocumentsLayoutPanel");
				var supportingDocumentsUserControl = supportingDocumentsPanel.FindSingle<UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid>("UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid");
				var supportingDocumentsGrid = supportingDocumentsPanel.FindSingle<ZGrid>("SupportingDocumentsGrid");

				var additionalInformationTabPage = mainTabDocumentsTabControl.FindSingle<ZTabPage>("AdditionalInfoUserControlTabPage");
				var additionalInformationPanel = additionalInformationTabPage.FindSingle<DynamicLayoutPanel>("AdditionalInfosLayoutPanel");
				var additionalInformationUserControl = additionalInformationPanel.FindSingle<UCC6TemporaryStorageAdditionalInfosUserControlWithGrid>("UCC6TemporaryStorageAdditionalInfosUserControlWithGrid");
				var additionalInformationGrid = additionalInformationPanel.FindSingle<ZGrid>("AdditionalInfosGrid");

				var arrivalTransportMeansCodeTextBox = mainTabPanel.FindSingle<ZTextBox>("ArrivalTransportMeansCodeTextBox");

				var containersTabPage = mainTabControl.FindSingle<ZTabPage>("ContainerTabPage");
				var containerControl = containersTabPage.FindSingle<UCC6TemporaryStorageContainerControl>("UCC6TemporaryStorageContainerControl");
				var containerDataSplitContainer = containerControl.FindSingle<KSplitContainer>("containerDataSplitContainer");
				var containerSplitterPanel0 = containerDataSplitContainer.Controls[0];
				var containersGrid = containerSplitterPanel0.FindSingle<ZGrid>("ContainersGrid");
				var containerSplitterPanel1GroupBox = containerDataSplitContainer.Controls[1].Controls[0];
				var containersAdditionalSealsGrid = containerSplitterPanel1GroupBox.FindSingle<ZGrid>("AdditionalSealsGrid");

				CombineAssertions(() =>
				{
					AssertNotNull("Main tab", mainTabPage);

					AssertEquals("Destination Customs office description box is read-only", expectedValue, destinationCustomsOfficeDescriptionBox.ReadOnly);
					AssertEquals("Destination Customs office button is read-only", expectedValue, destinationCustomsOfficeButton.ReadOnly);
					AssertEquals("Destination Customs office code box is read-only", expectedValue, destinationCustomsOfficeCodeBox.ReadOnly);

					AssertEquals("Country description box is read-only", expectedValue, countryDescriptionBox.ReadOnly);
					AssertEquals("Country button is read-only", expectedValue, countryButton.ReadOnly);
					AssertEquals("Country code box is read-only", expectedValue, countryCodeBox.ReadOnly);
					AssertEquals("Message type description box is read-only", expectedValue, messageTypeDescriptionBox.ReadOnly);
					AssertEquals("Message type code box is read-only", expectedValue, messageTypeCodeBox.ReadOnly);
					AssertEquals("Transport mode description box is read-only", expectedValue, transportModeDescriptionBox.ReadOnly);
					AssertEquals("Transport mode drop edit is read-only", expectedValue, transportModeCodeBox.ReadOnly);
					AssertEquals("Transport type description box is read-only", expectedValue, transportTypeDescriptionBox.ReadOnly);
					AssertEquals("Transport type code box is read-only", expectedValue, transportTypeCodeBox.ReadOnly);
					AssertEquals("Declarant address text box is read-only", expectedValue, declarantAddressTextBox.ReadOnly);
					AssertEquals("Declarant address description box is read-only", expectedValue, declarantAddressDescriptionBox.ReadOnly);
					AssertEquals("Declarant address code box is read-only", expectedValue, declarantAddressCodeBox.ReadOnly);
					AssertEquals("Declarant organisation description box is read-only", expectedValue, declarantOrganizationDescriptionBox.ReadOnly);
					AssertEquals("Declarant organisation button is read-only", expectedValue, declarantOrganizationButton.ReadOnly);
					AssertEquals("Declarant organisation code box is read-only", expectedValue, declarantOrganizationCodeBox.ReadOnly);
					AssertEquals("Representative address text box is read-only", expectedValue, representativeAddressTextBox.ReadOnly);
					AssertEquals("Representative address description box is read-only", expectedValue, representativeAddressDescriptionBox.ReadOnly);
					AssertEquals("Representative address code box is read-only", expectedValue, representativeAddressCodeBox.ReadOnly);
					AssertEquals("Representative organisation description box is read-only", expectedValue, representativeOrganizationDescriptionBox.ReadOnly);
					AssertEquals("Representative organisation button is read-only", expectedValue, representativeOrganizationButton.ReadOnly);
					AssertEquals("Representative organisation code box is read-only", expectedValue, representativeOrganizationCodeBox.ReadOnly);
					AssertEquals("Departure customs office description box is read-only", expectedValue, departureCustomsOfficeDescriptionBox.ReadOnly);
					AssertEquals("Departure customs office button is read-only", expectedValue, departureCustomsOfficeButton.ReadOnly);
					AssertEquals("Departure customs office code box is read-only", expectedValue, departureCustomsOfficeCodeBox.ReadOnly);
					AssertEquals("Location of goods button is read-only", expectedValue, locationofGoodsButton.ReadOnly);
					AssertEquals("Location of goods description is read-only", expectedValue, locationOfGoodsDescription.ReadOnly);
					AssertEquals("Departure goods location more button is read-only", expectedValue, departureGoodsLocationMoreButton.ReadOnly);
					AssertEquals("Departure goods location description is read-only", expectedValue, departureGoodsLocationOfGoodsDescription.ReadOnly);
					AssertEquals("Transport document type description box is read-only", expectedValue, transportDocumentTypeDescriptionBox.ReadOnly);
					AssertEquals("Transport document type code box is read-only", expectedValue, transportDocumentTypeCodeBox.ReadOnly);
					AssertEquals("Transport document text box is read-only", expectedValue, transportDocumentTextBox.ReadOnly);
					AssertEquals("Authorization type description box is read-only", expectedValue, authorizationTypeDescriptionBox.ReadOnly);
					AssertEquals("Authorization type code box is read-only", expectedValue, authorizationTypeCodeBox.ReadOnly);
					AssertEquals("Authorization owner description box is read-only", expectedValue, authorizationOwnerDescriptionBox.ReadOnly);
					AssertEquals("Authorization owner button is read-only", expectedValue, authorizationOwnerButton.ReadOnly);
					AssertEquals("Authorization owner code box is read-only", expectedValue, authorizationOwnerCodeBox.ReadOnly);
					AssertEquals("Authorization number description box is read-only", expectedValue, authorizationNumberDescriptionBox.ReadOnly);
					AssertEquals("Authorization number popup button is read-only", expectedValue, authorizationNumberPopupButton.ReadOnly);
					AssertEquals("Authorization number code box is read-only", expectedValue, authorizationNumberCodeBox.ReadOnly);
					AssertEquals("Broker description box is never read-only", false, cusAgentDescriptionBox.ReadOnly);
					AssertEquals("Broker button is never read-only", false, cusAgentButton.ReadOnly);
					AssertEquals("Broker code box is never read-only", false, cusAgentCodeBox.ReadOnly);
					AssertEquals("Certificate code box is never read-only", false, certificateCodeBox.ReadOnly);
					AssertEquals("LRN text box is read-only", expectedValue, trainingCheckBox.ReadOnly);

					AssertEquals("LRN text box is read-only", expectedValue, lrnTextBox.ReadOnly);
					AssertEquals("MRN text box is read-only", expectedValue, mrnTextBox.ReadOnly);
					AssertEquals("Customs status drop edit is read-only", expectedValue, customsStatusDropEdit.ReadOnly);
					AssertEquals("Message status drop edit is read-only", expectedValue, messageStatusDropEdit.ReadOnly);
					AssertEquals("Circuit text box is read-only", expectedValue, circuitTextBox.ReadOnly);
					AssertEquals("Acceptance date date edit is read-only", expectedValue, acceptanceDateDateEdit.ReadOnly);
					AssertEquals("Clearance number text box is read-only", expectedValue, clearanceNumberTextBox.ReadOnly);
					AssertEquals("DSDT SD Format number text box is read-only", expectedValue, dsdtSdFormatNumberTextBox.ReadOnly);
					AssertEquals("DSDT MRN number text box is read-only", expectedValue, dsdtMrnNumberTextBox.ReadOnly);

					AssertEquals("Bond number description box is read-only", expectedValue, bondNumberDescriptionBox.ReadOnly);
					AssertEquals("Bond number popup button is read-only", expectedValue, bondNumberPopupButton.ReadOnly);
					AssertEquals("Bond number code box is read-only", expectedValue, bondNumberCodeBox.ReadOnly);
					AssertEquals("Liability amount calc edit is read-only", expectedValue, liabilityAmountCalcEdit.ReadOnly);
					AssertEquals("Liability amount description box is read-only", expectedValue, liabilityAmountUnitDescriptionBox.ReadOnly);
					AssertEquals("Currency code box is read-only", expectedValue, liabilityAmountUnitCodeBox.ReadOnly);
					AssertEquals("Override checkbox is read-only", expectedValue, overrideCheckBox.ReadOnly);
					AssertEquals("Arrival transport means code text box is read-only", expectedValue, arrivalTransportMeansCodeTextBox.ReadOnly);
				});
			}
		}

		public void TestUnionGoodsValueChanged_GuaranteeReadOnly()
		{
			tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempStorage.Guarantee.PW_Override = true;
			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] Guarantee Number not readonly", false, tempStorage.Guarantee.PW_BondNumberInfo.ReadOnly);
					AssertEquals("[PreReq] Guarantee Override not readonly", false, tempStorage.Guarantee.PW_OverrideInfo.ReadOnly);
					AssertEquals("[PreReq] Guarantee Amount not readonly", false, tempStorage.Guarantee.PW_BondAmountInfo.ReadOnly);
				});

				CombineAssertions(() =>
				{
					tempStorage.UnionGoods = true;
					AssertEquals("Guarantee Number readonly", true, tempStorage.Guarantee.PW_BondNumberInfo.ReadOnly);
					AssertEquals("Guarantee Override readonly", true, tempStorage.Guarantee.PW_OverrideInfo.ReadOnly);
					AssertEquals("Guarantee Amount readonly", true, tempStorage.Guarantee.PW_BondAmountInfo.ReadOnly);
				});
			}
		}

		public void TestUnionGoodsValueChanged_GuaranteeReadOnly_MessageType()
		{
			tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempStorage.UnionGoods = true;

			using (var form = new G5V1TemporaryStorageForm(tempStorage))
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] Guarantee Number readonly", true, tempStorage.Guarantee.PW_BondNumberInfo.ReadOnly);
					AssertEquals("[PreReq] Guarantee Override readonly", true, tempStorage.Guarantee.PW_OverrideInfo.ReadOnly);
					AssertEquals("[PreReq] Guarantee Amount readonly", true, tempStorage.Guarantee.PW_BondAmountInfo.ReadOnly);
					AssertEquals("[PreReq] UnionGoods true", true, tempStorage.UnionGoods);
				});

				CombineAssertions(() =>
				{
					tempStorage.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
					tempStorage.Guarantee.PW_Override = true;
					AssertEquals("Guarantee Number not readonly", false, tempStorage.Guarantee.PW_BondNumberInfo.ReadOnly);
					AssertEquals("Guarantee Override not readonly", false, tempStorage.Guarantee.PW_OverrideInfo.ReadOnly);
					AssertEquals("Guarantee Amount not readonly", false, tempStorage.Guarantee.PW_BondAmountInfo.ReadOnly);
					AssertEquals("UnionGoods change to false when not TSM", false, tempStorage.UnionGoods);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			var bill = tempStorage.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.PackedItems.AddNew().FillWithValidTestData();
			_ = tempStorage.PresentationCustomsOfficeCode;
			_ = tempStorage.DestinationCustomsOfficeCode;
			Factory.Save();
			var form = new G5V1TemporaryStorageForm(tempStorage);
			form.ControllerID = ControllerIDs.Customs.EU.UCC6TemporaryStorage;
			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();
			tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
		}
		TemporaryStorageHeader tempStorage;
	}
}
