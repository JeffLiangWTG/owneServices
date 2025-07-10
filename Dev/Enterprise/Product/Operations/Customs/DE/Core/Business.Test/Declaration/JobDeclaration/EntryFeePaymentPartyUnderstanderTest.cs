using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class EntryFeePaymentPartyUnderstanderTest : TestCaseWithFactory
	{
		public void TestNonEFGZMethodOfPayment_DefermentPartyIsOrgProxy()
		{
			var nonSpecialMethodsOfPayment = new ZString[] {
				UniversalReferenceConstants.MethodOfPaymentTypes.A,
				UniversalReferenceConstants.MethodOfPaymentTypes.C,
				UniversalReferenceConstants.MethodOfPaymentTypes.D
			};

			CombineAssertions("Anything other than E, F, G, Z should return true only if the deferment party is the OrgProxy.", () =>
			{
				foreach (var methodOfPayment in nonSpecialMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, string.Empty, string.Empty, string.Empty, string.Empty);
					CreateDefermentParty(declaration, defermentParty, "1", true);

					AssertShouldBrokerPayThisFee(declaration, true, true);
				}
			});
		}

		public void TestNonEFGZMethodOfPayment_DefermentPartyIsNotOrgProxy()
		{
			var nonSpecialMethodsOfPayment = new ZString[] {
				UniversalReferenceConstants.MethodOfPaymentTypes.A,
				UniversalReferenceConstants.MethodOfPaymentTypes.C,
				UniversalReferenceConstants.MethodOfPaymentTypes.D
			};

			CombineAssertions("Anything other than E, F, G, Z should return true only if the deferment party is the OrgProxy.", () =>
			{
				foreach (var methodOfPayment in nonSpecialMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, string.Empty, string.Empty, string.Empty, string.Empty);
					CreateDefermentParty(declaration, defermentParty, "1", false);

					AssertShouldBrokerPayThisFee(declaration, false, false);
				}
			});
		}

		public void TestNonEFGZMethodOfPayment_NullBranch()
		{
			var nonSpecialMethodsOfPayment = new ZString[] {
				UniversalReferenceConstants.MethodOfPaymentTypes.A,
				UniversalReferenceConstants.MethodOfPaymentTypes.C,
				UniversalReferenceConstants.MethodOfPaymentTypes.D
			};

			CombineAssertions("If the branch is null for some reason we return false because the deferment party can't be the org proxy.", () =>
			{
				foreach (var methodOfPayment in nonSpecialMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, string.Empty, string.Empty, string.Empty, string.Empty);
					CreateDefermentParty(declaration, defermentParty, "1", true);

					declaration.JE_GB = ZGuid.Empty;

					AssertShouldBrokerPayThisFee(declaration, false, false);
				}
			});
		}

		public void TestEmptyMethodOfPayment()
		{
			var declaration = CreateDeclaration(string.Empty, declarant, "1", string.Empty, string.Empty);
			CreateDeclarant(declaration, account10, "1", true);

			CombineAssertions("Empty method of payment should return false no matter what.", () =>
			{
				AssertShouldBrokerPayThisFee(declaration, false, false);
			});
		}

		public void TestEFGZMethodOfPayment_NeitherPartyIsBroker()
		{
			CombineAssertions("Should always be false when neither party is broker.", () =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account10, "1", false);
					CreateRepresentative(declaration, account15, "1", false);

					AssertShouldBrokerPayThisFee(declaration, false, false);
				}
			});
		}

		public void TestEFGZMethodOfPayment_EmptySecondAccount()
		{
			CombineAssertions(() =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					// 10
					var declaration = CreateDeclaration(methodOfPayment, declarant, "1", string.Empty, string.Empty);
					var originalOrgProxy = declaration.Branch.GB_OH_OrgProxy;
					CreateDeclarant(declaration, account10, "1", true);
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration.Branch.GB_OH_OrgProxy = originalOrgProxy; // So account 1 will no longer be the broker.
					AssertShouldBrokerPayThisFee(declaration, false, false);

					// 15
					declaration = CreateDeclaration(methodOfPayment, declarant, "1", string.Empty, string.Empty);
					CreateDeclarant(declaration, account15, "1", true);
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration.Branch.GB_OH_OrgProxy = originalOrgProxy; // So account 1 will no longer be the broker.
					AssertShouldBrokerPayThisFee(declaration, true, false);

					// 20
					declaration = CreateDeclaration(methodOfPayment, declarant, "1", string.Empty, string.Empty);
					CreateDeclarant(declaration, account20, "1", true);
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration.Branch.GB_OH_OrgProxy = originalOrgProxy; // So account 1 will no longer be the broker.
					AssertShouldBrokerPayThisFee(declaration, false, true);
				}
			});
		}

		public void TestEFGZMethodOfPayment_Accounts10And15()
		{
			CombineAssertions(() =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, declarant, "1", declarant, "2");
					var declarantOrg = CreateDeclarant(declaration, account10, "1", true);
					CreateOrgCusAccount(declarantOrg, account15, "2");
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account10, "1", true);
					CreateRepresentative(declaration, account15, "1", false);
					AssertShouldBrokerPayThisFee(declaration, true, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account10, "1", false);
					CreateRepresentative(declaration, account15, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account10, "1", false);
					CreateRepresentative(declaration, account15, "1", true);
					AssertShouldBrokerPayThisFee(declaration, false, true);
				}
			});
		}

		public void TestEFGZMethodOfPayment_Accounts10And20()
		{
			CombineAssertions(() =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, declarant, "1", declarant, "2");
					var declarantOrg = CreateDeclarant(declaration, account10, "1", true);
					CreateOrgCusAccount(declarantOrg, account20, "2");
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account10, "1", true);
					CreateRepresentative(declaration, account20, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account10, "1", false);
					CreateRepresentative(declaration, account20, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account10, "1", false);
					CreateRepresentative(declaration, account20, "1", true);
					AssertShouldBrokerPayThisFee(declaration, true, false);
				}
			});
		}

		public void TestEFGZMethodOfPayment_Accounts15And10()
		{
			CombineAssertions(() =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, declarant, "1", declarant, "2");
					var declarantOrg = CreateDeclarant(declaration, account15, "1", true);
					CreateOrgCusAccount(declarantOrg, account10, "2");
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account15, "1", true);
					CreateRepresentative(declaration, account10, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account15, "1", false);
					CreateRepresentative(declaration, account10, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account15, "1", false);
					CreateRepresentative(declaration, account10, "1", true);
					AssertShouldBrokerPayThisFee(declaration, true, false);
				}
			});
		}

		public void TestEFGZMethodOfPayment_Accounts15And20()
		{
			CombineAssertions(() =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, declarant, "1", declarant, "2");
					var declarantOrg = CreateDeclarant(declaration, account15, "1", true);
					CreateOrgCusAccount(declarantOrg, account20, "2");
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account15, "1", true);
					CreateRepresentative(declaration, account20, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account15, "1", false);
					CreateRepresentative(declaration, account20, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account15, "1", false);
					CreateRepresentative(declaration, account20, "1", true);
					AssertShouldBrokerPayThisFee(declaration, true, false);
				}
			});
		}

		public void TestEFGZMethodOfPayment_Accounts20And10()
		{
			CombineAssertions(() =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, declarant, "1", declarant, "2");
					var declarantOrg = CreateDeclarant(declaration, account20, "1", true);
					CreateOrgCusAccount(declarantOrg, account10, "2");
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account20, "1", true);
					CreateRepresentative(declaration, account10, "1", false);
					AssertShouldBrokerPayThisFee(declaration, true, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account20, "1", false);
					CreateRepresentative(declaration, account10, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account20, "1", false);
					CreateRepresentative(declaration, account10, "1", true);
					AssertShouldBrokerPayThisFee(declaration, false, true);
				}
			});
		}

		public void TestEFGZMethodOfPayment_Accounts20And15()
		{
			CombineAssertions(() =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, declarant, "1", declarant, "2");
					var declarantOrg = CreateDeclarant(declaration, account20, "1", true);
					CreateOrgCusAccount(declarantOrg, account15, "2");
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account20, "1", true);
					CreateRepresentative(declaration, account15, "1", false);
					AssertShouldBrokerPayThisFee(declaration, true, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account20, "1", false);
					CreateRepresentative(declaration, account15, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", representative, "1");
					CreateDeclarant(declaration, account20, "1", false);
					CreateRepresentative(declaration, account15, "1", true);
					AssertShouldBrokerPayThisFee(declaration, false, true);
				}
			});
		}

		public void TestEFGZMethodOfPayment_DefermentPartyAlsoWorks()
		{
			CombineAssertions(() =>
			{
				foreach (var methodOfPayment in MethodOfPaymentHelper.DeferredMethodsOfPayment)
				{
					var declaration = CreateDeclaration(methodOfPayment, defermentParty, "1", defermentParty, "2");
					var defermentPartyOrg = CreateDefermentParty(declaration, account20, "1", true);
					CreateOrgCusAccount(defermentPartyOrg, account15, "2");
					AssertShouldBrokerPayThisFee(declaration, true, true);

					declaration = CreateDeclaration(methodOfPayment, defermentParty, "1", representative, "1");
					CreateDefermentParty(declaration, account20, "1", true);
					CreateRepresentative(declaration, account15, "1", false);
					AssertShouldBrokerPayThisFee(declaration, true, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", defermentParty, "1");
					CreateDeclarant(declaration, account20, "1", false);
					CreateDefermentParty(declaration, account15, "1", false);
					AssertShouldBrokerPayThisFee(declaration, false, false);

					declaration = CreateDeclaration(methodOfPayment, declarant, "1", defermentParty, "1");
					CreateDeclarant(declaration, account20, "1", false);
					CreateDefermentParty(declaration, account15, "1", true);
					AssertShouldBrokerPayThisFee(declaration, false, true);
				}
			});
		}

		#region Object Creation Helper Methods

		JobDeclaration CreateDeclaration(string methodOfPayment, string account1Type, string account1Number, string account2Type, string account2Number)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_MethodOfPayment = methodOfPayment;
			declaration.JE_PaymentMethod = account1Type;
			declaration.ZG_VATDeferType = account2Type;
			declaration.JE_DefermentAccountNumber = account1Number;
			declaration.ZG_VATDeferNumber = account2Number;

			return declaration;
		}

		OrgHeader CreateDeclarant(JobDeclaration declaration, string accountCode, string accountNumber, bool shouldBeOrgProxy)
		{
			var org = CreateOrg(declaration, accountCode, accountNumber, shouldBeOrgProxy, "Declarant");
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;

			return org;
		}

		OrgHeader CreateRepresentative(JobDeclaration declaration, string accountCode, string accountNumber, bool shouldBeOrgProxy)
		{
			var org = CreateOrg(declaration, accountCode, accountNumber, shouldBeOrgProxy, "Representative");
			declaration.JE_OA_Representative = org.MainAddress.PK;

			return org;
		}

		OrgHeader CreateDefermentParty(JobDeclaration declaration, string accountCode, string accountNumber, bool shouldBeOrgProxy)
		{
			var org = CreateOrg(declaration, accountCode, accountNumber, shouldBeOrgProxy, "DefermentParty");
			declaration.DefermentPartyDocAddress.OrganisationPK = org.PK;

			return org;
		}

		OrgHeader CreateOrg(JobDeclaration declaration, string accountCode, string accountNumber, bool shouldBeOrgProxy, string name)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			CreateOrgCusAccount(org, accountCode, accountNumber);

			if (shouldBeOrgProxy)
			{
				declaration.Branch.GB_OH_OrgProxy = org.PK;
			}

			return org;
		}

		static void CreateOrgCusAccount(OrgHeader org, string code, string accountNumber)
		{
			var account = org.DefermentAccountNumberCollection.AddNew();
			account.CZ_Code = code;
			account.CZ_Account = accountNumber;
		}

		#endregion

		#region Assertions

		static void AssertShouldBrokerPayThisFee(JobDeclaration declaration, bool expectedResultForVat, bool expectedResultForDuty)
		{
			AssertShouldBrokerPayThisFee(declaration, vat, expectedResultForVat);
			AssertShouldBrokerPayThisFee(declaration, nonVat, expectedResultForDuty);
		}

		static void AssertShouldBrokerPayThisFee(JobDeclaration declaration, string feeCode, bool expectedResult)
		{
			var logger = new DummyLogger();
			var understander = new EntryFeePaymentPartyUnderstander(declaration);
			var result = understander.ShouldBrokerPayThisFee(feeCode, declaration.ZG_MethodOfPayment, logger);

			var message = $@"
ZG_MethodOfPayment: {declaration.ZG_MethodOfPayment}
JE_PaymentMethod: {declaration.JE_PaymentMethod}
ZG_VATDeferType: {declaration.ZG_VATDeferType}
JE_DefermentAccountNumber: {declaration.JE_DefermentAccountNumber}
ZG_VATDeferNumber: {declaration.ZG_VATDeferNumber}
OrgProxy: {declaration.Branch?.OrgProxy.OH_FullName}
FeeCode: {feeCode}";

			AssertEquals(message, expectedResult, result);
		}

		#endregion

		#region Codes

		const string declarant = DeferralPaymentPartyList.Codes.Declarant;
		const string representative = DeferralPaymentPartyList.Codes.Representative;
		const string defermentParty = DeferralPaymentPartyList.Codes.DefermentParty;
		const string account10 = OrgCusAccountCodeList.Codes.ImportDutiesOneMonth;
		const string account15 = OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT;
		const string account20 = OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity;
		const string vat = Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat;
		const string nonVat = Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty;

		#endregion
	}
}
