using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	public class TransactionMatchLinkGroupCriticalValidationTest : CriticalValidationTest<TransactionMatchLinkGroup>
	{
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			return new List<TestCaseDefinitionWithDelegate_Obsolete>
			{
				new TestCaseDefinitionWithDelegate_Obsolete(
					"Balanced MatchGroup"
					, factory => GetBalancedMatchGroup(factory)),
				new TestCaseDefinitionWithDelegate_Obsolete(
					"UnBalanced MatchGroup"
					, factory => GetUnBalancedMatchGroup(factory)
					,true
					, CriticalValidationErrorType.TransactionMatchGroupOutOfBalance_2
					, "Match Group  is out of balance"
					, "Transaction Match Group Balance = "),
				new TestCaseDefinitionWithDelegate_Obsolete(
					"AllTranscationHeaderSameCompany"
					, factory => GetMatchGroupWithTransactionHeaderCompaniesAreSame(factory)),
				new TestCaseDefinitionWithDelegate_Obsolete(
					"NotAllTranscationHeaderSameCompany"
					, factory => GetMatchGroupWithTransactionHeaderCompaniesAreNotSame(factory)
					, onSavingCheckShouldFail: true
					, errorType: CriticalValidationErrorType.TransactionMatchGroupNotAllHeaderInSameCompanies_1
					, userErrorMessage: CriticalValidationMessageTemplate.TransactionMatchGroupNotAllHeaderInSameCompaniesErrorMessage
					, expectedTechDetailsInThisOrderIntoErrorMessage:  "only return different company matchLinks"),
			};
		}

		TransactionMatchLinkGroup GetBalancedMatchGroup(BusinessObjectFactory factory)
		{
			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(factory);
			TransactionMatchLink link1 = group.AddNew();
			TransactionMatchLink link2 = group.AddNew();
			link1.AP_Amount = 5m;
			link2.AP_Amount = -5m;
			AssertEquals("MatchLinkAmountsBalanceToZero", 0M, group.GetBalance());

			return group;
		}
		TransactionMatchLinkGroup GetUnBalancedMatchGroup(BusinessObjectFactory factory)
		{
			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(factory);
			TransactionMatchLink link1 = group.AddNew();
			TransactionMatchLink link2 = group.AddNew();
			link1.AP_Amount = 10m;
			link2.AP_Amount = -5m;
			AssertNotEquals("Should not be MatchLinkAmountsBalanceToZero", 0M, group.GetBalance());

			return group;
		}

		TransactionMatchLinkGroup GetMatchGroupWithTransactionHeaderCompaniesAreSame(BusinessObjectFactory factory)
		{
			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(factory);

			TransactionMatchLink link1 = group.AddNew();
			link1.AP_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;

			TransactionMatchLink link2 = group.AddNew();
			link2.AP_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;

			Factory.Save();
			link1.TransactionHeader.AH_GC = new Guid("DCEC02FD-84F3-4A05-90EF-C70162B49F0E");
			link2.TransactionHeader.AH_GC = new Guid("DCEC02FD-84F3-4A05-90EF-C70162B49F0E");
			AssertEquals("TransactionHeaderCompanies should all be Same", link1.TransactionHeader.AH_GC, link2.TransactionHeader.AH_GC);

			return group;
		}

		TransactionMatchLinkGroup GetMatchGroupWithTransactionHeaderCompaniesAreNotSame(BusinessObjectFactory factory)
		{
			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(factory);

			TransactionMatchLink link1 = group.AddNew();
			link1.AP_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;

			TransactionMatchLink link2 = group.AddNew();
			link2.AP_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;

			Factory.Save();
			link1.TransactionHeader.AH_GC = new Guid("D6DA43E6-7DD3-450B-BEC0-56CE6AE5DE66");
			link2.TransactionHeader.AH_GC = new Guid("DCEC02FD-84F3-4A05-90EF-C70162B49F0E");
			AssertNotEquals("TransactionHeaderCompanies should not be Same", link1.TransactionHeader.AH_GC, link2.TransactionHeader.AH_GC);

			return group;
		}

		protected override ISupportCriticalValidation GetCriticalValidationParent()
		{
			return null;
		}
	}
}