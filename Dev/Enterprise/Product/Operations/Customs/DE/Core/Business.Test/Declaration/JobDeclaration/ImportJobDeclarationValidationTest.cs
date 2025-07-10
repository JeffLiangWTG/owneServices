using System;
using System.Collections.Generic;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ImportJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJE_RL_NKPortOfFirstArrival()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.ZG_IsHighValueOvrd = false;
				jobDeclaration.Validation.ValidateJE_RL_NKPortOfFirstArrival();
				AssertNoMessageErrorContaining("ZG_IsHighValueOvrd is false", jobDeclaration.JE_RL_NKPortOfFirstArrivalInfo, MandatoryValidation.YouHaveNotEntered);

				jobDeclaration.ZG_IsHighValueOvrd = true;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_RL_NKPortOfFirstArrivalInfo, MandatoryValidation.YouHaveNotEntered, "ZG_IsHighValueOvrd is true");
			});
		}

		public void TestCheckJE_GoodsOrigin()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_GoodsOriginInfo);
		}

		public void TestCheckJE_GoodsOrigin_InwardProcessingAVABR()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			jobDeclaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoNotifications(jobDeclaration.JE_GoodsOriginInfo);
		}

		public void TestCheckJE_GoodsDestination()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_GoodsDestinationInfo);
		}

		public void TestCheckJE_GoodsDestination_InwardProcessingAVABR()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			jobDeclaration.Validation.ValidateJE_GoodsDestination();
			AssertNoNotifications(jobDeclaration.JE_GoodsDestinationInfo);
		}

		public void TestCheckJE_RL_NKFinalDestinationMandatory_EZA()
		{
			AssertJE_RL_NKFinalDestinationMandatory(ImportDeclarationTypeList.Codes.EZA);
		}

		public void TestCheckJE_RL_NKFinalDestinationMandatory_EAV()
		{
			AssertJE_RL_NKFinalDestinationMandatory(ImportDeclarationTypeList.Codes.EAV);
		}

		public void TestCheckJE_RL_NKFinalDestinationMandatory_EZL()
		{
			jobDeclaration.JE_StatisticStatus = StatisticStatusCodeList.Codes.C04;
			AssertJE_RL_NKFinalDestinationMandatory(ImportDeclarationTypeList.Codes.EZL);

			jobDeclaration.JE_StatisticStatus = StatisticStatusCodeList.Codes.C01;
			jobDeclaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertNoMessageError("JE_StatisticStatus <> '04', German destination, no State", jobDeclaration.JE_RL_NKFinalDestinationInfo, "You have not entered a [17] Destination.");
		}

		public void TestCheckJE_RL_NKFinalDestinationStateMandatory_EZA()
		{
			AssertJE_RL_NKFinalDestinationStateMandatory(ImportDeclarationTypeList.Codes.EZA);
		}

		public void TestCheckJE_RL_NKFinalDestinationStateMandatory_EAV()
		{
			AssertJE_RL_NKFinalDestinationStateMandatory(ImportDeclarationTypeList.Codes.EAV);
		}

		public void TestCheckJE_RL_NKFinalDestinationStateMandatory_EZL()
		{
			jobDeclaration.JE_StatisticStatus = StatisticStatusCodeList.Codes.C04;
			AssertJE_RL_NKFinalDestinationStateMandatory(ImportDeclarationTypeList.Codes.EZL);

			var germanRefUNLOCO = CreateNewOrGetExistingRefUNLOCO(Core.Constants.CountryCodes.Germany, "DEHAM", ZGuid.Empty);
			jobDeclaration.JE_StatisticStatus = StatisticStatusCodeList.Codes.C01;
			jobDeclaration.JE_RL_NKFinalDestination = germanRefUNLOCO.Code;
			AssertNoMessageError("JE_StatisticStatus <> '04', German destination, no State", jobDeclaration.JE_RL_NKFinalDestinationInfo, "You have not entered a State for this [17] Destination.");
		}

		public void TestCheckJE_DefermentAccountNumber()
		{
			jobDeclaration.JE_OA_DeclarantAddress = Factory.CreateDeferralParty("10", "1111").MainAddress.PK;

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;
			jobDeclaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertHasMessageErrorContaining(jobDeclaration.JE_DefermentAccountNumberInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_DefermentAccountNumber = "XXX";
			AssertNoMessageErrorContaining(jobDeclaration.JE_DefermentAccountNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(jobDeclaration.JE_DefermentAccountNumberInfo, ListValidation.InvalidCodeMessageError);

			jobDeclaration.JE_DefermentAccountNumber = "XXX";
			AssertNoNotifications(jobDeclaration.ZG_VATDeferNumberInfo);

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;
			jobDeclaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoNotifications(jobDeclaration.JE_DefermentAccountNumberInfo);
		}

		public void TestCheckJE_OA_SellerAddress()
		{
			const string EoriOrAddressMustExist = "Seller must have an EORI Number entered in Organizations Registration Numbers / Codes. Or Seller Address must have Company Name, Address 1, Country/Region and City entered and Post Code must be entered if Country/Region is Germany.";

			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var address = orgHeader.Addresses.AddNew();

			jobDeclaration.ZG_IsHighValueOvrd = ZBool.True;
			jobDeclaration.JE_OA_SellerAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(jobDeclaration.JE_OA_SellerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_OA_SellerAddress = address.PK;
			AssertNoMessageErrorContaining(jobDeclaration.JE_OA_SellerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(jobDeclaration.JE_OA_SellerAddressInfo, EoriOrAddressMustExist);

			var eori = orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Greece);
			jobDeclaration.JE_OA_SellerAddress = address.PK;
			AssertNoNotifications(jobDeclaration.JE_OA_SellerAddressInfo);

			orgHeader.CustomsCodes.RemoveAndDelete(eori);
			address.OA_CompanyNameOverride = "XXX";
			jobDeclaration.JE_OA_SellerAddress = address.PK;
			AssertHasMessageError(jobDeclaration.JE_OA_SellerAddressInfo, EoriOrAddressMustExist);
			address.OA_Address1 = "Address1";
			jobDeclaration.JE_OA_SellerAddress = address.PK;
			AssertHasMessageError(jobDeclaration.JE_OA_SellerAddressInfo, EoriOrAddressMustExist);
			address.OA_City = "City";
			jobDeclaration.JE_OA_SellerAddress = address.PK;
			AssertHasMessageError(jobDeclaration.JE_OA_SellerAddressInfo, EoriOrAddressMustExist);
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			jobDeclaration.JE_OA_SellerAddress = address.PK;
			AssertNoNotifications(jobDeclaration.JE_OA_SellerAddressInfo);
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			jobDeclaration.JE_OA_SellerAddress = address.PK;
			AssertHasMessageError(jobDeclaration.JE_OA_SellerAddressInfo, EoriOrAddressMustExist);
			address.OA_PostCode = "PostCode";
			jobDeclaration.JE_OA_SellerAddress = address.PK;
			AssertNoNotifications(jobDeclaration.JE_OA_SellerAddressInfo);

			jobDeclaration.ZG_IsHighValueOvrd = ZBool.False;
			jobDeclaration.JE_OA_SellerAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining(jobDeclaration.JE_OA_SellerAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_OA_ConsigneeAddress()
		{
			const string EoriOrAddressMustExist = "Buyer must have an EORI Number entered in Organizations Registration Numbers / Codes. Or Buyer Address must have Company Name, Address 1, Country/Region and City entered and Post Code must be entered if Country/Region is Germany.";

			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var address = orgHeader.Addresses.AddNew();

			jobDeclaration.ZG_IsHighValueOvrd = ZBool.True;
			jobDeclaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(jobDeclaration.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_OA_ConsigneeAddress = address.PK;
			AssertNoMessageErrorContaining(jobDeclaration.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(jobDeclaration.JE_OA_ConsigneeAddressInfo, EoriOrAddressMustExist);

			var eori = orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Greece);
			jobDeclaration.JE_OA_ConsigneeAddress = address.PK;
			AssertNoNotifications(jobDeclaration.JE_OA_ConsigneeAddressInfo);

			orgHeader.CustomsCodes.RemoveAndDelete(eori);
			address.OA_CompanyNameOverride = "XXX";
			jobDeclaration.JE_OA_ConsigneeAddress = address.PK;
			AssertHasMessageError(jobDeclaration.JE_OA_ConsigneeAddressInfo, EoriOrAddressMustExist);
			address.OA_Address1 = "Address1";
			jobDeclaration.JE_OA_ConsigneeAddress = address.PK;
			AssertHasMessageError(jobDeclaration.JE_OA_ConsigneeAddressInfo, EoriOrAddressMustExist);
			address.OA_City = "City";
			jobDeclaration.JE_OA_ConsigneeAddress = address.PK;
			AssertHasMessageError(jobDeclaration.JE_OA_ConsigneeAddressInfo, EoriOrAddressMustExist);
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			jobDeclaration.JE_OA_ConsigneeAddress = address.PK;
			AssertNoNotifications(jobDeclaration.JE_OA_ConsigneeAddressInfo);
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			jobDeclaration.JE_OA_ConsigneeAddress = address.PK;
			AssertHasMessageError(jobDeclaration.JE_OA_ConsigneeAddressInfo, EoriOrAddressMustExist);
			address.OA_PostCode = "PostCode";
			jobDeclaration.JE_OA_ConsigneeAddress = address.PK;
			AssertNoNotifications(jobDeclaration.JE_OA_ConsigneeAddressInfo);

			jobDeclaration.ZG_IsHighValueOvrd = ZBool.False;
			jobDeclaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining(jobDeclaration.JE_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_IATALoadPort()
		{
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			jobDeclaration.ZG_IsHighValueOvrd = ZBool.True;
			jobDeclaration.JE_IATALoadPort = ZString.Empty;
			AssertHasWarning(jobDeclaration.JE_IATALoadPortInfo, "For an automated split of the total air freight costs you should enter an IATA Code!");

			jobDeclaration.JE_IATALoadPort = "XX";
			AssertNoWarning(jobDeclaration.JE_IATALoadPortInfo, "For an automated split of the total air freight costs you should enter an IATA Code!");

			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			jobDeclaration.ZG_IsHighValueOvrd = ZBool.False;
			jobDeclaration.JE_IATALoadPort = ZString.Empty;
			AssertNoWarning(jobDeclaration.JE_IATALoadPortInfo, "For an automated split of the total air freight costs you should enter an IATA Code!");

			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			jobDeclaration.ZG_IsHighValueOvrd = ZBool.True;
			jobDeclaration.JE_IATALoadPort = ZString.Empty;
			AssertNoWarning(jobDeclaration.JE_IATALoadPortInfo, "For an automated split of the total air freight costs you should enter an IATA Code!");
		}

		public void TestCheckJE_OA_Representative_MandatoryForRepTypeDIR()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_OA_RepresentativeInfo, "Please enter a Representative for Rep. Type 'DIR'.", "RepType 'DIR");
		}

		public void TestCheckJE_OA_Representative_MustHaveEORINumber()
		{
			var message = "Representative must have an EORI Number entered in Organizations Registration Numbers / Codes.";
			Enterprise.Customs.DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var orgHeader = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				jobDeclaration.JE_OA_Representative = orgHeader.MainAddress.PK;
				AssertNoMessageError("RepType not 'DIR', no EORI-number", jobDeclaration.JE_OA_RepresentativeInfo, message);

				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				jobDeclaration.JE_OA_Representative = orgHeader.MainAddress.PK; // prior setting JE_DeclarantType = RepresentationTypeList.Codes._2Direct autopopulates Representative
				AssertHasMessageError("RepType 'DIR', no EORI-number", jobDeclaration.JE_OA_RepresentativeInfo, message);

				orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Greece);
				jobDeclaration.Validation.ValidateJE_OA_Representative();
				AssertNoMessageError("RepType 'DIR', has EORI-number", jobDeclaration.JE_OA_RepresentativeInfo, message);
			});
		}

		public void TestCheckJE_OA_Representative_JE_OA_Representative_ValidGUIDbutNoCorrespondingOrgAddressInFactory()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertNoExceptionThrown("Triggers CheckJE_OA_Representative", () => jobDeclaration.JE_OA_Representative = new ZGuid("D82F67FC-2F75-4F4E-B62F-534F93C5CC7B"));
		}

		public void TestCheckJE_OA_Representative_TAO()
		{
			const string errorMessage = "Representative must have a Registration Number / Code of type 'TAO' for the requested customs procedure.";
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var orgWithoutRegistrationTAO = Factory.NewWithValidTestData<OrgHeader>();
			var orgWithRegistrationTAO = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = orgWithRegistrationTAO.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice;
			orgCusCode.OK_CustomsRegNo = "DE12345";

			CombineAssertions(() =>
			{
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				invoiceLine.JI_Procedure = "8010";
				jobDeclaration.JE_OA_Representative = orgWithoutRegistrationTAO.MainAddress.PK;
				AssertNoMessageError("Rep Type 'DIR', CPC '8010', No TAO", jobDeclaration.JE_OA_RepresentativeInfo, errorMessage);

				invoiceLine.JI_Procedure = "4210";
				jobDeclaration.Validation.ValidateJE_OA_Representative();
				AssertHasMessageError("Rep Type 'DIR', CPC '4210', No TAO", jobDeclaration.JE_OA_RepresentativeInfo, errorMessage);

				jobDeclaration.JE_OA_Representative = orgWithRegistrationTAO.MainAddress.PK;
				AssertNoMessageError("Rep Type 'DIR', CPC '4210', Has TAO", jobDeclaration.JE_OA_RepresentativeInfo, errorMessage);

				invoiceLine.JI_Procedure = "6310";
				jobDeclaration.Validation.ValidateJE_OA_Representative();
				AssertNoMessageError("Rep Type 'DIR', CPC '6310', Has TAO", jobDeclaration.JE_OA_RepresentativeInfo, errorMessage);

				jobDeclaration.JE_OA_Representative = orgWithoutRegistrationTAO.MainAddress.PK;
				AssertHasMessageError("Rep Type 'DIR', CPC '6310', No TAO", jobDeclaration.JE_OA_RepresentativeInfo, errorMessage);

				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				jobDeclaration.Validation.ValidateJE_OA_Representative();
				AssertNoMessageError("Rep Type 'IND', CPC '6310', No TAO", jobDeclaration.JE_OA_RepresentativeInfo, errorMessage);

				invoiceLine.JI_Procedure = "4210";
				jobDeclaration.Validation.ValidateJE_OA_Representative();
				AssertNoMessageError("Rep Type 'IND', CPC '4210', No TAO", jobDeclaration.JE_OA_RepresentativeInfo, errorMessage);
			});
		}

		public void TestCheckJE_OA_Representative_UST()
		{
			const string message = "Representative must have a Registration Number / Code of type 'UST' for the requested customs procedure.";
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var organisationWithUST = Factory.New<OrgHeader>();
			organisationWithUST.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "123", Core.Constants.CountryCodes.Germany);
			var representativeAddressWithUST = organisationWithUST.MainAddress.PK;
			var organisationWithoutUST = Factory.New<OrgHeader>();
			var representativeAddressWithoutUST = organisationWithoutUST.MainAddress.PK;

			CombineAssertions(() =>
			{
				foreach (var procedure in new ZString[] { "4210", "6310", "8010" })
				{
					invoiceLine.JI_Procedure = procedure;

					foreach (var declarantType in new RepresentationTypeList().GetAllCodes())
					{
						jobDeclaration.JE_DeclarantType = declarantType;
						jobDeclaration.JE_OA_Representative = representativeAddressWithoutUST;

						if ((procedure.StartsWith("42") || procedure.StartsWith("63")) && declarantType == RepresentationTypeList.Codes._2Direct)
						{
							AssertHasMessageError($"{declarantType}, {procedure}, no UST", jobDeclaration.JE_OA_RepresentativeInfo, message);

							jobDeclaration.JE_OA_Representative = representativeAddressWithUST;
							AssertNoMessageError($"{declarantType}, {procedure}, has UST", jobDeclaration.JE_OA_RepresentativeInfo, message);
						}
						else
						{
							AssertNoMessageError($"{declarantType}, {procedure}, no UST", jobDeclaration.JE_OA_RepresentativeInfo, message);
						}
					}
				}
			});
		}

		public void TestCheckJE_OA_DeclarantAddress_Mandatory()
		{
			CombineAssertions("IsDeclarantAddressRequired is always true for Import declaration", () =>
			{
				ValidationTestHelper.AssertErrorIfNotEntered(jobDeclaration.JE_OA_DeclarantAddressInfo);
			});
		}

		public void TestCheckJE_OA_DeclarantAddress_UST()
		{
			const string message = "Declarant must have a Registration Number / Code of type 'UST' for the requested customs procedure.";
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var organisationWithUST = Factory.New<OrgHeader>();
			organisationWithUST.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "123", Core.Constants.CountryCodes.Germany);
			var declarantAddressWithUST = organisationWithUST.MainAddress.PK;
			var organisationWithoutUST = Factory.New<OrgHeader>();
			var declarantAddressWithoutUST = organisationWithoutUST.MainAddress.PK;

			CombineAssertions(() =>
			{
				foreach (var procedure in new ZString[] { "4210", "6310", "8010" })
				{
					invoiceLine.JI_Procedure = procedure;

					foreach (var declarantType in new RepresentationTypeList().GetAllCodes())
					{
						jobDeclaration.JE_DeclarantType = declarantType;
						jobDeclaration.JE_OA_DeclarantAddress = declarantAddressWithoutUST;

						if ((procedure.StartsWith("42") || procedure.StartsWith("63")) &&
							(declarantType == RepresentationTypeList.Codes._1Self || declarantType == RepresentationTypeList.Codes._3Indirect))
						{
							AssertHasMessageError($"{declarantType}, {procedure}, no UST", jobDeclaration.JE_OA_DeclarantAddressInfo, message);

							jobDeclaration.JE_OA_DeclarantAddress = declarantAddressWithUST;
							AssertNoMessageError($"{declarantType}, {procedure}, has UST", jobDeclaration.JE_OA_DeclarantAddressInfo, message);
						}
						else
						{
							AssertNoMessageError($"{declarantType}, {procedure}, no UST", jobDeclaration.JE_OA_DeclarantAddressInfo, message);
						}
					}
				}
			});
		}

		public void TestCheckJE_OA_DeclarantAddress_TAO()
		{
			const string message = "Declarant must have a Registration Number / Code of type 'TAO' for the requested customs procedure.";
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var organisationWithTAO = Factory.New<OrgHeader>();
			organisationWithTAO.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "123", Core.Constants.CountryCodes.Germany);
			var declarantAddressWithTAO = organisationWithTAO.MainAddress.PK;
			var organisationWithoutTAO = Factory.New<OrgHeader>();
			var declarantAddressWithoutTAO = organisationWithoutTAO.MainAddress.PK;

			CombineAssertions(() =>
			{
				foreach (var procedure in new ZString[] { "4210", "6310", "8010" })
				{
					invoiceLine.JI_Procedure = procedure;

					foreach (var declarantType in new RepresentationTypeList().GetAllCodes())
					{
						jobDeclaration.JE_DeclarantType = declarantType;
						jobDeclaration.JE_OA_DeclarantAddress = declarantAddressWithoutTAO;

						if ((procedure.StartsWith("42") || procedure.StartsWith("63")) &&
							(declarantType == RepresentationTypeList.Codes._1Self || declarantType == RepresentationTypeList.Codes._3Indirect))
						{
							AssertHasMessageError($"{declarantType}, {procedure}, no TAO", jobDeclaration.JE_OA_DeclarantAddressInfo, message);

							jobDeclaration.JE_OA_DeclarantAddress = declarantAddressWithTAO;
							AssertNoMessageError($"{declarantType}, {procedure}, has TAO", jobDeclaration.JE_OA_DeclarantAddressInfo, message);
						}
						else
						{
							AssertNoMessageError($"{declarantType}, {procedure}, no TAO", jobDeclaration.JE_OA_DeclarantAddressInfo, message);
						}
					}
				}
			});
		}

		public void TestCheckJE_OA_DeclarantAddress_CompanyIsNotDE()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var deCountryData = declarant.GetCountryData(Core.Constants.CountryCodes.Germany);
			((DEOrgImpAddInfo)deCountryData.ImpAddInfo).ZO_VATClaimBack = YesNoList.Codes.Yes;
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				AssertNoExceptionThrown(() => jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress());
			}
		}

		public void TestCheckJE_OA_BuyingAgentAddress()
		{
			const string message = "Please enter a Represented Party for Rep. Type 'IND'.";
			jobDeclaration.JE_OA_BuyingAgentAddress = ZGuid.Empty;

			CombineAssertions(() =>
			{
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				jobDeclaration.Validation.ValidateJE_OA_BuyingAgentAddress();
				AssertNoMessageError("JE_DeclarantType SEL, no JE_OA_BuyingAgentAddress", jobDeclaration.JE_OA_BuyingAgentAddressInfo, message);

				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				jobDeclaration.Validation.ValidateJE_OA_BuyingAgentAddress();
				AssertNoMessageError("JE_DeclarantType DIR, no JE_OA_BuyingAgentAddress", jobDeclaration.JE_OA_BuyingAgentAddressInfo, message);

				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				jobDeclaration.Validation.ValidateJE_OA_BuyingAgentAddress();
				AssertHasMessageError("JE_DeclarantType IND, no JE_OA_BuyingAgentAddress", jobDeclaration.JE_OA_BuyingAgentAddressInfo, message);

				jobDeclaration.JE_OA_BuyingAgentAddress = Factory.New<OrgHeader>().MainAddress.PK;
				AssertNoMessageError("JE_DeclarantType IND, has JE_OA_BuyingAgentAddress", jobDeclaration.JE_OA_BuyingAgentAddressInfo, message);
			});
		}

		public void TestCheckJE_VesselName()
		{
			jobDeclaration.JE_VesselName = ZString.Empty;

			var mandatoryModes = new List<string>() { ImportBorderTransportMeansList.Codes.Other };
			var otherModes = new List<string>() { ImportBorderTransportMeansList.Codes.Aircraft, ImportBorderTransportMeansList.Codes.Truck, ImportBorderTransportMeansList.Codes.Without, ImportBorderTransportMeansList.Codes.Car, ImportBorderTransportMeansList.Codes.Vessel, ImportBorderTransportMeansList.Codes.Wagon };

			CheckValidation(mandatoryModes, true);
			CheckValidation(otherModes, false);

			void CheckValidation(List<string> list, bool shouldHaveMessageError)
			{
				foreach (var item in list)
				{
					jobDeclaration.ZG_BorderTransportMeans = item;
					jobDeclaration.Validation.ValidateJE_VesselName();

					if (shouldHaveMessageError)
					{
						AssertHasMessageErrorContaining(jobDeclaration.JE_VesselNameInfo, "mandatory");
					}
					else
					{
						AssertNoMessageErrorContaining(jobDeclaration.JE_VesselNameInfo, "mandatory");
					}
				}
			}
		}

		public void TestCheckJE_VesselName_MaxLength()
		{
			const string message = "The maximum length for [21] Vessel is 17 characters.";
			CombineAssertions(() =>
			{
				jobDeclaration.JE_VesselName = ZString.Empty.PadLeft(18, 'A');
				AssertHasWarning("18 characters", jobDeclaration.JE_VesselNameInfo, message);
				jobDeclaration.JE_VesselName = ZString.Empty.PadLeft(17, 'A');
				AssertNoWarning("17 characters", jobDeclaration.JE_VesselNameInfo, message);
			});
		}

		public void TestCheckJE_RN_NKTransportNationality()
		{
			jobDeclaration.JE_RN_NKTransportNationality = ZString.Empty;

			var mandatoryTransportModes = new List<string>() { TransportTypeList.Codes.Air, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Road, TransportTypeList.Codes.Sea };
			var otherTransportModes = new List<string>() { TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.Mail, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.Rail };

			CheckValidation(mandatoryTransportModes, true);
			CheckValidation(otherTransportModes, false);

			void CheckValidation(List<string> list, bool shouldHaveMessageError)
			{
				foreach (var item in list)
				{
					jobDeclaration.JE_TransportMode = item;
					jobDeclaration.Validation.ValidateJE_RN_NKTransportNationality();

					if (shouldHaveMessageError)
					{
						AssertHasMessageErrorContaining(jobDeclaration.JE_RN_NKTransportNationalityInfo, "mandatory");
					}
					else
					{
						AssertNoMessageErrorContaining(jobDeclaration.JE_RN_NKTransportNationalityInfo, "mandatory");
					}
				}
			}
		}

		public void TestCheckJE_PaymentMethod()
		{
			CombineAssertions(() =>
			{
				jobDeclaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
				jobDeclaration.Validation.ValidateJE_PaymentMethod();
				AssertHasMessageErrorContaining(jobDeclaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);
				jobDeclaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.F;
				jobDeclaration.Validation.ValidateJE_PaymentMethod();
				AssertHasMessageErrorContaining(jobDeclaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);
				jobDeclaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.G;
				jobDeclaration.Validation.ValidateJE_PaymentMethod();
				AssertHasMessageErrorContaining(jobDeclaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);
				jobDeclaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.Z;
				jobDeclaration.Validation.ValidateJE_PaymentMethod();
				AssertHasMessageErrorContaining(jobDeclaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);

				jobDeclaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.A;
				jobDeclaration.Validation.ValidateJE_PaymentMethod();
				AssertNoMessageErrorContaining(jobDeclaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);

				jobDeclaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.Declarant;
				jobDeclaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.Z;
				jobDeclaration.Validation.ValidateJE_PaymentMethod();
				AssertNoMessageErrorContaining(jobDeclaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJE_OA_DeclarantAddress_InvoiceLineSupportingDocsRequiringEndUseAuthorization()
		{
			const string errorMessage = "The Declarant must have an End Use Authorization (EUS) for this declaration.";
			var orgHeader = Factory.New<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = orgHeader.Addresses.AddNew().PK;
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N990;
			invoiceLine.SupportingDocuments.AddNew().CSI_Code = "9002";

			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
				AssertNoMessageError("Doesn't require EUS Authorisation", jobDeclaration.JE_OA_DeclarantAddressInfo, errorMessage);

				var supportingDoc = invoiceLine.SupportingDocuments.AddNew();
				foreach (var type in CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation)
				{
					supportingDoc.CSI_Code = type;
					jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
					AssertHasMessageError($"{type}, No EUS Authorisation", jobDeclaration.JE_OA_DeclarantAddressInfo, errorMessage);
				}

				orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "DEEUS123");

				foreach (var type in CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation)
				{
					supportingDoc.CSI_Code = type;
					jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
					AssertNoMessageError($"{type}, has EUS Authorisation", jobDeclaration.JE_OA_DeclarantAddressInfo, errorMessage);
				}
			});
		}

		public void TestJE_ShipmentIncoTerm()
		{
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredAtFrontier;
			AssertNoMessageErrors(jobDeclaration.JE_ShipmentIncoTermInfo);
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredExShip;
			AssertNoMessageErrors(jobDeclaration.JE_ShipmentIncoTermInfo);
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredExQuay;
			AssertNoMessageErrors(jobDeclaration.JE_ShipmentIncoTermInfo);
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredDutyUnpaid;
			AssertNoMessageErrors(jobDeclaration.JE_ShipmentIncoTermInfo);
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertNoMessageErrors(jobDeclaration.JE_ShipmentIncoTermInfo);
		}

		public void TestCheck_MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantitiesCoreValue()
		{
			AssertEquals("Total Gross Weight on declaration must equal the sum of the Gross Weight of all lines.", jobDeclaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);
		}

		public void TestCheckJE_ContainerMode_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_ContainerModeInfo);
		}

		public void TestCheckJE_ContainerMode_ListValidation()
		{
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_ContainerModeInfo, "XX", Core.Constants.ContainerModes.Loose);
		}

		public void TestCheckJE_RS_NKServiceLevel_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_RS_NKServiceLevelInfo);
		}

		public void TestCheckJE_RS_NKServiceLevel_ListValidation()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "GGG";

			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_RS_NKServiceLevelInfo, "XX", "GGG");
		}

		public void TestCheckJE_ApplicationCode_InvalidCodeOrEmpty()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(jobDeclaration.JE_ApplicationCodeInfo, "XXX", DeclarationApplicationCodeList.Codes.Builtin);
		}

		public void TestCheckJE_OwnerRefEmptyWarning() => CombineAssertions(() =>
		{
			const string warningMessage = "If Declarant's Reference is empty, Declaration Reference (B00069) will be determined as Local Reference Number and sent to Customs.";
			jobDeclaration.JE_DeclarationReference = "B00069";

			jobDeclaration.Validation.ValidateJE_OwnerRef();
			AssertHasWarning("Has warning", jobDeclaration.JE_OwnerRefInfo, warningMessage);

			jobDeclaration.JE_OwnerRef = "Owner reference";
			AssertNoWarning("Has no warning", jobDeclaration.JE_OwnerRefInfo, warningMessage);
		});

		public void TestCheckJE_OwnerRefLengthWarning()
		{
			var warningMessage = $"As the max. length for LRN is 22 characters, {BrandingFactory.Instance.ProductName} will use the first 22 characters of Declarant's Reference to create the LRN.";

			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				CombineAssertions(() =>
				{
					jobDeclaration.JE_OwnerRef = "Owner reference";
					AssertNoWarning("Has no length warning while less than 22 on ATLAS 10.1", jobDeclaration.JE_OwnerRefInfo, warningMessage);

					jobDeclaration.JE_OwnerRef = "Longer owner reference number";
					AssertHasWarning("Has length warning while longer than 22 on ATLAS 10.1", jobDeclaration.JE_OwnerRefInfo, warningMessage);
				});
			}
		}

		public void TestCheckJE_OwnerRef()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = jobDeclaration.PK;
			jobDeclaration.Validation.ValidateJE_OwnerRef();
			AssertNoNotifications(jobDeclaration.JE_OwnerRefInfo);
		}

		public void TestCheckJE_GS_NKCusAgent()
		{
			const string missingWorkPhoneErrorMessage = "Please add the 'Work Phone' to the Broker's user profile. This is mandatory information for customs messages.";
			const string missingJobTitleErrorMessage = "Please add the 'Job Title' to the Broker's user profile. This is mandatory information for customs messages.";
			var staff = CreateStaffRecord();

			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertNoMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingWorkPhoneErrorMessage);
				AssertNoMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingJobTitleErrorMessage);

				jobDeclaration.JE_GS_NKCusAgent = staff.GS_Code;
				AssertNoMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingWorkPhoneErrorMessage);
				AssertNoMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingJobTitleErrorMessage);

				staff.GS_WorkPhone = string.Empty;
				staff.GS_Title = string.Empty;
				jobDeclaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertHasMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingWorkPhoneErrorMessage);
				AssertHasMessageError(jobDeclaration.JE_GS_NKCusAgentInfo, missingJobTitleErrorMessage);
			});
		}

		public void TestCheckJE_TotalWeight()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = jobDeclaration.PK;
			jobDeclaration.Validation.ValidateJE_TotalWeight();
			AssertNoNotifications(jobDeclaration.JE_TotalWeightInfo);
		}

		GlbStaff CreateStaffRecord()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_WorkPhone = "06131474747";
			staff.GS_Title = "Builder";
			return staff;
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		}
		JobDeclaration jobDeclaration;

		void AssertJE_RL_NKFinalDestinationMandatory(ZString instructionStyle)
		{
			const string errorMessage = "You have not entered a [17] Destination.";
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			jobDeclaration.CustomsEntryInstructions.AddNew().CEI_Style = ImportDeclarationTypeList.Codes.VZA;

			CombineAssertions(() =>
			{
				jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
				AssertNoMessageError($"No {instructionStyle}", jobDeclaration.JE_RL_NKFinalDestinationInfo, errorMessage);

				jobDeclaration.CustomsEntryInstructions.AddNew().CEI_Style = instructionStyle;
				jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
				AssertHasMessageError($"Has {instructionStyle}", jobDeclaration.JE_RL_NKFinalDestinationInfo, errorMessage);

				jobDeclaration.JE_RL_NKFinalDestination = "XYZ";
				AssertNoMessageError("Has JE_RL_NKFinalDestination", jobDeclaration.JE_RL_NKFinalDestinationInfo, errorMessage);
			});
		}

		void AssertJE_RL_NKFinalDestinationStateMandatory(string instructionStyle)
		{
			const string errorMessage = "You have not entered a State for this [17] Destination.";
			var germanRefUNLOCO = CreateNewOrGetExistingRefUNLOCO(Core.Constants.CountryCodes.Germany, "DEHAM", ZGuid.Empty);
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			jobDeclaration.CustomsEntryInstructions.AddNew().CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			jobDeclaration.JE_RL_NKFinalDestination = germanRefUNLOCO.Code;

			CombineAssertions(() =>
			{
				AssertNoMessageError($"No {instructionStyle}", jobDeclaration.JE_RL_NKFinalDestinationInfo, errorMessage);

				jobDeclaration.CustomsEntryInstructions.AddNew().CEI_Style = instructionStyle;
				jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
				AssertHasMessageError($"Has {instructionStyle}, German destination, no State", jobDeclaration.JE_RL_NKFinalDestinationInfo, errorMessage);

				var state = CreateNewOrGetExistingRefCountryStates(Core.Constants.CountryCodes.Germany, "HH", "Hamburg");
				germanRefUNLOCO.RL_RW = state.PK;
				jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
				AssertNoMessageError("Has State", jobDeclaration.JE_RL_NKFinalDestinationInfo, errorMessage);

				var nonGermanRefUNLOCO = CreateNewOrGetExistingRefUNLOCO(Core.Constants.CountryCodes.UnitedStates, "USATL", ZGuid.Empty);
				jobDeclaration.JE_RL_NKFinalDestination = nonGermanRefUNLOCO.Code;
				AssertNoMessageError("Non-German destination, no State", jobDeclaration.JE_RL_NKFinalDestinationInfo, errorMessage);
			});
		}

		RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString country, ZString code, ZGuid statePK)
		{
			var result = new RefUNLOCO.Loader(Factory).Load(code);
			if (result == null)
			{
				result = Factory.New<RefUNLOCO>();
				result.RL_Code = code;
			}
			result.RL_RN_NKCountryCode = country;
			result.RL_RW = statePK;
			return result;
		}

		RefCountryStates CreateNewOrGetExistingRefCountryStates(ZString country, ZString code, ZString description)
		{
			var result = new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode(code, country);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<RefCountryStates>();
				result.RW_RN_NKCountryCode = country;
				result.RW_Code = code;
				result.RW_Description = description;
			}
			return result;
		}
	}
}
