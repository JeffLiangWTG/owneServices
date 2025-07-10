using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(NonPersistentCusDV1DetailPivotCollection<CusEntryInstruction, NonPersistentCusDV1DetailPivot>))]
	class NonPersistentCusDV1DetailPivotCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentCusDV1DetailPivotCollection<CusEntryInstruction, NonPersistentCusDV1DetailPivot>>
	{
		public void TestConstructorArgumentNull()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => new NonPersistentCusDV1DetailPivotCollection<CusEntryInstruction, NonPersistentCusDV1DetailPivot>(null, (e) => new(e)));
				AssertExceptionThrown<ArgumentNullException>(() => new NonPersistentCusDV1DetailPivotCollection<CusEntryInstruction, NonPersistentCusDV1DetailPivot>(entryInstruction, null));
			});
		}

		public void TestConstructorPopulatesCollection()
		{
			declaration.DV1Details.AddNew();
			var newEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("DV1Details Count same as Pivots Count", 2, newEntryInstruction.DV1DetailsPivots.Count);
				AssertEquals("Has Changes", false, newEntryInstruction.DV1DetailsPivots.HasChanges);
			});
		}

		public void TestAdditionOfDV1DetailOnDeclarationAdds()
		{
			var dv1DetailsPivots = entryInstruction.DV1DetailsPivots;
			CombineAssertions(() =>
			{
				AssertEquals("DV1Details Count same as Pivots Count", 1, entryInstruction.DV1DetailsPivots.Count);
				var newDV1Detail = declaration.DV1Details.AddNew();
				AssertEquals("Pivots increased when Dec has new DV1Detail Added", 2, entryInstruction.DV1DetailsPivots.Count);
			});
		}

		public void TestRemovalOfDV1DetailOnDeclarationRemoves()
		{
			var newDV1Detail = declaration.DV1Details.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("DV1Details Count same as Pivots Count", 2, entryInstruction.DV1DetailsPivots.Count);
				declaration.DV1Details.Remove(newDV1Detail);
				AssertEquals("Pivots decreased when Dec has DV1Detail deleted", 1, entryInstruction.DV1DetailsPivots.Count);
			});
		}

		public void TestAllowNewCore()
		{
			AssertEquals("Not allowed", false, entryInstruction.DV1DetailsPivots.AllowNew);
		}

		protected override NonPersistentCusDV1DetailPivotCollection<CusEntryInstruction, NonPersistentCusDV1DetailPivot> GetCollectionToTest() => new (entryInstruction, e => new(e));

		protected override BusinessObject GetNewElementToAddToTheCollection() => new NonPersistentCusDV1DetailPivot(entryInstruction);

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.DV1Details.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
	}
}
