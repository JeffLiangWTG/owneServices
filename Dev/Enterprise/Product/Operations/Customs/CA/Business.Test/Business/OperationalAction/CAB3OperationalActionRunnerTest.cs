using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.OperationalAction.Testing
{
	sealed class CAB3OperationalActionRunnerTest : TestCaseWithFactory
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		[TestDate(2014, 12, 21)]
		public void TestPerformFunctionOperationalAction()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, new ZDateTime(2014, 12, 21), true))
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreatePreferenceForCountry("02", "General Tariff", Core.Constants.CountryCodes.Canada);
				helper.CreatePreferenceForCountry("03", "Most-Favored-Nation Tariff", Core.Constants.CountryCodes.Canada);
				var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
				customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Enterprise.Customs.CA.Registry.CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "test");
				var testOperationalActionLog = new DummyOperationalActionSectionLog();
				var testUserNotification = new TestMessageInstructionUserNotification();
				var logAndNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(testUserNotification, testOperationalActionLog, false, false, false);

				var testRunner = new CAB3OperationalActionRunner(logAndNotificationWrapper, new SendsMessagesToCustomsShutterUpperer(), true);

				var testDeclaration_IMP = Factory.New<JobDeclaration>();
				var testDeclaration_IMP_NoEntry = Factory.New<JobDeclaration>();
				var testDeclaration_IMP_NoB3CEntry = Factory.New<JobDeclaration>();
				var testDeclaration_IMP_DoMerge = Factory.New<JobDeclaration>();
				var testDeclaration_EXP = Factory.New<JobDeclaration>();
				var testDeclaration_LVS = Factory.New<JobDeclaration>();
				var testDeclaration_LVS_RequiresMerge = Factory.New<JobDeclaration>();

				testDeclaration_IMP.JE_MessageType = JobMessageTypeList.Codes.Import;
				testDeclaration_IMP.CA_ServiceOption = ZString.Empty;
				testDeclaration_IMP_NoEntry.JE_MessageType = JobMessageTypeList.Codes.Import;
				testDeclaration_IMP_DoMerge.JE_MessageType = JobMessageTypeList.Codes.Import;
				var header = testDeclaration_IMP_DoMerge.Invoices.AddNew();
				header.InvoiceLines.AddNew();
				testDeclaration_EXP.JE_MessageType = JobMessageTypeList.Codes.Export;
				testDeclaration_LVS.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				testDeclaration_LVS.JE_EntryAuthorisationDate = ZDateTime.Now;
				testDeclaration_LVS_RequiresMerge.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;

				testDeclaration_IMP.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				testDeclaration_IMP.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.EDIRelease;
				testDeclaration_IMP_NoB3CEntry.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.EDIRelease;
				testDeclaration_LVS.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				testDeclaration_LVS_RequiresMerge.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				testDeclaration_LVS_RequiresMerge.CA_RequiresMerge = true;

				Factory.Save();

				var ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(0, ediMessageCollection.Length);

				var entryNumber = testDeclaration_IMP.B3EntryHeader.EntryNumber;
				Factory.Save();

				testRunner.PerformFunctionOperationalAction(new BusinessObject[]
				{
				testDeclaration_IMP
				, testDeclaration_IMP_NoEntry
				, testDeclaration_IMP_NoB3CEntry
				, testDeclaration_IMP_DoMerge
				, testDeclaration_LVS_RequiresMerge
				, testDeclaration_EXP
				, testDeclaration_LVS
				});

				CombineAssertions(() =>
				{
					ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
					AssertEquals("Message after runner", 3, ediMessageCollection.Length);
					Assert("message for IMP", ediMessageCollection.Any(msg => msg.EM_LinkUniqueID == testDeclaration_IMP.GetEntryHeaderFor(MessageTypeList.Codes.CommercialAccountingDeclaration).PK));
					Assert("message for IMP", ediMessageCollection.Any(msg => msg.EM_LinkUniqueID == testDeclaration_IMP_DoMerge.GetEntryHeaderFor(MessageTypeList.Codes.CommercialAccountingDeclaration).PK));
					Assert("message for LVS", ediMessageCollection.Any(msg => msg.EM_LinkUniqueID == testDeclaration_LVS.GetEntryHeaderFor(MessageTypeList.Codes.CommercialAccountingDeclaration).PK));

					var expectlog = string.Format(@$"INFO: ----------------------------------------
WARNING: Can't merge [HL B00001000]. Merge failed reason : You can't merge this entry because there are no invoice headers.
INFO: Processing [HL B00001000]
INFO: Sending message for [HL B00001000]
INFO: [Awaiting User's Confirmation]:It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Bond Type: Importer is not marked as being on the CARM portal under Config > Canada > Accounting Declaration > Bond Details
Carrier Code: At least one CCN must be entered on the 'Numbers' tab, or on the 'Cargo Control Numbers' grid on the 'Packing' tab.
Customs Port of Clearance: You have not entered a Port Of Clearance.
Importer: You have not entered an Importer.
Total Weight: Total Weight cannot be zero.
Mode of Transportation: You have not entered a Mode of Transportation.

Do you want to send the message(s) despite these errors?

This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.
INFO: [Awaiting User's Confirmation]:The following unusual situations have been detected. Please read the following carefully.
The Release Status indicates that this shipment has not yet been cleared.
Are you sure you wish to continue and send the {{0}} message at this time?
INFO: [User's Answer]:Continue sending despite of rationality warnings.
INFO: Original {{0}} Message for Declaration B00001000 has been generated.
INFO: [Successful]
INFO: ----------------------------------------
WARNING: Can't merge [HL B00001001]. Merge failed reason : You can't merge this entry because there are no invoice headers.
INFO: Processing [HL B00001001]
ERROR: No Entries exist for this declaration, please ensure at least one Invoice Header and Line have been entered and select 'Generate Entries' from the brokerage menu.
INFO: ----------------------------------------
WARNING: Can't merge [HL B00001002]. Merge failed reason : You can't merge this entry because there are no invoice headers.
INFO: Processing [HL B00001002]
ERROR: The Declaration is not eligible to send Entry Message since it's not a Import Job or LVS Job.
INFO: ----------------------------------------
INFO: Successfully merge entry for [HL B00001003]
INFO: Processing [HL B00001003]
INFO: Sending message for [HL B00001003]
INFO: [Awaiting User's Confirmation]:This job needs Duty and Tax recalculated. Would you like the system to recalculate now using the most recent data?
INFO: [User's Answer]:Yes
INFO: Duty and Tax has been recalculated. These new values will be sent in the {{0}} message.
INFO: [Awaiting User's Confirmation]:It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Authority Number: The total Value for Duty of this job is less than the minimum but you have not entered an OIC regular remission.
C Vfor Curr Conv: value cannot be zero.
Page Number: You have not entered a value.
Value For Duty Code: You have not entered a value.
Goods Origin: You have not entered a Country/Region of Origin.
Goods Description: You have not entered a Goods Description.
Tariff: You have not entered a Classification Tariff code.
Export: You have not entered a value.
Incoterm: Please enter an Incoterm.
Inv. Total Amount: You have not entered an Inv. Total Amount.
Invoice No.: You have not entered an Invoice No..
Curr | Invoice Currency: You have not entered a Curr | Invoice Currency.
Valuation Date Override: You have not entered a Direct Shipment Date, which is required when no ATD is entered on the Declaration tab..
Bond Type: Importer is not marked as being on the CARM portal under Config > Canada > Accounting Declaration > Bond Details
Exam Location Code: You must enter either a exam location code or a text description.
Importer Documentary Address: Organization: The Importer Documentary Address is not in Canada, and you have not specified a Canadian Purchaser or Consignee on the Invoice Header tab.
Effective CCN: You have not entered a value.
Packages Actual Package Count: No packages have been associated with a bill of lading, which is required for IID filings
Carrier Code: At least one CCN must be entered on the 'Numbers' tab, or on the 'Cargo Control Numbers' grid on the 'Packing' tab.
Customs Port of Clearance: You have not entered a Port Of Clearance.
Arrival Date at First Port of Arrival: You have not entered an Arrival Date at First Port of Arrival.
Importer: You have not entered an Importer.
Importer: The Importer Documentary Address is not in Canada, and you have not specified a Canadian Purchaser or Consignee on the Invoice Header tab.
Type: The code you have selected is not in the list.
Total Weight: Total Weight cannot be zero.
Mode of Transportation: You have not entered a Mode of Transportation.

Do you want to send the message(s) despite these errors?

This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.
INFO: [Awaiting User's Confirmation]:The following unusual situations have been detected. Please read the following carefully.
One, or more, invoice lines do not have any GST details. It is recommended that you do NOT continue. You should run 'Generate Entries (Merge)' from the Brokerage menu and then check the GST details on lines.
The Release Status indicates that this shipment has not yet been cleared.
Are you sure you wish to continue and send the {{0}} message at this time?
INFO: [User's Answer]:Continue sending despite of rationality warnings.
INFO: Original {{0}} Message for Declaration B00001003 has been generated.
INFO: [Successful]
INFO: ----------------------------------------
WARNING: Can't merge [HL B00001006]. Merge failed reason : You can't merge this entry because there are no invoice headers.
INFO: Processing [HL B00001006]
ERROR: Required entry doesn't exist, or it requires merge. Please run the option to Generate Entries from the brokerage menu, then save and try again. Entry type: CAD, Declaration job reference: B00001006
INFO: ----------------------------------------
WARNING: Can't merge [HL B00001004]. Merge failed reason : You can't merge this entry because there are no invoice headers.
INFO: Processing [HL B00001004]
ERROR: The Declaration is not eligible to send Entry Message since it's not a Import Job or LVS Job.
INFO: ----------------------------------------
WARNING: Can't merge [HL B00001005]. Merge failed reason : You can't merge this entry because there are no invoice headers.
INFO: Processing [HL B00001005]
INFO: Sending message for [HL B00001005]
INFO: [Awaiting User's Confirmation]:It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Bond Type: Importer is not marked as being on the CARM portal under Config > Canada > Accounting Declaration > Bond Details
Customs Port of Clearance: You have not entered a Reporting Port.
Declaration Type: The Proxy Organization of the branch of the Customs Broker specified on this job, or the current login branch, must have a Business Number for Low Value Shipments (BRL) defined. Please go to Config > Registration Numbers/Codes of the Organization specified on the appropriate branch.

Do you want to send the message(s) despite these errors?

This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs.
INFO: [Awaiting User's Confirmation]:The following unusual situations have been detected. Please read the following carefully.
The due date for accounting of this job (released on 21-Dec-14) appears to fall after the Monthly Statement cut-off date of 24-Dec-14, based on the system settings the Entry message will be deferred until the next pay period.
Are you sure you wish to continue and schedule the {{0}} message at this time?
INFO: [User's Answer]:Continue sending despite of rationality warnings.
INFO: Original {{0}} Message for Declaration B00001005 has been generated.
INFO: [Successful]
", "CAD");
					AssertMultilineASCIIEquals(expectlog, testOperationalActionLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
				});
			}
		}

		public void TestPerformSendB3MessageWhenServiceProviderInterfaceIsConfiguredInRegistry()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
			{
				var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
				customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "test");
				var log = new DummyOperationalActionSectionLog();
				var testUserNotification = new TestMessageInstructionUserNotification();
				var logAndNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(testUserNotification, log, false, false, false);

				var runner = new CAB3OperationalActionRunner(logAndNotificationWrapper, new SendsMessagesToCustomsShutterUpperer(), true);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ZString.Empty;
				declaration.JE_DeclarationReference = "B000001";
				Factory.Save();

				var messages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(0, messages.Length);

				runner.PerformFunctionOperationalAction(new BusinessObject[] { declaration });

				CombineAssertions(() =>
				{
					messages = Factory.Load<EDIMessage>(new ZQuery());
					AssertEquals("Message after runner", 0, messages.Length);
					var expectlog = @"INFO: ----------------------------------------
INFO: Successfully merge entry for [HL B000001]
INFO: Processing [HL B000001]
ERROR: The Declaration is not eligible to send Entry message because this job is configured to submit through a designated service provider interface.";
					AssertMultilineASCIIEquals(expectlog, log.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
				});
			}
		}
	}
}
