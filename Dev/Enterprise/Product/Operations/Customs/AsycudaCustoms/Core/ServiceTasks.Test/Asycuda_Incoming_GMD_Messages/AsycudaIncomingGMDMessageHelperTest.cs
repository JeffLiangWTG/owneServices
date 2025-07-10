using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks.Testing
{
	class AsycudaIncomingGMDMessageHelperTest : TestCaseWithFactory
	{
		public void TestGetBGMReferenceFromXML_ASYCUDA()
		{
			var xml = @"<ASYCUDA>
  <Declarant>
    <Declarant_name>DEMO COMPANY</Declarant_name>
    <Reference>
      <Number>DR1234567899</Number>
    </Reference>
  </Declarant>
</ASYCUDA>";
			AssertEquals("For ASYCUDA", "DR1234567899", AsycudaIncomingGMDMessageHelper.GetBGMReferenceFromXML(xml));
		}

		public void TestGetBGMReferenceFromXML_ESAD()
		{
			var xml = @"<ESAD>
  <header>
    <declarant>
      <reference_number>DR1234567899</reference_number>
    </declarant>
  </header>
</ESAD>";
			AssertEquals("For ESAD", "DR1234567899", AsycudaIncomingGMDMessageHelper.GetBGMReferenceFromXML(xml));
		}
	}
}
