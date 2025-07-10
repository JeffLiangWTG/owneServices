using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonDeliveryTermsWrapperTest : WrapperHelperTest<CommonDeliveryTermsWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => new CommonDeliveryTermsWrapper(null));

			var entryHeader = Factory.New<CusEntryHeader>();
			AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => new CommonDeliveryTermsWrapper(entryHeader));
		});
	}

	public void TestIncoterm()
	{
		CombineAssertions(() =>
		{
			declaration.JE_ShipmentIncoTerm = "CIF";
			AssertEquals("Expected filled Incoterm with JE_ShipmentIncoTerm", "CIF", wrapper.Incoterm);

			invoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("Expected filled Incoterm with JZ_IncoTerm (even when JE_ShipmentIncoTerm is declared)", "FOB", wrapper.Incoterm);
		});
	}

	public void TestUNLCode()
	{
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_IncoTerm = "XXX";
			declaration.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("In Declaration, expected empty UNLCode when Incoterm is XXX", ZString.Empty, wrapper.UNLCode);

			invoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("In Declaration, expected filled UNLCode when Incoterm is not XXX", "ESADT", wrapper.UNLCode);

			declaration.ZG_AgreedPlaceCode = "ES";
			AssertEquals("In Declaration, expected empty UNLCode when length <= 2 and Incoterm is not XXX", ZString.Empty, wrapper.UNLCode);

			invoiceHeader.JZ_IncoTerm = "XXX";
			invoiceHeader.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("In InvoiceHeader, expected empty UNLCode when Incoterm is XXX", ZString.Empty, wrapper.UNLCode);

			invoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("In InvoiceHeader, expected filled UNLCode when Incoterm is not XXX", "ESADT", wrapper.UNLCode);

			invoiceHeader.ZG_AgreedPlaceCode = "ES";
			AssertEquals("In InvoiceHeader, expected empty UNLCode when length <= 2 and Incoterm is not XXX", ZString.Empty, wrapper.UNLCode);
		});
	}

	public void TestIncotermLocation()
	{
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_IncoTerm = "XXX";
			declaration.JE_ShipmentIncoTermPlace = "BARCELONA";
			AssertEquals("Place in declaration and incoterm in invoiceHeader, expected empty IncotermLocation when Incoterm is XXX, with JE_ShipmentIncoTermPlace", ZString.Empty, wrapper.IncotermLocation);

			invoiceHeader.JZ_IncoTermPlace = "MADRID";
			AssertEquals("Place in invoiceHeader and incoterm in invoiceHeader, expected empty IncotermLocation when Incoterm is XXX, with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", ZString.Empty, wrapper.IncotermLocation);

			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			declaration.JE_ShipmentIncoTermPlace = "BARCELONA";
			AssertEquals("Place in declaration and incoterm in invoiceHeader, expected filled IncotermLocation when Incoterm is not XXX and unlocode is empty, with JE_ShipmentIncoTermPlace", "BARCELONA", wrapper.IncotermLocation);

			declaration.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("Place in declaration and incoterm in invoiceHeader, expected empty IncotermLocation when Incoterm is not XXX but unlocode is not empty", ZString.Empty, wrapper.IncotermLocation);

			declaration.ZG_AgreedPlaceCode = ZString.Empty;
			invoiceHeader.JZ_IncoTermPlace = "MADRID";
			AssertEquals("Incoterm in InvoiceHeader and place in invoiceHeader, expected filled IncotermLocation when Incoterm is not XXX and unlocode is empty, with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", "MADRID", wrapper.IncotermLocation);

			invoiceHeader.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("Place in invoiceHeader and incoterm in invoiceHeader, expected empty IncotermLocation when Incoterm is not XXX but unlocode is not empty", ZString.Empty, wrapper.IncotermLocation);

			invoiceHeader.JZ_IncoTerm = ZString.Empty;
			invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
			declaration.JE_ShipmentIncoTerm = "XXX";
			invoiceHeader.JZ_IncoTermPlace = "BARCELONA";
			AssertEquals("Place in invoiceHeader and incoterm in declaration, expected empty IncotermLocation when Incoterm is XXX, with JE_ShipmentIncoTermPlace", ZString.Empty, wrapper.IncotermLocation);

			declaration.JE_ShipmentIncoTermPlace = "MADRID";
			AssertEquals("Place in declaration and incoterm in declaration, expected empty IncotermLocation when Incoterm is XXX, with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", ZString.Empty, wrapper.IncotermLocation);

			declaration.JE_ShipmentIncoTerm = "FOB";
			declaration.JE_ShipmentIncoTermPlace = "BARCELONA";
			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			AssertEquals("Place in InvoiceHeader and incoterm in declaration, expected filled IncotermLocation when Incoterm is not XXX and unlocode is empty, with JE_ShipmentIncoTermPlace", "BARCELONA", wrapper.IncotermLocation);

			declaration.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("Place in InvoiceHeader and incoterm in declaration, expected empty IncotermLocation when Incoterm is not XXX but unlocode is not empty", ZString.Empty, wrapper.IncotermLocation);

			declaration.ZG_AgreedPlaceCode = ZString.Empty;
			invoiceHeader.JZ_IncoTermPlace = "MADRID";
			AssertEquals("Incoterm in declaration and place in declaration, expected filled IncotermLocation when Incoterm is not XXX and unlocode is empty, with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", "MADRID", wrapper.IncotermLocation);

			invoiceHeader.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("Incoterm in declaration and place in declaration, expected empty IncotermLocation when Incoterm is not XXX but unlocode is not empty", ZString.Empty, wrapper.IncotermLocation);
		});
	}

	public void TestDeliveryCountry()
	{
		CombineAssertions(() =>
		{
			declaration.JE_ShipmentIncoTerm = "FOB";
			declaration.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("In Declaration, when length > 2, expected empty DeliveryCountry when Incoterm is not XXX", ZString.Empty, wrapper.DeliveryCountry);

			declaration.ZG_AgreedPlaceCode = "ES";
			AssertEquals("In Declaration, when length = 2, expected filled DeliveryCountry when Incoterm is not XXX", "ES", wrapper.DeliveryCountry);

			declaration.JE_ShipmentIncoTerm = "XXX";
			AssertEquals("In Declaration, when length = 2, expected empty DeliveryCountry when Incoterm is XXX", ZString.Empty, wrapper.DeliveryCountry);

			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			declaration.ZG_AgreedPlaceCode = "E";
			AssertEquals("In Declaration, when length = 1, expected empty DeliveryCountry when Incoterm is not XXX", ZString.Empty, wrapper.DeliveryCountry);

			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("In InvoiceHeader, when length > 2, expected empty DeliveryCountry when Incoterm is not XXX", ZString.Empty, wrapper.DeliveryCountry);

			invoiceHeader.ZG_AgreedPlaceCode = "ES";
			AssertEquals("In InvoiceHeader, when length = 2, expected filled DeliveryCountry when Incoterm is not XXX", "ES", wrapper.DeliveryCountry);

			invoiceHeader.JZ_IncoTerm = "XXX";
			AssertEquals("In InvoiceHeader, when length = 2, expected filled DeliveryCountry when Incoterm is XXX", ZString.Empty, wrapper.DeliveryCountry);

			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.ZG_AgreedPlaceCode = "E";
			AssertEquals("In InvoiceHeader, when length = 1, expected empty DeliveryCountry when Incoterm is not XXX", ZString.Empty, wrapper.DeliveryCountry);
		});
	}

	public void TestDeliveryText()
	{
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_IncoTerm = "FOB";
			declaration.JE_ShipmentIncoTermPlace = "BARCELONA";
			AssertEquals("Place in declaration and incoterm in invoiceHeader, expected empty DeliveryText when Incoterm is not XXX, with JE_ShipmentIncoTermPlace", ZString.Empty, wrapper.DeliveryText);

			invoiceHeader.JZ_IncoTermPlace = "MADRID";
			AssertEquals("Place in invoiceHeader and incoterm in invoiceHeader, expected empty DeliveryText when Incoterm is not XXX, with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", ZString.Empty, wrapper.DeliveryText);

			invoiceHeader.JZ_IncoTerm = "XXX";
			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			declaration.JE_ShipmentIncoTermPlace = "BARCELONA";
			AssertEquals("Place in declaration and incoterm in invoiceHeader, expected filled DeliveryText when Incoterm is XXX, with JE_ShipmentIncoTermPlace", "BARCELONA", wrapper.DeliveryText);

			invoiceHeader.JZ_IncoTermPlace = "MADRID";
			AssertEquals("Incoterm in InvoiceHeader and place in invoiceHeader, expected filled DeliveryText when Incoterm is XXX, with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", "MADRID", wrapper.DeliveryText);

			invoiceHeader.JZ_IncoTerm = ZString.Empty;
			declaration.JE_ShipmentIncoTerm = "FOB";
			invoiceHeader.JZ_IncoTermPlace = "BARCELONA";
			AssertEquals("Place in invoiceHeader and incoterm in declaration, expected empty DeliveryText when Incoterm is not XXX, with JE_ShipmentIncoTermPlace", ZString.Empty, wrapper.DeliveryText);

			declaration.JE_ShipmentIncoTermPlace = "MADRID";
			AssertEquals("Place in declaration and incoterm in declaration, expected empty DeliveryText when Incoterm is not XXX, with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", ZString.Empty, wrapper.DeliveryText);

			declaration.JE_ShipmentIncoTerm = "XXX";
			declaration.JE_ShipmentIncoTermPlace = "BARCELONA";
			invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
			AssertEquals("Place in InvoiceHeader and incoterm in declaration, expected filled DeliveryText when Incoterm is XXX, with JE_ShipmentIncoTermPlace", "BARCELONA", wrapper.DeliveryText);

			invoiceHeader.JZ_IncoTermPlace = "MADRID";
			AssertEquals("Incoterm in declaration and place in declaration, expected filled DeliveryText when Incoterm is XXX, with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", "MADRID", wrapper.DeliveryText);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = new CommonDeliveryTermsWrapper(entryHeader);
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	CusEntryHeader entryHeader;
	CommonDeliveryTermsWrapper wrapper;

	protected override CommonDeliveryTermsWrapper GetProvider() => wrapper;
}
