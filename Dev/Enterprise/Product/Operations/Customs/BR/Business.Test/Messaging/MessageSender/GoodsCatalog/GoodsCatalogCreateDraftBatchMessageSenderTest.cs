using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class GoodsCatalogCreateDraftBatchMessageSenderTest : BaseGoodsCatalogBatchMessageSenderTest
	{
		protected override BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log)
		{
			return new GoodsCatalogCreateDraftBatchMessageSender(goodsCatalogs, log);
		}

		protected override string SendAction => ActionList.Codes.CreateDraft;

		protected override string ExpectedCatalogSituation => Constants.Situation.Draft;

		protected override string ExpectedSendingLog => @"INFO: Searching for catalogs that attend criteria: “Catalogs that have not yet been sent to Customs (Customs Status: empty and Message Status: NOT).”
WARNING: [HL TC_0_NST_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_0_NST_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_0_NST_3] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_0_ACC_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_0_ACC_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_0_ACC_3] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_1_NST_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_1_NST_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_1_NST_3] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_1_ACC_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_1_ACC_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_1_ACC_3] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_2_NST_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_2_NST_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_2_NST_3] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_2_ACC_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_2_ACC_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_2_ACC_3] has not been sent because does not attend the searching criteria.
INFO: [HL TC_X_NST_1] has been sent.
INFO: [HL TC_X_NST_2] has been sent.
INFO: [HL TC_X_NST_3] has been sent.
WARNING: [HL TC_X_ACC_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_X_ACC_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_X_ACC_3] has not been sent because does not attend the searching criteria.";
	}
}
