using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemEntryNum))]
	sealed class AsycudaPackedItemEntryNumTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableNameCore()
		{
			var entryNum = (AsycudaPackedItemEntryNum)GetNewBusinessObject();
			entryNum.CE_EntryNum = "LRN1234";
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			AssertEquals("LRN1234 (LRN)", entryNum.HumanReadableName);
		}

		public void TestCanDelete()
		{
			var entryNumber = Factory.New<AsycudaPackedItemEntryNum>();

			entryNumber.CE_EntryIsSystemGenerated = true;
			Assert(!entryNumber.CanDelete);

			entryNumber.CE_EntryIsSystemGenerated = false;
			Assert(entryNumber.CanDelete);
		}

		public void TestReadOnly()
		{
			var entryNumber = Factory.New<AsycudaPackedItemEntryNum>();

			entryNumber.CE_EntryIsSystemGenerated = true;
			Assert(entryNumber.ReadOnly);

			entryNumber.CE_EntryIsSystemGenerated = false;
			Assert(!entryNumber.ReadOnly);
		}

		public void TestCE_EntryIsSystemGenerated()
		{
			var entryNumber = Factory.New<AsycudaPackedItemEntryNum>();
			AssertEquals("Should be default to false.", false, entryNumber.CE_EntryIsSystemGenerated);
		}

		public void TestCaption()
		{
			var entryNumber = Factory.New<AsycudaPackedItemEntryNum>();
			CombineAssertions("Check all captions to be correct", () =>
				{
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(entryNumber.CE_EntryTypeInfo, multipleResourceKeys: null, "Type", "Type", "Type", "Indicates the type of reference number.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(entryNumber.CE_EntryNumInfo, multipleResourceKeys: null, "Reference Number", "Ref. No.", "Reference No.", "Reference number associated with the selected reference number type.");
				}
			);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem = pack.CreatePackedItemForTesting();
			return packedItem.CustomsEntryNumbers.AddNew();
		}
	}
}
