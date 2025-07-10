using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class RevenueRecognizerTest : TestCaseWithFactory
	{
		public void TestProcessForShipmentWhenRecognitionDateHasError()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge.JR_AL_APLine = line.PK;
			var expectedDate = ZDateTime.Today.AddDays(-11);
			Factory.Save();

			shipment.JS_E_ARV = expectedDate;
			Factory.Save();
			job.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job);
			AssertEquals("Precondition: line should be unrecognized.", ZDateTime.Empty, line.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job.", 0, job.RevenueRecognitionCollection.Count);

			((IProcessor)new RevenueRecognizer(shipment)).Process(new NotificationBuffer());
			AssertEquals("Correct recognition date should not be added.", 0, job.RevenueRecognitionCollection.Count);
			AssertEquals("line should not be recognized.", ZDateTime.Empty, line.AL_ReverseDate);
			AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Email subject", "Revenue recognition errors for Job", Env.OutgoingMailManager.EmailsCreated[0].Subject);

			TestObjectCreator.CreateTestPeriods(expectedDate);
			Factory.Save();
			((IProcessor)new RevenueRecognizer(shipment)).Process(new NotificationBuffer());
			AssertEquals("Correct recognition date should be added.", expectedDate, job.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("Any new emails shouldn't be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessForShipmentWhenJobHasError()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var expectedDate = ZDateTime.Today.AddDays(-11);
			TestObjectCreator.CreateTestPeriods(expectedDate);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge.JR_AL_APLine = line.PK;
			Factory.Save();

			shipment.JS_E_ARV = expectedDate;
			Factory.Save();
			job.RunPreSaveValidation();
			AssertEquals("Precondition: job should contain errors.", true, job.HasErrors);
			AssertEquals("Precondition: line should be unrecognized.", ZDateTime.Empty, line.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job.", 0, job.RevenueRecognitionCollection.Count);

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => ((IProcessor)new RevenueRecognizer(shipment)).Process(new NotificationBuffer()));
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Emails Count", 1, exceptionThrown.Emails.Length);
			AssertContains("Email subject", "Revenue recognition errors for Job", ((RevenueRecognitionEmail)exceptionThrown.Emails[0]).GetSubject_ForTestOnly());

			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			job.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job);
			Factory.Save();
			((IProcessor)new RevenueRecognizer(shipment)).Process(new NotificationBuffer());
			AssertEquals("Correct recognition date should be added.", expectedDate, job.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("Any new emails shouldn't be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessForShipment_JobShouldNotSave()
		{
			var valuesForTest = new RevenueRecognitionCollection();
			var setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var expectedDate = ZDateTime.Today.AddDays(-11);
			TestObjectCreator.CreateTestPeriods(expectedDate);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge.JR_AL_APLine = line.PK;

			Factory.Save();

			shipment.JS_E_ARV = expectedDate;
			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job);

			AssertEquals("Job should not have change before process.", false, job.HasChanges);

			AssertEquals("Precondition: line should be unrecognized.", ZDateTime.Empty, line.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job.", 0, job.RevenueRecognitionCollection.Count);

			((IProcessor)new RevenueRecognizer(shipment)).Process(new NotificationBuffer());
			AssertEquals("Job should have change after process because the process should not save the job.", true, job.HasChanges);
			AssertEquals("Correct recognition date should be added.", expectedDate, job.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate, line.AL_ReverseDate);
			AssertEquals("Any new emails shouldn't be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessForConsol()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var expectedDate1 = ZDateTime.Today.AddDays(-11);
			TestObjectCreator.CreateTestPeriods(expectedDate1);
			var expectedDate2 = ZDateTime.Today.AddMonths(-11);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			consol.Transports[0].JW_ETA = ZDateTime.Empty;
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);

			var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line1 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job1, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge1.JR_AL_APLine = line1.PK;

			job1.RunPreSaveValidation();
			AssertEquals("Precondition: job should contain errors.", true, job1.HasErrors);

			var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var line2 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job2, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge2.JR_AL_APLine = line2.PK;
			Factory.Save();

			shipment1.JS_E_ARV = expectedDate1;
			shipment2.JS_E_ARV = expectedDate2;
			Factory.Save();
			job2.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job2);

			AssertEquals("Precondition: line should be unrecognized.", ZDateTime.Empty, line1.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job.", 0, job1.RevenueRecognitionCollection.Count);

			AssertEquals("Precondition: line should be unrecognized.", ZDateTime.Empty, line2.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job.", 0, job2.RevenueRecognitionCollection.Count);

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => ((IProcessor)new RevenueRecognizer((IJobCostingPlugIn)consol)).Process(new NotificationBuffer()));
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Emails Count", 2, exceptionThrown.Emails.Length);
			AssertContains("Email subject", "Revenue recognition errors for Job", ((RevenueRecognitionEmail)exceptionThrown.Emails[0]).GetSubject_ForTestOnly());
			AssertContains("Email subject", "Revenue recognition errors for Job", ((RevenueRecognitionEmail)exceptionThrown.Emails[1]).GetSubject_ForTestOnly());

			TestObjectCreator.CreateTestPeriods(expectedDate2);
			Factory.Save();
			exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => ((IProcessor)new RevenueRecognizer((IJobCostingPlugIn)consol)).Process(new NotificationBuffer()));
			AssertEquals("Correct recognition date should be added.", expectedDate2, job2.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate2, line2.AL_ReverseDate);
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Emails Count", 1, exceptionThrown.Emails.Length);
			AssertContains("Email subject", "Revenue recognition errors for Job", ((RevenueRecognitionEmail)exceptionThrown.Emails[0]).GetSubject_ForTestOnly());

			job1.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();
			job1.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job1);

			((IProcessor)new RevenueRecognizer((IJobCostingPlugIn)consol)).Process(new NotificationBuffer());
			AssertEquals("Correct recognition date should be added.", expectedDate1, job1.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate1, line1.AL_ReverseDate);

			AssertEquals("Correct recognition date should be added.", expectedDate2, job2.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate2, line2.AL_ReverseDate);
			AssertEquals("Any new emails shouldn't be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessForConsol_JobShouldNotSave()
		{
			var valuesForTest = new RevenueRecognitionCollection();
			var setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var expectedDate1 = ZDateTime.Today.AddDays(-11);
			TestObjectCreator.CreateTestPeriods(expectedDate1);
			var expectedDate2 = ZDateTime.Today.AddMonths(-11);
			TestObjectCreator.CreateTestPeriods(expectedDate2);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			consol.Transports[0].JW_ETA = ZDateTime.Empty;

			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);

			var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line1 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job1, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge1.JR_AL_APLine = line1.PK;

			var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var line2 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job2, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge2.JR_AL_APLine = line2.PK;
			Factory.Save();

			shipment1.JS_E_ARV = expectedDate1;
			shipment2.JS_E_ARV = expectedDate2;
			Factory.Save();

			job1.RunPreSaveValidation();
			AssertNoErrors("Precondition: job1 shouldn't contain errors.", job1);

			job2.RunPreSaveValidation();
			AssertNoErrors("Precondition: job2 shouldn't contain errors.", job2);

			AssertEquals("Job1 should not have change before process.", false, job1.HasChanges);
			AssertEquals("Job2 should not have change before process.", false, job2.HasChanges);

			((IProcessor)new RevenueRecognizer((IJobCostingPlugIn)consol)).Process(new NotificationBuffer());
			AssertEquals("Job1 should have change after process because the process should not save the job.", true, job1.HasChanges);
			AssertEquals("Correct recognition date should be added.", expectedDate1, job1.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate1, line1.AL_ReverseDate);

			AssertEquals("Job2 should have change after process because the process should not save the job.", true, job2.HasChanges);
			AssertEquals("Correct recognition date should be added.", expectedDate2, job2.GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("line should be recognized.", expectedDate2, line2.AL_ReverseDate);
			AssertEquals("Any new emails shouldn't be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessForConsol_RecognitionDateHasError()
		{
			var valuesForTest = new RevenueRecognitionCollection();
			var setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var expectedDate1 = ZDateTime.Today.AddDays(-11);
			var expectedDate2 = ZDateTime.Today.AddMonths(-11);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			consol.Transports[0].JW_ETA = ZDateTime.Empty;

			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);

			var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line1 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job1, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge1.JR_AL_APLine = line1.PK;

			var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var line2 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job2, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			charge2.JR_AL_APLine = line2.PK;
			Factory.Save();

			shipment1.JS_E_ARV = expectedDate1;
			shipment2.JS_E_ARV = expectedDate2;
			Factory.Save();

			job1.RunPreSaveValidation();
			AssertNoErrors("Precondition: job1 shouldn't contain errors.", job1);
			AssertEquals("Precondition: line1 should be unrecognized.", ZDateTime.Empty, line1.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job1.", 0, job1.RevenueRecognitionCollection.Count);

			job2.RunPreSaveValidation();
			AssertNoErrors("Precondition: job2 shouldn't contain errors.", job2);
			AssertEquals("Precondition: line2 should be unrecognized.", ZDateTime.Empty, line2.AL_ReverseDate);
			AssertEquals("Any recognition dates should not be added to a job2.", 0, job2.RevenueRecognitionCollection.Count);

			((IProcessor)new RevenueRecognizer((IJobCostingPlugIn)consol)).Process(new NotificationBuffer());
			AssertEquals("Correct recognition date should not be added.", 0, job1.RevenueRecognitionCollection.Count);
			AssertEquals("line1 should not be recognized.", ZDateTime.Empty, line1.AL_ReverseDate);
			AssertEquals("Correct recognition date should not be added.", 0, job2.RevenueRecognitionCollection.Count);
			AssertEquals("line2 should not be recognized.", ZDateTime.Empty, line2.AL_ReverseDate);

			AssertEquals("Emails should be sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Email subject", "Revenue recognition errors for Job", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertContains("Email subject", "Revenue recognition errors for Job", Env.OutgoingMailManager.EmailsCreated[1].Subject);

			TestObjectCreator.CreateTestPeriods(expectedDate1);
			TestObjectCreator.CreateTestPeriods(expectedDate2);
			Factory.Save();

			((IProcessor)new RevenueRecognizer((IJobCostingPlugIn)consol)).Process(new NotificationBuffer());
			AssertEquals("Correct recognition date should be added.", 1, job1.RevenueRecognitionCollection.Count);
			AssertEquals("line1 should be recognized.", expectedDate1, line1.AL_ReverseDate);
			AssertEquals("Correct recognition date should be added.", 1, job2.RevenueRecognitionCollection.Count);
			AssertEquals("line1 should be recognized.", expectedDate2, line2.AL_ReverseDate);
			AssertEquals("Any new emails shouldn't be sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;

		protected override void SetUp()
		{
			base.SetUp();

			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK));
			currentStaffMember.GS_EmailAddress = new ZString("blahblah@whatever.example");
			GlbGroup group = staffMemberFactory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);
			staffMemberFactory.Save();

			AccountingConfigurationRegistry.Instance.RevenueRecognitionNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}
	}
}
