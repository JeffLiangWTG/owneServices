using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusFiscalReferenceCollection<>))]
	abstract class CusFiscalReferenceCollectionAbstractTest<TCusFiscalReferenceCollection, TCusFiscalReference> : BusinessObjectCollectionTestCase
		where TCusFiscalReferenceCollection : CusFiscalReferenceCollection<TCusFiscalReference>
		where TCusFiscalReference : CusFiscalReference
	{
		public void TestFiscalReferenceCollectionMaxCount()
		{
			AssertEquals(99, GetCollectionToTest().MaxCount);
		}

		public void TestAdditionalFilter()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var cusFiscalReferenceForEntryInstruction = Factory.NewWithValidTestData<CusFiscalReference>();
			cusFiscalReferenceForEntryInstruction.CFR_ParentID = entryInstruction.PK;
			cusFiscalReferenceForEntryInstruction.CFR_ParentTableCode = entryInstruction.TablePrefix;
			var cusFiscalReferenceForOtherObject = Factory.NewWithValidTestData<CusFiscalReference>();
			cusFiscalReferenceForOtherObject.CFR_ParentID = ZGuid.NewZGuid();
			cusFiscalReferenceForOtherObject.CFR_ParentTableCode = entryInstruction.TablePrefix;
			var cusSupplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			cusSupplyChainActorReference.CFR_ParentID = entryInstruction.PK;
			cusSupplyChainActorReference.CFR_ParentTableCode = entryInstruction.TablePrefix;

			CombineAssertions(() =>
			{
				var collection = (TCusFiscalReferenceCollection)Activator.CreateInstance(typeof(TCusFiscalReferenceCollection), entryInstruction);
				var filter = collection.CompleteFilter;
				AssertEquals("Fiscal Reference", true, cusFiscalReferenceForEntryInstruction.MatchesFilter(filter));
				AssertEquals("Fiscal Reference For other entity", false, cusFiscalReferenceForOtherObject.MatchesFilter(filter));
				AssertEquals("Not Fiscal Reference", false, cusSupplyChainActorReference.MatchesFilter(filter));
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => (TCusFiscalReferenceCollection)Activator.CreateInstance(typeof(TCusFiscalReferenceCollection), Factory.New<CusEntryInstruction>());
	}

	class CusFiscalReferenceCollectionBaseOnlyTest : CusFiscalReferenceCollectionAbstractTest<CusFiscalReferenceCollection<CusFiscalReference>, CusFiscalReference>
	{
	}
}
