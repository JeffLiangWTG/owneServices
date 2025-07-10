using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TransactionNumberSetting))]
	sealed class TransactionNumberSettingTest : NumberSettingTest
	{
		protected override ZString GenerateNumberWithCheckDigit(ZInt number)
		{
			var result = "12345678" + number.ToString().PadLeft(8, '0');
			result += TransactionNumber.GetCheckDigit(result);
			return result;
		}

		protected override ZString GetNumberCountryCode()
		{
			return Core.Constants.CountryCodes.Canada;
		}

		protected override NumberSetting GetNumberSetting()
		{
			return new TransactionNumberSetting(Factory, "Test", "12345678", null, null, false);
		}

		protected override ZString GetNumberType()
		{
			return CusEntryNumber.EntryType.CATransactionNumber;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransactionNumberSetting(Factory, "Test", "12345", null, null, false);
		}

		public void TestRangesSeparator()
		{
			var branch = Factory.New<GlbBranch>();

			var defaultSetting = new TransactionNumberSetting(Factory, "Test", null, null, null, true);
			var securitySetting = new TransactionNumberSetting(Factory, "Test", "12345", null, null, true);
			var branchSetting = new TransactionNumberSetting(Factory, "Test", "12345", branch, null, true);
			var branchAndTypeSetting = new TransactionNumberSetting(Factory, "Test", "12345", branch, "IMP", true);
			var difSetting = new TransactionNumberSetting(Factory, "Test", "12345", null, TransactionNumber.DIFNumberDeclarationType, true);

			AssertEquals("", defaultSetting.RangesSeparator);
			AssertEquals("12345", securitySetting.RangesSeparator);
			AssertEquals($"12345{branch.PK}", branchSetting.RangesSeparator);
			AssertEquals($"12345{branch.PK}IMP", branchAndTypeSetting.RangesSeparator);
			AssertEquals("12345DIF", difSetting.RangesSeparator);
		}

		[UseSnapshotProtection]
		public void TestPersistNextAndMinMaxNumbers()
		{
			DeleteGeneratorFountainNumber();

			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company1.GC_Code = "TC1";

			var company2 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company1.GC_Code = "TC2";

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "22222");

			var settingBO = new TransactionNumberSettingBO(Factory);
			var existingCollection = settingBO.ExistingTransactionNumberSettingCollection;
			var availableCollection = settingBO.AvailableTransactionNumberSettingCollection;
			var transactionNumberSetting1 = existingCollection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "");
			var transactionNumberSetting2 = availableCollection.OfType<TransactionNumberSetting>().Single(x => x.RangesSeparator == "11111");
			var transactionNumberSetting3 = availableCollection.OfType<TransactionNumberSetting>().Single(x => x.RangesSeparator == "22222");
			var transactionNumberSetting4 = availableCollection.OfType<TransactionNumberSetting>().Single(x => x.RangesSeparator == "22222DIF");

			AssertNull("DEFAULT number fountain was removed", transactionNumberSetting1);

			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting2.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting2.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting2.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting3.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting3.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting3.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", 1L, (long)transactionNumberSetting4.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 1L, (long)transactionNumberSetting4.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 99999999L, (long)transactionNumberSetting4.InheritedMaxNumberForTest);

			transactionNumberSetting2.MinNumber = 20000000;
			transactionNumberSetting2.NextNumber = 21000000L;
			transactionNumberSetting2.MaxNumber = 29999999;
			Factory.Save();

			AssertEquals("CurrentNextNumber", 20000000L, (long)transactionNumberSetting2.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 21000000L, (long)transactionNumberSetting2.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 29999999L, (long)transactionNumberSetting2.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting3.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting3.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", -1L, (long)transactionNumberSetting3.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", 1L, (long)transactionNumberSetting4.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 1L, (long)transactionNumberSetting4.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 99999999L, (long)transactionNumberSetting4.InheritedMaxNumberForTest);

			transactionNumberSetting3.MinNumber = 30000000;
			transactionNumberSetting3.NextNumber = 31000000;
			transactionNumberSetting3.MaxNumber = 39999999;
			Factory.Save();

			AssertEquals("CurrentNextNumber", 20000000L, (long)transactionNumberSetting2.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 21000000L, (long)transactionNumberSetting2.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 29999999L, (long)transactionNumberSetting2.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", 30000000L, (long)transactionNumberSetting3.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 31000000L, (long)transactionNumberSetting3.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 39999999L, (long)transactionNumberSetting3.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", 1L, (long)transactionNumberSetting4.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 1L, (long)transactionNumberSetting4.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 99999999L, (long)transactionNumberSetting4.InheritedMaxNumberForTest);

			transactionNumberSetting4.MinNumber = 40000000;
			transactionNumberSetting4.NextNumber = 41000000;
			transactionNumberSetting4.MaxNumber = 49999999;
			Factory.Save();

			AssertEquals("CurrentNextNumber", 20000000L, (long)transactionNumberSetting2.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 21000000L, (long)transactionNumberSetting2.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 29999999L, (long)transactionNumberSetting2.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", 30000000L, (long)transactionNumberSetting3.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 31000000L, (long)transactionNumberSetting3.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 39999999L, (long)transactionNumberSetting3.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", 40000000L, (long)transactionNumberSetting4.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 41000000L, (long)transactionNumberSetting4.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 49999999L, (long)transactionNumberSetting4.InheritedMaxNumberForTest);

			// changing only one parameter for each number fountain, the rest should be the same
			transactionNumberSetting2.MinNumber = 20000100;
			transactionNumberSetting3.NextNumber = 32000000;
			transactionNumberSetting4.MaxNumber = 48999999;
			Factory.Save();

			AssertEquals("CurrentNextNumber", 20000100L, (long)transactionNumberSetting2.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 21000000L, (long)transactionNumberSetting2.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 29999999L, (long)transactionNumberSetting2.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", 30000000L, (long)transactionNumberSetting3.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 32000000L, (long)transactionNumberSetting3.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 39999999L, (long)transactionNumberSetting3.InheritedMaxNumberForTest);
			AssertEquals("CurrentNextNumber", 40000000L, (long)transactionNumberSetting4.InheritedMinNumberForTest);
			AssertEquals("CurrentNextNumber", 41000000L, (long)transactionNumberSetting4.InheritedNextNumberForTest);
			AssertEquals("CurrentNextNumber", 48999999L, (long)transactionNumberSetting4.InheritedMaxNumberForTest);
		}

		[UseSnapshotProtection]
		public void TestIntersectionValidation()
		{
			DeleteGeneratorFountainNumber();

			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company1.GC_Code = "TC1";

			var company2 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company2.GC_Code = "TC2";

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "GB1";
			branch1.GB_GC = company1.PK;

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "GB2";
			branch2.GB_GC = company2.PK;

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "22222");

			Factory.Save();

			var settingBO = new TransactionNumberSettingBO(Factory);
			var existingCollection = settingBO.ExistingTransactionNumberSettingCollection;
			var availableCollection = settingBO.AvailableTransactionNumberSettingCollection;
			var transactionNumberSetting1 = existingCollection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "");
			var transactionNumberSetting2 = availableCollection.OfType<TransactionNumberSetting>().Single(x => x.RangesSeparator == $"11111{branch1.PK}IMP");
			var transactionNumberSetting3 = availableCollection.OfType<TransactionNumberSetting>().Single(x => x.RangesSeparator == $"22222{branch2.PK}IMP");
			var transactionNumberSetting4 = availableCollection.OfType<TransactionNumberSetting>().Single(x => x.RangesSeparator == "22222DIF");

			AssertNull(transactionNumberSetting1);

			AssertEquals("CurrentMinNumber", -1L, (long)transactionNumberSetting2.InheritedMinNumberForTest);
			AssertEquals("CurrentMinNumber", -1L, (long)transactionNumberSetting3.InheritedMinNumberForTest);
			AssertEquals("CurrentMinNumber", 1L, (long)transactionNumberSetting4.InheritedMinNumberForTest);

			AssertEquals("CurrentMaxNumber", -1L, (long)transactionNumberSetting2.InheritedMaxNumberForTest);
			AssertEquals("CurrentMaxNumber", -1L, (long)transactionNumberSetting3.InheritedMaxNumberForTest);
			AssertEquals("CurrentMaxNumber", 99999999L, (long)transactionNumberSetting4.InheritedMaxNumberForTest);

			transactionNumberSetting2.RunPreSaveValidation();
			transactionNumberSetting3.RunPreSaveValidation();
			transactionNumberSetting4.RunPreSaveValidation();

			// no errors or warnings because there are no number fountains in database and none of available number fountains were marked for saving
			AssertNoRowErrors(transactionNumberSetting2);
			AssertNoRowErrors(transactionNumberSetting3);
			AssertNoRowErrors(transactionNumberSetting4);
			AssertNoRowWarnings(transactionNumberSetting2);
			AssertNoRowWarnings(transactionNumberSetting3);
			AssertNoRowWarnings(transactionNumberSetting4);

			availableCollection.Remove(transactionNumberSetting2);
			availableCollection.Remove(transactionNumberSetting3);
			availableCollection.Remove(transactionNumberSetting4);

			transactionNumberSetting2.RunPreSaveValidation();
			transactionNumberSetting3.RunPreSaveValidation();
			transactionNumberSetting4.RunPreSaveValidation();

			// no errors because there are no changes to be saved
			AssertNoRowErrors(transactionNumberSetting2);
			AssertNoRowErrors(transactionNumberSetting3);
			AssertNoRowErrors(transactionNumberSetting4);

			// warnings for intersecting rows
			AssertNoRowWarnings(transactionNumberSetting2);
			AssertHasRowWarning(transactionNumberSetting3, "The range 'TC2 22222 IMP GB2' [1..99999999] intersects with range 'TC2 22222 DIF' [1..99999999].");
			AssertHasRowWarning(transactionNumberSetting4, "The range 'TC2 22222 DIF' [1..99999999] intersects with range 'TC2 22222 IMP GB2' [1..99999999].");

			transactionNumberSetting2.MinNumber = 200000;
			transactionNumberSetting2.NextNumber = 257457;
			transactionNumberSetting2.MaxNumber = 299999;
			transactionNumberSetting3.MinNumber = 200000;
			transactionNumberSetting3.NextNumber = 254768;
			transactionNumberSetting3.MaxNumber = 299999;
			transactionNumberSetting4.MinNumber = 200000;
			transactionNumberSetting4.NextNumber = 245634;
			transactionNumberSetting4.MaxNumber = 299999;

			transactionNumberSetting2.RunPreSaveValidation();
			transactionNumberSetting3.RunPreSaveValidation();
			transactionNumberSetting4.RunPreSaveValidation();

			// no errors for ranges that have different security number, no warnings (errors instead of warnings) because all ranged should be saved
			AssertNoRowErrors(transactionNumberSetting2);
			AssertHasRowError(transactionNumberSetting3, "The range 'TC2 22222 IMP GB2' [200000..299999] intersects with range 'TC2 22222 DIF' [200000..299999].");
			AssertHasRowError(transactionNumberSetting4, "The range 'TC2 22222 DIF' [200000..299999] intersects with range 'TC2 22222 IMP GB2' [200000..299999].");

			AssertNoRowWarnings(transactionNumberSetting2);
			AssertNoRowWarnings(transactionNumberSetting3);
			AssertNoRowWarnings(transactionNumberSetting3);

			transactionNumberSetting3.MinNumber = 200000;
			transactionNumberSetting3.NextNumber = 243564;
			transactionNumberSetting3.MaxNumber = 300000;
			transactionNumberSetting4.MinNumber = 300000;
			transactionNumberSetting4.NextNumber = 367856;
			transactionNumberSetting4.MaxNumber = 399999;

			transactionNumberSetting2.RunPreSaveValidation();
			transactionNumberSetting3.RunPreSaveValidation();
			transactionNumberSetting4.RunPreSaveValidation();

			// one number in intersection
			AssertNoRowErrors(transactionNumberSetting2);
			AssertHasRowError(transactionNumberSetting3, "The range 'TC2 22222 IMP GB2' [200000..300000] intersects with range 'TC2 22222 DIF' [300000..399999].");
			AssertHasRowError(transactionNumberSetting4, "The range 'TC2 22222 DIF' [300000..399999] intersects with range 'TC2 22222 IMP GB2' [200000..300000].");

			AssertNoRowWarnings(transactionNumberSetting2);
			AssertNoRowWarnings(transactionNumberSetting3);
			AssertNoRowWarnings(transactionNumberSetting3);

			transactionNumberSetting3.MinNumber = 200000;
			transactionNumberSetting3.MaxNumber = 299999;
			transactionNumberSetting4.MinNumber = 300000;
			transactionNumberSetting4.MaxNumber = 399999;

			transactionNumberSetting2.RunPreSaveValidation();
			transactionNumberSetting3.RunPreSaveValidation();
			transactionNumberSetting4.RunPreSaveValidation();

			// no intersections
			AssertNoRowErrors(transactionNumberSetting2);
			AssertNoRowErrors(transactionNumberSetting3);
			AssertNoRowErrors(transactionNumberSetting4);

			AssertNoRowWarnings(transactionNumberSetting2);
			AssertNoRowWarnings(transactionNumberSetting3);
			AssertNoRowWarnings(transactionNumberSetting4);
		}

		public void TestNumberOrderValidations()
		{
			var setting = new TransactionNumberSetting(Factory, "Test", null, null, null, true);

			setting.MinNumber = 10000;
			setting.NextNumber = 15000;
			setting.MaxNumber = 20000;

			Factory.Save();

			AssertEquals(10000L, (long)setting.CurrentMinNumber);
			AssertEquals(15000L, (long)setting.CurrentNextNumber);
			AssertEquals(20000L, (long)setting.CurrentMaxNumber);

			setting.NextNumber = 20001;
			setting.RunPreSaveValidation();

			AssertHasError(setting.NextNumberInfo, "The next number should not be greater than the maximum number.");

			setting.NextNumber = 9999;
			setting.RunPreSaveValidation();

			AssertHasError(setting.NextNumberInfo, "The next number should not be less than the minimum number.");

			setting.NextNumber = 11000;
			setting.MaxNumber = 14999;
			setting.RunPreSaveValidation();

			AssertHasError(setting.MaxNumberInfo, "The maximum number should not be less than the current next number.");

			setting.NextNumber = 18000;
			setting.MaxNumber = 17999;
			setting.RunPreSaveValidation();

			AssertHasError(setting.MaxNumberInfo, "The maximum number should not be less than the next number.");

			setting.MinNumber = 15001;
			setting.NextNumber = 0;
			setting.MaxNumber = 0;
			setting.RunPreSaveValidation();

			AssertHasError(setting.MinNumberInfo, "The minimum number should not be greater than the next number.");
		}

		public void TestRemainingCapacityValidation()
		{
			var setting = new TransactionNumberSetting(Factory, "Test", null, null, null, true);

			setting.MinNumber = 10000;
			setting.NextNumber = 15000;
			setting.MaxNumber = 20000;

			Factory.Save();

			AssertEquals(10000L, (long)setting.CurrentMinNumber);
			AssertEquals(15000L, (long)setting.CurrentNextNumber);
			AssertEquals(20000L, (long)setting.CurrentMaxNumber);

			AssertNoRowErrors(setting);

			setting.MinNumber = 0;
			setting.NextNumber = 19901;
			setting.MaxNumber = 0;
			setting.RunPreSaveValidation();

			AssertNoRowErrors(setting);

			setting.MinNumber = 0;
			setting.NextNumber = 19902;
			setting.MaxNumber = 0;
			setting.RunPreSaveValidation();

			AssertHasRowError(setting, "There are less than 100 available numbers left.");

			setting.MinNumber = 0;
			setting.NextNumber = 0;
			setting.MaxNumber = 15099;
			setting.RunPreSaveValidation();

			AssertNoRowErrors(setting);

			setting.MinNumber = 0;
			setting.NextNumber = 0;
			setting.MaxNumber = 15098;
			setting.RunPreSaveValidation();

			AssertHasRowError(setting, "There are less than 100 available numbers left.");
		}

		public void TestNextNumberBoundariesValidation()
		{
			var setting = new TransactionNumberSetting(Factory, "Test", null, null, null, true);

			setting.NextNumber = decimal.MaxValue;
			AssertHasError(setting.NextNumberInfo, "Invalid number.");

			setting.NextNumber = decimal.MinValue;
			AssertHasError(setting.NextNumberInfo, "Invalid number.");

			setting.NextNumber = -1;
			AssertHasError(setting.NextNumberInfo, "The next number should not be negative.");
		}

		public void TestMinNumberBoundariesValidation()
		{
			var setting = new TransactionNumberSetting(Factory, "Test", null, null, null, true);

			setting.MinNumber = decimal.MaxValue;
			AssertHasError(setting.MinNumberInfo, "Invalid number.");

			setting.MinNumber = decimal.MinValue;
			AssertHasError(setting.MinNumberInfo, "Invalid number.");

			setting.MinNumber = -1;
			AssertHasError(setting.MinNumberInfo, "The minimum number should not be negative.");
		}

		public void TestMaxNumberBoundariesValidation()
		{
			var setting = new TransactionNumberSetting(Factory, "Test", null, null, null, true);

			setting.MaxNumber = decimal.MaxValue;
			AssertHasError(setting.MaxNumberInfo, "Invalid number.");

			setting.MaxNumber = decimal.MinValue;
			AssertHasError(setting.MaxNumberInfo, "Invalid number.");

			setting.MaxNumber = 99_999_999 + 1;
			AssertHasError(setting.MaxNumberInfo, "The maximum number should not be greater than 99999999.");

			setting.MaxNumber = -1;
			AssertHasError(setting.MaxNumberInfo, "The maximum number should not be negative.");
		}

		public void TestConcurrentChanges()
		{
			var setting1 = new TransactionNumberSetting(Factory, "Test", null, null, null, true);
			setting1.MinNumber = 10000;
			setting1.NextNumber = 11235;
			setting1.MaxNumber = 19999;

			Factory.Save();

			AssertEquals(0m, setting1.MinNumber);
			AssertEquals(0m, setting1.NextNumber);
			AssertEquals(0m, setting1.MaxNumber);

			var secondFactory = new BusinessObjectFactory();
			var setting2 = new TransactionNumberSetting(secondFactory, "Test", null, null, null, true);

			setting1.NextNumber = 16002;
			setting2.MaxNumber = 16000;

			setting1.RunPreSaveValidation();
			setting2.RunPreSaveValidation();

			// both changes are valid by themselves
			AssertNoRowErrors(setting1);
			AssertNoErrors(setting1.MinNumberInfo);
			AssertNoErrors(setting1.NextNumberInfo);
			AssertNoErrors(setting1.MaxNumberInfo);

			AssertNoRowErrors(setting2);
			AssertNoErrors(setting2.MinNumberInfo);
			AssertNoErrors(setting2.NextNumberInfo);
			AssertNoErrors(setting2.MaxNumberInfo);

			Factory.Save();
			var message = AssertExceptionThrown<SqlException>(() => secondFactory.Save())
				.ToString();
			AssertContains($"Proposed fountain values do not follow the rules: 0 < MinValue <= NextValue <= MaxValue(+1 for non-rollover)", message, ignoreCase: true);
			AssertContains($"Passed values: Min = 0, Next = 0, Max = 16000", message, ignoreCase: true);
			AssertContains($"Adjusted values: Min = 10000, Next = 16002, Max = 16000, CanRollover = 0", message, ignoreCase: true);
		}

		[UseSnapshotProtection]
		public void TestRevalidateRelated()
		{
			DeleteGeneratorFountainNumber();

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company.GC_Code = "TC1";

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "GB1";
			branch1.GB_GC = company.PK;

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "GB2";
			branch2.GB_GC = company.PK;

			var settingBO = new TransactionNumberSettingBO(Factory);
			var collection = settingBO.ExistingTransactionNumberSettingCollection;
			var defaultSetting = collection.OfType<TransactionNumberSetting>().SingleOrDefault(x => x.RangesSeparator == "");
			var setting1 = new TransactionNumberSetting(Factory, "Test Company", "77777", branch1, "IMP", true);
			var setting2 = new TransactionNumberSetting(Factory, "Test Company", "77777", branch2, "IMP", true);
			collection.Add(setting1);
			collection.Add(setting2);

			AssertNull(defaultSetting);
			defaultSetting = new TransactionNumberSetting(Factory, null, null, null, null, true);
			settingBO.ExistingTransactionNumberSettingCollection.Add(defaultSetting);

			defaultSetting.MinNumber = 80000000;
			defaultSetting.NextNumber = 81000000;
			defaultSetting.MaxNumber = 89999999;

			setting1.MinNumber = 10000000;
			setting1.NextNumber = 11000000;
			setting1.MaxNumber = 19999999;

			setting2.MinNumber = 20000000;
			setting2.NextNumber = 21000000;
			setting2.MaxNumber = 29999999;

			AssertNoRowErrors(setting1);
			AssertNoRowErrors(setting2);

			// Changing olny one setting.
			setting2.MinNumber = 19000000;

			// Both settings should have error because they are related.
			AssertHasRowError(setting1, "The range 'Test Company 77777 IMP GB1' [10000000..19999999] intersects with range 'Test Company 77777 IMP GB2' [19000000..29999999].");
			AssertHasRowError(setting2, "The range 'Test Company 77777 IMP GB2' [19000000..29999999] intersects with range 'Test Company 77777 IMP GB1' [10000000..19999999].");
		}

		void DeleteGeneratorFountainNumber()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmNums WHERE SN_Name like 'GeneratorFountain-C-%'");
		}
	}
}
