using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.EInvoicing.Egypt.Schemas;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using UniversalRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.EInvoicing.Egypt.Testing
{
	public class XUTtoDocumentMapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo();

			var constructorError = AssertExceptionThrown<ArgumentException>(() => new XUTtoDocumentMapper(null, DocumentType.i, OrgConstants.Category.Business));
			AssertContains("Value cannot be null.\r\nParameter name: transactionInfo", constructorError.Message);

			AssertNoExceptionThrown(() => new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business));

			constructorError = AssertExceptionThrown<ArgumentException>(() => new XUTtoDocumentMapper(transactionInfo, DocumentType.d, OrgConstants.Category.Business));
			AssertContains("Value cannot be null.\r\nParameter name: originalTransactionId", constructorError.Message);

			constructorError = AssertExceptionThrown<ArgumentException>(() => new XUTtoDocumentMapper(transactionInfo, DocumentType.d, OrgConstants.Category.Business, null, "S98L2CP1SMVBIU"));
			AssertContains("Value cannot be null.\r\nParameter name: OriginalReference", constructorError.Message);

			transactionInfo.OriginalReference = new OriginalReference();
			AssertNoExceptionThrown(() => new XUTtoDocumentMapper(transactionInfo, DocumentType.d, OrgConstants.Category.Business, null, "S98L2CP1SMVBIU"));
		}

		public void TestValidateTransactionInfo()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo();

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var regToRemove = transactionInfo.BranchAddress.RegistrationNumberCollection.Find(x => x.Type.Code.ToString() == OrgCusCode.CodeTypes.CorporationCode);
			transactionInfo.BranchAddress.RegistrationNumberCollection.Remove(regToRemove);

			Assert(!mapper.ValidateTransactionInfo(notifications));
			Assert(notifications.HasErrors);
			AssertContains("Missing required Egypt GCR registration number of the BRN branch.", notifications.AsString);

			regToRemove = transactionInfo.BranchAddress.RegistrationNumberCollection.Find(x => x.Type.Code.ToString() == OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber);
			transactionInfo.BranchAddress.RegistrationNumberCollection.Remove(regToRemove);

			notifications = new NotificationBuffer();
			Assert(!mapper.ValidateTransactionInfo(notifications));
			Assert(notifications.HasErrors);
			AssertContains("Missing required Egypt COM registration number of the BRN branch.", notifications.AsString);

			regToRemove = transactionInfo.BranchAddress.RegistrationNumberCollection.Find(x => x.Type.Code.ToString() == OrgCusCode.CodeTypes.GovBusinessCode);
			transactionInfo.BranchAddress.RegistrationNumberCollection.Remove(regToRemove);

			notifications = new NotificationBuffer();
			Assert(!mapper.ValidateTransactionInfo(notifications));
			Assert(notifications.HasErrors);
			AssertContains("Missing required Egypt GBR registration number of the BRN branch.", notifications.AsString);
		}

		public void TestMapToDocument_Invoice()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV);

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business, GlbBranch.CurrentBranch.HomePort.TimeZoneSet);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.B, "ORGCOM001", "EG", "Luxor Governorate");
			AssertNull("references", document.references);
			AssertNull("purchaseOrderReference", document.purchaseOrderReference);
			AssertNull("delivery", document.delivery);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_CreditNote()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.CRD);

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.c, OrgConstants.Category.Business, GlbBranch.CurrentBranch.HomePort.TimeZoneSet);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR CRD ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.B, "ORGCOM001", "EG", "Luxor Governorate");
			AssertNull("references", document.references);
			AssertNull("purchaseOrderReference", document.purchaseOrderReference);
			AssertNull("delivery", document.delivery);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_DebitNote()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV);
			transactionInfo.OriginalReference = new OriginalReference();

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.d, OrgConstants.Category.Business, GlbBranch.CurrentBranch.HomePort.TimeZoneSet, "S98L2CP1SMVBIU");

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.B, "ORGCOM001", "EG", "Luxor Governorate");
			AssertNotNull("references", document.references);
			AssertEquals("Has 1 reference", 1, document.references.Length);
			AssertEquals("UUID", "S98L2CP1SMVBIU", document.references[0]);
			AssertNull("purchaseOrderReference", document.purchaseOrderReference);
			AssertNull("delivery", document.delivery);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_WhenProductionLicense()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV);

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001", docTypeVersion: "1.0");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.B, "ORGCOM001", "EG", "Luxor Governorate");
			AssertNull("references", document.references);
			AssertNull("purchaseOrderReference", document.purchaseOrderReference);
			AssertNull("delivery", document.delivery);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_WhenForeignReceiverStateCodeNotPresent()
		{
			var orgAddressRegistrations = new List<UniversalRegistrationNumber>();
			AddRegistrationNumber(orgAddressRegistrations, OrgCusCode.CodeTypes.VATCode, "ORGVAT001", "FJ");
			var orgAddressWithoutStateCode = CreateAddress("Debtor", "", "Luxor", "3003", "2 Pharaon Dr", null, "FJ");
			orgAddressWithoutStateCode.OrganizationCode = "ORGTSTCODE";
			orgAddressWithoutStateCode.SetRegistrationNumberCollection(() => orgAddressRegistrations);
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV, orgAddressWithoutStateCode);

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.F, "ORGVAT001", "FJ", "FJ");
			AssertNull("references", document.references);
			AssertNull("purchaseOrderReference", document.purchaseOrderReference);
			AssertNull("delivery", document.delivery);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_WhenForeignReceiverPostalCodeNotPresent()
		{
			var orgAddressRegistrations = new List<UniversalRegistrationNumber>();
			AddRegistrationNumber(orgAddressRegistrations, OrgCusCode.CodeTypes.VATCode, "ORGVAT001", "FJ");
			var orgAddressWithoutPostalCode = CreateAddress("Debtor", "LX", "Luxor", "", "2 Pharaon Dr", null, "FJ");
			orgAddressWithoutPostalCode.OrganizationCode = "ORGTSTCODE";
			orgAddressWithoutPostalCode.SetRegistrationNumberCollection(() => orgAddressRegistrations);
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV, orgAddressWithoutPostalCode);

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.F, "ORGVAT001", "FJ", "LX", "0000");
			AssertNull("references", document.references);
			AssertNull("purchaseOrderReference", document.purchaseOrderReference);
			AssertNull("delivery", document.delivery);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_WhenForeignReceiverStateAndPostalCodeNotPresent()
		{
			var orgAddressRegistrations = new List<UniversalRegistrationNumber>();
			AddRegistrationNumber(orgAddressRegistrations, OrgCusCode.CodeTypes.VATCode, "ORGVAT001", "FJ");
			var orgAddressWithoutStateOrPostalCode = CreateAddress("Debtor", "", "Luxor", "", "2 Pharaon Dr", null, "FJ");
			orgAddressWithoutStateOrPostalCode.OrganizationCode = "ORGTSTCODE";
			orgAddressWithoutStateOrPostalCode.SetRegistrationNumberCollection(() => orgAddressRegistrations);
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV, orgAddressWithoutStateOrPostalCode);

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.F, "ORGVAT001", "FJ", "FJ", "0000");
			AssertNull("references", document.references);
			AssertNull("purchaseOrderReference", document.purchaseOrderReference);
			AssertNull("delivery", document.delivery);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_WhenEgyptReceiverStateAndPostalCodeNotPresent()
		{
			var orgAddressRegistrations = new List<UniversalRegistrationNumber>();
			AddRegistrationNumber(orgAddressRegistrations, OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber, "ORGCOM001");
			var orgAddressWithoutPostalCode = CreateAddress("Debtor", "", "Luxor", "", "2 Pharaon Dr");
			orgAddressWithoutPostalCode.OrganizationCode = "ORGTSTCODE";
			orgAddressWithoutPostalCode.SetRegistrationNumberCollection(() => orgAddressRegistrations);
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV, orgAddressWithoutPostalCode);

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.B, "ORGCOM001", "EG", null, "");
			AssertNull("references", document.references);
			AssertNull("purchaseOrderReference", document.purchaseOrderReference);
			AssertNull("delivery", document.delivery);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_ForwardingShipment_OrderRef()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.DocsAndCartage.JP_OrderItemsAsString = "ABCD1234";
			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business, shipment: shipment);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.B, "ORGCOM001", "EG", "Luxor Governorate");
			AssertNull("references", document.references);
			AssertEquals("purchaseOrderReference", "ABCD1234", document.purchaseOrderReference);
			AssertEquals("delivery", 0m, document.delivery.grossWeight);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_ForwardingShipment_GrossWeight()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualWeight = 100.23456m;
			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business, shipment: shipment);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.B, "ORGCOM001", "EG", "Luxor Governorate");
			AssertNull("references", document.references);
			AssertEquals("purchaseOrderReference", "", document.purchaseOrderReference);
			AssertEquals("delivery", 100.23500m, document.delivery.grossWeight);
			AssertInvoiceProperties(document.invoiceLines);
			AssertTaxTotals(document.taxTotals);
		}

		public void TestMapToDocument_ForTaxType_T1_NonZero()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 1, TaxGroupCodeT1)
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T1", 1) },
				expectedTaxTotals: new (string, decimal)[] { ("T1", 20.02m) });
		}

		public void TestMapToDocument_ForTaxType_T1_Zero()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 0, TaxGroupCodeT1)
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T1", 0) },
				expectedTaxTotals: new (string, decimal)[] { ("T1", 0m) });
		}

		public void TestMapToDocument_ForTaxType_T2_NonZero()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 1, TaxGroupCodeT2)
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T2", 1) },
				expectedTaxTotals: new (string, decimal)[] { ("T2", 20.02m) });
		}

		public void TestMapToDocument_ForTaxType_T2_Zero()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 0, TaxGroupCodeT2)
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T2", 0) },
				expectedTaxTotals: new (string, decimal)[] { ("T2", 0m) });
		}

		public void TestMapToDocument_ForTaxType_T1_NonZero_T2_Zero()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 1, TaxGroupCodeT1),
				GetPostingJournal(multiplier: 1, TaxGroupCodeT1),
				GetPostingJournal(multiplier: 0, TaxGroupCodeT2)
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T1", 1), ("T1", 1), ("T2", 0) },
				expectedTaxTotals: new (string, decimal)[] { ("T1", 40.04m), ("T2", 0m) });
		}

		public void TestMapToDocument_ForTaxType_T1_Zero_T2_NonZero()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 0, TaxGroupCodeT1),
				GetPostingJournal(multiplier: 1, TaxGroupCodeT2),
				GetPostingJournal(multiplier: 1, TaxGroupCodeT2)
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T1", 0), ("T2", 1), ("T2", 1) },
				expectedTaxTotals: new (string, decimal)[] { ("T1", 0m), ("T2", 40.04m) });
		}

		public void TestMapToDocument_ForTaxType_T1_NonZero_T2_NonZero()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 1, TaxGroupCodeT1),
				GetPostingJournal(multiplier: 1, TaxGroupCodeT2)
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T1", 1), ("T2", 1) },
				expectedTaxTotals: new (string, decimal)[] { ("T1", 20.02m), ("T2", 20.02m) });
		}

		public void TestMapToDocument_ForTaxType_T1_Zero_T2_Zero()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 0, TaxGroupCodeT1),
				GetPostingJournal(multiplier: 0, TaxGroupCodeT2)
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T1", 0), ("T2", 0) },
				expectedTaxTotals: new (string, decimal)[] { ("T1", 0m), ("T2", 0m) });
		}

		public void TestMapToDocument_ForAnyTaxType()
		{
			var postingJournals = new List<PostingJournal>()
			{
				GetPostingJournal(multiplier: 1, CreateTaxGroupCode(taxType: "ᾮ:)"))
			};
			AssertMapToDocument_ForDifferentTaxTypes(postingJournals,
				expectedInvoiceLinesTaxMultiplier: new (string, int)[] { ("T1", 1) },
				expectedTaxTotals: new (string, decimal)[] { ("T1", 20.02m) });
		}

		void AssertMapToDocument_ForDifferentTaxTypes(List<PostingJournal> postingJournals, (string taxType, int multiplier)[] expectedInvoiceLinesTaxMultiplier, (string taxType, decimal amount)[] expectedTaxTotals)
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV, customPostingJournals: postingJournals);
			transactionInfo.OriginalReference = new OriginalReference();
			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.d, OrgConstants.Category.Business, GlbBranch.CurrentBranch.HomePort.TimeZoneSet, "S98L2CP1SMVBIU");

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertHeaderProperties(document, "AR INV ORGTSTCODE 10001");
			AssertIssuer(document.issuer);
			AssertReceiver(document.receiver, receiverPersonaTypeEnum.B, "ORGCOM001", "EG", "Luxor Governorate");
			AssertNotNull("references", document.references);
			AssertEquals("Has 1 reference", 1, document.references.Length);
			AssertEquals("UUID", "S98L2CP1SMVBIU", document.references[0]);

			AssertNotNull("document.invoiceLines", document.invoiceLines);
			AssertEquals("Has correct number of invoice lines", expectedInvoiceLinesTaxMultiplier.Length, document.invoiceLines.Length);
			for (int i = 0; i < expectedInvoiceLinesTaxMultiplier.Length; i++)
			{
				var taxType = expectedInvoiceLinesTaxMultiplier[i].taxType;
				var multiplier = expectedInvoiceLinesTaxMultiplier[i].multiplier;
				AssertLine(document.invoiceLines[i], "Line Description", 120.12m * multiplier, 100.10m * multiplier, 20.02m * multiplier);
				AssertLineUnitValue(document.invoiceLines[i].unitValue, 100.10m * multiplier);

				AssertNotNull("document.invoiceLines[i].taxableItems", document.invoiceLines[i].taxableItems);
				AssertEquals("Has 1 taxable item", 1, document.invoiceLines[i].taxableItems.Length);
				AssertLineTaxableItem(document.invoiceLines[i].taxableItems[0], taxType, 20.02m * multiplier, "V001", 10m);
			}

			AssertNotNull("taxTotals", document.taxTotals);
			AssertEquals("Has correct number of Tax totals", expectedTaxTotals.Length, document.taxTotals.Length);
			for (int i = 0; i < expectedTaxTotals.Length; i++)
			{
				AssertEquals("taxType", expectedTaxTotals[i].taxType, document.taxTotals[i].taxType);
				AssertEquals("amount", expectedTaxTotals[i].amount, document.taxTotals[i].amount);
			}
		}

		void AssertHeaderProperties(document document, string id, string docTypeVersion = "0.9")
		{
			AssertNotNull("document", document);
			AssertEquals("documentTypeVersion", docTypeVersion, document.documentTypeVersion);
			AssertEquals("dateTimeIssued", new DateTime(2021, 5, 4, 23, 45, 10), document.dateTimeIssued);
			AssertEquals("taxpayerActivityCode", "BRNGBR001", document.taxpayerActivityCode);
			AssertEquals("internalID", id, document.internalID);
			AssertEquals("totalSalesAmount", 100.10m, document.totalSalesAmount);
			AssertEquals("netAmount", 100.10m, document.netAmount);
			AssertEquals("totalAmount", 120.12m, document.totalAmount);
			AssertEquals("totalDiscountAmount", 0m, document.totalDiscountAmount);
			AssertEquals("extraDiscountAmount", 0m, document.extraDiscountAmount);
			AssertEquals("totalItemsDiscountAmount", 0m, document.totalItemsDiscountAmount);
		}

		void AssertIssuer(issuerType issuer)
		{
			AssertNotNull("issuer", issuer);
			AssertEquals("issuer.type", issuerPersonaTypeEnum.B, issuer.type);
			AssertEquals("issuer.id", "BRNCOM001", issuer.id);
			AssertEquals("issuer.name", "My Company Branch", issuer.name);
			AssertEquals("issuer.address.branchID", "BRNGCR001", issuer.address.branchID);
			AssertEquals("issuer.address.country", "EG", issuer.address.country);
			AssertEquals("issuer.address.governate", "Cairo Governorate", issuer.address.governate);
			AssertEquals("issuer.address.regionCity", "Cairo", issuer.address.regionCity);
			AssertEquals("issuer.address.street", "101 Pyramid St", issuer.address.street);
			AssertEquals("issuer.address.buildingNumber", "-", issuer.address.buildingNumber);
			AssertEquals("issuer.address.postalCode", "1001", issuer.address.postalCode);
		}

		void AssertReceiver(receiverType receiver, receiverPersonaTypeEnum type, string id, string country, string governate, string postalCode = "3003")
		{
			AssertNotNull("receiver", receiver);
			AssertEquals("receiver.type", type, receiver.type);
			AssertEquals("receiver.id", id, receiver.id);
			AssertEquals("receiver.name", "Debtor", receiver.name);
			AssertEquals("receiver.address.country", country, receiver.address.country);
			AssertEquals("receiver.address.governate", governate, receiver.address.governate);
			AssertEquals("receiver.address.regionCity", "Luxor", receiver.address.regionCity);
			AssertEquals("receiver.address.street", "2 Pharaon Dr", receiver.address.street);
			AssertEquals("receiver.address.buildingNumber", "-", receiver.address.buildingNumber);
			AssertEquals("receiver.address.postalCode", postalCode, receiver.address.postalCode);
		}

		void AssertInvoiceProperties(invoiceLineType[] invoiceLines)
		{
			AssertNotNull("document.invoiceLines", invoiceLines);
			AssertEquals("Has 3 line", 3, invoiceLines.Length);
			for (int i = 0; i < invoiceLines.Length; i++)
			{
				AssertLine(invoiceLines[i], "Line Description", 120.12m, 100.10m, 20.02m);
				AssertLineUnitValue(invoiceLines[i].unitValue, 100.10m);
				AssertNotNull($"document.invoiceLines[{i}].taxableItems", invoiceLines[i].taxableItems);
				AssertEquals("Has 1 taxable item", 1, invoiceLines[i].taxableItems.Length);
			}

			AssertLineTaxableItem(invoiceLines[0].taxableItems[0], "T1", 20.02m, "V001", 10m);
			AssertLineTaxableItem(invoiceLines[1].taxableItems[0], "T20", 20.02m, "OF03", 10m);
			AssertLineTaxableItem(invoiceLines[2].taxableItems[0], "T20", 20.02m, "OF04", 10m);
		}

		void AssertLine(invoiceLineType line, string description, decimal localTotal, decimal localExTax, decimal localTax)
		{
			AssertEquals("line.description", description, line.description);
			AssertEquals("line.itemType", "EGS", line.itemType);
			AssertEquals("line.unitType", "EA", line.unitType);
			AssertEquals("line.quantity", 1m, line.quantity);
			AssertEquals("line.total", localTotal, line.total);
			AssertEquals("line.salesTotal", localExTax, line.salesTotal);
			AssertEquals("line.netTotal", localExTax, line.netTotal);
			AssertEquals("line.totalTaxableFees", 0m, line.totalTaxableFees);
			AssertEquals("line.valueDifference", 0m, line.valueDifference);
		}

		void AssertLineUnitValue(unitValueType unitValue, decimal localAmount, string currency = "EGP", decimal osAmount = 0m, decimal exRate = 0m, bool isForeign = false)
		{
			AssertNotNull("line.unitValue", unitValue);
			AssertEquals("line.unitValue.currencySold", currency, unitValue.currencySold);
			AssertEquals("line.unitValue.amountEGP", localAmount, unitValue.amountEGP);
			AssertEquals("line.unitValue.amountSold", osAmount, unitValue.amountSold);
			AssertEquals("line.unitValue.amountSoldSpecified", isForeign, unitValue.amountSoldSpecified);
			AssertEquals("line.unitValue.currencyExchangeRate", exRate, unitValue.currencyExchangeRate);
			AssertEquals("line.unitValue.currencyExchangeRateSpecified", isForeign, unitValue.currencyExchangeRateSpecified);
		}

		void AssertLineTaxableItem(taxableItemType taxableItem, string taxType, decimal taxAmount, string subType, decimal taxRate, bool rateSpecified = true)
		{
			AssertNotNull("line.taxableItem", taxableItem);
			AssertEquals("line.taxableItem.taxType", taxType, taxableItem.taxType);
			AssertEquals("line.taxableItem.amount", taxAmount, taxableItem.amount);
			AssertEquals("line.taxableItem.subType", subType, taxableItem.subType);
			AssertEquals("line.taxableItem.rate", taxRate, taxableItem.rate);
			AssertEquals("line.taxableItem.rateSpecified", rateSpecified, taxableItem.rateSpecified);
		}

		void AssertTaxTotals(taxTotalType[] taxTotals)
		{
			AssertNotNull("taxTotals", taxTotals);
			AssertEquals("taxTotals.Length", 2, taxTotals.Length);
			AssertEquals("taxType", "T1", taxTotals[0].taxType);
			AssertEquals("amount", 20.02m, taxTotals[0].amount);
			AssertEquals("taxType", "T20", taxTotals[1].taxType);
			AssertEquals("amount", 40.04m, taxTotals[1].amount);
		}

		public void TestMapToXML_Invoice()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo();

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business, GlbBranch.CurrentBranch.HomePort.TimeZoneSet);
			var notifications = new NotificationBuffer();

			var mappingResult = mapper.MapToXml(notifications);
			Assert(!notifications.HasErrors);
			Assert(mappingResult.Success);

			var expectedXml = @"<document xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <issuer>
    <type>B</type>
    <id>BRNCOM001</id>
    <name>My Company Branch</name>
    <address>
      <buildingNumber>-</buildingNumber>
      <street>101 Pyramid St</street>
      <governate>Cairo Governorate</governate>
      <regionCity>Cairo</regionCity>
      <postalCode>1001</postalCode>
      <country>EG</country>
      <branchID>BRNGCR001</branchID>
    </address>
  </issuer>
  <receiver>
    <type>B</type>
    <id>ORGCOM001</id>
    <name>Debtor</name>
    <address>
      <buildingNumber>-</buildingNumber>
      <street>2 Pharaon Dr</street>
      <governate>Luxor Governorate</governate>
      <regionCity>Luxor</regionCity>
      <postalCode>3003</postalCode>
      <country>EG</country>
    </address>
  </receiver>
  <documentType>i</documentType>
  <documentTypeVersion>0.9</documentTypeVersion>
  <dateTimeIssued>2021-05-04T23:45:10Z</dateTimeIssued>
  <taxpayerActivityCode>BRNGBR001</taxpayerActivityCode>
  <internalID>AR INV ORGTSTCODE 10001</internalID>
  <invoiceLines>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T1</taxType>
          <amount>20.02</amount>
          <subType>V001</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T20</taxType>
          <amount>20.02</amount>
          <subType>OF03</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T20</taxType>
          <amount>20.02</amount>
          <subType>OF04</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
  </invoiceLines>
  <totalSalesAmount>100.10</totalSalesAmount>
  <totalDiscountAmount>0</totalDiscountAmount>
  <netAmount>100.10</netAmount>
  <taxTotals>
    <taxTotal>
      <taxType>T1</taxType>
      <amount>20.02</amount>
    </taxTotal>
    <taxTotal>
      <taxType>T20</taxType>
      <amount>40.04</amount>
    </taxTotal>
  </taxTotals>
  <totalAmount>120.12</totalAmount>
  <totalItemsDiscountAmount>0</totalItemsDiscountAmount>
  <extraDiscountAmount>0</extraDiscountAmount>
</document>";
			this.AssertXMLEqualsByDiff(expectedXml, mappingResult.Xml);
		}

		public void TestMapToXML_CreditNote()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.CRD);

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.c, OrgConstants.Category.Business, GlbBranch.CurrentBranch.HomePort.TimeZoneSet);
			var notifications = new NotificationBuffer();

			var mappingResult = mapper.MapToXml(notifications);
			Assert(!notifications.HasErrors);
			Assert(mappingResult.Success);

			var expectedXml = @"<document xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <issuer>
    <type>B</type>
    <id>BRNCOM001</id>
    <name>My Company Branch</name>
    <address>
      <buildingNumber>-</buildingNumber>
      <street>101 Pyramid St</street>
      <governate>Cairo Governorate</governate>
      <regionCity>Cairo</regionCity>
      <postalCode>1001</postalCode>
      <country>EG</country>
      <branchID>BRNGCR001</branchID>
    </address>
  </issuer>
  <receiver>
    <type>B</type>
    <id>ORGCOM001</id>
    <name>Debtor</name>
    <address>
      <buildingNumber>-</buildingNumber>
      <street>2 Pharaon Dr</street>
      <governate>Luxor Governorate</governate>
      <regionCity>Luxor</regionCity>
      <postalCode>3003</postalCode>
      <country>EG</country>
    </address>
  </receiver>
  <documentType>c</documentType>
  <documentTypeVersion>0.9</documentTypeVersion>
  <dateTimeIssued>2021-05-04T23:45:10Z</dateTimeIssued>
  <taxpayerActivityCode>BRNGBR001</taxpayerActivityCode>
  <internalID>AR CRD ORGTSTCODE 10001</internalID>
  <invoiceLines>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T1</taxType>
          <amount>20.02</amount>
          <subType>V001</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T20</taxType>
          <amount>20.02</amount>
          <subType>OF03</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T20</taxType>
          <amount>20.02</amount>
          <subType>OF04</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
  </invoiceLines>
  <totalSalesAmount>100.10</totalSalesAmount>
  <totalDiscountAmount>0</totalDiscountAmount>
  <netAmount>100.10</netAmount>
  <taxTotals>
    <taxTotal>
      <taxType>T1</taxType>
      <amount>20.02</amount>
    </taxTotal>
    <taxTotal>
      <taxType>T20</taxType>
      <amount>40.04</amount>
    </taxTotal>
  </taxTotals>
  <totalAmount>120.12</totalAmount>
  <totalItemsDiscountAmount>0</totalItemsDiscountAmount>
  <extraDiscountAmount>0</extraDiscountAmount>
</document>";
			this.AssertXMLEqualsByDiff(expectedXml, mappingResult.Xml);
		}

		public void TestMapToXML_DebitNote()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV);
			transactionInfo.OriginalReference = new OriginalReference();

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.d, OrgConstants.Category.Business, GlbBranch.CurrentBranch.HomePort.TimeZoneSet, "S98L2CP1SMVBIU");
			var notifications = new NotificationBuffer();

			var mappingResult = mapper.MapToXml(notifications);
			Assert(!notifications.HasErrors);
			Assert(mappingResult.Success);

			var expectedXml = @"<document xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <issuer>
    <type>B</type>
    <id>BRNCOM001</id>
    <name>My Company Branch</name>
    <address>
      <buildingNumber>-</buildingNumber>
      <street>101 Pyramid St</street>
      <governate>Cairo Governorate</governate>
      <regionCity>Cairo</regionCity>
      <postalCode>1001</postalCode>
      <country>EG</country>
      <branchID>BRNGCR001</branchID>
    </address>
  </issuer>
  <receiver>
    <type>B</type>
    <id>ORGCOM001</id>
    <name>Debtor</name>
    <address>
      <buildingNumber>-</buildingNumber>
      <street>2 Pharaon Dr</street>
      <governate>Luxor Governorate</governate>
      <regionCity>Luxor</regionCity>
      <postalCode>3003</postalCode>
      <country>EG</country>
    </address>
  </receiver>
  <documentType>d</documentType>
  <documentTypeVersion>0.9</documentTypeVersion>
  <dateTimeIssued>2021-05-04T23:45:10Z</dateTimeIssued>
  <taxpayerActivityCode>BRNGBR001</taxpayerActivityCode>
  <internalID>AR INV ORGTSTCODE 10001</internalID>
  <references>
    <reference>S98L2CP1SMVBIU</reference>
  </references>
  <invoiceLines>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T1</taxType>
          <amount>20.02</amount>
          <subType>V001</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T20</taxType>
          <amount>20.02</amount>
          <subType>OF03</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
    <invoiceLine>
      <description>Line Description</description>
      <itemType>EGS</itemType>
      <itemCode>GOVCC1</itemCode>
      <unitType>EA</unitType>
      <quantity>1</quantity>
      <unitValue>
        <currencySold>EGP</currencySold>
        <amountEGP>100.10</amountEGP>
      </unitValue>
      <salesTotal>100.10</salesTotal>
      <taxableItems>
        <taxableItem>
          <taxType>T20</taxType>
          <amount>20.02</amount>
          <subType>OF04</subType>
          <rate>10</rate>
        </taxableItem>
      </taxableItems>
      <netTotal>100.10</netTotal>
      <totalTaxableFees>0</totalTaxableFees>
      <valueDifference>0</valueDifference>
      <total>120.12</total>
    </invoiceLine>
  </invoiceLines>
  <totalSalesAmount>100.10</totalSalesAmount>
  <totalDiscountAmount>0</totalDiscountAmount>
  <netAmount>100.10</netAmount>
  <taxTotals>
    <taxTotal>
      <taxType>T1</taxType>
      <amount>20.02</amount>
    </taxTotal>
    <taxTotal>
      <taxType>T20</taxType>
      <amount>40.04</amount>
    </taxTotal>
  </taxTotals>
  <totalAmount>120.12</totalAmount>
  <totalItemsDiscountAmount>0</totalItemsDiscountAmount>
  <extraDiscountAmount>0</extraDiscountAmount>
</document>";
			this.AssertXMLEqualsByDiff(expectedXml, mappingResult.Xml);
		}

		public void TestMapToXML_MiscForeignInvoice()
		{
			var transactionInfo = GetPopulatedUniversalTransactionInfo();
			transactionInfo.OSCurrency = new Currency { Code = "USD", Description = "United States Dollar" };
			transactionInfo.ExchangeRate = 1.33m;

			transactionInfo.PostingJournalCollection.ForEach(x =>
			{
				x.ChargeCurrency = null;
				x.ChargeExchangeRate = null;
			});

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			var document = mapper.MapToDocument();

			AssertLineUnitValue(document.invoiceLines[0].unitValue, 100.10m, "USD", 120.12m, 1.33m, true);
		}

		public void TestMapToXML_MiscForeignInvoiceLineExchangeRate()
		{
			var postingJournal1 = new PostingJournal()
			{
				Sequence = 1,
				Description = "International Freight",
				OSCurrency = new Currency { Code = Constants.CurrencyCodes.UnitedStates },
				OSAmount = 1655.00m,
				OSGSTVATAmount = 231.7m,
				OSTotalAmount = 1886.7m,
				ChargeCurrency = new Currency() { Code = Constants.CurrencyCodes.Egypt },
				LocalAmount = 51061.72m,
				LocalGSTVATAmount = 7148.64m,
				LocalTotalAmount = 58210.36m,
				VATTaxID = new TaxID()
				{
					TaxCode = "VAT",
					TaxRate = 14m
				},
				ChargeExchangeRate = 30.853003m,
			};

			var postingJournal2 = new PostingJournal()
			{
				Sequence = 2,
				Description = "Delivery Cartage",
				OSCurrency = new Currency { Code = Constants.CurrencyCodes.UnitedStates },
				OSAmount = 255.00m,
				OSGSTVATAmount = 35.70m,
				OSTotalAmount = 290.70m,
				ChargeCurrency = new Currency() { Code = Constants.CurrencyCodes.Egypt },
				LocalAmount = 7867.52m,
				LocalGSTVATAmount = 1101.45m,
				LocalTotalAmount = 8968.97m,
				VATTaxID = new TaxID()
				{
					TaxCode = "VAT",
					TaxRate = 14m
				},
				ChargeExchangeRate = 30.853020m,
			};

			var postingJournals = new List<PostingJournal>()
			{
				postingJournal1,
				postingJournal2,
			};

			var transactionInfo = GetPopulatedUniversalTransactionInfo(TransactionType.INV, customPostingJournals: postingJournals);
			transactionInfo.OSCurrency = new Currency { Code = "USD", Description = "United States Dollar" };
			transactionInfo.ExchangeRate = 30.853000m;

			var mapper = new XUTtoDocumentMapper(transactionInfo, DocumentType.i, OrgConstants.Category.Business);

			var notifications = new NotificationBuffer();
			Assert(mapper.ValidateTransactionInfo(notifications));
			Assert(!notifications.HasErrors);

			//Precondition: Egypt expects the same exchange rate on each line item. For testing purposes, line level rates should be different to header rate before mapping
			AssertNotEquals(transactionInfo.ExchangeRate.Value, transactionInfo.PostingJournalCollection[0].ChargeExchangeRate.Value);
			AssertNotEquals(transactionInfo.ExchangeRate.Value, transactionInfo.PostingJournalCollection[1].ChargeExchangeRate.Value);

			//Action
			var document = mapper.MapToDocument();

			//After mapping, header rate will be applied to the lines, and then all the lines will have the same exchange rate
			AssertLineUnitValue(document.invoiceLines[0].unitValue, 51061.72m, "USD", 1655.00m, 30.853000m, true);
			AssertLineUnitValue(document.invoiceLines[1].unitValue, 7867.52m, "USD", 255.00m, 30.853000m, true);
		}

		UniversalTransactionInfo GetPopulatedUniversalTransactionInfo(TransactionType transactionType = TransactionType.INV, OrganizationAddress customOrgAddress = null, List<PostingJournal> customPostingJournals = null)
		{
			var branchAddressRegistrations = new List<UniversalRegistrationNumber>();
			AddRegistrationNumber(branchAddressRegistrations, OrgCusCode.CodeTypes.CorporationCode, "BRNGCR001");
			AddRegistrationNumber(branchAddressRegistrations, OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber, "BRNCOM001");
			AddRegistrationNumber(branchAddressRegistrations, OrgCusCode.CodeTypes.GovBusinessCode, "BRNGBR001");

			var branchAddress = CreateAddress("My Company Branch", "C", "Cairo", "1001", "101 Pyramid St", "");
			branchAddress.SetRegistrationNumberCollection(() => branchAddressRegistrations);

			OrganizationAddress orgAddress;
			if (customOrgAddress != null)
			{
				orgAddress = customOrgAddress;
			}
			else
			{
				var orgAddressRegistrations = new List<UniversalRegistrationNumber>();
				AddRegistrationNumber(orgAddressRegistrations, OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber, "ORGCOM001");

				orgAddress = CreateAddress("Debtor", "LX", "Luxor", "3003", "2 Pharaon Dr");
				orgAddress.OrganizationCode = "ORGTSTCODE";
				orgAddress.SetRegistrationNumberCollection(() => orgAddressRegistrations);
			}

			var amountMultiplier = transactionType == TransactionType.CRD ? -1m : 1m;
			var result = new UniversalTransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch()
				{
					Code = "BRN",
					Name = "My Branch"
				},
				BranchAddress = branchAddress,
				OrganizationAddress = orgAddress,
				TransactionDate = new ZDateTime(2021, 5, 5, 9, 45, 10),
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = transactionType,
				Number = "10001",
				LocalExVATAmount = amountMultiplier * 100.10m,
				LocalTotal = amountMultiplier * 120.12m,
			};
			result.SetPostingJournalCollection(() => customPostingJournals ?? GetPostingJournalCollection(amountMultiplier));

			return result;
		}

		void AddRegistrationNumber(List<UniversalRegistrationNumber> numbers, string type, string number, string country = Constants.CountryCodes.Egypt)
		{
			numbers.Add(new UniversalRegistrationNumber()
			{
				Type = new RegistrationNumberType() { Code = type },
				CountryOfIssue = new Country() { Code = country },
				Value = number
			});
		}

		OrganizationAddress CreateAddress(string name, string state, string city, string post, string line1, string line2 = null, string country = Constants.CountryCodes.Egypt)
			=> new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = name,
				Country = new Country() { Code = country },
				State = state,
				City = city,
				Address1 = line1,
				Address2 = line2,
				Postcode = post,
			};

		List<PostingJournal> GetPostingJournalCollection(decimal multiplier = 1m)
		{
			var postingJournals = new List<PostingJournal>
			{
				GetPostingJournal(multiplier, TaxGroupCodeE01),
				GetPostingJournal(multiplier, TaxGroupCodeN01),
				GetPostingJournal(multiplier, TaxGroupCodeN02),
				new PostingJournal()
				{
					ChargeCode = new ChargeCode()
					{
						Code = "ABC",
						ChargeType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair()
						{
							Code = Constants.ChargeType.Comment,
							Description = "Test Charge Type Description",
						}
					},
					Description = "Test Comment"
				}
			};

			return postingJournals;
		}

		TaxGroupCodeType CreateTaxGroupCode(string taxType)
			=> new() { Code = taxType, Description = "Tax Group E01", GovernmentCode = "V001" };

		TaxGroupCodeType TaxGroupCodeT1
			=> CreateTaxGroupCode(taxType: "T1");

		TaxGroupCodeType TaxGroupCodeT2
			=> CreateTaxGroupCode(taxType: "T2");

		TaxGroupCodeType TaxGroupCodeE01
			=> new() { Code = "E01", Description = "Tax Group E01", GovernmentCode = "V001" };

		TaxGroupCodeType TaxGroupCodeN01
			=> new() { Code = "N01", Description = "Other fees (rate)", GovernmentCode = "OF03" };

		TaxGroupCodeType TaxGroupCodeN02
			=> new() { Code = "N02", Description = "Other fees (amount)", GovernmentCode = "OF04" };

		PostingJournal GetPostingJournal(decimal multiplier, TaxGroupCodeType taxGroupCode)
		{
			return new PostingJournal()
			{
				Description = "Line Description",
				GovernmentReportingChargeCode = "GOVCC1",
				ChargeCurrency = new Currency() { Code = Constants.CurrencyCodes.Egypt },
				ChargeExchangeRate = 1m,
				OSAmount = multiplier * 120.12m,
				LocalTotalAmount = multiplier * 120.12m,
				LocalAmount = multiplier * 100.10m,
				LocalGSTVATAmount = multiplier * 20.02m,
				TaxMessageID = new TaxMessageID()
				{
					TaxMessageCode = "TMC",
					Description = "Tax Message Description",
					TaxGroupCode = taxGroupCode
				},
				VATTaxID = new TaxID() { TaxRate = 10m },
			};
		}
	}
}
