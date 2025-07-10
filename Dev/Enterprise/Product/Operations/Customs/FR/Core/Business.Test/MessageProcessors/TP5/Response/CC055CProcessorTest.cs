using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC055C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC055CProcessorTest : TP5BaseProcessorTest<Cc055CType, CC055CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.GuaranteeNotValid;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC055CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedMessageStatus => Common.EU.LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => "015";

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

		public void TestUpdateGuaranteeTransactions()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var eoriCode = "123456789000";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader1 = GetGuaranteeHeader(orgHeader, "GRN1");
			guaranteeHeader1.AddTransaction("Opening Bal", "Opening Bal for GRN1", "111", ZString.Empty, 200m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader1.AddTransaction("2595950308700261001902", "CMT-CON GRN1", "111", "", -50m, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			guaranteeHeader1.AddTransaction("2595950308700261001902", "NCTS departure for GRN1", "111", ZString.Empty, -10, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);

			var guaranteeHeader2 = GetGuaranteeHeader(orgHeader, "GRN2");
			guaranteeHeader2.AddTransaction("Opening Bal", "Opening Bal for GRN2", ZString.Empty, ZString.Empty, 1000m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader2.AddTransaction("2595950308700261001902", "NCTS departure for GRN2", "111", ZString.Empty, -500m, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader2.CPH_Balance += guaranteeHeader2.GetTransactions().Sum(x => x.CPL_TranValue);
			Factory.Save();

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "2595950308700261001902";
			nctsHeader.Principal.E2_OA_Address = guaranteeHeader1.PermitHolder.MainAddress.PK;

			nctsHeader.GetEffectiveGuarantees().DeleteAll();
			var guarantee1 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee1.PW_BondNumber = guaranteeHeader1.CPH_Number;

			var guarantee2 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee2.PW_BondNumber = guaranteeHeader2.CPH_Number;

			AssertEquals("Prerequisite: Guarantee1 should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees()[0].CusGuarantee.PK, guarantee1.CusGuarantee.PK);
			AssertEquals("Prerequisite: Guarantee2 should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees()[1].CusGuarantee.PK, guarantee2.CusGuarantee.PK);

			var outgoingMessage = GetNCTSFREDIMessage(nctsHeader);
			outgoingMessage.EM_Status = EDIMessage.Status.Received;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = TP5MessageTypeList.Codes.CC015C;
			nctsHeader.MovementHeader.Messages.Add(outgoingMessage);

			var incomingMessage = GetNCTSFREDIMessage(nctsHeader);
			nctsHeader.Messages.Add(incomingMessage);

			var messageDataObject = (NCTSMessageDataObject<Cc055CType>)incomingMessage.MessageDataObject;
			AssertEquals("There should be three message guarantees in response xml file", 3, messageDataObject.ResponseMessage.GuaranteeReference.Count);

			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals("PermitAppId of LastOutgoingMessage should be equal to MessageReferenceNumber", "111", FRPermitHelper.GetPermitAppIdForMessage(nctsHeader.GetOutgoingMessage(incomingMessage)));
			AssertEquals("MessageSubType of incoming message should be 055", TP5ResponseMessageSubTypeList.Codes.GuaranteeNotValid, nctsHeader.Messages.LastIncomingMessage.EM_MessageSubType);

			var permitHelper = new PermitTestDataHelper(Factory);
			var query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", $"NCTS write-off {FRPermitHelper.GetPermitAppIdForMessage(incomingMessage)} [{ExpectedMRN}]", nctsHeader.Messages.LastIncomingMessage.EM_MessageNum, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			var transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one write-off transaction created", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be confirmed", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);
			AssertEquals("Transaction CPL_Reference should be equal to BM_PaperlessInbondNum", nctsHeader.MovementHeader.BM_PaperlessInbondNum, transactionRequested[0].CPL_Reference);
			AssertEquals("Transaction CPL_AppId should be equal to EM_MessageNum", nctsHeader.Messages.LastIncomingMessage.EM_MessageNum, transactionRequested[0].CPL_AppId);
			AssertEquals("Transaction CPL_TranValue should counter confirmed transaction's amount of 50", 50m, transactionRequested[0].CPL_TranValue);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "NCTS departure for GRN1", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for this query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be deleted", PermitTransactionStatusList.Codes.Deleted, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "NCTS departure for GRN2", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for this query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be deleted", PermitTransactionStatusList.Codes.Deleted, transactionRequested[0].CPL_TransactionStatus);
		}
	}
}
