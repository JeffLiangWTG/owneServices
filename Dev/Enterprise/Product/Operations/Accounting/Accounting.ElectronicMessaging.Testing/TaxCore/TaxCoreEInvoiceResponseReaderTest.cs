using System;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	public class TaxCoreEInvoiceResponseReaderTest : TestCase
	{
		public void TestRead_EmptyString()
		{
			var notifications = new NotificationBuffer();
			var reader = new TaxCoreEInvoiceResponseReader() as ITaxCoreEInvoiceResponseReader;
			AssertExceptionThrown<ArgumentException>(() => reader.Read(string.Empty));

			reader = new TaxCoreEInvoiceResponseReader();
			AssertExceptionThrown<FormatException>(() => reader.Read("Some string"));
		}

		public void TestRead_AllRequiredFields()
		{
			var jsonBuilder = new ZStringBuilder();
			foreach (var field in TaxCoreEInvoiceResponseRequiredFields)
			{
				jsonBuilder.Append(field);
			}

			var minimumJson = GetJsonString(jsonBuilder);
			var reader = new TaxCoreEInvoiceResponseReader() as ITaxCoreEInvoiceResponseReader;
			TaxCoreEInvoiceResponse response = null;
			AssertNoExceptionThrown(() => response = reader.Read(Convert.ToBase64String(Encoding.UTF8.GetBytes(minimumJson))));
			AssertNotNull("Json object is not null when all required fields are present", response);
		}

		public void TestRead_InvalidString()
		{
			var jsonBuilder = new ZStringBuilder();
			foreach (var field in TaxCoreEInvoiceResponseRequiredFields)
			{
				jsonBuilder.Append(field);
			}
			jsonBuilder.Append("'District'''"); // Invalid field format missing :

			var invalidJson = GetJsonString(jsonBuilder);
			TaxCoreEInvoiceResponse response = null;
			try
			{
				var reader = new TaxCoreEInvoiceResponseReader() as ITaxCoreEInvoiceResponseReader;
				response = reader.Read(Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidJson)));
			}
			catch (DeserializationException ex)
			{
				AssertContains(invalidJson, ex.Message);
				AssertNull(response);
			}
			catch (Exception ex)
			{
				Assert(FormattableString.Invariant($"Error of type {ex.GetType()} is not suppose to be thrown"), false);
			}
		}

		public void TestTooLargeEInvoiceResponse()
		{
			var expectedErrorMessage =
@"E-Invoice is too large to process.
Received Response from govt.:
The page was not displayed because the request entity is too large.";

			TaxCoreEInvoiceResponse response = null;
			try
			{
				var reader = new TaxCoreEInvoiceResponseReader() as ITaxCoreEInvoiceResponseReader;
				response = reader.Read(Convert.ToBase64String(Encoding.UTF8.GetBytes("The page was not displayed because the request entity is too large.")));
			}
			catch (DeserializationException ex)
			{
				AssertContains(expectedErrorMessage, ex.Message);
				AssertNull(response);
			}
			catch (Exception ex)
			{
				Assert(FormattableString.Invariant($"Error of type {ex.GetType()} is not suppose to be thrown"), false);
			}
		}

		public void TestRead_AllPopulatedFields()
		{
			var jsonBuilder = new ZStringBuilder();
			foreach (var field in TaxCoreEInvoiceResponsePopulatedFields)
			{
				jsonBuilder.Append(field);
			}

			var minimumJson = GetJsonString(jsonBuilder);

			AssertForAllPopulatedFields(minimumJson);
		}

		public void TestForCompleteV3Payload()
		{
			AssertForAllPopulatedFields(TaxCoreEInvoiceResponseVersion3);
		}

		void AssertForAllPopulatedFields(string minimumJson)
		{
			var reader = new TaxCoreEInvoiceResponseReader() as ITaxCoreEInvoiceResponseReader;
			var response = reader.Read(Convert.ToBase64String(Encoding.UTF8.GetBytes(minimumJson)));
			AssertNotNull("Json object is not null when all required fields are present", response);

			AssertEquals(nameof(response.RequestedBy), "ABC", response.RequestedBy);
			AssertEquals(nameof(response.SignedBy), "CDE", response.SignedBy);
			AssertEquals(nameof(response.DT), "2019-07-16T15:26:00Z", response.DT);
			AssertEquals(nameof(response.InvoiceCounter), "1", response.InvoiceCounter);
			AssertEquals(nameof(response.InvoiceCounterExtension), "X", response.InvoiceCounterExtension);
			AssertEquals(nameof(response.InvoiceNumber), "123", response.InvoiceNumber);
			AssertEquals(nameof(response.VerificationUrl), "Blah", response.VerificationUrl);
			AssertEquals(nameof(response.Messages), "Msg", response.Messages);
			AssertEquals(nameof(response.TotalCounter), 2, response.TotalCounter);
			AssertEquals(nameof(response.TransactionTypeCounter), 1, response.TransactionTypeCounter);
			AssertEquals(nameof(response.TotalAmount), 100m, response.TotalAmount);
			AssertEquals(nameof(response.InternalData), "345", response.InternalData);
			AssertEquals(nameof(response.Signature), "FFEE", response.Signature);
			AssertNotNull(nameof(response.TaxItems), response.TaxItems);
			AssertEquals(nameof(response.TaxItems.Count), 1, response.TaxItems.Count);
			AssertEquals(nameof(TaxCoreEInvoiceResponse.TaxItem.Label), "A", response.TaxItems[0].Label);
			AssertEquals(nameof(TaxCoreEInvoiceResponse.TaxItem.CategoryName), "VAT", response.TaxItems[0].CategoryName);
			AssertEquals(nameof(TaxCoreEInvoiceResponse.TaxItem.Rate), 12.5m, response.TaxItems[0].Rate);
			AssertEquals(nameof(TaxCoreEInvoiceResponse.TaxItem.Amount), 10m, response.TaxItems[0].Amount);
			AssertEquals(nameof(response.BusinessName), "Businessman", response.BusinessName);
			AssertEquals(nameof(response.LocationName), "Somewhere", response.LocationName);
			AssertEquals(nameof(response.Address), "Unknown Address", response.Address);
			AssertEquals(nameof(response.TIN), "01234567890123456789", response.TIN);
			AssertEquals(nameof(response.District), "QQQ", response.District);
			AssertEquals(nameof(response.MRC), "MakeCode-SW-Serial", response.MRC);
		}

		#region Implementation

		string[] TaxCoreEInvoiceResponseRequiredFields => new string[]
		{
			"'RequestedBy':''",
			"'SignedBy':''",
			"'DT':'2019-07-16T15:26:00Z'",
			"'IC':'0'",
			"'InvoiceCounterExtension':''",
			"'IN':''",
			"'VerificationUrl':''",
			"'TotalCounter':0",
			"'TransactionTypeCounter':0",
			"'TotalAmount':0",
			"'ID':''",
			"'S':''",
			"'TaxItems':[]",
		};

		string[] TaxCoreEInvoiceResponsePopulatedFields => new string[]
		{
			"'RequestedBy':'ABC'",
			"'SignedBy':'CDE'",
			"'DT':'2019-07-16T15:26:00Z'",
			"'IC':'1'",
			"'InvoiceCounterExtension':'X'",
			"'IN':'123'",
			"'VerificationUrl':'Blah'",
			"'Messages':'Msg'",
			"'TotalCounter':2",
			"'TransactionTypeCounter':1",
			"'TotalAmount':100.00",
			"'ID':'345'",
			"'S':'FFEE'",
			"'TaxItems':[{'Label':'A','CategoryName':'VAT','Rate':12.50,'Amount':10}]",
			"'BusinessName':'Businessman'",
			"'LocationName':'Somewhere'",
			"'Address':'Unknown Address'",
			"'TIN':'01234567890123456789'",
			"'District':'QQQ'",
			"'MRC':'MakeCode-SW-Serial'",
		};

		string GetJsonString(ZStringBuilder jsonBuilder) => ("{" + jsonBuilder.ToStringWithDelimiterBetweenAppends(",") + "}").Replace("'", "\"");

		#endregion

		const string TaxCoreEInvoiceResponseVersion3 = """
			{
				"requestedBy": "ABC",
				"sdcDateTime": "2019-07-16T15:26:00Z",
				"invoiceCounter": "1",
				"invoiceCounterExtension": "X",
				"invoiceNumber": "123",
				"taxItems": [
					{
						"categoryType": 0,
						"rateId": 0,
						"label": "A",
						"amount": 10.0000,
						"rate": 12.50,
						"categoryName": "VAT"
					}
				],
				"verificationUrl": "Blah",
				"verificationQRCode": null,
				"journal": null,
				"messages": "Msg",
				"signedBy": "CDE",
				"encryptedInternalData": "345",
				"signature": "FFEE",
				"totalCounter": 2,
				"transactionTypeCounter": 1,
				"totalAmount": 100.00,
				"taxGroupRevision": 4,
				"businessName": "Businessman",
				"tin": "01234567890123456789",
				"locationName": "Somewhere",
				"address": "Unknown Address",
				"district": "QQQ",
				"mrc": "MakeCode-SW-Serial"
			}
			""";
	}
}
