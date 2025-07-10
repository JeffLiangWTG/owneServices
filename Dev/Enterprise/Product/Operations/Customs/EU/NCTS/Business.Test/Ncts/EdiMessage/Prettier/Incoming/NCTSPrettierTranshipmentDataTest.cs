using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierTranshipmentDataTest : TestCase
	{
		public void TestTranshipmentDataContainerIndicator() => AssertEquals(true, prettierTranshipmentData.ContainerIndicator);
		public void TestTranshipmentDataTransportMeansNationality() => AssertEquals("AF", prettierTranshipmentData.TransportMeansNationality);
		public void TestTranshipmentDataTransportMeansIdentificationNumber() => AssertEquals("0012", prettierTranshipmentData.TransportMeansIdentificationNumber);
		public void TestTranshipmentDataTransportMeansTypeOfIdentification() => AssertEquals("30", prettierTranshipmentData.TransportMeansTypeOfIdentification);

		protected override void SetUp()
		{
			base.SetUp();
			prettierTranshipmentData = new NCTSPrettierTranshipmentData("1", "AF", "0012", "30");
		}
		NCTSPrettierTranshipmentData prettierTranshipmentData;
	}
}
