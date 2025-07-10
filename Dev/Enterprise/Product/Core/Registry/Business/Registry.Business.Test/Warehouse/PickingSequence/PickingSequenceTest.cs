using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PickingSequence))]
	sealed class PickingSequenceTest : RegistryBusinessObjectTemplateTestCase<PickingSequence>
	{
		#region TestDefaultValues

		public void TestDefaultValues()
		{
			var pickSequence = BizObj;
			AssertEquals("HighPriorityLocations", (ZByte)1, pickSequence.HighPriorityLocations);
			AssertEquals("FifoFallback", (ZByte)2, pickSequence.FifoFallback);
			AssertEquals("FifoOption should be 2.", (ZByte)2, pickSequence.FifoOption);
			AssertEquals("FullPallets", ZByte.Zero, pickSequence.FullPallets);
			AssertEquals("ConsolidatedPallets", (ZByte)0, pickSequence.ConsolidatedPallets);
			AssertEquals("ExpiryDate", (ZByte)0, pickSequence.ExpiryDate);
			AssertEquals("PickFaces", ZByte.Zero, pickSequence.PickFaces);
			AssertEquals("PalletOverflow", ZByte.Zero, pickSequence.PalletOverflow);
			AssertEquals("BrokenPallets", ZByte.Zero, pickSequence.BrokenPallets);
			AssertEquals("Fifo Bulk only value should be zero.", ZByte.Zero, pickSequence.FifoBulkOnly);
			AssertEquals("FifoBulkOnlyFallback default value should be zero.", ZByte.Zero, pickSequence.FifoBulkOnly);
			AssertEquals("IsPickfaceEmptyPreventPickingFromBulk must not be selected by default.", false, pickSequence.IsPickfaceEmptyPreventPickingFromBulk);

			AssertEquals("Row", (ZByte)1, pickSequence.Row);
			AssertEquals("Column", (ZByte)2, pickSequence.Column);
			AssertEquals("Level", (ZByte)3, pickSequence.Level);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override PickingSequence GetBusinessObjectToClone()
		{
			var result = new PickingSequence();

			result.BrokenPallets = 1;
			result.ConsolidatedPallets = 2;
			result.ExpiryDate = 7;
			result.FifoFallback = 3;
			result.FifoBulkOnly = 0;
			result.FullPallets = 4;
			result.PalletOverflow = 5;
			result.PickFaces = 6;
			result.HighPriorityLocations = 8;

			result.Column = 1;
			result.Level = 2;
			result.Row = 3;

			return result;
		}

		protected override PickingSequence GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
