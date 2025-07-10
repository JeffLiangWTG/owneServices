using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestNonUnique()
		{
			CodeDescriptionPairList list = null;
			AssertNoExceptionThrown(() => list.NonUnique());

			list = new CodeDescriptionPairList();
			NUnit.Framework.Assert.That(list.NonUnique().Length, Is.EqualTo(0));

			list.AddPair("AAA", "DESC1");
			NUnit.Framework.Assert.That(list.NonUnique().Length, Is.EqualTo(0));

			var numberType1 = new CustomsReferenceNumberType();
			numberType1.Code = "BBB";
			numberType1.IsUnique = false;
			list.Add(numberType1);

			var numberType2 = new CustomsReferenceNumberType();
			numberType2.Code = "CCC";
			numberType2.IsUnique = true;
			list.Add(numberType2);

			var numberType3 = new CustomsReferenceNumberType();
			numberType3.Code = "DDD";
			numberType3.IsUnique = false;
			list.Add(numberType3);

			NUnit.Framework.Assert.That(list.NonUnique(), Is.EquivalentTo(new[] { "BBB", "DDD" }));
		}

		[ExpectNoExceptions]
		public void TestGetChildLockByInfo()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			using (ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment))
			{
				NUnit.Framework.Assert.That(mutex.Lock(), Is.EqualTo(true), "mutex Locked OK?");
				var result = Factory.GetChildLockByInfo(MutexIDs.CusSCAOceanBillJobBeingCreatedForConsol, dummyBO.PK, Core.Constants.CountryCodes.NewZealand);
				NUnit.Framework.Assert.That(result, Is.EqualTo("Someone else"), "Mutex locked by");
			}
		}

		[ExpectNoExceptions]
		public void TestRoundBetweenZeroAndOneWillBeOne()
		{
			NUnit.Framework.Assert.That(new ZDecimal(0.8m).RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundBetweenZeroAndOneWillBeOne");
			NUnit.Framework.Assert.That(new ZDecimal(0.3m).RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundBetweenZeroAndOneWillBeOne");
		}

		[ExpectNoExceptions]
		public void TestValueLessThanCriticalValueWillRoundDown()
		{
			NUnit.Framework.Assert.That(new ZDecimal(1.1m).RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "ValueLessThanCriticalValueWillRoundDown");
			NUnit.Framework.Assert.That(new ZDecimal(1.5001m).RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(0.51m), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "ValueLessThanCriticalValueWillRoundDown");
		}

		[ExpectNoExceptions]
		public void TestValueGreaterThanOrEqualToCriticalValueWillRoundUp()
		{
			NUnit.Framework.Assert.That(new ZDecimal(1.5m).RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(2m).Using(CustomComparers.TypeComparison), "ValueGreaterThanOrEqualToCriticalValueWillRoundUp");
			NUnit.Framework.Assert.That(new ZDecimal(1.51m).RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(0.51m), Is.EqualTo(2m).Using(CustomComparers.TypeComparison), "ValueGreaterThanOrEqualToCriticalValueWillRoundUp");
		}

		[ExpectNoExceptions]
		public void TestRoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise()
		{
			NUnit.Framework.Assert.That(new ZDecimal(0.8m).RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise");
			NUnit.Framework.Assert.That(new ZDecimal(0.3m).RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise - this should actually round to Zero if valid to");
			NUnit.Framework.Assert.That(new ZDecimal(1.8m).RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(2m).Using(CustomComparers.TypeComparison), "RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise");
			NUnit.Framework.Assert.That(new ZDecimal(1.5m).RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise");
			NUnit.Framework.Assert.That(new ZDecimal(1.3m).RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise");
			NUnit.Framework.Assert.That(new ZDecimal(0.5m).RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise - this should actually round to Zero if valid to");
			NUnit.Framework.Assert.That(new ZDecimal(0.51m).RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise");
		}

		[ExpectNoExceptions]
		public void TestIsChildLocked()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji), Is.True);
			Factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji), Is.True);
			Factory.UnlockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji);
		}

		[ExpectNoExceptions]
		public void TestLockChild()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			NUnit.Framework.Assert.That(Factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji), Is.True);
			NUnit.Framework.Assert.That(Factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji), Is.True);
			Factory.UnlockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji);
		}

		[ExpectNoExceptions]
		public void TestUnLockChild()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			Factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji);
			Factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji), Is.True);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu), Is.True);
			Factory.UnlockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji);
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji), Is.True);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu), Is.True);
			Factory.UnlockChild(MutexIDs.AMSJobBeingCreatedForConsol, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu), Is.True);
			Factory.UnlockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu);
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu), Is.True);
		}

		[ExpectNoExceptions]
		public void TestUnlockAllChildrenOnParent()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			Factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji);
			Factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji), Is.True);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu), Is.True);
			Factory.UnlockAllChildrenOnParent(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK);
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Fiji), Is.True);
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, dummyBO.PK, Core.Constants.CountryCodes.Vanuatu), Is.True);
		}

		[ExpectNoExceptions]
		public void TestMixedMutexIDsOnConsol()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			Factory.LockChild(MutexIDs.AgencySundry, dummyBO.PK, Core.Constants.CountryCodes.Australia);
			Factory.LockChild(MutexIDs.ArchivingOffline, dummyBO.PK, Core.Constants.CountryCodes.Australia);
			Factory.LockChild(MutexIDs.ArchivingOffline, dummyBO.PK, Core.Constants.CountryCodes.Bahamas);
			Factory.LockChild(MutexIDs.BatchAggregtorRunning, dummyBO.PK, Core.Constants.CountryCodes.Azerbaijan);

			Factory.UnlockAllChildrenOnParent(MutexIDs.ArchivingOffline, dummyBO.PK);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.AgencySundry, dummyBO.PK, Core.Constants.CountryCodes.Australia), Is.True);
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.ArchivingOffline, dummyBO.PK, Core.Constants.CountryCodes.Australia), Is.True);
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.ArchivingOffline, dummyBO.PK, Core.Constants.CountryCodes.Bahamas), Is.True);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.BatchAggregtorRunning, dummyBO.PK, Core.Constants.CountryCodes.Azerbaijan), Is.True);

			Factory.UnlockAllChildrenOnParent(MutexIDs.AgencySundry, dummyBO.PK);
			Factory.UnlockAllChildrenOnParent(MutexIDs.BatchAggregtorRunning, dummyBO.PK);
		}

		[ExpectNoExceptions]
		public void TestMultipleParentsWithSameChildKey()
		{
			var dummyBO1 = Factory.New<DummyBusinessObject>();
			var dummyBO2 = Factory.New<DummyBusinessObject>();
			Factory.LockChild(MutexIDs.ArchivingOffline, dummyBO1.PK, Core.Constants.CountryCodes.Australia);
			Factory.LockChild(MutexIDs.ArchivingOffline, dummyBO2.PK, Core.Constants.CountryCodes.Australia);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.ArchivingOffline, dummyBO1.PK, Core.Constants.CountryCodes.Australia), Is.True);
			NUnit.Framework.Assert.That(Factory.IsChildLocked(MutexIDs.ArchivingOffline, dummyBO2.PK, Core.Constants.CountryCodes.Australia), Is.True);
			Factory.UnlockChild(MutexIDs.ArchivingOffline, dummyBO1.PK, Core.Constants.CountryCodes.Australia);
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.ArchivingOffline, dummyBO1.PK, Core.Constants.CountryCodes.Australia), Is.True);
			Factory.UnlockChild(MutexIDs.ArchivingOffline, dummyBO2.PK, Core.Constants.CountryCodes.Australia);
			NUnit.Framework.Assert.That(!Factory.IsChildLocked(MutexIDs.ArchivingOffline, dummyBO2.PK, Core.Constants.CountryCodes.Australia), Is.True);
		}

		[ExpectNoExceptions]
		public void TestAddFetchHintWithClusterKey()
		{
			var orgAddress = Factory.New<OrgAddress>();
			NUnit.Framework.Assert.That(Factory.ActiveFetchHintsForTable(OrgHeaderSchema.Constants.TableName), Is.EqualTo(0));
			Factory.AddFetchHintWithClusterKey(orgAddress.OA_JobLoadingDuration, orgAddress.OA_OH, typeof(OrgHeader), OrgAddressSchema.OA_JobLoadingDuration, OrgAddressSchema.OA_OH, false);
			NUnit.Framework.Assert.That(Factory.ActiveFetchHintsForTable(OrgHeaderSchema.Constants.TableName), Is.EqualTo(1));
		}
	}
}
