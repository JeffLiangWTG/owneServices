using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCDECHeaderProvider))]
	sealed class CFCDECHeaderProviderTest : ImportHeaderProviderAbstractTest<CFCDECHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCDECHeaderProvider(null));
		}

		public void TestInputTaxDeductionFlag()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "OH1";
			var impAddInfo = (DEOrgImpAddInfo)declarant.CountryData.ImpAddInfo;
			impAddInfo.ZO_VATClaimBack = YesNoList.Codes.No;

			CombineAssertions(() =>
			{
				declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				AssertEquals("JE_VATClaimBack is N", false, Provider.InputTaxDeductionFlag);

				declaration.JE_VATClaimBack = YesNoList.Codes.Yes;
				AssertEquals("JE_VATClaimBack is Y", true, Provider.InputTaxDeductionFlag);

				declaration.JE_VATClaimBack = ZString.Empty;
				AssertEquals("JE_VATClaimBack is empty and IsDeclarantEntitledToClaimBackVAT is false", false, Provider.InputTaxDeductionFlag);

				impAddInfo.ZO_VATClaimBack = YesNoList.Codes.Yes;
				AssertEquals("JE_VATClaimBack is empty and IsDeclarantEntitledToClaimBackVAT is true", true, Provider.InputTaxDeductionFlag);
			});
		}

		public void TestPaymentMethod()
		{
			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
			AssertEquals(UniversalReferenceConstants.MethodOfPaymentTypes.E, Provider.PaymentMethod);
		}

		public void TestTaxOffice_SEL()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._1Self);
			AssertEquals("DE000666", Provider.TaxOffice);
		}

		public void TestTaxOffice_DIR()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._2Direct);
			AssertEquals("DE000777", Provider.TaxOffice);
		}

		public void TestTaxOffice_IND()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._3Indirect);
			AssertEquals("DE000666", Provider.TaxOffice);
		}

		public void TestTaxOffice_DeclarationSenderNull_Representative()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_Representative = Guid.Empty;
			AssertNull(Provider.TaxOffice);
		}

		public void TestTaxOffice_DeclarationSenderNull_Declarant()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			declaration.Branch.GB_OH_OrgProxy = Guid.Empty;
			declaration.JE_OA_DeclarantAddress = Guid.Empty;
			AssertNull(Provider.TaxOffice);
		}

		public void TestDutyDefermentApprovals_DECREP()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._1Self);
			SetRepresentedPartyAndDefermentParty();
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;
			declaration.JE_DefermentAccountNumber = "123456";
			declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.Representative;
			declaration.ZG_VATDeferNumber = "123457";
			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
			CombineAssertions(() =>
			{
				AssertEquals(2, Provider.DutyDefermentApprovals.Count);
				AssertEquals("EOR1_123456_BIN", Provider.DutyDefermentApprovals.First().AuthorisationNumber);
				AssertEquals("EOR2_123457_BIN", Provider.DutyDefermentApprovals.Last().AuthorisationNumber);
			});
		}

		public void TestDutyDefermentApprovals_RPPDEF()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._1Self);
			SetRepresentedPartyAndDefermentParty();
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.RepresentedParty;
			declaration.JE_DefermentAccountNumber = "123456";
			declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.DefermentParty;
			declaration.ZG_VATDeferNumber = "123457";
			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
			CombineAssertions(() =>
			{
				AssertEquals(2, Provider.DutyDefermentApprovals.Count);
				AssertEquals("EOR3_123456_BIN", Provider.DutyDefermentApprovals.First().AuthorisationNumber);
				AssertEquals("EOR4_123457_BIN", Provider.DutyDefermentApprovals.Last().AuthorisationNumber);
			});
		}

		public void TestDutyDefermentApprovals_RPPDEF_NoAddress()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._1Self);
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.RepresentedParty;
			declaration.JE_DefermentAccountNumber = "123456";
			declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.DefermentParty;
			declaration.ZG_VATDeferNumber = "123457";
			AssertEquals(0, Provider.DutyDefermentApprovals.Count);
		}

		public void TestDutyDefermentApprovals_NoValidAccount()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._1Self);
			SetRepresentedPartyAndDefermentParty();
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.RepresentedParty;
			declaration.JE_DefermentAccountNumber = "123456X";
			declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.DefermentParty;
			declaration.ZG_VATDeferNumber = "123457X";
			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
			AssertEquals(0, Provider.DutyDefermentApprovals.Count);
		}

		public void TestDutyDefermentApprovalEmptyWhenPaymentMethod_A() => AssertDutyDefermentApproval(UniversalReferenceConstants.MethodOfPaymentTypes.A, 0);

		public void TestDutyDefermentApprovalEmptyWhenPaymentMethod_C() => AssertDutyDefermentApproval(UniversalReferenceConstants.MethodOfPaymentTypes.C, 0);

		public void TestDutyDefermentApprovalEmptyWhenPaymentMethod_D() => AssertDutyDefermentApproval(UniversalReferenceConstants.MethodOfPaymentTypes.D, 0);

		public void TestDutyDefermentApprovalEmptyWhenPaymentMethod_E() => AssertDutyDefermentApproval(UniversalReferenceConstants.MethodOfPaymentTypes.E, 2);

		public void TestDutyDefermentApprovalEmptyWhenPaymentMethod_F() => AssertDutyDefermentApproval(UniversalReferenceConstants.MethodOfPaymentTypes.F, 2);

		public void TestDutyDefermentApprovalEmptyWhenPaymentMethod_G() => AssertDutyDefermentApproval(UniversalReferenceConstants.MethodOfPaymentTypes.G, 2);

		public void TestDutyDefermentApprovalEmptyWhenPaymentMethod_Z() => AssertDutyDefermentApproval(UniversalReferenceConstants.MethodOfPaymentTypes.Z, 2);

		public void TestArrivalTransportMeansIdentity_FIX()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.ZG_Box18TransportID = "AA-BB123";
			AssertNull(Provider.ArrivalTransportMeansIdentity);
		}

		public void TestArrivalTransportMeansIdentity_Other()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ZG_Box18TransportID = "AA-BB123";
			AssertEquals("AA-BB123", Provider.ArrivalTransportMeansIdentity);
		}

		public void TestCustomsValue_false()
		{
			declaration.ZG_IsHighValueOvrd = false;
			AssertNull(Provider.CustomsValue);
		}

		public void TestCustomsValue_true()
		{
			declaration.ZG_IsHighValueOvrd = true;
			AssertNotNull(Provider.CustomsValue);
		}

		public void TestForeignTradeStatisticsTransactionType()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_ValuationCode = YesNoList.Codes.Yes;

			AssertEquals(YesNoList.Codes.Yes, Provider.ForeignTradeStatisticsTransactionType);
		}

		public void TestForeignTradeStatisticsTransactionType_NoInvoice()
		{
			AssertNull(Provider.ForeignTradeStatisticsTransactionType);
		}

		public void TestConsignor_Null()
		{
			AssertNull(Provider.Consignor);
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var consignorAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.SupplierDocumentaryAddress.E2_OA_Address = consignorAddress.PK;
				AssertNotNull("Populated", Provider.Consignor);
				AssertEquals("Consignor's EORI", "GREOR1", Provider.Consignor.Identification.EoriNumber);
			});
		}

		public void TestAdditionalDutyReferences()
		{
			var fiscalReference1 = entryInstruction.FiscalReferences.AddNew();
			fiscalReference1.CFR_Code = "RF1";
			fiscalReference1.CFR_Reference = "RN001";
			var fiscalReference2 = entryInstruction.FiscalReferences.AddNew();
			fiscalReference2.CFR_Code = "RF2";
			fiscalReference2.CFR_Reference = "RN002";
			AssertEquals(2, Provider.AdditionalDutyReferences.Count);
		}

		public void TestAdditionalDutyReferences_Empty()
		{
			AssertEquals(0, Provider.AdditionalDutyReferences.Count);
		}

		public void TestLines()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryHeader.MergedLines.AddNew().PK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
			AssertEquals(2, Provider.Lines.Count);
		}

		protected override CFCDECHeaderProvider GetProvider() => new CFCDECHeaderProvider(entryHeader);

		void SetDeclarantAndRepresentative(string declarantType)
		{
			declaration.JE_DeclarantType = declarantType;
			var declarantAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declarantAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Greece;
			declarantAddress.Header.CustomsCodes.AddNew(OrgCusCode.GreeceCodeTypes.AFM, "08154711", Core.Constants.CountryCodes.Greece);
			declarantAddress.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000666", Core.Constants.CountryCodes.Germany);
			AddDefermentAccountToOrg(declarantAddress.Header, "123456", "EOR1_123456_BIN");
			AddDefermentAccountToOrg(declarantAddress.Header, "123457", "EOR1_123457_BIN");
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OA_BuyingAgentAddress = declarantAddress.PK;
			var representativeAddress = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS2");
			representativeAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Greece;
			representativeAddress.Header.CustomsCodes.AddNew(OrgCusCode.GreeceCodeTypes.AFM, "08154712", Core.Constants.CountryCodes.Greece);
			representativeAddress.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000777", Core.Constants.CountryCodes.Germany);
			AddDefermentAccountToOrg(representativeAddress.Header, "123456", "EOR2_123456_BIN");
			AddDefermentAccountToOrg(representativeAddress.Header, "123457", "EOR2_123457_BIN");
			declaration.JE_OA_Representative = representativeAddress.PK;
		}

		void SetRepresentedPartyAndDefermentParty()
		{
			var representedParty = GetOrgWithEORNumberAndEORIBranch("EOR3", "EBS3");
			representedParty.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "08154713", Core.Constants.CountryCodes.Germany);
			representedParty.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000888", Core.Constants.CountryCodes.Germany);
			AddDefermentAccountToOrg(representedParty.Header, "123456", "EOR3_123456_BIN");
			AddDefermentAccountToOrg(representedParty.Header, "123457", "EOR3_123457_BIN");
			declaration.JE_OA_BuyingAgentAddress = representedParty.PK;
			var defermentParty = GetOrgWithEORNumberAndEORIBranch("EOR4", "EBS4");
			defermentParty.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "08154714", Core.Constants.CountryCodes.Germany);
			defermentParty.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000999", Core.Constants.CountryCodes.Germany);
			AddDefermentAccountToOrg(defermentParty.Header, "123456", "EOR4_123456_BIN");
			AddDefermentAccountToOrg(defermentParty.Header, "123457", "EOR4_123457_BIN");
			declaration.DefermentPartyDocAddress.OrganisationPK = defermentParty.Header.PK;
		}

		void AddDefermentAccountToOrg(OrgHeader org, string accountNumber, string bin)
		{
			var account = org.AddDefermentAccountNumber("E", accountNumber);
			account.CZ_Type = "10";
			account.CZ_Issuer = "F";
			account.DecryptedPassword = bin;
		}

		void AssertDutyDefermentApproval(string mop, int numberOfExpectedEntries)
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._1Self);
			SetRepresentedPartyAndDefermentParty();
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.RepresentedParty;
			declaration.JE_DefermentAccountNumber = "123456";
			declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.DefermentParty;
			declaration.ZG_VATDeferNumber = "123457";

			declaration.ZG_MethodOfPayment = mop;

			AssertEquals(mop, numberOfExpectedEntries, Provider.DutyDefermentApprovals.Count);
		}

		new ICFCDECHeader Provider => base.Provider;
	}
}
