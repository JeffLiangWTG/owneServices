using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class MonthlyClosingMessagingMenuTest : TestCaseWithFactory
	{
		public void TestMenuCaption()
		{
			AssertEquals("&Messages", menu.CaptionResourceString.Caption);
		}

		public void TestAllMenuItems()
		{
			AssertArrayEqualsByElements(new[]
				{
					"Final Message (47)",
					"First Partial Message (9)",
					"Amendment Message (2)",
					"Modification Message (36)",
					"Finalization Message (22)"
				},
				menu.MenuItems.Cast<ZMenuItem>().Select(x => x.Caption.ToString()).ToArray());
		}

		public void TestSendMonthlyClosingDeclaration_FormPreSaved()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZA;
				using (var form = new MonthlyClosingForm(declaration))
				{
					form.Show();
					var menu = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Messages");
					CombineAssertions(() =>
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						menu.MenuItems.FindByText("Modification Message (36)").PerformClick();
						AssertEquals("Caption", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("HasChanges", true, declaration.HasChanges);
					});
				}
			}
		}

		public void TestFinalMessageMenuItem_RegistrationNumberEmpty()
		{
			var menuItem = menu.MenuItems.FindByText("Final Message (47)");
			AssertEquals("'Final Message(47)' is enabled when RegistrationNumber is empty", true, menuItem.Enabled);
		}

		public void TestFinalMessageMenuItem_RegistrationNumberNotEmpty()
		{
			AddRegistrationNumberToDeclaration();
			var menuItem = menu.MenuItems.FindByText("Final Message (47)");
			AssertEquals("'Final Message(47)' is disabled when RegistrationNumber is not empty", false, menuItem.Enabled);
		}

		public void TestFinalMessageMenuItem_StatusSNT()
		{
			declaration.CRD_MessageStatus = "SNT";
			var menuItem = menu.MenuItems.FindByText("Final Message (47)");

			AssertEquals("'Final Message(47)' is disabled when MessageStatus = SNT", false, menuItem.Enabled);
		}

		public void TestFinalMessageMenuItem_StatusNotSNT()
		{
			declaration.CRD_MessageStatus = string.Empty;

			var menuItem = menu.MenuItems.FindByText("Final Message (47)");

			AssertEquals("'Final Message(47)' is enabled when MessageStatus != SNT", true, menuItem.Enabled);
		}

		public void TestFirstPartialMessageMenuItem_RegistrationNumberEmpty()
		{
			var menuItem = menu.MenuItems.FindByText("First Partial Message (9)");
			AssertEquals("'First Partial Message (9)' is enabled when RegistrationNumber is empty", true, menuItem.Enabled);
		}

		public void TestFirstPartialMessageMenuItem_RegistrationNumberNotEmpty()
		{
			AddRegistrationNumberToDeclaration();
			var menuItem = menu.MenuItems.FindByText("First Partial Message (9)");
			AssertEquals("'First Partial Message (9)' is disabled when RegistrationNumber is not empty", false, menuItem.Enabled);
		}

		public void TestFirstPartialMessageMenuItem_StatusSNT()
		{
			declaration.CRD_MessageStatus = "SNT";
			var menuItem = menu.MenuItems.FindByText("First Partial Message (9)");

			AssertEquals("'First Partial Message (9)' is disabled when MessageStatus is SNT", false, menuItem.Enabled);
		}

		public void TestFirstPartialMessageMenuItem_StatusNotSNT()
		{
			declaration.CRD_MessageStatus = string.Empty;
			var menuItem = menu.MenuItems.FindByText("First Partial Message (9)");

			AssertEquals("'First Partial Message (9)' is enabled when MessageStatus is not SNT", true, menuItem.Enabled);
		}

		public void TestAmendmentMessageMenuItem_RegistrationNumberEmpty()
		{
			var menuItem = menu.MenuItems.FindByText("Amendment Message (2)");
			AssertEquals("'Amendment Message (2)' is disabled when RegistrationNumber is empty", false, menuItem.Enabled);
		}

		public void TestAmendmentMessageMenuItem_RegistrationNumberNotEmpty()
		{
			AddRegistrationNumberToDeclaration();
			var menuItem = menu.MenuItems.FindByText("Amendment Message (2)");
			AssertEquals("'Amendment Message (2)' is enabled when RegistrationNumber is not empty", true, menuItem.Enabled);
		}

		public void TestAmendmentMessageMenuItem_StatusSNT()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_MessageStatus = "SNT";
			var menuItem = menu.MenuItems.FindByText("Amendment Message (2)");

			AssertEquals("'Amendment Message (2)' is disabled when MessageStatus is SNT", false, menuItem.Enabled);
		}

		public void TestAmendmentMessageMenuItem_StatusNotSNT()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_MessageStatus = string.Empty;
			declaration.CreateFinalizationFlagNote("0");
			var menuItem = menu.MenuItems.FindByText("Amendment Message (2)");

			AssertEquals("'Amendment Message (2)' is enabled when MessageStatus is not SNT", true, menuItem.Enabled);
		}

		public void TestAmendmentMessageMenuItem_FinalizationOne()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_MessageStatus = string.Empty;
			declaration.CreateFinalizationFlagNote("1");
			var menuItem = menu.MenuItems.FindByText("Amendment Message (2)");

			AssertEquals("'Amendment Message (2)' is disabled when Finalization is '1'", false, menuItem.Enabled);
		}

		public void TestModificationMessageMenuItem_RegistrationNumberEmpty()
		{
			var menuItem = menu.MenuItems.FindByText("Modification Message (36)");
			AssertEquals("'Modification Message (36)' is disabled when RegistrationNumber is empty", false, menuItem.Enabled);
		}

		public void TestModificationMessageMenuItem_RegistrationNumberNotEmpty()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "ERR";
			var menuItem = menu.MenuItems.FindByText("Modification Message (36)");
			AssertEquals("'Modification Message (36)' is disabled when RegistrationNumber is not empty, CustomsStatus does not fit", false, menuItem.Enabled);
		}

		public void TestModificationMessageMenuItem_RegistrationNumberNotEmptyStatusRC2()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "RC2";
			var menuItem = menu.MenuItems.FindByText("Modification Message (36)");
			AssertEquals("'Modification Message (36)' is enabled when RegistrationNumber is not empty, CustomsStatus RC2", true, menuItem.Enabled);
		}

		public void TestModificationMessageMenuItem_RegistrationNumberNotEmptyStatusTX4()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "TX4";
			var menuItem = menu.MenuItems.FindByText("Modification Message (36)");
			AssertEquals("'Modification Message (36)' is enabled when RegistrationNumber is not empty, CustomsStatus TX2", true, menuItem.Enabled);
		}

		public void TestModificationMessageMenuItem_StatusSNT()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_MessageStatus = "SNT";
			declaration.CRD_CustomsStatus = "TX4";
			var menuItem = menu.MenuItems.FindByText("Modification Message (36)");
			AssertEquals("'Modification Message (36)' is disabled when MessageStatus is SNT", false, menuItem.Enabled);
		}

		public void TestModificationMessageMenuItem_ForBondedWarehouse()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "TX5";
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZL;
			var menuItem = menu.MenuItems.FindByText("Modification Message (36)");
			AssertEquals("'Modification Message (36)' is enabled when RegistrationNumber is not empty, CustomsStatus TX5, DeclarationType VZL", true, menuItem.Enabled);

			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AZL;
			var menuItem2 = menu.MenuItems.FindByText("Modification Message (36)");
			AssertEquals("'Modification Message (36)' is enabled when RegistrationNumber is not empty, CustomsStatus TX5, DeclarationType AZL", true, menuItem2.Enabled);

			declaration.CRD_CustomsStatus = "TX3";
			var menuItem3 = menu.MenuItems.FindByText("Modification Message (36)");
			AssertEquals("'Modification Message (36)' is not enabled when RegistrationNumber is not empty, CustomsStatus TX3, DeclarationType AZL", false, menuItem3.Enabled);
		}

		public void TestFinalizationMessageMenuItem_RegistrationNumberEmpty()
		{
			var menuItem = menu.MenuItems.FindByText("Finalization Message (22)");
			AssertEquals("'Finalization Message (22)' is disabled when RegistrationNumber is empty", false, menuItem.Enabled);
		}

		public void TestFinalizationMessageMenuItem_RegistrationNumberNotEmpty()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "ERR";
			var menuItem = menu.MenuItems.FindByText("Finalization Message (22)");
			AssertEquals("'Finalization Message (22)' is disabled when RegistrationNumber is not empty, CustomsStatus does not fit", false, menuItem.Enabled);
		}

		public void TestFinalizationMessageMenuItem_RegistrationNumberNotEmptyStatusRC2()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "RC2";
			var menuItem = menu.MenuItems.FindByText("Finalization Message (22)");
			AssertEquals("'Finalization Message (22)' is enabled when RegistrationNumber is not empty, CustomsStatus RC2", true, menuItem.Enabled);
		}

		public void TestFinalizationMessageMenuItem_RegistrationNumberNotEmptyStatusTX4()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "TX4";
			var menuItem = menu.MenuItems.FindByText("Finalization Message (22)");
			AssertEquals("'Finalization Message (22)' is enabled when RegistrationNumber is not empty, CustomsStatus TX2", true, menuItem.Enabled);
		}

		public void TestFinalizationMessageMenuItem_StatusSNT()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_MessageStatus = "SNT";
			declaration.CRD_CustomsStatus = "TX4";
			var menuItem = menu.MenuItems.FindByText("Finalization Message (22)");
			AssertEquals("'Finalization Message (22)' is disabled when MessageStatus is SNT", false, menuItem.Enabled);
		}

		public void TestFinalMessageMenuItem_Click()
		{
			AssertClickAndMessageRole("Final Message (47)", "47");
		}

		public void TestFirstPartialMessageMenuItem_Click()
		{
			AssertClickAndMessageRole("First Partial Message (9)", "9");
		}

		public void TestAmendmentMessageMenuItem_Click()
		{
			AddRegistrationNumberToDeclaration();
			AssertClickAndMessageRole("Amendment Message (2)", "2");
		}

		public void TestModificationMessageMenuItem_Click()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "TX4";
			reconEntryLine.CRL_CustomsStatus = "ERR";
			reconEntry.CusReconSnapshots.AddNew().CRS_Type = "CUR";
			AssertClickAndMessageRole("Modification Message (36)", "36");
		}

		public void TestFinalizationMessageMenuItem_Click()
		{
			AddRegistrationNumberToDeclaration();
			declaration.CRD_CustomsStatus = "TX4";
			AssertClickAndMessageRole("Finalization Message (22)", "22");
		}

		public void TestModificationMessageMenuItem_Click_NoData()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZA;
				using (var form = new MonthlyClosingForm(declaration))
				{
					form.Show();
					var menu = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Messages");
					CombineAssertions(() =>
					{
						menu.MenuItems.FindByText("Modification Message (36)").PerformClick();
						AssertEquals("Message has been sent notification appears", "No items found that match this message function. No Message was sent.", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		public void TestModificationMessageMenuItem_Click_IsValidBondedWarehouseData()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };

			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				declaration.CRD_CustomsStatus = "TX5";
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZL;
				reconEntryLine.CRL_CustomsStatus = "TX5";
				var snapshot = reconEntry.CusReconSnapshots.AddNew();
				snapshot.CRS_Type = CusReconConstants.Current;

				using (var form = new MonthlyClosingForm(declaration))
				{
					form.Show();
					var menu = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Messages");

					CombineAssertions(() =>
					{
						AssertEquals("Has CUR snapshot", "Yes", reconEntry.EntryHasChanges);
						menu.MenuItems.FindByText("Modification Message (36)").PerformClick();
						AssertEquals("Message has been sent notification appears", "The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}

				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AZL;
				reconEntryLine.CRL_CustomsStatus = "TX2";

				using (var form = new MonthlyClosingForm(declaration))
				{
					form.Show();
					var menu = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Messages");

					CombineAssertions(() =>
					{
						menu.MenuItems.FindByText("Modification Message (36)").PerformClick();
						AssertEquals("No items found that match this message function. No Message was sent.", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}

				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZL;
				reconEntryLine.CRL_CustomsStatus = "TX5";
				snapshot.CRS_Type = CusReconConstants.Lodged;

				using (var form = new MonthlyClosingForm(declaration))
				{
					form.Show();
					var menu = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Messages");

					CombineAssertions(() =>
					{
						AssertEquals("No CUR snapshot", ZString.Empty, reconEntry.EntryHasChanges);
						menu.MenuItems.FindByText("Modification Message (36)").PerformClick();
						AssertEquals("No change: No message sent", "No items found that match this message function. No Message was sent.", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		public void TestSendMonthlyClosingDeclaration_InvalidDeclarationType()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				declaration.CRD_DeclarationType = ImportDeclarationTypeList.Codes.EZA;
				using (var form = new MonthlyClosingForm(declaration))
				{
					form.Show();
					var menu = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Messages");
					menu.MenuItems.FindByText("Final Message (47)").PerformClick();
					AssertEquals(0, declaration.Messages.Count);
				}
			}
		}

		public void TestSendMonthlyClosingDeclarationFreeCirculation()
		{
			new[] { MonthlyClosingDeclarationTypeList.Codes.AZ, MonthlyClosingDeclarationTypeList.Codes.VZA }.ForEach(x => AssertSendMonthlyClosingDeclaration(x, nameof(ATLASVersion10_1.ECFCPF)));
		}

		public void TestSendMonthlyClosingDeclarationInwardProcessing()
		{
			new[] { MonthlyClosingDeclarationTypeList.Codes.AAV, MonthlyClosingDeclarationTypeList.Codes.VAV }.ForEach(x => AssertSendMonthlyClosingDeclaration(x, nameof(ATLASVersion10_1.VSCIPK)));
		}

		public void TestSendMonthlyClosingDeclarationCustomsWarehouse()
		{
			new[] { MonthlyClosingDeclarationTypeList.Codes.AZL, MonthlyClosingDeclarationTypeList.Codes.VZL }.ForEach(x => AssertSendMonthlyClosingDeclaration(x, nameof(ATLASVersion10_1.LSCWPM)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<CusReconDeclaration>();
			// setup declaration with something to send
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 42;
			var invoice = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			reconEntry = declaration.CusReconEntries.AddNew();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryType = ImportDeclarationTypeList.Codes.AZ;
			reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			reconEntryLine = reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_OriginalEntryLineNumber = entryLine.CL_LineNumber;
		}
		CusReconDeclaration declaration;
		CusReconEntry reconEntry;
		CusReconEntryLine reconEntryLine;
		MonthlyClosingMessagingMenu menu => new MonthlyClosingMessagingMenu(declaration);

		void AssertClickAndMessageRole(string menuItemText, string roleNumber)
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VZA;
				using (var form = new MonthlyClosingForm(declaration))
				{
					form.Show();
					var menu = form.Menu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(x => x.Text == "&Messages") as MonthlyClosingMessagingMenu;
					var menuItem = menu.MenuItems.FindByText(menuItemText);
					CombineAssertions(() =>
					{
						menuItem.PerformClick();
						AssertEquals("Message has been sent notification appears", "The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains("Correct RoleNumber in message text", $"<MessageRole>{roleNumber}</MessageRole>", ((EDIMessage)declaration.Messages.SingleOrDefault()).EM_MessageText);
						menu.ShowPopupMenu();
						AssertEquals("MenuItem is disabled as the Status is now SNT", false, menuItem.Enabled);
					});
				}
			}
		}

		void AssertSendMonthlyClosingDeclaration(string declarationType, string expectedApplicationReference)
		{
			declaration.Messages.RemoveAll();
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				declaration.CRD_DeclarationType = declarationType;
				using (var form = new MonthlyClosingForm(declaration))
				{
					form.Show();
					var menu = form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Messages");
					menu.MenuItems.FindByText("Final Message (47)").PerformClick();
					var message = (EDIMessage)declaration.Messages.SingleOrDefault();
					AssertEquals("EM_ApplicationReference", expectedApplicationReference, message.EM_ApplicationReference);
				}
			}
		}

		void AddRegistrationNumberToDeclaration()
		{
			var entryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			entryNumber.CE_EntryNum = "12345678";
		}
	}
}
