using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.ELG.ServiceTasks.Testing
{
	[TestedType(typeof(SagAccountsServiceTask))]
	class SagAccountsServiceTaskTest : ServiceTaskTestCase<SagAccountsServiceTask>
	{
		[TestDate(2014, 6, 20, 13, 30, 23)]
		public void TestIsEnvironmentValid()
		{
			RunTaskSchedule(ServiceTask);
			Assert(!ServiceTask.GetBuffer().AsString.Contains("Sage accounts data export started"));
			TestHelper.SetRegistry(Temp.TempPath);
			TestHelper.setInvalidRegistry(GlbCompany.CurrentCompany.PK.ToGuid());
			Factory.Save();
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("Sage Data Export Settings for Company 'EDI' are not set or are invalid"));
			TestHelper.SetValidRegistryAll();
			TestHelper.NewPMG();
			Factory.Save();
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.GetBuffer().AsString.Contains("Sage accounts data export started"));
		}

		/*Please figure out which code create JobHeader incorrectly*/
		[SuspendToTestReportJobIsChangedByDifferentCompany]
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[TestDate(2014, 6, 20, 13, 30, 23)]
		public void TestExportForMultipleCompany()
		{
			GlbBranch[] validBranches = GlbBranch.GetOneActiveBranchPerCompany();
			TestHelper.SetValidRegistryDataTransfer(ZDateTime.Now.AddMinutes(-30));
			TestHelper.SetValidRegistryBranchDepartmentCodeCollectionItem();
			TestHelper.SetValidRegistryTransportAndChargeCodeCollectionItem();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem();
			TestHelper.NewPMG();
			Factory.Save();
			OrgHeader debtor = TestHelper.AddOrg();
			debtor.OH_FullName = "CargoWise";
			Factory.Save();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsReceivable, debtor.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsPayable, debtor.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, Core.Constants.CurrencyCodes.Singapore, LedgerTypes.AccountsReceivable, debtor.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, Core.Constants.CurrencyCodes.Singapore, LedgerTypes.AccountsPayable, debtor.OH_Code);
			debtor = TestHelper.AddOrg();
			debtor.OH_FullName = "ABC International";
			Factory.Save();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsReceivable, debtor.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsPayable, debtor.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, Core.Constants.CurrencyCodes.Singapore, LedgerTypes.AccountsReceivable, debtor.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, Core.Constants.CurrencyCodes.Singapore, LedgerTypes.AccountsPayable, debtor.OH_Code);
			Factory.Save();
			foreach (GlbBranch branch in validBranches)
			{
				using (branch.SetAsTemporaryContext())
				{
					var companyCode = branch.Company.GC_Code;
					var expectedDirectory = Path.Combine(Env.TempPath, companyCode);
					Directory.CreateDirectory(expectedDirectory);
					TestHelper.SetExportDirectoryForCompany(branch.Company.PK.ToGuid(), Path.Combine(Env.TempPath, branch.Company.GC_Code));
					TestHelper.NewInvoices(LedgerTypes.AccountsReceivable);
				}
			}

			Factory.Save();
			try
			{
				RunTaskSchedule(ServiceTask);
				foreach (GlbBranch branch in validBranches)
				{
					var companyCode = branch.Company.GC_Code;
					var expectedDirectory = Path.Combine(Env.TempPath, companyCode);
					var directoryPath = new DirectoryInfo(expectedDirectory);
					AssertEquals(1, directoryPath.GetFiles().Length);
					AssertEquals("AR_" + companyCode + "_20140620013023_0001.csv", directoryPath.GetFiles()[0].Name);
				}
			}
			finally
			{
				TempDirectory.DeleteDirectory(Env.TempPath);
			}

			foreach (GlbBranch branch in validBranches)
			{
				using (branch.SetAsTemporaryContext())
				{
					var companyCode = branch.Company.GC_Code;
					if (companyCode == "SIN")
					{
						var expectedDirectory = Path.Combine(Env.TempPath, companyCode);
						Directory.CreateDirectory(expectedDirectory);
						TestHelper.SetExportDirectoryForCompany(branch.Company.PK.ToGuid(), Path.Combine(Env.TempPath, branch.Company.GC_Code));
					}
					else
					{
						TestHelper.SetEnableInterfaceValueForCompany(branch.Company.PK.ToGuid(), false);
					}

					TestHelper.NewInvoices(LedgerTypes.AccountsReceivable);
				}
			}

			Factory.Save();
			try
			{
				RunTaskSchedule(ServiceTask);
				foreach (GlbBranch branch in validBranches)
				{
					var companyCode = branch.Company.GC_Code;
					var expectedDirectory = Path.Combine(Env.TempPath, companyCode);
					var directoryPath = new DirectoryInfo(expectedDirectory);
					if (!directoryPath.Exists)
					{
						directoryPath.Create();
					}

					if (companyCode == "SIN")
					{
						AssertEquals(1, directoryPath.GetFiles().Length);
						AssertEquals("AR_" + companyCode + "_20140620013023_0002.csv", directoryPath.GetFiles()[0].Name);
					}
					else
					{
						AssertEquals(0, directoryPath.GetFiles().Length);
					}
				}
			}
			finally
			{
				TempDirectory.DeleteDirectory(Env.TempPath);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		ELGTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new ELGTestHelper(Factory));
			}
		}

		ELGTestHelper testHelper;
		SagAccountsServiceTask ServiceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			ServiceTask = new SagAccountsServiceTask();
			InitialiseTaskSchedule(ServiceTask);
		}
	}
}
