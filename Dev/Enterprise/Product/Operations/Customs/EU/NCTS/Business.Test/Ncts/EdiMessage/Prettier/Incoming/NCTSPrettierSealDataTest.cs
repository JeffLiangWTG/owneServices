using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierSealDataTest : TestCase
	{
		public void TestIncidentLocationAddressSequenceNumber() => AssertEquals(1, prettierIncidentLocationAddressData.SequenceNumber);
		public void TestIncidentLocationAddressIdentifier() => AssertEquals("1234", prettierIncidentLocationAddressData.Identifier);
		protected override void SetUp()
		{
			base.SetUp();
			prettierIncidentLocationAddressData = new NCTSPrettierSealData(sequenceNumber: "1", identifier: "1234");
		}
		NCTSPrettierSealData prettierIncidentLocationAddressData;
	}
}
