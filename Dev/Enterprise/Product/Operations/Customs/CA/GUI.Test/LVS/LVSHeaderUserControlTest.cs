using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVSHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestCA_DeclarationExceptionInfo_ValueChangedEventHandlerRemovedWhenControlDispose()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var valueChangedDictionary = GetBizObjectValueChangedDictionary(declaration);
				valueChangedDictionary.TryGetValue(CAAddInfoSchema.Constants.CA_DeclarationException, out var declarationException);
				AssertNotNull(declarationException);
				AssertEquals("CA_DeclarationExceptionInfo_ValueChanged", declarationException.FirstOrDefault().Value.Method.Name);
			}

			var valueChangedDictionary1 = GetBizObjectValueChangedDictionary(declaration);
			valueChangedDictionary1.TryGetValue(CAAddInfoSchema.Constants.CA_DeclarationException, out var declarationException1);
			AssertNull(declarationException1);
		}

		Dictionary<string, Dictionary<BusinessObject, EventHandler>> GetBizObjectValueChangedDictionary(BusinessObject bizObject)
		{
			var propertyInfoStorageField = bizObject.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(bizObject.Factory);
			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);
			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			return (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);
		}

		public void TestCADSubmittedDateEditAndCADStatusTextBox()
		{
			using (var control = new LVSHeaderUserControl())
			{
				var cADSubmittedDateEdit = control.Controls.Find("CADSubmittedDateEdit", true)[0];
				AssertNotNull(cADSubmittedDateEdit);
				var cADStatusTextBox = control.Controls.Find("CADStatusTextBox", true)[0];
				AssertNotNull(cADSubmittedDateEdit);
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

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
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
			}

			CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_OH_Importer = importer.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("12345", declaration.TransactionNumber.AccountSecurityCode);
			}
		}

		public void TestB3ScheduleContolVisibility()
		{
			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions("Test display without schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(!scheduledB3Box.Visible);
				}
			});

			CombineAssertions("Test display with schedule B3 Message Time", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var testheader = testDeclaration.ActiveEntryHeaders.AddNew();
				testheader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testheader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = ZDateTime.UtcNow;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(scheduledB3Box.Visible);
					AssertEquals("BackColorCheck", Color.Yellow, scheduledB3Box.DateTextBox.BackColor);
				}
			});

			CombineAssertions("Test display with Auto-Sending schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(scheduledB3Box.Visible);
					AssertEquals("BackColorCheck", SystemColors.Control, scheduledB3Box.DateTextBox.BackColor);
					AssertEquals(new ZDateTime(2015, 5, 20).ToLongTimeString().ToUpper(), scheduledB3Box.Text);
				}
				JobDeclarationB3SendingStrategy.StrategiesForTesting = null;
			});

			CombineAssertions("Test display with schedule B3 Message Time", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var testheader = testDeclaration.ActiveEntryHeaders.AddNew();
				testheader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testheader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = new ZDateTime(2015, 5, 19);
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(scheduledB3Box.Visible);
					AssertEquals("BackColorCheck", Color.Yellow, scheduledB3Box.DateTextBox.BackColor);
					AssertEquals(testDeclaration.ScheduledB3MessageTime.ToLongTimeString().ToUpper(), scheduledB3Box.Text);
				}
			});

			CombineAssertions("Test display with Auto-Sending schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var testheader = testDeclaration.ActiveEntryHeaders.AddNew();
				testheader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testheader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = new ZDateTime(2015, 5, 21);
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var scheduledB3Box = testForm.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(scheduledB3Box.Visible);
					AssertEquals("BackColorCheck", SystemColors.Control, scheduledB3Box.DateTextBox.BackColor);
					AssertEquals(testDeclaration.ScheduledB3AutoSendingDate.ToLongTimeString().ToUpper(), scheduledB3Box.Text);
				}
				JobDeclarationB3SendingStrategy.StrategiesForTesting = null;
			});
		}

		public void TestAllowOICCheckBoxVisibility()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var allowOICCheckBox = testForm.Controls.Find("AllowOICCheckBox", true)[0] as ZCheckBox;
				Assert("Visible", allowOICCheckBox.Visible);
			}
			testDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var allowOICCheckBox = testForm.Controls.Find("AllowOICCheckBox", true)[0] as ZCheckBox;
				Assert("Visible", !allowOICCheckBox.Visible);
			}
		}

		public void TestCADSubmittedDateEditAndCADStatusTextBoxVisibility()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				testDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var cadSubmittedDateEdit = testForm.Controls.Find("CADSubmittedDateEdit", true)[0] as ZDateEdit;
					Assert("IsCADEnable == true", cadSubmittedDateEdit.Visible);
					var cadStatusTextBox = testForm.Controls.Find("CADStatusTextBox", true)[0] as ZTextBox;
					Assert("IsCADEnable == true", cadStatusTextBox.Visible);
				}
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				testDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var cadSubmittedDateEdit = testForm.Controls.Find("CADSubmittedDateEdit", true)[0] as ZDateEdit;
					Assert("IsCADEnable == false", !cadSubmittedDateEdit.Visible);
					var cadStatusTextBox = testForm.Controls.Find("CADStatusTextBox", true)[0] as ZTextBox;
					Assert("IsCADEnable == false", !cadStatusTextBox.Visible);
				}
			}
		}

		public void TestLVSLinesGridCaptions()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "org1";
			orgHeader1.MiscServ.OM_IMPartAttrib1Name = "org1 attr 1";
			orgHeader1.MiscServ.OM_IMPartAttrib2Name = "org1 attr 2";
			orgHeader1.MiscServ.OM_IMPartAttrib3Name = "org1 attr 3";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "org2";
			orgHeader2.MiscServ.OM_IMPartAttrib1Name = "org2 attr 1";
			orgHeader2.MiscServ.OM_IMPartAttrib2Name = "org2 attr 2";
			orgHeader2.MiscServ.OM_IMPartAttrib3Name = "org2 attr 3";

			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice1 = testDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = testDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.JZ_OH_Buyer = orgHeader2.PK;
			Factory.Save();
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var fLVSLinesGrid = testForm.Controls.Find("LVSLinesGrid", true)[0] as ZGrid;
				CombineAssertions("Default part attribute captions", () =>
				{
					AssertEquals("Part Attrib. 1", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertEquals("Part Attrib. 2", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertEquals("Part Attrib. 3", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				});

				var lvsSubHeadersGrid = testForm.Controls.Find("LVSSubHeadersGrid", true)[0] as LVSSubHeadersGrid;
				lvsSubHeadersGrid.ListManager.Position = 1;
				CombineAssertions("Customized part attribute captions by buyer", () =>
				{
					AssertEquals("org2 attr 1", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertEquals("org2 attr 2", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertEquals("org2 attr 3", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				});

				invoice1.JZ_OH_Buyer = orgHeader1.PK;
				CombineAssertions("Customized part attribute captions by buyer", () =>
				{
					AssertEquals("org2 attr 1", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertEquals("org2 attr 2", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertEquals("org2 attr 3", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				});

				lvsSubHeadersGrid.ListManager.Position = 0;
				CombineAssertions("Customized part attribute captions by importer", () =>
				{
					AssertEquals("org1 attr 1", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertEquals("org1 attr 2", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertEquals("org1 attr 3", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				});
			}
		}

		public void TestTransactionNumberControlVisibility()
		{
			CombineAssertions("Test when registry is set to NO", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var formattedTransactionNumberTextBox = form.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = form.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = form.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = form.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(formattedTransactionNumberTextBox.Visible);
					Assert(!securityCodeTextBox.Visible);
					Assert(!sequentialNumberTextBox.Visible);
					Assert(!checkDigitTextBox.Visible);
				}
			});

			CombineAssertions("Test when registry is overridden to YES", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var formattedTransactionNumberTextBox = form.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = form.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = form.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = form.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(!formattedTransactionNumberTextBox.Visible);
					Assert(securityCodeTextBox.Visible);
					Assert(sequentialNumberTextBox.Visible);
					Assert(checkDigitTextBox.Visible);
				}
			});
		}
	}
}
