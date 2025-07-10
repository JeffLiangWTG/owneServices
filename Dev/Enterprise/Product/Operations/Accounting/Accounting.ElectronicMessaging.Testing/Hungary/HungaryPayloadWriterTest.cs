using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	[TestedType(typeof(HungaryPayloadWriter))]
	class HungaryPayloadWriterTest : TransactionBatchToXmlWriterTest
	{
		protected override TransactionBatchToPayloadWriterBase GetTestWriter() => new HungaryPayloadWriter();

		public override void TestGetPayloadValidation()
		{
			var payloadWriter = GetTestWriter() as ITransactionBatchToPayloadWriter;

			AssertNull("Validation not implemented", payloadWriter.GetPayloadValidation(ZString.Empty));
			AssertNull("Validation not implemented", payloadWriter.GetPayloadValidation(HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest));
		}

		public void TestWritePayloadToStream_InvalidMessageType_ThrowsArgumentException()
		{
			// Arrange
			using (var stream = new MemoryStream())
			{
				var eInvoicingBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				transactionBatch.TransactionCollection.Add(transactionInfo);
				var payloadWriter = GetTestWriter() as ITransactionBatchToPayloadWriter;

				void WritePayloadToStream(string badMessageType)
				{
					// Act
					payloadWriter.WritePayloadToStream(transactionBatch, stream, badMessageType, eInvoicingBatch, new Logger(), new Logger());
				}

				// Assert
				AssertExceptionThrown<ArgumentException>("Message Type should not be empty.", () => WritePayloadToStream(""));
				AssertExceptionThrown<ArgumentException>("Only the GEN message type is valid.", () => WritePayloadToStream("BAD"));
			}
		}

		[TestDate(2024, 04, 04)]
		public void TestWritePayloadToStream_SetsTaxDate()
		{
			// Arrange
			using (var stream = new MemoryStream())
			{
				var client = Factory.NewWithValidTestData<OrgHeader>();
				client.OH_FullName = "Cookie Monster";
				client.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

				var transactionBatch = GetTransactionBatch();
				var transactionInfo = transactionBatch.TransactionCollection[0];
				transactionInfo.OriginalReference = new OriginalReference();

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				invoice.AH_OH = client.PK;
				var taxLine = invoice.Lines.AddNew();
				taxLine.AL_TaxDate = new ZDate(2024, 02, 02);

				var eInvoicingBatch = GetAccBatch(invoice);
				var payloadWriter = GetTestWriter() as ITransactionBatchToPayloadWriter;

				// Act
				payloadWriter.WritePayloadToStream(transactionBatch, stream, HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, eInvoicingBatch, new Logger(), new Logger());

				// Assert
				#region Expected XML strings

				var expectedInvoiceData = """
<?xml version="1.0" encoding="utf-8"?>
<InvoiceData
	xmlns="http://schemas.nav.gov.hu/OSA/3.0/data"
	xmlns:ns2="http://schemas.nav.gov.hu/OSA/3.0/base">
	<invoiceNumber>HUNN1</invoiceNumber>
	<invoiceIssueDate></invoiceIssueDate>
	<completenessIndicator>false</completenessIndicator>
	<invoiceMain>
		<invoice>
			<invoiceHead>
				<supplierInfo>
					<supplierAddress>
						<ns2:simpleAddress>
							<ns2:countryCode />
							<ns2:postalCode>0000</ns2:postalCode>
							<ns2:additionalAddressDetail></ns2:additionalAddressDetail>
						</ns2:simpleAddress>
					</supplierAddress>
				</supplierInfo>
				<customerInfo>
					<customerVatStatus>PRIVATE_PERSON</customerVatStatus>
				</customerInfo>
				<invoiceDetail>
					<invoiceCategory></invoiceCategory>
					<invoiceDeliveryDate>2024-02-02</invoiceDeliveryDate>
					<currencyCode></currencyCode>
					<exchangeRate></exchangeRate>
					<paymentDate></paymentDate>
					<invoiceAppearance>UNKNOWN</invoiceAppearance>
				</invoiceDetail>
			</invoiceHead>
			<invoiceLines>
				<mergedItemIndicator>false</mergedItemIndicator>
				<line>
					<lineNumber>1</lineNumber>
					<lineExpressionIndicator>false</lineExpressionIndicator>
					<lineNatureIndicator>SERVICE</lineNatureIndicator>
					<lineDescription></lineDescription>
					<lineAmountsNormal>
						<lineNetAmountData>
							<lineNetAmount>100.00</lineNetAmount>
							<lineNetAmountHUF></lineNetAmountHUF>
						</lineNetAmountData>
						<lineVatRate />
					</lineAmountsNormal>
				</line>
			</invoiceLines>
			<invoiceSummary>
				<summaryNormal>
					<invoiceNetAmount>100.00</invoiceNetAmount>
					<invoiceNetAmountHUF>0.00</invoiceNetAmountHUF>
					<invoiceVatAmount>0.00</invoiceVatAmount>
					<invoiceVatAmountHUF>0.00</invoiceVatAmountHUF>
				</summaryNormal>
				<summaryGrossData>
					<invoiceGrossAmount></invoiceGrossAmount>
					<invoiceGrossAmountHUF></invoiceGrossAmountHUF>
				</summaryGrossData>
			</invoiceSummary>
		</invoice>
	</invoiceMain>
</InvoiceData>
""".Trim();

				#endregion

				stream.Position = 0;
				var actualXML = new StreamReader(stream).ReadToEnd();
				AssertNull(transactionInfo.OriginalReference.OriginalTransactionDate);
				AssertEquals(new ZDateTime(2024, 02, 02), transactionInfo.PostingJournalCollection[0].TaxDate);
				this.AssertXMLEqualsIgnoreChildOrder("PayloadWriter shold create encoded invoice data", expectedInvoiceData, GetXMLInvoiceData(actualXML));
			}
		}

		[TestDate(2024, 04, 04)]
		public void TestWritePayloadToStream_WithOriginalReferenceInvoice()
		{
			// Arrange
			using (var stream = new MemoryStream())
			{
				var client = Factory.NewWithValidTestData<OrgHeader>();
				client.OH_FullName = "Cookie Monster";
				client.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

				var transactionBatch = GetTransactionBatch();
				var transactionInfo = transactionBatch.TransactionCollection[0];
				transactionInfo.OriginalReference = new OriginalReference();
				transactionInfo.OriginalReference.OriginalTransactionNumber = "ABC123";
				transactionInfo.OriginalReference.OriginalTransactionDate = new ZDateTime(2024, 04, 02);

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var originalInvoice = Factory.NewWithValidTestData<ARInvoice>();
				originalInvoice.AH_OH = client.PK;
				originalInvoice.AH_TransactionNum = "ABC123";
				originalInvoice.AH_InvoiceDate = new ZDateTime(2024, 01, 01);
				originalInvoice.AH_PostDate = new ZDateTime(2024, 01, 11);
				invoice.OriginalTransactionReference = originalInvoice.PK;

				var eInvoicingBatch = GetAccBatch(invoice);
				var payloadWriter = GetTestWriter() as ITransactionBatchToPayloadWriter;

				// Act
				payloadWriter.WritePayloadToStream(transactionBatch, stream, HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, eInvoicingBatch, new Logger(), new Logger());

				// Assert
				#region Expected XML strings

				var version = new EnterpriseInformationRetriever().VersionNumber;
				var expectedXML = $"""
<?xml version="1.0" encoding="utf-8"?>
<ManageInvoiceRequest
	xmlns:common="http://schemas.nav.gov.hu/NTCA/1.0/common"
	xmlns="http://schemas.nav.gov.hu/OSA/3.0/api">
	<common:header>
		<common:requestId>$$requestIdReplaceMe$$</common:requestId>
		<common:timestamp>$$timestampReplaceMe$$</common:timestamp>
		<common:requestVersion>3.0</common:requestVersion>
		<common:headerVersion>1.0</common:headerVersion>
	</common:header>
	<common:user>
		<common:requestSignature cryptoType="SHA3-512">$$requestSignatureReplaceMe$$</common:requestSignature>
	</common:user>
	<software>
		<softwareId>AU658947CARGOWISE1</softwareId>
		<softwareName>CargoWise</softwareName>
		<softwareOperation>ONLINE_SERVICE</softwareOperation>
		<softwareMainVersion>{version}</softwareMainVersion>
		<softwareDevName>Wisetech Global Limited</softwareDevName>
		<softwareDevContact>eInvoice.Hungary@WisetechGlobal.com</softwareDevContact>
		<softwareDevCountryCode>AU</softwareDevCountryCode>
		<softwareDevTaxNumber>41065894724</softwareDevTaxNumber>
	</software>
	<exchangeToken>$$exchangeTokenReplaceMe$$</exchangeToken>
	<invoiceOperations>
		<compressedContent>false</compressedContent>
		<invoiceOperation>
			<index>1</index>
			<invoiceOperation>MODIFY</invoiceOperation>
			<invoiceData>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48SW52b2ljZURhdGEgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9kYXRhIiB4bWxuczpuczI9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9iYXNlIj48aW52b2ljZU51bWJlcj5IVU5OMTwvaW52b2ljZU51bWJlcj48aW52b2ljZUlzc3VlRGF0ZT48L2ludm9pY2VJc3N1ZURhdGU+PGNvbXBsZXRlbmVzc0luZGljYXRvcj5mYWxzZTwvY29tcGxldGVuZXNzSW5kaWNhdG9yPjxpbnZvaWNlTWFpbj48aW52b2ljZT48aW52b2ljZVJlZmVyZW5jZT48b3JpZ2luYWxJbnZvaWNlTnVtYmVyPkFCQzEyMzwvb3JpZ2luYWxJbnZvaWNlTnVtYmVyPjxtb2RpZnlXaXRob3V0TWFzdGVyPnRydWU8L21vZGlmeVdpdGhvdXRNYXN0ZXI+PG1vZGlmaWNhdGlvbkluZGV4PjE8L21vZGlmaWNhdGlvbkluZGV4PjwvaW52b2ljZVJlZmVyZW5jZT48aW52b2ljZUhlYWQ+PHN1cHBsaWVySW5mbz48c3VwcGxpZXJBZGRyZXNzPjxuczI6c2ltcGxlQWRkcmVzcz48bnMyOmNvdW50cnlDb2RlIC8+PG5zMjpwb3N0YWxDb2RlPjAwMDA8L25zMjpwb3N0YWxDb2RlPjxuczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6c2ltcGxlQWRkcmVzcz48L3N1cHBsaWVyQWRkcmVzcz48L3N1cHBsaWVySW5mbz48Y3VzdG9tZXJJbmZvPjxjdXN0b21lclZhdFN0YXR1cz5QUklWQVRFX1BFUlNPTjwvY3VzdG9tZXJWYXRTdGF0dXM+PC9jdXN0b21lckluZm8+PGludm9pY2VEZXRhaWw+PGludm9pY2VDYXRlZ29yeT48L2ludm9pY2VDYXRlZ29yeT48aW52b2ljZURlbGl2ZXJ5RGF0ZT48L2ludm9pY2VEZWxpdmVyeURhdGU+PGN1cnJlbmN5Q29kZT48L2N1cnJlbmN5Q29kZT48ZXhjaGFuZ2VSYXRlPjwvZXhjaGFuZ2VSYXRlPjxwYXltZW50RGF0ZT48L3BheW1lbnREYXRlPjxpbnZvaWNlQXBwZWFyYW5jZT5VTktOT1dOPC9pbnZvaWNlQXBwZWFyYW5jZT48L2ludm9pY2VEZXRhaWw+PC9pbnZvaWNlSGVhZD48aW52b2ljZUxpbmVzPjxtZXJnZWRJdGVtSW5kaWNhdG9yPmZhbHNlPC9tZXJnZWRJdGVtSW5kaWNhdG9yPjxsaW5lPjxsaW5lTnVtYmVyPjE8L2xpbmVOdW1iZXI+PGxpbmVNb2RpZmljYXRpb25SZWZlcmVuY2U+PGxpbmVOdW1iZXJSZWZlcmVuY2U+MTwvbGluZU51bWJlclJlZmVyZW5jZT48bGluZU9wZXJhdGlvbj5DUkVBVEU8L2xpbmVPcGVyYXRpb24+PC9saW5lTW9kaWZpY2F0aW9uUmVmZXJlbmNlPjxsaW5lRXhwcmVzc2lvbkluZGljYXRvcj5mYWxzZTwvbGluZUV4cHJlc3Npb25JbmRpY2F0b3I+PGxpbmVOYXR1cmVJbmRpY2F0b3I+U0VSVklDRTwvbGluZU5hdHVyZUluZGljYXRvcj48bGluZURlc2NyaXB0aW9uPjwvbGluZURlc2NyaXB0aW9uPjxsaW5lQW1vdW50c05vcm1hbD48bGluZU5ldEFtb3VudERhdGE+PGxpbmVOZXRBbW91bnQ+MTAwLjAwPC9saW5lTmV0QW1vdW50PjxsaW5lTmV0QW1vdW50SFVGPjwvbGluZU5ldEFtb3VudEhVRj48L2xpbmVOZXRBbW91bnREYXRhPjxsaW5lVmF0UmF0ZSAvPjwvbGluZUFtb3VudHNOb3JtYWw+PC9saW5lPjwvaW52b2ljZUxpbmVzPjxpbnZvaWNlU3VtbWFyeT48c3VtbWFyeU5vcm1hbD48aW52b2ljZU5ldEFtb3VudD4xMDAuMDA8L2ludm9pY2VOZXRBbW91bnQ+PGludm9pY2VOZXRBbW91bnRIVUY+MC4wMDwvaW52b2ljZU5ldEFtb3VudEhVRj48aW52b2ljZVZhdEFtb3VudD4wLjAwPC9pbnZvaWNlVmF0QW1vdW50PjxpbnZvaWNlVmF0QW1vdW50SFVGPjAuMDA8L2ludm9pY2VWYXRBbW91bnRIVUY+PC9zdW1tYXJ5Tm9ybWFsPjxzdW1tYXJ5R3Jvc3NEYXRhPjxpbnZvaWNlR3Jvc3NBbW91bnQ+PC9pbnZvaWNlR3Jvc3NBbW91bnQ+PGludm9pY2VHcm9zc0Ftb3VudEhVRj48L2ludm9pY2VHcm9zc0Ftb3VudEhVRj48L3N1bW1hcnlHcm9zc0RhdGE+PC9pbnZvaWNlU3VtbWFyeT48L2ludm9pY2U+PC9pbnZvaWNlTWFpbj48L0ludm9pY2VEYXRhPg==</invoiceData>
			<electronicInvoiceHash cryptoType="SHA3-512">$$invoiceHashReplaceMe$$</electronicInvoiceHash>
		</invoiceOperation>
	</invoiceOperations>
</ManageInvoiceRequest>
""".Trim();

				var expectedInvoiceData = """
<?xml version="1.0" encoding="utf-8"?>
<InvoiceData
	xmlns="http://schemas.nav.gov.hu/OSA/3.0/data"
	xmlns:ns2="http://schemas.nav.gov.hu/OSA/3.0/base">
	<invoiceNumber>HUNN1</invoiceNumber>
	<invoiceIssueDate></invoiceIssueDate>
	<completenessIndicator>false</completenessIndicator>
	<invoiceMain>
		<invoice>
			<invoiceReference>
				<originalInvoiceNumber>ABC123</originalInvoiceNumber>
				<modifyWithoutMaster>true</modifyWithoutMaster>
				<modificationIndex>1</modificationIndex>
			</invoiceReference>
			<invoiceHead>
				<supplierInfo>
					<supplierAddress>
						<ns2:simpleAddress>
							<ns2:countryCode />
							<ns2:postalCode>0000</ns2:postalCode>
							<ns2:additionalAddressDetail></ns2:additionalAddressDetail>
						</ns2:simpleAddress>
					</supplierAddress>
				</supplierInfo>
				<customerInfo>
					<customerVatStatus>PRIVATE_PERSON</customerVatStatus>
				</customerInfo>
				<invoiceDetail>
					<invoiceCategory></invoiceCategory>
					<invoiceDeliveryDate></invoiceDeliveryDate>
					<currencyCode></currencyCode>
					<exchangeRate></exchangeRate>
					<paymentDate></paymentDate>
					<invoiceAppearance>UNKNOWN</invoiceAppearance>
				</invoiceDetail>
			</invoiceHead>
			<invoiceLines>
				<mergedItemIndicator>false</mergedItemIndicator>
				<line>
					<lineNumber>1</lineNumber>
					<lineModificationReference>
						<lineNumberReference>1</lineNumberReference>
						<lineOperation>CREATE</lineOperation>
					</lineModificationReference>
					<lineExpressionIndicator>false</lineExpressionIndicator>
					<lineNatureIndicator>SERVICE</lineNatureIndicator>
					<lineDescription></lineDescription>
					<lineAmountsNormal>
						<lineNetAmountData>
							<lineNetAmount>100.00</lineNetAmount>
							<lineNetAmountHUF></lineNetAmountHUF>
						</lineNetAmountData>
						<lineVatRate />
					</lineAmountsNormal>
				</line>
			</invoiceLines>
			<invoiceSummary>
				<summaryNormal>
					<invoiceNetAmount>100.00</invoiceNetAmount>
					<invoiceNetAmountHUF>0.00</invoiceNetAmountHUF>
					<invoiceVatAmount>0.00</invoiceVatAmount>
					<invoiceVatAmountHUF>0.00</invoiceVatAmountHUF>
				</summaryNormal>
				<summaryGrossData>
					<invoiceGrossAmount></invoiceGrossAmount>
					<invoiceGrossAmountHUF></invoiceGrossAmountHUF>
				</summaryGrossData>
			</invoiceSummary>
		</invoice>
	</invoiceMain>
</InvoiceData>
""".Trim();

				#endregion

				stream.Position = 0;
				var actualXML = new StreamReader(stream).ReadToEnd();

				AssertEquals(new ZDateTime(2024, 01, 01), originalInvoice.AH_InvoiceDate);
				AssertEquals(new ZDateTime(2024, 01, 11), originalInvoice.AH_PostDate);
				AssertEquals(new ZDateTime(2024, 01, 01), transactionInfo.OriginalReference.OriginalTransactionDate);
				this.AssertXMLEqualsIgnoreChildOrder("PayloadWriter shold create encoded invoice data", expectedInvoiceData, GetXMLInvoiceData(actualXML));
				this.AssertXMLEqualsIgnoreChildOrder("PayloadWriter shold create XML", expectedXML, actualXML);
			}
		}

		public void TestWritePayloadToStream_WithoutInvoice()
		{
			// Arrange
			using (var stream = new MemoryStream())
			{
				var eInvoicingBatch = GetAccBatch(null);
				var transactionBatch = GetTransactionBatch();
				var payloadWriter = GetTestWriter() as ITransactionBatchToPayloadWriter;

				// Act
				payloadWriter.WritePayloadToStream(transactionBatch, stream, HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, eInvoicingBatch, new Logger(), new Logger());

				// Assert
				var version = new EnterpriseInformationRetriever().VersionNumber;
				var expected = $"""
<?xml version="1.0" encoding="utf-8"?>
<ManageInvoiceRequest
	xmlns:common="http://schemas.nav.gov.hu/NTCA/1.0/common"
	xmlns="http://schemas.nav.gov.hu/OSA/3.0/api">
	<common:header>
		<common:requestId>$$requestIdReplaceMe$$</common:requestId>
		<common:timestamp>$$timestampReplaceMe$$</common:timestamp>
		<common:requestVersion>3.0</common:requestVersion>
		<common:headerVersion>1.0</common:headerVersion>
	</common:header>
	<common:user>
		<common:requestSignature cryptoType="SHA3-512">$$requestSignatureReplaceMe$$</common:requestSignature>
	</common:user>
	<software>
		<softwareId>AU658947CARGOWISE1</softwareId>
		<softwareName>CargoWise</softwareName>
		<softwareOperation>ONLINE_SERVICE</softwareOperation>
		<softwareMainVersion>{version}</softwareMainVersion>
		<softwareDevName>Wisetech Global Limited</softwareDevName>
		<softwareDevContact>eInvoice.Hungary@WisetechGlobal.com</softwareDevContact>
		<softwareDevCountryCode>AU</softwareDevCountryCode>
		<softwareDevTaxNumber>41065894724</softwareDevTaxNumber>
	</software>
	<exchangeToken>$$exchangeTokenReplaceMe$$</exchangeToken>
	<invoiceOperations>
		<compressedContent>false</compressedContent>
		<invoiceOperation>
			<index>1</index>
			<invoiceOperation>CREATE</invoiceOperation>
			<invoiceData>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48SW52b2ljZURhdGEgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9kYXRhIiB4bWxuczpuczI9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9iYXNlIj48aW52b2ljZU51bWJlcj5IVU5OMTwvaW52b2ljZU51bWJlcj48aW52b2ljZUlzc3VlRGF0ZT48L2ludm9pY2VJc3N1ZURhdGU+PGNvbXBsZXRlbmVzc0luZGljYXRvcj5mYWxzZTwvY29tcGxldGVuZXNzSW5kaWNhdG9yPjxpbnZvaWNlTWFpbj48aW52b2ljZT48aW52b2ljZUhlYWQ+PHN1cHBsaWVySW5mbz48c3VwcGxpZXJBZGRyZXNzPjxuczI6c2ltcGxlQWRkcmVzcz48bnMyOmNvdW50cnlDb2RlIC8+PG5zMjpwb3N0YWxDb2RlPjAwMDA8L25zMjpwb3N0YWxDb2RlPjxuczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6c2ltcGxlQWRkcmVzcz48L3N1cHBsaWVyQWRkcmVzcz48L3N1cHBsaWVySW5mbz48Y3VzdG9tZXJJbmZvPjxjdXN0b21lclZhdFN0YXR1cz5PVEhFUjwvY3VzdG9tZXJWYXRTdGF0dXM+PGN1c3RvbWVyQWRkcmVzcz48bnMyOnNpbXBsZUFkZHJlc3M+PG5zMjpwb3N0YWxDb2RlPjAwMDA8L25zMjpwb3N0YWxDb2RlPjxuczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6YWRkaXRpb25hbEFkZHJlc3NEZXRhaWw+PC9uczI6c2ltcGxlQWRkcmVzcz48L2N1c3RvbWVyQWRkcmVzcz48L2N1c3RvbWVySW5mbz48aW52b2ljZURldGFpbD48aW52b2ljZUNhdGVnb3J5PjwvaW52b2ljZUNhdGVnb3J5PjxpbnZvaWNlRGVsaXZlcnlEYXRlPjIwMjQtMDItMDQ8L2ludm9pY2VEZWxpdmVyeURhdGU+PGN1cnJlbmN5Q29kZT48L2N1cnJlbmN5Q29kZT48ZXhjaGFuZ2VSYXRlPjwvZXhjaGFuZ2VSYXRlPjxwYXltZW50RGF0ZT48L3BheW1lbnREYXRlPjxpbnZvaWNlQXBwZWFyYW5jZT5FTEVDVFJPTklDPC9pbnZvaWNlQXBwZWFyYW5jZT48L2ludm9pY2VEZXRhaWw+PC9pbnZvaWNlSGVhZD48aW52b2ljZUxpbmVzPjxtZXJnZWRJdGVtSW5kaWNhdG9yPmZhbHNlPC9tZXJnZWRJdGVtSW5kaWNhdG9yPjxsaW5lPjxsaW5lTnVtYmVyPjE8L2xpbmVOdW1iZXI+PGxpbmVFeHByZXNzaW9uSW5kaWNhdG9yPmZhbHNlPC9saW5lRXhwcmVzc2lvbkluZGljYXRvcj48bGluZU5hdHVyZUluZGljYXRvcj5TRVJWSUNFPC9saW5lTmF0dXJlSW5kaWNhdG9yPjxsaW5lRGVzY3JpcHRpb24+PC9saW5lRGVzY3JpcHRpb24+PGxpbmVBbW91bnRzTm9ybWFsPjxsaW5lTmV0QW1vdW50RGF0YT48bGluZU5ldEFtb3VudD4xMDAuMDA8L2xpbmVOZXRBbW91bnQ+PGxpbmVOZXRBbW91bnRIVUY+PC9saW5lTmV0QW1vdW50SFVGPjwvbGluZU5ldEFtb3VudERhdGE+PGxpbmVWYXRSYXRlIC8+PC9saW5lQW1vdW50c05vcm1hbD48L2xpbmU+PC9pbnZvaWNlTGluZXM+PGludm9pY2VTdW1tYXJ5PjxzdW1tYXJ5Tm9ybWFsPjxpbnZvaWNlTmV0QW1vdW50PjEwMC4wMDwvaW52b2ljZU5ldEFtb3VudD48aW52b2ljZU5ldEFtb3VudEhVRj4wLjAwPC9pbnZvaWNlTmV0QW1vdW50SFVGPjxpbnZvaWNlVmF0QW1vdW50PjAuMDA8L2ludm9pY2VWYXRBbW91bnQ+PGludm9pY2VWYXRBbW91bnRIVUY+MC4wMDwvaW52b2ljZVZhdEFtb3VudEhVRj48L3N1bW1hcnlOb3JtYWw+PHN1bW1hcnlHcm9zc0RhdGE+PGludm9pY2VHcm9zc0Ftb3VudD48L2ludm9pY2VHcm9zc0Ftb3VudD48aW52b2ljZUdyb3NzQW1vdW50SFVGPjwvaW52b2ljZUdyb3NzQW1vdW50SFVGPjwvc3VtbWFyeUdyb3NzRGF0YT48L2ludm9pY2VTdW1tYXJ5PjwvaW52b2ljZT48L2ludm9pY2VNYWluPjwvSW52b2ljZURhdGE+</invoiceData>
			<electronicInvoiceHash cryptoType="SHA3-512">$$invoiceHashReplaceMe$$</electronicInvoiceHash>
		</invoiceOperation>
	</invoiceOperations>
</ManageInvoiceRequest>
""".Trim();
				stream.Position = 0;
				var actual = new StreamReader(stream).ReadToEnd();
				this.AssertXMLEqualsIgnoreChildOrder("Exported XML should match XML file", expected, actual);
			}
		}

		public void TestInvoiceAppearanceFromIdmCusCodeForTransactionBatch_EDI()
		{
			PerformTestInvoiceAppearanceFromIdmCusCodeForTransactionBatch(InvoiceDeliveryMethod.EDI);
		}

		public void TestInvoiceAppearanceFromIdmCusCodeForTransactionBatch_ELECTRONIC()
		{
			PerformTestInvoiceAppearanceFromIdmCusCodeForTransactionBatch(InvoiceDeliveryMethod.ELECTRONIC);
		}

		public void TestInvoiceAppearanceFromIdmCusCodeForTransactionBatch_PAPER()
		{
			PerformTestInvoiceAppearanceFromIdmCusCodeForTransactionBatch(InvoiceDeliveryMethod.PAPER);
		}

		public void TestInvoiceAppearanceFromIdmCusCodeForTransactionBatch_UNKNOWN()
		{
			PerformTestInvoiceAppearanceFromIdmCusCodeForTransactionBatch(InvoiceDeliveryMethod.UNKNOWN);
		}

		public void PerformTestInvoiceAppearanceFromIdmCusCodeForTransactionBatch(InvoiceDeliveryMethod invoiceDeliveryMethod)
		{
			Helper.SetupControlAccounts();

			// Arrange
			Helper.AddCustomsCodeForCountryIfMissing(TestObjectCreator.Debtor, CountryCodes.Hungary,
				HungaryOrgCusCodeInfo.OrgCusCodes.IDM, invoiceDeliveryMethod.ToString());
			Factory.Save();

			using (var stream = new MemoryStream())
			{
				var transactionBatch = GetTransactionBatch();
				var transactionInfo = transactionBatch.TransactionCollection[0];
				transactionInfo.OriginalReference = new OriginalReference();

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), $"001{invoiceDeliveryMethod}", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				Factory.Save();

				var eInvoicingBatch = GetAccBatch(arInvoice);
				var payloadWriter = GetTestWriter() as ITransactionBatchToPayloadWriter;

				// Act
				payloadWriter.WritePayloadToStream(transactionBatch, stream, HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, eInvoicingBatch, new Logger(), new Logger());

				// Assert
				stream.Position = 0;
				var actualXML = new StreamReader(stream).ReadToEnd();

				var payloadXmlDocument = new XmlDocument();
				payloadXmlDocument.LoadXml(actualXML);
				var invoiceDataXmlDocument = new XmlDocument();
				invoiceDataXmlDocument.LoadXml(payloadXmlDocument.GetElementsByTagName("invoiceData")[0].InnerXml.ToUTF8FromBase64());

				AssertEquals("Generated completenessIndicator should match [false]", "false", invoiceDataXmlDocument.GetElementsByTagName("completenessIndicator")[0].InnerXml);
				AssertEquals("Generated invoiceAppearance should match the provided invoiceDeliveryMethod", invoiceDeliveryMethod.ToString(), invoiceDataXmlDocument.GetElementsByTagName("invoiceAppearance")[0].InnerXml);
			}
		}

		public void TestInvoiceAppearanceFromInvalidStringIdmCusCode()
		{
			TestInvoiceAppearanceFromInvalidIdmCusCodeForTransactionBatch("Invalid");
		}

		public void TestInvoiceAppearanceFromInvalidIntegerIdmCusCode()
		{
			TestInvoiceAppearanceFromInvalidIdmCusCodeForTransactionBatch("123");
		}

		public void TestInvoiceAppearanceFromInvalidIdmCusCodeForTransactionBatch(string invalidIdmCode)
		{
			Helper.SetupControlAccounts();

			// Arrange
			Helper.AddCustomsCodeForCountryIfMissing(TestObjectCreator.Debtor, CountryCodes.Hungary, HungaryOrgCusCodeInfo.OrgCusCodes.IDM, invalidIdmCode);
			Factory.Save();

			using (var stream = new MemoryStream())
			{
				var transactionBatch = GetTransactionBatch();
				var transactionInfo = transactionBatch.TransactionCollection[0];
				transactionInfo.OriginalReference = new OriginalReference();

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				Factory.Save();

				var eInvoicingBatch = GetAccBatch(arInvoice);
				var payloadWriter = GetTestWriter() as ITransactionBatchToPayloadWriter;

				// Assert
				AssertExceptionThrown<ArgumentException>("For INVALID IDM type: ArgumentException is expected", $"Invalid Invoice Delivery Method {invalidIdmCode} is specified.", () =>
				{
				// Act
					payloadWriter.WritePayloadToStream(transactionBatch, stream, HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, eInvoicingBatch, new Logger(), new Logger());
				});
			}
		}

		public void TestInvoiceAppearanceForTransactionBatchForNaturalPerson()
		{
			// Arrange
			using (var stream = new MemoryStream())
			{
				var transactionBatch = GetTransactionBatch();
				var transactionInfo = transactionBatch.TransactionCollection[0];
				transactionInfo.OriginalReference = new OriginalReference();

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				arInvoice.Header.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				Factory.Save();

				var eInvoicingBatch = GetAccBatch(arInvoice);
				var payloadWriter = GetTestWriter() as ITransactionBatchToPayloadWriter;

				// Act
				payloadWriter.WritePayloadToStream(transactionBatch, stream, HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, eInvoicingBatch, new Logger(), new Logger());

				// Assert
				stream.Position = 0;
				var actualXML = new StreamReader(stream).ReadToEnd();

				var payloadXmlDocument = new XmlDocument();
				payloadXmlDocument.LoadXml(actualXML);

				var invoiceDataXmlDocument = new XmlDocument();
				invoiceDataXmlDocument.LoadXml(payloadXmlDocument.GetElementsByTagName("invoiceData")[0].InnerXml.ToUTF8FromBase64());

				var generatedCompletenessIndicatorValue = invoiceDataXmlDocument.GetElementsByTagName("completenessIndicator")[0].InnerXml;
				var generatedInvoiceAppearanceValue = invoiceDataXmlDocument.GetElementsByTagName("invoiceAppearance")[0].InnerXml;

				AssertEquals("Generated completenessIndicator should match [false]", "false", generatedCompletenessIndicatorValue);
				AssertEquals("Generated invoiceAppearance should match the provided invoiceDeliveryMethod", nameof(InvoiceDeliveryMethod.UNKNOWN), generatedInvoiceAppearanceValue);
			}
		}

		#region Implementation

		AccEInvoicingBatch GetAccBatch(InvoicingBase invoice)
		{
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var pivot = batch.TransactionPivots.AddNew();
			if (invoice != null)
			{
				pivot.AIP_ParentID = invoice.PK;
			}
			return batch;
		}

		TransactionBatch GetTransactionBatch()
		{
			var branch = Factory.NewCompanyAndBranchWith("HUN", "HUN", Core.Constants.CountryCodes.Hungary);
			Factory.Save();

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.Branch = new Branch() { Code = "HUN" };
			transactionInfo.Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transactionInfo.TransactionType = TransactionType.INV;
			transactionInfo.Number = "HUNN1";

			var postingJournal = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSAmount = 100,
				TaxDate = new ZDate(2024, 02, 04),
				VATTaxID = new TaxID
				{
					TaxCode = "VAT",
					TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = AccTaxRate.Types.Rated }
				}
			};
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>() { postingJournal });

			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);
			return transactionBatch;
		}

		string GetXMLInvoiceData(string xml)
		{
			var endIndex = xml.IndexOf("</invoiceData>");
			var startIndex = xml.IndexOf("<invoiceData>") + 13;
			var rawInvoiceData = xml.Remove(endIndex).Substring(startIndex);
			var result = Encoding.UTF8.GetString(Convert.FromBase64String(rawInvoiceData));
			return result;
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected EInvoicingTestHelper Helper => helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper helper;

		#endregion
	}
}
