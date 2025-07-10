using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration.Testing
{
	public class NctsMessageGeneratorHelperTests : TestCaseWithFactory
	{
		public void TestAddPermitRecords()
		{
			Business.Testing.NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Number = "GUA1";
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader1.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			var transaction1 = guaranteeHeader1.CusGuaranteeLineTransactions.AddNew();
			transaction1.CPL_Reference = "Entry Number";
			transaction1.CPL_TranValue = 100m;
			transaction1.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction1.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction1.CPL_AppId = "Entry Reference";
			transaction1.CPL_Comment = "Instruction Desc.";
			transaction1.CPL_Procedure = "AAA";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.FillWithValidTestData();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();

			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);

				var message = Factory.New<TestEdiMessage>();
				nctsHeader.Messages.Add(message);
				NctsMessageGeneratorHelper.AddPermitRecords(nctsHeader, message);
				Factory.Save();

				var transactionsGuarantee1 = guarantee1.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee1", 2, transactionsGuarantee1.Count);

				var transaction = transactionsGuarantee1[1];
				AssertEquals("Guarantee1's Transaction Reference", nctsHeader.BH_JobReference, transaction.CPL_Reference);
				AssertEquals("Guarantee1's Transaction Comment", "NCTS departure " + nctsHeader.LocalReferenceNumber, transaction.CPL_Comment);
				AssertEquals("Guarantee1's Transaction Category", PermitTransactionCategoryList.Codes.CUM, transaction.CPL_TransactionCategory);
				AssertEquals("Guarantee1's Transaction Type", PermitTransactionTypeList.Codes.TRA, transaction.CPL_TransactionType);
				AssertEquals("Guarantee1's Transaction Value", -guarantee1.PW_BondAmount, transaction.CPL_TranValue);
				AssertEquals("Guarantee1's Transaction ID", "1", transaction.CPL_AppId);
				AssertEquals("Guarantee1's Transaction Status", PermitTransactionStatusList.Codes.Pending, transaction.CPL_TransactionStatus);
				AssertEquals("Guarantee1's Transaction Reference Line No.", 0, transaction.CPL_ReferenceNumberLine);
			});
		}
	}
}
