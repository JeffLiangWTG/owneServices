using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class ElectronicMessagingProcessingServiceTaskTest<T> : ServiceTaskTestCase<T> where T : ElectronicMessagingProcessingServiceTask
	{
		protected abstract T GetCountrySpecificServiceTask();

		public virtual void TestDataProviderType()
		{
			var serviceTask = GetCountrySpecificServiceTask();
			AssertType<ElectronicMessagingProcessingServiceTaskDataProvider>(serviceTask.DataProvider);
		}

		[TestDate(2018, 2, 1)]
		public virtual void TestSuccessfulCreationOfEDIInterchange()
		{
			var company1 = Helper.CreateCompanyAndBranch("MN1", "BR1", CountryCode, true);

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var batch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, company1);
					var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.AUD, 1m, Helper.ObjectCreator.AALSHI);
					Helper.ObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: Helper.ObjectCreator.GST1);
					var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Queued);
					Factory.Save();
				}

				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				CombineAssertions(logger.ToString(), new VoidParameterlessDelegate(() =>
				{
					var getInboxInvoiceListIdentifier = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(company1.Country.Code)?.ApTransactionListRequestBatchId;
					var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company1.PK, EInvoicingBatchState.Sent);
					if (string.IsNullOrEmpty(getInboxInvoiceListIdentifier))
					{
						AssertEquals(1, batches.Length);
					}
					else
					{
						AssertEquals(2, batches.Length);
						AssertNotNull(batches.Where(x => x.AIB_GovernmentAllocatedNumber == getInboxInvoiceListIdentifier).FirstOrDefault());
					}
					AssertEDIInterchanges(new ZQuery(), string.IsNullOrEmpty(getInboxInvoiceListIdentifier) ? 1 : 2, (interchangePK) => { AssertEDIMessages(interchangePK, company1.FirstActiveBranch.PK, GlbDepartment.CurrentDepartment.PK, GetLinkedObjectIDs(batches)); });
				}));
			}
		}

		[TestDate(2018, 2, 1)]
		public void TestEDIInterchangeIsNotCreatedWhenThereIsNoEnabledCompany()
		{
			var company1 = Helper.CreateCompanyAndBranch("~" + CountryCode, "BR1", CountryCode, true);
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var serviceTask = GetCountrySpecificServiceTask();

				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				var log = logger.ToString();
				AssertContains($"Debug|{serviceTask.TaskName} service task started.", log);
				AssertContains($"Debug|No {serviceTask.CountryCode} company has E-Reporting enabled in registry. {serviceTask.TaskName} service task completed early.", log);

				CombineAssertions(logger.ToString(), new VoidParameterlessDelegate(() => AssertEDIInterchanges(new ZQuery(), 0, null)));
			}
		}

		[TestDate(2018, 2, 1)]
		public void TestEDIInterchangeIsNotCreatedWhenThereIsNoQueuedPivot()
		{
			var company1 = Helper.CreateCompanyAndBranch("~" + CountryCode, "BR1", CountryCode, true);
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = GetCountrySpecificServiceTask();

				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				var log = logger.ToString();
				AssertContains($"Debug|{serviceTask.TaskName} service task started.", log);
				AssertContains($"Debug|There is no {serviceTask.MessageName} to send. {serviceTask.TaskName} service task completed early.", log);

				CombineAssertions(logger.ToString(), new VoidParameterlessDelegate(() => AssertEDIInterchanges(new ZQuery(), 0, null)));
			}
		}

		public void TestServiceTaskRunFrequency()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			var defaultSchedule = thisClassAttribute?.DefaultScheduleRunEvery ?? "";

			_ = ServiceTaskFrequencyTestHelper.ParseFrequency(defaultSchedule, out var scheduleFrequency, out var scheduleRecurrence);

			CombineAssertions($"Nudged service tasks should run every {ExpectedServiceTaskRunFrequency.period} {ExpectedServiceTaskRunFrequency.runningEvery}. Override {nameof(ExpectedServiceTaskRunFrequency)} if your service task is not nudged.", () =>
			{
				AssertEquals(ExpectedServiceTaskRunFrequency.period, scheduleFrequency);
				AssertEquals(ConvertToScheduleRecurrence(ExpectedServiceTaskRunFrequency.runningEvery), scheduleRecurrence);
			});
		}

		protected virtual (int period, RunsEvery runningEvery) ExpectedServiceTaskRunFrequency
			=> (6, RunsEvery.Hour);

		public void TestServiceTaskAttributes()
		{
			var attrib = GetHostedServiceAttributes().FirstOrDefault();
			AssertNotNull(attrib);
			AssertEquals($"{attrib.Code} service task IsMandatory: {ExpectedServiceTaskIsMandatory.reason}", ExpectedServiceTaskIsMandatory.value, attrib.IsMandatory);
			AssertEquals($"{attrib.Code} service task schedule must be editable so that users can control schedule in absence of nudging", false, attrib.IsScheduleReadOnly);
			AssertEquals($"{attrib.Code} service task does not allow multiple instances", false, attrib.AllowsMultipleInstances);
			AssertEquals($"{attrib.Code} service task country should match service task requirement", CountryCode, attrib.RequiresCompanyInCountry);

			var serviceTaskCode = GetHostedServiceAttributes().Single().Code;
			var usesNudging = typeof(T).Assembly.GetCustomAttributes(typeof(HostedServiceBusinessObjectBindingAttribute), false)
								.Cast<HostedServiceBusinessObjectBindingAttribute>()
								.Any(atr => atr.ServiceTaskCode == serviceTaskCode);
			if (usesNudging)
			{
				AssertEquals($"{attrib.Code} service task minimum period is 15 minutes when nudged", "15minutes", attrib.MinimumPeriod);
				AssertEquals($"{attrib.Code} service task maximum period is 12 hours when nudged", "12hours", attrib.MaximumPeriod);
			}
			else
			{
				AssertEquals($"{attrib.Code} service task minimum period is 5 minutes when scheduled", "5minutes", attrib.MinimumPeriod);
				AssertEquals($"{attrib.Code} service task maximum period is 1 hour when scheduled", "1hour", attrib.MaximumPeriod);
			}

			AssertEquals($"{attrib.Code} service task can run in any branch", true, attrib.CanRunInAnyBranch);
		}

		protected virtual (bool value, string reason) ExpectedServiceTaskIsMandatory
			=> (true, "Because legal compliance");

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccEInvoicingTransactionPivotSchema.Constants.TableName,
						null,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + GetCountrySpecificServiceTask().CountryCode),
				};
			}
		}

		public abstract void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled();

		public abstract void TestSuccessfulCreationOfEDIInterchange_OneCompany();

		public abstract void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany();

		protected void AssertEDIInterchanges(ZQuery ediInterchangeFilter, int expectedNumberOfInterchange, Action<ZGuid> assertEDIMessages, string servicePointSuffix = "")
		{
			var interchanges = Factory.Load<IXmlEDIInterchange>(ediInterchangeFilter);
			AssertEquals("Interchange Count", expectedNumberOfInterchange, interchanges.Length);

			foreach (var interchange in interchanges)
			{
				AssertEDIInterchange(interchange, servicePointSuffix);
				assertEDIMessages?.Invoke(interchange.PK);
			}
		}

		protected void AssertEDIMessages(ZGuid interchangePK, ZGuid branchPK, ZGuid departmentPK, params ZGuid[] linkedObjectIDs)
		{
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchangePK));

			ediMessages.Select(m => m.EM_LinkUniqueID).ToList().ForEach(x => AssertCollectionContains("Message to LinkedObjectID", x, linkedObjectIDs));

			foreach (var message in ediMessages)
			{
				foreach (var linkedObjectID in linkedObjectIDs)
				{
					if (message.EM_LinkedObject == linkedObjectID)
					{
						AssertEDIMessage(message, branchPK, departmentPK);
						break;
					}
				}
			}
		}

		protected abstract void AssertEDIMessage(EDIMessage message, ZGuid branchPK, ZGuid departmentPK);

		protected abstract void AssertEDIInterchange(IXmlEDIInterchange interchange, ZString servicePointSuffix);

		protected abstract ZGuid[] GetLinkedObjectIDs(params AccEInvoicingBatch[] batches);

		protected override void SetUpCore()
		{
			base.SetUpCore();
			Helper.SetupControlAccounts();
		}

		protected EInvoicingTestHelper Helper
		{
			get { return helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator)); }
		}
		EInvoicingTestHelper helper;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected abstract ZString CountryCode { get; }
	}
}
