using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC016A;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC016AProcessorTest : DTBaseProcessorTest<Cc016AType>
	{
		public void TestPendingTransactionLinkedToTheRejectedDepartureMessageIsDeleted()
		{
			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Number = "19860101";
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_Type = EUGuaranteeTypeList.Codes.TRA;

			var permit1TransLine1 = guaranteeHeader1.AddTransaction("NCT00050167", "CMT-DEL", "111", "", 100m, 200m, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader1.AddTransaction("NCT00050167", "CMT-CON", "111", "", -10m, 200m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			Factory.Save();

			AssertType<CusGuaranteeHeader>(permit1TransLine1.PermitHeader);
			var transactions = guaranteeHeader1.GetTransactions();
			foreach (var transaction in transactions)
			{
				AssertEquals(false, transaction.CPL_TransactionStatus.Equals(PermitTransactionStatusList.Codes.Deleted));
			}
			var guaranteeHeaderList = new List<CusGuaranteeHeader>();
			guaranteeHeaderList.Add(guaranteeHeader1);

			var header = helper.GetProcessReceivedMessageHeader("DT016A_MESSAGE.xml", guaranteeHeaderList, org1);

			AssertEquals(true, header.BH_JobReference.Equals("NCT00050167"));
			AssertEquals(true, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage(header.Messages.LastOutgoingMessage).Equals("111"));
			AssertEquals(2, header.Messages.Count);
			AssertEquals("016", header.Messages.LastIncomingMessage.EM_MessageType);

			var query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactionRequested = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-DEL", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Deleted, transactionRequested[0].CPL_TransactionStatus);
			AssertEquals(1, transactionRequested.Length);
		}

		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT016A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors,
					ExpectedNewDepartureStatus = EU.NCTS.Business.NctsTransitStatusList.Codes.DeclarationRejected,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewMessageInterpretation = @"
<p>New message status: Message Syntax or Business Rule Errors</p>
<p>New departure status: Declaration Rejected</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>NUMERO AGREMENT of HEADER is in error : Infrigement to rule NAT070 - Original value : 00000004<br>
CODE of AUTHORISED LOCATION OF GOODS in HEADER is in error : Infrigement to rule NAT011<br>
CODE of AUTHORISED LOCATION OF GOODS in HEADER is in error<br>
<br></p>"
				};
			}
		}
	}
}
