using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class CustomsInterchangeHeaderTest : TestCase
{
	public void TestReadValidContentNationalUser()
	{
		var headerContent = @"0T60            0T600115.Q92180000621979025100    006738F         046 00004    INVIO IN AMBIENTE REALE       ";
		var customsInterchangeHeader = new CustomsInterchangeHeaderForTest();
		customsInterchangeHeader.LoadExposed(headerContent);
		AssertNotNull(customsInterchangeHeader);
		AssertEquals("0T60", customsInterchangeHeader.AuthorizedUserCode);
		AssertEquals("0T600115.Q92", customsInterchangeHeader.FileName);
		AssertEquals("025100", customsInterchangeHeader.CustomsOfficeSectionCode);
		AssertEquals("", customsInterchangeHeader.CountryCode);
		AssertEquals("006738F", customsInterchangeHeader.TaxCodeOrVATRegistrationNumber);
		AssertEquals(46, customsInterchangeHeader.ProgressiveSeatAuthorizedAccount);
		AssertEquals(4, customsInterchangeHeader.NumberOfRecordsInTheFile);
		AssertEquals("INVIO IN AMBIENTE REALE", customsInterchangeHeader.TransmissionEnvironment);
		Assert(customsInterchangeHeader.IsProductionTransmissionEnvironment);

		AssertCustomsInterchangeHeaderProperties(
			authorizedUserCode: "0T60",
			fileName: "0T600115.Q92",
			customsOfficeSectionCode: "025100",
			countryCode: "",
			taxCodeOrVATRegistrationNumber: "006738F",
			progressiveSeatAuthorizedAccount: 46,
			numberOfRecordsInTheFile: 4,
			transmissionEnvironment: "INVIO IN AMBIENTE REALE",
			isProductionTransmissionEnvironment: true,
			innerText: headerContent,
			customsInterchangeHeader);
	}

	public void TestReadValidContentNonNationalUser()
	{
		var headerContent = @"1KQY            1KQY0102.BOR190000027153221100    DTMFRC78S04H199M001 00003    INVIO IN AMBIENTE REALE       ";
		var customsInterchangeHeader = new CustomsInterchangeHeaderForTest();
		customsInterchangeHeader.LoadExposed(headerContent);

		AssertCustomsInterchangeHeaderProperties(
			authorizedUserCode: "1KQY",
			fileName: "1KQY0102.BOR",
			customsOfficeSectionCode: "221100",
			countryCode: "",
			taxCodeOrVATRegistrationNumber: "DTMFRC78S04H199M",
			progressiveSeatAuthorizedAccount: 1,
			numberOfRecordsInTheFile: 3,
			transmissionEnvironment: "INVIO IN AMBIENTE REALE",
			isProductionTransmissionEnvironment: true,
			innerText: headerContent,
			customsInterchangeHeader);
	}

	public void TestReadMalformedContent()
	{
		AssertExceptionThrown<ArgumentException>(() =>
		{
			var customsInterchangeHeader = new CustomsInterchangeHeaderForTest();
			customsInterchangeHeader.LoadExposed(@"1KQY0102.BOR190000027153221100");
		});
		AssertExceptionThrown<ArgumentException>(() =>
		{
			var customsInterchangeHeader = new CustomsInterchangeHeaderForTest();
			customsInterchangeHeader.LoadExposed(@"");
		});
	}

	public void TestNewForEhubSending()
	{
		var customsInterchangeHeader = CustomsInterchangeHeader.NewForEhubSending(
			authorizedUserCode: "1111",
			filename: "845A1009.R00",
			customsOfficeSectionCode: "999999",
			taxCodeOrVATRegistrationNumber: "22222222222",
			progressiveSeatAuthorizedAccount: 1,
			numberOfRecordsInTheFile: 6);

		var expectedInnerText = "1111            845A1009.R00            999999    22222222222     001 00006";

		AssertCustomsInterchangeHeaderProperties(
			authorizedUserCode: "1111",
			fileName: "845A1009.R00",
			customsOfficeSectionCode: "999999",
			countryCode: "",
			taxCodeOrVATRegistrationNumber: "22222222222",
			progressiveSeatAuthorizedAccount: 1,
			numberOfRecordsInTheFile: 6,
			transmissionEnvironment: "",
			isProductionTransmissionEnvironment: false,
			innerText: expectedInnerText,
			customsInterchangeHeader);
	}

	public void TestNewFromText()
	{
		AssertExceptionThrown<ArgumentException>("Exception expected when value text is empy", () => CustomsInterchangeHeader.NewFromText(""));

		var headerContent = "1KQY            1KQY0102.BOR190000027153221100    DTMFRC78S04H199M001 00003    INVIO IN AMBIENTE REALE       ";
		var customsInterchangeHeader = CustomsInterchangeHeader.NewFromText(headerContent);

		AssertNotNull("Customs Interchange Header", customsInterchangeHeader);
		AssertCustomsInterchangeHeaderProperties(
			authorizedUserCode: "1KQY",
			fileName: "1KQY0102.BOR",
			customsOfficeSectionCode: "221100",
			countryCode: "",
			taxCodeOrVATRegistrationNumber: "DTMFRC78S04H199M",
			progressiveSeatAuthorizedAccount: 1,
			numberOfRecordsInTheFile: 3,
			transmissionEnvironment: "INVIO IN AMBIENTE REALE",
			isProductionTransmissionEnvironment: true,
			innerText: headerContent,
			customsInterchangeHeader);
	}

	public void TestNewFromTextIgnoringTrasmissionEnviroment()
	{
		var headerContent = "1KQY            1KQY0102.BOR190000027153221100    DTMFRC78S04H199M001 00003";
		var customsInterchangeHeader = CustomsInterchangeHeader.NewFromText(headerContent, ignoreTrasmissionEnviroment: true);

		AssertNotNull("Customs Interchange Header", customsInterchangeHeader);
		AssertCustomsInterchangeHeaderProperties(
			authorizedUserCode: "1KQY",
			fileName: "1KQY0102.BOR",
			customsOfficeSectionCode: "221100",
			countryCode: "",
			taxCodeOrVATRegistrationNumber: "DTMFRC78S04H199M",
			progressiveSeatAuthorizedAccount: 1,
			numberOfRecordsInTheFile: 3,
			transmissionEnvironment: "",
			isProductionTransmissionEnvironment: false,
			innerText: headerContent,
			customsInterchangeHeader);
	}

	public void TestMatchCustomsMessageFileNames()
	{
		CombineAssertions("Testing MatchCustomsMessageFileNames", () =>
		{
			AssertEquals("When input params are null, They don't match", false, CustomsInterchangeHeader.MatchCustomsMessageFileNames("", ""));
			AssertEquals("When input params do not have the correct lentgth", false, CustomsInterchangeHeader.MatchCustomsMessageFileNames("abc", "abc"));
			AssertEquals("When input params are different extensions", false, CustomsInterchangeHeader.MatchCustomsMessageFileNames("0RPE1003.U81", "0RPE1003.U82"));
			AssertEquals("When input params are different date", false, CustomsInterchangeHeader.MatchCustomsMessageFileNames("0RPE1004.U81", "0RPE1003.U81"));
			AssertEquals("When input params matches", true, CustomsInterchangeHeader.MatchCustomsMessageFileNames("0RPE1003.R81", "0RPE1003.U81"));
		});
	}

	void AssertCustomsInterchangeHeaderProperties(string authorizedUserCode,
		string fileName,
		string customsOfficeSectionCode,
		string countryCode,
		string taxCodeOrVATRegistrationNumber,
		int progressiveSeatAuthorizedAccount,
		int numberOfRecordsInTheFile,
		string transmissionEnvironment,
		bool isProductionTransmissionEnvironment,
		string innerText,
		CustomsInterchangeHeader actualCustomsInterchangeHeader
	)
	{
		CombineAssertions("Interchange Header properties", () =>
		{
			AssertEquals("Authorized User Code", authorizedUserCode, actualCustomsInterchangeHeader.AuthorizedUserCode);
			AssertEquals("File Name", fileName, actualCustomsInterchangeHeader.FileName);
			AssertEquals("Customs Office SectionCode", customsOfficeSectionCode, actualCustomsInterchangeHeader.CustomsOfficeSectionCode);
			AssertEquals("Country Code", countryCode, actualCustomsInterchangeHeader.CountryCode);
			AssertEquals("Tax Code Or VAT Registration Number", taxCodeOrVATRegistrationNumber, actualCustomsInterchangeHeader.TaxCodeOrVATRegistrationNumber);
			AssertEquals("Progressive Seat Authorized Account", progressiveSeatAuthorizedAccount, actualCustomsInterchangeHeader.ProgressiveSeatAuthorizedAccount);
			AssertEquals("Number Of Records In The File", numberOfRecordsInTheFile, actualCustomsInterchangeHeader.NumberOfRecordsInTheFile);
			AssertEquals("Transmission Environment", transmissionEnvironment, actualCustomsInterchangeHeader.TransmissionEnvironment);
			AssertEquals("Is Production Transmission Environment", isProductionTransmissionEnvironment, actualCustomsInterchangeHeader.IsProductionTransmissionEnvironment);
			AssertEquals("Inner Text", innerText, actualCustomsInterchangeHeader.InnerText);
		});
	}
}

class CustomsInterchangeHeaderForTest : CustomsInterchangeHeader
{
	public void LoadExposed(ZString content) => base.Load(content);
}
