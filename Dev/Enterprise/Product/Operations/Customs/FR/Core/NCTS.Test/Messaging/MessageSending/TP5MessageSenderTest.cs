using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	sealed class TP5MessageSenderTest : TestCaseWithFactory
	{
		public void TestGetBuilderManager()
		{
			AssertType<TP5MessageBuilderManager>(sender.GetBuilderManager());
		}

		public void TestSendMessage()
		{
			var messageSendSuccessful = "Message sent successfully";
			var result = sender.Send();
			var message = nctsHeader.MovementHeader.Messages[0];

			AssertEquals(messageSendSuccessful, result);
			AssertType<FREDIMessage>(message);
			AssertEquals(FR.Business.MessageTypeList.Codes.TP5, (string)message.EM_MessageType);
		}

		public void TestForSequentialDeclarationGoodsItemNumbers()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill1 = nctsHeader.Bills.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();
			var bill3 = nctsHeader.Bills.AddNew();

			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 0);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill1, 1);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill2, 6);
			AddGoodsItemWithDeclarationGoodsItemNumber(bill2, 9);
			var goodsItem1 = bill3.GoodsItems.AddNew();
			goodsItem1.BY_LineNo = 7;

			var goodsItems = nctsHeader.GetGoodsItems();
			var goodsItemsNumbers = goodsItems.Select(g => g.BY_DeclarationGoodsItemNumber).ToArray();
			AssertContainsExactElementsInExactOrder("[PRE-CONDITION] The DeclarationGoodsItemNumber are not sequential.", new ZInt[] { 0, 1, 6, 9, 0 }, goodsItemsNumbers);

			sender.Send();
			goodsItemsNumbers = goodsItems.Select(g => g.BY_DeclarationGoodsItemNumber).ToArray();
			AssertContainsExactElementsInExactOrder("The DeclarationGoodsItemNumber are sequential.", new ZInt[] { 1, 2, 3, 4, 5 }, goodsItemsNumbers);

			void AddGoodsItemWithDeclarationGoodsItemNumber(NctsBill bill, ZInt number)
			{
				var goodsItem = bill.GoodsItems.AddNew();
				goodsItem.BY_DeclarationGoodsItemNumber = number;
			}
		}

		[TestDate]
		public void TestSettingValuationDate_CC015C()
		{
			objectToSend.MessageType = TP5MessageTypeList.Codes.CC015C;

			nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;

			sender.Send();

			AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);
		}

		[TestDate]
		public void TestSettingValuationDate_CC013C()
		{
			objectToSend.MessageType = TP5MessageTypeList.Codes.CC013C;

			sender.Send();

			AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);

			nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;

			sender.Send();

			AssertEquals(ZDateTime.BrettsBirthday, nctsHeader.MovementHeader.BM_ValuationDate);
		}

		[TestDate]
		public void TestSettingValuationDate_Other()
		{
			objectToSend.MessageType = TP5MessageTypeList.Codes.CC014C;

			sender.Send();

			AssertEquals(ZDateTime.Empty, nctsHeader.MovementHeader.BM_ValuationDate);
		}

		public void TestAddPermitIfApplicable()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var cusGuaranteeHeader = Factory.NewWithValidTestData<EU.Business.CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_Number = "GUA1";
			cusGuaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.COD;
			cusGuaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusGuaranteeHeader.CPH_SubType = "1";

			nctsHeader.Principal.E2_OA_Address = cusGuaranteeHeader.PermitHolder.MainAddress.PK;

			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondAmount = 10m;
			guarantee.PW_BondNumber = "GUA1";

			AssertEquals("Prerequisite: Guarantee should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees().First().CusGuarantee.PK, guarantee.CusGuarantee.PK);

			var transaction = cusGuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "Entry Number";
			transaction.CPL_TranValue = 100m;
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction.CPL_AppId = "Entry Reference";
			transaction.CPL_Comment = "Instruction Desc.";
			transaction.CPL_Procedure = "AAA";

			AssertEquals("Guarantee transactions count should be 1", 1, guarantee.CusGuarantee.CusGuaranteeLineTransactions.Count);

			CombineAssertions("Verify new guarantee transaction added after sending message", () =>
			{
				sender.Send();
				var guaranteeTransactions = guarantee.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Guarantee transactions count should be 2", 2, guaranteeTransactions.Count);

				var newTransaction = guaranteeTransactions[1];
				AssertEquals("Transaction Reference", "2595950308700261001902", newTransaction.CPL_Reference);
				AssertEquals("Transaction Comment", "NCTS departure 2595950308700261001902", newTransaction.CPL_Comment);
				AssertEquals("Transaction Category", PermitTransactionCategoryList.Codes.CUM, newTransaction.CPL_TransactionCategory);
				AssertEquals("Transaction Type", PermitTransactionTypeList.Codes.TRA, newTransaction.CPL_TransactionType);
				AssertEquals("Transaction Value", -10m, newTransaction.CPL_TranValue);
				AssertEquals("Transaction ID", "1", newTransaction.CPL_AppId);
				AssertEquals("Transaction Status", PermitTransactionStatusList.Codes.Pending, newTransaction.CPL_TransactionStatus);
				AssertEquals("Transaction Reference Line No.", 0, newTransaction.CPL_ReferenceNumberLine);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "2595950308700261001902";
			objectToSend = new TP5MessageSendingObject(nctsHeader);
			errorCollector = new ErrorCollector();
			sender = new TP5MessageSenderForTest(objectToSend, errorCollector);
		}

		ErrorCollector errorCollector;
		TP5MessageSendingObject objectToSend;
		NctsHeader nctsHeader;
		TP5MessageSenderForTest sender;
	}

	public class TP5MessageSenderForTest : TP5MessageSender
	{
		public TP5MessageSenderForTest(TP5MessageSendingObject tP5MessageSendingObject, ErrorCollector errorCollector) : base(tP5MessageSendingObject, errorCollector)
		{
		}

		public new MessageBuilderManager<TP5MessageSendingObject> GetBuilderManager() => base.GetBuilderManager();
	}
}
