using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DEPDATSender))]
	sealed class DEPDATSenderTest : NctsHeaderSenderTest
	{
		public void TestPreSend()
		{
			var orgAddress = NCTSTestHelper.CreateOrgAddressForTest(Factory, "WAP", "7");
			orgAddress.Header.OH_IsWarehouseClient = true;
			nctsHeader.MovementHeader.BM_OA_WarehouseAddress = orgAddress.PK;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			nctsHeader.MessageInitiator = messageInitiator;
			GetNctsDeclarationSender().Send();
			AssertEquals("WarehouseNctsHeaderSender.PreSend Invoked", true, messageInitiator.InvalidOperationText.Contains("Error - Cannot Import Order"));
		}

		public void TestGuaranteeTransactions()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			guaranteeHeader.CPH_Balance = 1000.0m;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._1;
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 145.0m;
			guarantee.PW_Override = true;
			guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;
			guarantee.PW_GuaranteeDescription = "Should have transactions";

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondType = NctsGuaranteeTypeList.Codes._8;
			guarantee2.PW_BondNumber = "GUA1";
			guarantee2.PW_BondAmount = 150.0m;
			guarantee.PW_Override = true;
			guarantee2.PW_CPH_Guarantee = guaranteeHeader.PK;
			guarantee.PW_GuaranteeDescription = "Should NOT have transactions";

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			nctsHeader.MessageInitiator = messageInitiator;
			CombineAssertions(() => {
				AssertEquals("Initial situation GUA1", 1, guaranteeHeader.GetTransactions().Count());
				GetNctsDeclarationSender().Send();
				AssertEquals("After send GUA1", 2, guaranteeHeader.GetTransactions().Count());
			});
		}

		[TestDate(2024, 11, 12)]
		public void TestPreSend_ShouldSetValuationDate()
		{
			AssertEquals("Precondition", ZDateTime.Empty, nctsHeader.MovementHeader.BM_ValuationDate);

			GetNctsDeclarationSender().Send();

			AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			messagingObject.MessageType = NctsMessageTypeList.Codes.DEPDAT;
		}

		protected override ZString ExpectedMessageSubType => NctsMessageSubTypeList.Codes.DepartureMessage;

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETPDD);

		protected override ZString ExpectedLogbookRegistrationNumber => ZString.Empty;

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override ZString ExpectedCustomsStatus => ZString.Empty;

		protected override ZString MovementType => NctsMovementType.Codes.Departure;

		protected override NctsHeaderSender GetNctsDeclarationSender() => new DEPDATSender(messagingObject);
	}
}
