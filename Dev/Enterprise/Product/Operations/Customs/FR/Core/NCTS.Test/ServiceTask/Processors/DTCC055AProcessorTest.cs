using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC055A;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC055AProcessorTest : DTBaseProcessorTest<Cc055AType>
	{
		public void TestPendingTransactionLinkedToInvalidGuaranteesMessageIsDeleted()
		{
			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);

			var eoriCode = "123456789000";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;

			var permitTransLine = guaranteeHeader.AddTransaction("NCT00050167", "CMT-DEL", "111", "", 100m, 200m, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader.AddTransaction("NCT00050167", "CMT-CON", "111", "", -10m, 200m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			Factory.Save();

			AssertType<CusGuaranteeHeader>(permitTransLine.PermitHeader);
			var transactions = guaranteeHeader.GetTransactions();
			foreach (var transaction in transactions)
			{
				AssertEquals(false, transaction.CPL_TransactionStatus.Equals(PermitTransactionStatusList.Codes.Deleted));
			}
			var guaranteeHeaderList = new List<CusGuaranteeHeader>();
			guaranteeHeaderList.Add(guaranteeHeader);

			var header = helper.GetProcessReceivedMessageHeader("DT055A_MESSAGE.xml", guaranteeHeaderList, org);

			AssertEquals(true, header.BH_JobReference.Equals("NCT00050167"));
			AssertEquals(true, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage(header.Messages.LastOutgoingMessage).Equals("111"));
			AssertEquals(2, header.Messages.Count);
			AssertEquals("055", header.Messages.LastIncomingMessage.EM_MessageType);

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
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT055A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = EU.NCTS.Business.NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration Guarantees Not Valid</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Submitted guarantee: 17FR0000000000021</p>
<p>Reason: Code d'accès invalide 1</p>
<p>Submitted guarantee: 17FR0000000000022</p>
<p>Reason: Code d'accès invalide 2</p>
<p>Action required: You can submit an amendment request, a cancellation request or do nothing. In this last case, the declaration status will automatically turn into ""Goods Not released for Transit"" after 30 days.</p>"
				};
			}
		}
	}
}
