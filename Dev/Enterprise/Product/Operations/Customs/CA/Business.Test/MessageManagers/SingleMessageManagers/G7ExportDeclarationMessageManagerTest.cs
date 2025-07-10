using System;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(G7ExportDeclarationMessageManager))]
	sealed class G7ExportDeclarationMessageManagerTest : CAMessageManagerTestCase
	{
		public override void TestMessageFriendlyName()
		{
			AssertContains("MessageFriendlyName", "G7 Export Declaration for ", messageManager.MessageFriendlyName);
		}

		public void TestLogCustomsCommenced()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "123456");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			Factory.Save();

			var manager = new G7ExportDeclarationMessageManagerForTesting(new G7ExportMessageWrapper(entryHeader), notification);
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Create, false);

			var сommencedEvent = declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced, "CA Export");
			AssertNotNull("Export Customs Commenced event should exist on declaration", сommencedEvent);

			manager.ResetDeclaration();
			сommencedEvent = declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced, "CA Export");
			AssertNull("Export Customs Commenced event should be cancelled", сommencedEvent);
			var cancelledEvent = declaration.Logs.MostRecentLogByEventTime(Events.Cancelled);
			AssertNotNull("Export Customs Commenced event cancelled should exist on declaration", cancelledEvent);
		}

		public override void TestPopulateMessages()
		{
			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "21311X";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			var helper = new DeclarationTestHelper(Factory, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = helper.Consignor.PK;
			declaration.JE_OH_Importer = helper.Consignee.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B00123457";
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			invoice1Line1.JI_CL = entryLine.PK;

			var manager = new G7ExportDeclarationMessageManagerForTesting(new G7ExportMessageWrapper(declaration.CustomsEntryHeaders[0]), notification);
			manager.PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals("1 message", 1, entryHeader.Messages.Count);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodes.Codes.Original, entryHeader.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		}

		public override void TestCanSendThisMessage()
		{
			using (ZArchitecture.Environment.Globals.SetIsWinzorForTest(true))
			using (ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(true))
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var customsDefaultToCurrentLoginDeptRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
				customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.CAG7MsgSend.IsAllowed = false;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoice1 = declaration.Invoices.AddNew();
				var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				Factory.Save();
				entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;

				var manager = new G7ExportDeclarationMessageManagerForTesting(new G7ExportMessageWrapper(declaration.CustomsEntryHeaders[0]), notification);

				ZString messageText;
				Assert("Has Changes", declaration.HasChanges);
				Assert("Has Changes", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				Assert("Has Changes(" + messageText + ")", messageText.Contains("Job not yet saved, Please save before sending."));

				entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				entryLine.HasChanges = false;
				entryHeader.HasChanges = false;
				invoice1Line1.HasChanges = false;
				invoice1.HasChanges = false;
				declaration.HasChanges = false;

				Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				Assert(messageText, messageText.Contains("You do not have the appropriate security rights to run this function."));
				Assert(messageText, messageText.Contains("G7 Export Message Send"));

				Env.Security.CAG7MsgSend.IsAllowed = true;
				Assert("License not set", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
				Assert("License not set (" + messageText + ")", messageText.Contains("An Export License Number is not set."));

				var company = GlbCompany.GetCurrentCompany(Factory);
				var companyProxy = company.OrgProxy;
				companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICEN", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));

				entryHeader.CH_EntryStatus = "";
				entryHeader.HasChanges = false;
				declaration.HasChanges = false;
				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				entryHeader.EntryNumber = "XLICEN000000001";
				Assert("Not Exists", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
				Assert("Not Exists(" + messageText + ")", messageText.Contains("the Export Declaration has not been reported yet."));
				entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				declaration.DeriveDeclarationStatus();
				entryHeader.HasChanges = false;
				declaration.HasChanges = false;
				Assert("OK to send Withdraw", manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));

				var importer = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.Importer.CompanyData.OB_AROnCreditHold = true;
				declaration.Importer.CompanyData.OB_IsDebtor = true;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				entryHeader.CH_EntryStatus = ZString.Empty;
				declaration.DeriveDeclarationStatus();
				Factory.Save();

				Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
				AssertEquals("Submit message with credit restriction canceled.", messageText);

				entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
				declaration.DeriveDeclarationStatus();
				Factory.Save();

				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				entryHeader.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
				declaration.DeriveDeclarationStatus();
				Factory.Save();

				Assert("Not OK to send message", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
				AssertEquals("Submit message with credit restriction canceled.", messageText);

				declaration.Importer.CompanyData.OB_IsDebtor = false;
				declaration.Importer.CompanyData.OB_AROnCreditHold = false;
				declaration.HasChanges = false;
				declaration.Importer.HasChanges = false;
				Factory.Save();

				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				entryHeader.EntryNumber = "";
				Assert("Recreate Transaction Number", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));

				entryHeader.EntryNumber = "XLICEN000000001";
				Assert("With Transaction Number", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
				Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			}
		}

		public override void TestGetMessageBuilder()
		{
			AssertEquals("GetMessageBuilder", typeof(G7ExportMessageBuilder), ((G7ExportDeclarationMessageManagerForTesting)messageManager).GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
		}

		public void TestCanSendThisMessage_CheckCompanyOrgProxyExportLicenceNumber()
		{
			var branch = GlbBranch.GetCurrentBranch(Factory);
			branch.Company.GC_Name = "Developer Company";
			branch.GB_BranchName = "Developer Branch";
			var orgProxy = branch.OrgProxy;
			orgProxy.OH_Code = "~ORG~";
			Factory.Save();

			var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.CAG7MsgSend.IsAllowed = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			Factory.Save();
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;

			var manager = new G7ExportDeclarationMessageManagerForTesting(new G7ExportMessageWrapper(declaration.CustomsEntryHeaders[0]), notification);

			ZString messageText;
			Assert("Has Changes", declaration.HasChanges);
			Assert("Has Changes", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert("Has Changes(" + messageText + ")", messageText.Contains("Job not yet saved, Please save before sending."));

			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
			entryLine.HasChanges = false;
			entryHeader.HasChanges = false;
			invoice1Line1.HasChanges = false;
			invoice1.HasChanges = false;
			declaration.HasChanges = false;

			Assert("License not set", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert("License not set (" + messageText + ")", messageText.Contains("An Export License Number is not set."));

			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "X", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			entryHeader.CH_EntryStatus = "";
			entryHeader.HasChanges = false;
			declaration.HasChanges = false;
			Assert("License not valid: Should be 6 bits", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert("License not valid (" + messageText + ")", messageText.Contains("The Export License Number (CAX) on Organization Proxy ~ORG~ is invalid. Please configure it under Organization ~ORG~ > Details > Config > Registration Numbers / Codes."));

			companyProxy.CustomsCodes.DeleteAll();
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "X1234~", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			var exportLicenceNumberInfo = entryHeader.GetType().GetField("exportLicenceNumber", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
			exportLicenceNumberInfo.SetValue(entryHeader, ZString.Empty);
			entryHeader.CH_EntryStatus = "";
			entryHeader.HasChanges = false;
			declaration.HasChanges = false;
			Assert("License not valid: Should be only number and Letters", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			Assert("License not valid (" + messageText + ")", messageText.Contains("The Export License Number (CAX) on Organization Proxy ~ORG~ is invalid. Please configure it under Organization ~ORG~ > Details > Config > Registration Numbers / Codes."));

			companyProxy.CustomsCodes.DeleteAll();
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "X12345", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			exportLicenceNumberInfo.SetValue(entryHeader, ZString.Empty);
			entryHeader.CH_EntryStatus = "";
			entryHeader.HasChanges = false;
			declaration.HasChanges = false;
			Assert("Transaction Number not set", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			entryHeader.EntryNumber = "XLICEN000000001";
			Assert("OK to send message", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			Assert("No error message", messageText.IsEmpty);
		}

		[TestDate(2016, 3, 14)]
		public void TestSetEntrySubmittedDate()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "123456");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			Factory.Save();

			var manager = new G7ExportDeclarationMessageManagerForTesting(new G7ExportMessageWrapper(entryHeader), notification);
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Create, false);
			AssertEquals("JE_EntrySubmittedDate", new ZDateTime(2016, 3, 14), declaration.JE_EntrySubmittedDate);

			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			manager.SendMessage(MessageSubTypes.Amend, false);
			AssertEquals("JE_EntrySubmittedDate", ZDateTime.Empty, declaration.JE_EntrySubmittedDate);
		}

		public void TestAdditionalWarningsMessage_MQWarningMessage()
		{
			var mQwarningText = CAMessageManager.MQWarningMessage;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPP");
				var manager = new G7ExportDeclarationMessageManagerForTesting(new G7ExportMessageWrapper(entryHeader), notification);
				AssertContains("AdditionalWarnings", mQwarningText, manager.GetAdditionalWarningsMessage(MessageSubTypes.Request));

				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "RCCECECPW");
				manager = new G7ExportDeclarationMessageManagerForTesting(new G7ExportMessageWrapper(entryHeader), notification);
				AssertNotContains("AdditionalWarnings", mQwarningText, manager.GetAdditionalWarningsMessage(MessageSubTypes.Request));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var manager = new G7ExportDeclarationMessageManagerForTesting(new G7ExportMessageWrapper(entryHeader), notification);
				AssertNotContains("AdditionalWarnings", mQwarningText, manager.GetAdditionalWarningsMessage(MessageSubTypes.Request));
			}
		}

		#region Implementation

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			return new G7ExportMessageWrapper(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew());
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new G7ExportDeclarationMessageManagerForTesting((IG7Export)dataWrapper, notification);
		}

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			var messageWrapper = new G7ExportMessageWrapper(entryHeader);
			var manager = new G7ExportDeclarationMessageManagerForTesting(messageWrapper, notification);
			AssertEquals(expectedMessage, manager.CanSendThisMessage());
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			Assert(true);
		}

		#region G7ExportDeclarationMessageManagerForTesting

		class G7ExportDeclarationMessageManagerForTesting : G7ExportDeclarationMessageManager
		{
			public G7ExportDeclarationMessageManagerForTesting(IG7Export g7ExportDeclarationWrapper, IUserNotification notification)
				: base(g7ExportDeclarationWrapper, notification)
			{
			}

			public bool CanSendThisMessage_Exposed(MessageSubTypes actionCode, out ZString messageText)
			{
				return CanSendThisMessage(actionCode, out messageText);
			}

			public void PopulateMessage_Exposed(MessageSubTypes actionCode)
			{
				PopulateMessage(actionCode);
			}

			public ZString CanSendThisMessage(MessageSubTypes actionCode)
			{
				var result = ZString.Empty;
				base.CanSendThisMessage(actionCode, out result);
				return result;
			}

			public ZString CanSendThisMessage() => CanSendThisMessage(MessageSubTypes.Create);

			public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode)
			{
				return GetMessageBuilder(actionCode);
			}

			protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
			{
				messageText = string.Empty;
				return OverrideCanSendThisMessage || base.CanSendThisMessage(actionCode, out messageText);
			}

			public new ZString GetAdditionalWarningsMessage(MessageSubTypes actionCode)
			{
				return base.GetAdditionalWarningsMessage(actionCode);
			}

			public bool OverrideCanSendThisMessage { private get; set; }
		}

		#endregion

		#endregion
	}
}
