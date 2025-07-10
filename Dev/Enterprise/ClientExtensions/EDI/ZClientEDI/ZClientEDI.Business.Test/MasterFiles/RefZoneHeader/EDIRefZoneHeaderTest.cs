using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIRefZoneHeader))]
	sealed class EDIRefZoneHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertEquals(typeof(EDIRefZoneHeader), Factory.New<RefZoneHeader>().GetType());
		}

		public void TestLookups()
		{
			EDIRefZoneHeader header = Factory.New<EDIRefZoneHeader>();

			AssertEquals(typeof(EDIRefZoneHeaderLookups), header.Lookups.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}
	}
}
