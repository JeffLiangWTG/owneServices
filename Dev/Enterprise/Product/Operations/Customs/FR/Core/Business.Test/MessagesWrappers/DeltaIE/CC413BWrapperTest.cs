using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CC413BWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC413BWrapper>
	{
		protected override CC413BWrapper GetProvider()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.FillWithValidTestData();
			buyer.OH_FullName = "buyer";
			buyer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Seychelles;

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_FullName = "Importer";
			importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.WallisAndFutunaIslands;

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_FullName = "supplier";
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var seller = Factory.New<OrgHeader>();
			seller.FillWithValidTestData();
			seller.OH_FullName = "seller";
			seller.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mauritius;

			var permit = Factory.New<Customs.Business.CusAuthorisationHeader>();
			permit.FillWithValidTestData();
			permit.CPH_OH_PermitHolder = supplier.PK;
			permit.CPH_OA_AppliesTo = supplier.MainAddress.PK;
			permit.CPH_Number = "num";
			permit.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;

			var rule = permit.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "OTH";

			var rule2 = permit.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = "CAN";
			rule2.CPR_ValueFrom = "ATH";

			var dfpOrg = Factory.NewWithValidTestData<OrgHeader>();
			dfpOrg.OH_Code = "DFRPORG";
			dfpOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.France);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CUS";
			staff.GS_FullName = "Name";
			staff.GS_WorkPhone = "staffPhone";
			var email = staff.EmailAddresses.AddNew();
			email.GSE_EmailAddress = "emailAddress";
			email.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;

			Factory.Save();

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "Declarant";
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.France);
			declarant.MainAddress.OA_Address1 = "177 Impasse Jane Poupelet";
			declarant.MainAddress.OA_Address2 = "Lescuretie";
			declarant.MainAddress.OA_PostCode = "24140";
			declarant.MainAddress.OA_City = "EYRAUD CREMPSE MAURENS";
			declarant.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = "CUS";
			declaration.CustomsOffices.RemoveAndDeleteAll();
			var supervisingOffice = declaration.CustomsOffices.AddNew();
			supervisingOffice.CY_Code = Enterprise.Customs.EU.Business.EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep;
			supervisingOffice.CY_Data = "FR000001";
			var customsOfficesOfDischarge = declaration.CustomsOffices.AddNew();
			customsOfficesOfDischarge.CY_Code = FrOfficeCodesTypes.Codes.OfficeOfDischarge;
			customsOfficesOfDischarge.CY_Data = "FR000002";
			var customsOfficesOfDischarge2 = declaration.CustomsOffices.AddNew();
			customsOfficesOfDischarge2.CY_Code = FrOfficeCodesTypes.Codes.OfficeOfDischarge;
			customsOfficesOfDischarge2.CY_Data = "FR000003";
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_EntryStyle = "IM";

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedKingdom;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_CustomsOffice = "FR230023";
			instruction.CEI_OA_Warehouse2 = supplier.MainAddress.PK;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MRN = "mrnNumber";
			entry.CRN = "crnNumber";
			var entryLine = entry.AllEntryLines.AddNew();

			var fiscalreference = instruction.FiscalReferences.AddNew();
			fiscalreference.CFR_Reference = "fiscref";
			instruction.FiscalReferences.AddNew().FillWithValidTestData();
			var cusSupplyChainActorReferences = instruction.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReferences.CFR_Reference = "actref";
			instruction.CusSupplyChainActorReferences.AddNew().FillWithValidTestData();
			instruction.CEI_DateForDuty = new ZDateTime(2022, 10, 30, 14, 41, 57);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RN_NKCountryOfExport = Core.Constants.CountryCodes.France;
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			instruction.ZG_TransNature = "A";
			invoice.JZ_InvoiceDisplaySequence = 1;
			invoice.JZ_InvoiceAmount = 12m;
			invoice.JZ_InvoiceCurrExRate = 14m;

			var prev = invoice.PreviousDocuments.AddNew();
			prev.CSI_ReferenceNumber = "prev";
			invoice.PreviousDocuments.AddNew();
			var sup = invoice.SupportingDocuments.AddNew();
			sup.CSI_ReferenceNumber = "sup";
			invoice.SupportingDocuments.AddNew();

			var refAdditionalInfo = invoice.AdditionalInfos.AddNew();
			refAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			refAdditionalInfo.CSI_Code = "9008";
			var infAdditionalInfo = invoice.AdditionalInfos.AddNew();
			infAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			infAdditionalInfo.CSI_Code = "9005";
			invoice.JZ_OH_Buyer = buyer.PK;
			invoice.JZ_UCR = "refUCR";
			instruction.ZG_TransNature = "A";
			invoice.JZ_OA_SellerAddress = seller.MainAddress.PK;

			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.RemoveAndDeleteAll();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "98765432", Core.Constants.CountryCodes.France);
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			var personProvidingGuarantee = Factory.NewWithValidTestData<OrgHeader>();
			personProvidingGuarantee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "23456789", Core.Constants.CountryCodes.France);
			declaration.JE_OH_ControllingCustomer = personProvidingGuarantee.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;

			var auth = instruction.CusAuthorizationUsages.AddNew();
			auth.AGC_OH_Owner = supplier.PK;
			auth.AGC_Number = "5678";
			auth.AGC_Code = "code";

			var auth2 = instruction.CusAuthorizationUsages.AddNew();
			auth2.AGC_OH_Owner = supplier.PK;
			auth2.AGC_Number = "5789";
			auth2.AGC_Code = "cod2";

			var charge = invoice.Charges.AddNew();
			charge.J7_Amount = 18m;
			invoice.Charges.AddNew().FillWithValidTestData();

			invoice.ZG_AgreedPlaceCode = "3";

			var guarantee = invoiceLine.EntryInstruction.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GRN0001";
			guarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;
			guarantee.PW_Password = "1234";
			guarantee.PW_RX_NKCurrency = Core.Constants.CurrencyCodes.France;
			guarantee.PW_BondNumber2 = "OTH0001";
			guarantee.PW_BondFiledPort = "FR000001";
			guarantee.PW_BondAmount = 123.456m;

			var docAddress = declaration.DocAddresses.AddNew();
			docAddress.E2_AddressType = "DFP";
			docAddress.E2_OA_Address = dfpOrg.MainAddress.PK;

			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			entry.CH_EntrySubmittedDate = new ZDateTime(2020, 01, 01, 12, 30, 00);

			var sendingObject = new DeltaIEJobDeclarationMessageSendingObject(entry);
			sendingObject.VOCReason = "VOC Reason";

			return CC413BWrapper.New(sendingObject);
		}

		protected CC413BWrapper GetProviderForEmptyCollectionTest(JobDeclaration jobDeclaration = null)
		{
			var declaration = jobDeclaration ?? Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var sendingObject = new DeltaIEJobDeclarationMessageSendingObject(entry);
			return CC413BWrapper.New(sendingObject);
		}

		public void TestAuthorisation()
		{
			AssertType<Collection<IAuthorisation>>("Authorisation type", Provider.Authorisation);
			AssertEquals("There should be 2 Authorisation  as there are 2 CusAuthorizationUsages in the invoiceLines", 2, Provider.Authorisation.Count);
			AssertEquals("ReferenceNumber should be equal to the first Authorisation AGC_Number.", "5678", Provider.Authorisation.ElementAt(0).ReferenceNumber);

			AssertEquals("There should be 0 Authorisation  as there are 0 CusAuthorizationUsages in the invoiceLines", 0, GetProviderForEmptyCollectionTest().Authorisation.Count);
		}

		public void TestCurrencyExchange()
		{
			AssertType<CurrencyExchangeWrapper>("CurrencyExchange type", Provider.CurrencyExchange);
			AssertEquals("InternalCurrencyUnit should equal EUR", "EUR", Provider.CurrencyExchange.InternalCurrencyUnit);
		}

		public void TestCustomsOfficeOfPresentation()
		{
			AssertType<CustomsOfficeOfPresentationWrapper>("CustomsOfficeOfPresentation type", Provider.CustomsOfficeOfPresentation);
			AssertEquals("ReferenceNumber should equal JE_customsOffice", "FR230023", Provider.CustomsOfficeOfPresentation.ReferenceNumber);
		}

		#region declarant

		public void TestDeclarant()
		{
			AssertType<DeclarantWrapper>("Declarant type", Provider.Declarant);
			AssertEquals("IdentificationNumber value should equal the first Declarant eori", "FR12345678", Provider.Declarant.IdentificationNumber);
		}

		public void TestDeclarant_IdentificationNumber_ForOperational()
		{
			var provider = GetProviderForDeclarant();
			AssertEquals("IdentificationNumber should be empty when EORI is not set.", ZString.Empty, provider.Declarant.IdentificationNumber);

			SetupGlobalBranch();
			provider = GetProviderForDeclarant();
			AssertEquals("IdentificationNumber should be equal to declarant EORI.", "FRABC", provider.Declarant.IdentificationNumber);
		}

		public void TestDeclarant_ContactPerson()
		{
			var provider = Provider;
			AssertEquals("Name", provider.Declarant.ContactPerson.Name);
			AssertEquals("emailAddress", provider.Declarant.ContactPerson.EMailAddress);
			AssertEquals("staffPhone", provider.Declarant.ContactPerson.PhoneNumber);
		}

		public void TestDeclarant_Name_ForOperational()
		{
			SetupGlobalBranch();
			var provider = GetProviderForDeclarant();
			AssertNull("Name should be null when IdentificationNumber is not null or empty.", provider.Declarant.Name);

			SetupGlobalBranch();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			provider = GetProviderForDeclarant();
			AssertEquals("Name should be CurrentBranch.OrgProxy.OH_FullName when IdentificationNumber is null or empty", "Declarant Global Branch Name", provider.Declarant.Name);
		}

		public void TestDeclarant_Address_ForOperational()
		{
			SetupGlobalBranch();
			var provider = GetProviderForDeclarant();
			AssertNull("Address should not be mapped when IdentificationNumber is non null or empty.", provider.Declarant.Address);

			SetupGlobalBranch();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			provider = GetProviderForDeclarant();
			var address = provider.Declarant.Address;
			CombineAssertions("Address should use OrganisationAddressWrapper when IdentificationNumber is null or empty.", () =>
			{
				AssertEquals("City should equal OA_City.", "Global City", address.City);
				AssertEquals("Country should equal OA_RN_NKCountryCode.", Core.Constants.CountryCodes.France, address.Country);
				AssertEquals("Postcode should equal OA_PostCode.", "56789", address.Postcode);
				AssertEquals("StreetAndNumber should equal OA_Address1 + OA_Address2, coma separated.", "Global Road, Global Building", address.StreetAndNumber);
			});
		}

		CC413BWrapper GetProviderForDeclarant()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var sendingObject = new DeltaIEJobDeclarationMessageSendingObject(entry);
			sendingObject.VOCReason = "VOC Reason";

			return CC413BWrapper.New(sendingObject, true);
		}

		void SetupGlobalBranch()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "Declarant Global Branch Name";
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ABC", Core.Constants.CountryCodes.France);
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address1 = "Global Road";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address2 = "Global Building";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_PostCode = "56789";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_City = "Global City";
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		}

		#endregion declarant

		public void TestDeferredPayment()
		{
			AssertType<Collection<IDeferredPayment>>("DeferredPayment type", Provider.DeferredPayment);
			AssertEquals("DeferredPayment count should by default be equal 1.", 1, Provider.DeferredPayment.Count);

			AssertEquals("DeferredPayment count should by default be equal 1.", 1, GetProviderForEmptyCollectionTest().DeferredPayment.Count);
		}

		public void TestDeferredPaymentForUCC6()
		{
			var importer = CreateImporter("IMP", "DANXXX", "DIE001", "535D2B5E");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			var provider = GetProviderForEmptyCollectionTest(declaration);
			var deferredPayment = provider.DeferredPayment.ElementAt(0);

			Factory.Save();

			AssertEquals("DeferredPayment is not filled as DefermentAccountNumber is not defaulted", string.Empty, deferredPayment.DeferredPayment);
			AssertEquals("Declaration is not UCC6, guarantee mapping should not exist", 0, provider.Guarantee.Count);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("DeferredPayment is not filled, guarantee mapping should not exist", 0, provider.Guarantee.Count);

				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("DefermentAccountNumber is defaulted, a guarantee mapping should exist", 1, provider.Guarantee.Count);
				AssertEquals("DeferredPayment GuaranteeType should be 1", "1", provider.Guarantee.ElementAt(0).GuaranteeType);

				CombineAssertions("GuaranteeReference assertions", () =>
				{
					var guaranteeReference = provider.Guarantee.ElementAt(0).GuaranteeReference.ElementAt(0);
					AssertEquals("GuaranteeReference AmountToBeCovered should be 0", 0.00D, guaranteeReference.AmountToBeCovered);
					AssertEquals("GuaranteeReference Currency should be EUR", "EUR", guaranteeReference.CurrencyCode);
					AssertEquals("GuaranteeReference GRN should be same as DeferredPayment", deferredPayment.DeferredPayment, guaranteeReference.Grn);
					AssertEquals("GuaranteeReference CustomsOfficeOfGuarantee should be empty", string.Empty, guaranteeReference.CustomsOfficeOfGuarantee.ReferenceNumber);
					AssertNull("GuaranteeReference CcQualifier is not mapped", guaranteeReference.CcQualifier);
					AssertNull("GuaranteeReference AccessCode is not mapped", guaranteeReference.AccessCode);
					AssertNull("GuaranteeReference OtherGuaranteeReference is not mapped", guaranteeReference.OtherGuaranteeReference);
				});
			}

			OrgHeader CreateImporter(ZString importerCode, ZString customsRegNo, ZString account, ZString representativeId)
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.OH_Code = importerCode;
				importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, customsRegNo, Core.Constants.CountryCodes.France);
				importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, account, ZString.Empty, ZString.Empty, representativeId);
				return importer;
			}
		}

		public void TestGoodsShipment()
		{
			AssertType<Collection<IGoodsShipment>>("GoodsShipment type", Provider.GoodsShipment);
			AssertEquals("GoodsShipment count should always equal 1 because there's is always a single entry per message.", 1, Provider.GoodsShipment.Count);

			AssertEquals("GoodsShipment count should always equal 1 because there's is always a single entry per message.", 1, GetProviderForEmptyCollectionTest().GoodsShipment.Count);

			AssertType<GoodsShipmentWrapperFor415And413>("GoodsShipmentWrapper type should be GoodsShipmentWrapperFor415And413", Provider.GoodsShipment.First());
		}

		public void TestGuarantee()
		{
			AssertType<Collection<IGuarantee>>("GoodsShipment type", Provider.Guarantee);
			AssertEquals("Guarantee count should equal 1 as there is 1 Guarantee.", 1, Provider.Guarantee.Count);
			AssertEquals("Guarantee should equal EU.Business.CodeDescriptionPairLists.NctsGuarantee.Codes.ComprehensiveGuarantee = 1", EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, Provider.Guarantee.ElementAt(0).GuaranteeType);
			AssertEquals("Guarantee count should equal 0 as there is 0 Guarantee.", 0, GetProviderForEmptyCollectionTest().Guarantee.Count);
		}

		public void TestImporter()
		{
			AssertEquals("Importer name should equal declaration importer OH_name", "Importer", Provider.Importer.Name);
			AssertEquals("Importer adress country should equal declaration importer documentary address country", Core.Constants.CountryCodes.WallisAndFutunaIslands, Provider.Importer.Address.Country);
		}

		public void TestImportOperation()
		{
			AssertEquals("DeclarationType should equal declaration JE_EntryStyle.", "IM", Provider.ImportOperation.DeclarationType);
			AssertEquals("MRN should be equal to CusEntryHeader MRN.", "mrnNumber", Provider.ImportOperation.MRN);
			AssertEquals("CustomsRegistrationNumber should be equal to CusEntryHeader CRN.", "crnNumber", Provider.ImportOperation.CustomsRegistrationNumber);
		}

		public void TestPersonPayingCustomsDuty()
		{
			AssertEquals("IdentificationNumber should equal person EORI.", "FR12345678", Provider.PersonPayingCustomsDuty.IdentificationNumber);
		}

		public void TestPersonProvidingAGuarantee()
		{
			AssertEquals("IdentificationNumber should equal empty.", string.Empty, Provider.PersonProvidingAGuarantee.IdentificationNumber);
		}

		public void TestRepresentative()
		{
			AssertEquals("IdentificationNumber should equal representative EORI number.", "FR98765432", Provider.Representative.IdentificationNumber);
			AssertEquals("Name", Provider.Representative.ContactPerson.Name);
			AssertEquals("staffPhone", Provider.Representative.ContactPerson.PhoneNumber);
			AssertEquals("emailAddress", Provider.Representative.ContactPerson.EMailAddress);
		}

		public void TestSupervisingCustomsOffice()
		{
			AssertEquals("ReferenceNumber should equal declaration customsOffice with role = CAU.", "FR000001", Provider.SupervisingCustomsOffice.ReferenceNumber);
		}

		public void TestCustomsOfficesOfDischarge()
		{
			AssertType<List<ICustomsOfficesOfDischarge>>("ReferenceNumber should equal declaration customsOffice with role = DIS.", Provider.CustomsOfficesOfDischarge.ToList());
			AssertEquals("Count of customsOfficesOfDischarge", 2, Provider.CustomsOfficesOfDischarge.ToList().Count);
			AssertEquals("CustomsOfficesOfDischarge values should be equal to to customs office CY_Data where role is DIS", "FR000002", Provider.CustomsOfficesOfDischarge.ToList()[0].ReferenceNumber);
			AssertEquals("CustomsOfficesOfDischarge values should be equal to to customs office CY_Data where role is DIS", "FR000003", Provider.CustomsOfficesOfDischarge.ToList()[1].ReferenceNumber);
		}

		[TestDate(2024, 03, 18, 12, 30, 00)]
		public void TestRequest()
		{
			AssertEquals("AmendmentRequestDateAndTime should be equal to sendingObject Date.", new ZDateTime(2024, 03, 18, 12, 30, 00).ToString("yyyy-MM-ddTHH:mm:ss"), Provider.Request.AmendmentRequestDateAndTime);
			AssertEquals("AmendmentReason should be equal to sendingObject VOCReason.", "VOC Reason", Provider.Request.AmendmentReason);
		}

		public void TestIsSimplified()
		{
			AssertEquals("IsSimplified should be false when EntryInstruction is not simplified.", false, Provider.IsSimplified);

			var declaration = Factory.New<JobDeclaration>();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "I1";
			instruction.CEI_SubStyle = "C";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var provider = CC415BWrapper.New(entry);
			AssertEquals("IsSimplified should be true when EntryInstruction is simplified.", true, provider.IsSimplified);
		}
	}
}
