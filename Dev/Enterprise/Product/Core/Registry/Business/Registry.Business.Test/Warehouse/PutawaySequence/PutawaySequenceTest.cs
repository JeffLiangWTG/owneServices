using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PutawaySequence))]
	sealed class PutawaySequenceTest : RegistryBusinessObjectTemplateTestCase<PutawaySequence>
	{
		public void TestDefaultValues()
		{
			AssertEquals("ClientArea", ZByte.Zero, BizObj.ClientArea);
			AssertEquals("Location", (ZByte)1, BizObj.Location);
			AssertEquals("PickFace", ZByte.Zero, BizObj.PickFace);
			AssertEquals("ProductArea", ZByte.Zero, BizObj.ProductArea);

			AssertEquals("Column", (ZByte)2, BizObj.Column);
			AssertEquals("Level", (ZByte)3, BizObj.Level);
			AssertEquals("Row", (ZByte)1, BizObj.Row);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.ClientArea = 9;
			BizObj.Location = 9;
			BizObj.PickFace = 9;
			BizObj.ProductArea = 9;

			BizObj.Column = 9;
			BizObj.Level = 9;
			BizObj.Row = 9;

			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.ClientAreaInfo);
			AssertHasErrors(BizObj.LocationInfo);
			AssertHasErrors(BizObj.PickFaceInfo);
			AssertHasErrors(BizObj.ProductAreaInfo);

			AssertHasErrors(BizObj.ColumnInfo);
			AssertHasErrors(BizObj.LevelInfo);
			AssertHasErrors(BizObj.RowInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override PutawaySequence GetBusinessObjectToClone()
		{
			PutawaySequence result = new PutawaySequence();

			result.ClientArea = 1;
			result.Location = 2;
			result.PickFace = 3;
			result.ProductArea = 4;

			result.Column = 1;
			result.Level = 2;
			result.Row = 3;

			return result;
		}

		protected override PutawaySequence GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
