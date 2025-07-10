using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TIRMessageSerializationTest : TestCaseWithFactory
{
	public void TestSerialization()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_ExportDate = new ZDateTime(2021, 09, 28);
		movementHeader.BM_EntryDate = ZDate.Today;
		movementHeader.GoodsItems.AddNew();

		var serializedMessage = ((IOutgoingCustomsMessageCreationStrategy)new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, new TIRMessageSendingObject(nctsHeader))).GenerateMessage().EM_MessageText;

		AssertMultilineASCIIEquals(
			$"TET           <<MSGNO PLACEHOLDER>>00				{movementHeader.BM_EntryDate.ToString("ddMMyyyy")}	0	0		1																 								  	  				0																												0																																										0	0			28092021				\r\n" +
			"?ET1          <<MSGNO PLACEHOLDER>>00																																	0	0						1			0		0																								0											0	0.00	0.00	"
			, serializedMessage);
	}
}
