using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TransactionNumberSettingBO))]
	sealed class TransactionNumberSettingBOTest : NonPersistentBusinessObjectTestCase
	{
		[UseSnapshotProtection]
		public void TestProperties()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmNums WHERE SN_Name != 'GeneratorFountain-C-8E86F7C5'");
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "GB1";
			branch.GB_GC = company.PK;
			Factory.Save();
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var companySetting = new TransactionNumberSetting(Factory, "Test", "12345", null, null, true);
			companySetting.MinNumber = 10000000;
			companySetting.NextNumber = 11000000;
			companySetting.MaxNumber = 19999999;

			Factory.Save();

			var transNums = new TransactionNumberSettingBO(Factory);

			// 3 per each branch + 1 for securityNo without branch + 1 DIF for securityNo without branch
			int allRowsCount = 3 * transNums.ExistingTransactionNumberSettingCollection.CABranches.Count() + 2;

			// only company number fountain is set up
			AssertEquals("Existing transaction number setting count", 1, TransactionNumberSettingBO.ExistingTransactionNumberSettingCollection.Count);
			AssertEquals("Available transaction number setting count", allRowsCount - 1, TransactionNumberSettingBO.AvailableTransactionNumberSettingCollection.Count);
		}

		public void TestOnlyExistingCollectionIsSaved()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "GB1";
			branch.GB_GC = company.PK;

			Factory.Save();

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var bo = new TransactionNumberSettingBO(Factory);

			TransactionNumberSetting defaultSetting = bo.ExistingTransactionNumberSettingCollection.OfType<TransactionNumberSetting>().SingleOrDefault(s => s.RangeName == "DEFAULT");
			TransactionNumberSetting impSetting = bo.AvailableTransactionNumberSettingCollection.OfType<TransactionNumberSetting>().Single(s => s.RangeName == "CA1 12345 IMP GB1");
			TransactionNumberSetting difSetting = bo.AvailableTransactionNumberSettingCollection.OfType<TransactionNumberSetting>().Single(s => s.RangeName == "CA1 12345 DIF");

			AssertNull(defaultSetting);
			defaultSetting = new TransactionNumberSetting(Factory, "", null, null, null, true);
			bo.ExistingTransactionNumberSettingCollection.Add(defaultSetting);

			bo.AvailableTransactionNumberSettingCollection.Remove(impSetting);
			bo.AvailableTransactionNumberSettingCollection.Remove(difSetting);

			AssertEquals(true, bo.ExistingTransactionNumberSettingCollection.Contains(impSetting));
			AssertEquals(true, bo.ExistingTransactionNumberSettingCollection.Contains(difSetting));

			defaultSetting.MinNumber = 40000000;
			defaultSetting.NextNumber = 40000777;
			defaultSetting.MaxNumber = 40999999;

			impSetting.MinNumber = 30000000;
			impSetting.NextNumber = 30000777;
			impSetting.MaxNumber = 30999999;

			difSetting.MinNumber = 30000000;
			difSetting.NextNumber = 30000777;
			difSetting.MaxNumber = 30999999;

			bo.ExistingTransactionNumberSettingCollection.Remove(impSetting);

			AssertEquals(true, bo.AvailableTransactionNumberSettingCollection.Contains(impSetting));

			Factory.Save();

			AssertEquals(false, impSetting.IsInitialized);
			AssertEquals(true, difSetting.IsInitialized);
			AssertEquals((ZDecimal)30000000, difSetting.CurrentMinNumber);
			AssertEquals((ZDecimal)30000777, difSetting.CurrentNextNumber);
			AssertEquals((ZDecimal)30999999, difSetting.CurrentMaxNumber);
		}

		#region Implementation

		TransactionNumberSettingBO TransactionNumberSettingBO
		{
			get { return transactionNumberSettingBO ?? (transactionNumberSettingBO = new TransactionNumberSettingBO(Factory)); }
		}

		TransactionNumberSettingBO transactionNumberSettingBO;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransactionNumberSettingBO(Factory);
		}

		#endregion
	}
}
