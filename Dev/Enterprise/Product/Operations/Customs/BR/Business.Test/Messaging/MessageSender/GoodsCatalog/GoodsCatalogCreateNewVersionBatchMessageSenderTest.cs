using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class GoodsCatalogCreateNewVersionBatchMessageSenderTest : BaseGoodsCatalogBatchMessageSenderTest
	{
		protected override BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log)
		{
			return new GoodsCatalogCreateNewVersionBatchMessageSender(goodsCatalogs, log);
		}

		protected override string SendAction => ActionList.Codes.CreateNewVersion;

		protected override string ExpectedCatalogSituation => Constants.Situation.Active;

		protected override string ExpectedSendingLog => @"INFO: Searching for catalogs that attend criteria: “Active Catalogs (Customs Status: Active and Message Status: NOT).”
INFO: [HL TC_0_NST_1] has been sent.
INFO: [HL TC_0_NST_2] has been sent.
INFO: [HL TC_0_NST_3] has been sent.
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
WARNING: [HL TC_X_NST_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_X_NST_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_X_NST_3] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_X_ACC_1] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_X_ACC_2] has not been sent because does not attend the searching criteria.
WARNING: [HL TC_X_ACC_3] has not been sent because does not attend the searching criteria.";
	}
}
