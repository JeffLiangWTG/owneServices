using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC045C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC045CProcessorTest : TP5BaseProcessorTest<Cc045CType, CC045CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.WriteOffNotification;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC045CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedMessageStatus => Common.EU.LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => "015";

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;

		public void TestUpdateGuaranteeTransactions()
		{
			EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var eoriCode = "123456789000";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader = GetGuaranteeHeader(orgHeader, "19860101");
			guaranteeHeader.AddTransaction("Opening Bal", "Opening Bal", "111", "", 200m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader.AddTransaction("2595950308700261001902", "CMT-CON", "111", "", -50m, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			guaranteeHeader.AddTransaction("2595950308700261001902", "CMT-TO-CONF", "111", "", -100m, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			Factory.Save();

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "2595950308700261001902";
			nctsHeader.Principal.E2_OA_Address = guaranteeHeader.PermitHolder.MainAddress.PK;

			nctsHeader.GetEffectiveGuarantees().DeleteAll();
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals("Prerequisite: Guarantee should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees().First().CusGuarantee.PK, guarantee.CusGuarantee.PK);

			var outgoingMessage = GetNCTSFREDIMessage(nctsHeader);
			outgoingMessage.EM_Status = EDIMessage.Status.Received;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = TP5MessageTypeList.Codes.CC015C;
			nctsHeader.MovementHeader.Messages.Add(outgoingMessage);

			var incomingMessage = GetNCTSFREDIMessage(nctsHeader);
			incomingMessage.EM_MessageNum = "111";
			nctsHeader.Messages.Add(incomingMessage);

			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals("PermitAppId of LastOutgoingMessage should be equal to MessageReferenceNumber", "111", FRPermitHelper.GetPermitAppIdForMessage(nctsHeader.GetOutgoingMessage(incomingMessage)));
			AssertEquals("MessageSubType of incoming message should be 045", TP5ResponseMessageSubTypeList.Codes.WriteOffNotification, nctsHeader.Messages.LastIncomingMessage.EM_MessageSubType);

			var permitHelper = new PermitTestDataHelper(Factory);
			var query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", $"NCTS write-off {FRPermitHelper.GetPermitAppIdForMessage(incomingMessage)} [{ExpectedMRN}]", nctsHeader.Messages.LastIncomingMessage.EM_MessageNum, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			var transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one write-off transaction created", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be confirmed", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);
			AssertEquals("Transaction CPL_Reference should be equal to BM_PaperlessInbondNum", nctsHeader.MovementHeader.BM_PaperlessInbondNum, transactionRequested[0].CPL_Reference);
			AssertEquals("Transaction CPL_AppId should be equal to EM_MessageNum", nctsHeader.Messages.LastIncomingMessage.EM_MessageNum, transactionRequested[0].CPL_AppId);
			AssertEquals("Transaction CPL_TranValue should counter confirmed transaction's amount of 50", 50m, transactionRequested[0].CPL_TranValue);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for the CMT-CON query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be confirmed", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for the CMT-TO-CONF query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be deleted", PermitTransactionStatusList.Codes.Deleted, transactionRequested[0].CPL_TransactionStatus);
		}
	}
}
