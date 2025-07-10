using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageInvoicingSupporter))]
	sealed class TemporaryStorageInvoicingSupporterTest : Enterprise.MasterFiles.Business.Testing.JobInvoicingSupporterTest
	{
		public void TestOverriddenDepartmentPK()
		{
			var department = Factory.New<GlbDepartment>();
			var departmentPK = department.PK;
			AccountingConfigurationRegistry.Instance.CustomsExWarehouse.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentPK.ToGuid());
			var header = Factory.New<TemporaryStorageHeader>();
			using var job = new Job.Loader(Factory, header).TryCreateWithMutex();
			Assert("Precondition: department PK is not ZGuid.Empty", departmentPK.IsValid);
			AssertEquals("JH_GE is defaulted to CustomsExWarehouse value", departmentPK, job.JH_GE);
		}

		public void TestGetJobInvoicingSecurityCore()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals("SecurityCheckpointToSendWithMessageError", Env.Security.CustomsTemporaryStorageJobInvoicing, tempHeader.InvoicingSupporter.JobInvoicingSecurity);
		}

		public void TestConsumerType()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals("ConsumerType", JobInvoicingConsumerTypes.CustomsTemporaryStorage, tempHeader.InvoicingSupporter.ConsumerType);
		}

		public void TestCreateAccountingJobOnSavingOfOperationsJob()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals("CreateAccountingJobOnSavingOfOperationsJob", true, tempHeader.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<TemporaryStorageHeader>();
		}
	}
}
