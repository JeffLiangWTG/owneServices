using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(SecondCusBondDetail))]
	class SecondCusBondDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPW_BondAmount_SetProperty_Uncommitted()
		{
			AssertSetProperty_Uncommitted(r => r.PW_BondAmountInfo, new ZDecimal(1));
		}

		public void TestPW_BondNumber2_SetProperty_Uncommitted()
		{
			AssertSetProperty_Uncommitted(r => r.PW_BondNumber2Info, new ZString("ABC"));
		}

		public void TestPW_BondEffectiveDate_SetProperty_Uncommitted()
		{
			AssertSetProperty_Uncommitted(r => r.PW_BondEffectiveDateInfo, ZDateTime.BrettsBirthday);
		}

		public void TestPW_BondAmount_LockGuaranteeManagementMutex()
		{
			AssertSetProperty_LockGuaranteeManagementMutex(r => r.PW_BondAmountInfo);
		}

		public void TestPW_BondNumber2_LockGuaranteeManagementMutex()
		{
			AssertSetProperty_LockGuaranteeManagementMutex(r => r.PW_BondNumber2Info);
		}

		public void TestPW_BondEffectiveDate_LockGuaranteeManagementMutex()
		{
			AssertSetProperty_LockGuaranteeManagementMutex(r => r.PW_BondEffectiveDateInfo);
		}

		public void TestPW_BondAmount_LockGuaranteeManagementMutex_LockedByOthers()
		{
			AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(r => r.PW_BondAmountInfo);
		}

		public void TestPW_BondNumber2_LockGuaranteeManagementMutex_LockedByOthers()
		{
			AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(r => r.PW_BondNumber2Info);
		}

		public void TestPW_BondEffectiveDate_LockGuaranteeManagementMutex_LockedByOthers()
		{
			AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(r => r.PW_BondEffectiveDateInfo);
		}

		public void TestCanDelete_LockGuaranteeManagementMutex()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			releaseGuarantee.PW_BondNumber2 = "Entry Reference";
			releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
			releaseGuarantee.PW_BondAmount = 1m;
			Factory.Save();
			using (var mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, guarantee.Instruction.PK.ToString()))
			{
				mutex.Lock();
				AssertEquals(false, releaseGuarantee.CanDelete);
				mutex.Unlock();
			}
		}

		public void TestPW_BondNumber2_ReadOnly_SingleTransaction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondType = GuaranteeBondTypeList.Codes.SingleTransaction;
			guarantee.PW_BondNumber2 = "Entry Reference";
			guarantee.PW_BondEffectiveDate = ZDateTime.Today;
			guarantee.PW_BondAmount = 1m;
			Factory.Save();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("New", false, releaseGuarantee.PW_BondNumber2Info.ReadOnly);
				releaseGuarantee.PW_BondNumber2 = "Entry Reference";
				releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
				releaseGuarantee.PW_BondAmount = 1m;
				Factory.Save();
				AssertEquals("InDatabase", false, releaseGuarantee.PW_BondNumber2Info.ReadOnly);
			});
		}

		public void TestPW_BondEffectiveDate_ReadOnly_SingleTransaction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			guarantee.PW_BondType = GuaranteeBondTypeList.Codes.SingleTransaction;
			guarantee.PW_BondNumber2 = "Entry Reference";
			guarantee.PW_BondEffectiveDate = ZDateTime.Today;
			guarantee.PW_BondAmount = 1m;
			Factory.Save();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("New", false, releaseGuarantee.PW_BondEffectiveDateInfo.ReadOnly);
				releaseGuarantee.PW_BondNumber2 = "Entry Reference";
				releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
				releaseGuarantee.PW_BondAmount = 1m;
				Factory.Save();
				AssertEquals("InDatabase", false, releaseGuarantee.PW_BondEffectiveDateInfo.ReadOnly);
			});
		}

		public void TestPW_BondNumber2_ReadOnly_Continuous()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("New", false, releaseGuarantee.PW_BondNumber2Info.ReadOnly);
				releaseGuarantee.PW_BondNumber2 = "Entry Reference";
				releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
				releaseGuarantee.PW_BondAmount = 1m;
				Factory.Save();
				AssertEquals("InDatabase", true, releaseGuarantee.PW_BondNumber2Info.ReadOnly);
			});
		}

		public void TestPW_BondEffectiveDate_ReadOnly_Continuous()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("New", false, releaseGuarantee.PW_BondEffectiveDateInfo.ReadOnly);
				releaseGuarantee.PW_BondNumber2 = "Entry Reference";
				releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
				releaseGuarantee.PW_BondAmount = 1m;
				Factory.Save();
				AssertEquals("InDatabase", true, releaseGuarantee.PW_BondEffectiveDateInfo.ReadOnly);
			});
		}

		public void TestValidation()
		{
			AssertType<SecondCusBondDetailValidation>(Factory.New<SecondCusBondDetail>().Validation);
		}

		public void TestPW_ApplicationCode_IfSetToNot2ND()
		{
			var releaseGuarantee = Factory.New<SecondCusBondDetail>();
			releaseGuarantee.PW_ApplicationCode = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("LastMessageReported", "PW_ApplicationCode for SecondCusBondDetail should not be set to anything beside '2ND'.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				AssertEquals("Unchanged", SecondCusBondDetail.ApplicationCode, releaseGuarantee.PW_ApplicationCode);
			});
		}

		public void TestPW_ActivityCode_IfSetToNotREL()
		{
			var releaseGuarantee = Factory.New<SecondCusBondDetail>();
			releaseGuarantee.PW_ActivityCode = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("LastMessageReported", "PW_ActivityCode for SecondCusBondDetail should not be set to anything beside 'REL'.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				AssertEquals("Unchanged", GuaranteeActivityCodeList.Codes.ReleasesGuarantee, releaseGuarantee.PW_ActivityCode);
			});
		}

		public void TestHumanReadableName()
		{
			var releaseGuarantee = Factory.New<SecondCusBondDetail>();
			releaseGuarantee.PW_BondNumber2 = "REF1";
			releaseGuarantee.PW_BondEffectiveDate = new ZDateTime(2021, 7, 15, 8, 18, 1);
			AssertEquals("Release Guarantee (Reference Number 'REF1', Issue Date '15 Jul 2021 08:18')", releaseGuarantee.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			var releaseGuarantee = Factory.New<SecondCusBondDetail>();
			CombineAssertions(() =>
			{
				AssertEquals("PW_ActivityCode", GuaranteeActivityCodeList.Codes.ReleasesGuarantee, releaseGuarantee.PW_ActivityCode);
				AssertEquals("PW_ApplicationCode", SecondCusBondDetail.ApplicationCode, releaseGuarantee.PW_ApplicationCode);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			return instruction.ReleaseGuarantees.AddNew();
		}

		void AssertSetProperty_LockGuaranteeManagementMutex(Func<SecondCusBondDetail, ZPropertyInfo> getPropertyInfo)
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			releaseGuarantee.PW_BondNumber2 = "Entry Reference";
			releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
			releaseGuarantee.PW_BondAmount = 1m;
			Factory.Save();
			CombineAssertions(() =>
			{
				var guaranteeManagementMutex = guarantee.Instruction.GuaranteeManagementMutex;
				AssertEquals("Pre-condition", false, guaranteeManagementMutex.IsLocked && guaranteeManagementMutex.HasLock);
				getPropertyInfo(releaseGuarantee).Value = getPropertyInfo(releaseGuarantee).DefaultValue;
				AssertEquals("Locked by current", true, guaranteeManagementMutex.IsLocked && guaranteeManagementMutex.HasLock);
				AssertEquals(getPropertyInfo(releaseGuarantee).Name, getPropertyInfo(releaseGuarantee).DefaultValue, getPropertyInfo(releaseGuarantee).Value);
				guarantee.Instruction.UnlockGuaranteeManagementMutex();
			});
		}

		void AssertSetProperty_LockGuaranteeManagementMutex_LockedByOthers(Func<SecondCusBondDetail, ZPropertyInfo> getPropertyInfo)
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var releaseGuarantee = guarantee.Instruction.ReleaseGuarantees.AddNew();
			releaseGuarantee.PW_BondNumber2 = "Entry Reference";
			releaseGuarantee.PW_BondEffectiveDate = ZDateTime.Today;
			releaseGuarantee.PW_BondAmount = 1m;
			Factory.Save();
			var count = 0;
			getPropertyInfo(releaseGuarantee).ValueChanged += (s, e) => count++;
			using (var mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, guarantee.Instruction.PK.ToString()))
			{
				mutex.Lock();
				getPropertyInfo(releaseGuarantee).Value = getPropertyInfo(releaseGuarantee).DefaultValue;
				CombineAssertions(() =>
				{
					AssertEquals("Unchanged", getPropertyInfo(releaseGuarantee).OriginalValue, getPropertyInfo(releaseGuarantee).Value);
					AssertEquals("PropertyInfo refreshes", 1, count);
				});
				mutex.Unlock();
			}
		}

		void AssertSetProperty_Uncommitted<T>(Func<SecondCusBondDetail, ZPropertyInfo> getPropertyInfo, T value) where T : IZType
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			var releaseGuarantee = (SecondCusBondDetail)((IBindingList)guarantee.Instruction.ReleaseGuarantees).AddNew();
			getPropertyInfo(releaseGuarantee).Value = value;
			CombineAssertions(() =>
			{
				AssertEquals("Not locked", false, guarantee.Instruction.GuaranteeManagementMutex.IsLocked);
				AssertEquals("Changed", value, getPropertyInfo(releaseGuarantee).Value);
			});
		}
	}
}
