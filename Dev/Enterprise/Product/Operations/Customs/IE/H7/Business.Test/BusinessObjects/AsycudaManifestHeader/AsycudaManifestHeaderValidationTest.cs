using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_AgentType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_AgentType = ZString.Empty;
			AssertHasMessageError(header.AMA_AgentTypeInfo, "[C0638] You have not entered a Rep. Status.");

			header.AMA_AgentType = "ABC";
			AssertHasMessageError(header.AMA_AgentTypeInfo, "[BR0020] The code you have selected is not in the list.");

			header.AMA_AgentType = "DIR";
			AssertNoMessageErrors(header.AMA_AgentTypeInfo);
		}

		public void TestValidateCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "customs office");
			var mockTypeCodes = new List<string> { "IEARK100", "IEATH200" };
			mockTypeCodes.ForEach(li => helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				li,
				"desc",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime));
			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			manifestHeader.AMA_CustomsOffice = "IEBTP100";
			manifestHeader.Validation.ValidateAMA_CustomsOffice();
			AssertHasMessageErrorContaining(manifestHeader.AMA_CustomsOfficeInfo, "[BR0020] The code you have selected is not in the list.");

			manifestHeader.AMA_CustomsOffice = "IEARK100";
			manifestHeader.Validation.ValidateAMA_CustomsOffice();
			AssertNoNotifications(manifestHeader.AMA_CustomsOfficeInfo);
		}

		public void TestValidateRepresentative()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_Representative = orgAddress.PK;
			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError("Contact info is required", header.AMA_OA_RepresentativeInfo, "'CUS - Customs' type contact is required when Representative is provided. It can be configured within the Organization > Contact > Allocated Contact.");

			var contact = orgHeader.Contacts.AddNew();
			var personalAlloc = contact.Allocations.AddNew();
			personalAlloc.PC_Type = OrgConstants.ContactAllocationType.CUS;
			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError("Contact phone number is required", header.AMA_OA_RepresentativeInfo, "Phone number is required for 'CUS - Customs' type contact.");
			AssertHasMessageError("Contact email address is required", header.AMA_OA_RepresentativeInfo, "Email address is required for 'CUS - Customs' type contact.");

			contact.OC_IsActive = false;
			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError("Contact info is still required if the only qualified contact is not active", header.AMA_OA_RepresentativeInfo, "'CUS - Customs' type contact is required when Representative is provided. It can be configured within the Organization > Contact > Allocated Contact.");
		}

		public void TestValidateDeclarant()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_Declarant = ZGuid.Empty;
			header.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError(header.AMA_OA_DeclarantInfo, "You have not entered a Declarant.");

			header.AMA_OA_Declarant = orgAddress.PK;
			header.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError(header.AMA_OA_DeclarantInfo, "You have not entered an EORI number for the Declarant.");
			AssertHasMessageError(header.AMA_OA_DeclarantInfo, "'CUS - Customs' type contact is required when Declarant is provided. It can be configured within the Organization > Contact > Allocated Contact.");

			var contact = orgHeader.Contacts.AddNew();
			var personalAlloc = contact.Allocations.AddNew();
			personalAlloc.PC_Type = OrgConstants.ContactAllocationType.CUS;
			header.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError(header.AMA_OA_DeclarantInfo, "Phone number is required for 'CUS - Customs' type contact.");
			AssertHasMessageError(header.AMA_OA_DeclarantInfo, "Email address is required for 'CUS - Customs' type contact.");

			var country = Factory.NewWithValidTestData<RefCountry>();
			country.Code = "AA";
			header.Declarant.OA_RN_NKCountryCode = country.Code;
			header.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError(header.AMA_OA_DeclarantInfo, "The Country/Region of Declarant is not valid.");

			contact.OC_IsActive = false;
			header.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError("Contact info is still required if the only qualified contact is not active", header.AMA_OA_DeclarantInfo, "'CUS - Customs' type contact is required when Declarant is provided. It can be configured within the Organization > Contact > Allocated Contact.");
		}

		public void TestValidatePaymentMethod()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var validPaymentMethodList = header.Lookups.MethodOfPaymentList;
			if (validPaymentMethodList is { Count: > 0 })
			{
				header.AMA_PaymentMethod = validPaymentMethodList.GetAllCodesZString()[0];
				header.Validation.ValidateAMA_PaymentMethod();
				AssertNoMessageErrors(header.AMA_PaymentMethodInfo);
			}

			header.AMA_PaymentMethod = "XXX";
			header.Validation.ValidateAMA_PaymentMethod();
			AssertHasMessageError(header.AMA_PaymentMethodInfo, "[BR0020] The code you have selected is not in the list.");
		}
	}
}
