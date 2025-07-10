using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusExitDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCED_MovementReferenceNumber_Length()
		{
			CombineAssertions("Check length of CED_MovementReferenceNumber", () =>
			{
				exitDetail.CED_MovementReferenceNumber = "21DE12345678901234";
				AssertNoNotifications("Valid MRN number", exitDetail.CED_MovementReferenceNumberInfo);

				exitDetail.CED_MovementReferenceNumber = "21DE1234567890123";
				AssertHasMessageErrorContaining("MRN number is less than 18 characters", exitDetail.CED_MovementReferenceNumberInfo, "Please enter a MRN in the following format with only numbers and upper case letters");
			});
		}

		public void TestCheckCED_MovementReferenceNumber_Format()
		{
			CombineAssertions("Check format of CED_MovementReferenceNumber", () =>
			{
				exitDetail.CED_MovementReferenceNumber = "21DE27364916384836";
				AssertNoNotifications("Valid MRN number", exitDetail.CED_MovementReferenceNumberInfo);

				exitDetail.CED_MovementReferenceNumber = "21DE27364916384835";
				AssertHasMessageErrorContaining("Invalid MRN check digit", exitDetail.CED_MovementReferenceNumberInfo, "MRN does not have a valid last digit");

				exitDetail.CED_MovementReferenceNumber = "21AB27364916384830";
				AssertHasMessageErrorContaining("Invalid MRN country code", exitDetail.CED_MovementReferenceNumberInfo, "MRN does not contain a valid country/region code");
			});
		}

		public void TestCheckCED_MovementReferenceNumber_Unique()
		{
			const string message = "MRN already exists on another Movement in this Exit Summary Job.";
			var exitDetail2 = exitHeader.CusExitDetails.AddNew();

			NUnit.Framework.Assert.Multiple(() =>
			{
				exitDetail.Validation.ValidateCED_MovementReferenceNumber();
				AssertNoWarning("CED_MovementReferenceNumber empty, not validated", exitDetail.CED_MovementReferenceNumberInfo, message);

				exitDetail.CED_MovementReferenceNumber = "123";
				AssertNoWarning("CED_MovementReferenceNumber unique", exitDetail.CED_MovementReferenceNumberInfo, message);

				exitDetail2.CED_MovementReferenceNumber = "123";
				AssertHasWarning("CED_MovementReferenceNumber duplicated", exitDetail2.CED_MovementReferenceNumberInfo, message);

				exitDetail2.CED_Status = UniversalReferenceConstants.CusExitDetailStatus._371;
				exitDetail2.Validation.ValidateCED_MovementReferenceNumber();
				AssertNoWarning("CED_Status '371', not validated", exitDetail2.CED_MovementReferenceNumberInfo, message);

				exitDetail.Validation.ValidateCED_MovementReferenceNumber();
				AssertNoWarning("CED_MovementReferenceNumber not duplicated because exitDetail2.CED_Status='371'", exitDetail.CED_MovementReferenceNumberInfo, message);

				exitDetail2.CED_Status = UniversalReferenceConstants.CusExitDetailStatus._372;
				exitDetail2.Validation.ValidateCED_MovementReferenceNumber();
				AssertNoWarning("CED_Status '372', not validated", exitDetail2.CED_MovementReferenceNumberInfo, message);

				exitDetail.Validation.ValidateCED_MovementReferenceNumber();
				AssertNoWarning("CED_MovementReferenceNumber not duplicated because exitDetail2.CED_Status='372'", exitDetail.CED_MovementReferenceNumberInfo, message);
			});
		}

		[ExpectNoExceptions]
		public void TestCheckCED_MovementReferenceNumber_Unique_HeaderNull()
		{
			const string message = "MRN already exists on another Movement in this Exit Summary Job.";
			exitDetail.CED_MovementReferenceNumber = "123";
			NUnit.Framework.Assert.That(exitDetail.CED_MovementReferenceNumberInfo.HasWarning(message), NUnit.Framework.Is.EqualTo(false), "Header is null, shouldn't have notification or exception");
		}

		public void TestCheckCED_CustomsOffice_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(exitDetail.CED_CustomsOfficeInfo);
		}

		public void TestCheckCED_ExitDate_Mandatory()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				exitDetail.CED_Status = "309";
				exitDetail.Validation.ValidateCED_ExitDate();
				AssertNoWarningContaining("CED_Status not in ('310', '342', '353')", exitDetail.CED_ExitDateInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var status in new[]
				{
					UniversalReferenceConstants.CusExitDetailStatus._310,
					UniversalReferenceConstants.CusExitDetailStatus._342,
					UniversalReferenceConstants.CusExitDetailStatus._353
				})
				{
					exitDetail.CED_Status = status;
					ValidationTestHelper.AssertWarningIfNotEntered(exitDetail.CED_ExitDateInfo, MandatoryValidation.YouHaveNotEntered, $"CED_Status '{status}'");
				}
			});
		}

		public void TestCheckCED_TransportID_Mandatory()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				exitDetail.CED_Status = "309";
				exitDetail.Validation.ValidateCED_TransportID();
				AssertNoWarningContaining("CED_Status not in ('310', '342', '353')", exitDetail.CED_TransportIDInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var status in new[]
				{
					UniversalReferenceConstants.CusExitDetailStatus._310,
					UniversalReferenceConstants.CusExitDetailStatus._342,
					UniversalReferenceConstants.CusExitDetailStatus._353
				})
				{
					exitDetail.CED_Status = status;
					ValidationTestHelper.AssertWarningIfNotEntered(exitDetail.CED_TransportIDInfo, MandatoryValidation.YouHaveNotEntered, $"CED_Status '{status}'");
				}
			});
		}

		public void TestCheckCED_OA_Carrier_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(exitDetail.CED_OA_CarrierInfo, "You have not entered a Carrier.");
		}

		public void TestCED_OA_Carrier_EoriAndEbs()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			var targetInfo = exitDetail.CED_OA_CarrierInfo;

			NUnit.Framework.Assert.Multiple(() =>
			{
				exitDetail.CED_OA_Carrier = ZGuid.Empty;
				AssertNoMessageErrorContaining("CED_OA_Carrier empty", targetInfo, "Carrier is missing");

				exitDetail.CED_OA_Carrier = orgAddress.PK;
				AssertHasMessageError("CED_OA_Carrier doesn't have EORI and EBS", targetInfo, "Carrier is missing EORI number and branch.");

				var eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI123", Core.Constants.CountryCodes.Greece);
				exitDetail.Validation.ValidateCED_OA_Carrier();
				AssertHasMessageError("CED_OA_Carrier doesn't have EBS", targetInfo, "Carrier is missing EORI branch.");

				var ebsCusCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS123", Core.Constants.CountryCodes.Germany);
				ebsCusCode.OK_OA_PremisesAddress = orgAddress.PK;
				exitDetail.Validation.ValidateCED_OA_Carrier();
				AssertNoMessageErrorContaining("CED_OA_Carrier has EORI and EBS", targetInfo, "Carrier is missing");

				eoriCusCode.Delete();
				exitDetail.Validation.ValidateCED_OA_Carrier();
				AssertHasMessageError("CED_OA_Carrier doesn't have EORI", targetInfo, "Carrier is missing EORI number.");
				AssertHasMessageErrorContaining("Because AssertNoMessageErrorContaining requires AssertHasMessageErrorContaining", targetInfo, "Carrier is missing");
			});
		}

		public void TestCED_Status_NotMandatory()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(exitDetail.CED_StatusInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitControlHeader>();
			exitDetail = exitHeader.CusExitDetails.AddNew();
		}
		CusExitControlHeader exitHeader;
		CusExitDetail exitDetail;
	}
}
