using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSDISQueryMessageValidationTests : TestCaseWithFactory
	{
		public void TestEM_ApplicationReference()
		{
			AssertNoError(queryMessage.EM_ApplicationReferenceInfo, "Please enter a Reference.");
			queryMessage.EM_ApplicationReference = string.Empty;
			AssertHasError(queryMessage.EM_ApplicationReferenceInfo, "Please enter a Reference.");
		}

		public void TestEM_MessageOwner()
		{
			queryMessage.EM_MessageOwner = string.Empty;
			AssertHasError(queryMessage.EM_MessageOwnerInfo, "Please enter a CDS Messaging Profile.");
			queryMessage.EM_MessageOwner = "Notvalid";
			AssertHasError(queryMessage.EM_MessageOwnerInfo, "Enter a valid CDS Messaging Profile.");
			TestDataHelper.CreateCredentials(Factory.CreateNewFactory(), GlbCompany.CurrentCompany.PK, "ABC", "12345678901234", PasswordTypesList.Codes.CDS);
			queryMessage.EM_MessageOwner = queryMessage.Lookups.ProfileList[0].Code;
			AssertNoError(queryMessage.EM_MessageOwnerInfo, "Enter a valid CDS Messaging Profile.");

			var pwd = queryMessage.GlbExternalPassword;
			pwd.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			queryMessage.Validation.ValidateEM_MessageOwner();
			AssertHasMessageErrorContaining(queryMessage.EM_MessageOwnerInfo, "Please select a valid profile (token).");
			AssertNoMessageErrorContaining(queryMessage.EM_MessageOwnerInfo, "and the status message is 'Access Token - Could Not obtain access token'");

			pwd.GP_StatusReason = "Access Token - Could Not obtain access token";
			queryMessage.Validation.ValidateEM_MessageOwner();
			AssertHasMessageErrorContaining(queryMessage.EM_MessageOwnerInfo, "Please select a valid profile (token).");
			AssertHasMessageErrorContaining(queryMessage.EM_MessageOwnerInfo, "and the status message is 'Access Token - Could Not obtain access token'");

			Factory.Save();
			queryMessage.Validation.ValidateEM_MessageOwner();
			AssertNoErrors("Should not show error once the message is saved", queryMessage.EM_MessageOwnerInfo);
		}

		public void TestEM_MessageText()
		{
			queryMessage.EM_MessageText = "<Value>{\"eoris\":[\"西ヨーロッパ以外の文字\"]}</Value>";
			queryMessage.Validation.ValidateEM_MessageText();
			AssertNoErrors("Should allow non-Western European characters in EM_MessageText", queryMessage.EM_MessageTextInfo);
		}

		public void TestDeclarationCategory()
		{
			AssertNoError(queryMessage.DeclarationCategoryInfo, "Enter a valid Declaration Category.");
			queryMessage.DeclarationCategory = string.Empty;
			AssertHasError(queryMessage.DeclarationCategoryInfo, "Please enter a Declaration Category.");
			queryMessage.DeclarationCategory = "AR1";
			AssertHasError(queryMessage.DeclarationCategoryInfo, "Enter a valid Declaration Category.");
		}

		public void TestDateFrom()
		{
			queryMessage.DateFrom = ZDate.Empty;
			AssertHasError(queryMessage.DateFromInfo, "Please enter a Date From.");
			queryMessage.DateFrom = ZDate.Invalid;
			AssertHasError(queryMessage.DateFromInfo, "Enter a valid Date From.");
			queryMessage.DateTo = ZDate.Today.AddDays(-3);
			queryMessage.DateFrom = ZDate.Today;
			AssertHasError(queryMessage.DateFromInfo, "Date From cannot be greater then Date To.");
		}

		public void TestDateTo()
		{
			queryMessage.DateTo = ZDate.Empty;
			AssertHasError(queryMessage.DateToInfo, "Please enter a Date To.");
			queryMessage.DateTo = ZDate.Invalid;
			AssertHasError(queryMessage.DateToInfo, "Enter a valid Date To.");
			queryMessage.DateFrom = ZDate.Today;
			queryMessage.DateTo = ZDate.Today.AddDays(-1);
			AssertHasError(queryMessage.DateToInfo, "Date To cannot be lower then Date From.");
		}

		public void TestDeclarationStatus()
		{
			AssertNoError(queryMessage.DeclarationStatusInfo, "Enter a valid Declaration Status.");
			queryMessage.DeclarationStatus = string.Empty;
			AssertHasError(queryMessage.DeclarationStatusInfo, "Please enter a Declaration Status.");
			queryMessage.DeclarationStatus = "AR1";
			AssertHasError(queryMessage.DeclarationStatusInfo, "Enter a valid Declaration Status.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			queryMessage = Factory.New<CDSDISQueryMessage>();
		}

		CDSDISQueryMessage queryMessage;
	}
}
