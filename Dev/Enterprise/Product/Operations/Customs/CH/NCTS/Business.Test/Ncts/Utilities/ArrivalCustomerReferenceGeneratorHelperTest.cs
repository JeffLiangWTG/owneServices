using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class ArrivalCustomerReferenceGeneratorHelperTest : TestCaseWithFactory
{
	[TestDate(2023, 12, 31, 12, 00, 0)]
	public void TestGenerateArrivalCustomerReferenceSequenceNumberLength() => CombineAssertions(() =>
	{
		TestConnection.BeginTransaction();
		try
		{
			Env.NumberFountains.GetNctsLocalReferenceNumberFountain($"{GlbCompany.CurrentCompany.PK}0020000").SetNext(TestConnection, 123);

			using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
			{
				var arrivalReferenceNumber = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "002");
				AssertEquals(GetMessage(customFormat002), "EF202300000123GH", arrivalReferenceNumber);

				arrivalReferenceNumber = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "002");
				AssertEquals(GetMessage(customFormat002), "EF202300000124GH", arrivalReferenceNumber);
			}
		}
		finally
		{
			TestConnection.RollbackTransaction();
		}
	});

	[TestDate(2023, 12, 31, 12, 00, 0)]
	public void TestGenerateArrivalCustomerReferenceYearOption() => CombineAssertions(() =>
	{
		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		{
			var arrivalReferenceNumber = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "003");
			AssertEquals(GetMessage(customFormat003), "AB1CD", arrivalReferenceNumber);

			arrivalReferenceNumber = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "004");
			AssertEquals(GetMessage(customFormat004), "AB231CD", arrivalReferenceNumber);

			arrivalReferenceNumber = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "005");
			AssertEquals(GetMessage(customFormat005), "AB20231CD", arrivalReferenceNumber);

			arrivalReferenceNumber = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "006");
			AssertEquals(GetMessage(customFormat006), "AB1CD23", arrivalReferenceNumber);

			arrivalReferenceNumber = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "007");
			AssertEquals(GetMessage(customFormat007), "AB1CD2023", arrivalReferenceNumber);
		}
	});

	[TestDate(2023, 12, 31, 12, 00, 0)]
	[TestUtcOffset(2, 0, 0)]
	public void TestGenerateArrivalCustomerReferenceNewYear() => CombineAssertions(() =>
	{
		customFormat001.IsRestartOnNewYear = true;
		customFormat003.IsRestartOnNewYear = false;
		Factory.Save();

		using (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceFormat))
		{
			var arrivalReferenceNumber1 = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "001");
			AssertEquals(GetMessage(customFormat001), "AB20231CD", arrivalReferenceNumber1);

			arrivalReferenceNumber1 = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "001");
			AssertEquals(GetMessage(customFormat001), "AB20232CD", arrivalReferenceNumber1);

			arrivalReferenceNumber1 = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "001");
			AssertEquals(GetMessage(customFormat001), "AB20233CD", arrivalReferenceNumber1);

			var arrivalReferenceNumber2 = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "004");
			AssertEquals(GetMessage(customFormat004), "AB231CD", arrivalReferenceNumber2);

			arrivalReferenceNumber2 = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "004");
			AssertEquals(GetMessage(customFormat004), "AB232CD", arrivalReferenceNumber2);

			TestDateAttribute.AddDays(1);
			arrivalReferenceNumber1 = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "001");
			AssertEquals(GetMessage(customFormat001), "AB20241CD", arrivalReferenceNumber1);

			arrivalReferenceNumber2 = ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, "004");
			AssertEquals(GetMessage(customFormat004), "AB243CD", arrivalReferenceNumber2);
		}
	});

	string GetMessage(CustomArrivalCustomerReferenceFormat format)
	{
		return $"Current Year: {ZDateTime.Now.Year}, Auth. Code: {format.AuthorizationLocationCode}, Prefix: {format.Prefix}, Suffix: {format.Suffix}, YearOption: {format.YearOption}, SequenceNumberLength:{format.SequenceNumberLength}, IsRemoveLeadingZeros: {format.IsRemoveLeadingZeros}";
	}

	protected override void SetUp()
	{
		base.SetUp();
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_Code = "1234";
		orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "2233445566", "CH");
		GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy = orgHeader.PK;

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, orgHeader.PK, "001");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, orgHeader.PK, "002");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, orgHeader.PK, "003");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, orgHeader.PK, "004");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, orgHeader.PK, "005");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, orgHeader.PK, "006");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, orgHeader.PK, "007");

		referenceFormat = new ArrivalCustomerReferenceFormat();
		referenceFormat.UseSystemDefinedFormat = false;

		customFormat001 = new CustomArrivalCustomerReferenceFormat();
		customFormat001.AuthorizationLocationCode = "001";
		customFormat001.Prefix = "AB";
		customFormat001.Suffix = "CD";
		customFormat001.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4;
		customFormat001.SequenceNumberLength = 8;
		customFormat001.IsRemoveLeadingZeros = true;

		customFormat002 = new CustomArrivalCustomerReferenceFormat();
		customFormat002.AuthorizationLocationCode = "002";
		customFormat002.Prefix = "EF";
		customFormat002.Suffix = "GH";
		customFormat002.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4;
		customFormat002.SequenceNumberLength = 8;
		customFormat002.IsRemoveLeadingZeros = false;

		customFormat003 = new CustomArrivalCustomerReferenceFormat();
		customFormat003.AuthorizationLocationCode = "003";
		customFormat003.Prefix = "AB";
		customFormat003.Suffix = "CD";
		customFormat003.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.None;
		customFormat003.SequenceNumberLength = 8;
		customFormat003.IsRemoveLeadingZeros = true;

		customFormat004 = new CustomArrivalCustomerReferenceFormat();
		customFormat004.AuthorizationLocationCode = "004";
		customFormat004.Prefix = "AB";
		customFormat004.Suffix = "CD";
		customFormat004.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix2;
		customFormat004.SequenceNumberLength = 8;
		customFormat004.IsRemoveLeadingZeros = true;

		customFormat005 = new CustomArrivalCustomerReferenceFormat();
		customFormat005.AuthorizationLocationCode = "005";
		customFormat005.Prefix = "AB";
		customFormat005.Suffix = "CD";
		customFormat005.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4;
		customFormat005.SequenceNumberLength = 8;
		customFormat005.IsRemoveLeadingZeros = true;

		customFormat006 = new CustomArrivalCustomerReferenceFormat();
		customFormat006.AuthorizationLocationCode = "006";
		customFormat006.Prefix = "AB";
		customFormat006.Suffix = "CD";
		customFormat006.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix2;
		customFormat006.SequenceNumberLength = 8;
		customFormat006.IsRemoveLeadingZeros = true;

		customFormat007 = new CustomArrivalCustomerReferenceFormat();
		customFormat007.AuthorizationLocationCode = "007";
		customFormat007.Prefix = "AB";
		customFormat007.Suffix = "CD";
		customFormat007.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix4;
		customFormat007.SequenceNumberLength = 8;
		customFormat007.IsRemoveLeadingZeros = true;

		referenceFormat.CustomFormats.Add(customFormat001);
		referenceFormat.CustomFormats.Add(customFormat002);
		referenceFormat.CustomFormats.Add(customFormat003);
		referenceFormat.CustomFormats.Add(customFormat004);
		referenceFormat.CustomFormats.Add(customFormat005);
		referenceFormat.CustomFormats.Add(customFormat006);
		referenceFormat.CustomFormats.Add(customFormat007);
	}

	ArrivalCustomerReferenceFormat referenceFormat;
	CustomArrivalCustomerReferenceFormat customFormat001;
	CustomArrivalCustomerReferenceFormat customFormat002;
	CustomArrivalCustomerReferenceFormat customFormat003;
	CustomArrivalCustomerReferenceFormat customFormat004;
	CustomArrivalCustomerReferenceFormat customFormat005;
	CustomArrivalCustomerReferenceFormat customFormat006;
	CustomArrivalCustomerReferenceFormat customFormat007;
}
