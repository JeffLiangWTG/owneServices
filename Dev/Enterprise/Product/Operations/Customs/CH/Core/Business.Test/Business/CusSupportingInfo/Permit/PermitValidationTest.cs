using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PermitValidation))]
sealed class PermitValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_IssuerType() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreatePermitAuthorityCodeList(Factory);

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Permit.CSI_IssuerTypeInfo, RefCusCodeTestHelper.InvalidPermitAuthorityCode, RefCusCodeTestHelper.ValidPermitAuthorityCode);
	});

	public void TestCheckCSI_ReferenceNumber() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Permit.CSI_ReferenceNumberInfo);

	public void TestCheckCSI_Code_Import() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreatePermitTypeCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Permit.CSI_CodeInfo, RefCusCodeTestHelper.InvalidPermitTypeCode, RefCusCodeTestHelper.ValidPermitTypeCode);
	});

	public void TestCheckCSI_Code_R313() => CombineAssertions(() =>
	{
		var messageError = ValidationMessages.Plausi.MessageR313;
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var invalidCodes = new[] { UniversalReferenceConstants.PermitCodes.SingleEPermit, UniversalReferenceConstants.PermitCodes.GeneralEPermit };
		var validCode = "10";
		var invalidIssuerTypes = new[]
		{
				PermitAuthorityCodes.FOAG,
				PermitAuthorityCodes.AAT,
				PermitAuthorityCodes.FOPH,
				PermitAuthorityCodes.FOEN,
				PermitAuthorityCodes.COE,
				PermitAuthorityCodes.CA,
				PermitAuthorityCodes.FSF,
				PermitAuthorityCodes.TOS,
				PermitAuthorityCodes.FOE,
				PermitAuthorityCodes.COW,
				PermitAuthorityCodes.SM,
				PermitAuthorityCodes.STB,
				PermitAuthorityCodes.RS,
				PermitAuthorityCodes.FOC,
				PermitAuthorityCodes.IVI,
				PermitAuthorityCodes.FTA,
				PermitAuthorityCodes.FOCBS_MOT,
				PermitAuthorityCodes.FOCBS_COV,
				PermitAuthorityCodes.FOCBS_Other,
				PermitAuthorityCodes.Other
		};
		var validIssuerTypes = new ZString[] { PermitAuthorityCodes.BWIP, ZString.Empty };

		foreach (var invalidCode in invalidCodes)
		{
			foreach (var issuerType in invalidIssuerTypes)
			{
				Permit.CSI_Code = invalidCode;
				Permit.CSI_IssuerType = issuerType;

				AssertHasMessageError(GetAssertionMessage(), Permit.CSI_CodeInfo, messageError);
			}

			foreach (var validIssuerType in validIssuerTypes)
			{
				Permit.CSI_Code = invalidCode;
				Permit.CSI_IssuerType = validIssuerType;

				AssertNoMessageError(GetAssertionMessage(), Permit.CSI_CodeInfo, messageError);
			}
		}

		foreach (var invalidIssuerType in invalidIssuerTypes)
		{
			Permit.CSI_Code = validCode;
			Permit.CSI_IssuerType = invalidIssuerType;

			AssertNoMessageError(GetAssertionMessage(), Permit.CSI_CodeInfo, messageError);
		}

		string GetAssertionMessage() => $"CSI_IssuerType = {Permit.CSI_IssuerType}";
	});

	public void TestCheckCSI_Code_R316() => CombineAssertions(() =>
	{
		var messageError = ValidationMessages.Plausi.MessageR316;
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var invalidCode = "10";
		var validCodes = new[] { PermitCodes.SingleEPermit, PermitCodes.GeneralEPermit };
		var invalidIssuerTypes = new[]
		{
				PermitAuthorityCodes.BWIP, PermitAuthorityCodes.BWRP, PermitAuthorityCodes.FSVO_Other
		};
		var validIssuerTypes = new ZString[] { PermitAuthorityCodes.AAT, ZString.Empty };

		foreach (var invalidIssuerType in invalidIssuerTypes)
		{
			Permit.CSI_Code = invalidCode;
			Permit.CSI_IssuerType = invalidIssuerType;

			AssertHasMessageError(GetAssertionMessage(), Permit.CSI_CodeInfo, messageError);
		}

		foreach (var validIssuerType in validIssuerTypes)
		{
			Permit.CSI_Code = invalidCode;
			Permit.CSI_IssuerType = validIssuerType;
			AssertNoMessageError(GetAssertionMessage(), Permit.CSI_CodeInfo, messageError);
		}

		foreach (var validCode in validCodes)
		{
			foreach (var invalidIssuerType in invalidIssuerTypes)
			{
				Permit.CSI_Code = validCode;
				Permit.CSI_IssuerType = invalidIssuerType;

				AssertNoMessageError(GetAssertionMessage(), Permit.CSI_CodeInfo, messageError);
			}
		}

		string GetAssertionMessage() => $"CSI_IssuerType = {Permit.CSI_IssuerType}";
	});

	public void TestCheckCSI_IssuerType_R314() => CombineAssertions(() =>
	{
		var messageError = ValidationMessages.Plausi.MessageR314;
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var invalidCode = UniversalReferenceConstants.PermitCodes.SingleEPermit;
		var validCode = "10";
		var validIssuerTypes = new ZString[]
		{
				PermitAuthorityCodes.BWIP, PermitAuthorityCodes.BWRP, PermitAuthorityCodes.FSVO_CITES, PermitAuthorityCodes.FSVO_Other, ZString.Empty
		};
		var invalidIssuerType = PermitAuthorityCodes.AAT;

		Permit.CSI_Code = invalidCode;
		Permit.CSI_IssuerType = invalidIssuerType;

		AssertHasMessageError(GetAssertionMessage(), Permit.CSI_IssuerTypeInfo, messageError);

		Permit.CSI_Code = validCode;
		Permit.CSI_IssuerType = invalidIssuerType;

		AssertNoMessageError(GetAssertionMessage(), Permit.CSI_IssuerTypeInfo, messageError);

		foreach (var validIssuerType in validIssuerTypes)
		{
			Permit.CSI_Code = invalidCode;
			Permit.CSI_IssuerType = validIssuerType;

			AssertNoMessageError(GetAssertionMessage(), Permit.CSI_IssuerTypeInfo, messageError);
		}

		string GetAssertionMessage() => $"CSI_Code = {Permit.CSI_Code}";
	});

	public void TestCheckCSI_IssuerType_R315() => CombineAssertions(() =>
	{
		var messageError = ValidationMessages.Plausi.MessageR315;
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var invalidCode = UniversalReferenceConstants.PermitCodes.GeneralEPermit;
		var validCode = "10";
		var validIssuerTypes = new ZString[]
		{
				PermitAuthorityCodes.BWIP, PermitAuthorityCodes.BWRP, PermitAuthorityCodes.FSVO_Other, ZString.Empty
		};
		var invalidIssuerType = PermitAuthorityCodes.AAT;

		Permit.CSI_Code = invalidCode;
		Permit.CSI_IssuerType = invalidIssuerType;

		AssertHasMessageError(GetAssertionMessage(), Permit.CSI_IssuerTypeInfo, messageError);

		Permit.CSI_Code = validCode;
		Permit.CSI_IssuerType = invalidIssuerType;

		AssertNoMessageError(GetAssertionMessage(), Permit.CSI_IssuerTypeInfo, messageError);

		foreach (var validIssuerType in validIssuerTypes)
		{
			Permit.CSI_Code = invalidCode;
			Permit.CSI_IssuerType = validIssuerType;

			AssertNoMessageError(GetAssertionMessage(), Permit.CSI_IssuerTypeInfo, messageError);
		}

		string GetAssertionMessage() => $"CSI_Code = {Permit.CSI_Code}";
	});

	[TestDate(2024, 05, 10)]
	public void TestCheckCSI_DateOfIssue_OlderThan10YearsNotifications() => CombineAssertions(() =>
	{
		var messageWarning = @"The date '09-May-2014' is more than 10 years old.";
		var messageError = "is more than 10 years old and thus is not valid";
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		Permit.CSI_DateOfIssue = new ZDateTime(2014, 05, 09);
		AssertHasWarning("Having a date older than 10 years returns a warning", Permit.CSI_DateOfIssueInfo, messageWarning);

		Permit.CSI_DateOfIssue = new ZDateTime(2014, 05, 10);
		AssertNoWarning("Having a date below of 10 years does not return a warning", Permit.CSI_DateOfIssueInfo, messageWarning);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

		Permit.CSI_DateOfIssue = new ZDateTime(2014, 05, 09);
		AssertHasErrorContaining("Having a date older than 10 years returns an error", Permit.CSI_DateOfIssueInfo, messageError);

		Permit.CSI_DateOfIssue = new ZDateTime(2014, 05, 10);
		Permit.Validation.ValidateCSI_DateOfIssue();
		AssertNoErrorContaining("Having a date below of 10 years does not return an error", Permit.CSI_DateOfIssueInfo, messageError);
	});

	[TestDate(2024, 05, 10)]
	public void TestCheckCSI_DateOfIssue_OverThan5YearsError() => CombineAssertions(() =>
	{
		var message = "is more than 5 years from now and thus is not valid";

		Permit.CSI_DateOfIssue = new ZDateTime(2029, 05, 10);
		AssertNoErrorContaining("Having a date below over 5 years does not return an error", Permit.CSI_DateOfIssueInfo, message);

		Permit.CSI_DateOfIssue = new ZDateTime(2029, 05, 11);
		AssertHasErrorContaining("Having a date over 5 years return an error", Permit.CSI_DateOfIssueInfo, message);
	});

	public void TestCheckCSI_CodeAndCSI_IssuerType_R249c() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		foreach (string tariff in TariffNumbers.TobaccoQuantityBasedTaxationTariffNumbers)
		{
			Permit.Parent.JI_Tariff = tariff + "000911";
			Permit.CSI_Code = ZString.Empty;
			Permit.CSI_IssuerType = PermitAuthorityCodes.STB;
			AssertNoMessageError($"Tariff = {Permit.Parent.JI_Tariff} and IssuerType = 21 and no type", Permit.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR249c);
			AssertNoMessageError($"Tariff = {Permit.Parent.JI_Tariff} and IssuerType = 21 and no type", Permit.CSI_CodeInfo, ValidationMessages.Plausi.MessageR249c);
			Permit.CSI_Code = PermitCodes.ReversTobacco;
			AssertHasMessageError($"Tariff = {Permit.Parent.JI_Tariff} and IssuerType = 21 and Type = 4", Permit.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR249c);
			AssertHasMessageError($"Tariff = {Permit.Parent.JI_Tariff} and IssuerType = 21 and Type = 4", Permit.CSI_CodeInfo, ValidationMessages.Plausi.MessageR249c);
			Permit.CSI_IssuerType = PermitAuthorityCodes.RS;
			AssertNoMessageError($"Tariff = {Permit.Parent.JI_Tariff} and IssuerType = 22 and type = 4", Permit.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR249c);
			AssertNoMessageError($"Tariff = {Permit.Parent.JI_Tariff} and IssuerType = 22 and type = 4", Permit.CSI_CodeInfo, ValidationMessages.Plausi.MessageR249c);
		}
		Permit.CSI_IssuerType = PermitAuthorityCodes.STB;
		Permit.Parent.JI_Tariff = $"{TariffNumbers.CigarCherootsCigarillosContainingTobacco}000999";
		Permit.Validation.ValidateCSI_IssuerType();
		Permit.Validation.ValidateCSI_Code();
		AssertNoMessageError("Statistical code not equal to 911", Permit.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR249c);

		Permit.Parent.JI_Tariff = $"{TariffNumbers.ProductsContainingTobaccoOther}000911";
		Permit.Validation.ValidateCSI_IssuerType();
		Permit.Validation.ValidateCSI_Code();
		AssertNoMessageError("Tariff not in set for rule R249", Permit.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR249c);
	});

	public void TestCheckCSI_CodeAndCSI_IssuerTypeAndMandatoryDetails_CH0005() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		TestSingleCase(SharedJobMessageTypeList.Codes.Import, PermitCodes.SingleEPermit, PermitAuthorityCodes.SM, false);
		TestSingleCase(SharedJobMessageTypeList.Codes.Import, PermitCodes.SingleEPermit, PermitAuthorityCodes.BWIP, true);
		TestSingleCase(SharedJobMessageTypeList.Codes.Import, PermitCodes.SingleEPermit, PermitAuthorityCodes.BWRP, true);
		TestSingleCase(SharedJobMessageTypeList.Codes.Import, PermitCodes.SingleEPermit, PermitAuthorityCodes.FSVO_CITES, true);
		TestSingleCase(SharedJobMessageTypeList.Codes.Import, PermitCodes.SingleEPermit, PermitAuthorityCodes.FSVO_Other, true);
		TestSingleCase(SharedJobMessageTypeList.Codes.Import, PermitCodes.ReversTobacco, PermitAuthorityCodes.BWIP, false);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

		TestSingleCase(SharedJobMessageTypeList.Codes.Export, PermitCodes.SingleEPermit, PermitAuthorityCodes.SM, false);
		TestSingleCase(SharedJobMessageTypeList.Codes.Export, PermitCodes.SingleEPermit, PermitAuthorityCodes.BWIP, false);
		TestSingleCase(SharedJobMessageTypeList.Codes.Export, PermitCodes.SingleEPermit, PermitAuthorityCodes.BWRP, false);
		TestSingleCase(SharedJobMessageTypeList.Codes.Export, PermitCodes.SingleEPermit, PermitAuthorityCodes.FSVO_CITES, false);
		TestSingleCase(SharedJobMessageTypeList.Codes.Export, PermitCodes.SingleEPermit, PermitAuthorityCodes.FSVO_Other, false);
		TestSingleCase(SharedJobMessageTypeList.Codes.Export, PermitCodes.ReversTobacco, PermitAuthorityCodes.BWIP, false);

		void TestSingleCase(ZString declarationType, ZString csiCode, ZString issuerType, bool errorExpected)
		{
			Permit.CSI_Code = csiCode;
			Permit.CSI_IssuerType = issuerType;
			Permit.PermitItemDetails.RemoveAll();
			Permit.Validation.ValidateCSI_Code();
			TestAssertion(csiCode, issuerType, [], errorExpected);
			Permit.PermitItemDetails.AddNew().CY_Code = PermitItemDetailKeyList.Codes.Key1;
			Permit.Validation.ValidateCSI_Code();
			TestAssertion(csiCode, issuerType, [PermitItemDetailKeyList.Codes.Key1], errorExpected);
			Permit.PermitItemDetails.AddNew().CY_Code = PermitItemDetailKeyList.Codes.Key2;
			Permit.Validation.ValidateCSI_Code();
			TestAssertion(csiCode, issuerType, [PermitItemDetailKeyList.Codes.Key1, PermitItemDetailKeyList.Codes.Key2], false);

			void TestAssertion(string csiCode, string issuerType, string[] detailsKeys, bool errorExpected)
			{
				string message = $"{declarationType}, Code {csiCode}, IssuerType {issuerType}, details keys {string.Join(", ", detailsKeys)}";
				if (errorExpected)
				{
					AssertHasMessageError(message, Permit.CSI_CodeInfo, ValidationMessages.Plausi.MessageCH0005);
				}
				else
				{
					AssertNoMessageError(message, Permit.CSI_CodeInfo, ValidationMessages.Plausi.MessageCH0005);
				}
			}
		}
	});

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	Permit Permit => permit ??= Declaration.Invoices.AddNew().InvoiceLines.AddNew().Permits.AddNew();
	Permit permit;
}
