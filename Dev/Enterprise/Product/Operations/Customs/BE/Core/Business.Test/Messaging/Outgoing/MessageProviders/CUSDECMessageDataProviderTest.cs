using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BE.Business.Testing;

class CUSDECMessageDataProviderTest : TestCaseWithFactory
{
	public void TestType()
	{
		Assert(new CUSDECMessageDataProvider(CreateFirstEntryNoMerge(new[] { declaration.InvoiceLines[0] })) is ICUSDECMessageDataProvider);
	}

	public void TestEmptyEntry()
	{
		AssertExceptionThrown<Exception>(() => new CUSDECMessageDataProvider(CreateFirstEntryNoMerge(Array.Empty<JobComInvoiceLine>())));
	}

	public void TestSingleInvoiceLine()
	{
		var cusCode = declaration.Importer.CustomsCodes.AddNew();
		cusCode.OK_CodeType = "EOR";
		cusCode.OK_RN_NKCodeCountry = CountryCodes.Belgium;
		var invoiceLine = declaration.InvoiceLines[new Random().Next(0, declaration.InvoiceLines.Count)];
		var entry = CreateFirstEntryNoMerge(new[] { invoiceLine });
		var provider = new CUSDECMessageDataProvider(entry);
		AssertEquals("Language Code", GlbStaff.CurrentUser.Language, provider.LanguageCode);

		AssertGoodsDeclartion(provider.GoodsDeclaration, entry);

		AssertGoodsItem(provider.GoodsItem[0] as GoodsItem, invoiceLine);
	}

	public void TestDeclarant()
	{
		var invoiceLine = declaration.InvoiceLines[new Random().Next(0, declaration.InvoiceLines.Count)];
		var entry = CreateFirstEntryNoMerge(new[] { invoiceLine });
		var cusCodes = Factory.Load<OrgCusCode>(new ZQuery(new ZQuery(OrgCusCodeSchema.OK_CodeType, "EOR"), new ZQuery(OrgCusCodeSchema.OK_OH, GlbCompany.CurrentCompany.OrgProxy.PK)));
		foreach (var cusCode in cusCodes)
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Belgium;
			cusCode.OK_CustomsRegNo = "100";
		}
		AssertEquals(CountryCodes.Belgium, new CUSDECMessageDataProvider(entry).MessageSender.Country);
		AssertEquals("005", new CUSDECMessageDataProvider(entry).MessageSender.Identifier);
		AssertEquals("100", new CUSDECMessageDataProvider(entry).MessageSender.OperatorIdentity);

		declaration.JE_DeclarantType = "SEL";
		AssertEquals("declarantStatus", "1", new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.DeclarantStatus);
		declaration.JE_DeclarantType = "DIR";
		AssertEquals("declarantStatus", "2", new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.DeclarantStatus);
		declaration.JE_DeclarantType = "IND";
		AssertEquals("declarantStatus", "3", new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.DeclarantStatus);

		AssertEquals("operatorName", GlbCompany.CurrentCompany.OrgProxy.OH_FullName, new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.OperatorName);
		AssertEquals("postalCode", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_PostCode, new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.OperatorAddress.PostalCode);
		AssertEquals("streetandNumber1", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address1, new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.OperatorAddress.StreetAndNumber1);
		AssertEquals("streetandNumber2", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address2, new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.OperatorAddress.StreetAndNumber2);
		AssertEquals("city",
			Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_RL_NKRelatedPortCode).RL_PortName,
			new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.OperatorAddress.City);
		AssertEquals("country", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_RL_NKRelatedPortCode.Left(2), new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.OperatorAddress.Country);

		AssertEquals("contactPersonName", GlbStaff.CurrentUser.GS_FullName, new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.ContactPerson.ContactPersonName);
		AssertEquals("contactPersonCommunicationNumber", GlbStaff.CurrentUser.GS_WorkPhone, new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.ContactPerson.ContactPersonCommunicationNumber);
		AssertEquals("contactPersonEmail", GlbStaff.CurrentUser.GS_EmailAddress, new CUSDECMessageDataProvider(entry).GoodsDeclaration.Declarant.ContactPerson.ContactPersonEmail);
	}

	void AssertGoodsDeclartion(ITGoodsDeclarationImport goodsDeclaration, CusEntryHeader entry)
	{
		var invoice = entry.InvoiceHeaders[0];
		invoice.InvoiceLines.MarkAsNeedingValidation();

		entry.CH_BGMReference = "localReferenceNumber";
		AssertEquals("localReferenceNumber", goodsDeclaration.LocalReferenceNumber);
		entry.Declaration.JE_DeclarationReference = "commercialReference";
		AssertEquals("commercialReference", goodsDeclaration.CommercialReference);
		AssertEquals("items", 1m, goodsDeclaration.Totals.Items);

		#region Totals
		ZDecimal totalGrossMass = 0;
		ZDecimal totalNetMass = 0;
		ZDecimal packages = 0;
		Random r = new Random();
		foreach (var line in entry.MergedLines)
		{
			foreach (var ilBizo in line.InvoiceLines)
			{
				JobComInvoiceLine invoiceLine = (JobComInvoiceLine)ilBizo;

				// NetWeight has to be set first, as it overrides CustomsQuantity. This might be a problem
				invoiceLine.JI_NetWeight = r.Next(1, 100);
				invoiceLine.JI_NetWeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
				totalNetMass += invoiceLine.JI_NetWeight;

				invoiceLine.JI_CustomsQuantity = r.Next(1, 100);
				invoiceLine.JI_CustomsUnitQty = Enterprise.Core.Constants.Weight.Kilograms;
				totalGrossMass += invoiceLine.JI_CustomsQuantity;

				var pivot = invoiceLine.PackagesPivot.AddNew();
				var package = declaration.Packages.AddNew();
				pivot.CHC_CW = package.PK;
				package.CW_PackQty = r.Next(1, 100);
				packages += package.CW_PackQty;
			}
		}
		AssertEquals("totalGrossmass", totalGrossMass, goodsDeclaration.Totals.TotalGrossmass);
		AssertEquals("totalNetmass", totalNetMass, goodsDeclaration.Totals.TotalNetmass);
		AssertEquals("packages", packages, goodsDeclaration.Totals.Packages);
		#endregion

		foreach (var inv in entry.InvoiceHeaders)
		{
			inv.JZ_ValuationCode = "LR";
		}
		AssertEquals("TransactionNature1", "L", goodsDeclaration.TransactionNature.TransactionNature1);
		AssertEquals("TransactionNature2", "R", goodsDeclaration.TransactionNature.TransactionNature2);

		AssertStartsWith("IssuePlace", GlbCompany.CurrentCompany.OrgProxy.MainAddress.Postcode, goodsDeclaration.IssuePlace);
		AssertEndsWith("IssuePlace", GlbCompany.CurrentCompany.OrgProxy.MainAddress.City, goodsDeclaration.IssuePlace);

		declaration.JE_EntryStyle = "1";
		entry.EntryInstruction.CEI_SubStyle = "2";
		AssertEquals("typePartOne", "1", goodsDeclaration.TypePartOne);
		AssertEquals("typePartTwo", "2", goodsDeclaration.TypePartTwo);

		declaration.JE_PaymentMethod = "A";
		declaration.JE_DefermentAccountNumber = "0000088";
		AssertEquals("paymentMethodTaxes", "A", goodsDeclaration.PaymentTaxes.PaymentMethodTaxes);
		AssertEquals("deferredPayment", "0000088", goodsDeclaration.PaymentTaxes.DeferredPayment);
		AssertEquals("deferredPaymentAccountHolder", declaration.JE_DefermentAccountNumber, goodsDeclaration.PaymentTaxes.DeferredPayment);
		AssertEquals("deferredPaymentAccountHolder", goodsDeclaration.Declarant.Country + goodsDeclaration.Declarant.OperatorIdentity, goodsDeclaration.PaymentTaxes.DeferredPaymentAccountHolder);
		declaration.JE_PaymentMethod = "X";
		AssertEquals("deferredPaymentAccountHolder", goodsDeclaration.Consignee.Country + goodsDeclaration.Consignee.OperatorIdentity, goodsDeclaration.PaymentTaxes.DeferredPaymentAccountHolder);

		declaration.ZG_VATDeferType = "Z";
		declaration.ZG_VATDeferNumber = "3334";
		AssertEquals("deferredPaymentVat", "Z", goodsDeclaration.PaymentVat.DeferredPaymentVat);
		AssertEquals("paymentMethodVat", "3334", goodsDeclaration.PaymentVat.PaymentMethodVat);

		declaration.JE_CustomsOffice = "CO";
		AssertEquals("GoodsLocation - precise", "CO", goodsDeclaration.Customs.GoodsLocation.Precise);
		AssertEquals("GoodsLocation - precise", "CO", goodsDeclaration.Customs.ValidationOffice);
		declaration.JE_LocationOfGoods = "Real Location";
		AssertEquals("GoodsLocation - precise", "Real Location", goodsDeclaration.Customs.GoodsLocation.Precise);

		invoice.JZ_IncoTerm = IncoTermRegistry.Keys[new Random().Next(0, IncoTermRegistry.Keys.Length - 1)];
		AssertEquals("deliveryTerms", invoice.JZ_IncoTerm, goodsDeclaration.TransportMeans.DeliveryTerms.DeliveryTerms);
		invoice.JZ_IncoTermPlace = "Hell";
		AssertEquals("deliveryTermsPlace", "Hell", goodsDeclaration.TransportMeans.DeliveryTerms.DeliveryTermsPlace);
		declaration.ZG_AgreedPlaceCode = "D";

		declaration.JE_TransportMode = "AIR";
		AssertEquals("borderMode", "4", goodsDeclaration.TransportMeans.BorderMode);
		declaration.ZG_Box18TransportNationality = "OZ";
		AssertEquals("BorderNationality", "OZ", goodsDeclaration.TransportMeans.BorderNationality);
		declaration.JE_TransportModeInland = "SEA";
		AssertEquals("inlandMode", "1", goodsDeclaration.TransportMeans.InlandMode);
		declaration.JE_GoodsOrigin = "CA";
		AssertEquals("dispatchCountry", "CA", goodsDeclaration.TransportMeans.DispatchCountry);
		declaration.ZG_Box18TransportID = "ageta";
		AssertEquals("departureIdentity", "ageta", goodsDeclaration.TransportMeans.DepartureIdentity);
	}

	void AssertGoodsItem(GoodsItem goodsItem, JobComInvoiceLine invoiceLine)
	{
		invoiceLine.CusEntryLine.CL_LineNumber = 7;
		AssertEquals("Sequence", 7m, goodsItem.Sequence);
		invoiceLine.JI_Tariff = "06";
		AssertEquals("CommodityCode", "06", goodsItem.CommodityCode);

		invoiceLine.JI_NetWeight = 0.5;
		invoiceLine.JI_NetWeightUQ = Enterprise.Core.Constants.Weight.Tonnes;
		AssertEquals("NetMass", 500m, goodsItem.NetMass);
		invoiceLine.JI_Weight = 5000;
		invoiceLine.JI_WeightUQ = Enterprise.Core.Constants.Weight.Grams;
		AssertEquals("GrossMass", 5m, goodsItem.GrossMass);
		invoiceLine.JI_Description = "whatever";
		AssertEquals("GoodsDescription", "whatever", goodsItem.GoodsDescription);

		#region Packaging
		invoiceLine.PackagesPivot.RemoveAll();
		var pivot = invoiceLine.PackagesPivot.AddNew();
		var package = declaration.Packages.AddNew();
		pivot.CHC_CW = package.PK;
		pivot.CHC_NumberOfPacks = 43;
		package.CW_MarksAndNos = "abc123";
		package.CW_PackType = "H";
		AssertEquals("Packages", 43m, goodsItem.Packaging[0].Packages);
		AssertEquals("MarksNumber", "abc123", goodsItem.Packaging[0].MarksNumber);
		AssertEquals("PackageType", "H", goodsItem.Packaging[0].PackageType);
		#endregion

		invoiceLine.PackagesPivot.RemoveAll();
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "535";
		invoiceLine.ContainersPivot.AddNew().C2_CO = container.PK;
		container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "00584";
		invoiceLine.ContainersPivot.AddNew().C2_CO = container.PK;
		AssertContainsExactElementsInAnyOrder("containerIdentifier", new[] { "535", "00584" }, goodsItem.ContainerIdentifier);

		#region ProducedDocument
		var producedDoc = invoiceLine.SupportingDocuments.AddNew();
		producedDoc.CSI_Type = "SUP";
		producedDoc.CSI_ReferenceNumber = "ref200";
		producedDoc.CSI_Code = "SUP";
		producedDoc.CSI_CustomsOffice = "CO2";
		producedDoc.CSI_DateOfIssue = new ZDateTime("01/01/2000");
		producedDoc.CSI_AdditionalDescription = "Sup doc";
		producedDoc.CSI_UnitOfQuantity = "KG";
		producedDoc.CSI_Quantity = 17;
		AssertEquals("documentReference", "ref200", goodsItem.ProducedDocument[0].DocumentReference);
		AssertEquals("DocumentType", "SUP", goodsItem.ProducedDocument[0].DocumentType);
		AssertEquals("ProducedDocumentsValidationOffice", "CO2", goodsItem.ProducedDocument[0].ProducedDocumentsValidationOffice);
		AssertEquals("ProducedDocumentsInformationDate", new ZDateTime("01/01/2000"), goodsItem.ProducedDocument[0].ProducedDocumentsInformationDate);
		AssertEquals("ComplementaryInformation", "Sup doc", goodsItem.ProducedDocument[0].ComplementaryInformation);
		AssertEquals("DocumentType", 17m, goodsItem.ProducedDocument[0].DocumentQuantity.Quantity);
		AssertEquals("DocumentType", "KG", goodsItem.ProducedDocument[0].DocumentQuantity.QuantityCode);
		invoiceLine.SupportingDocuments.AddNew().CSI_ReferenceNumber = "ref300";
		AssertEquals("documentReference", "ref300", goodsItem.ProducedDocument[1].DocumentReference);
		#endregion

		invoiceLine.ZG_CountryOfDestination = CountryCodes.Afghanistan;
		AssertEquals("DestinationCountry", CountryCodes.Afghanistan, goodsItem.DestinationCountry);

		invoiceLine.InvoiceHeader.JZ_IncoTerm = IncoTermRegistry.Keys[new Random().Next(0, IncoTermRegistry.Keys.Length - 1)];
		AssertEquals("deliveryTerms", invoiceLine.InvoiceHeader.JZ_IncoTerm, goodsItem.DeliveryTerms.DeliveryTerms);
		invoiceLine.InvoiceHeader.JZ_IncoTermPlace = "Alexandria";
		AssertEquals("deliveryTermsPlace", "Alexandria", goodsItem.DeliveryTerms.DeliveryTermsPlace);

		invoiceLine.JI_CustomsSecondQuantity = 89;
		invoiceLine.JI_CustomsSecondUnitQty = "KM";
		AssertEquals("SupplementaryUnits", 89m, goodsItem.SupplementaryUnits.SupplementaryUnits);
		AssertEquals("SupplementaryUnitsCode", "KM", goodsItem.SupplementaryUnits.SupplementaryUnitsCode);

		invoiceLine.InvoiceHeader.JZ_ValuationCode = "FS";
		AssertEquals("TransactionNature1", "F", goodsItem.TransactionNature.TransactionNature1);
		AssertEquals("TransactionNature2", "S", goodsItem.TransactionNature.TransactionNature2);

		invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Zimbabwe;
		AssertEquals("OriginCountry", Enterprise.Core.Constants.CountryCodes.Zimbabwe, goodsItem.OriginCountry);

		invoiceLine.JI_ConcessionOrder = "q1";
		AssertEquals("Quota", "q1", goodsItem.CustomsTreatment.Quota);
		invoiceLine.JI_ValuationCode = "V";
		AssertEquals("ValuationMethod", "V", goodsItem.CustomsTreatment.ValuationMethod);
		invoiceLine.JI_PrimaryPreference = "123";
		AssertEquals("Preference1", "1", goodsItem.CustomsTreatment.Preference.Preference1);
		AssertEquals("Preference2", "23", goodsItem.CustomsTreatment.Preference.Preference2);
		invoiceLine.JI_Procedure = "procedu";
		AssertEquals("ProcedurePart1", "pr", goodsItem.CustomsTreatment.Procedure.ProcedurePart1);
		AssertEquals("ProcedurePart2", "oc", goodsItem.CustomsTreatment.Procedure.ProcedurePart2);
		AssertContainsExactElementsInAnyOrder("NationalProcedureCode", new[] { "edu" }, goodsItem.CustomsTreatment.Procedure.NationalProcedureCode);
		declaration.CustomsEntryInstructions[0].CEI_Style = "CS";
		AssertEquals("ProcedureType", "CS", goodsItem.CustomsTreatment.Procedure.ProcedureType);

		#region CalculationUnits
		invoiceLine.JI_CustomsSecondQuantity = 101;
		invoiceLine.JI_CustomsSecondUnitQty = "M";
		invoiceLine.JI_CustomsThirdQuantity = 201;
		invoiceLine.JI_CustomsThirdUnitQty = "IN";
		AssertEquals("CalculationUnits", 101m, goodsItem.CustomsTreatment.CalculationUnits[0].CalculationUnits);
		AssertEquals("CalculationCode", "M", goodsItem.CustomsTreatment.CalculationUnits[0].CalculationCode);
		AssertEquals("CalculationUnits", 201m, goodsItem.CustomsTreatment.CalculationUnits[1].CalculationUnits);
		AssertEquals("CalculationCode", "IN", goodsItem.CustomsTreatment.CalculationUnits[1].CalculationCode);
		#endregion

		AssertEquals("CalculationCode", invoiceLine.JI_LinePrice = 99m, goodsItem.Price.Amount);
		AssertEquals("ExchangeRate", invoiceLine.InvoiceHeader.JZ_InvoiceCurrExRate = 67m, goodsItem.Price.ExchangeRate.ExchangeRate);
		AssertEquals("Currency", invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.Denmark, goodsItem.Price.ExchangeRate.Currency);

		#region PreviousDocument
		var previousdDoc = invoiceLine.PreviousDocuments.AddNew();
		AssertEquals("documentReference", previousdDoc.CSI_ReferenceNumber = "ref500", goodsItem.PreviousDocument.DocumentReference);
		AssertEquals("DocumentType", previousdDoc.CSI_Code = "PRE", goodsItem.PreviousDocument.DocumentType);
		AssertEquals("PreviousDocumentCategory", previousdDoc.CSI_SubType = "T", goodsItem.PreviousDocument.PreviousDocumentCategory);
		AssertEquals("PreviousDocumentDate", previousdDoc.CSI_DateOfIssue = new ZDateTime("01/01/3000"), goodsItem.PreviousDocument.PreviousDocumentDate);
		AssertEquals("PreviousDocumentDateSpecified", true, goodsItem.PreviousDocument.PreviousDocumentDateSpecified);
		AssertEquals("PreviousDocumentItem", previousdDoc.CSI_LineNo = 128, int.Parse(goodsItem.PreviousDocument.PreviousDocumentItem));
		AssertEquals("PreviousDocumentLoc", previousdDoc.CSI_CustomsOffice = "CO9", goodsItem.PreviousDocument.PreviousDocumentLoc);
		AssertEquals("PreviousDocumentBillOfLoading", previousdDoc.CSI_ReferenceNumber2 = "B/L", goodsItem.PreviousDocument.PreviousDocumentBillOfLoading);
		#endregion
	}

	JobDeclaration declaration;
	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.Bills.AddNew();

		var declarant = Factory.New<OrgCusCode>();
		declarant.OK_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		declarant.OK_CodeType = "EOR";

		declaration.JE_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true), (new ZQuery(OrgHeaderSchema.OH_IsConsignee, false)))).PK;
		var consignor = Factory.New<OrgCusCode>();
		consignor.OK_OH = declaration.JE_OH_Supplier;
		consignor.OK_CodeType = "EOR";

		declaration.JE_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true), new ZQuery(OrgHeaderSchema.OH_IsConsignor, false))).PK;
		var consignee = Factory.New<OrgCusCode>();
		consignee.OK_OH = declaration.JE_OH_Importer;
		consignee.OK_CodeType = "EOR";

		for (int i = 0; i < 3; i++)
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			declaration.Invoices.Add(invoice);
			for (int j = 0; j < 3; j++)
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				invoiceLine.JI_JZ = invoice.PK;
				invoice.InvoiceLines.Add(invoiceLine);
				declaration.InvoiceLines.Add(invoiceLine);
			}
		}
	}

	CusEntryHeader CreateFirstEntryNoMerge(IEnumerable<JobComInvoiceLine> invoiceLines)
	{
		var entry = Factory.New<CusEntryHeader>();
		entry.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;
		foreach (var invoiceLine in invoiceLines)
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.InvoiceLines.Add(invoiceLine);
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = declaration.CustomsEntryInstructions[0].PK;
			entry.MergedLines.Add(entryLine);
		}
		declaration.CustomsEntryHeaders.Add(entry);
		return entry;
	}

	public void TestConsignor()
	{
		var invoiceLine = declaration.InvoiceLines[new Random().Next(0, declaration.InvoiceLines.Count)];
		var entry = CreateFirstEntryNoMerge(new[] { invoiceLine });
		var cusCode = declaration.Supplier.CustomsCodes.AddNew();
		cusCode.OK_CodeType = "EOR";
		cusCode.OK_RN_NKCodeCountry = CountryCodes.Belgium;
		cusCode.OK_CustomsRegNo = "SUPPLIER100";
		var op = new CUSDECMessageDataProvider(entry).GoodsDeclaration.Consignor;
		AssertEquals("country", CountryCodes.Belgium, op.Country);
		AssertEquals("Identifier", "005", op.Identifier);
		AssertEquals("OperatorIdentity", "SUPPLIER100", op.OperatorIdentity);

		AssertEquals("operatorName", declaration.Supplier.OH_FullName, op.OperatorName);
		var address = Factory.LoadTop1<JobDocAddress>(new ZQuery(new ZQuery(JobDocAddressSchema.E2_AddressType, "SUD"), new ZQuery(JobDocAddressSchema.E2_ParentID, declaration.PK)));
		AssertEquals("postalCode", address.E2_Postcode, op.OperatorAddress.PostalCode);
		AssertEquals("streetandNumber1", address.E2_Address1, op.OperatorAddress.StreetAndNumber1);
		AssertEquals("streetandNumber2", address.E2_Address2, op.OperatorAddress.StreetAndNumber2);
		AssertEquals("city",
			Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, address.Address.OA_RL_NKRelatedPortCode).RL_PortName,
			op.OperatorAddress.City);
		AssertEquals("country", address.E2_RN_NKCountryCode, op.OperatorAddress.Country);
	}

	public void TestConsignee()
	{
		var invoiceLine = declaration.InvoiceLines[new Random().Next(0, declaration.InvoiceLines.Count)];
		var entry = CreateFirstEntryNoMerge(new[] { invoiceLine });
		var cusCode = declaration.Importer.CustomsCodes.AddNew();
		cusCode.OK_CodeType = "EOR";
		cusCode.OK_RN_NKCodeCountry = CountryCodes.Belgium;
		cusCode.OK_CustomsRegNo = "CONSIGNEE100";

		var op = new CUSDECMessageDataProvider(entry).GoodsDeclaration.Consignee;
		AssertEquals("country", CountryCodes.Belgium, op.Country);
		AssertEquals("Identifier", "005", op.Identifier);
		AssertEquals("OperatorIdentity", "CONSIGNEE100", op.OperatorIdentity);

		AssertEquals("operatorName", declaration.Importer.OH_FullName, op.OperatorName);
		var all = Factory.Load<JobDocAddress>(new ZQuery(new ZQuery(JobDocAddressSchema.E2_AddressType, "IMD"), new ZQuery(JobDocAddressSchema.E2_ParentID, declaration.PK)));
		var address = Factory.LoadTop1<JobDocAddress>(new ZQuery(new ZQuery(JobDocAddressSchema.E2_AddressType, "IMD"), new ZQuery(JobDocAddressSchema.E2_ParentID, declaration.PK)));
		AssertEquals("postalCode", address.E2_Postcode, op.OperatorAddress.PostalCode);
		AssertEquals("streetandNumber1", address.E2_Address1, op.OperatorAddress.StreetAndNumber1);
		AssertEquals("streetandNumber2", address.E2_Address2, op.OperatorAddress.StreetAndNumber2);
		AssertEquals("city",
			Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, address.Address.OA_RL_NKRelatedPortCode).RL_PortName,
			op.OperatorAddress.City);
		AssertEquals("country", address.E2_RN_NKCountryCode, op.OperatorAddress.Country);
	}
}
