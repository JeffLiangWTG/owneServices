using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationApprovalDetails))]
	class TransactionPendingAllocationApprovalDetailsTest : ApprovalRequestDetailsTest<TransactionPendingAllocationApprovalDetails>
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesTransactionPendingAllocationApprovalDetails()
		{
			var details = (TransactionPendingAllocationApprovalDetails)GetNewBusinessObject();

			var localList = new List<string>
			{
				nameof(details.LocalExTaxAmount),
				nameof(details.LocalTaxAmount)
			};

			var osList = new List<string>
			{
				nameof(details.OSExTaxAmount),
				nameof(details.OSTaxAmount)
			};

			var exList = new List<string>
			{
				nameof(details.ExRate)
			};

			var tester = new DecimalPlacesAttributeTester(details);
			tester.CheckLocalCurrency(localList, nameof(details.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(details.OSCurrencyDecimals), nameof(details.CurrencyCode), details);
			tester.CheckExchangeRate(exList, nameof(details.ExchangeRateDecimalPlaces));
		}

		public void TestUniversalTransaction()
		{
			var details = (TransactionPendingAllocationApprovalDetails)GetNewBusinessObject();

			AssertNotNull(details.UniversalTransaction);
			AssertEquals("", details.UniversalTransaction.Branch);
			AssertEquals(0m, details.UniversalTransaction.LocalExVATAmount);
			AssertEquals("", details.UniversalTransaction.TransactionNumber);
			AssertEquals("", details.UniversalTransaction.OSCurrency);
			AssertNotNull(details.UniversalTransaction.Lines);
			AssertEquals(0, details.UniversalTransaction.Lines.Count);

			details.SourceXML = @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
	<Branch>
	  <Code>BER</Code>
	  <Name>EDI - Sydney Training</Name>
	</Branch>
	<LocalExVATAmount>-60.0000</LocalExVATAmount>
	<Number>11112222</Number>
	<OSCurrency>
	  <Code>USD</Code>
	  <Description>United States Dollar</Description>
	</OSCurrency>
	<PlaceOfSupply>
      <Location>
       <Code>JH</Code>
       <Description>Jharkhand</Description>
      </Location>
      <LocationType>
       <Code>STA</Code>
       <Description>State</Description>
      </LocationType>
    </PlaceOfSupply>
	<PostingJournalCollection>
	  <PostingJournal>
		<Department>
		  <Code>BRN</Code>
		  <Name>Branch</Name>
		</Department>
		<GLAccount>
		  <AccountCode>1010.10.10</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
		<LocalAmount>-60.0000</LocalAmount>
		<Sequence>2</Sequence>
		<PlaceOfSupply>
		  <Location>
		   <Code>JH</Code>
		   <Description>Jharkhand</Description>
		  </Location>
		  <LocationType>
		   <Code>STA</Code>
		   <Description>State</Description>
		  </LocationType>
		</PlaceOfSupply>
	  </PostingJournal>
	</PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";

			AssertEquals("BER", details.UniversalTransaction.Branch);
			AssertEquals(-60m, details.UniversalTransaction.LocalExVATAmount);
			AssertEquals("11112222", details.UniversalTransaction.TransactionNumber);
			AssertEquals("USD", details.UniversalTransaction.OSCurrency);
			AssertEquals("JH", details.UniversalTransaction.PlaceOfSupply);
			AssertEquals("STA", details.UniversalTransaction.PlaceOfSupplyType);
			AssertEquals(1, details.UniversalTransaction.Lines.Count);
			var line = details.UniversalTransaction.Lines[0];
			AssertEquals("BRN", line.Department);
			AssertEquals("1010.10.10", line.GLAccount);
			AssertEquals(-60m, line.LocalAmount);
			AssertEquals(2, line.Sequence);
			AssertEquals("JH", line.PlaceOfSupply);
			AssertEquals("STA", line.PlaceOfSupplyType);

			details.SourceXML = @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
	<LocalExVATAmount>10.0000</LocalExVATAmount>
	<Number>1111</Number>
	 <OSCurrency>
	  <Code>NZD</Code>
	</OSCurrency>
 </TransactionInfo>
</UniversalTransaction>";
			AssertEquals("", details.UniversalTransaction.Branch);
			AssertEquals(10m, details.UniversalTransaction.LocalExVATAmount);
			AssertEquals("1111", details.UniversalTransaction.TransactionNumber);
			AssertEquals("NZD", details.UniversalTransaction.OSCurrency);
			AssertEquals(0, details.UniversalTransaction.Lines.Count);
		}

		public override void TestIsPostingActionTheSame()
		{
			var a = (TransactionPendingAllocationApprovalDetails)GetNewBusinessObject();
			var b = (TransactionPendingAllocationApprovalDetails)GetNewBusinessObject();

			a.MaxAmountToApprove = 1;
			b.MaxAmountToApprove = 0;
			Assert("Posting action is the same for any details", a.IsPostingActionTheSame(b));
			Assert("Posting action is the same for any details", b.IsPostingActionTheSame(a));
		}

		public void TestOpertorEqualCore()
		{
			var a = (TransactionPendingAllocationApprovalDetails)GetNewBusinessObject();
			var b = (TransactionPendingAllocationApprovalDetails)GetNewBusinessObject();

			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.TransactionDate = ZDateTime.Today;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.TransactionDate = ZDateTime.Today;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.PostDate = ZDateTime.Today;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.PostDate = ZDateTime.Today;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.TransactionNumber = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.TransactionNumber = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.CreditorPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.CreditorPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.DueDate = ZDateTime.Today;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.DueDate = ZDateTime.Today;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.CurrencyCode = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.CurrencyCode = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.ExRate = 1;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.ExRate = 1;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.OSExTaxAmount = 1;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.OSExTaxAmount = 1;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.OSTaxAmount = 1;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.OSTaxAmount = 1;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.LocalExTaxAmount = 1;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.LocalExTaxAmount = 1;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.LocalTaxAmount = 1;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.LocalTaxAmount = 1;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.Description = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.Description = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.BranchPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.BranchPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.DepartmentPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.DepartmentPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.AddressPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.AddressPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.ContactPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.ContactPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.NumberOfSupportingDocuments = 1;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.NumberOfSupportingDocuments = 1;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.SourceXML = "Some xml";
			AssertEquals("SourceXML shouldn't be counted in comparison", true, a == b);
			AssertEquals("SourceXML shouldn't be counted in comparison", true, b == a);

			a.IsCrossLedgerImportFromXML = true;
			AssertEquals("IsCrossLedgerImportFromXML shouldn't be counted in comparison", true, a == b);
			AssertEquals("IsCrossLedgerImportFromXML shouldn't be counted in comparison", true, b == a);

			a.PlaceOfSupply = "DL";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.PlaceOfSupply = "DL";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.PlaceOfSupplyType = "STA";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.PlaceOfSupplyType = "STA";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);
		}

		public void TestCopyFromCore()
		{
			var details = (TransactionPendingAllocationApprovalDetails)GetNewBusinessObject();

			details.MaxAmountToApprove = 11M;
			details.TransactionDate = ZDateTime.Today.AddDays(-3);
			details.PostDate = ZDateTime.Today.AddDays(-1);
			details.TransactionNumber = "INV12";
			details.CreditorPK = new ZGuid("00000000-0000-0000-0000-000000000000");
			details.DueDate = ZDateTime.Today.AddDays(4);
			details.CurrencyCode = "USD";
			details.ExRate = 2m;
			details.OSExTaxAmount = 100m;
			details.OSTaxAmount = 12m;
			details.LocalExTaxAmount = 200m;
			details.LocalTaxAmount = 24m;
			details.Description = "Some text";
			details.BranchPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			details.DepartmentPK = new ZGuid("20000000-0000-0000-0000-000000000000");
			details.AddressPK = new ZGuid("30000000-0000-0000-0000-000000000000");
			details.ContactPK = new ZGuid("40000000-0000-0000-0000-000000000000");
			details.NumberOfSupportingDocuments = 14;
			details.SourceXML = "Some xml";
			details.IsCrossLedgerImportFromXML = true;
			details.PlaceOfSupply = "DL";
			details.PlaceOfSupplyType = "STA";

			var newDetails = (TransactionPendingAllocationApprovalDetails)GetNewBusinessObject();

			newDetails.CopyFrom(details);
			AssertEquals("MaxAmountToApprove", 11M, newDetails.MaxAmountToApprove);
			AssertEquals("TransactionDate", ZDateTime.Today.AddDays(-3), newDetails.TransactionDate);
			AssertEquals("PostDate", ZDateTime.Today.AddDays(-1), newDetails.PostDate);
			AssertEquals("TransactionNumber", "INV12", newDetails.TransactionNumber);
			AssertEquals("CreditorPK", new ZGuid("00000000-0000-0000-0000-000000000000"), newDetails.CreditorPK);
			AssertEquals("DueDate", ZDateTime.Today.AddDays(4), newDetails.DueDate);
			AssertEquals("CurrencyCode", "USD", newDetails.CurrencyCode);
			AssertEquals("ExRate", 2m, newDetails.ExRate);
			AssertEquals("OSExTaxAmount", 100m, newDetails.OSExTaxAmount);
			AssertEquals("OSTaxAmount", 12m, newDetails.OSTaxAmount);
			AssertEquals("LocalExTaxAmount", 200m, newDetails.LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 24m, newDetails.LocalTaxAmount);
			AssertEquals("Description", "Some text", newDetails.Description);
			AssertEquals("BranchPK", new ZGuid("10000000-0000-0000-0000-000000000000"), newDetails.BranchPK);
			AssertEquals("DepartmentPK", new ZGuid("20000000-0000-0000-0000-000000000000"), newDetails.DepartmentPK);
			AssertEquals("AddressPK", new ZGuid("30000000-0000-0000-0000-000000000000"), newDetails.AddressPK);
			AssertEquals("ContactPK", new ZGuid("40000000-0000-0000-0000-000000000000"), newDetails.ContactPK);
			AssertEquals("NumberOfSupportingDocuments", (ZByte)14, newDetails.NumberOfSupportingDocuments);
			AssertEquals("SourceXML", "Some xml", newDetails.SourceXML);
			AssertEquals("IsCrossLedgerImportFromXML", true, newDetails.IsCrossLedgerImportFromXML);
			AssertEquals("PlaceOfSupply", "DL", newDetails.PlaceOfSupply);
			AssertEquals("ContactPK", "STA", newDetails.PlaceOfSupplyType);
		}

		[TestDate(2014, 11, 12)]
		public override void TestReadXML()
		{
			foreach (var suspender in new[] { SetFPOSSuspender.GetSuspender(), null })
			{
				using (suspender)
				{
					base.TestReadXML();
				}
			}
		}

		[TestDate(2014, 11, 12)]
		public override void TestWriteXML()
		{
			foreach (var suspender in new[] { SetFPOSSuspender.GetSuspender(), null })
			{
				using (suspender)
				{
					base.TestWriteXML();
				}
			}
		}

		protected override TransactionPendingAllocationApprovalDetails GetNewFullyPopulatedBusinessObjectForXMLTest()
		{
			var details = base.GetNewFullyPopulatedBusinessObjectForXMLTest();

			details.TransactionDate = ZDateTime.Today.AddDays(-3);
			details.PostDate = ZDateTime.Today.AddDays(-1);
			details.TransactionNumber = "INV12";
			details.CreditorPK = new ZGuid("00000000-0000-0000-0000-000000000000");
			details.DueDate = ZDateTime.Today.AddDays(4);
			details.CurrencyCode = "USD";
			details.ExRate = 2m;
			details.OSExTaxAmount = 100m;
			details.OSTaxAmount = 12m;
			details.LocalExTaxAmount = 200m;
			details.LocalTaxAmount = 24m;
			details.Description = "HeaderDesc";
			details.BranchPK = new ZGuid("10000000-0000-0000-0000-000000000000");
			details.DepartmentPK = new ZGuid("20000000-0000-0000-0000-000000000000");
			details.AddressPK = new ZGuid("30000000-0000-0000-0000-000000000000");
			details.ContactPK = new ZGuid("40000000-0000-0000-0000-000000000000");
			details.NumberOfSupportingDocuments = 14;
			details.SourceXML = "<xml>some xml</xml>";
			details.IsCrossLedgerImportFromXML = true;

			if (!SetFPOSSuspender.IsSuspended)
			{
				details.PlaceOfSupply = "DL";
				details.PlaceOfSupplyType = "STA";
			}

			return details;
		}

		FunctionalitySuspender SetFPOSSuspender => setFPOSSuspender ?? (setFPOSSuspender = new FunctionalitySuspender());
		FunctionalitySuspender setFPOSSuspender;

		protected override string GetExpectedXML(bool isReadTest)
		{
			return SetFPOSSuspender.IsSuspended
				? "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><TransactionPendingAllocationApprovalDetails><MaxAmountToApprove>100</MaxAmountToApprove><Description>HeaderDesc</Description><InvoiceTerm>CUS</InvoiceTerm><InvoiceTermDays>4</InvoiceTermDays><Details><TransactionDate>2014-11-09T00:00:00</TransactionDate><PostDate>2014-11-11T00:00:00</PostDate><TransactionNumber>INV12</TransactionNumber><CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK><DueDate>2014-11-16T00:00:00</DueDate><CurrencyCode>USD</CurrencyCode><ExRate>2</ExRate><OSExTaxAmount>100</OSExTaxAmount><OSTaxAmount>12</OSTaxAmount><LocalExTaxAmount>200</LocalExTaxAmount><LocalTaxAmount>24</LocalTaxAmount><Description>HeaderDesc</Description><BranchPK>10000000-0000-0000-0000-000000000000</BranchPK><DepartmentPK>20000000-0000-0000-0000-000000000000</DepartmentPK><AddressPK>30000000-0000-0000-0000-000000000000</AddressPK><ContactPK>40000000-0000-0000-0000-000000000000</ContactPK><NumberOfSupportingDocuments>14</NumberOfSupportingDocuments><SourceXML IsCrossLedgerImport=\"Y\"><XML>&lt;xml&gt;some xml&lt;/xml&gt;</XML></SourceXML></Details></TransactionPendingAllocationApprovalDetails>"
				: "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><TransactionPendingAllocationApprovalDetails><MaxAmountToApprove>100</MaxAmountToApprove><Description>HeaderDesc</Description><InvoiceTerm>CUS</InvoiceTerm><InvoiceTermDays>4</InvoiceTermDays><Details><TransactionDate>2014-11-09T00:00:00</TransactionDate><PostDate>2014-11-11T00:00:00</PostDate><TransactionNumber>INV12</TransactionNumber><CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK><DueDate>2014-11-16T00:00:00</DueDate><CurrencyCode>USD</CurrencyCode><ExRate>2</ExRate><OSExTaxAmount>100</OSExTaxAmount><OSTaxAmount>12</OSTaxAmount><LocalExTaxAmount>200</LocalExTaxAmount><LocalTaxAmount>24</LocalTaxAmount><Description>HeaderDesc</Description><BranchPK>10000000-0000-0000-0000-000000000000</BranchPK><DepartmentPK>20000000-0000-0000-0000-000000000000</DepartmentPK><AddressPK>30000000-0000-0000-0000-000000000000</AddressPK><ContactPK>40000000-0000-0000-0000-000000000000</ContactPK><NumberOfSupportingDocuments>14</NumberOfSupportingDocuments><SourceXML IsCrossLedgerImport=\"Y\"><XML>&lt;xml&gt;some xml&lt;/xml&gt;</XML></SourceXML><PlaceOfSupply>DL</PlaceOfSupply><PlaceOfSupplyType>STA</PlaceOfSupplyType></Details></TransactionPendingAllocationApprovalDetails>";
		}

		protected override string GetExpectedEmptyXML()
		{
			return "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><TransactionPendingAllocationApprovalDetails><MaxAmountToApprove>0</MaxAmountToApprove><Description /><InvoiceTerm /><InvoiceTermDays>0</InvoiceTermDays><Details><TransactionDate></TransactionDate><PostDate></PostDate><TransactionNumber></TransactionNumber><CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK><DueDate></DueDate><CurrencyCode></CurrencyCode><ExRate>0</ExRate><OSExTaxAmount>0</OSExTaxAmount><OSTaxAmount>0</OSTaxAmount><LocalExTaxAmount>0</LocalExTaxAmount><LocalTaxAmount>0</LocalTaxAmount><Description></Description><BranchPK>00000000-0000-0000-0000-000000000000</BranchPK><DepartmentPK>00000000-0000-0000-0000-000000000000</DepartmentPK><AddressPK>00000000-0000-0000-0000-000000000000</AddressPK><ContactPK>00000000-0000-0000-0000-000000000000</ContactPK><NumberOfSupportingDocuments>0</NumberOfSupportingDocuments></Details></TransactionPendingAllocationApprovalDetails>";
		}

		protected override string GetLegacyXML()
		{
			return "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><TransactionPendingAllocationApprovalDetails><MaxAmountToApprove>100</MaxAmountToApprove><Details><TransactionDate>2014-11-09T00:00:00</TransactionDate><PostDate>2014-11-11T00:00:00</PostDate><TransactionNumber>INV12</TransactionNumber><CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK><DueDate>2014-11-16T00:00:00</DueDate><CurrencyCode>USD</CurrencyCode><ExRate>2</ExRate><OSExTaxAmount>100</OSExTaxAmount><OSTaxAmount>12</OSTaxAmount><LocalExTaxAmount>200</LocalExTaxAmount><LocalTaxAmount>24</LocalTaxAmount><Description></Description><BranchPK>10000000-0000-0000-0000-000000000000</BranchPK><DepartmentPK>20000000-0000-0000-0000-000000000000</DepartmentPK><AddressPK>30000000-0000-0000-0000-000000000000</AddressPK><ContactPK>40000000-0000-0000-0000-000000000000</ContactPK><NumberOfSupportingDocuments>14</NumberOfSupportingDocuments><SourceXML IsCrossLedgerImport=\"Y\"><XML>&lt;xml&gt;some xml&lt;/xml&gt;</XML></SourceXML><PlaceOfSupply>DL</PlaceOfSupply><PlaceOfSupplyType>STA</PlaceOfSupplyType></Details></TransactionPendingAllocationApprovalDetails>";
		}

		protected override void AssertFullyPopulatedBusinessObjectForXMLTest(TransactionPendingAllocationApprovalDetails details, bool withNewFields = true)
		{
			base.AssertFullyPopulatedBusinessObjectForXMLTest(details, withNewFields);

			AssertEquals("TransactionDate", ZDateTime.Today.AddDays(-3), details.TransactionDate);
			AssertEquals("PostDate", ZDateTime.Today.AddDays(-1), details.PostDate);
			AssertEquals("TransactionNumber", "INV12", details.TransactionNumber);
			AssertEquals("CreditorPK", new ZGuid("00000000-0000-0000-0000-000000000000"), details.CreditorPK);
			AssertEquals("DueDate", ZDateTime.Today.AddDays(4), details.DueDate);
			AssertEquals("CurrencyCode", "USD", details.CurrencyCode);
			AssertEquals("ExRate", 2m, details.ExRate);
			AssertEquals("OSExTaxAmount", 100m, details.OSExTaxAmount);
			AssertEquals("OSTaxAmount", 12m, details.OSTaxAmount);
			AssertEquals("LocalExTaxAmount", 200m, details.LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 24m, details.LocalTaxAmount);
			AssertEquals("Description", withNewFields ? "HeaderDesc" : string.Empty, details.Description);
			AssertEquals("BranchPK", new ZGuid("10000000-0000-0000-0000-000000000000"), details.BranchPK);
			AssertEquals("DepartmentPK", new ZGuid("20000000-0000-0000-0000-000000000000"), details.DepartmentPK);
			AssertEquals("AddressPK", new ZGuid("30000000-0000-0000-0000-000000000000"), details.AddressPK);
			AssertEquals("ContactPK", new ZGuid("40000000-0000-0000-0000-000000000000"), details.ContactPK);
			AssertEquals("NumberOfSupportingDocuments", (ZByte)14, details.NumberOfSupportingDocuments);
			AssertEquals("SourceXML", "<xml>some xml</xml>", details.SourceXML);
			AssertEquals("IsCrossLedgerImportFromXML", true, details.IsCrossLedgerImportFromXML);
			if (!SetFPOSSuspender.IsSuspended)
			{
				AssertEquals("PlaceOfSupply", "DL", details.PlaceOfSupply);
				AssertEquals("PlaceOfSupplyType", "STA", details.PlaceOfSupplyType);
			}
		}
	}
}
