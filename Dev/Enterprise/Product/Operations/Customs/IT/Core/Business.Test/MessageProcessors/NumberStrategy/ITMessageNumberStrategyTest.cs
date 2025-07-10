using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.NumberFountain;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITMessageNumberStrategyTest : TestCaseWithFactory
{
	[TestDate(2019, 11, 21)]
	public void TestGetMessageReferenceNumber()
	{
		SetupDeclarant(currentValue: 999998);

		var fountainProviderMock = new CustomsMessageFountainProviderForTesting("1234", DefaultFountainType, GlbCompany.CurrentCompany);
		var numberStrategy = new ITMessageNumberStrategy(fountainProviderMock);
		AssertEquals("999998", numberStrategy.GetMessageReferenceNumber());

		Factory.Save();
		AssertEquals("999999", numberStrategy.GetMessageReferenceNumber());

		Factory.Save();
		AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() => numberStrategy.GetMessageReferenceNumber());
	}

	[TestDate(2022, 03, 03)]
	public void TestExceptionThrownWhenNoDeclarantSetup()
	{
		var fountainProviderMock = new CustomsMessageFountainProviderForTesting("", DefaultFountainType, GlbCompany.CurrentCompany);
		var numberStrategy = GetMessageNumberStrategy(fountainProviderMock);
		AssertInvalidDeclarantNodeExceptionThrown("", () => numberStrategy.GetMessageReferenceNumber());
	}

	[TestDate(2022, 03, 03)]
	public void TestExceptionThrownWhenNoNodeSetup()
	{
		SetupDeclarant();

		var fountainProviderMock = new CustomsMessageFountainProviderForTesting("", DefaultFountainType, GlbCompany.CurrentCompany);
		var numberStrategy = GetMessageNumberStrategy(fountainProviderMock);
		AssertInvalidDeclarantNodeExceptionThrown("", () => numberStrategy.GetMessageReferenceNumber());
	}

	[TestDate(2022, 03, 03)]
	public void TestExceptionThrownWhenNoNodeMatching()
	{
		SetupDeclarant();

		var fountainProviderMock = new CustomsMessageFountainProviderForTesting("ABCD", DefaultFountainType, GlbCompany.CurrentCompany);
		var numberStrategy = GetMessageNumberStrategy(fountainProviderMock);
		AssertInvalidDeclarantNodeExceptionThrown("ABCD", () => numberStrategy.GetMessageReferenceNumber());
	}

	[TestDate(2022, 03, 03)]
	public void TestExceptionThrownWhenNoNumberRangeSetup()
	{
		var fountainProviderMock = new CustomsMessageFountainProviderForTesting("ABCD", DefaultFountainType, GlbCompany.CurrentCompany);

		var declarantTaxNumer = "00000000001";
		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount(declarantTaxNumer + "-001", "ABCD")
			.AppendAccountDetail("ABCD-DEC1", "DEC1")
			.Build();

		AssertExceptionThrown<NumberRangeNotSetUpException>(
			"Attempting to get message reference number with no appropriate number range set up",
			$"A number range of Type '{DefaultFountainType}' and Prefix '{TestDateAttribute.Date.Year}:{declarantTaxNumer}' has not been setup for 'Company {GlbCompany.CurrentCompany.GC_Code}'.",
			() => new ITMessageNumberStrategy(fountainProviderMock).GetMessageReferenceNumber()
		);
	}

	[TestDate(2022, 03, 03)]
	public void TestExceptionThrownWhenDeclarationCustomsProfileIsNotValid()
	{
		var fountainProviderMock = new CustomsMessageFountainProviderForTesting("", DefaultFountainType, GlbCompany.CurrentCompany);
		var numberStrategy = GetMessageNumberStrategy(fountainProviderMock);
		// Customs Profile (node) not set
		AssertInvalidDeclarantNodeExceptionThrown("", () => numberStrategy.GetMessageReferenceNumber());

		// Customs Profile (node) does not match a registered IT customs account
		fountainProviderMock = new CustomsMessageFountainProviderForTesting("NNEX", DefaultFountainType, GlbCompany.CurrentCompany);
		numberStrategy = GetMessageNumberStrategy(fountainProviderMock);
		AssertInvalidDeclarantNodeExceptionThrown("NNEX", () => numberStrategy.GetMessageReferenceNumber());
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.NewWithValidTestData<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
	}

	string DefaultFountainType => NumberRangeTypeList.Codes.CustomsDeclarations;

	IMessageNumberStrategy GetMessageNumberStrategy(CustomsMessageFountainProvider customsMessageFountainProvider) => new ITMessageNumberStrategy(customsMessageFountainProvider);

	void SetupDeclarant(int currentValue = 1, string fountainType = null)
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var acc1Node = "1234";
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestGenericNumberRange(company, ZDate.Today.Year, AccDeclarantTaxNumber, fountainType ?? DefaultFountainType, currentValue);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount(AccDeclarantTaxNumber + "-001", acc1Node)
			.AppendAccountDetail(acc1Node + "-DEC1", "DEC1")
			.Build();
		Factory.Save();
	}

	void AssertInvalidDeclarantNodeExceptionThrown(string nodeValue, AnonymousMethod getNumberAction)
	{
		AssertExceptionThrown<InvalidOperationException>(
			"Attempting to get message reference number with " + (string.IsNullOrWhiteSpace(nodeValue) ? "an invalid declarant node" : "no matching registered account"),
			$"Cannot get message reference number. Declarant node [{nodeValue}] does not match any registered account.",
			getNumberAction);
	}

	const string AccDeclarantTaxNumber = "11111111111";
}
