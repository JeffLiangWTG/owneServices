using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>))]
	class CusSupplyChainActorReferenceCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCount()
		{
			AssertEquals(99, GetCollectionToTest().MaxCount);
		}

		public void TestAdditionalFilter()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var cusSupplyChainActorReferenceForEntryInstruction = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			cusSupplyChainActorReferenceForEntryInstruction.CFR_ParentID = entryInstruction.PK;
			cusSupplyChainActorReferenceForEntryInstruction.CFR_ParentTableCode = entryInstruction.TablePrefix;
			var cusSupplyChainActorReferenceForOtherObject = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			cusSupplyChainActorReferenceForOtherObject.CFR_ParentID = ZGuid.NewZGuid();
			cusSupplyChainActorReferenceForOtherObject.CFR_ParentTableCode = entryInstruction.TablePrefix;
			var cusFiscalReference = Factory.NewWithValidTestData<CusFiscalReference>();
			cusFiscalReference.CFR_ParentID = entryInstruction.PK;
			cusFiscalReference.CFR_ParentTableCode = entryInstruction.TablePrefix;

			CombineAssertions(() =>
			{
				var collection = new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(entryInstruction);
				var filter = collection.CompleteFilter;
				AssertEquals("CusSupplyChainActorReference", true, cusSupplyChainActorReferenceForEntryInstruction.MatchesFilter(filter));
				AssertEquals("CusSupplyChainActorReference For other entity", false, cusSupplyChainActorReferenceForOtherObject.MatchesFilter(filter));
				AssertEquals("Not CusSupplyChainActorReference", false, cusFiscalReference.MatchesFilter(filter));
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(Factory.New<CusEntryInstruction>());
	}

	[TestedType(typeof(CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>))]
	class CusSupplyChainActorReferenceCollectionBaseOnlyTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCount()
		{
			AssertEquals(99, GetCollectionToTest().MaxCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(Factory.New<CusEntryInstruction>());
	}
}
