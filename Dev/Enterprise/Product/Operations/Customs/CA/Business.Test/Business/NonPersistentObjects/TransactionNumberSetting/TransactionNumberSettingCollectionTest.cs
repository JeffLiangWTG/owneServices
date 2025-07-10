using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TransactionNumberSettingCollection))]
	sealed class TransactionNumberSettingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TransactionNumberSettingCollection>
	{
		protected override TransactionNumberSettingCollection GetCollectionToTest()
		{
			var settingBO = new TransactionNumberSettingBO(Factory);
			return settingBO.ExistingTransactionNumberSettingCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TransactionNumberSetting(Factory, "Test", "12345", null, null, false);
		}

		[UseSnapshotProtection]
		public override void TestDelete()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmNums WHERE SN_Name like 'GeneratorFountain-C-%'");

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "GB1";
			branch.GB_GC = company.PK;

			Factory.Save();

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var settingBO = new TransactionNumberSettingBO(Factory);
			var setting1 = settingBO.AvailableTransactionNumberSettingCollection.OfType<TransactionNumberSetting>().Single(s => s.RangeName == "CA1 12345");
			var setting2 = settingBO.AvailableTransactionNumberSettingCollection.OfType<TransactionNumberSetting>().Single(s => s.RangeName == "CA1 12345 DIF");

			foreach (var setting in new TransactionNumberSetting[] { setting1, setting2 })
			{
				settingBO.AvailableTransactionNumberSettingCollection.RemoveAndDelete(setting);
				AssertEquals(false, settingBO.AvailableTransactionNumberSettingCollection.Contains(setting));
				AssertEquals(true, settingBO.ExistingTransactionNumberSettingCollection.Contains(setting));
				AssertEquals(false, setting.IsDeleted);
				AssertHasError("new setting should have error to indicate for user that setting is not saved yet", setting.MinNumberInfo, "For newly added number ranges all parameters must be set explicitly.");
				AssertHasError("new setting should have error to indicate for user that setting is not saved yet", setting.NextNumberInfo, "For newly added number ranges all parameters must be set explicitly.");
				AssertHasError("new setting should have error to indicate for user that setting is not saved yet", setting.MaxNumberInfo, "For newly added number ranges all parameters must be set explicitly.");
			}

			setting2.MinNumber = 30000;
			setting2.NextNumber = 20000;
			setting2.MaxNumber = 10000;

			foreach (var setting in new TransactionNumberSetting[] { setting1, setting2 })
			{
				settingBO.ExistingTransactionNumberSettingCollection.RemoveAndDelete(setting);
				AssertEquals(true, settingBO.AvailableTransactionNumberSettingCollection.Contains(setting));
				AssertEquals(false, settingBO.ExistingTransactionNumberSettingCollection.Contains(setting));
				AssertEquals(false, setting.IsDeleted);
				AssertNoRowErrors("available settings should not have errors", setting);
				AssertNoError("available settings should not have errors", setting.MinNumberInfo, "For newly added number ranges all parameters must be set explicitly.");
				AssertNoError("available settings should not have errors", setting.NextNumberInfo, "For newly added number ranges all parameters must be set explicitly.");
				AssertNoError("available settings should not have errors", setting.MaxNumberInfo, "For newly added number ranges all parameters must be set explicitly.");
			}
		}

		[UseSnapshotProtection]
		public void TestLoadExistingTransactionNumbers()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmNums WHERE SN_Name like 'GeneratorFountain-C-%'");

			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CA1";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "GB1";
			branch1.GB_GC = company1.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "GB2";
			branch2.GB_GC = company1.PK;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CA2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "33333");

			var importer1 = Factory.New<OrgHeader>();
			var addInfo1 = OrgImpAddInfo.Get(importer1);
			importer1.OH_Code = "IM1";
			addInfo1.ZO_AccountSecurityNumber = "22222";
			addInfo1.ZO_AccountSecirityPassword = "222222";

			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("11111", branch1, "IMP"))).SetNext(Factory, 3);
			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("11111", branch1, "B2"))).SetNext(Factory, 10);
			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("22222", branch2, "IMP"))).SetNext(Factory, 11);
			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("22222", branch2, "LVS"))).SetNext(Factory, 12);
			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("11111", null, "DIF"))).SetNext(Factory, 13);

			Factory.Save();

			var settingBO = new TransactionNumberSettingBO(Factory);
			var collection = settingBO.ExistingTransactionNumberSettingCollection;

			AssertEquals("collection", 5, collection.Count);

			var transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "");
			AssertNull(transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"11111{branch1.PK}IMP");
			AssertEquals("CurrentNextNumber", 3L, (long)transactionNumberSetting.InheritedNextNumberForTest);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"11111{branch1.PK}B2");
			AssertEquals("CurrentNextNumber", 10L, (long)transactionNumberSetting.InheritedNextNumberForTest);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"11111{branch1.PK}LVS");
			AssertEquals(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"22222{branch2.PK}IMP");
			AssertEquals("CurrentNextNumber", 11L, (long)transactionNumberSetting.InheritedNextNumberForTest);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"22222{branch2.PK}B2");
			AssertEquals(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"22222{branch2.PK}LVS");
			AssertEquals("CurrentNextNumber", 12L, (long)transactionNumberSetting.InheritedNextNumberForTest);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "11111DIF");
			AssertEquals("CurrentNextNumber", 13L, (long)transactionNumberSetting.InheritedNextNumberForTest);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "22222DIF");
			AssertEquals(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "33333DIF");
			AssertEquals(null, transactionNumberSetting);
		}

		[UseSnapshotProtection]
		public void TestLoadAvailableTransactionNumbers()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CA1";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "GB1";
			branch1.GB_GC = company1.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "GB2";
			branch2.GB_GC = company1.PK;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CA2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "33333");

			var importer1 = Factory.New<OrgHeader>();
			var addInfo1 = OrgImpAddInfo.Get(importer1);
			importer1.OH_Code = "IM1";
			addInfo1.ZO_AccountSecurityNumber = "22222";
			addInfo1.ZO_AccountSecirityPassword = "222222";

			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("11111", branch1, "IMP"))).SetNext(Factory, 3);
			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("11111", branch1, "B2"))).SetNext(Factory, 10);
			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("22222", branch2, "IMP"))).SetNext(Factory, 11);
			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("22222", branch2, "LVS"))).SetNext(Factory, 12);
			Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("11111", null, "DIF"))).SetNext(Factory, 13);

			Factory.Save();

			var settingBO = new TransactionNumberSettingBO(Factory);
			var collection = settingBO.AvailableTransactionNumberSettingCollection;

			AssertEquals("collection",
				3 /*ASEC numbers*/ *
				(
					3 /*Declaration Types*/ * collection.CABranches.Count() +
					1 /*ASEC only, no branch or type*/
				) +
				2 /*DIF ranges for broker ASEC numbers*/
				- 5 /*Existing Number Ranges*/, collection.Count
			);

			// Available number fountains report -1 as current next number because there neither number fountais not fallbacks for that are setup.
			// For DIF number fountains there are no fallbacks, and default stats of the number fountain are used.

			var transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "");
			AssertNull(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"11111{branch1.PK}IMP");
			AssertEquals(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"11111{branch1.PK}B2");
			AssertEquals(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"11111{branch1.PK}LVS");
			AssertEquals("CurrentNextNumber", -1, (long)transactionNumberSetting.InheritedNextNumberForTest);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"22222{branch2.PK}IMP");
			AssertEquals(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"22222{branch2.PK}B2");
			AssertEquals("CurrentNextNumber", -1, (long)transactionNumberSetting.InheritedNextNumberForTest);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == $"22222{branch2.PK}LVS");
			AssertEquals(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "11111DIF");
			AssertEquals(null, transactionNumberSetting);

			// DIF number fountain can only be set up for brokers. There is no DIF number fountain for client ASEC numbers.
			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "22222DIF");
			AssertEquals(null, transactionNumberSetting);

			transactionNumberSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "33333DIF");
			AssertEquals("CurrentNextNumber", 1L, (long)transactionNumberSetting.InheritedNextNumberForTest);
		}

		public void TestOwner()
		{
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = "TST";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company.GC_OH_OrgProxy = orgProxy.PK;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "GB1";
			branch1.GB_GC = company.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "GB2";
			branch2.GB_GC = company.PK;

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");

			var importer1 = Factory.New<OrgHeader>();
			var addInfo1 = OrgImpAddInfo.Get(importer1);
			importer1.OH_Code = "IM1";
			addInfo1.ZO_AccountSecurityNumber = "22222";
			addInfo1.ZO_AccountSecirityPassword = "222222";

			Factory.Save();

			var settingBO = new TransactionNumberSettingBO(Factory);
			var collection = settingBO.AvailableTransactionNumberSettingCollection;
			var brokerSetting = collection.OfType<TransactionNumberSetting>().First(s => s.AccountSecurityCode == "11111");
			var clientSetting = collection.OfType<TransactionNumberSetting>().First(s => s.AccountSecurityCode == "22222");

			AssertEquals("CA1 / TST", brokerSetting.Owner);
			AssertEquals("IM1", clientSetting.Owner);
		}
	}
}
