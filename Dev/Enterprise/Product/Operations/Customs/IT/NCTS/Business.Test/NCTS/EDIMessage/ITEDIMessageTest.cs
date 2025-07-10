using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class ITEDIMessageTest : TestCaseWithFactory
{
	public void TestEM_Status()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var ediMessage = nctsHeader.Messages.AddNew();

		ediMessage.EM_MessageType = "R";
		ediMessage.EM_Status = "FAL";
		AssertEquals("MEE status for NCTS Header", "MEE", nctsHeader.EffectiveMessageStatus);
	}
}
