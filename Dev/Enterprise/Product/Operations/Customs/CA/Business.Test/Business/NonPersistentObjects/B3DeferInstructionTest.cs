using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3DeferInstruction))]
	sealed class B3DeferInstructionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestB3DeferInstructionConstructor()
		{
			var instruction = new B3DeferInstruction(Factory);
			AssertEquals(DeferredB3SendActionList.Codes.Now, instruction.DeferActionCode);
		}

		[TestDate(2015, 5, 01, 12, 6, 25)]
		public void TestMessageForNotificationLabel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();
			CombineAssertions("test for IMP declaration", () =>
			{
				var testDate = new ZDate(2015, 05, 01);
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				testEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
				testEntryLine.CL_CustomsValue = 50000;
				testDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 06, 01);
				var expectMessage = @"The due date for accounting of this job appears to fall after the Monthly Statement cut-off date.
Do you want to delay sending the Entry message?

Release Date:           01-Jun-15
Entry Due Date:            08-Jun-15
Statement cut-off Date: 01-May-15

If you choose to generate Entry message and schedule the transmission for a future date then the Entry message will be generated now, using the current status of the declaration, and scheduled for sending to Customs at the start of the next accounting period (you may override this scheduled date). Note that if subsequent changes are made in this declaration that would affect the data to be sent to Customs, then the new message will have to be generated prior to the submission date.";
				var testInstruction = new B3DeferInstruction(testDeclaration, testDate);
				AssertEquals("Message for normal declaration", expectMessage, testInstruction.MessageForNotificationLabel.Caption);
			});
			CombineAssertions("test for IMP - LVS declaration", () =>
			{
				var testDate = new ZDate(2015, 05, 01);
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var invoiceHeader = testDeclaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 1m;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1m;
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				testEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
				Factory.Save();
				testDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 06, 01);
				var expectMessage = @"The due date for accounting of this Low Value job appears to fall after the Monthly Statement cut-off date.
Do you want to delay sending the Entry message?

Release Date:           01-Jun-15
Entry Due Date:            08-Jun-15
Statement cut-off Date: 01-May-15

If you choose to generate Entry message and schedule the transmission for a future date then the Entry message will be generated now, using the current status of the declaration, and scheduled for sending to Customs at the start of the next accounting period (you may override this scheduled date). Note that if subsequent changes are made in this declaration that would affect the data to be sent to Customs, then the new message will have to be generated prior to the submission date.";
				var testInstruction = new B3DeferInstruction(testDeclaration, testDate);
				AssertEquals("Message for normal declaration", expectMessage, testInstruction.MessageForNotificationLabel.Caption);
			});
			CombineAssertions("test for LVS Declaration", () =>
			{
				var testDate = new ZDate(2015, 05, 01);
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				testEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
				testEntryLine.CL_CustomsValue = 1;
				testDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 06, 01);
				var expectMessage = @"The due date for accounting of this consolidated LVS job appears to fall after the Monthly Statement cut-off date.
Do you want to delay sending the Entry message?

LVS Period:             Jun-15
Entry Due Date:            24-Jul-15
Statement cut-off Date: 01-May-15

If you choose to generate Entry message and schedule the transmission for a future date then the Entry message will be generated now, using the current status of the declaration, and scheduled for sending to Customs at the start of the next accounting period (you may override this scheduled date). Note that if subsequent changes are made in this declaration that would affect the data to be sent to Customs, then the new message will have to be generated prior to the submission date.";
				var testInstruction = new B3DeferInstruction(testDeclaration, testDate);
				AssertEquals("Message for LVS declaration", expectMessage, testInstruction.MessageForNotificationLabel.Caption.Trim());
			});
		}

		public void TestDefaultValues()
		{
			var testDate = new ZDate(2015, 05, 01);
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 06, 01);
			var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
			testEntryLine.CL_CustomsValue = 1;
			var cutOffDate = testDeclaration.K84CutOffDate;
			var reftime = cutOffDate.AddDays(25 - cutOffDate.Day);

			CombineAssertions(() =>
			{
				var testInstruction = new B3DeferInstruction(testDeclaration, testDate);
				AssertEquals("deferInstructionCode", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
				AssertEquals("ScheduleTimeZone", B3ScheduleTimeZoneQualifierList.Codes.EST, testInstruction.ScheduleTimeZone);
				AssertEquals("HeldUntilDateTime", reftime.AddHours(4), testInstruction.HeldUntilDateTime);
				AssertEquals("HeldUntilDateTimeUTC", reftime.AddHours(IsOttawaDaylightSavingNow(testInstruction.HeldUntilDateTime.ToDateTime()) ? 8 : 9), testInstruction.heldUntilDateTimeUTC);
			});
		}

		ZBool IsOttawaDaylightSavingNow(DateTime time)
		{
			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CAOTT");
			var unlocoTimeZone = unloco.TimeZoneSet.GetCalculationTimeZone();
			return unlocoTimeZone.IsDaylightSavingBasedOnUtc(time);
		}

		public void TestGetDefaultDeferredActionCodeFromImporter_EmptyImporter()
		{
			B3DeferInstruction testInstruction;
			var registryInstance = CACustomsDataRegistry.Instance;

			var testK84Date = new ZDate(2015, 01, 01);
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var companyPK = testDeclaration.CompanyPK.ToGuid();
			var branchPK = testDeclaration.JE_GB.ToGuid();

			CombineAssertions("Pre-Check", () =>
			{
				AssertEquals("Registry Default Value - Normal", DeferredB3SendActionList.Codes.Defer, registryInstance.DefaultDeferredNormalEntrySendAction.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty));
				AssertEquals("Registry Default Value - LowValue", DeferredB3SendActionList.Codes.Defer, registryInstance.DefaultDeferredLowValueEntrySendAction.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty));
				AssertNull(testDeclaration.ImporterAddInfo);
			});

			testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
			AssertEquals("Default - Default to Defer in Registry Default", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
			AssertNotEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
			AssertNotEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

			registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
			testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
			AssertEquals("Default - reflect registry setting", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
			AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
			AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

			registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
			registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
			testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
			AssertEquals("Default - reflect registry setting", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
			AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
			AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

			registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
			registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
			registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Now);
			testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
			AssertEquals("Default - reflect registry setting", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
			AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
			AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
			AssertEquals("LVS - Default to Defer in Registry Default", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
			AssertNotEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
			AssertNotEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

			registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
			testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
			AssertEquals("LVS - reflect registry setting", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
			AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
			AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

			registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
			registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
			testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
			AssertEquals("LVS - reflect registry setting", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
			AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
			AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

			registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
			registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
			registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Now);
			testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
			AssertEquals("LVS - reflect registry setting", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
			AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
			AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);
		}

		public void TestGetDefaultDeferredActionCodeFromImporter_LowValue_1()
		{
			B3DeferInstruction testInstruction;
			var registryInstance = CACustomsDataRegistry.Instance;
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			var testImporterOrgImpAddInfo = OrgImpAddInfo.Get(testImporter);

			var testK84Date = new ZDate(2015, 01, 01);
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testDeclaration.JE_OH_Importer = testImporter.PK;

			var companyPK = testDeclaration.CompanyPK.ToGuid();
			var branchPK = testDeclaration.JE_GB.ToGuid();

			CombineAssertions("Pre-Check", () =>
			{
				AssertEquals("Registry Default Value - Normal", DeferredB3SendActionList.Codes.Defer, registryInstance.DefaultDeferredNormalEntrySendAction.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty));
				AssertEquals("Registry Default Value - LowValue", DeferredB3SendActionList.Codes.Defer, registryInstance.DefaultDeferredLowValueEntrySendAction.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty));
				AssertEquals("Organisation Default - Normal", DeferredB3SendActionListOverride.Codes.RegistryDefault, testDeclaration.ImporterAddInfo.ZO_DeferredNormalB3SendAction);
				AssertEquals("Organisation Default - LowValue", DeferredB3SendActionListOverride.Codes.RegistryDefault, testDeclaration.ImporterAddInfo.ZO_DeferredLowValueB3SendAction);
			});

			CombineAssertions("OrgAddInfo Empty", () =>
			{
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Default", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
				AssertEquals("EST", testInstruction.ScheduleTimeZone);
				AssertNotEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);
			});
		}

		public void TestGetDefaultDeferredActionCodeFromImporter_LowValue_2()
		{
			B3DeferInstruction testInstruction;
			var registryInstance = CACustomsDataRegistry.Instance;
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			var testImporterOrgImpAddInfo = OrgImpAddInfo.Get(testImporter);

			var testK84Date = new ZDate(2015, 01, 01);
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testDeclaration.JE_OH_Importer = testImporter.PK;

			var companyPK = testDeclaration.CompanyPK.ToGuid();
			var branchPK = testDeclaration.JE_GB.ToGuid();

			CombineAssertions("OrgAddInfo RegDefault", () =>
			{
				testImporterOrgImpAddInfo.ZO_DeferredLowValueB3SendAction = DeferredB3SendActionListOverride.Codes.RegistryDefault;
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Default", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
				AssertNotEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertNotEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredLowValueEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);
			});

			CombineAssertions("OrgAddInfo Value Set", () =>
			{
				testImporterOrgImpAddInfo.ZO_DeferredLowValueB3SendAction = DeferredB3SendActionListOverride.Codes.Now;
				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Default", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);
			});
		}

		public void TestGetDefaultDeferredActionCodeFromImporter_Normal_1()
		{
			B3DeferInstruction testInstruction;
			var registryInstance = CACustomsDataRegistry.Instance;
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			var testImporterOrgImpAddInfo = OrgImpAddInfo.Get(testImporter);

			var testK84Date = new ZDate(2015, 01, 01);
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDeclaration.JE_OH_Importer = testImporter.PK;

			var companyPK = testDeclaration.CompanyPK.ToGuid();
			var branchPK = testDeclaration.JE_GB.ToGuid();

			CombineAssertions("Pre-Check", () =>
			{
				AssertEquals("Registry Default Value - Normal", DeferredB3SendActionList.Codes.Defer, registryInstance.DefaultDeferredNormalEntrySendAction.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty));
				AssertEquals("Organisation Default - Normal", DeferredB3SendActionListOverride.Codes.RegistryDefault, testDeclaration.ImporterAddInfo.ZO_DeferredNormalB3SendAction);
				AssertEquals("Organisation Default - LowValue", DeferredB3SendActionListOverride.Codes.RegistryDefault, testDeclaration.ImporterAddInfo.ZO_DeferredLowValueB3SendAction);
			});

			CombineAssertions("OrgAddInfo Empty", () =>
			{
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Default", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
				AssertNotEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertNotEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);
			});
		}

		public void TestGetDefaultDeferredActionCodeFromImporter_Normal_2()
		{
			B3DeferInstruction testInstruction;
			var registryInstance = CACustomsDataRegistry.Instance;
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			var testImporterOrgImpAddInfo = OrgImpAddInfo.Get(testImporter);

			var testK84Date = new ZDate(2015, 01, 01);
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDeclaration.JE_OH_Importer = testImporter.PK;

			var companyPK = testDeclaration.CompanyPK.ToGuid();
			var branchPK = testDeclaration.JE_GB.ToGuid();

			CombineAssertions("OrgAddInfo RegDefault", () =>
			{
				testImporterOrgImpAddInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.RegistryDefault;
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Default", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
				AssertNotEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertNotEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(companyPK, Guid.Empty, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);
			});

			CombineAssertions("OrgAddInfo Value Set", () =>
			{
				testImporterOrgImpAddInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.Now;
				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Defer);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Default", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);

				registryInstance.DefaultDeferredNormalEntrySendAction.SetValue(Guid.Empty, branchPK, Guid.Empty, DeferredB3SendActionList.Codes.Now);
				testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				AssertEquals("reading registry Value", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals(ZString.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals(ZDateTime.Empty, testInstruction.HeldUntilDateTime);
			});
		}

		[TestDate(2015, 5, 01, 12, 6, 25)]
		public void TestHeldUntilDateTime_ReadOnly()
		{
			var testDate = new ZDate(2015, 05, 01);
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 06, 01);
			var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
			testEntryLine.CL_CustomsValue = 1;

			CombineAssertions(() =>
			{
				var testInstruction = new B3DeferInstruction(testDeclaration, testDate);
				AssertEquals("Default - deferInstructionCode", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
				AssertEquals("Default - ScheduleTimeZone", B3ScheduleTimeZoneQualifierList.Codes.EST, testInstruction.ScheduleTimeZone);
				AssertEquals("Default - HeldUntilDateTime", new ZDateTime(2015, 05, 25, 04, 00, 00), testInstruction.HeldUntilDateTime);
				AssertEquals("Default - ScheduleTimeZone_Readonly", false, testInstruction.ScheduleTimeZoneInfo.ReadOnly);
				AssertEquals("Default - HeldUntilDateTime_ReadOnly", false, testInstruction.HeldUntilDateTimeInfo.ReadOnly);

				testInstruction.DeferActionCode = DeferredB3SendActionList.Codes.Now;
				AssertEquals("Change to NOW - deferInstructionCode", DeferredB3SendActionList.Codes.Now, testInstruction.DeferActionCode);
				AssertEquals("Change to NOW - ScheduleTimeZone", string.Empty, testInstruction.ScheduleTimeZone);
				AssertEquals("Change to NOW - HeldUntilDateTime", ZDateTime.Empty, testInstruction.HeldUntilDateTime);
				AssertEquals("Change to NOW - ScheduleTimeZone_Readonly", true, testInstruction.ScheduleTimeZoneInfo.ReadOnly);
				AssertEquals("Change to NOW - HeldUntilDateTime_ReadOnly", true, testInstruction.HeldUntilDateTimeInfo.ReadOnly);

				testInstruction.DeferActionCode = DeferredB3SendActionList.Codes.Defer;
				AssertEquals("Change to DLY - deferInstructionCode", DeferredB3SendActionList.Codes.Defer, testInstruction.DeferActionCode);
				AssertEquals("Change to DLY - ScheduleTimeZone", B3ScheduleTimeZoneQualifierList.Codes.EST, testInstruction.ScheduleTimeZone);
				AssertEquals("Change to DLY - HeldUntilDateTime", new ZDateTime(2015, 05, 25, 04, 00, 00), testInstruction.HeldUntilDateTime);
				AssertEquals("Change to DLY - ScheduleTimeZone_Readonly", false, testInstruction.ScheduleTimeZoneInfo.ReadOnly);
				AssertEquals("Change to DLY - HeldUntilDateTime_ReadOnly", false, testInstruction.HeldUntilDateTimeInfo.ReadOnly);
			});
		}

		public void TestValidateDelayInstructionCode()
		{
			var testInstruction = GetNewBusinessObject() as B3DeferInstruction;
			AssertNoNotifications("Test_1", testInstruction.DeferActionCodeInfo);

			testInstruction.DeferActionCode = DeferredB3SendActionList.Codes.Now;
			AssertNoNotifications("Test_2", testInstruction.DeferActionCodeInfo);

			testInstruction.DeferActionCode = string.Empty;
			AssertHasErrorContaining(testInstruction.DeferActionCodeInfo, "Please enter a value.");

			testInstruction.DeferActionCode = "XXX";
			AssertHasErrorContaining(testInstruction.DeferActionCodeInfo, "Enter a valid selection.");
		}

		public void TestValidateScheduleTimeZone()
		{
			var testInstruction = GetNewBusinessObject() as B3DeferInstruction;
			AssertNoNotifications("Test_1", testInstruction.ScheduleTimeZoneInfo);

			testInstruction.ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.EST;
			AssertNoNotifications("Test_2", testInstruction.ScheduleTimeZoneInfo);

			testInstruction.ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.UTC;
			AssertNoNotifications("Test_3", testInstruction.ScheduleTimeZoneInfo);

			testInstruction.ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.Local;
			AssertNoNotifications("Test_4", testInstruction.ScheduleTimeZoneInfo);

			testInstruction.ScheduleTimeZone = "XXX";
			AssertHasErrorContaining(testInstruction.ScheduleTimeZoneInfo, "Enter a valid selection.");

			testInstruction.ScheduleTimeZone = string.Empty;
			AssertHasErrorContaining(testInstruction.ScheduleTimeZoneInfo, "Please enter a value.");

			testInstruction.DeferActionCode = DeferredB3SendActionList.Codes.Now;
			AssertNoNotifications("Test_4", testInstruction.ScheduleTimeZoneInfo);
		}

		[TestDate(2015, 05, 01)]
		public void TestValidateHeldUntilDateTime()
		{
			var testInstruction = GetNewBusinessObject() as B3DeferInstruction;
			AssertNoNotifications("Test_1", testInstruction.HeldUntilDateTimeInfo);

			testInstruction.HeldUntilDateTime = new ZDateTime(2015, 06, 01);
			AssertNoNotifications("Test_2", testInstruction.HeldUntilDateTimeInfo);

			testInstruction.HeldUntilDateTime = new ZDateTime(2015, 04, 29);
			AssertHasErrorContaining(testInstruction.HeldUntilDateTimeInfo, "The past date is not allowed for scheduling, please enter future date.");

			testInstruction.HeldUntilDateTime = ZDate.Empty;
			AssertHasErrorContaining(testInstruction.HeldUntilDateTimeInfo, "Please enter a value.");

			testInstruction.DeferActionCode = DeferredB3SendActionList.Codes.Now;
			AssertNoNotifications("Test_3", testInstruction.HeldUntilDateTimeInfo);
		}

		public void TestTimeZoneSwtichingChangeDisplayTime()
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = testCompany.Branches.AddNew();
			testBranch.FillWithValidTestData();
			testBranch.GB_RL_NKHomePort = "CNSHA";
			Factory.Save();
			using (Enterprise.Environment.DisposableEnvironment.ForBranch(testBranch.PK.ToGuid()))
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testK84Date = new ZDate(2015, 01, 01);
				var testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);
				var cutOffDate = testDeclaration.K84CutOffDate;
				var reftime = cutOffDate.AddDays(25 - cutOffDate.Day);
				var isDst = IsOttawaDaylightSavingNow(reftime.ToDateTime());
				var hoursOffset = isDst ? 8 : 9;
				AssertEquals(B3ScheduleTimeZoneQualifierList.Codes.EST, testInstruction.ScheduleTimeZone);
				AssertEquals(reftime.AddHours(4), testInstruction.HeldUntilDateTime);
				AssertEquals(reftime.AddHours(hoursOffset), testInstruction.heldUntilDateTimeUTC);

				testInstruction.ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.UTC;
				AssertEquals(reftime.AddHours(hoursOffset), testInstruction.HeldUntilDateTime);
				AssertEquals(reftime.AddHours(hoursOffset), testInstruction.heldUntilDateTimeUTC);

				testInstruction.ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.Local;
				AssertEquals(reftime.AddHours(isDst ? 16 : 17), testInstruction.HeldUntilDateTime);
				AssertEquals(reftime.AddHours(hoursOffset), testInstruction.heldUntilDateTimeUTC);

				testInstruction.ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.EST;
				testInstruction.HeldUntilDateTime = new ZDateTime(2015, 05, 23, 20, 00, 00);
				AssertEquals(new ZDateTime(2015, 05, 24, 00, 00, 00), testInstruction.heldUntilDateTimeUTC);

				testInstruction.ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.UTC;
				testInstruction.HeldUntilDateTime = new ZDateTime(2015, 05, 23, 20, 00, 00);
				AssertEquals(new ZDateTime(2015, 05, 23, 20, 00, 00), testInstruction.heldUntilDateTimeUTC);

				testInstruction.ScheduleTimeZone = B3ScheduleTimeZoneQualifierList.Codes.Local;
				testInstruction.HeldUntilDateTime = new ZDateTime(2015, 05, 23, 20, 00, 00);
				AssertEquals(new ZDateTime(2015, 05, 23, 12, 00, 00), testInstruction.heldUntilDateTimeUTC);
			}
		}

		public void TestDifferentActionList()
		{
			CombineAssertions("Normal Scheduling with 2 Actions", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testK84Date = new ZDate(2015, 01, 01);
				var testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);

				AssertEquals(2, testInstruction.DeferActionCodeList.Count);
				Assert("Containing Defer", testInstruction.DeferActionCodeList.ContainsCode(DeferredB3SendActionList.Codes.Defer));
				Assert("Containing Now", testInstruction.DeferActionCodeList.ContainsCode(DeferredB3SendActionList.Codes.Now));
			});

			CombineAssertions("existing deferred with 3 Actions", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var testEntryHeader = testDeclaration.ActiveEntryHeaders.AddNew();
				testEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testEntryHeader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = "QUE";
				testMessage.EM_ReceiveTransmit = "TRX";
				var existingScheduledTime = DateTime.UtcNow.AddDays(2);
				testMessage.EM_HeldUntilDate = existingScheduledTime;
				testMessage.EM_IsActive = true;
				Factory.Save();

				var testK84Date = new ZDate(2015, 01, 01);
				var testInstruction = new B3DeferInstruction(testDeclaration, testK84Date);

				AssertEquals(3, testInstruction.DeferActionCodeList.Count);
				Assert("Containing Defer", testInstruction.DeferActionCodeList.ContainsCode(DeferredB3SendActionList.Codes.Defer));
				Assert("Containing Now", testInstruction.DeferActionCodeList.ContainsCode(DeferredB3SendActionList.Codes.Now));
				Assert("Containing Cancel", testInstruction.DeferActionCodeList.ContainsCode(DeferredB3SendActionListWithCancel.Codes.Cancel));
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testK84Date = new ZDate(2015, 01, 01);
			return new B3DeferInstruction(testDeclaration, testK84Date);
		}
	}
}
