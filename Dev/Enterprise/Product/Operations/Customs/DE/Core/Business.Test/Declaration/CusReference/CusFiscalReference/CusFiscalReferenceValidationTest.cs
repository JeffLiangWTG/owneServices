using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class CusFiscalReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_ReferenceNumber_Length_FR1()
		{
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;
			fiscalReference.CFR_Reference = "DE123";
			AssertHasMessageError("FR1-Has exact length error", fiscalReference.CFR_ReferenceInfo, "The length of Reference has to be 11 characters.");
			fiscalReference.CFR_Reference = "DE123456789";
			AssertNoMessageError("FR1-Has exact length error", fiscalReference.CFR_ReferenceInfo, "The length of Reference has to be 11 characters.");
		}

		public void TestCheckCFR_ReferenceNumber_Length_FR2()
		{
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;
			fiscalReference.CFR_Reference = "FR2DE1234567890123";
			AssertHasMessageError("FR2-Has max length error", fiscalReference.CFR_ReferenceInfo, "The max length of 'Reference' must be 14.");
			fiscalReference.CFR_Reference = "FR2123456789";
			AssertNoMessageError("FR2-No max length error", fiscalReference.CFR_ReferenceInfo, "The max length of 'Reference' must be 14.");
		}

		public void TestCheckCFR_ReferenceNumber_Length_FR3()
		{
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;
			fiscalReference.CFR_Reference = "DE123";
			AssertHasMessageError("FR3-Has exact length error", fiscalReference.CFR_ReferenceInfo, "The length of Reference has to be 11 characters.");
			fiscalReference.CFR_Reference = "DE123456789";
			AssertNoMessageError("FR3-Has exact length error", fiscalReference.CFR_ReferenceInfo, "The length of Reference has to be 11 characters.");
		}

		public void TestCheckCFR_ReferenceNumber_Length_FR5()
		{
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5;
			fiscalReference.CFR_Reference = "DE12345";
			AssertHasMessageError("FR5-Has exact length error", fiscalReference.CFR_ReferenceInfo, "The length of Reference has to be 12 characters.");
			fiscalReference.CFR_Reference = "DE1234567890";
			AssertNoMessageError("FR5-Has exact length error", fiscalReference.CFR_ReferenceInfo, "The length of Reference has to be 12 characters.");
		}

		public void TestCheckCFR_ReferenceNumber_Pattern_FR1()
		{
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;
			fiscalReference.CFR_Reference = "22222222222";
			AssertHasMessageError("FR1-Has start with error", fiscalReference.CFR_ReferenceInfo, "Reference for Code 'FR1' must start with 'DE'.");
			fiscalReference.CFR_Reference = "DE123456789";
			AssertNoMessageError("FR1-No start with error", fiscalReference.CFR_ReferenceInfo, "Reference for Code 'FR1' must start with 'DE'.");
		}

		public void TestCheckCFR_ReferenceNumber_Pattern_FR2()
		{
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2;
			fiscalReference.CFR_Reference = "DE222";
			AssertHasMessageError("FR2-Has start with error", fiscalReference.CFR_ReferenceInfo, "Reference for Type FR2 can't be issued in DE.");
			fiscalReference.CFR_Reference = "123456789";
			AssertNoMessageError("FR2-No start with error", fiscalReference.CFR_ReferenceInfo, "Reference for Type FR2 can't be issued in DE.");
		}

		public void TestCheckCFR_ReferenceNumber_Pattern_FR3()
		{
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3;
			fiscalReference.CFR_Reference = "222";
			AssertHasMessageError("FR3-Has start with error", fiscalReference.CFR_ReferenceInfo, "Reference for Code 'FR3' must start with 'DE'.");
			fiscalReference.CFR_Reference = "DE123456789";
			AssertNoMessageError("FR3-No start with error", fiscalReference.CFR_ReferenceInfo, "Reference for Code 'FR3' must start with 'DE'.");
		}

		public void TestCheckCFR_ReferenceNumber_Pattern_FR5()
		{
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5;
			fiscalReference.CFR_Reference = "222";
			AssertHasMessageError("FR5-Has start with error", fiscalReference.CFR_ReferenceInfo, "Reference for Code 'FR5' must start with 'IM'.");
			fiscalReference.CFR_Reference = "IM123456789";
			AssertNoMessageError("FR5-No start with error", fiscalReference.CFR_ReferenceInfo, "Reference for Code 'FR5' must start with 'IM'.");
		}

		public void TestCheckCFR_OA_Owner()
		{
			CombineAssertions(() =>
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = organisation.Addresses.AddNew();

				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5;
				fiscalReference.Validation.ValidateCFR_OA_Owner();
				AssertNoMessageErrorContaining(fiscalReference.CFR_OA_OwnerInfo, MandatoryValidation.YouHaveNotEntered);

				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1;
				fiscalReference.CFR_OA_Owner = ZGuid.Empty;
				AssertHasMessageErrorContaining(fiscalReference.CFR_OA_OwnerInfo, MandatoryValidation.YouHaveNotEntered);

				fiscalReference.CFR_OA_Owner = orgAddress.PK;
				AssertNoMessageErrorContaining(fiscalReference.CFR_OA_OwnerInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			fiscalReference = entryInstruction.FiscalReferences.AddNew();
		}
		CusFiscalReference fiscalReference;
	}
}
