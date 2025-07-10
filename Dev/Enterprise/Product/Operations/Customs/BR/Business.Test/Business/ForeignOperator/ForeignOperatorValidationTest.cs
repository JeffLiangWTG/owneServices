using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ForeignOperatorValidation))]
	public class ForeignOperatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCountryCode()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();

			ValidationTestHelper.AssertInvalidCodeMessageError(foreignOperator.CountryCodeInfo, "XX", Core.Constants.CountryCodes.Brazil);

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ValidationTestHelper.AssertErrorIfNotEntered(foreignOperator.CountryCodeInfo);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(foreignOperator.CountryCodeInfo);
			}
		}

		public void TestCheckDuplicate()
		{
			var cusGoodsCatalog = Factory.New<CusGoodsCatalog>();
			var foreignOperator1 = cusGoodsCatalog.ForeignOperators.AddNew();
			var foreignOperator2 = cusGoodsCatalog.ForeignOperators.AddNew();

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				foreignOperator1.CountryCode = "BR";
				foreignOperator2.CountryCode = "BR";
				AssertHasDuplicateError();

				foreignOperator2.CountryCode = "CA";
				AssertNoDuplicateError();

				foreignOperator2.AuthorityCode = "111";
				AssertNoDuplicateError();

				foreignOperator2.CountryCode = "BR";
				AssertNoDuplicateError();

				foreignOperator1.AuthorityCode = "111";
				AssertHasDuplicateError();
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNoDuplicateError();
			}

			void AssertNoDuplicateError()
			{
				foreignOperator2.Validation.ValidateAuthorityCode();
				foreignOperator2.Validation.ValidateCountryCode();
				AssertNoError(foreignOperator2.CountryCodeInfo, "This Country of Origin and Authority Code already exists in this Goods Catalog");
				AssertNoError(foreignOperator2.AuthorityCodeInfo, "This Country of Origin and Authority Code already exists in this Goods Catalog");
			}

			void AssertHasDuplicateError()
			{
				foreignOperator2.Validation.ValidateAuthorityCode();
				foreignOperator2.Validation.ValidateCountryCode();
				AssertHasError(foreignOperator2.CountryCodeInfo, "This Country of Origin and Authority Code already exists in this Goods Catalog");
				AssertHasError(foreignOperator2.AuthorityCodeInfo, "This Country of Origin and Authority Code already exists in this Goods Catalog");
			}
		}

		public void TestCheckCGI_BFR_ForeignOperator()
		{
			var duplicatedMessage = "This manufacturer already exists in this Goods Catalog";
			var ownerNotMatchMessage = "There is no correspondence between manufacturer and Catalog Owner on Foreign Operator module.";
			var authorityIdentifierMessage = "The Foreign Operator is not Active.";
			var authorityIdentifierStatusMessage = "The Foreign Operator does not have an Authority Identifier.";
			var notSentMessageStatus = "This Foreign Operator should not be used because there might be messages that need to be sent (Message Status: NOT - NOT Sent).";
			var awaitingResponseMessageStatus = "This Foreign Operator should not be used as there is a message awaiting a response (Message Status: 'AWA - Awaiting Response)'.";

			var owner1 = Factory.New<OrgHeader>();
			var owner2 = Factory.New<OrgHeader>();

			var cusGoodsCatalog = Factory.New<CusGoodsCatalog>();
			cusGoodsCatalog.CGC_OH_Owner = owner1.PK;

			var cusBRForeignOperator1 = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator1.BFR_OH_Owner = owner1.PK;

			var cusBRForeignOperator2 = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator2.BFR_OH_Owner = owner2.PK;

			var foreignOperator1 = cusGoodsCatalog.ForeignOperators.AddNew();

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(foreignOperator1.CGI_BFR_ForeignOperatorInfo);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ValidationTestHelper.AssertErrorIfNotEntered(foreignOperator1.CGI_BFR_ForeignOperatorInfo);

				foreignOperator1.CGI_BFR_ForeignOperator = cusBRForeignOperator1.PK;
				AssertNoError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, ownerNotMatchMessage);
				AssertNoError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, duplicatedMessage);

				var foreignOperator2 = cusGoodsCatalog.ForeignOperators.AddNew();
				foreignOperator2.CGI_BFR_ForeignOperator = cusBRForeignOperator1.PK;
				AssertNoError(foreignOperator2.CGI_BFR_ForeignOperatorInfo, ownerNotMatchMessage);
				AssertHasError(foreignOperator2.CGI_BFR_ForeignOperatorInfo, duplicatedMessage);

				foreignOperator2.CGI_BFR_ForeignOperator = cusBRForeignOperator2.PK;
				AssertHasError(foreignOperator2.CGI_BFR_ForeignOperatorInfo, ownerNotMatchMessage);
				AssertNoError(foreignOperator2.CGI_BFR_ForeignOperatorInfo, duplicatedMessage);

				cusBRForeignOperator1.BFR_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Active;
				foreignOperator1.Validation.ValidateCGI_BFR_ForeignOperator();
				AssertNoMessageError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, authorityIdentifierMessage);

				cusBRForeignOperator1.BFR_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Inactive;
				foreignOperator1.Validation.ValidateCGI_BFR_ForeignOperator();
				AssertHasMessageError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, authorityIdentifierMessage);

				cusBRForeignOperator1.BFR_AuthorityIdentifier = "";
				cusBRForeignOperator1.BFR_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Active;
				foreignOperator1.Validation.ValidateCGI_BFR_ForeignOperator();
				AssertHasMessageError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, authorityIdentifierStatusMessage);

				cusBRForeignOperator1.BFR_AuthorityIdentifier = "1";
				foreignOperator1.Validation.ValidateCGI_BFR_ForeignOperator();
				AssertNoMessageError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, authorityIdentifierStatusMessage);

				cusBRForeignOperator1.BFR_MessageStatus = BRMessageStatusList.Codes.NotSent;
				foreignOperator1.Validation.ValidateCGI_BFR_ForeignOperator();
				AssertHasMessageError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, notSentMessageStatus);

				cusBRForeignOperator1.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
				foreignOperator1.Validation.ValidateCGI_BFR_ForeignOperator();
				AssertNoMessageError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, notSentMessageStatus);

				cusBRForeignOperator1.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
				foreignOperator1.Validation.ValidateCGI_BFR_ForeignOperator();
				AssertHasMessageError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, awaitingResponseMessageStatus);

				cusBRForeignOperator1.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
				foreignOperator1.Validation.ValidateCGI_BFR_ForeignOperator();
				AssertNoMessageError(foreignOperator1.CGI_BFR_ForeignOperatorInfo, awaitingResponseMessageStatus);

				foreignOperator1.CGI_Reference = Core.Constants.CountryCodes.Brazil;
				foreignOperator1.CGI_BFR_ForeignOperator = ZGuid.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(foreignOperator1.CGI_BFR_ForeignOperatorInfo);
			}
		}
	}
}
