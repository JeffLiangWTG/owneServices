using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC045A;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC045AProcessorTest : DTBaseProcessorTest<Cc045AType>
	{
		public void TestTransactionAdded()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>();
			var guaranteeHeader1 = GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 200m);
			Factory.Save();

			cusGuaranteeHeaderList.Add(guaranteeHeader1);
			var movementHeader = Factory.New<NctsHeader>();
			movementHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			movementHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";
			var header = helper.GetProcessReceivedMessageHeader("DT045A_MESSAGE.xml", cusGuaranteeHeaderList, org1, movementHeader);

			AssertEquals(true, header.BH_JobReference.Equals("NCT00050167"));
			AssertEquals(true, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage(header.Messages.LastOutgoingMessage).Equals("111"));
			AssertEquals(2, header.Messages.Count);
			AssertEquals("045", header.Messages.LastIncomingMessage.EM_MessageType);

			var query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "NCTS write-off NCT00050167 [NCTS0001]", header.Messages.LastIncomingMessage.EM_MessageNum, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			var transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactionRequested.Length);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			var bond = header.MovementHeader.Guarantees.Cast<CusBondDetail>().FirstOrDefault(x => x.PW_BondNumber == "19860101" && x.PW_BondAmount == 150m);
			AssertEquals(header.BH_JobReference, transactionRequested[0].CPL_Reference);
			AssertEquals(header.Messages.LastIncomingMessage.EM_MessageNum, transactionRequested[0].CPL_AppId);
			AssertEquals(bond.PW_BondAmount, transactionRequested[0].CPL_TranValue);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactionRequested.Length);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactionRequested.Length);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);
		}

		public void TestLogIfErrorUpdatingGuarantee()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>();
			var guaranteeHeader1 = GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 200m);
			Factory.Save();

			cusGuaranteeHeaderList.Add(guaranteeHeader1);
			var movementHeader = Factory.New<NctsHeader>();
			movementHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			movementHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";
			var cusGuarantee = movementHeader.MovementHeader.Guarantees.AddNew();
			cusGuarantee.PW_BondNumber = "19860102";
			var header = helper.GetProcessReceivedMessageHeader("DT045A_MESSAGE.xml", cusGuaranteeHeaderList, org1, movementHeader, "19860102");

			AssertEquals(true, header.BH_JobReference.Equals("NCT00050167"));
			AssertEquals("045", header.Messages.LastIncomingMessage.EM_MessageType);

			var errorMessageNoGuaranteeLink = "Guarantee was not written off for NCT00050167 because 19860102 does not refer to a guarantee managed by CW1.";
			Assert("NctsHeader should have an error as cusGuarantee.PW_BondNumber is not empty, but has no guantee link to that code.", header.Logs.Find(l => l.SL_SE_NKEvent == Events.DeclarationHasErrors.Code && l.SL_Reference == errorMessageNoGuaranteeLink).Any());
		}

		public void TestLogIfErrorUpdatingGuarantee_WhenBursting()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var guaranteeHeader1 = GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 200m);
			AssertEquals("Precondition: we have 40m remaining.", 40m, guaranteeHeader1.CPH_Balance);
			AssertEquals(3, guaranteeHeader1.CusGuaranteeLineTransactions.Count);
			Factory.Save();

			var movementHeader = Factory.New<NctsHeader>();
			movementHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			movementHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";
			movementHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var cusGuarantee = movementHeader.MovementHeader.Guarantees.Cast<FRNctsGuarantee>().FirstOrDefault() ?? movementHeader.GetEffectiveGuarantees().AddNew();
			cusGuarantee.PW_BondNumber = "19860101";
			cusGuarantee.PW_BondAmount = 180m;

			var header = helper.GetProcessReceivedMessageHeader("DT045A_MESSAGE.xml", new List<CusGuaranteeHeader>(), org1, movementHeader);

			AssertEquals(true, header.BH_JobReference.Equals("NCT00050167"));
			AssertEquals("045", header.Messages.LastIncomingMessage.EM_MessageType);

			var errorMessageGuaranteeIsExploded = "The guarantee 19860101 total amount will be exceeded by 20. The total is 200 and the available is 40.";
			Assert("NctsHeader should have an error as cusGuarantee.PW_BondAmount is 180m, and balance is 40m, if we add 180 on 40, the total amount will explode by 20.", header.Logs.Find(l => l.SL_SE_NKEvent == Events.DeclarationHasErrors.Code && l.SL_Reference == errorMessageGuaranteeIsExploded).Any());
			AssertEquals("count.", 1, header.Logs.Find(l => l.SL_SE_NKEvent == Events.DeclarationHasErrors.Code).Count());
			AssertEquals("No transaction was added.", 3, guaranteeHeader1.CusGuaranteeLineTransactions.Count);
		}

		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT045A_MESSAGE_TEMPLATE.xml").Replace("{IrrHEA1020}", "0"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.GoodsWrittenOff,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.NoIrregularities,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Goods Written-off</p>
<p>New detailed departure status: No Irregularities</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Written off date: 23/01/2020</p>
<p>Irregularities: No</p>"
				};

				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT045A_MESSAGE_TEMPLATE.xml").Replace("{IrrHEA1020}", "1"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.GoodsWrittenOff,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Irregularities,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Goods Written-off</p>
<p>New detailed departure status: Irregularities</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Written off date: 23/01/2020</p>
<p>Irregularities: Yes</p>"
				};
			}
		}

		CusGuaranteeHeader GenerateGuaranteeHeader(OrgHeader org1, ZString pW_BondNumber, ZString transactionReference, ZString transactionAppId, ZDecimal valueTransaction)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = pW_BondNumber;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			var permit1TransLine1 = guaranteeHeader.AddTransaction(transactionReference, "CMT-TO-CONF", transactionAppId, "", valueTransaction, 0, PermitTransactionStatusList.Codes.Pending, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			var permit1TransLine2 = guaranteeHeader.AddTransaction(transactionReference, "CMT-CON", transactionAppId, "", -10, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			var permit1TransLine3 = guaranteeHeader.AddTransaction(transactionReference, "Liability Transaction", transactionAppId, "", -150, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader.CPH_Balance += (permit1TransLine1.CPL_TranValue + permit1TransLine2.CPL_TranValue + permit1TransLine3.CPL_TranValue);
			AssertEquals(valueTransaction, guaranteeHeader.CPH_Calc_OpeningBalance);
			return guaranteeHeader;
		}
	}
}
