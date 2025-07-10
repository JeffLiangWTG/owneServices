using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class VATDeferStrategyTest : TestCaseWithFactory
	{
		public void TestPaymentMethodShouldBeDefaultedWhenOrgnisationChanged()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			var euOrgImpAddInfo = EUOrgImpAddInfo.Get(importer, declaration.CountryCode);
			euOrgImpAddInfo.Deserialise();
			var otherDeferType = euOrgImpAddInfo.ZO_OtherDeferType;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("It should default JE_PaymentMethod from Importer's ImpAddInfo.ZO_OtherDeferType when orgnisation changed.", otherDeferType, declaration.JE_PaymentMethod);  // Different countries have different list, we should assert it is A or B etc.
		}

		public virtual void TestDefermentAccountNumberShouldBeDefaultedWhenOrganisationChanged()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("It should default JE_DefermentAccountNumber from Importer's DAN when orgnisation changed.", "1000", declaration.JE_DefermentAccountNumber);
		}

		public virtual void TestDefermentAccountNumberShouldBeDefaultedWhenPaymentMethodChanged()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			declaration.JE_OH_Importer = importer.PK;

			var paymentMethod = declaration.JE_PaymentMethod;
			declaration.JE_PaymentMethodInfo.ClearValue();
			declaration.JE_DefermentAccountNumberInfo.ClearValue();
			declaration.JE_PaymentMethod = paymentMethod;  // Because valid payment methods vary from countries, it's not wise to set it as A or B etc.
			AssertEquals("It should default JE_DefermentAccountNumber from Importer's DAN when JE_PaymentMethod changed.", "1000", declaration.JE_DefermentAccountNumber);
		}

		public virtual void TestVATDeferNumberShouldBeDefaultedWhenOrganisationChanged()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("It should default ZG_VATDeferNumber from Importer's DAN when Organisation changed.", "1000", declaration.ZG_VATDeferNumber);
		}

		public virtual void TestVATDeferNumberShouldBeDefaultedWhenVATDeferTypeChanged()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			declaration.JE_OH_Importer = importer.PK;

			var vatDeferType = declaration.ZG_VATDeferType;
			declaration.ZG_VATDeferTypeInfo.ClearValue();
			declaration.JE_DefermentAccountNumberInfo.ClearValue();
			declaration.ZG_VATDeferType = vatDeferType;
			AssertEquals("It should default ZG_VATDeferNumber from Importer's DAN when ZG_VATDeferType changed.", "1000", declaration.ZG_VATDeferNumber);
		}

		public virtual void TestPaymentMethodSourceDependencies()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "2000");
			var euOrgImpAddInfo = EUOrgImpAddInfo.Get(importer, declaration.CountryCode);
			euOrgImpAddInfo.Deserialise();
			euOrgImpAddInfo.ZO_OtherDeferType = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;
			Factory.Save();

			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertEquals("GetPaymentMethodSource from the importer.", "1000", declaration.JE_DefermentAccountNumber);

			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountStandingAuthority;
			AssertEquals("GetPaymentMethodSource from the importer.", "1000", declaration.JE_DefermentAccountNumber);

			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration;
			AssertEquals("GetPaymentMethodSource from the importer.", "1000", declaration.JE_DefermentAccountNumber);

			declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			AssertEquals("GetPaymentMethodSource from the declarant.", "2000", declaration.JE_DefermentAccountNumber);

			declaration.JE_PaymentMethod = "";
			AssertNullOrEmpty("Deferment account number should be cleared as the payment method is cleared.", declaration.JE_DefermentAccountNumber);
		}

		public void TestDefermentAccountNumberOnInvalidPaymentMethodSet()
		{
			var (declaration, _) = CreateDeclarationAndImporter();

			declaration.JE_DefermentAccountNumber = "LOL";
			declaration.JE_PaymentMethod = "X";
			AssertEquals("We do nothing when the payment method is set as invalid.", ExpectedDefermentAccountNumberForInvalidMethodOfPayment, declaration.JE_DefermentAccountNumber);
		}

		protected virtual string ExpectedDefermentAccountNumberForInvalidMethodOfPayment => "LOL";

		public void TestGetPaymentMethodSourceWrapper()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			importer.OH_FullName = "IMPORTER";
			declaration.JE_OH_Importer = importer.PK;

			var declarant = CreateDeclarant();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			declaration.JE_PaymentMethod = "B";
			AssertPaymentMethodSourceOrganisation(declaration, importer);

			declaration.JE_PaymentMethod = "A";
			var expectedSourceOrgForPaymentMethodA = ExpectDeclarantAsSourceOrgForPaymentMethodA ? declaration.Declarant.Header : importer;
			AssertPaymentMethodSourceOrganisation(declaration, expectedSourceOrgForPaymentMethodA);
		}

		void AssertPaymentMethodSourceOrganisation(JobDeclaration declaration, OrgHeader expectedOrg)
		{
			var actualOrg = declaration.VATDeferStrategy.GetPaymentMethodSourceWrapper();

			CombineAssertions($"GetVATDeferTypeSource, when Declaration Payment Method is {declaration.JE_PaymentMethod}", () =>
			{
				AssertEquals("PK", expectedOrg.PK, actualOrg.PK);
				AssertEquals("Full Name", expectedOrg.OH_FullName, actualOrg.OH_FullName);
			});
		}

		protected virtual bool ExpectDeclarantAsSourceOrgForPaymentMethodA => true;

		public virtual void TestVATDeferTypeSourceDependencies()
		{
			var (declaration, importer) = CreateDeclarationAndImporter();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "2000");
			var euOrgImpAddInfo = EUOrgImpAddInfo.Get(importer, declaration.CountryCode);
			euOrgImpAddInfo.Deserialise();
			euOrgImpAddInfo.ZO_OtherDeferType = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;
			Factory.Save();

			declaration.ZG_VATDeferType = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertEquals("GetVATDeferTypeSource from the importer.", "1000", declaration.ZG_VATDeferNumber);

			declaration.ZG_VATDeferType = DefermentMethodList.Codes.ConsigneesAccountStandingAuthority;
			AssertEquals("GetVATDeferTypeSource from the importer.", "1000", declaration.ZG_VATDeferNumber);

			declaration.ZG_VATDeferType = DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration;
			AssertEquals("GetVATDeferTypeSource from the importer.", "1000", declaration.ZG_VATDeferNumber);

			declaration.ZG_VATDeferType = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			AssertEquals("GetVATDeferTypeSource from the declarant.", "2000", declaration.ZG_VATDeferNumber);
		}

		public void TestVatDutyDeferrer()
		{
			var declaration = Factory.New<JobDeclaration>();

			var declarant = declaration.Declarant;
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1234");

			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "4321");

			declaration.ZG_VATDeferNumber = "";
			declaration.ZG_VATDeferType = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			AssertEquals("1234", declaration.ZG_VATDeferNumber);

			declaration.ZG_VATDeferNumber = "";
			declaration.ZG_VATDeferType = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			AssertEquals("4321", declaration.ZG_VATDeferNumber);

			declaration.ZG_VATDeferNumber = "LOL";
			declaration.ZG_VATDeferType = "X";
			AssertEquals("We do nothing when the VAT defer type is set as invalid.", "LOL", declaration.ZG_VATDeferNumber);

			declaration.ZG_VATDeferType = "";
			AssertNullOrEmpty("VAT defer number should be cleared as the VAT defer type is cleared.", declaration.ZG_VATDeferNumber);
		}

		public void TestPaymentMethodWhenCountryIsNotInEU()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var orgHeader = company.GetNewOrgProxy(Factory);
			var euOrgImpAddInfo = (EUOrgImpAddInfo)orgHeader.CountryData.RegionSpecificImpAddInfo;
			euOrgImpAddInfo.ZO_OtherDeferType = DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration;

			declaration.JE_GC = company.PK;

			AssertEquals("Payment method should remain unchaged", DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority, declaration.JE_PaymentMethod);
		}

		protected virtual (JobDeclaration, OrgHeader) CreateDeclarationAndImporter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			declaration.ZG_VATDeferType = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1000", declaration.CountryCode);
			var euOrgImpAddInfo = EUOrgImpAddInfo.Get(importer, declaration.CountryCode);
			euOrgImpAddInfo.Deserialise();
			euOrgImpAddInfo.ZO_OtherDeferType = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			Factory.Save();

			return (declaration, importer);
		}

		protected virtual OrgHeader CreateDeclarant()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "DECLARANT";
			return declarant;
		}
	}
}
