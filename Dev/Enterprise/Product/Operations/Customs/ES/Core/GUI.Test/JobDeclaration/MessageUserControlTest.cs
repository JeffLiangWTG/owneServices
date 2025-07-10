using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Integration.Customs;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusExitControlHeader = Enterprise.Customs.ES.Business.Declaration.CusExitControlHeader;
using CusExitDetail = Enterprise.Customs.ES.Business.Declaration.CusExitDetail;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;
using NumberFountains = Enterprise.NumberFountain.NumberFountains;

namespace Enterprise.Customs.ES.GUI.Testing;

public class MessageUserControlTest : MessageUserControlForVirtualPropertiesTest<MessageUserControl>
{
	public virtual void TestSetUpEntryLineGridColumns()
	{
		var testDec = Factory.NewWithValidTestData<JobDeclaration>();
		testDec.JE_ApplicationCode = "BLT";
		testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		testDec.CustomsEntryInstructions.RemoveAndDeleteAll();

		var invoiceHeader = testDec.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			var mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			using (var form = new ZForm(testDec))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				var entryLineGrid = userControl.Controls.Find("EntryLineGrid", true).First() as ZGrid;

				AssertZCalcEditColumnStyleInfo((ZCalcEditColumnStyleInfo)FindColumnByName(entryLineGrid, "TotalGrossWeightInKG"), "Gross Weight", 5);
				AssertZCalcEditColumnStyleInfo((ZCalcEditColumnStyleInfo)FindColumnByName(entryLineGrid, "VehiclesOrPackagesQty"), "Packages", 0);
				AssertZTextBoxColumnStyleInfo((ZTextBoxColumnStyleInfo)FindColumnByName(entryLineGrid, "ProcedureCodeWithoutConcession"), "CPC");
				AssertZTextBoxColumnStyleInfo((ZTextBoxColumnStyleInfo)FindColumnByName(entryLineGrid, "CountryOfOriginCode"), "Origin");
				AssertZTextBoxColumnStyleInfo((ZTextBoxColumnStyleInfo)FindColumnByName(entryLineGrid, "PreferenceCode"), "Preference");

				void AssertZCalcEditColumnStyleInfo(ZCalcEditColumnStyleInfo column, string expectedCaption, int expectedDecimals)
				{
					AssertZTextBoxColumnStyleInfo(column, expectedCaption);
					AssertEquals($"Allows {expectedDecimals} decimals", expectedDecimals, column.Decimals);
				}

				void AssertZTextBoxColumnStyleInfo(ZTextBoxColumnStyleInfo column, string expectedCaption)
				{
					AssertNotNull($"Coulumn '{expectedCaption}' missing", column);
					AssertEquals("Caption", expectedCaption, column.CaptionResourceString.Caption);
				}
			}
		});
	}

	public void TestEntryLinesMessagesTabControlPages()
	{
		using (var userControl = new MessageUserControl())
		{
			var tabControl = (ZTabControl)userControl.Controls.Find("EntryLinesMessagesTabControl", true).FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertNotNull(tabControl);
				AssertEquals("NewEntryDetailsTabPage", tabControl.TabPages[0].Name);
				AssertEquals("EntryLinesTabPage", tabControl.TabPages[1].Name);
				AssertEquals("AnnexTabPage", tabControl.TabPages[2].Name);
				AssertEquals("MessageTabPage", tabControl.TabPages[3].Name);
			});
		}
	}

	protected override ZBool DefaultDynamicLayoutApplied => ZBool.True;

	public virtual void TestSetupEntryHeaderColumns()
	{
		var testDec = Factory.New<JobDeclaration>();

		using (var form = new ZForm(testDec))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			CombineAssertions(() =>
			{
				var acceptanceDateColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate];
				AssertNotNull("User control should have Acceptance Date column", acceptanceDateColumn);
				AssertEquals("Acceptance Date column name is correct", "Acceptance Date", acceptanceDateColumn.ColumnStyle.HeaderText);

				var circuitColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.FormattedCircuit];
				AssertNotNull("User control should have Circuit column", circuitColumn);
				AssertEquals("Circuit column name is correct", "Circuit", circuitColumn.ColumnStyle.HeaderText);

				var circuitCanColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.FormattedCircuitCan];
				AssertNotNull("User control should have Circuit Can column", circuitCanColumn);
				AssertEquals("Circuit Can column name is correct", "Circuit Can", circuitCanColumn.ColumnStyle.HeaderText);

				var csvClearanceColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CSVClearance];
				AssertNotNull("User control should have CSV Clearance column", csvClearanceColumn);
				AssertEquals("CSV Clearance column name is correct", "CSV Clearance", csvClearanceColumn.ColumnStyle.HeaderText);

				AssertNotNull("Context menu to reset canceled entry exists", userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Reset Canceled Entry"));

				AssertNotNull("Context menu to request inbox notifications for the entries exists", userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications"));

				AssertNotNull("Context menu to generate exit control movements from the entries exists", userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control"));

				AssertNotNull("Context menu to update csv clearance of an entry exists", userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance"));

				AssertNotNull("Context menu to view entries on customs website", userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website"));

				AssertNotNull("Context menu to re-eneble annexes messages", userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Re-enable Annexes Messages"));

				AssertNotNull("Context menu to request effective departure certificate", userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Request Certificate of Effective Departure"));

				var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
				AssertNotNull("Context menu for PUE Requests", pueRequestsMenuItem);
				AssertNotNull("Context menu to send pue annex documents", pueRequestsMenuItem.MenuItems.FindByText("Send Annex Documents"));
				AssertNotNull("Context menu to send pue message", pueRequestsMenuItem.MenuItems.FindByText("Send Message"));

				var rohsraeeMenuItem = pueRequestsMenuItem.MenuItems.FindByText("ROHS-RAEE");
				AssertNotNull("Context menu for ROHS-RAEE", rohsraeeMenuItem);
				AssertNotNull("Context menu to request rohs-raee certificate", rohsraeeMenuItem.MenuItems.FindByText("Request Certificate"));
				AssertNotNull("Context menu to send rohs-raee additional data", rohsraeeMenuItem.MenuItems.FindByText("Send Additional Data"));
				AssertNotNull("Context menu to query rohs-raee existing certificates", rohsraeeMenuItem.MenuItems.FindByText("Query Existing Certificates"));

				var comMenuItem = pueRequestsMenuItem.MenuItems.FindByText("COM");
				AssertNotNull("Context menu for COM", comMenuItem);
				AssertNotNull("Context menu to request com certificate", comMenuItem.MenuItems.FindByText("Request Certificate"));
				AssertNotNull("Context menu to send com additional data", comMenuItem.MenuItems.FindByText("Send Additional Data"));
				AssertNotNull("Context menu to query com existing certificates", comMenuItem.MenuItems.FindByText("Query Existing Certificates"));

				var ecoMenuItem = pueRequestsMenuItem.MenuItems.FindByText("ECO");
				AssertNotNull("Context menu for ECO", ecoMenuItem);
				AssertNotNull("Context menu to request com certificate", ecoMenuItem.MenuItems.FindByText("Request Certificate"));
				AssertNotNull("Context menu to send com additional data", ecoMenuItem.MenuItems.FindByText("Send Additional Data"));
				AssertNotNull("Context menu to query com existing certificates", ecoMenuItem.MenuItems.FindByText("Query Existing Certificates"));

				var createSupplementaryFromSimplifiedMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Create Supplementary Entry from Simplified Entry");
				AssertNotNull("Context menu for Create Supplementary Entry from Simplified Entry", createSupplementaryFromSimplifiedMenuItem);
				AssertNotNull("Context menu to new related declaration", createSupplementaryFromSimplifiedMenuItem.MenuItems.FindByText("New Related Declaration"));
				AssertNotNull("Context menu to New entry and instruction", createSupplementaryFromSimplifiedMenuItem.MenuItems.FindByText("New Entry and Instruction"));
			});
		}
	}

	public void TestGenerateExitControlMenuItemVisibility()
	{
		var testDec = Factory.New<JobDeclaration>();
		var registry = ObjectFactory.Get<EUExitControl.IExitControlCustomsDataRegistry>();

		CombineAssertions(() =>
		{
			using (registry.EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(testDec))
				using (var userControl = GetControlToTest())
				{
					form.Controls.Add(userControl);
					form.SetDataBinding(testDec, ".");
					form.Show();

					testDec.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals("Menu is visible for export when registry's EnableExitControl is true", true, userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control").Visible);

					testDec.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("Menu is not visible for import when registry's EnableExitControl is true", false, userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control").Visible);
				}
			}

			using (registry.EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				using (var form = new ZForm(testDec))
				using (var userControl = GetControlToTest())
				{
					form.Controls.Add(userControl);
					form.SetDataBinding(testDec, ".");
					form.Show();

					testDec.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals("Menu is not visible for export when registry's EnableExitControl is false", false, userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control").Visible);

					testDec.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("Menu is not visible for import when registry's EnableExitControl is false", false, userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control").Visible);
				}
			}
		});
	}

	public void TestUpdateCSVClearanceMenuItemVisibility()
	{
		var testDec = Factory.New<JobDeclaration>();

		using (var form = new ZForm(testDec))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			var updateCSVMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

			CombineAssertions(() =>
			{
				testDec.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Menu is visible for import", true, updateCSVMenuItem.Visible);

				testDec.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Menu is visible for export", true, updateCSVMenuItem.Visible);
			});
		}
	}

	public void TestRequestCertEffectiveDepartureMenuItemVisibility()
	{
		var testDec = Factory.New<JobDeclaration>();

		using (var form = new ZForm(testDec))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			var requestDepartureCertMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Request Certificate of Effective Departure");

			CombineAssertions(() =>
			{
				testDec.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Menu is not visible for import", false, requestDepartureCertMenuItem.Visible);

				testDec.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Menu is visible for export", true, requestDepartureCertMenuItem.Visible);
			});
		}
	}

	public void TestPUEMenuItemVisibility()
	{
		var testDec = Factory.New<JobDeclaration>();

		using (var form = new ZForm(testDec))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");

			CombineAssertions(() =>
			{
				testDec.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Menu is visible for import", true, pueRequestsMenuItem.Visible);

				testDec.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Menu is not visible for export", false, pueRequestsMenuItem.Visible);
			});
		}
	}

	public void TestResetCancelledEntry()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cancelled;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var resetCancelledEntryMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Reset Canceled Entry");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				resetCancelledEntryMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				resetCancelledEntryMenuItem.PerformClick();
				AssertEquals("Can only reset canceled entries", "Please select only Canceled or Invalidated Entries", UnitTestUserNotification.Instance.LastMessage.Text);

				entryHeader2.CH_EntryStatus = EntryStatusCodes.Invalidated;
				declaration.Factory.Save();

				declaration.HasChanges = true;
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before resetting entry", declaration.Factory);
				resetCancelledEntryMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After resetting entry", declaration.Factory);

				AssertEquals("Should have message asking to save the declaration before resetting", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				AssertEquals("Entries were reset correctly", "2 Entries were reset and sending data deleted", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Entry 1 CH_EntryStatus is empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("Entry 2 CH_EntryStatus is empty", ZString.Empty, entryHeader2.CH_EntryStatus);
			});
		}
	}

	#region Request Inbox Notifications

	public void TestRequestInboxNotifications_Validations()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true);
		declaration.JE_GS_NKCusAgent = ZString.Empty;
		declaration.JE_CustomsProfile = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var requestInboxNotifEntryMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusCDA_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_EHub(declaration, entryHeader1, "2 In-box Notification requests created",
												new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusCDA_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_xT(declaration, entryHeader1, "2 In-box Notification requests created",
													new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication },
													new ZString[] { InboxNotificationResponseTypes.AESClearance, InboxNotificationResponseTypes.AESNonConformity });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusCDA_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_EHub(declaration, entryHeader1, entryHeader2, entryHeader3, "4 In-box Notification requests created",
														new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusCDA_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_xT(declaration, entryHeader1, entryHeader2, entryHeader3, "4 In-box Notification requests created",
															new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication },
															new ZString[] { InboxNotificationResponseTypes.AESClearance, InboxNotificationResponseTypes.AESNonConformity });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusPDA_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_EHub(declaration, entryHeader1, "1 In-box Notification request created",
												new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusPDA_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_xT(declaration, entryHeader1, "1 In-box Notification request created",
													new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication },
													new ZString[] { InboxNotificationResponseTypes.AESInvalidation });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusPDA_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_EHub(declaration, entryHeader1, entryHeader2, entryHeader3, "2 In-box Notification requests created",
														new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusPDA_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_xT(declaration, entryHeader1, entryHeader2, entryHeader3, "2 In-box Notification requests created",
															new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication },
															new ZString[] { InboxNotificationResponseTypes.AESInvalidation });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusPCO_WithoutCSVClearance_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_EHub(declaration, entryHeader1, "3 In-box Notification requests created",
												new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusPCO_WithoutCSVClearance_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_xT(declaration, entryHeader1, "3 In-box Notification requests created",
													new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication },
													new ZString[] { InboxNotificationResponseTypes.AESClearance, InboxNotificationResponseTypes.AESNonConformity, InboxNotificationResponseTypes.AESCceControl });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusPCO_WithoutCSVClearance_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_EHub(declaration, entryHeader1, entryHeader2, entryHeader3, "6 In-box Notification requests created",
														new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusPCO_WithoutCSVClearance_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_xT(declaration, entryHeader1, entryHeader2, entryHeader3, "6 In-box Notification requests created",
															new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication },
															new ZString[] { InboxNotificationResponseTypes.AESClearance, InboxNotificationResponseTypes.AESNonConformity, InboxNotificationResponseTypes.AESCceControl });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusPCO_WithCSVClearance_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader1.ZG_UCC6Version = 1;
		entryHeader1.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_EHub(declaration, entryHeader1, "1 In-box Notification request created",
												new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusPCO_WithCSVClearance_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader1.ZG_UCC6Version = 1;
		entryHeader1.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_xT(declaration, entryHeader1, "1 In-box Notification request created",
													new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication },
													new ZString[] { InboxNotificationResponseTypes.AESInvalidation });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusPCO_WithCSVClearance_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader1.ZG_UCC6Version = 1;
		entryHeader1.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader2.ZG_UCC6Version = 1;
		entryHeader2.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;
		entryHeader3.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_EHub(declaration, entryHeader1, entryHeader2, entryHeader3, "2 In-box Notification requests created",
														new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusPCO_WithCSVClearance_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader1.ZG_UCC6Version = 1;
		entryHeader1.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PendingForEuOffice;
		entryHeader2.ZG_UCC6Version = 1;
		entryHeader2.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;
		entryHeader3.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_xT(declaration, entryHeader1, entryHeader2, entryHeader3, "2 In-box Notification requests created",
															new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication },
															new ZString[] { InboxNotificationResponseTypes.AESInvalidation });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusCCO_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.ControlsAtEuOffice;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_EHub(declaration, entryHeader1, "3 In-box Notification requests created",
												new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusCCO_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.ControlsAtEuOffice;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_xT(declaration, entryHeader1, "3 In-box Notification requests created",
													new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication },
													new ZString[] { InboxNotificationResponseTypes.AESClearance, InboxNotificationResponseTypes.AESNonConformity, InboxNotificationResponseTypes.AESCceControl });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusCCO_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.ControlsAtEuOffice;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.ControlsAtEuOffice;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_EHub(declaration, entryHeader1, entryHeader2, entryHeader3, "6 In-box Notification requests created",
														new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusCCO_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.ControlsAtEuOffice;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.ControlsAtEuOffice;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_xT(declaration, entryHeader1, entryHeader2, entryHeader3, "6 In-box Notification requests created",
															new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication },
															new ZString[] { InboxNotificationResponseTypes.AESClearance, InboxNotificationResponseTypes.AESNonConformity, InboxNotificationResponseTypes.AESCceControl });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusCLR_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_EHub(declaration, entryHeader1, "2 In-box Notification requests created",
												new ZString[] { DeclarationMessageTypeList.Codes.ExportExitResultCommunication, DeclarationMessageTypeList.Codes.ExportInvalidationCommunication });
	}

	public void TestRequestInboxNotifications_OneEntry_Export_Ucc6_EntryStatusCLR_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader1.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_xT(declaration, entryHeader1, "2 In-box Notification requests created",
													new ZString[] { DeclarationMessageTypeList.Codes.ExportExitResultCommunication, DeclarationMessageTypeList.Codes.ExportInvalidationCommunication },
													new ZString[] { InboxNotificationResponseTypes.AESExitResult, InboxNotificationResponseTypes.AESInvalidation });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusCLR_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_EHub(declaration, entryHeader1, entryHeader2, entryHeader3, "4 In-box Notification requests created",
														new ZString[] { DeclarationMessageTypeList.Codes.ExportExitResultCommunication, DeclarationMessageTypeList.Codes.ExportInvalidationCommunication });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Export_Ucc6_EntryStatusCLR_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader2.ZG_UCC6Version = 1;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_UCC6Version = 1;

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_xT(declaration, entryHeader1, entryHeader2, entryHeader3, "4 In-box Notification requests created",
															new ZString[] { DeclarationMessageTypeList.Codes.ExportExitResultCommunication, DeclarationMessageTypeList.Codes.ExportInvalidationCommunication },
															new ZString[] { InboxNotificationResponseTypes.AESExitResult, InboxNotificationResponseTypes.AESInvalidation });
	}

	public void TestRequestInboxNotifications_OneEntry_Import_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, declarant: Factory.GetNewDeclarant(), messageType: MessageTypeList.Codes.Import);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_EHub(declaration, entryHeader1, "1 In-box Notification request created",
												new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForImport });
	}

	public void TestRequestInboxNotifications_OneEntry_Import_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, declarant: Factory.GetNewDeclarant(), messageType: MessageTypeList.Codes.Import);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_xT(declaration, entryHeader1, "1 In-box Notification request created",
													new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForImport },
													new ZString[] { InboxNotificationResponseTypes.Import });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Import_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, declarant: Factory.GetNewDeclarant(), createSecondEntry: true, messageType: MessageTypeList.Codes.Import, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_EHub(declaration, entryHeader1, entryHeader2, entryHeader3, "2 In-box Notification requests created",
														new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForImport });
	}

	public void TestRequestInboxNotifications_MultipleEntries_Import_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, declarant: Factory.GetNewDeclarant(), createSecondEntry: true, messageType: MessageTypeList.Codes.Import, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_xT(declaration, entryHeader1, entryHeader2, entryHeader3, "2 In-box Notification requests created",
															new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForImport },
															new ZString[] { InboxNotificationResponseTypes.Import });
	}

	public void TestRequestInboxNotifications_OneEntry_ImportH2_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, declarant: Factory.GetNewDeclarant(), messageType: MessageTypeList.Codes.Import, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_EHub(declaration, entryHeader1, "1 In-box Notification request created",
												new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 });
	}

	public void TestRequestInboxNotifications_OneEntry_ImportH2_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, declarant: Factory.GetNewDeclarant(), messageType: MessageTypeList.Codes.Import, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		Factory.Save();

		AssertRequestInboxNotifications_OneEntry_xT(declaration, entryHeader1, "1 In-box Notification request created",
													new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 },
													new ZString[] { InboxNotificationResponseTypes.DVD });
	}

	public void TestRequestInboxNotifications_MultipleEntries_ImportH2_EHub()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, declarant: Factory.GetNewDeclarant(), createSecondEntry: true, messageType: MessageTypeList.Codes.Import, createThirdEntry: true, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader2.EntryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_EHub(declaration, entryHeader1, entryHeader2, entryHeader3, "2 In-box Notification requests created",
														new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 });
	}

	public void TestRequestInboxNotifications_MultipleEntries_ImportH2_xT()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, declarant: Factory.GetNewDeclarant(), createSecondEntry: true, messageType: MessageTypeList.Codes.Import, createThirdEntry: true, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader2.EntryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		Factory.Save();

		AssertRequestInboxNotifications_MultipleEntries_xT(declaration, entryHeader1, entryHeader2, entryHeader3, "2 In-box Notification requests created",
															new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 },
															new ZString[] { InboxNotificationResponseTypes.DVD });
	}

	void AssertRequestInboxNotifications_OneEntry_EHub(JobDeclaration declaration, CusEntryHeader entryHeader, ZString notificationText, ZString[] types)
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var requestInboxNotifEntryMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

				declaration.HasChanges = true;
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();

				CombineAssertions(() =>
				{
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending inbox notification request", declaration.Factory);
					requestInboxNotifEntryMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending inbox notification request", declaration.Factory);

					AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

					AssertEquals("Inbox Notification Request was sent correctly", notificationText, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNewRequestEDIMessagesCreated(entryHeader, types);
				});
			}
		}
	}

	void AssertRequestInboxNotifications_OneEntry_xT(JobDeclaration declaration, CusEntryHeader entryHeader, ZString notificationText, ZString[] types, ZString[] urls)
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_FullName = DeclarantName;
		declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);

		declaration.Declarant.OA_OH = declarant.PK;

		var mrnCode = "20ES00999830001277";
		entryHeader.MovementReferenceNumberSetter(mrnCode, ZDateTime.Today);

		declaration.Factory.Save();

		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var requestInboxNotifEntryMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

				declaration.HasChanges = true;
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();

				CombineAssertions(() =>
				{
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending inbox notification request", declaration.Factory);
					requestInboxNotifEntryMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending inbox notification request", declaration.Factory);

					AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

					AssertEquals("Inbox Notification Request was sent correctly", notificationText, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNewCusPollingTransaction(entryHeader.PK, entryHeader.TablePrefix, types, mrnCode);

					AssertNewInboxListMessages(urls);
				});
			}
		}
	}

	void AssertRequestInboxNotifications_MultipleEntries_EHub(JobDeclaration declaration, CusEntryHeader entryHeader1, CusEntryHeader entryHeader2, CusEntryHeader entryHeader3, ZString notificationText, ZString[] types)
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var requestInboxNotifEntryMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

				declaration.HasChanges = true;
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();

				CombineAssertions(() =>
				{
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending inbox notification request", declaration.Factory);
					requestInboxNotifEntryMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending inbox notification request", declaration.Factory);

					AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

					AssertEquals("Inbox Notification Request was sent correctly", notificationText, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNewRequestEDIMessagesCreated(entryHeader1, types);
					AssertNewRequestEDIMessagesCreated(entryHeader2, types);
					AssertEquals("entryHeader3 has no messages", false, entryHeader3.Messages.Any());
				});
			}
		}
	}

	void AssertRequestInboxNotifications_MultipleEntries_xT(JobDeclaration declaration, CusEntryHeader entryHeader1, CusEntryHeader entryHeader2, CusEntryHeader entryHeader3, ZString notificationText, ZString[] types, ZString[] urls)
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_FullName = DeclarantName;
		declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);

		declaration.Declarant.OA_OH = declarant.PK;

		var mrnCode1 = "20ES00999830001277";
		var mrnCode2 = "20ES00999830001288";
		entryHeader1.MovementReferenceNumberSetter(mrnCode1, ZDateTime.Today);
		entryHeader2.MovementReferenceNumberSetter(mrnCode2, ZDateTime.Today);

		declaration.Factory.Save();

		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var requestInboxNotifEntryMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

				declaration.HasChanges = true;
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();

				CombineAssertions(() =>
				{
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending inbox notification request", declaration.Factory);
					requestInboxNotifEntryMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending inbox notification request", declaration.Factory);

					AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

					AssertEquals("Inbox Notification Request was sent correctly", notificationText, UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNewCusPollingTransaction(entryHeader1.PK, entryHeader1.TablePrefix, types, mrnCode1);
					AssertNewCusPollingTransaction(entryHeader2.PK, entryHeader1.TablePrefix, types, mrnCode2);
					AssertEquals("entryHeader3 has no messages", false, entryHeader3.Messages.Any());

					AssertNewInboxListMessages(urls);
				});
			}
		}
	}

	void AssertNewCusPollingTransaction(ZGuid parentID, ZString parentTablePrefix, ZString[] messageTypes, ZString mrn)
	{
		var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
		query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, mrn);
		var newTransactions = Factory.Load<CusPollingTransaction>(query);

		AssertEquals("There should only be 1 ESC transaction for each messageType", messageTypes.Length, newTransactions.Length);

		foreach (var transaction in newTransactions)
		{
			AssertEquals(transaction.CPT_Type + " transaction.CPT_ApplicationCode", Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage, transaction.CPT_ApplicationCode);
			AssertEquals(transaction.CPT_Type + " transaction.CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, transaction.CPT_Status);
			AssertEquals(transaction.CPT_Type + " transaction.CPT_Type is in list", true, messageTypes.Contains(transaction.CPT_Type));
			AssertEquals(transaction.CPT_Type + " transaction.CPT_TransactionID", mrn, transaction.CPT_TransactionID);
			AssertEquals(transaction.CPT_Type + " transaction.CPT_ParentID", parentID, transaction.CPT_ParentID);
			AssertEquals(transaction.CPT_Type + " transaction.CPT_ParentTableCode", parentTablePrefix, transaction.CPT_ParentTableCode);
		}
		AssertContainsExactElementsInAnyOrder("All transaction.CPT_Type are correct", messageTypes, newTransactions.Select(x => x.CPT_Type));
	}

	void AssertNewInboxListMessages(ZString[] urls)
	{
		var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
		messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
		var newRequestMessages = NewFactory().Load<EDIMessage>(messagesQuery);

		AssertEquals("Messages count is correct", urls.Length, newRequestMessages.Length);

		var newRequestMessagesText = new List<ZString>();
		foreach (EDIMessage message in newRequestMessages)
		{
			AssertRequestEDIMessageCreated(message, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertEquals(message.EM_MessageType + " message.EM_MessageType", DeclarationMessageTypeList.Codes.InboxPendingList, message.EM_MessageType);

			var url = urls.FirstOrDefault(x => message.EM_MessageText.Contains(x));
			AssertMultilineASCIIEquals(message.EM_MessageType + " message.EM_MessageText", GetExpectedNewInboxMessageBodyText(url), message.EM_MessageText);
		}
	}

	ZString GetExpectedNewInboxMessageBodyText(ZString url) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<ListaDecV4Ent tipoRespuesta=""{0}"" xmlns=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <declarante>
    <NifDeclarante>{1}</NifDeclarante>
    <NombreDeclarante>{2}</NombreDeclarante>
  </declarante>
</ListaDecV4Ent>
  </soapenv:Body>
</soapenv:Envelope>", url, DeclarantId, DeclarantName);

	const string DeclarantId = "NIF22222222";
	const string DeclarantName = "Declarant Full Name";

	#endregion

	#region GenerateExitControl

	#region EDI
	public void TestGenerateExitControl_EDI_NotAcceptedHeader()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, entryInstructionSubStyle1: EntrySubStyleList.Codes.X);
		declaration.JE_GS_NKCusAgent = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = "CLR";

		var entryHeader2 = declaration.CustomsEntryHeaders[0];
		entryHeader2.CH_EntryStatus = ZString.Empty;

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Nothing will be done when the selected entries are not accepted", "0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestGenerateExitControl_EDI_AcceptedHeader_NewExitHeader()
	{
		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = "XZX";
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = true;

		var expectedDeclarantOrgHeader = Factory.New<OrgHeader>();
		expectedDeclarantOrgHeader.OH_Code = "Declarant";
		var expectedDeclarantAddress = expectedDeclarantOrgHeader.MainAddress;
		expectedDeclarantAddress.OA_Address1 = "Declarant Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OA_DeclarantAddress = expectedDeclarantAddress.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("1 new exit detail created", "1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeaders = GetExitHeadersForDeclarationEDI(declaration);
					AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);
					var exitHeader = exitHeaders[0];
					AssertEquals("There is 1 new exitDetail created", 1, exitHeader.CusExitDetails.Count);
					AssertExitDetail("New exit detail", exitHeader.CusExitDetails[0], ExpectedMRN1ForNewExitDetailOrReport, ExpectedCustomsOfficeForNewExitDetailOrReport, ZDateTime.Today, ZString.Empty, ZString.Empty, expectedDeclarantAddress.PK, expectedSupplierAddress.PK, declaration.PK, ExpectedReferenceForNewExitHeader, ZDateTime.Today, ZString.Empty);
				});
			}
		}
	}

	public void TestGenerateExitControl_EDI_AcceptedHeader_ExistingExitHeaderWithoutDetail()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";
		var expectedNotificationDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
		var expectedNotificationPlace = "Place";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedDeclarantOrgHeader = Factory.New<OrgHeader>();
		expectedDeclarantOrgHeader.OH_Code = "Declarant";
		var expectedDeclarantAddress = expectedDeclarantOrgHeader.MainAddress;
		expectedDeclarantAddress.OA_Address1 = "Declarant Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OA_DeclarantAddress = expectedDeclarantAddress.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;

			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			exitHeader.CEH_ReferenceNumber = expectedHeaderReference;
			exitHeader.CEH_ArrivalNotificationDate = expectedNotificationDate;
			exitHeader.CEH_ArrivalNotificationPlace = expectedNotificationPlace;

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("1 new exit detail created", "1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeadersAfterGeneration = GetExitHeadersForDeclarationEDI(declaration);
					AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
					var exitHeaderAfterGeneration = exitHeadersAfterGeneration[0];
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
					AssertEquals("There is 1 new exitDetail created", 1, exitHeaderAfterGeneration.CusExitDetails.Count);
					AssertExitDetail("New exit detail", exitHeaderAfterGeneration.CusExitDetails[0], ExpectedMRN1ForNewExitDetailOrReport, ExpectedCustomsOfficeForNewExitDetailOrReport, expectedNotificationDate, expectedNotificationPlace, expectedBrokerCode, expectedDeclarantAddress.PK, expectedSupplierAddress.PK, declaration.PK, expectedHeaderReference, expectedNotificationDate, expectedNotificationPlace);
				});
			}
		}
	}

	public void TestGenerateExitControl_EDI_AcceptedHeader_ExistingExitHeaderWithExistingDetailToOverwrite()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";
		var expectedNotificationDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
		var expectedNotificationPlace = "Place";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedDeclarantOrgHeader = Factory.New<OrgHeader>();
		expectedDeclarantOrgHeader.OH_Code = "Declarant";
		var expectedDeclarantAddress = expectedDeclarantOrgHeader.MainAddress;
		expectedDeclarantAddress.OA_Address1 = "Declarant Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OA_DeclarantAddress = expectedDeclarantAddress.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;

			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			exitHeader.CEH_ReferenceNumber = expectedHeaderReference;
			exitHeader.CEH_ArrivalNotificationDate = expectedNotificationDate;
			exitHeader.CEH_ArrivalNotificationPlace = expectedNotificationPlace;

			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = ExpectedMRN1ForNewExitDetailOrReport;
			exitDetail.CED_CustomsOffice = "AAA";
			exitDetail.CED_ArrivalNotificationPlace = "Detail place";

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum1 already exists in Exit Control list. Do you want to overwrite it?"));
					AssertEquals("Nothing was done when selected No in the pop up", "0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum1 already exists in Exit Control list. Do you want to overwrite it?"));
					AssertEquals("1 detail update when selecting Yes in the pop up", "1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeadersAfterGeneration = GetExitHeadersForDeclarationEDI(declaration);
					AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
					var exitHeaderAfterGeneration = exitHeadersAfterGeneration[0];
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
					AssertEquals("There is 1 exitDetail updated", 1, exitHeaderAfterGeneration.CusExitDetails.Count);
					AssertExitDetail("Updated exit detail", exitHeaderAfterGeneration.CusExitDetails[0], ExpectedMRN1ForNewExitDetailOrReport, ExpectedCustomsOfficeForNewExitDetailOrReport, expectedNotificationDate, expectedNotificationPlace, expectedBrokerCode, expectedDeclarantAddress.PK, expectedSupplierAddress.PK, declaration.PK, expectedHeaderReference, expectedNotificationDate, expectedNotificationPlace);
				});
			}
		}
	}

	public void TestGenerateExitControl_EDI_AcceptedHeader_ExistingExitHeaderWithExistingSentDetail()
	{
		var expectedHeaderReference = "Reference";
		var expectedUnchangedNotificationPlace = "Detail Place";
		var expectedUnchangedNotificationDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
		var expectedUnchangedCustomsOffice = "AAA";
		var expectedHeaderNotificationPlace = "Header place";
		var expectedHeaderNotificationDate = new ZDateTime(2021, 05, 08, 15, 50, 00);

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;

		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		exitHeader.CEH_Parent = declaration;
		exitHeader.CEH_ReferenceNumber = expectedHeaderReference;
		exitHeader.CEH_ArrivalNotificationDate = expectedHeaderNotificationDate;
		exitHeader.CEH_ArrivalNotificationPlace = expectedHeaderNotificationPlace;

		var exitDetail = exitHeader.CusExitDetails.AddNew();
		exitDetail.CED_MovementReferenceNumber = ExpectedMRN1ForNewExitDetailOrReport;
		exitDetail.CED_Status = "AWR";
		exitDetail.CED_CustomsOffice = expectedUnchangedCustomsOffice;
		exitDetail.CED_ArrivalNotificationPlace = expectedUnchangedNotificationPlace;
		exitDetail.CED_ArrivalNotificationDate = expectedUnchangedNotificationDate;

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Can't update detail when sent or accepted and No details updated", "No movements have been generated for MRN refNum1 because it is sent or accepted\n\n0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeadersAfterGeneration = GetExitHeadersForDeclarationEDI(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
				var exitHeaderAfterGeneration = exitHeadersAfterGeneration[0];
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
				AssertEquals("There is 1 exitDetail as is was", 1, exitHeaderAfterGeneration.CusExitDetails.Count);
				AssertExitDetail("Not updated exit detail", exitHeaderAfterGeneration.CusExitDetails[0], ExpectedMRN1ForNewExitDetailOrReport, expectedUnchangedCustomsOffice, expectedUnchangedNotificationDate, expectedUnchangedNotificationPlace, ZString.Empty, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference, expectedHeaderNotificationDate, expectedHeaderNotificationPlace);
			});
		}
	}

	public void TestGenerateExitControl_EDI_MultipleHeaders_NewExitHeader()
	{
		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, declarationReference: ExpectedReferenceForNewExitHeader);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;

		Factory.Save();

		var expectedDefaultDeclarantPK = declaration.JE_OA_DeclarantAddress;

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("2 new exit details created", "2 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeaders = GetExitHeadersForDeclarationEDI(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);
				var exitHeader = exitHeaders[0];
				AssertEquals("There are 2 new exitDetails created", 2, exitHeader.CusExitDetails.Count);
				AssertContainsExactElementsInAnyOrder("The new exitDetaisl have the correct mrn codes", new ZString[] { ExpectedMRN1ForNewExitDetailOrReport, ExpectedMRN2ForNewExitDetailOrReport }, exitHeader.CusExitDetails.Cast<CusExitDetail>().Select(x => x.CED_MovementReferenceNumber).ToArray());
				AssertExitDetail("First new exit detail", exitHeader.CusExitDetails[0], ZString.Empty, ExpectedCustomsOfficeForNewExitDetailOrReport, ZDateTime.Today, ZString.Empty, ZString.Empty, expectedDefaultDeclarantPK, ZGuid.Empty, declaration.PK, ExpectedReferenceForNewExitHeader, ZDateTime.Today, ZString.Empty);
				AssertExitDetail("Second new exit detail", exitHeader.CusExitDetails[1], ZString.Empty, ExpectedCustomsOfficeForNewExitDetailOrReport, ZDateTime.Today, ZString.Empty, ZString.Empty, expectedDefaultDeclarantPK, ZGuid.Empty, declaration.PK, ExpectedReferenceForNewExitHeader, ZDateTime.Today, ZString.Empty);
			});
		}
	}

	public void TestGenerateExitControl_EDI_MultipleHeaders_ExistingExitHeaderWithoutDetails()
	{
		var expectedHeaderReference = "Reference";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, declarationReference: ExpectedReferenceForNewExitHeader);
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;

		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		exitHeader.CEH_Parent = declaration;
		exitHeader.CEH_ReferenceNumber = expectedHeaderReference;

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("2 new exit details created", "2 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeadersAfterGeneration = GetExitHeadersForDeclarationEDI(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
				var exitHeaderAfterGeneration = exitHeadersAfterGeneration[0];
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
				AssertEquals("There are 2 new exitDetails created", 2, exitHeaderAfterGeneration.CusExitDetails.Count);
				AssertContainsExactElementsInAnyOrder("The new exitDetaisl have the correct mrn codes", new ZString[] { ExpectedMRN1ForNewExitDetailOrReport, ExpectedMRN2ForNewExitDetailOrReport }, exitHeaderAfterGeneration.CusExitDetails.Cast<CusExitDetail>().Select(x => x.CED_MovementReferenceNumber).ToArray());
				AssertExitDetail("First new exit detail", exitHeaderAfterGeneration.CusExitDetails[0], ZString.Empty, ZString.Empty, ZDateTime.Today, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference, ZDateTime.Today, ZString.Empty);
				AssertExitDetail("Second new exit detail", exitHeaderAfterGeneration.CusExitDetails[1], ZString.Empty, ZString.Empty, ZDateTime.Today, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference, ZDateTime.Today, ZString.Empty);
			});
		}
	}

	public void TestGenerateExitControl_EDI_MultipleHeaders_ExistingExitHeaderWithExistingDetails()
	{
		var expectedHeaderReference = "Reference";
		var expectedDetailNotificationPlace = "Detail Place";
		var expectedDetailNotificationDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
		var expectedDetailCustomsOffice = "AAA";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader);
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;

		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		exitHeader.CEH_Parent = declaration;
		exitHeader.CEH_ReferenceNumber = expectedHeaderReference;

		var exitDetail1 = exitHeader.CusExitDetails.AddNew();
		exitDetail1.CED_MovementReferenceNumber = ExpectedMRN1ForNewExitDetailOrReport;
		exitDetail1.CED_Status = "AWR";
		exitDetail1.CED_CustomsOffice = expectedDetailCustomsOffice;
		exitDetail1.CED_ArrivalNotificationPlace = expectedDetailNotificationPlace;
		exitDetail1.CED_ArrivalNotificationDate = expectedDetailNotificationDate;

		var exitDetail2 = exitHeader.CusExitDetails.AddNew();
		exitDetail2.CED_MovementReferenceNumber = ExpectedMRN2ForNewExitDetailOrReport;
		exitDetail2.CED_CustomsOffice = expectedDetailCustomsOffice;
		exitDetail2.CED_ArrivalNotificationPlace = expectedDetailNotificationPlace;
		exitDetail2.CED_ArrivalNotificationDate = expectedDetailNotificationDate;

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum2 already exists in Exit Control list. Do you want to overwrite it?"));
				AssertEquals("Can't update detail when sent or accepted and 1 detail update and 1 created when selecting Yes in the pop up", "No movements have been generated for MRN refNum1 because it is sent or accepted\n\n2 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeadersAfterGeneration = GetExitHeadersForDeclarationEDI(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
				var exitHeaderAfterGeneration = exitHeadersAfterGeneration[0];
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
				AssertEquals("There are 3 exitDetails, 1 created, 1 updated and 1 as it was", 3, exitHeaderAfterGeneration.CusExitDetails.Count);
				AssertContainsExactElementsInAnyOrder("The new exitDetaisl have the correct mrn codes", new ZString[] { ExpectedMRN1ForNewExitDetailOrReport, ExpectedMRN2ForNewExitDetailOrReport, ExpectedMRN3ForNewExitDetailOrReport }, exitHeaderAfterGeneration.CusExitDetails.Cast<CusExitDetail>().Select(x => x.CED_MovementReferenceNumber).ToArray());
				AssertExitDetail("Not updated exit detail", exitHeaderAfterGeneration.CusExitDetails.Cast<CusExitDetail>().First(x => x.CED_MovementReferenceNumber == ExpectedMRN1ForNewExitDetailOrReport), ExpectedMRN1ForNewExitDetailOrReport, expectedDetailCustomsOffice, expectedDetailNotificationDate, expectedDetailNotificationPlace, ZString.Empty, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference, ZDateTime.Today, ZString.Empty);
				AssertExitDetail("Updated exit detail", exitHeaderAfterGeneration.CusExitDetails.Cast<CusExitDetail>().First(x => x.CED_MovementReferenceNumber == ExpectedMRN2ForNewExitDetailOrReport), ExpectedMRN2ForNewExitDetailOrReport, ZString.Empty, ZDateTime.Today, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference, ZDateTime.Today, ZString.Empty);
				AssertExitDetail("New exit detail", exitHeaderAfterGeneration.CusExitDetails.Cast<CusExitDetail>().First(x => x.CED_MovementReferenceNumber == ExpectedMRN3ForNewExitDetailOrReport), ExpectedMRN3ForNewExitDetailOrReport, ZString.Empty, ZDateTime.Today, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference, ZDateTime.Today, ZString.Empty);

				exitHeaderAfterGeneration.CusExitDetails[0].CED_Status = "AWR";
				exitHeaderAfterGeneration.CusExitDetails[1].CED_Status = "CLR";
				exitHeaderAfterGeneration.CusExitDetails[2].CED_Status = "CDA";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Can't update detail when sent or accepted and No details updated", "No movements have been generated for MRN refNum1, refNum2, refNum3 because they are sent or accepted\n\n0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	void AssertExitDetail(string message, CusExitDetail exitDetail, ZString expectedMRN, string expectedExitCustomsOffice, ZDateTime expectedNotificationDate, string expectedNotificationPlace, string expectedBroker, ZGuid expectedAgentID, ZGuid expectedCarrierID, ZGuid expectedParentID, string expectedHeaderReference, ZDateTime expectedHeaderNotificationDate, string expectedHeaderNotificationPlace)
	{
		if (!expectedMRN.IsEmpty)
		{
			AssertEquals(message + " MRN", expectedMRN, exitDetail.CED_MovementReferenceNumber);
		}
		AssertEquals(message + " CustomsOffice", expectedExitCustomsOffice, exitDetail.CED_CustomsOffice);
		AssertEquals(message + " ArrivalNotificationDate", expectedNotificationDate, exitDetail.CED_ArrivalNotificationDate);
		AssertEquals(message + " ArrivalNotificationPlace", expectedNotificationPlace, exitDetail.CED_ArrivalNotificationPlace);

		AssertEquals(message + " Header.ArrivalNotificationDate", expectedHeaderNotificationDate, exitDetail.Header.CEH_ArrivalNotificationDate);
		AssertEquals(message + " Header.ArrivalNotificationPlace", expectedHeaderNotificationPlace, exitDetail.Header.CEH_ArrivalNotificationPlace);
		AssertEquals(message + " Header.NKCustomsAgent", expectedBroker, exitDetail.Header.CEH_GS_NKCustomsAgent);
		AssertEquals(message + " Header.OA_Agent", expectedAgentID, exitDetail.Header.CEH_OA_Agent);
		AssertEquals(message + " Header.OA_Carrier", expectedCarrierID, exitDetail.Header.CEH_OA_Carrier);
		AssertEquals(message + " Header.Parent", expectedParentID, exitDetail.Header.CEH_ParentID);
		AssertEquals(message + " Header.ParentTableCode", "JE", exitDetail.Header.CEH_ParentTableCode);
		AssertEquals(message + " Header.ReferenceNumber", expectedHeaderReference, exitDetail.Header.CEH_ReferenceNumber);
	}

	#endregion

	#region AES

	public void TestGenerateExitControl_AES_NotAcceptedHeader()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, entryInstructionSubStyle1: EntrySubStyleList.Codes.X);
		declaration.JE_GS_NKCusAgent = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = "CLR";
		entryHeader1.ZG_UCC6Version = 1;

		var entryHeader2 = declaration.CustomsEntryHeaders[0];
		entryHeader2.CH_EntryStatus = ZString.Empty;
		entryHeader2.ZG_UCC6Version = 1;

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Nothing will be done when the selected entries are not accepted", "0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestGenerateExitControl_AES_AcceptedHeader_NewExitHeader()
	{
		var expectedBrokerCode = "XZX";
		var expectedCertificate = "TESTCERT1";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;
		var wrapper = GlbStaffWrapper.Get(staffCurrentUser);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = expectedCertificate;
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
		expectedCarrierOrgHeader.OH_Code = "Carrier";
		var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
		expectedCarrierAddress.OA_Address1 = "Carrier Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("1 new exit detail created", "1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeaders = GetExitHeadersForDeclarationAES(declaration);
					AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);
					AssertExitHeader("New ExitHeader", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
										expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedCertificate: expectedCertificate);
				});
			}
		}
	}

	public void TestGenerateExitControl_AES_AcceptedHeader_ExistingExitHeaderWithoutConsignment()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
		expectedCarrierOrgHeader.OH_Code = "Carrier";
		var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
		expectedCarrierAddress.OA_Address1 = "Carrier Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration, expectedHeaderReference));

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("1 new exit detail created", "1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeadersAfterGeneration = GetExitHeadersForDeclarationAES(declaration);
					AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeadersAfterGeneration[0]);

					AssertExitHeader("Existing exit header with new data", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
									expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}
	}

	public void TestGenerateExitControl_AES_AcceptedHeader_ExistingExitHeaderAndConsignmentWithoutReportToOverwrite()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
		expectedCarrierOrgHeader.OH_Code = "Carrier";
		var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
		expectedCarrierAddress.OA_Address1 = "Carrier Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration, expectedHeaderReference));
			var exitConsignment = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN1ForNewExitDetailOrReport, "local ref"));

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum1 already exists in Exit Control list. Do you want to overwrite it?"));
					AssertEquals("Nothing was done when selected No in the pop up", "0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum1 already exists in Exit Control list. Do you want to overwrite it?"));
					AssertEquals("1 detail update when selecting Yes in the pop up", "1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeadersAfterGeneration = GetExitHeadersForDeclarationAES(declaration);
					AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeadersAfterGeneration[0]);

					AssertExitHeader("Existing exit header with new data", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
									expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}
	}

	public void TestGenerateExitControl_AES_AcceptedHeader_ExistingExitHeaderWithConsignmentAndReportToOverwrite()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
		expectedCarrierOrgHeader.OH_Code = "Carrier";
		var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
		expectedCarrierAddress.OA_Address1 = "Carrier Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration, expectedHeaderReference));
			var exitConsignment = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN1ForNewExitDetailOrReport, "local ref"));
			var exitReport = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader.PK, exitConsignment.PK, "AAA", "AIR"));

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum1 already exists in Exit Control list. Do you want to overwrite it?"));
					AssertEquals("Nothing was done when selected No in the pop up", "0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum1 already exists in Exit Control list. Do you want to overwrite it?"));
					AssertEquals("1 detail update when selecting Yes in the pop up", "1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeadersAfterGeneration = GetExitHeadersForDeclarationAES(declaration);
					AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeadersAfterGeneration[0]);

					AssertExitHeader("Existing exit header with new data", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { ExpectedLRN1ForNewExitConsignment },
									expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference);
				});
			}
		}
	}

	public void TestGenerateExitControl_AES_AcceptedHeader_ExistingExitHeaderWithExistingSentReport()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";
		var expectedUnchangedConsignmentLRN = "local ref";
		var expectedUnchangedCustomsOffice = "AAA";
		var expectedUnchangedInlandMOT = "AIR";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
		expectedCarrierOrgHeader.OH_Code = "Carrier";
		var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
		expectedCarrierAddress.OA_Address1 = "Carrier Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration, expectedHeaderReference));
			var exitConsignment = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN1ForNewExitDetailOrReport, expectedUnchangedConsignmentLRN));
			var exitReport = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader.PK, exitConsignment.PK, expectedUnchangedCustomsOffice, expectedUnchangedInlandMOT, messageStatus: "SNT"));

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("Can't update detail when sent or accepted and No details updated", "No movements have been generated for MRN refNum1 because it is sent or accepted\n\n0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeadersAfterGeneration = GetExitHeadersForDeclarationAES(declaration);
					AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
					AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeadersAfterGeneration[0]);

					AssertExitHeader("Existing exit header without new data", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { expectedUnchangedConsignmentLRN },
									expectedSupplierOrgHeader.PK, expectedCarrierAddress.PK, declaration.PK, expectedBroker: expectedBrokerCode, expectedHeaderReference: expectedHeaderReference,
									expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
				});
			}
		}
	}

	public void TestGenerateExitControl_AES_AcceptedHeader_ExistingExitHeaderWithConsignmentNotAssociated()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";
		var expectedUnchangedConsignmentLRN = "local ref";
		var expectedUnchangedCustomsOffice = "AAA";
		var expectedUnchangedInlandMOT = "AIR";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
		expectedCarrierOrgHeader.OH_Code = "Carrier";
		var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
		expectedCarrierAddress.OA_Address1 = "Carrier Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(null, expectedHeaderReference));
			var exitConsignment = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN1ForNewExitDetailOrReport, expectedUnchangedConsignmentLRN));
			var exitReport = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader.PK, exitConsignment.PK, expectedUnchangedCustomsOffice, expectedUnchangedInlandMOT));

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("Can't update exit control when it is not associated to the declaration", "A movement for MRN refNum1 already exists in Exit Control Reference\n\n0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeaders = GetExitHeadersForDeclarationAES(declaration);
					AssertEquals("No exitHeaders associated to the declaration", 0, exitHeaders.Length);

					AssertExitHeader("Existing exit header without new data", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { expectedUnchangedConsignmentLRN },
									ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, expectedParentTableCode: ZString.Empty,
									expectedHeaderReference: expectedHeaderReference, expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
				});
			}
		}
	}

	public void TestGenerateExitControl_AES_AcceptedHeader_ExistingExitHeaderWithConsignmentNotAssociatedExpectedDeclaration()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";
		var expectedUnchangedConsignmentLRN = "local ref";
		var expectedUnchangedCustomsOffice = "AAA";
		var expectedUnchangedInlandMOT = "AIR";
		var expectedDeclarationReference = "DecRef";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedCarrierOrgHeader = Factory.New<OrgHeader>();
		expectedCarrierOrgHeader.OH_Code = "Carrier";
		var expectedCarrierAddress = expectedCarrierOrgHeader.MainAddress;
		expectedCarrierAddress.OA_Address1 = "Carrier Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address}";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetailOrReport);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OH_ShippingLine = expectedCarrierOrgHeader.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;
			declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_DeclarationReference = expectedDeclarationReference;

			var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration2, expectedHeaderReference));
			var exitConsignment = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN1ForNewExitDetailOrReport, expectedUnchangedConsignmentLRN));
			var exitReport = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader.PK, exitConsignment.PK, expectedUnchangedCustomsOffice, expectedUnchangedInlandMOT));

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

				CombineAssertions(() =>
				{
					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					generateExitControlMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.EntriesBoundGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
					generateExitControlMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
					AssertEquals("Can't update exit control when it is not associated to the declaration", "A movement for MRN refNum1 already exists in Job Number DecRef\n\n0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

					var exitHeaders = GetExitHeadersForDeclarationAES(declaration);
					AssertEquals("No exitHeaders associated to the declaration", 0, exitHeaders.Length);

					AssertExitHeader("Existing exit header without new data", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { expectedUnchangedConsignmentLRN },
									ZGuid.Empty, ZGuid.Empty, declaration2.PK, expectedHeaderReference: expectedHeaderReference,
									expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
				});
			}
		}
	}

	public void TestGenerateExitControl_AES_MultipleHeaders_NewExitHeader()
	{
		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;
		declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("2 new exit details created", "2 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeaders = GetExitHeadersForDeclarationAES(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);

				AssertExitHeader("New ExitHeader", 2, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport, ExpectedMRN2ForNewExitDetailOrReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK);
			});
		}
	}

	public void TestGenerateExitControl_AES_MultipleHeaders_ExistingExitHeaderWithoutConsignments()
	{
		var expectedHeaderReference = "Reference";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;
		declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

		var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration, expectedHeaderReference));

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("2 new exit details created", "2 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeadersAfterGeneration = GetExitHeadersForDeclarationAES(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeadersAfterGeneration[0]);

				AssertExitHeader("Existing exit header with new data", 2, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport, ExpectedMRN2ForNewExitDetailOrReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);
			});
		}
	}

	public void TestGenerateExitControl_AES_MultipleHeaders_ExistingExitHeaderAndConsignmentsWithoutReports()
	{
		var expectedHeaderReference = "Reference";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;
		declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

		var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration, expectedHeaderReference));
		var exitConsignment1 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN1ForNewExitDetailOrReport, "local ref 1"));
		var exitConsignment2 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN2ForNewExitDetailOrReport, "local ref 2"));

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum1 already exists in Exit Control list. Do you want to overwrite it?"));
				AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum2 already exists in Exit Control list. Do you want to overwrite it?"));
				AssertEquals("2 details updated when selecting Yes in the pop up and 1 detail created", "3 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeadersAfterGeneration = GetExitHeadersForDeclarationAES(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeadersAfterGeneration[0]);

				AssertExitHeader("Existing exit header with new data", 3, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport, ExpectedMRN2ForNewExitDetailOrReport, ExpectedMRN3ForNewExitDetailOrReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, ExpectedLRN2ForNewExitConsignment, ExpectedLRN3ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference);

				UpdateCusExitReportStatus(exitHeader.PK, "COX");
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Can't update detail when sent or accepted and No details updated", "No movements have been generated for MRN refNum1, refNum2, refNum3 because they are sent or accepted\n\n0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestGenerateExitControl_AES_MultipleHeaders_ExistingExitHeaderWithConsignmentsAndReports()
	{
		var expectedHeaderReference = "Reference";
		var expectedUnchangedConsignmentLRN2 = "local ref2";
		var expectedUnchangedCustomsOffice = "AAA";
		var expectedUnchangedInlandMOT = "AIR";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;
		declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

		var exitHeader = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration, expectedHeaderReference));
		var exitConsignment1 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN1ForNewExitDetailOrReport, "local ref 1"));
		var exitReport1 = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader.PK, exitConsignment1.PK, "AAA", "AIR"));

		var exitConsignment2 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader.PK, ExpectedMRN2ForNewExitDetailOrReport, expectedUnchangedConsignmentLRN2));
		var exitReport2 = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader.PK, exitConsignment2.PK, expectedUnchangedCustomsOffice, expectedUnchangedInlandMOT, messageStatus: "SNT"));

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum1 already exists in Exit Control list. Do you want to overwrite it?"));
				AssertEquals("Can't update detail when sent or accepted and 1 detail update and 1 created when selecting Yes in the pop up", "No movements have been generated for MRN refNum2 because it is sent or accepted\n\n2 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeadersAfterGeneration = GetExitHeadersForDeclarationAES(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeadersAfterGeneration[0]);

				AssertExitHeader("Existing exit header with new data", 3, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport, ExpectedMRN2ForNewExitDetailOrReport, ExpectedMRN3ForNewExitDetailOrReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment, expectedUnchangedConsignmentLRN2, ExpectedLRN3ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference,
									expectedCustomsOffices: new ZString[] { ExpectedCustomsOfficeForNewExitDetailOrReport, expectedUnchangedCustomsOffice, ExpectedCustomsOfficeForNewExitDetailOrReport },
									expectedInlandMOTs: new ZString[] { ExpectedInlandMOTForNewExitReport, expectedUnchangedInlandMOT, ExpectedInlandMOTForNewExitReport });

				UpdateCusExitReportStatus(exitHeader.PK, "REF");
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Can't update detail when sent or accepted and No details updated", "No movements have been generated for MRN refNum1, refNum2, refNum3 because they are sent or accepted\n\n0 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestGenerateExitControl_AES_MultipleHeaders_ExistingExitHeaderWithConsignmentWrongAssociation_Update()
	{
		var expectedHeaderReference1 = "Reference1";
		var expectedHeaderReference2 = "Reference2";
		var expectedHeaderReference3 = "Reference3";
		var expectedUnchangedConsignmentLRN1 = "local ref 1";
		var expectedUnchangedConsignmentLRN2 = "local ref 2";
		var expectedUnchangedCustomsOffice = "AAA";
		var expectedUnchangedInlandMOT = "AIR";
		var expectedDeclarationReference = "DecRef";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;
		declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

		var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
		declaration2.JE_DeclarationReference = expectedDeclarationReference;

		var exitHeader1 = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration2, expectedHeaderReference1));
		var exitConsignment1 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader1.PK, ExpectedMRN1ForNewExitDetailOrReport, expectedUnchangedConsignmentLRN1));
		var exitReport1 = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader1.PK, exitConsignment1.PK, expectedUnchangedCustomsOffice, expectedUnchangedInlandMOT));

		var exitHeader2 = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(null, expectedHeaderReference2, 2));
		var exitConsignment2 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader2.PK, ExpectedMRN3ForNewExitDetailOrReport, expectedUnchangedConsignmentLRN2));
		var exitReport2 = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader2.PK, exitConsignment2.PK, expectedUnchangedCustomsOffice, expectedUnchangedInlandMOT));

		var exitHeader3 = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration, expectedHeaderReference3, 3));
		var exitConsignment3 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader3.PK, ExpectedMRN2ForNewExitDetailOrReport, "local ref 3", 3));

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Should have message asking if the detail should be overwritten", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("A movement for MRN refNum2 already exists in Exit Control list. Do you want to overwrite it?"));
				AssertEquals("Can't update exit control when it is not associated to the declaration and 1 updated when selecting Yes in the pop up",
					"A movement for MRN refNum1 already exists in Job Number DecRef\n\nA movement for MRN refNum3 already exists in Exit Control Reference2\n\n1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeadersAfterGeneration = GetExitHeadersForDeclarationAES(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeadersAfterGeneration.Length);
				AssertEquals("There is no new exitHeader created since there was already one associated to the declaration (exitHeader3)", exitHeader3, exitHeadersAfterGeneration[0]);

				AssertExitHeader("New ExitHeader", 1, new ZString[] { ExpectedMRN2ForNewExitDetailOrReport },
									new ZString[] { ExpectedLRN2ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference: expectedHeaderReference3);

				AssertExitHeader("Existing exit header 1 without new data", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { expectedUnchangedConsignmentLRN1 },
								ZGuid.Empty, ZGuid.Empty, declaration2.PK, expectedHeaderReference: expectedHeaderReference1,
								expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });

				AssertExitHeader("Existing exit header 2 without new data", 1, new ZString[] { ExpectedMRN3ForNewExitDetailOrReport }, new ZString[] { expectedUnchangedConsignmentLRN2 },
								ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, expectedParentTableCode: ZString.Empty, expectedHeaderReference: expectedHeaderReference2,
								expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
			});
		}
	}

	public void TestGenerateExitControl_AES_MultipleHeaders_ExistingExitHeaderWithConsignmentWrongAssociation()
	{
		var expectedHeaderReference1 = "Reference1";
		var expectedHeaderReference2 = "Reference2";
		var expectedUnchangedConsignmentLRN1 = "local ref 1";
		var expectedUnchangedConsignmentLRN2 = "local ref 2";
		var expectedUnchangedCustomsOffice = "AAA";
		var expectedUnchangedInlandMOT = "AIR";
		var expectedDeclarationReference = "DecRef";

		var universalClusterKeyNumberFountain = new NumberFountains().ClusterKeyNumber.Wrap();
		var exitHeader1ClusterKey = (int)universalClusterKeyNumberFountain.GetNext(Factory);
		var exitHeader2ClusterKey = (int)universalClusterKeyNumberFountain.GetNext(Factory);

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true, true, declarationReference: ExpectedReferenceForNewExitHeader, shouldEntriesBeUcc6: true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;
		declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

		var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
		declaration2.JE_DeclarationReference = expectedDeclarationReference;

		var exitHeader1 = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(declaration2, expectedHeaderReference1, exitHeader1ClusterKey));
		var exitConsignment1 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader1.PK, ExpectedMRN1ForNewExitDetailOrReport, expectedUnchangedConsignmentLRN1));
		var exitReport1 = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader1.PK, exitConsignment1.PK, expectedUnchangedCustomsOffice, expectedUnchangedInlandMOT));

		var exitHeader2 = Factory.Load(CusExitHeaderSchema.Constants.Prefix, CreateCusExitHeader(null, expectedHeaderReference2, exitHeader2ClusterKey));
		var exitConsignment2 = Factory.Load(CusExitConsignmentSchema.Constants.Prefix, CreateCusExitConsignment(exitHeader2.PK, ExpectedMRN3ForNewExitDetailOrReport, expectedUnchangedConsignmentLRN2));
		var exitReport2 = Factory.Load(CusExitReportSchema.Constants.Prefix, CreateCusExitReport(exitHeader2.PK, exitConsignment2.PK, expectedUnchangedCustomsOffice, expectedUnchangedInlandMOT));

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("Can't update exit control when it is not associated to the declaration and 1 created when selecting Yes in the pop up",
					"A movement for MRN refNum1 already exists in Job Number DecRef\n\nA movement for MRN refNum3 already exists in Exit Control Reference2\n\n1 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeaders = GetExitHeadersForDeclarationAES(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);

				AssertExitHeader("New ExitHeader", 1, new ZString[] { ExpectedMRN2ForNewExitDetailOrReport },
									new ZString[] { ExpectedLRN2ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK);

				AssertExitHeader("Existing exit header 1 without new data", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport }, new ZString[] { expectedUnchangedConsignmentLRN1 },
								ZGuid.Empty, ZGuid.Empty, declaration2.PK, expectedHeaderReference: expectedHeaderReference1,
								expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });

				AssertExitHeader("Existing exit header 2 without new data", 1, new ZString[] { ExpectedMRN3ForNewExitDetailOrReport }, new ZString[] { expectedUnchangedConsignmentLRN2 },
								ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, expectedParentTableCode: ZString.Empty, expectedHeaderReference: expectedHeaderReference2,
								expectedCustomsOffices: new ZString[] { expectedUnchangedCustomsOffice }, expectedInlandMOTs: new ZString[] { expectedUnchangedInlandMOT });
			});
		}
	}

	void AssertExitHeader(string message, int expectedConsignmentReportCount, ZString[] expectedMRNCodes, ZString[] expectedLRNCodes, ZGuid expectedExporterID, ZGuid expectedCarrierID, ZGuid expectedParentID, string expectedBroker = "", string expectedParentTableCode = "JE",
							string expectedCertificate = "", string expectedHeaderReference = ExpectedReferenceForNewExitHeader, ZString[] expectedCustomsOffices = null, ZString[] expectedInlandMOTs = null)
	{
		var query = new ZQuery(CusExitHeaderSchema.CXH_JobReference, expectedHeaderReference);
		if (!expectedParentID.IsEmpty)
		{
			query.AddToFilter(CusExitHeaderSchema.CXH_ParentID, expectedParentID);
		}
		query.AddToFilter(CusExitHeaderSchema.CXH_ParentTableCode, expectedParentTableCode);
		query.AddToFilter(CusExitHeaderSchema.CXH_GS_NKCustomsAgent, expectedBroker);
		query.AddToFilter(CusExitHeaderSchema.CXH_CustomsProfile, expectedCertificate);
		if (!expectedExporterID.IsEmpty)
		{
			query.AddToFilter(CusExitHeaderSchema.CXH_OH_Exporter, expectedExporterID);
		}
		if (!expectedCarrierID.IsEmpty)
		{
			query.AddToFilter(CusExitHeaderSchema.CXH_OA_Carrier, expectedCarrierID);
		}
		var exitHeader = (BusinessObject)Factory.LoadTop1<ESExitControl.ICusExitHeader>(query);

		AssertNotNull(message + " Header with all the needed data exists", exitHeader);

		AssertExitConsignments(expectedConsignmentReportCount, exitHeader.PK, expectedMRNCodes, expectedLRNCodes);

		AssertExitReports(expectedConsignmentReportCount, exitHeader.PK, expectedMRNCodes, expectedCustomsOffices, expectedInlandMOTs);
	}

	void AssertExitConsignments(int expectedConsignmentCount, ZGuid exitHeaderPK, ZString[] expectedMRNCodes, ZString[] expectedLRNCodes)
	{
		var query = new ZQuery(CusExitConsignmentSchema.CXC_CXH_Header, exitHeaderPK);
		query.AddToFilter(CusExitConsignmentSchema.CXC_MovementReference, (from ZString mrn in expectedMRNCodes select mrn));
		query.AddToFilter(CusExitConsignmentSchema.CXC_LocalReference, (from ZString lrn in expectedLRNCodes select lrn));
		var exitConsignments = (IEnumerable<BusinessObject>)Factory.Load<ESExitControl.ICusExitConsignment>(query);

		AssertEquals("ExitConsignments count is correct", expectedConsignmentCount, exitConsignments.Count());
	}

	void AssertExitReports(int expectedReportCount, ZGuid exitHeaderPK, ZString[] expectedMRNCodes, ZString[] expectedCustomsOffices = null, ZString[] expectedInlandMOTs = null)
	{
		var query = new ZDBOnlyQuery(ObjectFactory.GetType<EUExitControl.ICusExitReport>());
		query.AddToFilter(CusExitReportSchema.CER_CXH_Header, exitHeaderPK);

		var exitConsignmentQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<EUExitControl.ICusExitConsignment>(), CusExitConsignmentSchema.PK);
		exitConsignmentQuery.AddToFilter(CusExitConsignmentSchema.CXC_MovementReference, (from ZString mrn in expectedMRNCodes select mrn));
		query.AddSubQuery(CusExitReportSchema.CER_CXC_Consignment, exitConsignmentQuery, JoinCondition.And);

		if (expectedCustomsOffices == null)
		{
			query.AddToFilter(CusExitReportSchema.CER_OfficeOfExit, ExpectedCustomsOfficeForNewExitDetailOrReport);
		}
		else
		{
			query.AddToFilter(CusExitReportSchema.CER_OfficeOfExit, (from ZString office in expectedCustomsOffices select office));
		}

		if (expectedInlandMOTs == null)
		{
			query.AddToFilter(CusExitReportSchema.CER_TransportMode, ExpectedInlandMOTForNewExitReport);
		}
		else
		{
			query.AddToFilter(CusExitReportSchema.CER_TransportMode, (from ZString mot in expectedInlandMOTs select mot));
		}

		var exitReports = (IEnumerable<BusinessObject>)Factory.Load<ESExitControl.ICusExitReport>(query);

		AssertEquals("ExitReports count is correct", expectedReportCount, exitReports.Count());
	}

	#endregion

	public void TestGenerateExitControl_AESAndEDI_MultipleHeaders_NewExitHeaders()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetailOrReport;
		declaration.JE_TransportModeInland = ExpectedInlandMOTForNewExitReport;

		declaration.JE_DeclarationReference = ExpectedReferenceForNewExitHeader;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = "CLR";
		entryHeader1.MovementReferenceNumber = ExpectedMRN1ForNewExitDetailOrReport;
		entryHeader1.CH_BGMReference = ExpectedLRN1ForNewExitConsignment;
		entryHeader1.ZG_UCC6Version = UCC6VersionCodes.UCC6;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = "CDA";
		entryHeader2.MovementReferenceNumber = ExpectedMRN2ForNewExitDetailOrReport;
		entryHeader2.CH_BGMReference = ExpectedLRN2ForNewExitConsignment;
		entryHeader2.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;

		Factory.Save();

		var expectedDefaultDeclarantPK = declaration.JE_OA_DeclarantAddress;

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var generateExitControlMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Generate Exit Control");

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				generateExitControlMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.EntriesBoundGrid.SelectAllElements();
				TestHelper.CheckFactoryHasNoPendingChanges("Before generating exit control", declaration.Factory);
				generateExitControlMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After generating exit control", declaration.Factory);
				AssertEquals("2 new exit details created", "2 EAL(s) created successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitHeaders = GetExitHeadersForDeclarationAES(declaration);
				AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);

				AssertExitHeader("New ExitHeader for AES entry", 1, new ZString[] { ExpectedMRN1ForNewExitDetailOrReport },
									new ZString[] { ExpectedLRN1ForNewExitConsignment }, ZGuid.Empty, ZGuid.Empty, declaration.PK);

				var exitHeaderEDIs = GetExitHeadersForDeclarationEDI(declaration);
				AssertEquals("Only one EDI exitHeader associated to the declaration", 1, exitHeaderEDIs.Length);
				var exitHeaderEDI = exitHeaderEDIs[0];
				AssertEquals("There is 1 new exitDetail created for EDI entry", 1, exitHeaderEDI.CusExitDetails.Count);
				AssertExitDetail("New exit detail for EDI entry", exitHeaderEDI.CusExitDetails[0], ExpectedMRN2ForNewExitDetailOrReport, ExpectedCustomsOfficeForNewExitDetailOrReport, ZDateTime.Today, ZString.Empty, ZString.Empty, expectedDefaultDeclarantPK, ZGuid.Empty, declaration.PK, ExpectedReferenceForNewExitHeader, ZDateTime.Today, ZString.Empty);
			});
		}
	}

	#endregion

	#region UpdateCSV

	#region Export

	public void TestUpdateCSVClearanceMessagesExport()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);
		declaration.JE_GS_NKCusAgent = ZString.Empty;
		declaration.JE_CustomsProfile = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var entriesGrid = userControl.EntriesBoundGrid;
			var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectAllElements();
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader3.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN and entry instruction (missing entry instruction)", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestUpdateCSVClearanceActionsExport()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff);
			declaration.ZG_CTStatusID = "T2LF";
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000002";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L Nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.ZG_CSVT2L);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					userControl.secondaryCSVCode = "-++**__aa/#€&@";
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.ZG_CSVT2L);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					userControl.secondaryCSVCode = "T2LCLEARANCE1234";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has not been changed when the pop up was accepted  but date is empty", ZString.Empty, entryHeader.ZG_CSVT2L);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					userControl.secondaryCSVCode = "T2LCLEARANCE1234";
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has been changed when the pop up was accepted", "T2LCLEARANCE1234", entryHeader.ZG_CSVT2L);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000002|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000002|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertEquals("CSV T2L New event in logs", "|NEW=T2LCLEARANCE1234|RES=Manually Added CSV T2L Code to Entry ES000002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=T2LCLEARANCE1234")).SL_Reference);
					AssertContains("Updating CSV Clearance and CSV T2L triggers document capture request", "2 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					userControl.secondaryCSVCode = "BBBBBBBBBBBBBBBB";
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_E_AEAT_CLR.pdf", "CLR");
					var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_E_AEAT_t2lf.pdf", "CAU");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has been changed when the pop up was accepted a second time", "BBBBBBBBBBBBBBBB", entryHeader.ZG_CSVT2L);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000002|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000002|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertEquals("CSV T2L New event in logs", "|NEW=BBBBBBBBBBBBBBBB|OLD=T2LCLEARANCE1234|RES=Manually Added CSV T2L Code to Entry ES000002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=BBBBBBBBBBBBBBBB")).SL_Reference);
					AssertContains("Updating CSV Clearance and CSV T2L triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 2, eDocs.Count());
					var eDocNamesChanged = new List<ZString>() { entryHeader.MovementReferenceNumber + "_E_AEAT_CLR_OLD_CLEARANCE1234567.pdf", entryHeader.MovementReferenceNumber + "_E_AEAT_t2lf_OLD_CLEARANCE1234567.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	public void TestUpdateCSVClearanceActionsExportAES()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff);
			declaration.ZG_CTStatusID = "T2LF";
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ZG_UCC6Version = 1;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);
			entryHeader.IndirectExport = true;

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000002";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L Nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.ZG_CSVT2L);
					AssertEquals("CSV Exit Certificate Nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.ZG_CSVExitCertificate);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					userControl.secondaryCSVCode = "-++**__aa/#€&@";
					userControl.thirdCSVCode = "-++**__aa/#€&@";
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.ZG_CSVT2L);
					AssertEquals("CSV Exit Certificate has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.ZG_CSVExitCertificate);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					userControl.secondaryCSVCode = "T2LCLEARANCE1234";
					userControl.thirdCSVCode = "A1234567890";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has not been changed when the pop up was accepted  but date is empty", ZString.Empty, entryHeader.ZG_CSVT2L);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					userControl.secondaryCSVCode = "T2LCLEARANCE1234";
					userControl.thirdCSVCode = "A1234567890";
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has been changed when the pop up was accepted", "T2LCLEARANCE1234", entryHeader.ZG_CSVT2L);
					AssertEquals("CSV Exit Certificate has been changed when the pop up was accepted", "A1234567890", entryHeader.ZG_CSVExitCertificate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000002|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000002|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertEquals("CSV T2L New event in logs", "|NEW=T2LCLEARANCE1234|RES=Manually Added CSV T2L Code to Entry ES000002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=T2LCLEARANCE1234")).SL_Reference);
					AssertEquals("CSV Exit Certificate New event in logs", "|NEW=A1234567890|RES=Manually Added CSV Exit Certificate Code to Entry ES000002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=A1234567890")).SL_Reference);
					AssertContains("Updating CSV Clearance, CSV T2L and CSV Exit Certificate triggers document capture request", "4 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					userControl.secondaryCSVCode = "BBBBBBBBBBBBBBBB";
					userControl.thirdCSVCode = "CCCCCCCCCCCCCCCC";
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_E_AEAT_CLR.pdf", "CLR");
					var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_E_AEAT_t2lf.pdf", "CAU");
					var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_E_AEAT_CLR_EXT.pdf", "CLR");
					var eDoc4 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_E_AEAT_ead.pdf", "EAD");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has been changed when the pop up was accepted a second time", "BBBBBBBBBBBBBBBB", entryHeader.ZG_CSVT2L);
					AssertEquals("CSV Exit Certificate has been changed when the pop up was accepted a second time", "CCCCCCCCCCCCCCCC", entryHeader.ZG_CSVExitCertificate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000002|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000002|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertEquals("CSV T2L New event in logs", "|NEW=BBBBBBBBBBBBBBBB|OLD=T2LCLEARANCE1234|RES=Manually Added CSV T2L Code to Entry ES000002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=BBBBBBBBBBBBBBBB")).SL_Reference);
					AssertEquals("CSV T2L New event in logs", "|NEW=CCCCCCCCCCCCCCCC|OLD=A1234567890|RES=Manually Added CSV Exit Certificate Code to Entry ES000002|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CCCCCCCCCCCCCCCC")).SL_Reference);
					AssertContains("Updating CSV Clearance, CSV T2L and CSV Exit Certificate triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 4, eDocs.Count());
					var eDocNamesChanged = new List<ZString>()
											{ entryHeader.MovementReferenceNumber + "_E_AEAT_CLR_OLD_CLEARANCE1234567.pdf",
												entryHeader.MovementReferenceNumber + "_E_AEAT_t2lf_OLD_CLEARANCE1234567.pdf",
												entryHeader.MovementReferenceNumber + "_E_AEAT_CLR_EXT_OLD_CLEARANCE1234567.pdf",
												entryHeader.MovementReferenceNumber + "_E_AEAT_ead_OLD_CLEARANCE1234567.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	#endregion

	#region Import

	public void TestUpdateCSVClearanceMessagesImport()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, messageType: MessageTypeList.Codes.Import, createThirdEntry: true);
		declaration.JE_GS_NKCusAgent = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var entriesGrid = userControl.EntriesBoundGrid;
			var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectAllElements();
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader3.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN and entry instruction (missing entry instruction)", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Declarations needs broker to choose certificate from", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestUpdateCSVClearanceActionsImport()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000004";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV ImportCertificate Nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.ZG_CSVT2L);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					userControl.secondaryCSVCode = "-++**__aa/#€&@";
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV ImportCertificate has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.ZG_CSVImportCertificate);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					userControl.secondaryCSVCode = "CSVIMPORTCER1234";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has not been changed when the pop up was accepted  but date is empty", ZString.Empty, entryHeader.ZG_CSVT2L);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					userControl.secondaryCSVCode = "CSVIMPORTCER1234";
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV ImportCertificate has been changed when the pop up was accepted", "CSVIMPORTCER1234", entryHeader.ZG_CSVImportCertificate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000004|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000004|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertEquals("CSV ImportCertificate New event in logs", "|NEW=CSVIMPORTCER1234|RES=Manually Added CSV Import Certificate Code to Entry ES000004|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CSVIMPORTCER1234")).SL_Reference);
					AssertContains("Updating CSV Clearance and CSV Import Certificate triggers document capture request", "4 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					userControl.secondaryCSVCode = "BBBBBBBBBBBBBBBB";
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_I_AEAT_CLR.pdf", "CLR");
					var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_I_AEAT_CER.pdf", "CAU");
					var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_I_AEAT_M031.pdf", "CAU");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV ImportCertificate has been changed when the pop up was accepted a second time", "BBBBBBBBBBBBBBBB", entryHeader.ZG_CSVImportCertificate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000004|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000004|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertEquals("CSV ImportCertificate New event in logs", "|NEW=BBBBBBBBBBBBBBBB|OLD=CSVIMPORTCER1234|RES=Manually Added CSV Import Certificate Code to Entry ES000004|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=BBBBBBBBBBBBBBBB")).SL_Reference);
					AssertContains("Updating CSV Clearance and CSV Import Certificate triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 3, eDocs.Count());
					var eDocNamesChanged = new List<ZString>() { entryHeader.MovementReferenceNumber + "_I_AEAT_CLR_OLD_CLEARANCE1234567.pdf", entryHeader.MovementReferenceNumber + "_I_AEAT_CER_OLD_CLEARANCE1234567.pdf", entryHeader.MovementReferenceNumber + "_I_AEAT_M031.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	public void TestUpdateCSVClearanceActionsImportH1()
	{
		using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.H1))
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000004";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV ImportCertificate Nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.ZG_CSVT2L);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					userControl.secondaryCSVCode = "-++**__aa/#€&@";
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV ImportCertificate has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.ZG_CSVImportCertificate);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					userControl.secondaryCSVCode = "CSVIMPORTCER1234";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV T2L has not been changed when the pop up was accepted  but date is empty", ZString.Empty, entryHeader.ZG_CSVT2L);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					userControl.secondaryCSVCode = "CSVIMPORTCER1234";
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV ImportCertificate has been changed when the pop up was accepted", "CSVIMPORTCER1234", entryHeader.ZG_CSVImportCertificate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000004|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000004|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertEquals("CSV ImportCertificate New event in logs", "|NEW=CSVIMPORTCER1234|RES=Manually Added CSV Import Certificate Code to Entry ES000004|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CSVIMPORTCER1234")).SL_Reference);
					AssertContains("Updating CSV Clearance and CSV Import Certificate triggers document capture request", "4 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					userControl.secondaryCSVCode = "BBBBBBBBBBBBBBBB";
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_I_AEAT_CLR.pdf", "CLR");
					var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_I_AEAT_CER.pdf", "CAU");
					var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_I_AEAT_M031.pdf", "CAU");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV ImportCertificate has been changed when the pop up was accepted a second time", "BBBBBBBBBBBBBBBB", entryHeader.ZG_CSVImportCertificate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000004|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000004|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertEquals("CSV ImportCertificate New event in logs", "|NEW=BBBBBBBBBBBBBBBB|OLD=CSVIMPORTCER1234|RES=Manually Added CSV Import Certificate Code to Entry ES000004|TYP=CSV", entryHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=BBBBBBBBBBBBBBBB")).SL_Reference);
					AssertContains("Updating CSV Clearance and CSV Import Certificate triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 3, eDocs.Count());
					var eDocNamesChanged = new List<ZString>() { entryHeader.MovementReferenceNumber + "_I_AEAT_CLR_OLD_CLEARANCE1234567.pdf", entryHeader.MovementReferenceNumber + "_I_AEAT_CER_OLD_CLEARANCE1234567.pdf", entryHeader.MovementReferenceNumber + "_I_AEAT_M031.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	#endregion

	#region Export T2L

	public void TestUpdateCSVClearanceMessagesExportT2L()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L, createThirdEntry: true);
		declaration.JE_GS_NKCusAgent = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var entriesGrid = userControl.EntriesBoundGrid;
			var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectAllElements();
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader3.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN and entry instruction (missing entry instruction)", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Declarations needs broker to choose certificate from", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestUpdateCSVClearanceActionsExportT2L()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000006";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is T2L, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request", "1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_E_AEAT_T2L_CLR.pdf", "CLR");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is T2L, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 1, eDocs.Count());
					var eDocNamesChanged = new List<ZString>() { entryHeader.MovementReferenceNumber + "_E_AEAT_T2L_CLR_OLD_CLEARANCE1234567.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	#endregion

	#region Import T2L POUS2

	public void TestUpdateCSVClearanceMessagesImportT2LPOUS2()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, messageType: MessageTypeList.Codes.Import, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L, createThirdEntry: true);
		declaration.JE_GS_NKCusAgent = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		CreateT2CEntryNumber(entryHeader1, "20ES00999830001277");

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var entriesGrid = userControl.EntriesBoundGrid;
			var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				AssertEquals("POUSVersion is 2 for entryHeader1", 2, entryHeader1.ZG_POUSVersion);

				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectAllElements();
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader3.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN and entry instruction (missing entry instruction)", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Declarations needs broker to choose certificate from", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestUpdateCSVClearanceActionsImportT2LPOUS2()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			CreateT2CEntryNumber(entryHeader, "20ES00999830001277");

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000006";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					AssertEquals("POUSVersion is 2 for entryHeader", 2, entryHeader.ZG_POUSVersion);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is T2L, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request", "1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.T2CMovementReferenceNumber + "_I_AEAT_T2LR_CLR.pdf", "CLR");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is T2L, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 1, eDocs.Count());
					var eDocNamesChanged = new List<ZString>() { entryHeader.T2CMovementReferenceNumber + "_I_AEAT_T2LR_CLR_OLD_CLEARANCE1234567.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	#endregion

	#region T2C

	public void TestUpdateCSVClearanceMessagesT2C()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, messageType: MessageTypeList.Codes.Import, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2C, createThirdEntry: true);
		declaration.JE_GS_NKCusAgent = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		CreateT2CEntryNumber(entryHeader1, "20ES00999830001277");

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var entriesGrid = userControl.EntriesBoundGrid;
			var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectAllElements();
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader3.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN and entry instruction (missing entry instruction)", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Declarations needs broker to choose certificate from", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestUpdateCSVClearanceActionsT2C()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2C);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			CreateT2CEntryNumber(entryHeader, "20ES00999830001277");

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000006";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is T2C, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request", "1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.T2CMovementReferenceNumber + "_I_AEAT_T2L_CLR.pdf", "CLR");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is T2C, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 1, eDocs.Count());
					var eDocNamesChanged = new List<ZString>() { entryHeader.T2CMovementReferenceNumber + "_I_AEAT_T2L_CLR_OLD_CLEARANCE1234567.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	#endregion

	#region EXS

	public void TestUpdateCSVClearanceMessagesEXS()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, createThirdEntry: true);
		declaration.JE_GS_NKCusAgent = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var entriesGrid = userControl.EntriesBoundGrid;
			var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectAllElements();
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader3.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN and entry instruction (missing entry instruction)", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Declarations needs broker to choose certificate from", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestUpdateCSVClearanceActionsEXS()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000006";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is EXS, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request", "1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_E_AEAT_EXS_CLR.pdf", "CLR");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is EXS, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 1, eDocs.Count());
					var eDocNamesChanged = new List<ZString>() { entryHeader.MovementReferenceNumber + "_E_AEAT_EXS_CLR_OLD_CLEARANCE1234567.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	#endregion

	#region DVD

	public void TestUpdateCSVClearanceMessagesDVD()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, createSecondEntry: true, createThirdEntry: true);
		declaration.JE_GS_NKCusAgent = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader1.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var entriesGrid = userControl.EntriesBoundGrid;
			var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectAllElements();
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader3.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Can only update csv on entries with MRN and entry instruction (missing entry instruction)", "Please select only an entry with MRN", UnitTestUserNotification.Instance.LastMessage.Text);

				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Declarations needs broker to choose certificate from", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				Factory.Save();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				updateCSVClearanceMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestUpdateCSVClearanceActionsDVD()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.MovementReferenceNumberSetter("20ES00999830001277", ZDateTime.Today);

			Factory.Save();

			entryHeader.CH_BGMReference = "ES000006";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControlForTesting())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var entriesGrid = userControl.EntriesBoundGrid;
				var updateCSVClearanceMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					entriesGrid.Select();
					entriesGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance nothing has been done when the pop up was cancelled", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (pop up cancelled)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was cancelled", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "+--**__aa/#€&@";
					userControl.clearanceDate = ZDateTime.Invalid;
					var expectedIncorrectFormatMessage = "Nothing was updated because there were errors";
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (code is invalid)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is not valid", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when values are invalid", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = ZDateTime.Empty;
					entriesGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV Clearance has not been changed when the pop up was accepted but date is empty", ZString.Empty, entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is not set entry status is left as is (date is empty)", EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date nothing has been done when the pop up was accepted but date is empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
					AssertEquals("Should have message telling nothing was done when date is empty", expectedIncorrectFormatMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "CLEARANCE1234567";
					userControl.clearanceDate = new ZDateTime(2023, 06, 14, 11, 12, 00);
					entriesGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted", "CLEARANCE1234567", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted", new ZDateTime(2023, 06, 14, 11, 12, 00), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=CLEARANCE1234567")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2023-06-14T11:12:00")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request", "1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					userControl.csvCode = "AAAAAAAAAAAAAAAA";
					userControl.clearanceDate = new ZDateTime(2024, 04, 28, 10, 09, 08);
					entriesGrid.SelectAllElements();

					var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
					var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], entryHeader.MovementReferenceNumber + "_D_AEAT_CLR.pdf", "CLR");
					docManagerInfo.Save();

					entryHeader.CH_EntryStatus = "AAA";
					AssertEquals("Prereq: Entry status is AAA", "AAA", entryHeader.CH_EntryStatus);
					Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before document capture", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After document capture", declaration.Factory);
					AssertEquals("CSV Clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", entryHeader.CSVClearance);
					AssertEquals("When CSV Clearance is set to a new value and entry instruction is A, entry status is set to CLR", EntryStatusCodes.Cleared, entryHeader.CH_EntryStatus);
					AssertEquals("Entry Release Date has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), entryHeader.CH_EntryReleaseDate);
					AssertEquals("CSV Clearance New event in logs", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=AAAAAAAAAAAAAAAA")).SL_Reference);
					AssertEquals("Entry Release Date New event in logs", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:00|RES=Manually Added Clearance Date to Entry ES000006|TYP=CSV", entryHeader.Logs.EarliestLogByEventTime(Events.ChangeOfIdentifier, x => x.SL_Reference.StartsWith("|NEW=2024-04-28T10:09:08")).SL_Reference);
					AssertContains("Updating CSV Clearance triggers document capture request but none are requested since they already are", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					var eDocs = docManagerInfo.GetRelatedEDocs();
					AssertEquals("Number of eDocs is correct", 1, eDocs.Count());
					var eDocNamesChanged = new List<ZString>() { entryHeader.MovementReferenceNumber + "_D_AEAT_CLR_OLD_CLEARANCE1234567.pdf" };
					AssertContainsExactElementsInAnyOrder("eDocs contains all documents with names changed", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
				});
			}
		}
	}

	#endregion

	#endregion

	#region ViewOnCustomsWebsite

	public void TestViewOnCustomsWebsite_OneEntry_Import_PDI()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.advu.jdit.web.cons.DetalleVUAInt?operacion=3000&CABECERA_MRN=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Import_NotPDI_Mainland()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImporInternet?CABECERA_MRN=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		declaration.ZG_DestinationState = "28";

		var entryHeader = declaration.CustomsEntryHeaders[0];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Import_NotPDI_CanaryIsland()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adip.inter.cnt.CImpInternetVexcan?CABECERA_MRN=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		declaration.ZG_DestinationState = BuilderHelperTest.CanaryIslandCode;

		var entryHeader = declaration.CustomsEntryHeaders[0];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Import_T2L_NoPOUS()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/L/inwinvoc/es.aeat.dit.adu.adtl.gestion.cnt.DetExpedicionT2L?retorno=es.aeat.dit.adu.adtl.gestion.qry.QDocsT2LInt&operacion=1001&clave=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Import_T2L_POUS1()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/L/inwinvoc/es.aeat.dit.adu.adtl.gestion.cnt.DetExpedicionT2L?retorno=es.aeat.dit.adu.adtl.gestion.qry.QDocsT2LInt&operacion=1001&clave=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;
		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Import_T2L_POUS2()
	{
		var expectedT2CMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/T2LDetalle?mrn=" + expectedT2CMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS2;
		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no t2c mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = "mrn";
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when mrn but no t2c mrn", WebUrlLauncher.LastUrlLaunched);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = expectedT2CMRN;
				newEntryNumber.CE_IssueDate = ZDateTime.Today;
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Import_T2C()
	{
		var expectedT2CMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/JECDetalle?mrn=" + expectedT2CMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2C);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no t2c mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = "mrn";
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when mrn but no t2c mrn", WebUrlLauncher.LastUrlLaunched);

				var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = expectedT2CMRN;
				newEntryNumber.CE_IssueDate = ZDateTime.Today;
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_ImportH1_Status()
	{
		var expectedMRN = "24ES00999930000JPB";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleSH1?anyo=24&pais=ES&recinto=009999&numero=30000JPB";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_ImportH1_StatusCanaryIsland()
	{
		var expectedMRN = "24ES00999930000JPB";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleXSH1?anyo=24&pais=ES&recinto=009999&numero=30000JPB";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);
		declaration.ZG_DestinationState = BuilderHelperTest.CanaryIslandCode;

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_ImportH1_DJPStatus()
	{
		var expectedMRNDJP = "JPBTEST00123456789";
		var expectedMRN = "22ES00999912345678";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADIP-JDIT/DetalleUltH1?mrn=" + expectedMRNDJP;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				entryHeader.ZG_DJPMRN = expectedMRNDJP;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Export()
	{
		var actualMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalle?CLAVE=" + actualMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = actualMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Export_T2L()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTL-JDIT/T2LDetalle?mrn=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Export_EXS()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adrx.inter.CtrInternet?operacion=1032&clave=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Import_H2()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADDV-H2UC/CntAEATInternet?operacion=2250&CABECERA_MRN=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2);

		var entryHeader = declaration.CustomsEntryHeaders[0];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	public void TestViewOnCustomsWebsite_OneEntry_Import_H2_CanaryIsland()
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADDV-H2UC/CntATCInternet?operacion=2250&CABECERA_MRN=" + expectedMRN;
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2);
		declaration.JE_CustomsOffice = "ES003500";

		var entryHeader = declaration.CustomsEntryHeaders[0];

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var viewOnCustomsWebsiteMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("View on Customs Website");
			WebUrlLauncher.ClearLastUrlLaunched();

			CombineAssertions(() =>
			{
				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();

				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertNullOrEmpty("No url was launched when no mrn", WebUrlLauncher.LastUrlLaunched);

				entryHeader.MovementReferenceNumber = expectedMRN;
				userControl.EntriesBoundGrid.SelectAllElements();
				viewOnCustomsWebsiteMenuItem.PerformClick();
				AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			});
		}
	}

	#endregion

	#region Re-enable Annexes Messages

	public void TestReEnableAnnexesMessagesMenuItemValidations()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.ZG_RequestDispatch = YesNoList.Codes.Yes;
		entryHeader1.CH_BGMReference = "ES000001";
		entryHeader1.CH_Status = Customs.Common.EU.MessageStatusList.Codes.AwaitingResponse;
		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.ZG_RequestDispatch = YesNoList.Codes.No;
		entryHeader2.CH_BGMReference = "ES000002";
		var entryHeader3 = declaration.CustomsEntryHeaders[2];
		entryHeader3.ZG_RequestDispatch = ZString.Empty;
		entryHeader3.CH_BGMReference = "ES000003";

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesGrid = userControl.EntriesBoundGrid;
			var reenableAnnexesMessagesMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Re-enable Annexes Messages");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				reenableAnnexesMessagesMenuItem.PerformClick();
				AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				reenableAnnexesMessagesMenuItem.PerformClick();
				AssertEquals("Can only reenable Annexes Messages when ZG_RequestDispatch is Yes (ZG_RequestDispatch is No)", "You have not sent any Annex message with Request Dispatch flag in the selected entry (ES000002)", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader3.PK);
				reenableAnnexesMessagesMenuItem.PerformClick();
				AssertEquals("Can only reenable Annexes Messages when ZG_RequestDispatch is Yes (ZG_RequestDispatch is empty)", "You have not sent any Annex message with Request Dispatch flag in the selected entry (ES000003)", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectAllElements();
				reenableAnnexesMessagesMenuItem.PerformClick();
				AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				reenableAnnexesMessagesMenuItem.PerformClick();
				AssertEquals("Can only reenable Annexes Messages when ZG_RequestDispatch is Yes (ZG_RequestDispatch is Yes) and message status is not AWR", "Cannot trigger this action while entry ES000001 is awaiting response", UnitTestUserNotification.Instance.LastMessage.Text);

				entryHeader1.CH_Status = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				reenableAnnexesMessagesMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating when ZG_RequestDispatch is Yes", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));
			});
		}
	}

	public void TestReEnableAnnexesMessages()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true);
		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.ZG_RequestDispatch = YesNoList.Codes.Yes;
		entryHeader1.CH_BGMReference = "ES000001";
		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.ZG_RequestDispatch = YesNoList.Codes.Yes;
		entryHeader2.CH_BGMReference = "ES000002";

		using (var form = new ZForm(declaration))
		using (var userControl = new MessageUserControlForTesting())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var entriesGrid = userControl.EntriesBoundGrid;
			var reenableAnnexesMessagesMenuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Re-enable Annexes Messages");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				reenableAnnexesMessagesMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				AssertEquals("ZG_RequestDispatch was changed for entryHeader1", ZString.Empty, entryHeader1.ZG_RequestDispatch);
				AssertEquals("ZG_RequestDispatch was not changed for entryHeader2", YesNoList.Codes.Yes, entryHeader2.ZG_RequestDispatch);
				AssertEquals("Should have message informing of the action done", "Annexes Messages have been enabled by removing the Request Dispatch flag. You can now send a new Annex message for entry ES000001", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				entriesGrid.SelectSingleElementByPK(entryHeader2.PK);
				reenableAnnexesMessagesMenuItem.PerformClick();
				AssertEquals("Should not have message asking to save the declaration before updating because it was saved in the previous update", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				AssertEquals("ZG_RequestDispatch was not changed for entryHeader1", ZString.Empty, entryHeader1.ZG_RequestDispatch);
				AssertEquals("ZG_RequestDispatch was changed for entryHeader2", ZString.Empty, entryHeader2.ZG_RequestDispatch);
				AssertEquals("Should have message informing of the action done", "Annexes Messages have been enabled by removing the Request Dispatch flag. You can now send a new Annex message for entry ES000002", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	#endregion

	#region PUERequests

	public void TestPUERequest_SendAnnexDocuments()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ENVIODOCPUE";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var menuItem = pueRequestsMenuItem.MenuItems.FindByText("Send Annex Documents");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestPUERequest_SendMessage()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EnvioMensajePUE";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var menuItem = pueRequestsMenuItem.MenuItems.FindByText("Send Message");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	#region ROHSRAEE

	public void TestROHSRAEE_RequestCertificate()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/RohsSolicitudForm";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var rohsraeeMenuItem = pueRequestsMenuItem.MenuItems.FindByText("ROHS-RAEE");
			var menuItem = rohsraeeMenuItem.MenuItems.FindByText("Request Certificate");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestROHSRAEE_SendAdditionalData()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/RohsDatosAdiForm";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var rohsraeeMenuItem = pueRequestsMenuItem.MenuItems.FindByText("ROHS-RAEE");
			var menuItem = rohsraeeMenuItem.MenuItems.FindByText("Send Additional Data");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestROHSRAEE_QueryExistingCertificates()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvRohsSolQuery";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var rohsraeeMenuItem = pueRequestsMenuItem.MenuItems.FindByText("ROHS-RAEE");
			var menuItem = rohsraeeMenuItem.MenuItems.FindByText("Query Existing Certificates");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	#endregion

	#region COM

	public void TestCOM_RequestCertificate()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ComSolicitudForm";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var comMenuItem = pueRequestsMenuItem.MenuItems.FindByText("COM");
			var menuItem = comMenuItem.MenuItems.FindByText("Request Certificate");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestCOM_SendAdditionalData()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/ComDatosAdiForm";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var comMenuItem = pueRequestsMenuItem.MenuItems.FindByText("COM");
			var menuItem = comMenuItem.MenuItems.FindByText("Send Additional Data");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestCOM_QueryExistingCertificates()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvComSolQuery";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var comMenuItem = pueRequestsMenuItem.MenuItems.FindByText("COM");
			var menuItem = comMenuItem.MenuItems.FindByText("Query Existing Certificates");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	#endregion

	#region ECO

	public void TestECO_RequestCertificate()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EcoSolicitudForm";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var ecoMenuItem = pueRequestsMenuItem.MenuItems.FindByText("ECO");
			var menuItem = ecoMenuItem.MenuItems.FindByText("Request Certificate");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestECO_SendAdditionalData()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/EcoDatosAdiForm";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var ecoMenuItem = pueRequestsMenuItem.MenuItems.FindByText("ECO");
			var menuItem = ecoMenuItem.MenuItems.FindByText("Send Additional Data");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestECO_QueryExistingCertificates()
	{
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/AD44-JDIT/SvEcoSolQuery";
		var declaration = Factory.GetNewJobDeclaration(Staff, messageType: MessageTypeList.Codes.Import);

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			var pueRequestsMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("PUE Requests");
			var ecoMenuItem = pueRequestsMenuItem.MenuItems.FindByText("ECO");
			var menuItem = ecoMenuItem.MenuItems.FindByText("Query Existing Certificates");
			WebUrlLauncher.ClearLastUrlLaunched();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();

			menuItem.PerformClick();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
		}
	}

	#endregion

	#endregion

	public void TestRequestCertEffectiveDeparture_Validations()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true, createThirdEntry: true);
		RequestCertEffectiveDeparture_Validations(UCC6VersionCodes.NoUCC6, declaration);
		RequestCertEffectiveDeparture_Validations(UCC6VersionCodes.UCC6, declaration);
	}

	void RequestCertEffectiveDeparture_Validations(int ucc6, JobDeclaration declaration)
	{
		var messageEstension = " with parameter ucc6 = " + ucc6;
		declaration.JE_GS_NKCusAgent = ZString.Empty;
		declaration.JE_CustomsProfile = ZString.Empty;

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
		entryHeader1.ZG_UCC6Version = ucc6;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		var entryHeader3 = declaration.CustomsEntryHeaders[2];

		Factory.Save();

		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var entriesGrid = userControl.EntriesBoundGrid;
			var requestDepartureCertMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Request Certificate of Effective Departure");

			CombineAssertions(() =>
			{
				entriesGrid.Select();
				entriesGrid.Focus();

				requestDepartureCertMenuItem.PerformClick();
				AssertEquals("Needs to select a row" + messageEstension, "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.EntriesBoundGrid.SelectAllElements();
				requestDepartureCertMenuItem.PerformClick();
				AssertEquals("Can only request departure certificate for entries with EFD entry status" + messageEstension, "Certificate Request is only available for Effective Departures (Entry Status = EFD and Entry Instruction A, B, C, X, Y or Z)", UnitTestUserNotification.Instance.LastMessage.Text);

				entryHeader2.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
				entryHeader3.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
				entryHeader1.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryHeader2.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryHeader3.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.Factory.Save();

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.Refresh();
				userControl.EntriesBoundGrid.SelectAllElements();
				requestDepartureCertMenuItem.PerformClick();
				AssertEquals("Can only request departure certificate for entries with entry instruction (A, B, C, X, Y or Z), no entry has these conditions" + messageEstension, "Certificate Request is only available for Effective Departures (Entry Status = EFD and Entry Instruction A, B, C, X, Y or Z)", UnitTestUserNotification.Instance.LastMessage.Text);

				entryHeader1.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				requestDepartureCertMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared" + messageEstension, "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_GS_NKCusAgent = Staff.GS_Code;
				declaration.JE_CustomsProfile = ZString.Empty;
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				requestDepartureCertMenuItem.PerformClick();
				AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty" + messageEstension, "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = "INVALID";
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				requestDepartureCertMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid" + messageEstension, "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_CustomsProfile = BuilderHelperTest.CertificateName;
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				requestDepartureCertMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared" + messageEstension, "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.Reload();
				userControl.Refresh();
				entriesGrid.SelectSingleElementByPK(entryHeader1.PK);
				requestDepartureCertMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestRequestCertEffectiveDeparture_CreateOneMessage()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
		entryHeader1.ZG_UCC6Version = UCC6VersionCodes.UCC6;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var requestDepartureCertMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Request Certificate of Effective Departure");

			declaration.HasChanges = true;
			userControl.Refresh();
			userControl.EntriesBoundGrid.SelectAllElements();

			CombineAssertions(() =>
			{
				TestHelper.CheckFactoryHasNoPendingChanges("Before sending inbox notification request", declaration.Factory);
				requestDepartureCertMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After sending inbox notification request", declaration.Factory);

				AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				AssertEquals("Certificate Request was sent correctly", "1 Certificate Request created", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNewRequestEDIMessagesCreated(entryHeader1, new ZString[] { DeclarationMessageTypeList.Codes.RequestExportExitCertificate }, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			});
		}
	}

	public void TestRequestCertEffectiveDeparture_CreateMultipleMessages()
	{
		var declaration = Factory.GetNewJobDeclaration(Staff, createSecondEntry: true);

		var entryHeader1 = declaration.CustomsEntryHeaders[0];
		entryHeader1.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
		entryHeader1.ZG_UCC6Version = UCC6VersionCodes.UCC6;

		var entryHeader2 = declaration.CustomsEntryHeaders[1];
		entryHeader2.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
		entryHeader2.ZG_UCC6Version = UCC6VersionCodes.UCC6;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var form = new ZForm(declaration))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var requestDepartureCertMenuItem = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Request Certificate of Effective Departure");

			declaration.HasChanges = true;
			userControl.Refresh();
			userControl.EntriesBoundGrid.SelectAllElements();

			CombineAssertions(() =>
			{
				TestHelper.CheckFactoryHasNoPendingChanges("Before sending inbox notification request", declaration.Factory);
				requestDepartureCertMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After sending inbox notification request", declaration.Factory);

				AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

				AssertEquals("Certificate Requests were sent correctly", "2 Certificate Requests created", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNewRequestEDIMessagesCreated(entryHeader1, new ZString[] { DeclarationMessageTypeList.Codes.RequestExportExitCertificate }, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
				AssertNewRequestEDIMessagesCreated(entryHeader2, new ZString[] { DeclarationMessageTypeList.Codes.RequestExportExitCertificate }, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			});
		}
	}

	#region Create Supplementary Entry from Simplified Entry

	public void TestSetCreateSupplementaryFromSimplifiedMenuItemVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertCreateSupplementaryFromSimplifiedMenuItemVisibility(declaration, visible: false);

		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertCreateSupplementaryFromSimplifiedMenuItemVisibility(declaration, visible: false);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.ZG_UCC6Version = 1;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		AssertCreateSupplementaryFromSimplifiedMenuItemVisibility(declaration, visible: true);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertCreateSupplementaryFromSimplifiedMenuItemVisibility(declaration, visible: false);
	}

	void AssertCreateSupplementaryFromSimplifiedMenuItemVisibility(JobDeclaration declaration, bool visible)
	{
		CombineAssertions(() =>
		{
			using (var form = new ZForm(declaration))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var menu = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Create Supplementary Entry from Simplified Entry");
				AssertEquals($"menu {(visible ? "is" : " is not")} visible", expected: visible, menu.Visible);
			}
		});
	}

	public void TestNewRelatedDeclaration()
	{
		using (var userControl = GetControlToTest())
		{
			var menu = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Create Supplementary Entry from Simplified Entry");
			var subMenu = menu.MenuItems.FindByText("New Related Declaration");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			subMenu.PerformClick();
			var expectedDialogText = "Create related declaration will be done in another workflow";
			AssertContains("Dialog text", expectedDialogText, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestNewEntryAndInstruction()
	{
		using (var userControl = GetControlToTest())
		{
			var menu = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Create Supplementary Entry from Simplified Entry");
			var subMenu = menu.MenuItems.FindByText("New Entry and Instruction");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			subMenu.PerformClick();
			var expectedDialogText = "Adding new entry and instructions will be done in another workflow";
			AssertContains("Dialog text", expectedDialogText, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	#endregion

	ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName)
		=> grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");

	public void TestAnnexTabVisibility_T2L()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_ApplicationCode = "BLT";
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			CombineAssertions(() =>
			{
				using (var form = new ZForm(testDec))
				using (var userControl = GetControlToTest())
				{
					form.Controls.Add(userControl);
					form.SetDataBinding(testDec, ".");
					form.Show();

					userControl.EntriesBoundGrid.Select();
					userControl.EntriesBoundGrid.Focus();
					Application.DoEvents();

					AssertEquals("If there are no entryHeaders, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

					testDec.CustomsEntryInstructions.RemoveAndDeleteAll();
					var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					var entryInstruction2 = testDec.CustomsEntryInstructions.AddNew();
					entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

					var invoiceHeader = testDec.Invoices.AddNew();
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine.JI_CEI = entryInstruction.PK;
					var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine2.JI_CEI = entryInstruction2.PK;

					var mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
					AssertEquals("Merge done", true, mergeResult);
					Factory.Save();

					userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
					Application.DoEvents();

					AssertEquals("If there are no T2L entryHeaders, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

					entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
					mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
					AssertEquals("Merge done", true, mergeResult);
					Factory.Save();

					userControl.EntriesBoundGrid.ListManager.Position = 1;
					userControl.EntriesBoundGrid.ListManager.Position = 0;
					userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
					Application.DoEvents();

					AssertEquals("If the selected entryHeader is not T2L, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

					userControl.EntriesBoundGrid.ListManager.Position = 1;
					userControl.EntriesBoundGrid.ListManager.Position = 0;
					userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(1, 0);
					Application.DoEvents();

					AssertEquals("If the selected entryHeader is T2L, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);

					testDec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
					var supDoc = invoiceLine2.SupportingDocuments.AddNew();
					supDoc.CSI_Code = "9010";
					Factory.Save();

					userControl.EntriesBoundGrid.ListManager.Position = 1;
					userControl.EntriesBoundGrid.ListManager.Position = 0;
					userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(1, 0);
					Application.DoEvents();

					AssertEquals("If the selected entryHeader is T2L, export and has at least one supporting document with type 9010, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);
				}
			});
		}
	}

	public void TestAnnexTabVisibility_H1()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		testDec.JE_MessageType = MessageTypeList.Codes.Import;

		using (var form = new ZForm(testDec))
		using (var userControl = GetControlToTest())
		{
			CombineAssertions(() =>
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				Application.DoEvents();

				AssertEquals("If there are no entryHeaders, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				testDec.CustomsEntryInstructions.RemoveAndDeleteAll();
				var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var entryInstruction2 = testDec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

				var invoiceHeader = testDec.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				var mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);
				Factory.Save();

				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();

				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.ZG_UCC6Version = 0;
				entryHeader.MovementReferenceNumber = "MRNCODE";
				AssertEquals("With ZG_UCC6Version = 0, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				entryHeader.ZG_UCC6Version = 1;
				entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
				mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);
				Factory.Save();

				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With CH_EntryStatus = CDA and ZG_UCC6Version = 1, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);

				entryHeader.CH_EntryStatus = "JPB";
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With CH_EntryStatus = JPB and ZG_UCC6Version = 1, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With CH_EntryStatus = CDP and ZG_UCC6Version = 1, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);

				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With CH_EntryStatus = CDP, ZG_UCC6Version = 1 and CEI_Style = H2, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingJustificationCertificatesDJP;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With CH_EntryStatus = CLJ and ZG_UCC6Version = 1, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);

				entryHeader.CH_EntryStatus = "JPB";
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("Without Annex and IMP, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);

				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With Annex and IMP, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);
			});
		}

		testDec.JE_MessageType = MessageTypeList.Codes.Export;
		using (var form = new ZForm(testDec))
		using (var userControl = GetControlToTest())
		{
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			userControl.EntriesBoundGrid.Select();
			userControl.EntriesBoundGrid.Focus();
			Application.DoEvents();

			testDec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			var invoiceHeader = testDec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();

			var entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.ZG_UCC6Version = 1;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingJustificationCertificatesDJP;
			userControl.EntriesBoundGrid.ListManager.Position = 1;
			userControl.EntriesBoundGrid.ListManager.Position = 0;
			userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
			Application.DoEvents();
			AssertEquals("With CH_EntryStatus = CLJ, ZG_UCC6Version = 1 and EXP, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);
		}
	}

	public void TestAnnexTabVisibility_AES()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_ApplicationCode = "BLT";
		testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		testDec.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			using (var form = new ZForm(testDec))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				Application.DoEvents();

				AssertEquals("If there are no entryHeaders, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				testDec.CustomsEntryInstructions.RemoveAndDeleteAll();
				var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var entryInstruction2 = testDec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

				var invoiceHeader = testDec.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				var mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);
				Factory.Save();
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();

				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.MovementReferenceNumber = "MRNCODE";
				entryHeader.ZG_UCC6Version = 0;
				Factory.Save();
				AssertEquals("With ZG_UCC6Version = 0, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);
				Factory.Save();

				entryHeader.ZG_UCC6Version = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With MRN and ZG_UCC6Version = 1, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);

				entryHeader.SetCSVClearanceNum("CSV-TEST");
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With Clearance Number but no Annex, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				entryHeader.SetCSVClearanceNum("");
				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();

				var message = Factory.New<ESEDIMessage>();
				entryHeader.Messages.Add(message);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Received;
				var messagePivot = Factory.New<GenPivot>();
				messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
				messagePivot.XX_Relation1ID = docPivot.PK;
				messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
				messagePivot.XX_Relation2ID = message.PK;
				messagePivot.XX_Relation2TableCode = message.TablePrefix;

				Factory.Save();
				entryHeader.SetCSVClearanceNum("CSV-TEST");
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With Clearance Number and Annex with Message Status RCV, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);

				message.EM_Status = EDIMessage.Status.Rejected;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With Clearance Number and Annex with Message Status REJ, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);

				message.EM_Status = ZString.Empty;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With Clearance Number and Annex with Message Status empty, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);
			}
		});
	}

	public void TestAnnexTabVisibility_T2LPOUS()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_ApplicationCode = "BLT";
		testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		testDec.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			using (var form = new ZForm(testDec))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				Application.DoEvents();

				AssertEquals("If there are no entryHeaders, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				testDec.CustomsEntryInstructions.RemoveAndDeleteAll();
				var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				var entryInstruction2 = testDec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

				var invoiceHeader = testDec.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				var mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);
				Factory.Save();
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();

				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.MovementReferenceNumber = "MRNCODE";
				entryHeader.ZG_POUSVersion = 0;
				Factory.Save();
				AssertEquals("With ZG_POUSVersion = 0, Annex tab is not visible", false, userControl.AnnexTabPage.TabVisible);

				entryHeader.ZG_POUSVersion = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With MRN and ZG_POUSVersion = 1, Annex tab is visible", true, userControl.AnnexTabPage.TabVisible);

				entryHeader.SetCSVClearanceNum("CSV1234");
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With MRN and ZG_POUSVersion = 1, with clearance and no Annexes", false, userControl.AnnexTabPage.TabVisible);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);

				var message = Factory.New<ESEDIMessage>();
				entryHeader.Messages.Add(message);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Sent;
				var messagePivot = Factory.New<GenPivot>();
				messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
				messagePivot.XX_Relation1ID = docPivot.PK;
				messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
				messagePivot.XX_Relation2ID = message.PK;
				messagePivot.XX_Relation2TableCode = message.TablePrefix;

				Factory.Save();
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With MRN and ZG_POUSVersion = 1, with clearance but not message associated to he pivot with status sent ", true, userControl.AnnexTabPage.TabVisible);

				message.EM_Status = EDIMessage.Status.Rejected;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With MRN and ZG_POUSVersion = 1, with clearance but not message associated to he pivot with status rejected ", true, userControl.AnnexTabPage.TabVisible);

				message.EM_Status = ZString.Empty;
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With MRN and ZG_POUSVersion = 1, with clearance but not message associated to he pivot with status empty ", true, userControl.AnnexTabPage.TabVisible);

				entryHeader.SetCSVClearanceNum("");
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With MRN and ZG_POUSVersion = 1, without clearance but not message associated to he pivot with status rejected ", true, userControl.AnnexTabPage.TabVisible);
			}
		});
	}

	public void TestAnnexTabVisibility_AES_WithZG_RequestDispatch()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_ApplicationCode = "BLT";
		testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		testDec.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			using (var form = new ZForm(testDec))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				userControl.EntriesBoundGrid.Select();
				userControl.EntriesBoundGrid.Focus();
				Application.DoEvents();

				AssertEquals("If there are no entryHeaders, Annex tab is not ReadOnly", true, userControl.AnnexTabPage.Enabled);

				testDec.CustomsEntryInstructions.RemoveAndDeleteAll();
				var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var entryInstruction2 = testDec.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

				var invoiceHeader = testDec.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				var mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);
				Factory.Save();
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();

				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.MovementReferenceNumber = "MRNCODE";
				entryHeader.ZG_UCC6Version = 1;
				entryHeader.ZG_RequestDispatch = "N";
				Factory.Save();
				AssertEquals("With MRN, ZG_UCC6Version = 1 and ZG_RequestDispatch = N, Annex tab is not ReadOnly", true, userControl.AnnexTabPage.Enabled);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);
				Factory.Save();

				entryHeader.ZG_RequestDispatch = "Y";
				userControl.EntriesBoundGrid.ListManager.Position = 1;
				userControl.EntriesBoundGrid.ListManager.Position = 0;
				userControl.EntriesBoundGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("With MRN, ZG_UCC6Version = 1 and ZG_RequestDispatch = Y, Annex tab is ReadOnly", false, userControl.AnnexTabPage.Enabled);
			}
		});
	}

	public void TestAnnexTabVisibilityAndReadOnly_AES()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_ApplicationCode = "BLT";
		testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		testDec.JE_MessageType = MessageTypeList.Codes.Export;

		var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = testDec.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var mergeResult = testDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = testDec.CustomsEntryHeaders[0];
		entryHeader.MovementReferenceNumber = "MRNCODE";
		entryHeader.ZG_UCC6Version = 1;

		CombineAssertions(() =>
		{
			using (var form = new ZForm(testDec))
			using (var userControl = GetControlToTest())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();
				Application.DoEvents();

				entryHeader.ZG_RequestDispatch = YesNoList.Codes.No;
				AssertEquals("With MRN and ZG_UCC6Version = 1 and ZG_RequestDispatch = No, the tab is enabled = true ", true, userControl.AnnexTabPage.Enabled);

				entryHeader.ZG_RequestDispatch = YesNoList.Codes.Yes;
				AssertEquals("With MRN and ZG_UCC6Version = 1 and ZG_RequestDispatch = Yes, the tab is enabled = false ", false, userControl.AnnexTabPage.Enabled);
			}
		});
	}

	void AssertNewRequestEDIMessagesCreated(CusEntryHeader entryHeader, ZString[] messageTypes, string messageSubType = "")
	{
		var newRequestMessages = entryHeader.Messages;

		AssertEquals("messages count is correct", messageTypes.Length, newRequestMessages.Count);

		foreach (EDIMessage message in newRequestMessages)
		{
			AssertEquals("message.EM_MessageType", true, messageTypes.Contains(message.EM_MessageType));
			AssertRequestEDIMessageCreated(message, messageSubType);
		}
	}

	void AssertRequestEDIMessageCreated(EDIMessage message, string messageSubType = "")
	{
		AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
		AssertEquals("message.EM_MessageSubType", messageSubType, message.EM_MessageSubType);
		AssertEquals("message.EM_IsTestMessage", true, message.EM_IsTestMessage);
		AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		AssertEquals("message.EM_ApplicationReference", BuilderHelperTest.CertificateName, message.EM_ApplicationReference);
		AssertNull("message doesn't have interchange", message.Interchange);
	}

	protected virtual MessageUserControl GetControlToTest() => new MessageUserControl();

	class UpdateCSVClearanceFormForTesting : UpdateCSVClearanceForm
	{
		public UpdateCSVClearanceFormForTesting(CsvCodeInfo csvCodeInfo, ZString csvCode, ZString secondaryCSVCode, ZString thirdCSVCode, ZDateTime clearanceDate)
			: base(csvCodeInfo)
		{
			CSVClearance.Text = csvCode;
			SecondaryCSVNumber.Text = secondaryCSVCode;
			ThirdCSVNumber.Text = thirdCSVCode;
			ClearanceDate.DateTimeValue = clearanceDate;
			csvCodeInfo.CsvCodeFromUser = csvCode;
			csvCodeInfo.SecondaryCsvCodeFromUser = secondaryCSVCode;
			csvCodeInfo.ThirdCsvCodeFromUser = thirdCSVCode;
			csvCodeInfo.ClearanceDateFromUser = clearanceDate;
		}
	}

	class MessageUserControlForTesting : MessageUserControl
	{
		public ZString csvCode = ZString.Empty;
		public ZString secondaryCSVCode = ZString.Empty;
		public ZString thirdCSVCode = ZString.Empty;
		public ZDateTime clearanceDate = ZDateTime.Empty;

		protected override UpdateCSVClearanceForm GetUpdateCSVClearanceForm(CsvCodeInfo csvCodeInfo)
			=> new UpdateCSVClearanceFormForTesting(csvCodeInfo, csvCode, secondaryCSVCode, thirdCSVCode, clearanceDate);
	}

	public GlbStaff Staff
	{
		get
		{
			if (staff == null)
			{
				staff = Factory.GetStaffAccount();
			}

			return staff;
		}
	}
	GlbStaff staff;

	const string ExpectedMRN1ForNewExitDetailOrReport = "refNum1";
	const string ExpectedMRN2ForNewExitDetailOrReport = "refNum2";
	const string ExpectedMRN3ForNewExitDetailOrReport = "refNum3";
	const string ExpectedLRN1ForNewExitConsignment = "ES00001";
	const string ExpectedLRN2ForNewExitConsignment = "ES00002";
	const string ExpectedLRN3ForNewExitConsignment = "ES00003";
	const string ExpectedReferenceForNewExitHeader = "JD0001";
	const string ExpectedCustomsOfficeForNewExitDetailOrReport = "ES009999";
	const string ExpectedInlandMOTForNewExitReport = "SEA";

	CusExitControlHeader[] GetExitHeadersForDeclarationEDI(JobDeclaration declaration)
	{
		var query = new ZQuery(CusExitControlHeaderSchema.CEH_ParentID, declaration.PK);
		var exitHeaders = Factory.Load<CusExitControlHeader>(query);
		return exitHeaders;
	}

	BusinessObject[] GetExitHeadersForDeclarationAES(JobDeclaration declaration)
	{
		var query = new ZQuery(CusExitHeaderSchema.CXH_ParentID, declaration.PK);
		var exitHeaders = (BusinessObject[])Factory.Load<ESExitControl.ICusExitHeader>(query);
		return exitHeaders;
	}

	void CreateT2CEntryNumber(CusEntryHeader entryHeader, string mrnCode)
	{
		var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = mrnCode;
		newEntryNumber.CE_IssueDate = ZDateTime.Today;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
	}

	public static ZGuid CreateCusExitHeader(JobDeclaration declaration, string referenceNum, int clusterKey = 1)
	{
		var pkHeader = Guid.NewGuid();
		var sqlHeaderWithParent = @"
INSERT INTO dbo.CusExitHeader (CXH_PK, CXH_ApplicationCode, CXH_AutoVersion, CXH_ClusterKey, CXH_IsValid, CXH_JobReference, CXH_SystemCreateTimeUtc, CXH_SystemCreateUser, CXH_SystemLastEditTimeUtc, CXH_SystemLastEditUser, CXH_GB_Branch, CXH_GC_Company, CXH_ParentID, CXH_ParentTableCode)
VALUES
(@CXH_PK, 'XIT', 1, @CXH_ClusterKey, 1, @CXH_JobReference, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', @CXH_GB_Branch, @CXH_GC_Company, @CXH_ParentID, @CXH_ParentTableCode)
";
		var sqlHeaderWithoutParent = @"
INSERT INTO dbo.CusExitHeader (CXH_PK, CXH_ApplicationCode, CXH_AutoVersion, CXH_ClusterKey, CXH_IsValid, CXH_JobReference, CXH_SystemCreateTimeUtc, CXH_SystemCreateUser, CXH_SystemLastEditTimeUtc, CXH_SystemLastEditUser, CXH_GB_Branch, CXH_GC_Company)
VALUES
(@CXH_PK, 'XIT', 1, @CXH_ClusterKey, 1, @CXH_JobReference, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', @CXH_GB_Branch, @CXH_GC_Company)
";

		if (declaration != null)
		{
			using (DbCommand command = Db.Connection.Command(sqlHeaderWithParent))
			{
				command.AddParameter("@CXH_PK", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@CXH_GB_Branch", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				command.AddParameter("@CXH_GC_Company", SqlDbType.UniqueIdentifier, Env.CurrentCompanyPK);
				command.AddParameter("@CXH_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@CXH_JobReference", SqlDbType.VarChar, referenceNum);
				command.AddParameter("@CXH_ParentID", SqlDbType.UniqueIdentifier, declaration.PK.ToGuid());
				command.AddParameter("@CXH_ParentTableCode", SqlDbType.VarChar, declaration.TablePrefix);
				command.ExecuteNonQuery();
			}
		}
		else
		{
			using (DbCommand command = Db.Connection.Command(sqlHeaderWithoutParent))
			{
				command.AddParameter("@CXH_PK", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@CXH_GB_Branch", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				command.AddParameter("@CXH_GC_Company", SqlDbType.UniqueIdentifier, Env.CurrentCompanyPK);
				command.AddParameter("@CXH_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@CXH_JobReference", SqlDbType.VarChar, referenceNum);
				command.ExecuteNonQuery();
			}
		}

		return (ZGuid)pkHeader;
	}

	public static ZGuid CreateCusExitConsignment(ZGuid pkHeader, string mrn, string localReference, int clusterKey = 1)
	{
		var pkConsignment = Guid.NewGuid();
		var sqlConsginment = @"
INSERT INTO dbo.CusExitConsignment (CXC_PK, CXC_AutoVersion, CXC_ClusterKey, CXC_IsValid, CXC_CXH_Header, CXC_MovementReference, CXC_LocalReference, CXC_SystemCreateTimeUtc, CXC_SystemCreateUser, CXC_SystemLastEditTimeUtc, CXC_SystemLastEditUser)
	VALUES (@CXC_PK, 1, @CXC_ClusterKey, 1, @CXC_CXH_Header, @CXC_MovementReference, @CXC_LocalReference, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
		using (DbCommand command = Db.Connection.Command(sqlConsginment))
		{
			command.AddParameter("@CXC_PK", SqlDbType.UniqueIdentifier, pkConsignment);
			command.AddParameter("@CXC_CXH_Header", SqlDbType.UniqueIdentifier, pkHeader.ToGuid());
			command.AddParameter("@CXC_MovementReference", SqlDbType.VarChar, mrn);
			command.AddParameter("@CXC_LocalReference", SqlDbType.VarChar, localReference);
			command.AddParameter("@CXC_ClusterKey", SqlDbType.Int, clusterKey);
			command.ExecuteNonQuery();
		}
		return (ZGuid)pkConsignment;
	}

	public static ZGuid CreateCusExitReport(ZGuid pkHeader, ZGuid pkConsignment, string officeOfExit, string mot, string messageStatus = "", int clusterKey = 1)
	{
		var pkReport = Guid.NewGuid();
		var sqlReport = @"
INSERT INTO dbo.CusExitReport (CER_PK, CER_ClusterKey, CER_IsValid, CER_CXH_Header, CER_Type, CER_SystemCreateTimeUtc, CER_SystemCreateUser, CER_SystemLastEditTimeUtc, CER_SystemLastEditUser, CER_Behavior, CER_CXC_Consignment, CER_OfficeOfExit, CER_TransportMode, CER_MessageStatus)
	VALUES (@CER_PK, @CER_ClusterKey, 1, @CER_CXH_Header, 'PRE', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'DIS', @CER_CXC_Consignment, @CER_OfficeOfExit, @CER_TransportMode, @CER_MessageStatus)
";
		using (DbCommand command = Db.Connection.Command(sqlReport))
		{
			command.AddParameter("@CER_PK", SqlDbType.UniqueIdentifier, pkReport);
			command.AddParameter("@CER_CXH_Header", SqlDbType.UniqueIdentifier, pkHeader.ToGuid());
			command.AddParameter("@CER_CXC_Consignment", SqlDbType.UniqueIdentifier, pkConsignment.ToGuid());
			command.AddParameter("@CER_OfficeOfExit", SqlDbType.VarChar, officeOfExit);
			command.AddParameter("@CER_TransportMode", SqlDbType.VarChar, mot);
			command.AddParameter("@CER_MessageStatus", SqlDbType.VarChar, messageStatus);
			command.AddParameter("@CER_ClusterKey", SqlDbType.Int, clusterKey);
			command.ExecuteNonQuery();
		}
		return (ZGuid)pkReport;
	}

	public static void UpdateCusExitReportStatus(ZGuid pkHeader, string status)
	{
		var sqlReport = @"
UPDATE dbo.CusExitReport 
SET 
    CER_Status = @CER_Status, 
    CER_SystemLastEditTimeUtc = GETUTCDATE(), 
    CER_SystemLastEditUser = '~BP' 
WHERE 
    CER_CXH_Header = @CER_CXH_Header;";
		using (DbCommand command = Db.Connection.Command(sqlReport))
		{
			command.AddParameter("@CER_CXH_Header", SqlDbType.UniqueIdentifier, pkHeader.ToGuid());
			command.AddParameter("@CER_Status", SqlDbType.VarChar, status);
			command.ExecuteNonQuery();
		}
	}
}
