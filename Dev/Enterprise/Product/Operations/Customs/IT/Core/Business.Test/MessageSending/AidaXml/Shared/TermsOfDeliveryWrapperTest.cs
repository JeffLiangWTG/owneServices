using System;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class TermsOfDeliveryWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => TermsOfDeliveryWrapper.NewOrNull(null));
		AssertNull(nameof(TermsOfDeliveryWrapper.NewOrNull), TermsOfDeliveryWrapper.NewOrNull(invoiceHeader));
	}

	public void TestCountryCode()
	{
		invoiceHeader.JZ_IncoTerm = "CIM";
		var termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertNullOrEmpty(nameof(ITermsOfDelivery.CountryCode), termOfDeliveryWrapper.CountryCode);

		invoiceHeader.ZG_AgreedPlaceCode = "AU";
		termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertEquals(nameof(ITermsOfDelivery.CountryCode), "AU", termOfDeliveryWrapper.CountryCode);

		invoiceHeader.ZG_AgreedPlaceCode = "AUSYD";
		termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertNullOrEmpty(nameof(ITermsOfDelivery.CountryCode), termOfDeliveryWrapper.CountryCode);
	}

	public void TestIncotermCode()
	{
		invoiceHeader.ZG_AgreedPlaceCode = "AU";
		var termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertNullOrEmpty(nameof(ITermsOfDelivery.IncotermCode), termOfDeliveryWrapper.IncotermCode);

		invoiceHeader.JZ_IncoTerm = "FOB";
		termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertEquals(nameof(ITermsOfDelivery.IncotermCode), "FOB", termOfDeliveryWrapper.IncotermCode);
	}

	public void TestLocation()
	{
		invoiceHeader.JZ_IncoTerm = "CIM";
		var termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertNullOrEmpty(nameof(ITermsOfDelivery.Location), termOfDeliveryWrapper.Location);

		invoiceHeader.JZ_IncoTermPlace = "INCO Place";
		termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertEquals(nameof(ITermsOfDelivery.Location), "INCO Place", termOfDeliveryWrapper.Location);
	}

	public void TestUNLocode()
	{
		invoiceHeader.JZ_IncoTerm = "CIM";
		var termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertNullOrEmpty(nameof(ITermsOfDelivery.UNLocode), termOfDeliveryWrapper.UNLocode);

		invoiceHeader.ZG_AgreedPlaceCode = "AUSYD";
		termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertEquals(nameof(ITermsOfDelivery.UNLocode), "AUSYD", termOfDeliveryWrapper.UNLocode);

		invoiceHeader.ZG_AgreedPlaceCode = "AU";
		termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertNullOrEmpty(nameof(ITermsOfDelivery.UNLocode), termOfDeliveryWrapper.UNLocode);
	}

	public void TestAdditionalTerms()
	{
		invoiceHeader.JZ_IncoTerm = "CIM";
		var termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertNullOrEmpty(nameof(ITermsOfDelivery.AdditionalTerms), termOfDeliveryWrapper.AdditionalTerms);

		invoiceHeader.JZ_IncoTerm = "XXX";
		invoiceHeader.JZ_AdditionalTerms = "FREETEXT";
		termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
		AssertEquals(nameof(ITermsOfDelivery.AdditionalTerms), "FREETEXT", termOfDeliveryWrapper.AdditionalTerms);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
	}

	JobComInvoiceHeader invoiceHeader;

	ITermsOfDelivery GetNewTermOfDeliveryWrapper() => TermsOfDeliveryWrapper.NewOrNull(invoiceHeader);
}
