using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NBStandaloneMessageSerializationTest : TestCaseWithFactory
{
	public void TestSerialization()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		var previousDocument1 = goodsItem.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		previousDocument1.CSI_ReferenceNumber = "1A";
		previousDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		previousDocument1.CSI_Status = "X";
		previousDocument1.CSI_CustomsOffice = "IT137100";
		previousDocument1.CSI_LineNo = 1;

		var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "A3";
		previousDocument2.CSI_ReferenceNumber = "2";

		var previousDocument3 = goodsItem.PreviousDocuments.AddNew();
		previousDocument3.CSI_Procedure = "2";
		previousDocument3.CSI_ReferenceNumber = "1";

		var serializedMessage = ((IOutgoingCustomsMessageCreationStrategy)new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, new NBStandaloneMessageSendingObject(nctsHeader))).GenerateMessage().EM_MessageText;

		AssertMultilineASCIIEquals(
			"TNB           <<MSGNO PLACEHOLDER NB 1>>00				1	A3	1	A	010120	X	1	137100		2	1						0	0				A3	2							2	1						0	0																																												0	"
			, serializedMessage);
	}
}
