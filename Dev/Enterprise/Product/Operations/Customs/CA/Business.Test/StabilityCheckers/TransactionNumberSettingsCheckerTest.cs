using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business.StabilityCheckers.Testing
{
	sealed class TransactionNumberSettingsCheckerTest : TestCaseWithFactory
	{
		public void TestResults()
		{
			var checker = new TransactionNumberSettingsChecker();
			StabilityResult[] result = checker.Check();
			AssertEquals("result.Length", 0, result.Length);

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = "TST";

			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company1.GC_Code = "TC1";
			company1.GC_OH_OrgProxy = orgProxy.PK;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "GB1";
			branch1.GB_GC = company1.PK;

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");

			Factory.Save();

			var settingBO = new TransactionNumberSettingBO(Factory);
			var existingCollection = settingBO.ExistingTransactionNumberSettingCollection;
			var availableCollection = settingBO.AvailableTransactionNumberSettingCollection;
			var transactionNumberSetting1 = availableCollection.OfType<TransactionNumberSetting>().Single(x => x.RangesSeparator == "11111DIF");
			var transactionNumberSetting2 = availableCollection.OfType<TransactionNumberSetting>().Single(x => x.RangesSeparator == $"11111{branch1.PK}IMP");

			transactionNumberSetting2.IsEditable = true;
			existingCollection.Add(transactionNumberSetting2);
			availableCollection.Remove(transactionNumberSetting2);

			transactionNumberSetting1.MinNumber = 10000;
			transactionNumberSetting1.NextNumber = 15123;
			transactionNumberSetting1.MaxNumber = 19999;

			transactionNumberSetting2.MinNumber = 19000;
			transactionNumberSetting2.NextNumber = 29901;
			transactionNumberSetting2.MaxNumber = 29999;

			Factory.Save();

			result = checker.Check();
			AssertContainsExactElementsInAnyOrder(new ZString[]
				{
					"Canada Transaction Number Settings TC1 / TST 11111 DIF - has problems:\r\n" +
					"\tThe range 'TC1 / TST 11111 DIF' [10000..19999] intersects with range 'TC1 / TST 11111 IMP GB1' [19000..29999].",
					"Canada Transaction Number Settings TC1 / TST 11111 IMP GB1 - has problems:\r\n" +
					"\tThere are less than 100 available numbers left.\r\n" +
					"\tThe range 'TC1 / TST 11111 IMP GB1' [19000..29999] intersects with range 'TC1 / TST 11111 DIF' [10000..19999]."
				},
				result.Select(r => r.Description).ToArray()
			);
		}

		public void TestNoErrorReportWhenTheCheckerIsRunInSTBServiceTask()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.FillWithValidTestData();
			var importerAddInfo = OrgImpAddInfo.Get(org);
			importerAddInfo.ZO_AccountSecurityNumber = "11223";
			Factory.Save();

			ErrorReporter.Clear();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("STB", true))
			{
				var checker = new TransactionNumberSettingsChecker();
				_ = checker.Check();
				AssertEquals("There should no error report when running this checker via STB", ZString.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestThereIsNoExceptionWhenNoCABranch()
		{
			var allCompanies = Factory.Load<GlbCompany>(new ZQuery());
			allCompanies.ForEach(c => c.GC_IsActive = false);
			Factory.Save();

			var auBranchCompany = Factory.New<GlbCompany>();
			auBranchCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			auBranchCompany.GC_Code = "AUC";
			var auBranch = auBranchCompany.Branches.AddNew();
			auBranch.FillWithValidTestData();
			AssertNoExceptionThrown(() =>
			{
				var checker = new TransactionNumberSettingsChecker();
				_ = checker.Check();
			});
		}
	}
}
