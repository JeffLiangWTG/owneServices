using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
	{
		public override void TestMinimumSizeNotTooBig()
		{
			var minScreenWidthSupported = ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);
			var minScreenHeightSupported = ControlDpiScalingHelper.ScaleToCurrentDpiY(950);
			using (var form = new JobDeclarationForm(declaration))
			{
				Assert("DE Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("DE Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}

		public void TestEDIMenu()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
				AssertType<EDIMenu>(menu);
			}
		}

		public void TestRecalculateNetPriceMenu_Enabled()
		{
			CombineAssertions(() =>
			{
				using (var form = new JobDeclarationForm(declaration))
				{
					var actionMenu = form.Menu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Actio&ns");
					var recalculateNetPriceMenu = actionMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Recalculate Net Price");
					AssertNotNull("Menu is existing", recalculateNetPriceMenu);

					actionMenu.OnPopup(EventArgs.Empty);
					AssertEquals("Menu is disable default", false, recalculateNetPriceMenu.Enabled);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					actionMenu.OnPopup(EventArgs.Empty);
					AssertEquals("Menu is disable when JE_MessageType = IMP and ZG_IsHighValueOvrd is false", false, recalculateNetPriceMenu.Enabled);

					declaration.ZG_IsHighValueOvrd = true;
					actionMenu.OnPopup(EventArgs.Empty);
					AssertEquals("Menu is enabled when JE_MessageType = IMP and ZG_IsHighValueOvrd is true", true, recalculateNetPriceMenu.Enabled);
				}
			});
		}

		public void TestRecalculateNetPriceMenu_Click()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 25000m;
			var discount = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);
			discount.J7_Amount = 500m;
			discount.J7_RX_NKCurrency = "USD";
			invoiceLine.JI_NetPrice = 5555m;

			CombineAssertions(() =>
			{
				using (var form = new JobDeclarationForm(declaration))
				{
					var actionMenu = form.Menu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Actio&ns");
					var recalculateNetPriceMenu = actionMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Recalculate Net Price");

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddUserResponse("yes");

					recalculateNetPriceMenu.PerformClick();
					AssertMultilineASCIIEquals("Notice", @"
To calculate the Net Price the following fields must be filled:

-Invoice Currency
-[42] Price
-at least one charge code of type DIS

Do you really want to recalculate the Net Price? This will override an existing value!"
						, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Recalculated", 24640.29m, invoiceLine.JI_NetPrice);
				}
			});
		}

		public void TestPreviousDocumentConfirmationInstruction()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._447T2;
					AssertEquals("Value has not changed as cancel selected", PreviousProcedureList.Codes._ATAV, instruction.PreviousDocumentMaster.CSI_Procedure);
					AssertEquals("Message on display was", ClearPreviousDocumentsMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._447T2;
					AssertEquals("Value has changed as Ok selected", PreviousProcedureList.Codes._447T2, instruction.PreviousDocumentMaster.CSI_Procedure);
				});
			}
		}

		public void TestIncotermKeyChangeNotification()
		{
			const string notificationMessage = "Incoterm Key has been changed: Please make sure all entered charges against this Invoice are still correct!";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.ZG_AgreedPlaceCode = "1";

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				CombineAssertions(() =>
				{
					invHeader.ZG_AgreedPlaceCode = "2";
					Assert("No message shown when no charges", UnitTestUserNotification.Instance.LastMessage.WasNone);

					invHeader.Charges.AddNew();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					invHeader.ZG_AgreedPlaceCode = "3";
					Assert("No message shown for Export", UnitTestUserNotification.Instance.LastMessage.WasNone);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					invHeader.ZG_AgreedPlaceCode = "4";

					AssertEquals("Message was displayed for invoice header charge", notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Message was information", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

					UnitTestUserNotification.Instance.ClearMessages();
					invHeader.Charges.RemoveAll();
					var invoiceLine = invHeader.InvoiceLines.AddNew();
					invoiceLine.Charges.AddNew();
					invHeader.ZG_AgreedPlaceCode = "5";

					AssertEquals("Message was displayed for invoice line charge", notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					invoiceLine.Charges.RemoveAll();
					invoiceLine.ApportionedCharges.AddNew();
					invHeader.ZG_AgreedPlaceCode = "6";
					AssertEquals("Message was displayed for invoice line apportioned charge", notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					invHeader.InvoiceLines.RemoveAll();
					invHeader.GroupCharges.AddNew();
					invHeader.ZG_AgreedPlaceCode = "7";

					AssertEquals("Message was displayed for invoice header apportioned charge", notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					invHeader.GroupCharges.RemoveAll();
					invHeader.JobDeclaration.TopGroupInvoice.Charges.AddNew();
					invHeader.ZG_AgreedPlaceCode = "8";
					AssertEquals("Message was displayed for invoice header group charge", notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestPreviousDocumentConfirmationInvoiceLine()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CombineAssertions(() =>
				{
					invoiceLine.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._AE;
					AssertEquals("Value has not changed as cancel selected", PreviousProcedureList.Codes._ATAV, invoiceLine.PreviousDocumentMaster.CSI_Procedure);
					AssertEquals("Message on display was", ClearPreviousDocumentsMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					invoiceLine.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._AE;
					AssertEquals("Value has changed as Ok selected", PreviousProcedureList.Codes._AE, invoiceLine.PreviousDocumentMaster.CSI_Procedure);
				});
			}
		}

		public void TestInvoiceLinesTab_WhenFormMinimumSize_RightPanelShouldBeVisible()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				Application.DoEvents();

				var groupBox = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.InvoiceLinesSummaryGroupBox;
				form.Size = form.MinimumSize;
				Application.DoEvents();

				AssertLessThan("The group box should be fully visible even when the form is set to its minimum size.", groupBox.Right, form.Width);
			}
		}

		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			declaration.CusContainers.AddNew();
			var invoiceheader = declaration.Invoices.AddNew();
			invoiceheader.InvoiceLines.AddNew();
			declaration.Bills.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.MergedLines.AddNew();
			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		const string ClearPreviousDocumentsMessage = "The System is about to delete all existing Previous Document lines.\r\nDo you want to continue?";
	}

	public abstract class JobDeclarationFormPerformanceTest : EU.GUI.Testing.JobDeclarationFormPerformanceTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

		Dictionary<string, int> DEBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> DEBaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ RefPacksSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> DEBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ RefPacksSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> DEBaseFormMergeExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> DEBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> DEBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
		{
			{ JobDocAddressSchema.Constants.TableName, 5 },
			{ OrgHeaderSchema.Constants.TableName, 13 }
		};
		Dictionary<string, int> DEBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> DEBaseDeleteExpectedHits => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> DELoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> DEValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> DELightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> DEFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> DEUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> DEUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
		{
			{ OrgAddressSchema.Constants.TableName, 11 }
		};
		protected virtual Dictionary<string, int> DEUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{
			{ CusPermitHeaderSchema.Constants.TableName, 7 },
			{ OrgAddressSchema.Constants.TableName, 11 },
		};
		protected virtual Dictionary<string, int> DEDeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> EULoadEditableChildObjectsExpectedHits => ZipDictionaries(DEBaseLoadEditableChildObjectsExpectedHits, DELoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> EUValidateAllExpectedHits => ZipDictionaries(DEBaseValidateAllExpectedHits, DEValidateAllExpectedHits);
		protected override Dictionary<string, int> EULightFormValidationAndSaveExpectedHits => ZipDictionaries(DEBaseLightFormValidationAndSaveExpectedHits, DELightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> EUFormMergeExpectedHits => ZipDictionaries(DEBaseFormMergeExpectedHits, DEFormMergeExpectedHits);
		protected override Dictionary<string, int> EUUniversalXMLExportExpectedHits => ZipDictionaries(DEBaseUniversalXMLExportExpectedHits, DEUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> EUUniversalXMLImportUpdateExpectedHits => ZipDictionaries(DEBaseUniversalXMLImportUpdateExpectedHits, DEUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> EUUniversalXMLAddExpectedHits => ZipDictionaries(DEBaseUniversalXMLAddExpectedHits, DEUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> EUDeleteExpectedHits => ZipDictionaries(DEBaseDeleteExpectedHits, DEDeleteExpectedHits);
	}
}
