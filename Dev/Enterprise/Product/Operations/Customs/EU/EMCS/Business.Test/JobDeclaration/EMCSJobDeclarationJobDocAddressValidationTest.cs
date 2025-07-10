using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSJobDeclarationJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSupplierDocumentaryAddressMandatory()
		{
			AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.SupplierDocumentaryAddress);
		}

		public void TestSupplierDocumentaryAddressHasTraderExciseNumber()
		{
			AssertCustomsRegistrationNumberMandatory(DocAddressTypes.Codes.SupplierDocumentaryAddress, "Consignor", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
		}

		public void TestImporterDocumentaryAddressMandatory()
		{
			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.ImporterDocumentaryAddress;

			CombineAssertions(() =>
			{
				foreach (var submissionType in declaration.AddInfoLookups.SubmissionTypeList.GetAllCodes())
				{
					declaration.ZG_SubmissionType = submissionType;
					foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
					{
						declaration.JE_MessageSubType = messageSubType;
						jobDocAddress.OrganisationPK = ZGuid.Empty;
						jobDocAddress.Validation.ValidateOrganisationPK();

						if (declaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown && declaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForExport)
						{
							AssertHasMessageErrorContaining($"JE_MessageSubType {declaration.JE_MessageSubType}, ZG_SubmissionType {declaration.ZG_SubmissionType}, No Consignee", jobDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

							jobDocAddress.OrganisationPK = orgHeader.PK;
							AssertNoMessageErrorContaining($"JE_MessageSubType {declaration.JE_MessageSubType}, ZG_SubmissionType {declaration.ZG_SubmissionType}, Has Consignee", jobDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
						}
						else
						{
							AssertNoMessageErrorContaining($"JE_MessageSubType {declaration.JE_MessageSubType}, ZG_SubmissionType {declaration.ZG_SubmissionType}, Consignee Not Required", jobDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
						}
					}
				}
			});
		}

		public void TestImporterDocumentaryAddressTraderExciseNumber()
		{
			var expectedMessageError = $"Consignee must have a Registration Number / Code of type '{OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber}'";
			var messageSubTypesRequiringTEN = new HashSet<string>()
			{
				EMCSDestinationTypeList.Codes.DestinationTaxWarehouse,
				EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee,
				EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee,
				EMCSDestinationTypeList.Codes.DestinationDirectDelivery
			};

			var cusCodeTEN = Factory.New<OrgCusCode>();
			cusCodeTEN.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Greece;
			cusCodeTEN.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
			cusCodeTEN.OK_CustomsRegNo = "123456789";

			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = orgHeader.PK;

			CombineAssertions(() =>
			{
				foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
				{
					declaration.JE_MessageSubType = messageSubType;
					if (messageSubTypesRequiringTEN.Contains(messageSubType))
					{
						jobDocAddress.Validation.ValidateOrganisationPK();
						AssertHasMessageError($"JE_MessageSubType {messageSubType}, No TEN Code", jobDocAddress.OrganisationPKInfo, expectedMessageError);

						orgHeader.CustomsCodes.Add(cusCodeTEN);
						jobDocAddress.Validation.ValidateOrganisationPK();
						AssertNoMessageError($"JE_MessageSubType {messageSubType}, Has TEN Code", jobDocAddress.OrganisationPKInfo, expectedMessageError);
						orgHeader.CustomsCodes.RemoveAll();
					}
					else
					{
						AssertNoMessageError($"JE_MessageSubType {messageSubType}, Not Required TEN Code", jobDocAddress.OrganisationPKInfo, expectedMessageError);
					}
				}
			});
		}

		public void TestValidateAddress()
		{
			var addressToTest = orgHeader.Addresses.AddNew();
			addressToTest.OA_PostCode = string.Empty;
			addressToTest.OA_City = string.Empty;

			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.ImporterDocumentaryAddress;
			jobDocAddress.E2_OA_Address = addressToTest.PK;

			var targetInfo = jobDocAddress.OrganisationPKInfo;

			const string postCodeMessage = "Consignee must have a valid Postcode filled in.";
			const string cityMessage = "Consignee must have a valid Postcode filled in.";

			CombineAssertions("Empty check on org. address fields.", () =>
			{
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Consignee should have Postcode", targetInfo, postCodeMessage);
				AssertHasMessageError("Consignee should have City", targetInfo, cityMessage);

				addressToTest.OA_PostCode = "20313";
				addressToTest.OA_City = "PARIS";

				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Consignee should have Postcode(passed)", targetInfo, postCodeMessage);
				AssertNoMessageError("Consignee should have City(passed)", targetInfo, cityMessage);

				jobDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
				addressToTest.OA_PostCode = string.Empty;
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Consignor should have Postcode", targetInfo, "Consignor must have a valid Postcode filled in.");
			});
		}

		public void TestCarrierAgentDocumentaryAddressMandatoryForOwnerOfGoods()
		{
			declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.OwnerOfGoods;
			AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.CarrierAgent);
		}

		public void TestCarrierAgentDocumentaryAddressMandatoryForOther()
		{
			declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.CarrierAgent);
		}

		public void TestCarrierAgentDocumentaryAddressFields()
		{
			declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			var docAddress = orgHeader.Addresses.AddNew();
			var carrierAgentAddress = declaration.CarrierAgentDocumentaryAddress;
			carrierAgentAddress.E2_OA_Address = docAddress.PK;
			AssertJobDocAddressFields(carrierAgentAddress, docAddress);
		}

		public void TestTransporterDocumentaryAddressMandatory()
		{
			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.JointGuaranteeOfTheConsignorOfTheTransporterAndOfTheConsignee;
			AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.Transporter);
		}

		public void TestTransporterDocumentaryAddressFields()
		{
			var docAddress = orgHeader.Addresses.AddNew();
			var transporterDocumentaryAddress = declaration.TransporterDocumentaryAddress;
			transporterDocumentaryAddress.E2_OA_Address = docAddress.PK;
			AssertJobDocAddressFields(transporterDocumentaryAddress, docAddress);
		}

		public void TestDestinationWarehouseDocumentaryAddressMandatoryForDestinationTaxWarehouse()
		{
			if (DestinationTypesForDestinationWarehouseTest.Contains(EMCSDestinationTypeList.Codes.DestinationTaxWarehouse))
			{
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.DestinationWarehouse);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDestinationWarehouseDocumentaryAddressMandatoryForDestinationTemporaryRegisteredConsignee()
		{
			if (DestinationTypesForDestinationWarehouseTest.Contains(EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee))
			{
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee;
				AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.DestinationWarehouse);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDestinationWarehouseDocumentaryAddressMandatoryForDestinationDirectDelivery()
		{
			if (DestinationTypesForDestinationWarehouseTest.Contains(EMCSDestinationTypeList.Codes.DestinationDirectDelivery))
			{
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationDirectDelivery;
				AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.DestinationWarehouse);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDestinationWarehouseDocumentaryAddressMandatoryForDestinationExemptedConsignee()
		{
			if (DestinationTypesForDestinationWarehouseTest.Contains(EMCSDestinationTypeList.Codes.DestinationExemptedConsignee))
			{
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
				AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.DestinationWarehouse);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDestinationWarehouseDocumentaryAddressTraderID()
		{
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			AssertCustomsRegistrationNumberMandatory(DocAddressTypes.Codes.DestinationWarehouse, "Destination Warehouse", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);
		}

		public void TestDispatchWarehouseDocumentaryAddressMandatory()
		{
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
			AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.DispatchWarehouse);
		}

		public void TestDispatchWarehouseDocumentaryAddressTraderID()
		{
			declaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.StandardSubmission;
			AssertCustomsRegistrationNumberMandatory(DocAddressTypes.Codes.DispatchWarehouse, "Dispatch Warehouse", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);
			orgHeader.CustomsCodes.RemoveAll();
			declaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.SubmissionForExport;
			AssertCustomsRegistrationNumberMandatory(DocAddressTypes.Codes.DispatchWarehouse, "Dispatch Warehouse", OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);

			orgHeader.CustomsCodes.RemoveAll();
			declaration.DocAddresses.RemoveAll();
			declaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B;
			var expectedMessageError = $"Dispatch Warehouse must have a Registration Number / Code of type '" + OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID + "'";
			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.DispatchWarehouse;
			jobDocAddress.OrganisationPK = orgHeader.PK;
			jobDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError($"Dispatch Warehouse: No TID", jobDocAddress.OrganisationPKInfo, expectedMessageError);
		}

		public void TestGoodsOwnerDocumentaryAddressMandatory()
		{
			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts;
			AssertDocumentaryAddressMandatory(DocAddressTypes.Codes.GoodsOwner);
		}

		public void TestCheckE2_GovRegNumType()
		{
			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.GoodsOwner;
			jobDocAddress.E2_AddressOverride = true;

			CombineAssertions(() =>
			{
				jobDocAddress.E2_GovRegNumType = ZString.Empty;
				AssertNoMessageErrorContaining("E2_GovRegNumType is empty and E2_GovRegNum is empty, no 'YouHaveNotEntered' error", jobDocAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining("E2_GovRegNumType is empty and E2_GovRegNum is empty, no 'InvalidCodeMessageError' error", jobDocAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				jobDocAddress.E2_GovRegNum = "12345678";
				jobDocAddress.Validation.ValidateE2_GovRegNumType();
				AssertHasMessageErrorContaining("E2_GovRegNumType is empty and E2_GovRegNum isn't empty, has 'YouHaveNotEntered' error", jobDocAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);

				jobDocAddress.E2_GovRegNumType = "AAA";
				AssertHasMessageErrorContaining("E2_GovRegNumType is invalid and E2_GovRegNum isn't empty, has 'InvalidCodeMessageError' error", jobDocAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckE2_GovRegNum()
		{
			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.GoodsOwner;
			jobDocAddress.E2_AddressOverride = true;

			CombineAssertions(() =>
			{
				jobDocAddress.E2_GovRegNumType = ZString.Empty;
				jobDocAddress.Validation.ValidateE2_GovRegNum();
				AssertNoMessageErrorContaining("E2_GovRegNumType is empty and E2_GovRegNum is empty", jobDocAddress.E2_GovRegNumInfo, MandatoryValidation.YouHaveNotEntered);

				jobDocAddress.E2_GovRegNumType = GovRegNumTypeList.Codes.EoriCode;
				jobDocAddress.Validation.ValidateE2_GovRegNum();
				AssertHasMessageErrorContaining("E2_GovRegNumType isn't empty and E2_GovRegNum is empty", jobDocAddress.E2_GovRegNumInfo, MandatoryValidation.YouHaveNotEntered);

				jobDocAddress.E2_GovRegNum = "12345678";
				AssertNoMessageErrorContaining("E2_GovRegNumType isn't empty and E2_GovRegNum isn't empty", jobDocAddress.E2_GovRegNumInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "GBLON";
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		OrgHeader orgHeader;
		EMCSJobDeclaration declaration;

		protected virtual HashSet<ZString> DestinationTypesForDestinationWarehouseTest => new HashSet<ZString>
		{
			EMCSDestinationTypeList.Codes.DestinationTaxWarehouse,
			EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee,
			EMCSDestinationTypeList.Codes.DestinationDirectDelivery,
			EMCSDestinationTypeList.Codes.DestinationExemptedConsignee
		};

		void AssertDocumentaryAddressMandatory(string docAddressTypeCode)
		{
			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = docAddressTypeCode;
			jobDocAddress.OrganisationPK = ZGuid.Empty;
			jobDocAddress.Validation.ValidateOrganisationPK();
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining($"{docAddressTypeCode}: Has Mandatory Message Error", jobDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
				jobDocAddress.OrganisationPK = orgHeader.PK;
				AssertNoMessageErrorContaining($"{docAddressTypeCode}: No Mandatory Message Error", jobDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		void AssertCustomsRegistrationNumberMandatory(ZString docAddressTypeCode, ZString name, ZString customsCodeType)
		{
			var expectedMessageError = $"{name} must have a Registration Number / Code of type '{customsCodeType}'";
			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = docAddressTypeCode;

			jobDocAddress.OrganisationPK = orgHeader.PK;
			CombineAssertions(() =>
			{
				AssertHasMessageError($"{name}: No {customsCodeType}", jobDocAddress.OrganisationPKInfo, expectedMessageError);

				orgHeader.CustomsCodes.AddNew(customsCodeType, "123456789", Core.Constants.CountryCodes.Greece);
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError($"{name}: Has {customsCodeType}", jobDocAddress.OrganisationPKInfo, expectedMessageError);
			});
		}

		void AssertJobDocAddressFields(JobDocAddress jobDocAddress, OrgAddress orgAddress)
		{
			const string address1MessageError = "Address 1 is Required";
			const string cityMessageError = "City is Required";
			const string postcodeMessageError = "Postcode is Required";
			const string companyNameMessageError = "Company Name is Required";

			CombineAssertions(() =>
			{
				AssertHasMessageError("Address Error", jobDocAddress.E2_OA_AddressInfo, address1MessageError);
				AssertHasMessageError("City Error", jobDocAddress.E2_OA_AddressInfo, cityMessageError);
				AssertHasMessageError("Postcode Error", jobDocAddress.E2_OA_AddressInfo, postcodeMessageError);
				AssertHasMessageError("Company Name Error", jobDocAddress.E2_OA_AddressInfo, companyNameMessageError);

				orgAddress.Address1 = "ENTERED ADDRESS 1";
				jobDocAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("No Address Error", jobDocAddress.E2_OA_AddressInfo, address1MessageError);

				orgAddress.City = "CITY";
				jobDocAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("No City Error", jobDocAddress.E2_OA_AddressInfo, cityMessageError);

				orgAddress.Postcode = "123";
				jobDocAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("No Postcode Error", jobDocAddress.E2_OA_AddressInfo, postcodeMessageError);

				orgAddress.CompanyName = "Freds Bricks";
				jobDocAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("No Company Name Error", jobDocAddress.E2_OA_AddressInfo, companyNameMessageError);
			});
		}
	}
}
