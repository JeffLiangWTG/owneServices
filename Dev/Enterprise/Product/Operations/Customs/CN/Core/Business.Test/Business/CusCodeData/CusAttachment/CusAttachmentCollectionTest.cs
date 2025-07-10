using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusAttachmentCollection))]
	class CusAttachmentCollectionTest : CusCodeDataCollectionTest<CusAttachment>
	{
		public void TestDefaultValuesSetForNewChild()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var coll = new CusAttachmentCollection(instruction);
			var attachment = coll.AddNew();
			AssertEquals("Type", "ATH", attachment.CY_Type);
			AssertEquals("ParentID", instruction.PK, attachment.CY_ParentID);
			AssertEquals("ParentTable", "CEI", attachment.CY_ParentTableCode);
		}

		protected override CusCodeDataCollection<CusAttachment> GetCusCodeDataCollection()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new CusAttachmentCollection(instruction);
		}
	}
}
