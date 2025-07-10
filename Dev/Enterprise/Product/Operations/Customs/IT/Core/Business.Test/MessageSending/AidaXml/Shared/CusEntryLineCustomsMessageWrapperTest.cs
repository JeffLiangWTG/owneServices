using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class CusEntryLineCustomsMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryLineCustomsMessageWrapper(entryLine: null));
	}

	public void TestItemNumber()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ItemNumber), 0, itemWrapper.ItemNumber);

		entryLine.CL_LineNumber = 9;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ItemNumber), 9, itemWrapper.ItemNumber);
	}

	public void TestProcedure()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNull(nameof(ICusEntryLineCustomsMessageWrapper.Procedure), itemWrapper.Procedure);

		invoiceLine.JI_Procedure = "1000";
		itemWrapper = GetCusEntryLineWrapper();
		var procedure = itemWrapper.Procedure;
		AssertType<CustomsProcedureWrapper>(nameof(ICusEntryLineCustomsMessageWrapper.Procedure), procedure);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Procedure), procedure, itemWrapper.Procedure);
	}

	public void TestProcedure_AdditionalProcedureCodes()
	{
		invoiceLine.JI_Procedure = "1000";
		declaration.JE_MessageType = "IMP";

		var itemWrapper = GetCusEntryLineWrapper();
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.Procedure.AdditionalProcedures), new string[] { "1NN" }, itemWrapper.Procedure.AdditionalProcedures.ToArray());

		declaration.JE_MessageType = "EXP";
		itemWrapper = GetCusEntryLineWrapper();
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.Procedure.AdditionalProcedures), Array.Empty<string>(), itemWrapper.Procedure.AdditionalProcedures.ToArray());
	}

	public void TestPreviousDocuments_Export()
	{
		declaration.JE_MessageType = "EXP";
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments), itemWrapper.PreviousDocuments);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments), Array.Empty<IPreviousDocument>(), itemWrapper.PreviousDocuments.ToArray());

		invoiceLine.PreviousDocuments.AddNew().CSI_Code = "IP1";
		invoiceLine.PreviousDocuments.AddNew().CSI_Code = "IP2";
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.PreviousDocuments.AddNew().CSI_Code = "IP3";

		itemWrapper = GetCusEntryLineWrapper();
		var previousDocuments = itemWrapper.PreviousDocuments;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments)} count", 3, previousDocuments.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments), previousDocuments, itemWrapper.PreviousDocuments);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments)}", true, itemWrapper.PreviousDocuments.All(x => x is Export.PreviousDocumentWrapper));
	}

	public void TestPreviousDocuments_Import()
	{
		declaration.JE_MessageType = "IMP";
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments), itemWrapper.PreviousDocuments);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments), Array.Empty<IPreviousDocument>(), itemWrapper.PreviousDocuments.ToArray());

		var pd1 = invoiceLine.PreviousDocuments.AddNew();
		pd1.CSI_Code = "IP1";
		pd1.CSI_Quantity3 = 10;
		pd1.CSI_UnitOfQuantity3 = "KG";
		var pd2 = invoiceLine.PreviousDocuments.AddNew();
		pd2.CSI_Code = "IP2";
		pd2.CSI_Quantity3 = 10;
		pd2.CSI_UnitOfQuantity3 = "KG";

		var invoiceLine2 = AddNewInvoiceLineToEntry();
		var pd3 = invoiceLine2.PreviousDocuments.AddNew();
		pd3.CSI_Code = "IP1";
		pd3.CSI_Quantity3 = 10;
		pd2.CSI_UnitOfQuantity3 = "KG";

		itemWrapper = GetCusEntryLineWrapper();
		var previousDocuments = itemWrapper.PreviousDocuments;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments)} count", 2, previousDocuments.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments), previousDocuments, itemWrapper.PreviousDocuments);
		AssertEquals(
			$"{nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.PreviousDocuments)}",
			true,
			itemWrapper.PreviousDocuments.All(x => x is PreviousDocumentWrapper));

		var gpdw1 = previousDocuments.FirstOrDefault(x => x.DocumentType.Equals("IP1"));
		AssertEquals("Quantity for IP1", 20m, gpdw1.Quantity);

		var gpdw2 = previousDocuments.FirstOrDefault(x => x.DocumentType.Equals("IP2"));
		AssertEquals("Quantity for IP2", 10m, gpdw2.Quantity);
	}

	public void TestAdditionalInformation()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"[PRE-CONDITION] {nameof(ICusEntryLineCustomsMessageWrapper.AdditionalInformation)} count", 0, invoiceLine.AdditionalInfos.Count);
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalInformation), itemWrapper.AdditionalInformation);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.AdditionalInformation)} count", 1, itemWrapper.AdditionalInformation.Count);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.AdditionalInformation)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.SupportingDocuments)}", true, itemWrapper.SupportingDocuments.All(x => x is NoneOfAboveAdditionalInformationWrapper));

		invoiceLine.AdditionalInfos.AddNew().CSI_Code = "1";
		invoiceLine.AdditionalInfos.AddNew().CSI_Code = "2";
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.AdditionalInfos.AddNew().CSI_Code = "3";

		itemWrapper = GetCusEntryLineWrapper();
		var additionalInformation = itemWrapper.AdditionalInformation;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.AdditionalInformation)} count", 3, additionalInformation.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalInformation), additionalInformation, itemWrapper.AdditionalInformation);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.AdditionalInformation)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.AdditionalInformation)}", true, itemWrapper.AdditionalInformation.All(x => x is AdditionalInformationWrapper));
	}

	public void TestSupportingDocuments()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.SupportingDocuments), itemWrapper.SupportingDocuments);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.SupportingDocuments), Array.Empty<ISupportingDocument>(), itemWrapper.SupportingDocuments.ToArray());

		invoiceHeader.SupportingDocuments.AddNew().CSI_Code = "S0";
		invoiceLine.SupportingDocuments.AddNew().CSI_Code = "S1";
		invoiceLine.SupportingDocuments.AddNew().CSI_Code = "S2";
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.SupportingDocuments.AddNew().CSI_Code = "S3";

		itemWrapper = GetCusEntryLineWrapper();
		var supportingDocuments = itemWrapper.SupportingDocuments;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.SupportingDocuments)} count", 4, supportingDocuments.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.SupportingDocuments), supportingDocuments, itemWrapper.SupportingDocuments);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.SupportingDocuments)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.SupportingDocuments)}", true, itemWrapper.SupportingDocuments.All(x => x is SupportingDocumentWrapper));
	}

	public void TestExporter_WhenExporterAddressIsNotSet()
	{
		invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNull(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), itemWrapper.Exporter);
	}

	public void TestExporter_WhenEoriCodeIsNotProvided()
	{
		var exporter = GetNewExporter();
		invoiceLine.JI_OA_ExporterAddress = exporter.MainAddress.PK;
		var itemWrapper = GetCusEntryLineWrapper();
		var exporterWrapper = itemWrapper.Exporter;
		CombineAssertions(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), () =>
		{
			AssertType<TraderCustomsMessageWrapper>(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), exporterWrapper);
			TraderWrapperAssertionHelper.AssertTrader(exporterWrapper, "EXPORTER ADDRESS 1 EXPORTER ADDRESS 2", "EXPORTER CITY", "IT", expectedIdentificationNumber: ZString.Empty, "EXPORTER COMPANY NAME", "99999");
			AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), exporterWrapper, itemWrapper.Exporter);
		});
	}

	public void TestExporter_WhenEoriCodeIsProvided()
	{
		var exporter = GetNewExporter();
		exporter.CustomsCodes.AddNew("EOR", "385040449", "IT");
		invoiceLine.JI_OA_ExporterAddress = exporter.MainAddress.PK;
		var itemWrapper = GetCusEntryLineWrapper();
		var exporterWrapper = itemWrapper.Exporter;
		CombineAssertions(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), () =>
		{
			AssertType<TraderCustomsMessageWrapper>(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), exporterWrapper);
			AssertNull(nameof(ITrader.Address), exporterWrapper.Address);
			AssertEquals(nameof(ITrader.IdentificationNumber), "IT385040449", exporterWrapper.IdentificationNumber);
			AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), exporterWrapper, itemWrapper.Exporter);
		});
	}

	public void TestExporter_FallbackToDeclaration()
	{
		var exporter = GetNewExporter();
		invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
		declaration.JE_OH_Supplier = exporter.PK;
		var itemWrapper = GetCusEntryLineWrapper();
		var exporterWrapper = itemWrapper.Exporter;
		CombineAssertions(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), () =>
		{
			AssertType<TraderCustomsMessageWrapper>(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), exporterWrapper);
			TraderWrapperAssertionHelper.AssertTrader(exporterWrapper, "EXPORTER ADDRESS 1 EXPORTER ADDRESS 2", "EXPORTER CITY", "IT", expectedIdentificationNumber: ZString.Empty, "EXPORTER COMPANY NAME", "99999");
			AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Exporter), exporterWrapper, itemWrapper.Exporter);
		});
	}

	public void TestSeller()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNull(nameof(ICusEntryLineCustomsMessageWrapper.Seller), itemWrapper.Seller);

		var seller = Factory.New<OrgHeader>();
		seller.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var sellerAddress = seller.MainAddress;
		sellerAddress.CompanyName = "SELLER COMPANY NAME";
		sellerAddress.Address1 = "SELLER ADDRESS 1";
		sellerAddress.Address2 = "SELLER ADDRESS 2";
		sellerAddress.Postcode = "99999";
		sellerAddress.City = "SELLER CITY";
		sellerAddress.OA_RN_NKCountryCode = "IT";

		invoiceLine.SellerDocAddress.OrganisationPK = seller.PK;
		itemWrapper = GetCusEntryLineWrapper();
		var sellerWrapper = itemWrapper.Seller;
		CombineAssertions(nameof(ICusEntryLineCustomsMessageWrapper.Seller), () =>
		{
			AssertType<TraderWrapper>(nameof(ICusEntryLineCustomsMessageWrapper.Seller), sellerWrapper);
			TraderWrapperAssertionHelper.AssertTrader(sellerWrapper, "SELLER ADDRESS 1 SELLER ADDRESS 2", "SELLER CITY", "IT", "IT385040449", "SELLER COMPANY NAME", "99999");
			AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Seller), sellerWrapper, itemWrapper.Seller);
		});
	}

	public void TestBuyer()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNull(nameof(ICusEntryLineCustomsMessageWrapper.Buyer), itemWrapper.Buyer);

		var buyer = Factory.New<OrgHeader>();
		buyer.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var buyerAddress = buyer.MainAddress;
		buyerAddress.CompanyName = "BUYER COMPANY NAME";
		buyerAddress.Address1 = "BUYER ADDRESS 1";
		buyerAddress.Address2 = "BUYER ADDRESS 2";
		buyerAddress.Postcode = "99999";
		buyerAddress.City = "BUYER CITY";
		buyerAddress.OA_RN_NKCountryCode = "IT";

		invoiceLine.BuyerDocAddress.OrganisationPK = buyer.PK;
		itemWrapper = GetCusEntryLineWrapper();
		var buyerWrapper = itemWrapper.Buyer;
		CombineAssertions(nameof(ICusEntryLineCustomsMessageWrapper.Buyer), () =>
		{
			AssertType<TraderWrapper>(nameof(ICusEntryLineCustomsMessageWrapper.Buyer), buyerWrapper);
			TraderWrapperAssertionHelper.AssertTrader(buyerWrapper, "BUYER ADDRESS 1 BUYER ADDRESS 2", "BUYER CITY", "IT", "IT385040449", "BUYER COMPANY NAME", "99999");
			AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Buyer), buyerWrapper, itemWrapper.Buyer);
		});
	}

	public void TestAdditionalSupplyChainActors()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalSupplyChainActors), itemWrapper.AdditionalSupplyChainActors);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalSupplyChainActors), Array.Empty<IAdditionalSupplyChainActor>(), itemWrapper.AdditionalSupplyChainActors.ToArray());

		invoiceLine.CusSupplyChainActorReferences.AddNew().CFR_Code = "SC1";
		invoiceLine.CusSupplyChainActorReferences.AddNew().CFR_Code = "SC2";
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.CusSupplyChainActorReferences.AddNew().CFR_Code = "SC3";

		itemWrapper = GetCusEntryLineWrapper();
		var additionalSupplyChainActors = itemWrapper.AdditionalSupplyChainActors;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.AdditionalSupplyChainActors)} count", 3, additionalSupplyChainActors.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalSupplyChainActors), additionalSupplyChainActors, itemWrapper.AdditionalSupplyChainActors);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.AdditionalSupplyChainActors)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.AdditionalSupplyChainActors)}", true, itemWrapper.AdditionalSupplyChainActors.All(x => x is AdditionalSupplyChainActorWrapper));
	}

	public void TestFiscalReferences()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.FiscalReferences), itemWrapper.FiscalReferences);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.FiscalReferences), Array.Empty<IFiscalReference>(), itemWrapper.FiscalReferences.ToArray());

		invoiceLine.FiscalReferences.AddNew().CFR_Code = "FR1";
		invoiceLine.FiscalReferences.AddNew().CFR_Code = "FR2";
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.FiscalReferences.AddNew().CFR_Code = "FR3";

		itemWrapper = GetCusEntryLineWrapper();
		var fiscalReferences = itemWrapper.FiscalReferences;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.FiscalReferences)} count", 3, fiscalReferences.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.FiscalReferences), fiscalReferences, itemWrapper.FiscalReferences);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.FiscalReferences)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.FiscalReferences)}", true, itemWrapper.FiscalReferences.All(x => x is FiscalReferenceWrapper));
	}

	public void TestFees()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.Fees), itemWrapper.Fees);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.Fees), Array.Empty<IFee>(), itemWrapper.Fees.ToArray());

		AddNewFee(entryLine.Fees, "A00", "", 0m);
		AddNewFee(entryLine.Fees, "A01", "F", 0m);
		AddNewFee(entryLine.Fees, "A02", "X", 0m);

		AddNewFee(entryLine.Fees, "A03", "A", 0m);
		AddNewFee(entryLine.Fees, "A04", "E", 0m);
		AddNewFee(entryLine.Fees, "A05", "G", 0m);
		AddNewFee(entryLine.Fees, "A06", "T", 0m);
		AddNewFee(entryLine.Fees, "A07", "R", 0m);

		itemWrapper = GetCusEntryLineWrapper();
		var fees = itemWrapper.Fees;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.Fees)} count", 5, fees.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Fees), fees, itemWrapper.Fees);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.Fees)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.Fees)}", true, itemWrapper.Fees.All(x => x is FeeWrapper));
		AssertArrayEqualsByElements("Charge Types", new[] { "A03", "A04", "A05", "A06", "A07" }, fees.Select(x => x.ChargeType).ToArray());
	}

	public void TestFees_WithDifferentActionTypes()
	{
		var fee1 = entryLine.Fees.AddNew();
		fee1.CF_ChargeType = "A01";
		var fee2 = entryLine.Fees.AddNew();
		fee2.CF_ChargeType = "A18";

		var wrapper = GetCusEntryLineWrapper();
		CombineAssertions("All Actions are empty", () =>
		{
			AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.Fees)} count", 2, wrapper.Fees.Count);
			AssertArrayEqualsByElements("Charge Types", new[] { "A01", "A18" }, wrapper.Fees.Select(x => x.ChargeType).ToArray());
		});

		fee1.CF_RateOverrideReasonCode = "ADD";
		fee2.CF_RateOverrideReasonCode = "OVR";

		wrapper = GetCusEntryLineWrapper();
		CombineAssertions("Actions are ADD and OVR", () =>
		{
			AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.Fees)} count", 2, wrapper.Fees.Count);
			AssertArrayEqualsByElements("Charge Types", new[] { "A01", "A18" }, wrapper.Fees.Select(x => x.ChargeType).ToArray());
		});

		fee1.CF_RateOverrideReasonCode = "EXC";

		wrapper = GetCusEntryLineWrapper();
		CombineAssertions("Actions contains EXC", () =>
		{
			AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.Fees)} count", 1, wrapper.Fees.Count);
			AssertArrayEqualsByElements("Charge Types", new[] { "A18" }, wrapper.Fees.Select(x => x.ChargeType).ToArray());
		});
	}

	public void TestFeesOrder()
	{
		SetUpFees(entryLine.Fees, "407", "406", "405", "927", "911", "201", "165", "116", "A35", "A30", "A20", "A10", "A00");

		var itemWrapper = GetCusEntryLineWrapper();
		var fees = itemWrapper.Fees;
		AssertArrayEqualsByElements("Item Wrapper Fees order", new[] { "A00", "A10", "A20", "A30", "A35", "116", "165", "201", "911", "927", "405", "406", "407" }, fees.Select(x => x.ChargeType).ToArray());
	}

	public void TestTotalFeeAmount()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.TotalFeeAmount), 0m, itemWrapper.TotalFeeAmount);

		AddNewFee(entryLine.Fees, "A00", "", 10m);
		AddNewFee(entryLine.Fees, "A01", "F", 10m);
		AddNewFee(entryLine.Fees, "A02", "X", 10m);
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.TotalFeeAmount), 0m, itemWrapper.TotalFeeAmount);

		AddNewFee(entryLine.Fees, "A03", "A", 10.123m);
		AddNewFee(entryLine.Fees, "A04", "E", 20.123m);
		AddNewFee(entryLine.Fees, "A05", "G", 30.123m);
		AddNewFee(entryLine.Fees, "A06", "T", 40.123m);
		AddNewFee(entryLine.Fees, "A07", "R", 50.123m);
		AddNewFee(entryLine.Fees, "406", "R", -5.123m);
		AddNewFee(entryLine.Fees, "407", "R", -5.123m);
		var fee = AddNewFee(entryLine.Fees, "AAA", "R", 100m);
		fee.CF_RateOverrideReasonCode = "EXC";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.TotalFeeAmount), 140.36m, itemWrapper.TotalFeeAmount);
	}

	public void TestRelatedIndicatorWithAllInvoiceIndicators()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator), "0000", itemWrapper.RelatedIndicator);

		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.Yes;
		invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.Yes;
		invoiceLine.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.No;
		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.Yes;

		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator), "1101", itemWrapper.RelatedIndicator);

		invoiceHeader.RelatedIndicator3 = true;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator) + " value is ignored on header because it is set on line", "1101", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator) + " value is used on header", "1111", itemWrapper.RelatedIndicator);
	}

	public void TestRelatedIndicatorWithInvoiceRelatedIndicator()
	{
		invoiceHeader.RelatedIndicator = true;
		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.Yes;
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator)} and Line RelatedIndicator is {invoiceLine.JI_RelatedIndicator}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "1000", itemWrapper.RelatedIndicator);

		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator)} and Line RelatedIndicator is {invoiceLine.JI_RelatedIndicator}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "1000", itemWrapper.RelatedIndicator);

		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.No;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator)} and Line RelatedIndicator is {invoiceLine.JI_RelatedIndicator}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);

		invoiceHeader.RelatedIndicator = false;
		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.Yes;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator)} and Line RelatedIndicator is {invoiceLine.JI_RelatedIndicator}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "1000", itemWrapper.RelatedIndicator);

		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator)} and Line RelatedIndicator is {invoiceLine.JI_RelatedIndicator}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);

		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.No;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator)} and Line RelatedIndicator is {invoiceLine.JI_RelatedIndicator}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);
	}

	public void TestRelatedIndicatorWithInvoiceRelatedIndicator2()
	{
		invoiceHeader.RelatedIndicator2 = true;
		invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.Yes;
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator2 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator2)} and Line RelatedIndicator2 is {invoiceLine.ZG_RelatedIndicator2}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0100", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator2 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator2)} and Line RelatedIndicator2 is {invoiceLine.ZG_RelatedIndicator2}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0100", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.No;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator2 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator2)} and Line RelatedIndicator2 is {invoiceLine.ZG_RelatedIndicator2}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);

		invoiceHeader.RelatedIndicator2 = false;
		invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.Yes;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator2 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator2)} and Line RelatedIndicator2 is {invoiceLine.ZG_RelatedIndicator2}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0100", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator2 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator2)} and Line RelatedIndicator2 is {invoiceLine.ZG_RelatedIndicator2}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.No;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator2 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator2)} and Line RelatedIndicator2 is {invoiceLine.ZG_RelatedIndicator2}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);
	}

	public void TestRelatedIndicatorWithInvoiceRelatedIndicator3()
	{
		invoiceHeader.RelatedIndicator3 = true;
		invoiceLine.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.Yes;
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator3 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator3)} and Line RelatedIndicator3 is {invoiceLine.ZG_RelatedIndicator3}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0010", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator3 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator3)} and Line RelatedIndicator3 is {invoiceLine.ZG_RelatedIndicator3}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0010", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.No;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator3 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator3)} and Line RelatedIndicator3 is {invoiceLine.ZG_RelatedIndicator3}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);

		invoiceHeader.RelatedIndicator3 = false;
		invoiceLine.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.Yes;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator3 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator3)} and Line RelatedIndicator3 is {invoiceLine.ZG_RelatedIndicator3}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0010", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator3 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator3)} and Line RelatedIndicator3 is {invoiceLine.ZG_RelatedIndicator3}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator3 = ValuationIndicatorCodeList.Codes.No;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator3 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator3)} and Line RelatedIndicator3 is {invoiceLine.ZG_RelatedIndicator3}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);
	}

	public void TestRelatedIndicatorWithInvoiceRelatedIndicator4()
	{
		invoiceHeader.RelatedIndicator4 = true;
		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.Yes;
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator4 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator4)} and Line RelatedIndicator4 is {invoiceLine.ZG_RelatedIndicator4}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0001", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator4 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator4)} and Line RelatedIndicator4 is {invoiceLine.ZG_RelatedIndicator4}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0001", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.No;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator4 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator4)} and Line RelatedIndicator4 is {invoiceLine.ZG_RelatedIndicator4}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);

		invoiceHeader.RelatedIndicator4 = false;
		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.Yes;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator4 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator4)} and Line RelatedIndicator4 is {invoiceLine.ZG_RelatedIndicator4}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0001", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator4 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator4)} and Line RelatedIndicator4 is {invoiceLine.ZG_RelatedIndicator4}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);

		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.No;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When Header RelatedIndicator4 is {GetTickedOrUnticked(invoiceHeader.RelatedIndicator4)} and Line RelatedIndicator4 is {invoiceLine.ZG_RelatedIndicator4}, {nameof(ICusEntryLineCustomsMessageWrapper.RelatedIndicator)}", "0000", itemWrapper.RelatedIndicator);
	}

	public void TestItemPrice()
	{
		var today = ZDateTime.Today;
		var dummyCurrency = RefCurrency.New(Factory);
		dummyCurrency.RX_Code = "FTM";
		dummyCurrency.SetCustomsRate(today.AddDays(-1), today.AddDays(1), 2m);

		invoiceHeader.JZ_RX_NKInvoice_Currency = "FTM";
		invoiceLine.JI_LinePrice = 200.14m;
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ItemPrice), 200.14m, itemWrapper.ItemPrice);

		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ItemPrice), 200.14m, itemWrapper.ItemPrice);
	}

	public void TestValuationMethod()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"Default {nameof(ICusEntryLineCustomsMessageWrapper.ValuationMethod)}", 1, itemWrapper.ValuationMethod);

		invoiceLine.JI_ValuationCode = "1";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"Valid {nameof(ICusEntryLineCustomsMessageWrapper.ValuationMethod)}", 1, itemWrapper.ValuationMethod);

		invoiceLine.JI_ValuationCode = "A";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"Invalid {nameof(ICusEntryLineCustomsMessageWrapper.ValuationMethod)}", 0, itemWrapper.ValuationMethod);
	}

	public void TestPreferences()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNull(nameof(ICusEntryLineCustomsMessageWrapper.Preferences), itemWrapper.Preferences);

		invoiceLine.JI_PrimaryPreference = "1";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.Preferences), 1, itemWrapper.Preferences);

		invoiceLine.JI_PrimaryPreference = "A";
		itemWrapper = GetCusEntryLineWrapper();
		AssertNull(nameof(ICusEntryLineCustomsMessageWrapper.Preferences), itemWrapper.Preferences);
	}

	public void TestDestinationCountryCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.DestinationCountryCode), "", itemWrapper.DestinationCountryCode);

		declaration.JE_GoodsDestination = "CN";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.DestinationCountryCode), "CN", itemWrapper.DestinationCountryCode);
	}

	public void TesCountryOfDestinationWithSingleLine()
	{
		AssertExportWrapperFieldWithSingleLine("CountryOfDestination"
		, (line, value) => { line.ZG_CountryOfDestination = value; }
		, (declaration, value) => { declaration.JE_GoodsDestination = value; }
		, (wrapper) => wrapper.CountryOfDestination);
	}

	public void TestCountryOfDestination()
	{
		AssertExportWrapperField("CountryOfDestination"
		, (line, value) => { line.ZG_CountryOfDestination = value; }
		, (declaration, value) => { declaration.JE_GoodsDestination = value; }
		, (wrapper) => wrapper.CountryOfDestination);
	}

	public void TestCountryOfExportWithSingleLine()
	{
		AssertExportWrapperFieldWithSingleLine("CountryOfExport"
		, (line, value) => { line.JI_RN_NKCountryOfExport = value; }
		, (declaration, value) => { declaration.JE_GoodsOrigin = value; }
		, (wrapper) => wrapper.CountryOfExport);
	}

	public void TestCountryOfExport()
	{
		using (ITCustomsDataRegistry.Instance.ExportMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MessageVersionList.Codes.XML))
		{
			AssertExportWrapperField("CountryOfExport"
			, (line, value) => { line.JI_RN_NKCountryOfExport = value; }
			, (declaration, value) => { declaration.JE_GoodsOrigin = value; }
			, (wrapper) => wrapper.CountryOfExport);
		}
	}

	public void TestExportConsignorWithSingleLine()
	{
		AssertExportWrapperTraderWithSingleLine("Consignor"
		, (line, value) => { line.JI_OA_ExporterAddress = value; }
		, (declaration, value) => { declaration.SupplierDocumentaryAddress.E2_OA_Address = value; }
		, (wrapper) => wrapper.ExportConsignor);
	}

	public void TestExportConsignor()
	{
		AssertExportWrapperTrader("Consignor"
			, (line, value) => { line.JI_OA_ExporterAddress = value; }
			, (declaration, value) => { declaration.SupplierDocumentaryAddress.E2_OA_Address = value; }
			, (wrapper) => wrapper.ExportConsignor);
	}

	public void TestExportConsigeeWithSingleLine()
	{
		AssertExportWrapperTraderWithSingleLine("Consignee"
			, (line, value) => { line.JI_OA_ConsigneeAddress = value; }
			, (declaration, value) => { declaration.ImporterDocumentaryAddress.E2_OA_Address = value; }
			, (wrapper) => wrapper.ExportConsignee);
	}

	public void TestExportConsignee()
	{
		AssertExportWrapperTrader("Consignee"
			, (line, value) => { line.JI_OA_ConsigneeAddress = value; }
			, (declaration, value) => { declaration.ImporterDocumentaryAddress.E2_OA_Address = value; }
			, (wrapper) => wrapper.ExportConsignee);
	}

	public void TestDestinationStateCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.DestinationStateCode), "", itemWrapper.DestinationStateCode);

		var destinationUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "ITVCE");
		var refCountryState = Factory.New<RefCountryStates>();
		refCountryState.RW_Code = "XX";
		refCountryState.RW_RN_NKCountryCode = "IT";
		destinationUnloco.RL_RW = refCountryState.PK;

		declaration.JE_RL_NKFinalDestination = "ITVCE";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.DestinationStateCode), "XX", itemWrapper.DestinationStateCode);

		var smDestinationUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SMZAC");
		var smRefCountryState = Factory.New<RefCountryStates>();
		smRefCountryState.RW_Code = "ZA";
		smRefCountryState.RW_RN_NKCountryCode = "SM";
		smDestinationUnloco.RL_RW = smRefCountryState.PK;

		declaration.JE_RL_NKFinalDestination = "SMZAC";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.DestinationStateCode), "SM", itemWrapper.DestinationStateCode);
	}

	public void TestDispatchCountryCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.DispatchCountryCode), "", itemWrapper.DispatchCountryCode);

		declaration.JE_GoodsOrigin = "DE";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.DispatchCountryCode), "DE", itemWrapper.DispatchCountryCode);
	}

	public void TestOriginCountryCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.OriginCountryCode), "", itemWrapper.OriginCountryCode);

		invoiceLine.JI_PrimaryPreference = "";
		invoiceLine.JI_CountryOfOrigin = "ES";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.OriginCountryCode), "ES", itemWrapper.OriginCountryCode);

		invoiceLine.JI_PrimaryPreference = "1";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.OriginCountryCode), "ES", itemWrapper.OriginCountryCode);

		invoiceLine.JI_PrimaryPreference = "2";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals($"When JI_CountryOfOrigin is set but primary preference does not start with '1', {nameof(ICusEntryLineCustomsMessageWrapper.OriginCountryCode)}", "", itemWrapper.OriginCountryCode);
	}

	public void TestPreferredOriginCountryCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.PreferredOriginCountryCode), "", itemWrapper.PreferredOriginCountryCode);

		invoiceLine.JI_PrimaryPreference = "200";
		invoiceLine.JI_CountryOfOrigin = "ES";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.PreferredOriginCountryCode), "ES", itemWrapper.PreferredOriginCountryCode);
	}

	public void TestAcceptanceDate()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNull($"{nameof(ICusEntryLineCustomsMessageWrapper.AcceptanceDate)} not managed now", itemWrapper.AcceptanceDate);
	}

	public void TestNetMass()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.NetMass), 0m, itemWrapper.NetMass);

		invoiceLine.JI_CustomsQuantity = 20.30;
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.JI_CustomsQuantity = 22.30;

		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.NetMass), 42.60m, itemWrapper.NetMass);
	}

	public void TestSupplementaryUnit()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNull(nameof(ICusEntryLineCustomsMessageWrapper.SupplementaryUnit), itemWrapper.SupplementaryUnit);

		invoiceLine.JI_CustomsSecondQuantity = 10.30;
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.JI_CustomsSecondQuantity = 12.30;

		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.SupplementaryUnit), 22.60m, itemWrapper.SupplementaryUnit);
	}

	public void TestGrossMass()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.GrossMass), 0m, itemWrapper.GrossMass);

		invoiceLine.JI_Weight = 100.30;
		invoiceLine.JI_WeightUQ = "KG";
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.JI_Weight = 1.30;
		invoiceLine2.JI_WeightUQ = "T";

		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.GrossMass), 1400.3m, itemWrapper.GrossMass);
	}

	public void TestGoodsDescription()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.GoodsDescription), "", itemWrapper.GoodsDescription);

		invoiceLine.JI_Description = "GOODS DESCRIPTION";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.GoodsDescription), "GOODS DESCRIPTION", itemWrapper.GoodsDescription);
	}

	public void TestPackages()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.Packages), itemWrapper.Packages);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.Packages), Array.Empty<IPackage>(), itemWrapper.Packages.ToArray());

		var pack1 = declaration.Packages.AddNew();
		var pack2 = declaration.Packages.AddNew();
		declaration.Packages.AddNew();

		var invoiceLine2 = AddNewInvoiceLineToEntry();

		var packPivot1 = invoiceLine.PackagesPivot.AddNew();
		var packPivot2 = invoiceLine2.PackagesPivot.AddNew();

		packPivot1.CHC_CW = pack1.PK;
		packPivot2.CHC_CW = pack2.PK;

		entryLine.InvoiceLines.Reload(reLoadExistingRows: true);

		itemWrapper = GetCusEntryLineWrapper();
		var packages = itemWrapper.Packages;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.Packages)} count", 2, packages.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Packages), packages, itemWrapper.Packages);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.Packages)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.Packages)}", true, itemWrapper.Packages.All(x => x is PackageWrapper));
	}

	public void TestCusCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.CusCode), "", itemWrapper.CusCode);

		declaration.JE_MessageType = "EXP";
		invoiceLine.ZG_CusNumber = "0010001-6";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.CusCode), "0010001-6", itemWrapper.CusCode);

		declaration.JE_MessageType = "IMP";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.CusCode), "00100016", itemWrapper.CusCode);
	}

	public void TestNcCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.NcCode), "", itemWrapper.NcCode);

		invoiceLine.JI_FormattedTariff = "0123456789ABCD";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.NcCode), "01234567", itemWrapper.NcCode);
	}

	public void TestTaricCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.TaricCode), "", itemWrapper.TaricCode);

		invoiceLine.JI_FormattedTariff = "0123456789ABCD";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.TaricCode), "89", itemWrapper.TaricCode);
	}

	public void TestAdditionalCodes()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalCodes), itemWrapper.AdditionalCodes);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalCodes), Array.Empty<string>(), itemWrapper.AdditionalCodes.ToArray());

		invoiceLine.JI_SupplementaryCode1 = "20";
		invoiceLine.JI_SupplementaryCode2 = "10";
		invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "A45";

		itemWrapper = GetCusEntryLineWrapper();
		var additionalCodes = itemWrapper.AdditionalCodes;
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalCodes), new[] { "A45", "20" }, additionalCodes.ToArray());
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.AdditionalCodes), additionalCodes, itemWrapper.AdditionalCodes);
	}

	public void TestNationalAdditionalCodes()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.NationalAdditionalCodes), itemWrapper.NationalAdditionalCodes);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.NationalAdditionalCodes), Array.Empty<string>(), itemWrapper.NationalAdditionalCodes.ToArray());

		invoiceLine.JI_SupplementaryCode1 = "Q20";
		invoiceLine.JI_SupplementaryCode2 = "20";
		invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "P50";

		itemWrapper = GetCusEntryLineWrapper();
		var nationalAdditionalCodes = itemWrapper.NationalAdditionalCodes;
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.NationalAdditionalCodes), new[] { "Q20" }, nationalAdditionalCodes.ToArray());
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.NationalAdditionalCodes), nationalAdditionalCodes, itemWrapper.NationalAdditionalCodes);
	}

	public void TestContainers()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.Containers), itemWrapper.Containers);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.Containers), Array.Empty<string>(), itemWrapper.Containers.ToArray());

		var cnt1 = declaration.CusContainers.AddNew();
		cnt1.CO_ContainerNumber = "CNT1";
		var cnt2 = declaration.CusContainers.AddNew();
		cnt2.CO_ContainerNumber = "CNT2";
		var cnt3 = declaration.CusContainers.AddNew();
		cnt3.CO_ContainerNumber = "CNT2";
		var cnt4 = declaration.CusContainers.AddNew();
		cnt4.CO_ContainerNumber = "CNT4";

		var invoiceLine2 = AddNewInvoiceLineToEntry();

		var cntPivot1 = invoiceLine.ContainersPivot.AddNew();
		cntPivot1.C2_CO = cnt1.PK;
		var cntPivot2 = invoiceLine2.ContainersPivot.AddNew();
		cntPivot2.C2_CO = cnt2.PK;
		var cntPivot3 = invoiceLine2.ContainersPivot.AddNew();
		cntPivot3.C2_CO = cnt3.PK;

		itemWrapper = GetCusEntryLineWrapper();
		var containersCollectionWrapper = itemWrapper.Containers;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.Containers)} count", 2, containersCollectionWrapper.Count);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.Containers), new[] { "CNT1", "CNT2" }, containersCollectionWrapper.ToArray());
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.Containers), containersCollectionWrapper, itemWrapper.Containers);
	}

	public void TestConcessionOrder()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ConcessionOrder), "", itemWrapper.ConcessionOrder);

		invoiceLine.JI_ConcessionOrder = "55555";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ConcessionOrder), "55555", itemWrapper.ConcessionOrder);
	}

	public void TestTransactionNature()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.TransactionNature), 0, itemWrapper.TransactionNature);

		invoiceHeader.JZ_ValuationCode = "11";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.TransactionNature), 11, itemWrapper.TransactionNature);

		invoiceHeader.JZ_ValuationCode = "A";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.TransactionNature), 0, itemWrapper.TransactionNature);
	}

	public void TestExportNatureOfTransaction()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNull("When no invoices are found, ExportNatureOfTransaction", itemWrapper.ExportNatureOfTransaction);

		invoiceHeader.JZ_ValuationCode = "11";
		itemWrapper = GetCusEntryLineWrapper();
		AssertNull("When only one invoice is found, ExportNatureOfTransaction", itemWrapper.ExportNatureOfTransaction);

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		entryLine1.InvoiceLines.Add(invoiceLine1);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		entryLine2.InvoiceLines.Add(invoiceLine2);

		CombineAssertions(() =>
		{
			invoice1.JZ_ValuationCode = "11";
			invoice2.JZ_ValuationCode = "11";
			var wrapper1 = GetCusEntryLineWrapper(entryLine1);
			var wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertNull("When invoices have same ValuationCode - Line 1 - '11', ExportNatureOfTransaction", wrapper1.ExportNatureOfTransaction);
			AssertNull("When invoices have same ValuationCode - Line 2 - '11', ExportNatureOfTransaction", wrapper2.ExportNatureOfTransaction);

			invoice2.JZ_ValuationCode = "12";
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals("When invoices have different ValuationCode - Line 1 - '11', ExportNatureOfTransaction", 11, wrapper1.ExportNatureOfTransaction);
			AssertEquals("When invoices have different ValuationCode - Line 2 - '12', ExportNatureOfTransaction", 12, wrapper2.ExportNatureOfTransaction);

			invoice2.JZ_ValuationCode = "";
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals("When invoices have different ValuationCode - Line 1 - '11', ExportNatureOfTransaction", 11, wrapper1.ExportNatureOfTransaction);
			AssertNull("When invoices have different ValuationCode - Line 2 - '', ExportNatureOfTransaction", wrapper2.ExportNatureOfTransaction);

			invoice1.JZ_ValuationCode = "";
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertNull("When invoices have same ValuationCode - Line 1 - '', ExportNatureOfTransaction", wrapper1.ExportNatureOfTransaction);
			AssertNull("When invoices have same ValuationCode - Line 2 - '', ExportNatureOfTransaction", wrapper2.ExportNatureOfTransaction);
		});
	}

	public void TestStatisticalValue()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.StatisticalValue), 0m, itemWrapper.StatisticalValue);

		entryLine.CL_StatisticalValue = 18.3;
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.StatisticalValue), 18.3m, itemWrapper.StatisticalValue);
	}

	public void TestAdditionOrDeductions()
	{
		var chargeOne = invoiceLine
			.Charges
			.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 20.21m, Constants.CurrencyCodes.EuropeanUnion);
		chargeOne.J7_IsDutiable = ZBool.True;
		chargeOne.J7_IsIncludedInITOT = ZBool.False;

		var chargeTwo = invoiceLine
			.ApportionedCharges
			.AddNew(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge, 41.90m, Constants.CurrencyCodes.EuropeanUnion);
		chargeTwo.J7_IsDutiable = ZBool.False;
		chargeTwo.J7_IsIncludedInITOT = ZBool.True;

		var chargeThree = invoiceLine
			.ApportionedCharges
			.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 55.99m, Constants.CurrencyCodes.EuropeanUnion);
		chargeThree.J7_IsDutiable = ZBool.True;
		chargeThree.J7_IsIncludedInITOT = ZBool.False;

		var chargeFour = invoiceLine
			.ApportionedCharges
			.AddNew(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 79.69m, Constants.CurrencyCodes.EuropeanUnion);
		chargeFour.J7_IsDutiable = ZBool.False;
		chargeFour.J7_IsIncludedInITOT = ZBool.True;

		var discountCharge = invoiceLine
			.ApportionedCharges
			.AddNew(UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge, 9.99m, Constants.CurrencyCodes.EuropeanUnion);
		discountCharge.J7_IsDutiable = ZBool.False;
		discountCharge.J7_IsIncludedInITOT = ZBool.False;

		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.AdditionOrDeductions), itemWrapper.AdditionOrDeductions);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.AdditionOrDeductions)} Count", 3, itemWrapper.AdditionOrDeductions.Count);

		AssertCollectionContains("Code AK", itemWrapper.AdditionOrDeductions, c => c.Code == AdditionDeductionCodeList.Codes.AK);
		AssertCollectionContains("Code AK with Amount 76.2",
			itemWrapper.AdditionOrDeductions,
			predicate: c => c.Code == AdditionDeductionCodeList.Codes.AK && c.Amount == 76.2m);

		AssertCollectionContains("Code BE", itemWrapper.AdditionOrDeductions, c => c.Code == AdditionDeductionCodeList.Codes.BE);
		AssertCollectionContains("Code BE with Amount 41.9",
			itemWrapper.AdditionOrDeductions,
			predicate: c => c.Code == AdditionDeductionCodeList.Codes.BE && c.Amount == chargeTwo.J7_Amount);

		AssertCollectionContains("Code BB", itemWrapper.AdditionOrDeductions, c => c.Code == AdditionDeductionCodeList.Codes.BB);
		AssertCollectionContains("Code BB with Amount 89.68",
			itemWrapper.AdditionOrDeductions,
			predicate: c => c.Code == AdditionDeductionCodeList.Codes.BB && c.Amount == 89.68m);
	}

	public void TestBaseAmounts()
	{
		var wrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts), wrapper.BaseAmounts);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts), Array.Empty<IBaseAmount>(), wrapper.BaseAmounts.ToArray());

		AddNewFee(entryLine.Fees, "A00", "", 0m);
		AddNewFee(entryLine.Fees, "A01", "F", 0m);
		AddNewFee(entryLine.Fees, "A02", "X", 0m);

		AddNewFee(entryLine.Fees, "A03", "A", 0m, 10m);
		AddNewFee(entryLine.Fees, "A04", "E", 0m, 11m);
		AddNewFee(entryLine.Fees, "A05", "G", 0m, 12m);
		AddNewFee(entryLine.Fees, "A06", "T", 0m, 13m);
		AddNewFee(entryLine.Fees, "A07", "R", 0m, 14m);

		wrapper = GetCusEntryLineWrapper();
		var baseAmounts = wrapper.BaseAmounts;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts)} count", 5, baseAmounts.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts), baseAmounts, wrapper.BaseAmounts);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts)} type should be {nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts)}", true, wrapper.BaseAmounts.All(x => x is BaseAmountWrapper));
		AssertArrayEqualsByElements("Charge Types", new[] { 10m, 11m, 12m, 13m, 14m }, baseAmounts.Select(x => x.Amount).ToArray());
	}

	public void TestBaseAmounts_WithDifferentActionTypes()
	{
		var fee1 = AddNewFee(entryLine.Fees, "A00", "A", 0m, 10m);
		var fee2 = AddNewFee(entryLine.Fees, "A01", "A", 0m, 20m);

		var wrapper = GetCusEntryLineWrapper();
		CombineAssertions("All Actions are empty", () =>
		{
			AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts)} count", 2, wrapper.BaseAmounts.Count);
			AssertArrayEqualsByElements("Base Amounts", new[] { 10m, 20m }, wrapper.BaseAmounts.Select(x => x.Amount).ToArray());
		});

		fee1.CF_RateOverrideReasonCode = "ADD";
		fee2.CF_RateOverrideReasonCode = "OVR";

		wrapper = GetCusEntryLineWrapper();
		CombineAssertions("Actions are ADD and OVR", () =>
		{
			AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts)} count", 2, wrapper.BaseAmounts.Count);
			AssertArrayEqualsByElements("Base Amounts", new[] { 10m, 20m }, wrapper.BaseAmounts.Select(x => x.Amount).ToArray());
		});

		fee1.CF_RateOverrideReasonCode = "EXC";

		wrapper = GetCusEntryLineWrapper();
		CombineAssertions("Actions contains EXC", () =>
		{
			AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.BaseAmounts)} count", 1, wrapper.BaseAmounts.Count);
			AssertArrayEqualsByElements("Base Amounts", new[] { 20m }, wrapper.BaseAmounts.Select(x => x.Amount).ToArray());
		});
	}

	public void TestExportPreviousDocuments()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertNotNull(nameof(ICusEntryLineCustomsMessageWrapper.ExportPreviousDocuments), itemWrapper.ExportPreviousDocuments);
		AssertArrayEqualsByElements(nameof(ICusEntryLineCustomsMessageWrapper.ExportPreviousDocuments), Array.Empty<IPreviousDocument>(), itemWrapper.ExportPreviousDocuments.ToArray());

		invoiceLine.PreviousDocuments.AddNew().CSI_Code = "IP1";
		invoiceLine.PreviousDocuments.AddNew().CSI_Code = "IP2";
		var invoiceLine2 = AddNewInvoiceLineToEntry();
		invoiceLine2.PreviousDocuments.AddNew().CSI_Code = "IP3";

		itemWrapper = GetCusEntryLineWrapper();
		var previousDocuments = itemWrapper.ExportPreviousDocuments;
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportPreviousDocuments)} count", 3, previousDocuments.Count);
		AssertSame(nameof(ICusEntryLineCustomsMessageWrapper.ExportPreviousDocuments), previousDocuments, itemWrapper.ExportPreviousDocuments);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportPreviousDocuments)} Type", true, itemWrapper.ExportPreviousDocuments.All(x => x is Export.PreviousDocumentWrapper));
	}

	public void TestExportAdditionalInformation()
	{
		var wrapper = GetCusEntryLineWrapper();
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportAdditionalInformation)} count", 0, wrapper.ExportAdditionalInformation.Count);

		invoiceHeader.AdditionalInfos.AddNew().CSI_SubType = "XYZ";
		var invoiceAdditionalInformation1 = invoiceHeader.AdditionalInfos.AddNew();
		invoiceAdditionalInformation1.CSI_SubType = "INF";
		invoiceAdditionalInformation1.CSI_Code = "00";
		invoiceAdditionalInformation1.CSI_Description = "INV. DESC.";

		invoiceLine.AdditionalInfos.AddNew().CSI_SubType = "XYZ";
		var invoiceLineAdditionalInformation1 = invoiceLine.AdditionalInfos.AddNew();
		invoiceLineAdditionalInformation1.CSI_SubType = "INF";
		invoiceLineAdditionalInformation1.CSI_Code = "01";
		invoiceLineAdditionalInformation1.CSI_Description = "INV. LINE. DESC.";
		var invoiceLineAdditionalInformation2 = invoiceLine.AdditionalInfos.AddNew();
		invoiceLineAdditionalInformation2.CSI_SubType = "INF";
		invoiceLineAdditionalInformation2.CSI_Code = "00";
		invoiceLineAdditionalInformation2.CSI_Description = "INV. DESC.";

		wrapper = GetCusEntryLineWrapper();
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportAdditionalInformation)} count", 2, wrapper.ExportAdditionalInformation.Count);
		CombineAssertions(() =>
		{
			AssertExportAdditionalInformation(wrapper.ExportAdditionalInformation.ElementAt(0), "00", "INV. DESC.");
			AssertExportAdditionalInformation(wrapper.ExportAdditionalInformation.ElementAt(1), "01", "INV. LINE. DESC.");
		});

		void AssertExportAdditionalInformation(IAdditionalInformation additionalInformation, string expectedCode, string expectedDescription)
		{
			AssertEquals("Code", expectedCode, additionalInformation.Code);
			AssertEquals("Description", expectedDescription, additionalInformation.Description);
		}
	}

	public void TestExportAdditionalReferences()
	{
		var wrapper = GetCusEntryLineWrapper();
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportAdditionalReferences)} count", 0, wrapper.ExportAdditionalReferences.Count);

		invoiceHeader.AdditionalInfos.AddNew().CSI_SubType = "XYZ";
		var invoiceAdditionalInformation1 = invoiceHeader.AdditionalInfos.AddNew();
		invoiceAdditionalInformation1.CSI_SubType = "REF";
		invoiceAdditionalInformation1.CSI_Code = "00";
		invoiceAdditionalInformation1.CSI_ReferenceNumber = "INV. DESC.";

		invoiceLine.AdditionalInfos.AddNew().CSI_SubType = "XYZ";
		var invoiceLineAdditionalInformation1 = invoiceLine.AdditionalInfos.AddNew();
		invoiceLineAdditionalInformation1.CSI_SubType = "REF";
		invoiceLineAdditionalInformation1.CSI_Code = "01";
		invoiceLineAdditionalInformation1.CSI_ReferenceNumber = "INV. LINE. DESC.";
		var invoiceLineAdditionalInformation2 = invoiceLine.AdditionalInfos.AddNew();
		invoiceLineAdditionalInformation2.CSI_SubType = "REF";
		invoiceLineAdditionalInformation2.CSI_Code = "00";
		invoiceLineAdditionalInformation2.CSI_ReferenceNumber = "INV. DESC.";

		wrapper = GetCusEntryLineWrapper();
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportAdditionalReferences)} count", 2, wrapper.ExportAdditionalReferences.Count);
		CombineAssertions(() =>
		{
			AssertExportAdditionalReference(wrapper.ExportAdditionalReferences.ElementAt(0), "00", "INV. DESC.");
			AssertExportAdditionalReference(wrapper.ExportAdditionalReferences.ElementAt(1), "01", "INV. LINE. DESC.");
		});

		void AssertExportAdditionalReference(IAdditionalReference additionalInformation, string expectedReferenceType, string expectedReferenceNumber)
		{
			AssertEquals("ReferenceType", expectedReferenceType, additionalInformation.ReferenceType);
			AssertEquals("ReferenceNumber", expectedReferenceNumber, additionalInformation.ReferenceNumber);
		}
	}

	public void TestExportAuthorizations()
	{
		var wrapper = GetCusEntryLineWrapper();
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportAuthorizations)} count", 0, wrapper.ExportAuthorizations.Count);

		invoiceLine.CusAuthorizationUsages.AddNew();
		invoiceLine.CusAuthorizationUsages.AddNew();

		wrapper = GetCusEntryLineWrapper();
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportAuthorizations)} count", 2, wrapper.ExportAuthorizations.Count);
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportAuthorizations)} Type", true, wrapper.ExportAuthorizations.All(x => x is Export.CustomsCodeAuthorizationWrapper));
	}

	public void TestExportTransportDocuments_IsTransitionPeriodAES30()
	{
		using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.MessageVersion = MessageVersionList.Codes.XML;
			AssertEquals("Precondition: IsTransitionPeriodAES30", true, declaration.IsTransitionPeriodAES30);

			var wrapper = GetCusEntryLineWrapper();
			AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportTransportDocuments)} count", 0, wrapper.ExportTransportDocuments.Count);

			invoiceHeader.AdditionalInfos.AddNew().CSI_SubType = "XYZ";
			var invoiceAdditionalInformation1 = invoiceHeader.AdditionalInfos.AddNew();
			invoiceAdditionalInformation1.CSI_SubType = "TRA";
			invoiceAdditionalInformation1.CSI_Code = "00";
			invoiceAdditionalInformation1.CSI_ReferenceNumber = "INV. DESC.";

			invoiceLine.AdditionalInfos.AddNew().CSI_SubType = "XYZ";
			var invoiceLineAdditionalInformation1 = invoiceLine.AdditionalInfos.AddNew();
			invoiceLineAdditionalInformation1.CSI_SubType = "TRA";
			invoiceLineAdditionalInformation1.CSI_Code = "01";
			invoiceLineAdditionalInformation1.CSI_ReferenceNumber = "INV. LINE. DESC.";
			var invoiceLineAdditionalInformation2 = invoiceLine.AdditionalInfos.AddNew();
			invoiceLineAdditionalInformation2.CSI_SubType = "TRA";
			invoiceLineAdditionalInformation2.CSI_Code = "00";
			invoiceLineAdditionalInformation2.CSI_ReferenceNumber = "INV. DESC.";

			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.ExportTransportDocuments)} count", 2, wrapper.ExportTransportDocuments.Count);
			CombineAssertions(() =>
			{
				AssertExportTransportDocument(wrapper.ExportTransportDocuments.ElementAt(0), "00", "INV. DESC.");
				AssertExportTransportDocument(wrapper.ExportTransportDocuments.ElementAt(1), "01", "INV. LINE. DESC.");
			});
		}

		void AssertExportTransportDocument(ITransportDocument additionalInformation, string expectedDocumentType, string expectedReferenceNumber)
		{
			AssertEquals("DocumentType", expectedDocumentType, additionalInformation.DocumentType);
			AssertEquals("ReferenceNumber", expectedReferenceNumber, additionalInformation.ReferenceNumber);
		}
	}

	public void TestExportTransportDocuments_IsNotTransitionPeriodAES30()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.MessageVersion = MessageVersionList.Codes.XML;
			AssertEquals("Precondition: IsTransitionPeriodAES30", false, declaration.IsTransitionPeriodAES30);

			var invoiceAdditionalInformation1 = invoiceHeader.AdditionalInfos.AddNew();
			invoiceAdditionalInformation1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			invoiceAdditionalInformation1.CSI_Code = "00";
			invoiceAdditionalInformation1.CSI_ReferenceNumber = "INV. DESC.";
			var wrapper = GetCusEntryLineWrapper();
			AssertEquals("Count", 0, wrapper.ExportTransportDocuments.Count);
		});
	}

	public void TestExportTransportChargesMethodOfPayment()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportTransportChargesMethodOfPayment), "", itemWrapper.ExportTransportChargesMethodOfPayment);

		invoiceHeader.ZG_TransportChargesMethodOfPayment = "A";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals("For only one Invoice Header, ExportTransportChargesMethodOfPayment", "", itemWrapper.ExportTransportChargesMethodOfPayment);

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		entryLine1.InvoiceLines.Add(invoiceLine1);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		entryLine2.InvoiceLines.Add(invoiceLine2);

		CombineAssertions(() =>
		{
			invoice1.ZG_TransportChargesMethodOfPayment = "A";
			invoice2.ZG_TransportChargesMethodOfPayment = "A";
			var wrapper1 = GetCusEntryLineWrapper(entryLine1);
			var wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals("For Same MOP for headers - Line 1, ExportTransportChargesMethodOfPayment", "", wrapper1.ExportTransportChargesMethodOfPayment);
			AssertEquals("For Same MOP for headers - Line 2, ExportTransportChargesMethodOfPayment", "", wrapper2.ExportTransportChargesMethodOfPayment);

			invoice2.ZG_TransportChargesMethodOfPayment = "B";
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals("For different MOP for headers - Line 1, ExportTransportChargesMethodOfPayment", "A", wrapper1.ExportTransportChargesMethodOfPayment);
			AssertEquals("For different MOP for headers - Line 2, ExportTransportChargesMethodOfPayment", "B", wrapper2.ExportTransportChargesMethodOfPayment);

			invoice2.ZG_TransportChargesMethodOfPayment = "";
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals("Line 1 MOP filled, ExportTransportChargesMethodOfPayment", "A", wrapper1.ExportTransportChargesMethodOfPayment);
			AssertEquals("Line 2 MOP empty, ExportTransportChargesMethodOfPayment", "", wrapper2.ExportTransportChargesMethodOfPayment);

			invoice1.ZG_TransportChargesMethodOfPayment = "";
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals("Empty MOP at headers, ExportTransportChargesMethodOfPayment", "", wrapper1.ExportTransportChargesMethodOfPayment);
			AssertEquals("Empty MOP at headers, ExportTransportChargesMethodOfPayment", "", wrapper2.ExportTransportChargesMethodOfPayment);
		});
	}

	public void TestExportCountryOfOrigin()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportCountryOfOrigin), "", itemWrapper.ExportCountryOfOrigin);

		invoiceLine.JI_CountryOfOrigin = "CN";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportCountryOfOrigin), "CN", itemWrapper.ExportCountryOfOrigin);
	}

	public void TestExportRegionOfDispatch()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportRegionOfDispatch), "", itemWrapper.ExportRegionOfDispatch);

		invoiceLine.JI_StateOrRegionOfOrigin = "US";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportRegionOfDispatch), "US", itemWrapper.ExportRegionOfDispatch);
	}

	public void TestExportHsTariffCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportHsTariffCode), "", itemWrapper.ExportHsTariffCode);

		invoiceLine.JI_FormattedTariff = "12345678";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportHsTariffCode), "123456", itemWrapper.ExportHsTariffCode);
	}

	public void TestExportNcTariffCode()
	{
		var itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportNcTariffCode), "", itemWrapper.ExportNcTariffCode);

		invoiceLine.JI_FormattedTariff = "12345678";
		itemWrapper = GetCusEntryLineWrapper();
		AssertEquals(nameof(ICusEntryLineCustomsMessageWrapper.ExportNcTariffCode), "78", itemWrapper.ExportNcTariffCode);
	}

	public void TestDangerousGoodsCodes()
	{
		var wrapper = GetCusEntryLineWrapper();
		AssertEquals($"{nameof(ICusEntryLineCustomsMessageWrapper.DangerousGoodsCodes)} count", 0, wrapper.DangerousGoodsCodes.Count);

		var dangerousSubstance1 = Factory.New<UNDGSubstance>();
		dangerousSubstance1.DG_Code = "32abc";
		var dangerousSubstance2 = Factory.New<UNDGSubstance>();
		dangerousSubstance2.DG_Code = "44CDE";

		invoiceLine.UNDGs.AddNew().DI_DG = dangerousSubstance1.PK;
		invoiceLine.UNDGs.AddNew().DI_DG = dangerousSubstance2.PK;
		invoiceLine.UNDGs.AddNew();
		wrapper = GetCusEntryLineWrapper();
		AssertContainsExactElementsInAnyOrder("Codes must be uppercase and 4 chars long", new[] { "32AB", "44CD" }, wrapper.DangerousGoodsCodes.ToArray());
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryLine.InvoiceLines.Reload(true);
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;

	ICusEntryLineCustomsMessageWrapper GetCusEntryLineWrapper() => new CusEntryLineCustomsMessageWrapper(entryLine);

	ICusEntryLineCustomsMessageWrapper GetCusEntryLineWrapper(CusEntryLine entryLine) => new CusEntryLineCustomsMessageWrapper(entryLine);

	JobComInvoiceLine AddNewInvoiceLineToEntry()
	{
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		entryLine.InvoiceLines.Reload(true);
		return invoiceLine2;
	}

	string GetTickedOrUnticked(bool boolValue) => boolValue ? "Ticked" : "Unticked";

	OrgHeader GetNewExporter()
	{
		var exporter = Factory.New<OrgHeader>();
		var exporterAddress = exporter.MainAddress;
		exporterAddress.CompanyName = "EXPORTER COMPANY NAME";
		exporterAddress.Address1 = "EXPORTER ADDRESS 1";
		exporterAddress.Address2 = "EXPORTER ADDRESS 2";
		exporterAddress.Postcode = "99999";
		exporterAddress.City = "EXPORTER CITY";
		exporterAddress.OA_RN_NKCountryCode = "IT";
		return exporter;
	}

	void SetUpFees(CusEntryLineFeeCollection lineFeeCollection, params ZString[] rateCodesToAdd)
	{
		foreach (var rateCode in rateCodesToAdd)
		{
			lineFeeCollection.AddOrUpdate(rateCode, 0m);
		}
	}

	CusEntryLineFee AddNewFee(CusEntryLineFeeCollection lineFeeCollection, string chargeType, string methodOfPayment, decimal chargeAmount, decimal baseValue = 0)
	{
		var lineFee = lineFeeCollection.AddNew();
		lineFee.CF_ChargeType = chargeType;
		lineFee.CF_MethodOfPayment = methodOfPayment;
		lineFee.CF_ChargeAmount = chargeAmount;
		lineFee.CF_BaseValue = baseValue;
		return lineFee;
	}

	void AssertExportWrapperFieldWithSingleLine(string fieldName, Action<JobComInvoiceLine, string> setLineFieldTo, Action<JobDeclaration, string> setDeclarationTo, Func<ICusEntryLineCustomsMessageWrapper, string> getWrapperFieldValue)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		CombineAssertions(() =>
		{
			setDeclarationTo(declaration, "FI");
			setLineFieldTo(invoiceLine, string.Empty);

			var wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When {fieldName} is only provided at declaration level and not at invoice line, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine, "FI");
			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When invoice line and Declaration have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine, "US");
			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When invoice line has {fieldName} different from declaration, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setDeclarationTo(declaration, string.Empty);
			setLineFieldTo(invoiceLine, "US");
			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When invoice line has {fieldName} and declaration is empty, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine, string.Empty);
			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When both line and declaration don't have {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));
		});
	}

	void AssertExportWrapperTraderWithSingleLine(string fieldName, Action<JobComInvoiceLine, ZGuid> setLineFieldTo, Action<JobDeclaration, ZGuid> setDeclarationTo, Func<ICusEntryLineCustomsMessageWrapper, IEoriTrader> getWrapperFieldValue)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		CombineAssertions(() =>
		{
			var declarationOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declarationOrgHeader.CustomsCodes.AddNew("EOR", "135040449", "IT");

			var orgAddress1 = declarationOrgHeader.Addresses.AddNew();
			declaration.SupplierDocumentaryAddress.OrganisationPK = declarationOrgHeader.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = declarationOrgHeader.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress1.PK;

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew("EOR", "999040449", "IT");

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine, ZGuid.Empty);

			var wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When {fieldName} is only provided at declaration level and not at invoice line, {fieldName}", null, getWrapperFieldValue(wrapper));

			var lineAddress = Factory.New<OrgAddress>();
			lineAddress.OA_OH = orgHeader2.PK;

			setLineFieldTo(invoiceLine, orgAddress1.PK);
			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When invoice line and Declaration have same {fieldName}, {fieldName}", null, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine, lineAddress.PK);
			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When invoice line has {fieldName} different from declaration, {fieldName}", null, getWrapperFieldValue(wrapper));

			setDeclarationTo(declaration, ZGuid.Empty);
			setLineFieldTo(invoiceLine, orgAddress1.PK);
			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When invoice line has {fieldName} and declaration is empty, {fieldName}", null, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine, ZGuid.Empty);
			wrapper = GetCusEntryLineWrapper();
			AssertEquals($"When both line and declaration don't have {fieldName}, {fieldName}", null, getWrapperFieldValue(wrapper));
		});
	}

	void AssertExportWrapperField(string fieldName, Action<JobComInvoiceLine, string> setLineFieldTo, Action<JobDeclaration, string> setDeclarationTo, Func<ICusEntryLineCustomsMessageWrapper, string> getWrapperFieldValue)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		CombineAssertions($"{fieldName} only in invoice lines", () =>
		{
			setDeclarationTo(declaration, string.Empty);
			setLineFieldTo(invoiceLine1, "IT");
			setLineFieldTo(invoiceLine2, string.Empty);

			var wrapper1 = GetCusEntryLineWrapper(entryLine1);
			var wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is only available in one line, in line 1", "IT", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is only available in one line, in line 2", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(invoiceLine1, "US");
			setLineFieldTo(invoiceLine2, "US");
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When all invoice lines have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When all invoice lines have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(invoiceLine2, "BS");
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When invoices lines have different {fieldName}, {fieldName}", "US", getWrapperFieldValue(wrapper1));
			AssertEquals($"When invoices lines have different {fieldName}, {fieldName}", "BS", getWrapperFieldValue(wrapper2));

			setLineFieldTo(invoiceLine2, string.Empty);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When invoices lines have different {fieldName} (one does not have value), {fieldName}", "US", getWrapperFieldValue(wrapper1));
			AssertEquals($"When invoices lines have different {fieldName} (one does not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(invoiceLine1, string.Empty);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When invoices lines have same {fieldName} (all do not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When invoices lines have same {fieldName} (all do not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));
		});

		CombineAssertions($"{fieldName} both in invoice lines and declaration", () =>
		{
			setDeclarationTo(declaration, "US");
			setLineFieldTo(invoiceLine1, "US");
			setLineFieldTo(invoiceLine2, "US");
			var wrapper1 = GetCusEntryLineWrapper(entryLine1);
			var wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is the same in lines and declaration, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is the same in lines and declaration, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setDeclarationTo(declaration, "US");
			setLineFieldTo(invoiceLine1, "IT");
			setLineFieldTo(invoiceLine2, "IS");
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is present in lines and declaration and different in all cases, {fieldName}", "IT", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is present in lines and declaration and different in all cases, {fieldName}", "IS", getWrapperFieldValue(wrapper2));

			setDeclarationTo(declaration, "US");
			setLineFieldTo(invoiceLine1, "IT");
			setLineFieldTo(invoiceLine2, string.Empty);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", "IT", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", "US", getWrapperFieldValue(wrapper2));

			setDeclarationTo(declaration, "US");
			setLineFieldTo(invoiceLine1, string.Empty);
			setLineFieldTo(invoiceLine2, string.Empty);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is only available at declaration level, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is only available at declaration level, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));
		});
	}

	void AssertExportWrapperTrader(string fieldName, Action<JobComInvoiceLine, ZGuid> setLineFieldTo, Action<JobDeclaration, ZGuid> setDeclarationTo, Func<ICusEntryLineCustomsMessageWrapper, IEoriTrader> getWrapperFieldValue)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		var orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.CustomsCodes.AddNew("EOR", "222222222", "IT");

		var orgHeader3 = Factory.New<OrgHeader>();
		orgHeader3.CustomsCodes.AddNew("EOR", "333333333", "IT");

		var orgAddress2 = Factory.New<OrgAddress>();
		orgAddress2.OA_OH = orgHeader2.PK;

		var orgAddress3 = Factory.New<OrgAddress>();
		orgAddress3.OA_OH = orgHeader3.PK;

		CombineAssertions($"{fieldName} only in invoice lines", () =>
		{
			setDeclarationTo(declaration, ZGuid.Empty);
			setLineFieldTo(invoiceLine1, orgAddress2.PK);
			setLineFieldTo(invoiceLine2, ZGuid.Empty);

			var wrapper1 = GetCusEntryLineWrapper(entryLine1);
			var wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is only available in one line, in line 1", "IT222222222", getWrapperFieldValue(wrapper1).EoriNumber);
			AssertEquals($"When {fieldName} is only available in one line, in line 2", null, getWrapperFieldValue(wrapper2));

			setLineFieldTo(invoiceLine1, orgAddress2.PK);
			setLineFieldTo(invoiceLine2, orgAddress2.PK);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When all invoice lines have same {fieldName}, {fieldName}", null, getWrapperFieldValue(wrapper1));
			AssertEquals($"When all invoice lines have same {fieldName}, {fieldName}", null, getWrapperFieldValue(wrapper2));

			setLineFieldTo(invoiceLine2, orgAddress3.PK);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When invoices lines have different {fieldName}, {fieldName}", "IT222222222", getWrapperFieldValue(wrapper1).EoriNumber);
			AssertEquals($"When invoices lines have different {fieldName}, {fieldName}", "IT333333333", getWrapperFieldValue(wrapper2).EoriNumber);

			setLineFieldTo(invoiceLine2, ZGuid.Empty);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When invoices lines have different {fieldName} (one does not have value), {fieldName}", "IT222222222", getWrapperFieldValue(wrapper1).EoriNumber);
			AssertEquals($"When invoices lines have different {fieldName} (one does not have value), {fieldName}", null, getWrapperFieldValue(wrapper2));

			setLineFieldTo(invoiceLine1, ZGuid.Empty);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When invoices lines have same {fieldName} (all do not have value), {fieldName}", null, getWrapperFieldValue(wrapper1));
			AssertEquals($"When invoices lines have same {fieldName} (all do not have value), {fieldName}", null, getWrapperFieldValue(wrapper2));

			var orgHeader4 = Factory.New<OrgHeader>();
			var eoriCode4 = orgHeader4.CustomsCodes.AddNew("EOR", "444444444", "IT");

			var orgAddress4_1 = Factory.New<OrgAddress>();
			orgAddress4_1.OA_OH = orgHeader4.PK;
			orgAddress4_1.City = "City1";

			var orgAddress4_2 = Factory.New<OrgAddress>();
			orgAddress4_2.OA_OH = orgHeader4.PK;
			orgAddress4_2.City = "City2";

			setLineFieldTo(invoiceLine1, orgAddress4_1.PK);
			setLineFieldTo(invoiceLine2, orgAddress4_2.PK);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When invoices lines have same {fieldName} but different address, {fieldName}", null, getWrapperFieldValue(wrapper1));
			AssertEquals($"When invoices lines have same {fieldName} but different address, {fieldName}", null, getWrapperFieldValue(wrapper2));

			orgHeader4.CustomsCodes.Remove(eoriCode4);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When invoices lines have same {fieldName} with no Eori code and different address, {fieldName}", "City1", getWrapperFieldValue(wrapper1).Address.City);
			AssertEquals($"When invoices lines have same {fieldName} with no Eori code and different address, {fieldName}", "City2", getWrapperFieldValue(wrapper2).Address.City);
		});

		CombineAssertions($"{fieldName} both in invoice lines and declaration", () =>
		{
			var headerOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			headerOrgHeader.CustomsCodes.AddNew("EOR", "111111111", "IT");

			var orgAddress1 = headerOrgHeader.Addresses.AddNew();
			declaration.SupplierDocumentaryAddress.OrganisationPK = headerOrgHeader.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress1.PK;

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine1, orgAddress1.PK);
			setLineFieldTo(invoiceLine2, orgAddress1.PK);
			var wrapper1 = GetCusEntryLineWrapper(entryLine1);
			var wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is the same in lines and declaration, {fieldName}", null, getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is the same in lines and declaration, {fieldName}", null, getWrapperFieldValue(wrapper2));

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine1, orgAddress2.PK);
			setLineFieldTo(invoiceLine2, orgAddress3.PK);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is present in lines and declaration and different in all cases, {fieldName}", "IT222222222", getWrapperFieldValue(wrapper1).EoriNumber);
			AssertEquals($"When {fieldName} is present in lines and declaration and different in all cases, {fieldName}", "IT333333333", getWrapperFieldValue(wrapper2).EoriNumber);

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine1, orgAddress2.PK);
			setLineFieldTo(invoiceLine2, ZGuid.Empty);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", "IT222222222", getWrapperFieldValue(wrapper1).EoriNumber);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", "IT111111111", getWrapperFieldValue(wrapper2).EoriNumber);

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine1, ZGuid.Empty);
			setLineFieldTo(invoiceLine2, ZGuid.Empty);
			wrapper1 = GetCusEntryLineWrapper(entryLine1);
			wrapper2 = GetCusEntryLineWrapper(entryLine2);
			AssertEquals($"When {fieldName} is only available at declaration level, {fieldName}", null, getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is only available at declaration level, {fieldName}", null, getWrapperFieldValue(wrapper2));
		});
	}
}
