using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class DeferralPaymentPartyListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetDeferralAccountNumberList()
		{
			declaration.JE_OA_DeclarantAddress = deferralParty.MainAddress.PK;
			declaration.DefermentPartyDocAddress.OrganisationPK = Factory.CreateDeferralParty("10", "2222").PK;
			declaration.JE_OA_Representative = Factory.CreateDeferralParty("10", "3333").MainAddress.PK;
			declaration.JE_OA_BuyingAgentAddress = Factory.CreateDeferralParty("10", "4444").MainAddress.PK;

			NUnit.Framework.Assert.Multiple(() =>
			{
				declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;
				NUnit.Framework.Assert.That(declaration.Lookups.DefermentAccountNumberList.CodesAsString, Is.EqualTo(@"1111"), "Declarant-DEC");

				declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.DefermentParty;
				NUnit.Framework.Assert.That(declaration.Lookups.DefermentAccountNumberList.CodesAsString, Is.EqualTo("2222"), "DefermentParty-DEF");

				declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Representative;
				NUnit.Framework.Assert.That(declaration.Lookups.DefermentAccountNumberList.CodesAsString, Is.EqualTo("3333"), "Representative-REP");

				declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.RepresentedParty;
				NUnit.Framework.Assert.That(declaration.Lookups.DefermentAccountNumberList.CodesAsString, Is.EqualTo("4444"), "RepresentedParty-RPP");
			});
		}

		[ExpectNoExceptions]
		public void TestGetDeferralAccountNumberList_RemoveDuplicate()
		{
			deferralParty.AddDefermentAccountNumber("10", "1111");
			declaration.JE_OA_DeclarantAddress = deferralParty.MainAddress.PK;
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;

			NUnit.Framework.Assert.That(declaration.Lookups.DefermentAccountNumberList.ElementsAsString, Is.EqualTo(@"1111 - Account Type: 10"), "Declarant-DEC, Removes duplicate");
		}

		[ExpectNoExceptions]
		public void TestGetDeferralAccountNumberList_ExcludeDeclarantVATWithoutSecurity()
		{
			deferralParty.AddDefermentAccountNumber("20", "2222");
			declaration.JE_OA_DeclarantAddress = deferralParty.MainAddress.PK;
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;

			NUnit.Framework.Assert.That(declaration.Lookups.DefermentAccountNumberList.ElementsAsString, Is.EqualTo(@"1111 - Account Type: 10"));
		}

		[ExpectNoExceptions]
		public void TestGetDeferralAccountNumberList_IncludeDeclarantVATWithoutSecurity()
		{
			//Do not include account 20 when import VAT claim back is YES.
			deferralParty.AddDefermentAccountNumber("20", "2222");
			declaration.ImporterDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var import_orgImpAddInfo = (DEOrgImpAddInfo)declaration.Importer.CountryData.ImpAddInfo;
			import_orgImpAddInfo.ZO_VATClaimBack = YesNoList.Codes.Yes;
			declaration.JE_OA_DeclarantAddress = deferralParty.MainAddress.PK;
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;

			NUnit.Framework.Assert.That(declaration.Lookups.DefermentAccountNumberList.ElementsAsString, Is.EqualTo(@"1111 - Account Type: 10"));

			//Include account 20 when declarant VAT claim back is YES.
			var declarant_orgImpAddInfo = (DEOrgImpAddInfo)deferralParty.CountryData.ImpAddInfo;
			declarant_orgImpAddInfo.ZO_VATClaimBack = YesNoList.Codes.Yes;
			NUnit.Framework.Assert.That(declaration.Lookups.DefermentAccountNumberList.ElementsAsString, Is.EqualTo(@"1111 - Account Type: 10
2222 - Account Type: 20"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			deferralParty = Factory.CreateDeferralParty("10", "1111");
			declaration = Factory.New<JobDeclaration>();
		}

		OrgHeader deferralParty;
		JobDeclaration declaration;
	}
}
