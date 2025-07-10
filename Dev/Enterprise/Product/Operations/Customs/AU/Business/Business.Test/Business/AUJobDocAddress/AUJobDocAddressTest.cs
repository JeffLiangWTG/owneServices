using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUJobDocAddress))]
	sealed class AUJobDocAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPropertyAttributes()
		{
			var docAddress = Factory.New<AUJobDocAddress>();
			AssertEquals("EXDOCEstablishmentNumber.MaxLength", 6, docAddress.EXDOCEstablishmentNumberInfo.MaxLength);
		}

		public void TestAQISLoadingEstablishment()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();

			var aqisEstablishment = NewOrgWithTestCusCode("TAQS", "77");
			var org1 = NewOrgWithTestCusCode("OGH1", "88");
			var org2 = NewOrgWithTestCusCode("OGH2", "88");

			var locationAddress = invoice.AQISLoadingEstablishmentLocation;
			Assert("ShouldClearAddressFieldsWhenOverride", locationAddress.ShouldClearAddressFieldsWhenOverride);

			locationAddress.E2_OA_Address = aqisEstablishment.MainAddress.PK;
			AssertEquals("JobDocAddress.E2_GovRegNumType", OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, locationAddress.E2_GovRegNumType);
			AssertEquals("Select a address, update E2_GovRegNum", "77", locationAddress.E2_GovRegNum);
			AssertEquals("Select a address, update UserInputedGovRegNum", "77", locationAddress.EXDOCEstablishmentNumber);

			locationAddress.EXDOCEstablishmentNumber = "99";
			CombineAssertions("Input a ID which not link to any Address", () =>
			{
				Assert("Override Address", locationAddress.E2_AddressOverride);
				AssertNullOrEmpty("E2_CompanyName", locationAddress.E2_CompanyName);
				AssertEquals("E2_GovRegNumType", OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, locationAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "99", locationAddress.E2_GovRegNum);
				AssertEquals("UserInputedGovRegNum", "99", locationAddress.EXDOCEstablishmentNumber);

				locationAddress.E2_AddressOverride = false;
				Assert("Clean E2_OA_Address before override", locationAddress.E2_OA_Address.IsEmpty);
			});

			locationAddress.EXDOCEstablishmentNumber = "77";
			CombineAssertions("Input a ID which can link to one Address", () =>
			{
				Assert("Not override Address", !locationAddress.E2_AddressOverride);
				AssertEquals("E2_OA_Address", aqisEstablishment.MainAddress.PK, locationAddress.E2_OA_Address);
				AssertEquals("E2_GovRegNumType", OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, locationAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "77", locationAddress.E2_GovRegNum);
				AssertEquals("UserInputedGovRegNum", "77", locationAddress.EXDOCEstablishmentNumber);
			});

			locationAddress.EXDOCEstablishmentNumber = "88";
			CombineAssertions("Input a ID which can link to more than one Address", () =>
			{
				Assert("Override Address", locationAddress.E2_AddressOverride);
				AssertNullOrEmpty("E2_CompanyName", locationAddress.E2_CompanyName);
				AssertEquals("E2_GovRegNumType", OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, locationAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "88", locationAddress.E2_GovRegNum);
				AssertEquals("UserInputedGovRegNum", "88", locationAddress.EXDOCEstablishmentNumber);

				locationAddress.E2_AddressOverride = false;
				Assert("Clean E2_OA_Address before override", locationAddress.E2_OA_Address.IsEmpty);
			});

			OrgHeader NewOrgWithTestCusCode(string orgCode, string cusCode)
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = orgCode;
				org.OH_FullName = $"{orgCode} Org";
				var mainAddress = org.MainAddress;
				mainAddress.Address1 = $"{orgCode} 185 O'RIORDAN ST";
				mainAddress.City = "MASCOT";
				mainAddress.State = "NSW";
				mainAddress.OA_RN_NKCountryCode = "AU";
				mainAddress.Postcode = "2020";
				var esnCusCode = mainAddress.CustomsCodes.AddNew();
				esnCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				esnCusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
				esnCusCode.OK_CustomsRegNo = cusCode;
				return org;
			}
		}

		public void TestApprovalNumber_Set()
		{
			var testAddress = GetTestAUJobDocAddress();
			testAddress.ApprovalNumber = "1234";
			var jobDocAddressNumbers = testAddress.DocAddressNumbers.Cast<JobDocAddressNumber>().Where(x => x.E2N_NumberType == OrgCusCode.CodeTypes.EUTracesID).ToArray();
			AssertEquals(1, jobDocAddressNumbers.Length);
			AssertEquals("1234", jobDocAddressNumbers[0].E2N_Number);
			AssertEquals("AU", jobDocAddressNumbers[0].E2N_RN_NKCountryCode);

			testAddress.ApprovalNumber = "3456";
			jobDocAddressNumbers = testAddress.DocAddressNumbers.Cast<JobDocAddressNumber>().Where(x => x.E2N_NumberType == OrgCusCode.CodeTypes.EUTracesID).ToArray();
			AssertEquals(1, jobDocAddressNumbers.Length);
			AssertEquals("3456", jobDocAddressNumbers[0].E2N_Number);
			AssertEquals("AU", jobDocAddressNumbers[0].E2N_RN_NKCountryCode);

			testAddress.ApprovalNumber = "";
			jobDocAddressNumbers = testAddress.DocAddressNumbers.Cast<JobDocAddressNumber>().Where(x => x.E2N_NumberType == OrgCusCode.CodeTypes.EUTracesID).ToArray();
			AssertEquals(0, jobDocAddressNumbers.Length);
		}

		public void TestApprovalNumber_Get()
		{
			var testAddress = GetTestAUJobDocAddress();
			var jobDocAddressNumber = testAddress.DocAddressNumbers.AddNew();
			jobDocAddressNumber.E2N_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			jobDocAddressNumber.E2N_NumberType = OrgCusCode.CodeTypes.EUTracesID;
			jobDocAddressNumber.E2N_Number = "1234";
			AssertEquals("1234", testAddress.ApprovalNumber);

			testAddress.E2_AddressOverride = false;
			var orgHeader = Factory.New<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.EUTracesID;
			cusCode.OK_CustomsRegNo = "4567";
			testAddress.OrganisationPK = orgHeader.PK;
			AssertEquals("4567", testAddress.ApprovalNumber);
		}

		public void TestE2_AddressOverrideInfo_ValueChanged()
		{
			var testAddress = GetTestAUJobDocAddress();
			var orgHeader = Factory.New<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.EUTracesID;
			cusCode.OK_CustomsRegNo = "4567";
			testAddress.OrganisationPK = orgHeader.PK;

			testAddress.E2_AddressOverride = false;
			AssertEquals("4567", testAddress.ApprovalNumber);

			testAddress.E2_AddressOverride = true;
			AssertEquals("4567", testAddress.ApprovalNumber);
		}

		protected override BusinessObject GetNewBusinessObject() => GetTestAUJobDocAddress();

		AUJobDocAddress GetTestAUJobDocAddress()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var testAddress = invoice.AQISEUPlaceOfDestination;
			testAddress.E2_AddressOverride = true;
			return testAddress;
		}
	}
}
