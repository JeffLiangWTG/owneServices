using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CAJobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<CAJobDeclarationUserControl, JobDeclaration>
	{
		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed_ZPropertyInfoValueChangedEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
			}

			var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);

			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);

			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);

			foreach (var dictionaryOfSubcriber in dictionary)
			{
				foreach (var subcriber in dictionaryOfSubcriber.Value)
				{
					AssertNotEquals(string.Format("Method {0} should be detached.", subcriber.Value.Method.Name), typeof(CAJobDeclarationUserControl), subcriber.Value.Target.GetType());
				}
			}
		}

		public void TestTransportDetailsGroupBoxHeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var groupBox = form.FindSingle<ZGroupBox>("TransportDetailsGroupBox");
					AssertEquals("Import", 395, groupBox.Size.Height);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("Not Import", 270, groupBox.Size.Height);
				});
			}
		}

		public void TestShipmentDetailsGroupBox_Location()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var groupBox = form.FindSingle<ZGroupBox>("ShipmentDetailsGroupBox");
					AssertEquals("Import.Location.X", 253, groupBox.Location.X);
					AssertEquals("Import.Location.Y", 470, groupBox.Location.Y);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("Not Import.Location.X", 253, groupBox.Location.X);
					AssertEquals("Not Import.Location.Y", 342, groupBox.Location.Y);
				});
			}
		}

		public void TestDeliveryInstructionsPanel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				CombineAssertions(() =>
				{
					var groupBox = form.FindSingle<LongTextControl>("DeliveryInstructionsLongTextControl");
					AssertEquals("Import.Location.X", 113, groupBox.Location.X);
					AssertEquals("Import.Location.Y", 366, groupBox.Location.Y);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("Not Import.Location.X", 113, groupBox.Location.X);
					AssertEquals("Not Import.Location.Y", 243, groupBox.Location.Y);
				});
			}
		}

		public void TestCCNFieldCaptionAndVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				var control = (CAJobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;

				AssertEquals("Vessel Find Box does not have description", false, control.VesselFindBox.ShowDescriptionBox);
				AssertEquals("Text field Cargo Control No is visible", true, control.CargoControlNumberNoBoundTextBox.Visible);
				AssertEquals("Button Default HWB is visible", true, control.DefaultHWBButton.Visible);
			}

			declaration.JE_JS = Guid.NewGuid();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				var control = (CAJobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;

				AssertEquals("Button Default HWB should be visible", true, control.DefaultHWBButton.Visible);
			}
		}

		public void TestUseImporterSecurityNumberPopup()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "98765");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABCDEFGH");
			CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importer = Factory.New<OrgHeader>();
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "12345";
			addInfo.ZO_AccountSecirityPassword = "12345678";
			var importerOfRecord = Factory.New<OrgHeader>();
			var importerOfRecordAddInfo = OrgImpAddInfo.Get(importerOfRecord);
			importerOfRecordAddInfo.ZO_AccountSecurityNumber = "54321";
			importerOfRecordAddInfo.ZO_AccountSecirityPassword = "87654321";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("12345", declaration.TransactionNumber.AccountSecurityCode);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("98765", declaration.TransactionNumber.AccountSecurityCode);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
				AssertEquals("Do you want to use the Importer of Record's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("98765", declaration.TransactionNumber.AccountSecurityCode);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Do you want to use the Importer of Record's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("54321", declaration.TransactionNumber.AccountSecurityCode);

				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				AssertEquals("Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("12345", declaration.TransactionNumber.AccountSecurityCode);

				declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
				AssertEquals("Do you want to use the Importer of Record's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("54321", declaration.TransactionNumber.AccountSecurityCode);
			}

			CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_OH_Importer = importer.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("54321", declaration.TransactionNumber.AccountSecurityCode);

				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("12345", declaration.TransactionNumber.AccountSecurityCode);
			}
		}

		public void TestCaptionsAndVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			using (CACustomsDataRegistry.Instance.MakeSomeFieldsJobDocAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = (CAJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				form.Show();
				entryHeader.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
				declaration.JE_MessageType = ZString.Empty;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				AssertEquals("SupplierOrganisationControl.Visible", false, control.SupplierOrganisationControl.Visible);
				AssertEquals("SupplierDocumentAddress.Visible", false, control.SupplierDocumentAddress.Visible);
				AssertEquals("ImporterOrganisationControl.Visible", false, control.ImporterOrganisationControl.Visible);
				AssertEquals("SupplierOrganisationControl.Caption", "Exporter", control.SupplierOrganisationControl.Text);
				AssertEquals("ImporterOrganisationControl.Caption", "Consignee", control.ImporterOrganisationControl.Text);
				AssertEquals("ForwarderOrganisationControl.Caption", "Service Provider", control.ForwarderOrganisationControl.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("DataLoadingModule EntryNumberLabel.Text", "Form Key", control.ExportDeclarationNumberBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("EntryNumberLabel.Visible", true, control.ExportDeclarationNumberBoundTextBox.Visible);
				AssertEquals("TransactionNumberPanel.Visible", false, control.TransactionNumberPanel.Visible);
				AssertEquals("StatusTextBox.Visible", false, control.StatusTextBox.Visible);
				AssertEquals("B3EntryStatusDescriptionTextBox.Visible", true, control.B3EntryStatusDescriptionTextBox.Visible);
				AssertEquals("B3EntryStatusDescriptionTextBox.CharacterCasing", CharacterCasing.Upper, control.B3EntryStatusDescriptionTextBox.CharacterCasing);
				AssertEquals("B3EntryStatusDescription.Caption", "Entry Status", control.B3EntryStatusDescriptionTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("JE_TransportModeBoundDropDownEdit.Visible", true, control.JE_TransportModeBoundDropDownEdit.Visible);
				control.RightTabControl.SelectedTab = control.OrganisationsTabPage;
				AssertEquals("ImportOrganizationsTabControl", 2, control.OrganisationsTabPage.Controls.Count);
				Assert("ImporterOfRecordDocAddressControl.Visible", !control.ImporterOfRecordDocAddressControl.Visible);
				AssertEquals("ImporterOrgAddressControl.Visible", true, control.ImporterOrgAddressControl.Visible);
				AssertEquals("SupplierOrgAddressControl.Visible", true, control.SupplierOrgAddressControl.Visible);
				AssertEquals("ImporterDocumentaryAddress.Visible", true, control.ImporterDocumentaryAddress.Visible);

				entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
				declaration.JE_MessageType = ZString.Empty;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("SupplierOrganisationControl.Visible", false, control.SupplierOrganisationControl.Visible);
				AssertEquals("SupplierDocumentAddress.Visible", false, control.SupplierDocumentAddress.Visible);
				AssertEquals("SupplierOrganisationControl.Caption", "Exporter", control.SupplierOrganisationControl.Text);
				AssertEquals("ImporterOrganisationControl.Caption", "Consignee", control.ImporterOrganisationControl.Text);
				AssertEquals("ImporterOrganisationControl.Visible", false, control.ImporterOrganisationControl.Visible);
				AssertEquals("ForwarderOrganisationControl.Caption", "Service Provider", control.ForwarderOrganisationControl.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Export EntryNumberLabel.Text", "Transaction #", control.ExportDeclarationNumberBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("EntryNumberLabel.Visible", true, control.ExportDeclarationNumberBoundTextBox.Visible);
				AssertEquals("TransactionNumberPanel.Visible", false, control.TransactionNumberPanel.Visible);
				AssertEquals("StatusTextBox.Visible", false, control.StatusTextBox.Visible);
				AssertEquals("B3EntryStatusDescriptionTextBox.Visible", true, control.B3EntryStatusDescriptionTextBox.Visible);
				AssertEquals("B3EntryStatusDescriptionTextBox.CharacterCasing", CharacterCasing.Upper, control.B3EntryStatusDescriptionTextBox.CharacterCasing);
				AssertEquals("B3EntryStatusDescription.Caption", "G7 Status", control.B3EntryStatusDescriptionTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("JE_TransportModeBoundDropDownEdit.Visible", true, control.JE_TransportModeBoundDropDownEdit.Visible);
				AssertEquals("ImporterOrgAddressControl.Visible", true, control.ImporterOrgAddressControl.Visible);
				AssertEquals("SupplierOrgAddressControl.Visible", true, control.SupplierOrgAddressControl.Visible);
				AssertEquals("ImporterDocumentaryAddress.Visible", true, control.ImporterDocumentaryAddress.Visible);

				entryHeader.CH_MessageType = MessageTypeList.Codes.GenericIMPResponse;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("SupplierOrganisationControl.Visible", false, control.SupplierOrganisationControl.Visible);
				AssertEquals("SupplierDocumentAddress.Visible", true, control.SupplierDocumentAddress.Visible);
				AssertEquals("SupplierDocumentAddress.Caption", "Main Vendor", control.SupplierDocumentAddress.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("ImporterOrganisationControl.Visible", true, control.ImporterOrganisationControl.Visible);
				AssertEquals("ForwarderOrganisationControl.Caption", "Forwarder", control.ForwarderOrganisationControl.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("EntryNumberLabel.Visible", false, control.ExportDeclarationNumberBoundTextBox.Visible);
				AssertEquals("TransactionNumberPanel.Visible", true, control.TransactionNumberPanel.Visible);
				AssertEquals("StatusTextBox.Visible", true, control.StatusTextBox.Visible);
				AssertEquals("B3EntryStatusDescriptionTextBox.Visible", true, control.B3EntryStatusDescriptionTextBox.Visible);
				AssertEquals("B3EntryStatusDescriptionTextBox.CharacterCasing", CharacterCasing.Upper, control.B3EntryStatusDescriptionTextBox.CharacterCasing);
				AssertEquals("B3EntryStatusDescription.Caption", "Entry Status", control.B3EntryStatusDescriptionTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("JE_TransportModeBoundDropDownEdit.Visible", true, control.JE_TransportModeBoundDropDownEdit.Visible);
				AssertEquals("JE_MessageSubTypeBoundDropDownEdit.Caption", "Entry Type", control.JE_MessageSubTypeBoundDropDownEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				control.RightTabControl.SelectedTab = control.OrganisationsTabPage;
				Assert("ImporterOfRecordDocAddressControl.Visible", control.ImporterOfRecordDocAddressControl.Visible);
				AssertEquals("ImporterOrgAddressControl.Visible", false, control.ImporterOrgAddressControl.Visible);
				AssertEquals("SupplierOrgAddressControl.Visible", false, control.SupplierOrgAddressControl.Visible);
				AssertEquals("ImporterDocumentaryAddress.Visible", true, control.ImporterDocumentaryAddress.Visible);
				AssertEquals("ImporterDocumentaryAddress.Caption", "Consignee", control.ImporterDocumentaryAddress.GetExtension<ILabelCaptionRenderer>().Caption);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("JE_ContainerModeBoundDropDownEdit.Visible", true, control.JE_ContainerModeBoundDropDownEdit.Visible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("JE_ContainerModeBoundDropDownEdit.Visible", true, control.JE_ContainerModeBoundDropDownEdit.Visible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("JE_ContainerModeBoundDropDownEdit.Visible", false, control.JE_ContainerModeBoundDropDownEdit.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				AssertEquals("JE_ContainerModeBoundDropDownEdit.Visible", false, control.JE_ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("JE_TransportModeBoundDropDownEdit.Visible", false, control.JE_TransportModeBoundDropDownEdit.Visible);
				AssertEquals("JE_MessageSubTypeBoundDropDownEdit.Caption", "LVS Type", control.JE_MessageSubTypeBoundDropDownEdit.GetExtension<ILabelCaptionRenderer>().Caption);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
					declaration.JE_MessageType = ZString.Empty;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("B3EntryStatusDescriptionTextBox.Visible", true, control.B3EntryStatusDescriptionTextBox.Visible);
					AssertEquals("B3EntryStatusDescriptionTextBox.CharacterCasing", CharacterCasing.Normal, control.B3EntryStatusDescriptionTextBox.CharacterCasing);
					AssertEquals("B3EntryStatusDescription.Caption", "CAD Status", control.B3EntryStatusDescriptionTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("B3CSubmittedDate.Caption", "CAD Submitted Time", control.B3CSubmittedDate.GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
			using (CACustomsDataRegistry.Instance.MakeSomeFieldsJobDocAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = (CAJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				form.Show();
				entryHeader.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
				declaration.JE_MessageType = ZString.Empty;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("ImporterDocumentaryAddress.Visible", false, control.ImporterDocumentaryAddress.Visible);
			}
		}

		public void TestB3ScheduleContolVisibility()
		{
			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions("Test display without schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					var messageStatusBox = testForm.Controls.Find("B3EntryStatusDescriptionTextBox", true)[0] as ZTextBox;
					Assert(!scheduledB3Box.Visible);
					Assert(messageStatusBox.Visible);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(388), messageStatusBox.Width);
				}
			});

			CombineAssertions("Test display with schedule B3 Message Time", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var testHeader = testDeclaration.ActiveEntryHeaders.AddNew();
				testHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testHeader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = ZDateTime.UtcNow;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					var messageStatusBox = testForm.Controls.Find("B3EntryStatusDescriptionTextBox", true)[0] as ZTextBox;
					Assert(scheduledB3Box.Visible);
					AssertEquals(Color.Yellow, scheduledB3Box.DateTextBox.BackColor);
					Assert(messageStatusBox.Visible);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160), messageStatusBox.Width);
				}
			});

			CombineAssertions("Test display with schedule on non IMP job", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var testHeader = testDeclaration.ActiveEntryHeaders.AddNew();
				testHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testHeader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = ZDateTime.UtcNow;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					var messageStatusBox = testForm.Controls.Find("B3EntryStatusDescriptionTextBox", true)[0] as ZTextBox;
					Assert(!scheduledB3Box.Visible);
					Assert(messageStatusBox.Visible);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(388), messageStatusBox.Width);
				}
			});

			CombineAssertions("Test display without Auto-Sending schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					var messageStatusBox = testForm.Controls.Find("B3EntryStatusDescriptionTextBox", true)[0] as ZTextBox;
					Assert(!scheduledB3Box.Visible);
					Assert(messageStatusBox.Visible);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(388), messageStatusBox.Width);
				}
			});

			CombineAssertions("Test display with Auto-Sending schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					var messageStatusBox = testForm.Controls.Find("B3EntryStatusDescriptionTextBox", true)[0] as ZTextBox;
					Assert(scheduledB3Box.Visible);
					AssertEquals(SystemColors.Control, scheduledB3Box.DateTextBox.BackColor);
					AssertEquals(testDeclaration.ScheduledB3AutoSendingDate.ToLongTimeString().ToUpper(), scheduledB3Box.Text);
					Assert(messageStatusBox.Visible);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160), messageStatusBox.Width);
				}
				JobDeclarationB3SendingStrategy.StrategiesForTesting = null;
			});

			CombineAssertions("Test display with schedule B3 Message Time", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				var testheader = testDeclaration.ActiveEntryHeaders.AddNew();
				testheader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testheader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = new ZDateTime(2015, 5, 19);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					var messageStatusBox = testForm.Controls.Find("B3EntryStatusDescriptionTextBox", true)[0] as ZTextBox;
					Assert(scheduledB3Box.Visible);
					AssertEquals(Color.Yellow, scheduledB3Box.DateTextBox.BackColor);
					Assert(messageStatusBox.Visible);
					AssertEquals(testDeclaration.ScheduledB3MessageTime.ToLongTimeString().ToUpper(), scheduledB3Box.Text);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160), messageStatusBox.Width);
				}
				JobDeclarationB3SendingStrategy.StrategiesForTesting = null;
			});

			CombineAssertions("Test display with Auto-Sending schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				var testheader = testDeclaration.ActiveEntryHeaders.AddNew();
				testheader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testheader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = new ZDateTime(2015, 5, 21);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					var messageStatusBox = testForm.Controls.Find("B3EntryStatusDescriptionTextBox", true)[0] as ZTextBox;
					Assert(scheduledB3Box.Visible);
					AssertEquals(SystemColors.Control, scheduledB3Box.DateTextBox.BackColor);
					Assert(messageStatusBox.Visible);
					AssertEquals(testDeclaration.ScheduledB3AutoSendingDate.ToLongTimeString().ToUpper(), scheduledB3Box.Text);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160), messageStatusBox.Width);
				}
				JobDeclarationB3SendingStrategy.StrategiesForTesting = null;
			});
		}

		public void TestTransactionNumberControlVisibility()
		{
			CombineAssertions("Test display with messages sent declaration", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryHeader = testDeclaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				entryHeader.Messages.AddNew();
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var formattedTransactionNumberTextBox = testForm.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = testForm.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = testForm.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = testForm.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(formattedTransactionNumberTextBox.Visible);
					Assert(!securityCodeTextBox.Visible);
					Assert(!sequentialNumberTextBox.Visible);
					Assert(!checkDigitTextBox.Visible);
				}
			});

			CombineAssertions("Test display with empty declaration", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var formattedTransactionNumberTextBox = testForm.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = testForm.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = testForm.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = testForm.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(!formattedTransactionNumberTextBox.Visible);
					Assert(securityCodeTextBox.Visible);
					Assert(sequentialNumberTextBox.Visible);
					Assert(checkDigitTextBox.Visible);
				}
			});

			CombineAssertions("Test display with registry set", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var formattedTransactionNumberTextBox = testForm.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = testForm.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = testForm.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = testForm.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(formattedTransactionNumberTextBox.Visible);
					Assert(!securityCodeTextBox.Visible);
					Assert(!sequentialNumberTextBox.Visible);
					Assert(!checkDigitTextBox.Visible);
				}
			});
		}

		public void TestShowWarningWhenUserClearReleaseDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
				Assert("No dialog should be popped up", UnitTestUserNotification.Instance.LastMessage.WasNone);
				declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
				Assert("A warning should be popped up", UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("You have cleared the Actual Release Date. As a result any automatic Entry will not be sent, late Entry warnings will not occur, and the Age in Days will show as zero. If the job has actually been released then this may result in a penalty for not reporting the Entry.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestJE_VoyageFlightNoBoundTextBoxVisibilityAndCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var voyageFlightNoBoundTextBox = (ZTextBox)form.CustomsBrokerageUserControl.Controls.Find("JE_VoyageFlightNoBoundTextBox", true)[0];
				var labelCaptionRenderer = voyageFlightNoBoundTextBox.Extensions.FirstOrDefault(x => x is LabelCaptionRenderer) as LabelCaptionRenderer;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("JE_VoyageFlightNoBoundTextBox should be visible", true, voyageFlightNoBoundTextBox.Visible);
				AssertEquals("Caption for JE_VoyageFlightNoBoundTextBox", "Flight/Folio", labelCaptionRenderer.Caption);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("JE_VoyageFlightNoBoundTextBox should be visible", true, voyageFlightNoBoundTextBox.Visible);
				AssertEquals("Caption for JE_VoyageFlightNoBoundTextBox", "Voyage", labelCaptionRenderer.Caption);

				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("JE_VoyageFlightNoBoundTextBox should be visible", true, voyageFlightNoBoundTextBox.Visible);
				AssertEquals("Caption for JE_VoyageFlightNoBoundTextBox", "Registration", labelCaptionRenderer.Caption);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("JE_VoyageFlightNoBoundTextBox should be visible", true, voyageFlightNoBoundTextBox.Visible);
				AssertEquals("Caption for JE_VoyageFlightNoBoundTextBox", "Rail Car No.", labelCaptionRenderer.Caption);

				declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("JE_VoyageFlightNoBoundTextBox should be visible", true, voyageFlightNoBoundTextBox.Visible);
				AssertEquals("Caption for JE_VoyageFlightNoBoundTextBox", "Transport Ref.", labelCaptionRenderer.Caption);

				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				AssertEquals("JE_VoyageFlightNoBoundTextBox should be visible", true, voyageFlightNoBoundTextBox.Visible);
				AssertEquals("Caption for JE_VoyageFlightNoBoundTextBox", "Transport Ref.", labelCaptionRenderer.Caption);

				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals("JE_VoyageFlightNoBoundTextBox should be visible", true, voyageFlightNoBoundTextBox.Visible);
				AssertEquals("Caption for JE_VoyageFlightNoBoundTextBox", "Transport Ref.", labelCaptionRenderer.Caption);

				declaration.JE_TransportMode = TransportTypeList.Codes.NoCarrier;
				AssertEquals("JE_VoyageFlightNoBoundTextBox should be visible", true, voyageFlightNoBoundTextBox.Visible);
				AssertEquals("Caption for JE_VoyageFlightNoBoundTextBox", "Transport Ref.", labelCaptionRenderer.Caption);

				declaration.JE_TransportMode = "XXX";
				AssertEquals("JE_VoyageFlightNoBoundTextBox should NOT be visible", false, voyageFlightNoBoundTextBox.Visible);
			}
		}

		public void TestSuppressImporterSecurityNumberPopup()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "98765");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABCDEFGH");
			CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP";
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "12345";
			addInfo.ZO_AccountSecirityPassword = "12345678";
			var importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.OH_Code = "IOR";
			var importerOfRecordAddInfo = OrgImpAddInfo.Get(importerOfRecord);
			importerOfRecordAddInfo.ZO_AccountSecurityNumber = "54321";
			importerOfRecordAddInfo.ZO_AccountSecirityPassword = "87654321";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("12345", declaration.TransactionNumber.AccountSecurityCode);

				declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
				AssertEquals("Do you want to use the Importer of Record's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("54321", declaration.TransactionNumber.AccountSecurityCode);
			}

			declaration.TransactionNumber.AccountSecurityCode = "98765";
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "98765");

			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				declaration.JE_OH_Importer = importer.PK;
				AssertNull("Account Security Number should NOT be changed", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("98765", declaration.TransactionNumber.AccountSecurityCode);

				declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
				AssertNull("Account Security Number should NOT be changed", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("98765", declaration.TransactionNumber.AccountSecurityCode);
			}
		}

		public void TestExceptionDescriptionLabelNotBlockOtherControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				var control = (CAJobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				declaration.CA_DeclarationException = CAExceptionCodeList.Codes.EntryLodgedAndAcceptedNotReportedONDN;

				var exceptionDescriptionLabel = control.Find(c => c.Name == "ExceptionDescriptionLabel").FirstOrDefault();
				AssertNotNull("ExceptionDescriptionLabel is not null", exceptionDescriptionLabel);
				AssertEquals("Exception Description Label is visible", true, exceptionDescriptionLabel.Visible);
				var releaseDateEdit = control.Find(c => c.Name == "ReleaseDateDateEdit").FirstOrDefault();
				AssertNotNull("ReleaseDateDateEdit is not null", releaseDateEdit);
				bool isBelowOARSETADateEdit = releaseDateEdit.Location.Y + releaseDateEdit.Height < exceptionDescriptionLabel.Location.Y;
				AssertEquals("Exception Description Label is below OARSETA DateEdit", true, isBelowOARSETADateEdit);
			}
		}

		public void TestScreeningControlsVisibilityLinkedWithShipmentOrNot()
		{
			AssertScreeningControlsVisibilityLinkedWithShipmentOrNot(true, false, false);
			AssertScreeningControlsVisibilityLinkedWithShipmentOrNot(false, true, false);
			AssertScreeningControlsVisibilityLinkedWithShipmentOrNot(false, false, true);
		}

		public void TestSuppressShipmentRelatedFieldWhenSelected()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.SuppressShipmentRelatedFields = true;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var portOfLoadingFindBox = form.Controls.Find("PortOfLoadingFindBox", true)[0] as ZCodeFindBox;
				var portOfDischargeFindBox = form.Controls.Find("PortOfDischargeFindBox", true)[0] as ZCodeFindBox;
				var originFindBox = form.Controls.Find("OriginFindBox", true)[0] as ZCodeFindBox;
				var jE_ExportDateBoundDateEdit2 = form.Controls.Find("JE_ExportDateBoundDateEdit2", true)[0] as ZDateEdit;
				var netWeightCalcDropEdit = form.Controls.Find("NetWeightCalcDropEdit", true)[0] as ZCalcDropEdit;
				var screeningStatusDropEdit = form.Controls.Find("ScreeningStatusDropEdit", true)[0] as ZDropEdit;
				var volumeCalcDropEdit = form.Controls.Find("VolumeCalcDropEdit", true)[0] as ZCalcDropEdit;
				var jE_DateOfArrivalBoundDateEdit = form.Controls.Find("JE_DateOfArrivalBoundDateEdit", true)[0] as ZDateEdit;

				Assert(!portOfLoadingFindBox.Visible);
				Assert(!portOfDischargeFindBox.Visible);
				Assert(!originFindBox.Visible);
				Assert(!jE_ExportDateBoundDateEdit2.Visible);
				Assert(!netWeightCalcDropEdit.Visible);
				Assert(!screeningStatusDropEdit.Visible);
				Assert(!volumeCalcDropEdit.Visible);
				AssertEquals(jE_DateOfArrivalBoundDateEdit.AutoValidate, AutoValidate.Disable);
			}
		}

		public void TestCCNWhenRegistDisplayCargoControlNumberSeparately()
		{
			CACustomsDataRegistry.Instance.DisplayCargoControlNumberSeparately.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var cargoControlNumberNoBoundTextBox = form.Controls.Find("CargoControlNumberNoBoundTextBox", true)[0] as ZTextBox;
				var cargoControlNumberPrefixNoBoundTextBox = form.Controls.Find("CargoControlNumberPrefixNoBoundTextBox", true)[0] as ZTextBox;
				var cargoControlNumberSuffixNoBoundTextBox = form.Controls.Find("CargoControlNumberSuffixNoBoundTextBox", true)[0] as ZTextBox;

				Assert(!cargoControlNumberNoBoundTextBox.Visible);
				Assert(cargoControlNumberPrefixNoBoundTextBox.Visible);
				Assert(cargoControlNumberSuffixNoBoundTextBox.Visible);
			}
		}

		public void TestdefaultHWBWhentransportModeIsAir()
		{
			CACustomsDataRegistry.Instance.DisplayCargoControlNumberSeparately.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.EffectiveCCNPrefix = "4265";
			declaration.EffectiveCCNSuffix = "2378613968";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var defaultHWBButton = form.Controls.Find("DefaultHWBButton", true)[0] as ZButton;

				Assert(!defaultHWBButton.Visible);
				AssertEquals(declaration.JE_HouseBill, declaration.EffectiveCCNSuffix);
			}
		}

		public void TestComponentPropertyInfoHumanReadableName()
		{
			var declaration = Factory.NewWithValidTestData<DummyDeclaration>();
			var child1 = declaration.ComponentsForTest1.AddNew();
			var child2 = declaration.ComponentsForTest2.AddNew();

			using (var form = new ZForm(declaration))
			using (var grid1 = new ZGrid())
			using (var grid2 = new ZGrid())
			{
				grid1.ColumnStyles.Add(
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = "CA_Qty",
						Caption = "Test Qty1"
					}
				);

				form.Controls.Add(grid1);

				grid1.CopyCaptionsToPropertyHumanReadableNameForTest = true;
				grid1.SetDataBinding(declaration, nameof(DummyDeclaration.ComponentsForTest1));

				grid2.ColumnStyles.Add(
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = "CA_Qty",
						Caption = "Test Qty2"
					}
				);
				form.Controls.Add(grid2);

				grid2.CopyCaptionsToPropertyHumanReadableNameForTest = true;
				grid2.SetDataBinding(declaration, nameof(DummyDeclaration.ComponentsForTest2));

				var cusAddInfo1 = new ComponentAddInfo(child1.B7_AddInfoDataInfo);
				var cusAddInfo2 = new ComponentAddInfo(child2.B7_AddInfoDataInfo);

				var humanReadableNameProviderManager = Factory.ServiceContainer.GetService<IHumanReadableNameProvider>();
				AssertEquals("Test Qty1", humanReadableNameProviderManager.GetHumanReadableName(cusAddInfo1.CA_QtyInfo));
				AssertEquals("Test Qty2", humanReadableNameProviderManager.GetHumanReadableName(cusAddInfo2.CA_QtyInfo));
			}
		}

		public void TestSubmitTypeControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var submitTypeDropEdit = form.Controls.Find("JE_ApplicationCodeBoundDropEdit", true)[0] as ZDropEdit;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(true, submitTypeDropEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(true, submitTypeDropEdit.Visible);
				declaration.JE_MessageType = "!@#";
				AssertEquals(true, submitTypeDropEdit.Visible);
			}
		}

		void AssertScreeningControlsVisibilityLinkedWithShipmentOrNot(bool linkedWithShipment, bool enableCompliance, bool expectedToBeVisible)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.SuppressShipmentRelatedFields = false;

			if (linkedWithShipment)
			{
				var shipment = Factory.New<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;
			}

			var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			using (ObjectFactory.Substitute(featureControlMock.Object))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var screeningStatusDropEdit = form.Controls.Find("ScreeningStatusDropEdit", true)[0] as ZDropEdit;
				var screenButton = form.Controls.Find("ScreenButton", true)[0] as ZButton;
				AssertEquals(expectedToBeVisible, screeningStatusDropEdit.Visible);
				AssertEquals(expectedToBeVisible, screenButton.Visible);
			}
		}

		sealed class DummyDeclaration : JobDeclaration
		{
			public DummyDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ComponentCollection ComponentsForTest1
			{
				get
				{
					if (fComponentsForTest1 == null)
					{
						fComponentsForTest1 = new ComponentCollection(this);
						fComponentsForTest1.Load();
						RegisterEditableChildObject(fComponentsForTest1);
					}
					return fComponentsForTest1;
				}
			}
			ComponentCollection fComponentsForTest1;

			public ComponentCollection ComponentsForTest2
			{
				get
				{
					if (fComponentsForTest2 == null)
					{
						fComponentsForTest2 = new ComponentCollection(this);
						fComponentsForTest2.Load();
						RegisterEditableChildObject(fComponentsForTest2);
					}
					return fComponentsForTest2;
				}
			}
			ComponentCollection fComponentsForTest2;
		}
	}
}
