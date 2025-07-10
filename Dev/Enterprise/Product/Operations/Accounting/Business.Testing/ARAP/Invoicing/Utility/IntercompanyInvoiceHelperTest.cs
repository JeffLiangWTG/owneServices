using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class IntercompanyInvoiceHelperTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetReceivingOperator()
		{
			receivingCompanyJob.JH_GS_NKRepOps = "RV";

			AssertEquals("RV", IntercompanyInvoiceHelper.GetReceivingOperator(Factory, invoice.AH_JH));
			AssertEquals(string.Empty, IntercompanyInvoiceHelper.GetReceivingOperator(Factory, ZGuid.NewZGuid()));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetReceivingBranch()
		{
			receivingCompanyJob.JH_GB = nonCurrentBranch.PK;

			AssertEquals(nonCurrentBranch.PK, IntercompanyInvoiceHelper.GetReceivingBranch(Factory, invoice.AH_JH));
			AssertEquals(ZGuid.Empty, IntercompanyInvoiceHelper.GetReceivingBranch(Factory, ZGuid.NewZGuid()));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetReceivingDepartment()
		{
			receivingCompanyJob.JH_GB = TestObjectCreator.NonCurrentDepartment.PK;

			AssertEquals(TestObjectCreator.NonCurrentDepartment.PK, IntercompanyInvoiceHelper.GetReceivingDepartment(Factory, invoice.AH_JH));
			AssertEquals(ZGuid.Empty, IntercompanyInvoiceHelper.GetReceivingDepartment(Factory, ZGuid.NewZGuid()));
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC3.PK);

			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			currentCompany.GC_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;

			GlbCompany nonCurrentCompany = TestObjectCreator.NonCurrentNonDemoCompany;
			nonCurrentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, nonCurrentCompany.PK));

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var sendingCompanyJob = TestObjectCreator.CreateJobHeader();
			receivingCompanyJob = TestObjectCreator.CreateJobHeader();

			sendingCompanyJob.JH_JobNum = "S001001";
			sendingCompanyJob.JH_GC = currentCompany.PK;
			sendingCompanyJob.JH_GS_NKRepOps = "SN";
			sendingCompanyJob.JH_GB = GlbBranch.CurrentBranch.PK;
			sendingCompanyJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			receivingCompanyJob.JH_JobNum = "S001001";
			receivingCompanyJob.JH_GC = TestObjectCreator.NonCurrentNonDemoCompany.PK;
			receivingCompanyJob.JH_GS_NKRepOps = "RV";
			receivingCompanyJob.JH_GB = nonCurrentBranch.PK;
			receivingCompanyJob.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			invoice.AH_JH = sendingCompanyJob.PK;

			Factory.Save();

			userContext = Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), nonCurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
		}

		protected override void TearDown()
		{
			base.TearDown();
			userContext?.Dispose();
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator testObjectCreator;

		GlbBranch nonCurrentBranch;
		JobHeader receivingCompanyJob;
		ARInvoice invoice;
		IDisposable userContext;
	}
}
