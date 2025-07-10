using System;
using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing.Fiji;
using Enterprise.Accounting.Business.EInvoicing.TaxCore;
using Enterprise.MasterFiles.Business.CountryCompliance;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	public class TaxCoreEInvoiceAuthorisationRecordCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var arInvoice = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
			arInvoice.AH_TransactionReference = "REF001";
			Factory.Save();

			var response = CreateResponse();
			var creator = GetAuthorizationRecordCreator();
			var createdRecord = creator.Create(Factory, response, arInvoice.PK);
			Factory.Save();

			var record = Factory.Load<FijiAccTransactionHeaderAuthorisationRecord>(createdRecord.PK);
			AssertCreatedRecord(response, record, arInvoice.PK);
		}

		public void TestCreate_OnlyWithMandatoryValues()
		{
			var arInvoice = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
			arInvoice.AH_TransactionReference = "REF001";
			Factory.Save();

			var response = CreateResponse(true);
			var creator = GetAuthorizationRecordCreator();
			var createdRecord = creator.Create(Factory, response, arInvoice.PK);
			Factory.Save();

			DateTimeOffset.TryParse(response.DT, out var expectedResponseDate);
			var record = Factory.Load<FijiAccTransactionHeaderAuthorisationRecord>(createdRecord.PK);
			AssertEquals("AHF_AuthorisationData", 0, record.AHF_AuthorisationData.Length);
			AssertEquals("AHF_Counter", response.InvoiceCounter, record.AHF_Counter);
			AssertEquals("AHF_DateTime", expectedResponseDate, record.AHF_DateTime);
			AssertEquals("AHF_Number", response.InvoiceNumber, record.AHF_Number);
			AssertEquals("AHF_ParentId", arInvoice.PK, record.AHF_ParentId);
			AssertEquals("AHF_ParentTableCode", "AH", record.AHF_ParentTableCode);
			AssertEquals("AHF_PublicKey", 0, record.AHF_PublicKey.Length);
			AssertEquals("AHF_RecordType", AuthorizationRecordTypeCode, record.AHF_RecordType);
			AssertEquals("AHF_VerificationUrl", response.VerificationUrl, record.AHF_VerificationUrl);
			AssertEquals("BusinessName", response.BusinessName, record.BusinessName);
			AssertEquals("LocationName", response.LocationName, record.LocationName);
			AssertEquals("Address", response.Address, record.Address);
			AssertEquals("District", response.District, record.District);
		}

		public void TestDateFormat()
		{
			foreach (var dateOffsets in new[] {
										new { InvoiceNumber = "001", ResponseDate = "2019-07-16T10:16:56.5838665+12:00", ExpectedDateOffset = DateTimeOffset.Parse("2019-07-16T10:16:56.5838665+12:00") },
										new { InvoiceNumber = "002", ResponseDate = "2019-07-16T10:16:56.4325-5:00", ExpectedDateOffset = DateTimeOffset.Parse("2019-07-16T10:16:56.4325-5:00") },
										new { InvoiceNumber = "003", ResponseDate = "2011-08-12T20:17:46.3842333Z", ExpectedDateOffset = DateTimeOffset.Parse("2011-08-12T20:17:46.3842333Z") },
									})
			{
				var arInvoice = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), dateOffsets.InvoiceNumber, ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
				arInvoice.AH_TransactionReference = "REF001";
				Factory.Save();

				var response = CreateResponse(true, dateOffsets.ResponseDate);
				var creator = GetAuthorizationRecordCreator();
				var createdRecord = creator.Create(Factory, response, arInvoice.PK);
				Factory.Save();

				var record = Factory.Load<FijiAccTransactionHeaderAuthorisationRecord>(createdRecord.PK);
				AssertEquals("AHF_AuthorisationData", 0, record.AHF_AuthorisationData.Length);
				AssertEquals("AHF_Counter", response.InvoiceCounter, record.AHF_Counter);
				AssertEquals("AHF_DateTime", dateOffsets.ExpectedDateOffset, record.AHF_DateTime);
				AssertEquals("AHF_Number", response.InvoiceNumber, record.AHF_Number);
				AssertEquals("AHF_ParentId", arInvoice.PK, record.AHF_ParentId);
				AssertEquals("AHF_ParentTableCode", "AH", record.AHF_ParentTableCode);
				AssertEquals("AHF_PublicKey", 0, record.AHF_PublicKey.Length);
				AssertEquals("AHF_RecordType", AuthorizationRecordTypeCode, record.AHF_RecordType);
				AssertEquals("AHF_VerificationUrl", response.VerificationUrl, record.AHF_VerificationUrl);
				AssertEquals("BusinessName", response.BusinessName, record.BusinessName);
				AssertEquals("LocationName", response.LocationName, record.LocationName);
				AssertEquals("Address", response.Address, record.Address);
				AssertEquals("District", response.District, record.District);
			}
		}

		public void TestInvalidDateOffsets()
		{
			var arInvoice = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, ObjectCreator.Debtor, ObjectCreator.CC1.PK);
			arInvoice.AH_TransactionReference = "REF001";
			Factory.Save();

			var response = CreateResponse(true, "2019-14-16T10:16:56.5838665+25:00");
			var creator = GetAuthorizationRecordCreator();
			AssertExceptionThrown<InvalidZDateTimeResultException>("Error Message"
				, FormattableString.Invariant($"Failed to set AHF_DateTime, as '2019-14-16T10:16:56.5838665+25:00' cannot be converted to a DateTimeOffset value.")
				, () => creator.Create(Factory, response, arInvoice.PK));
		}

		protected virtual void AssertCreatedRecord(TaxCoreEInvoiceResponse response, TaxCoreAccTransactionHeaderAuthorisationRecord record, ZGuid transactionPK)
		{
			var expectedAutorizationData = ZBlob.FromUTF8(response.InternalData);
			AssertEquals("AHF_AuthorisationData", expectedAutorizationData, record.AHF_AuthorisationData);
			AssertEquals("AHF_Counter", response.InvoiceCounter, record.AHF_Counter);
			AssertEquals("AHF_DateTime", response.DT, record.AHF_DateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture));
			AssertEquals("AHF_Number", response.InvoiceNumber, record.AHF_Number);
			AssertEquals("AHF_Number", transactionPK, record.AHF_ParentId);
			AssertEquals("AHF_ParentTableCode", "AH", record.AHF_ParentTableCode);
			var expectedPublicKey = ZBlob.FromUTF8(response.Signature);
			AssertEquals("AHF_PublicKey", expectedPublicKey, record.AHF_PublicKey);
			AssertEquals("AHF_RecordType", AuthorizationRecordTypeCode, record.AHF_RecordType);
			AssertEquals("AHF_VerificationUrl", response.VerificationUrl, record.AHF_VerificationUrl);
			AssertEquals("BusinessName", response.BusinessName, record.BusinessName);
			AssertEquals("LocationName", response.LocationName, record.LocationName);
			AssertEquals("Address", response.Address, record.Address);
			AssertEquals("District", response.District, record.District);
		}

		TaxCoreEInvoiceResponse CreateResponse(bool setNullToNonMandatoryProperties = false)
		{
			return CreateResponse(setNullToNonMandatoryProperties, "2019-07-16T10:16:56.5838665+12:00");
		}

		TaxCoreEInvoiceResponse CreateResponse(bool setNullToNonMandatoryProperties, string responseDateString)
		{
			var response = new TaxCoreEInvoiceResponse()
			{
				Address = "Southern Cross Rd, Suva, Fiji",
				BusinessName = "WiseBusiness Fiji",
				District = "Suva",
				DT = responseDateString,
				Hash = setNullToNonMandatoryProperties ? string.Empty : "oyIN9/+O+ytyS0Twp8LsTQ==",
				InternalData = setNullToNonMandatoryProperties ? string.Empty : SampleInternalData,
				InvoiceCounter = "47/50NS",
				InvoiceCounterExtension = "NS",
				InvoiceNumber = "CA6Y8WJJ-T5UX48AJ-50",
				VerificationUrl = SampleVerificationUrl,
				Journal = null,
				LocationName = "WiseLocation",
				Messages = "Success",
				MRC = "99-0100-T5UX48AJ",
				RequestedBy = "CA6Y8WJJ",
				Signature = setNullToNonMandatoryProperties ? string.Empty : SampleSignature,
				SignedBy = "T5UX48AJ",
				TIN = "wisetech",
				TotalAmount = 3256M,
				TotalCounter = 50,
				TransactionTypeCounter = 47,
				VerificationQRCode = null
			};

			response.TaxItems.Add(
						new TaxCoreEInvoiceResponse.TaxItem()
						{
							Amount = 325.14M,
							CategoryName = "VAT",
							Label = "A",
							Rate = 9.0M,
						});

			return response;
		}

		ITaxCoreEInvoiceAuthorisationRecordCreator GetAuthorizationRecordCreator() => TaxCoreEInvoicingResponseObjectFactory.GetICountryEInvoicingResponseObjectFactory(CountryCode).GetAuthorisationRecordCreator();

		string SampleInternalData => @"clA/83P92nn4Y1ZuH6yrcTxmMwgiiY7MWSiOJkZc91GADQMtwm92L9YbSoaXTAPgzveN92DnOYT/QbCfrsvsPZcO/UAd9KulkS/ORukyZfrCsM5Vnwl9au8OkZgkiani0ZlejADPWVLMgZVbqdZnlWNGDkCEecb9K1vGWtGHHgv0ZojoAnUi9RyzYzehNOHxUXzqjUsb75nPCNPqRktEItBYNZnuXhkSdP/a9Dx1xM6Hl5ag1OdtAL7my6VRmvdNfYQ6luiepZgYAIpD6Z9GT2YJW5ndVcYfqa+CpD3ZW2hyfVEkWy7fTTugYR9et3tI7qST1AFGLw36RI2ES1N1IA==";

		string SampleVerificationUrl => @"https://staging.vms.frcs.org.fj/v/?vl=AkNBNlk4V0pKVDVVWDQ4QUoyAAAALwAAALCoRQIAAAAAAAABa%2Fe10gcAAAZzdHJpbmdyUD%2Fzc%2F3aefhjVm4frKtxPGYzCCKJjsxZKI4mRlz3UYANAy3Cb3Yv1htKhpdMA%2BDO9433YOc5hP9BsJ%2Buy%2Bw9lw79QB30q6WRL85G6TJl%2BsKwzlWfCX1q7w6RmCSJqeLRmV6MAM9ZUsyBlVup1meVY0YOQIR5xv0rW8Za0YceC%2FRmiOgCdSL1HLNjN6E04fFRfOqNSxvvmc8I0%2BpGS0Qi0Fg1me5eGRJ0%2F9r0PHXEzoeXlqDU520AvubLpVGa9019hDqW6J6lmBgAikPpn0ZPZglbmd1Vxh%2Bpr4KkPdlbaHJ9USRbLt9NO6BhH163e0jupJPUAUYvDfpEjYRLU3UgC2bEmI%2FAUMuq%2FFAiV%2B8zHg2SzDe32yVh28OFptN7QWD%2FfBkV6ntKjnVrCy1Y9iSINqrhQ0kU8Xc8WNte7FFSVlTgvUmNJ8Ef0k9ADd7Qr9aVluc8K5Lm5uS8fvOEOKt%2FG6mwVzRHojVzw4WKHZ9Ls5YQ4E7heoC6oQSIfWzFzEMw%2Fin%2FGVRJJkbO4iZRmxRejxyEKySJhbFRJc8bamWQPmNXoASt2vELlmRfTQZES0mbUzWBFpQCu%2B%2BORL3cQYEktvhaV2XOZeRox9ynsWc6E%2Byrpx3n%2BxO1wodLekPoZqjb3LjO3tCTpsPqhRdrerdad1cQHPQSRJySsTfQSQAnLQ%3D%3D";

		string SampleSignature => @"C2bEmI/AUMuq/FAiV+8zHg2SzDe32yVh28OFptN7QWD/fBkV6ntKjnVrCy1Y9iSINqrhQ0kU8Xc8WNte7FFSVlTgvUmNJ8Ef0k9ADd7Qr9aVluc8K5Lm5uS8fvOEOKt/G6mwVzRHojVzw4WKHZ9Ls5YQ4E7heoC6oQSIfWzFzEMw/in/GVRJJkbO4iZRmxRejxyEKySJhbFRJc8bamWQPmNXoASt2vELlmRfTQZES0mbUzWBFpQCu++ORL3cQYEktvhaV2XOZeRox9ynsWc6E+yrpx3n+xO1wodLekPoZqjb3LjO3tCTpsPqhRdrerdad1cQHPQSRJySsTfQSQAnLQ==";

		ZString AuthorizationRecordTypeCode => AccTransactionHeaderAuthorisationRecordTypes.Fiji;

		ZString CountryCode => CountryCodes.Fiji;

		protected TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
