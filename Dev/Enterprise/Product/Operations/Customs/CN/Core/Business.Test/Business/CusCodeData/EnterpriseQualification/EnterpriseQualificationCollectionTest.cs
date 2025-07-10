using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EnterpriseQualificationCollection))]
	class EnterpriseQualificationCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<EnterpriseQualification>
	{
		public void TestDefaultValuesSetForNewChild()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var coll = new EnterpriseQualificationCollection(instruction);
			var epq = coll.AddNew();
			AssertEquals("Type", "EPQ", epq.CY_Type);
			AssertEquals("ParentTable", instruction.PK, epq.CY_ParentID);
			AssertEquals("ParentTable", "CEI", epq.CY_ParentTableCode);
		}

		protected override CusCodeDataCollection<EnterpriseQualification> GetCusCodeDataCollection()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new EnterpriseQualificationCollection(instruction);
		}
	}
}
