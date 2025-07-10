using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;

namespace Enterprise.Customs.ES.Module.Testing
{
	[TestedType(typeof(EntryHeaderModule))]
	public class EntryHeaderModuleTest : EU.Module.Testing.EntryHeaderModuleTest
	{
		public void TestGuaranteeCheckAccountingStatusMenuItem()
		{
			using (var module = new EntryHeaderModule())
			{
				CombineAssertions(() =>
				{
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					AssertNotNull("actionMenuItem", actionMenuItem);

					var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");
					AssertNotNull("checkAccountingStatusMenuItem", checkAccountingStatusMenuItem);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ExportEntries()
		{
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export);

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				var entryHeader2 = declaration.CustomsEntryHeaders[1];

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are export", "No entries selected match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ImportEntriesT2L()
		{
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory, entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L, entryInstructionSubStyle2: EntrySubStyleList.Codes.T2C);

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				var entryHeader2 = declaration.CustomsEntryHeaders[1];

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import but T2L/T2C", "No entries selected match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ImportEntriesH2()
		{
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				var entryHeader2 = declaration.CustomsEntryHeaders[1];

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import but H2", "No entries selected match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ImportEntriesNoMRN()
		{
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory);

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				var entryHeader2 = declaration.CustomsEntryHeaders[1];

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import but without mrn", "No entries selected match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ImportEntriesNoCSVClearance()
		{
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory);

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);

				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader2.MovementReferenceNumberSetter("MRNCode2", ZDateTime.Today);

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import with mrn but without csv clearance", "No entries selected match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ImportEntriesNoGuarantees()
		{
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory);

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
				entryHeader1.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096", addOBLTransaction: false);
				ModuleTestHelper.AddGuaranteeToDeclaration(declaration, entryHeader1.CH_CEI_Instruction, "16ESAGL9990000096");

				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader2.MovementReferenceNumberSetter("MRNCode2", ZDateTime.Today);
				entryHeader2.SetCSVClearanceNum("34YEUXEP8E7GE9XD");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "22ESAGL9990000096", withOldEndDate: true);
				ModuleTestHelper.AddGuaranteeToDeclaration(declaration, entryHeader2.CH_CEI_Instruction, "22ESAGL9990000096");

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import with mrn and csv clearance but without a correct guarantee associated", "No entries selected match criteria to check accounting status", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ImportEntriesNoBrokerOrCertificate()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory);
				declaration.JE_DeclarationReference = "B00169757";

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
				entryHeader1.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096");
				ModuleTestHelper.AddGuaranteeToDeclaration(declaration, entryHeader1.CH_CEI_Instruction, "16ESAGL9990000096");

				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader2.MovementReferenceNumberSetter("MRNCode2", ZDateTime.Today);
				entryHeader2.SetCSVClearanceNum("34YEUXEP8E7GE9XD");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "22ESAGL9990000096");
				ModuleTestHelper.AddGuaranteeToDeclaration(declaration, entryHeader2.CH_CEI_Instruction, "22ESAGL9990000096");

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import with mrn and csv clearance and correct guarantees associated but no broker is declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options in the following declarations: B00169757.", UnitTestUserNotification.Instance.LastMessage.Text);

					declaration.JE_GS_NKCusAgent = staff.GS_Code;
					declaration.JE_CustomsProfile = ZString.Empty;
					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import with mrn and csv clearance and correct guarantees associated but no certificate is declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options in the following declarations: B00169757.", UnitTestUserNotification.Instance.LastMessage.Text);

					declaration.JE_CustomsProfile = "INVALID";
					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import with mrn and csv clearance and correct guarantees associated but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options in the following declarations: B00169757.", UnitTestUserNotification.Instance.LastMessage.Text);

					declaration.JE_CustomsProfile = cert.GP_Name;
					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when all entries selected are import with mrn and csv clearance and correct guarantees associated but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options in the following declarations: B00169757.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ImportEntriesAEAT()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory);
				declaration.JE_GS_NKCusAgent = staff.GS_Code;
				declaration.JE_CustomsProfile = cert.GP_Name;

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
				entryHeader1.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096");
				ModuleTestHelper.AddGuaranteeToDeclaration(declaration, entryHeader1.CH_CEI_Instruction, "16ESAGL9990000096");

				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader2.MovementReferenceNumberSetter("MRNCode2", ZDateTime.Today);
				entryHeader2.SetCSVClearanceNum("34YEUXEP8E7GE9XD");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "22ESAGL9990000096");
				ModuleTestHelper.AddGuaranteeToDeclaration(declaration, entryHeader2.CH_CEI_Instruction, "22ESAGL9990000096");

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Messages sent correctly", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("2 Messages sent successfully."));

					AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
					AssertEquals("Entry Message Status has changed to awaiting for second entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);

					ModuleTestHelper.AssertMessageSent("entryHeader1", entryHeader1, false);
					ModuleTestHelper.AssertMessageSent("entryHeader2", entryHeader2, false);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_ImportEntriesATC()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, BuilderHelperTest.CanaryIslandCode, "Test 61");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declaration = ModuleTestHelper.GetDeclarationWith2Entries(Factory);
				declaration.JE_GS_NKCusAgent = staff.GS_Code;
				declaration.JE_CustomsProfile = cert.GP_Name;
				declaration.ZG_DestinationState = BuilderHelperTest.CanaryIslandCode;

				var entryHeader1 = declaration.CustomsEntryHeaders[0];
				entryHeader1.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
				entryHeader1.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096");
				ModuleTestHelper.AddGuaranteeToDeclaration(declaration, entryHeader1.CH_CEI_Instruction, "16ESAGL9990000096");

				var entryHeader2 = declaration.CustomsEntryHeaders[1];
				entryHeader2.MovementReferenceNumberSetter("MRNCode2", ZDateTime.Today);
				entryHeader2.SetCSVClearanceNum("34YEUXEP8E7GE9XD");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "22ESAGL9990000096");
				ModuleTestHelper.AddGuaranteeToDeclaration(declaration, entryHeader2.CH_CEI_Instruction, "22ESAGL9990000096");

				var entryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				entryHeaders.Add(entryHeader1);
				entryHeaders.Add(entryHeader2);

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
				{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Messages sent correctly", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("2 Messages sent successfully."));

					AssertEquals("Entry Message Status has changed to awaiting for first entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
					AssertEquals("Entry Message Status has changed to awaiting for second entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);

					ModuleTestHelper.AssertMessageSent("entryHeader1", entryHeader1, true);
					ModuleTestHelper.AssertMessageSent("entryHeader2", entryHeader2, true);
				});
			}
		}

		public void TestGuaranteeCheckAccountingStatusMenuItem_Functionality_MultipleDeclarations()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, BuilderHelperTest.CanaryIslandCode, "Test 61");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var module = new EntryHeaderModule())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var checkAccountingStatusMenuItem = actionMenuItem.MenuItems.FindByText("Guarantee – Check accounting status");

				var declarationAEAT = ModuleTestHelper.GetDeclarationWith2Entries(Factory);
				declarationAEAT.JE_GS_NKCusAgent = staff.GS_Code;
				declarationAEAT.JE_CustomsProfile = cert.GP_Name;

				var entryHeaderAEAT1 = declarationAEAT.CustomsEntryHeaders[0];
				entryHeaderAEAT1.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
				entryHeaderAEAT1.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "16ESAGL9990000096");
				ModuleTestHelper.AddGuaranteeToDeclaration(declarationAEAT, entryHeaderAEAT1.CH_CEI_Instruction, "16ESAGL9990000096");

				var entryHeaderAEAT2 = declarationAEAT.CustomsEntryHeaders[1];
				entryHeaderAEAT2.MovementReferenceNumberSetter("MRNCode2", ZDateTime.Today);
				entryHeaderAEAT2.SetCSVClearanceNum("34YEUXEP8E7GE9XD");
				ModuleTestHelper.CreateGuaranteeHeaderDetail(Factory, "22ESAGL9990000096");
				ModuleTestHelper.AddGuaranteeToDeclaration(declarationAEAT, entryHeaderAEAT2.CH_CEI_Instruction, "22ESAGL9990000096");

				var declarationATC = ModuleTestHelper.GetDeclarationWith2Entries(Factory);
				declarationATC.JE_GS_NKCusAgent = staff.GS_Code;
				declarationATC.JE_CustomsProfile = cert.GP_Name;
				declarationATC.ZG_DestinationState = BuilderHelperTest.CanaryIslandCode;

				var entryHeaderATC1 = declarationATC.CustomsEntryHeaders[0];
				entryHeaderATC1.MovementReferenceNumberSetter("MRNCode1", ZDateTime.Today);
				entryHeaderATC1.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
				ModuleTestHelper.AddGuaranteeToDeclaration(declarationATC, entryHeaderATC1.CH_CEI_Instruction, "16ESAGL9990000096");

				var entryHeaderATC2 = declarationATC.CustomsEntryHeaders[1];
				entryHeaderATC2.SetCSVClearanceNum("34YEUXEP8E7GE9XD");
				ModuleTestHelper.AddGuaranteeToDeclaration(declarationATC, entryHeaderATC2.CH_CEI_Instruction, "22ESAGL9990000096");

				var declarationEXP = ModuleTestHelper.GetDeclarationWith2Entries(Factory, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export);
				var entryHeaderEXP1 = declarationEXP.CustomsEntryHeaders[0];
				var entryHeaderEXP2 = declarationEXP.CustomsEntryHeaders[1];

				var entryHeaders = new CusEntryHeader[] { entryHeaderAEAT1, entryHeaderAEAT2, entryHeaderATC1, entryHeaderATC2, entryHeaderEXP1, entryHeaderEXP2 };

				module.DisplayGrid.SetDataBinding(entryHeaders, "");

				CombineAssertions(() =>
	{
					module.DisplayGrid.Select();
					module.DisplayGrid.Focus();

					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Error message when no entry was selected", "Please select at least one entry", UnitTestUserNotification.Instance.LastMessage.Text);

					module.DisplayGrid.SelectAllElements();
					checkAccountingStatusMenuItem.PerformClick();
					AssertEquals("Messages sent correctly", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("3 Messages sent successfully."));

					AssertEquals("Entry Message Status has changed to awaiting for first entry header AEAT", MessageStatusList.Codes.AwaitingResponse, entryHeaderAEAT1.CH_Status);
					AssertEquals("Entry Message Status has changed to awaiting for second entry header AEAT", MessageStatusList.Codes.AwaitingResponse, entryHeaderAEAT2.CH_Status);
					AssertEquals("Entry Message Status has changed to awaiting for first entry header ATC", MessageStatusList.Codes.AwaitingResponse, entryHeaderATC1.CH_Status);
					AssertEquals("Entry Message Status has not changed for second entry header ATC", ZString.Empty, entryHeaderATC2.CH_Status);
					AssertEquals("Entry Message Status has not changed for first entry header EXP", ZString.Empty, entryHeaderEXP1.CH_Status);
					AssertEquals("Entry Message Status has not changed for second entry header EXP", ZString.Empty, entryHeaderEXP2.CH_Status);

					ModuleTestHelper.AssertMessageSent("entryHeaderAEAT1", entryHeaderAEAT1, false);
					ModuleTestHelper.AssertMessageSent("entryHeaderAEAT2", entryHeaderAEAT2, false);
					ModuleTestHelper.AssertMessageSent("entryHeaderATC1", entryHeaderATC1, true);
					AssertEquals("entryHeaderATC2 has no messages", 0, entryHeaderATC2.Messages.Count);
					AssertEquals("entryHeaderEXP1 has no messages", 0, entryHeaderEXP1.Messages.Count);
					AssertEquals("entryHeaderEXP2 has no messages", 0, entryHeaderEXP2.Messages.Count);
				});
			}
		}

		protected override EU.Module.EntryHeaderModule GetNewEntryHeaderModule() => new EntryHeaderModule();

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
