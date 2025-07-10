using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalRequestDetails))]
	public class ARCreditNoteApprovalRequestDetailsTest : ApprovalRequestDetailsWithChargesTest<ARCreditNoteApprovalRequestDetails, ARCreditNoteApprovalRequestChargeDetails>
	{
		public override void TestOpertorEqualCore()
		{
			base.TestOpertorEqualCore();

			ARCreditNoteApprovalRequestDetails a = null;
			ARCreditNoteApprovalRequestDetails b = null;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a = (ARCreditNoteApprovalRequestDetails)GetNewBusinessObject();
			b = null;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			a = null;
			b = (ARCreditNoteApprovalRequestDetails)GetNewBusinessObject();
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			a = (ARCreditNoteApprovalRequestDetails)GetNewBusinessObject();
			b = (ARCreditNoteApprovalRequestDetails)GetNewBusinessObject();
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.PostingOption = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.PostingOption = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			// Not compare ApprovingOption as this can be changed if user modifiy the registry setting after creating the request.
			a.ApprovingOption = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			b.ApprovingOption = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.ApprovingOption = "ONE";
			b.ApprovingOption = "";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.ApprovingOption = "";
			b.ApprovingOption = "ONE";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			// Not compare MaxAuthorisationLevelRequired as this can be changed if user modifiy the registry setting after creating the request.
			a.MaxAuthorisationLevelRequired = 6;
			b.MaxAuthorisationLevelRequired = 5;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.MaxAuthorisationLevelRequired = 4;
			b.MaxAuthorisationLevelRequired = 4;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.MaxAmountToApprove = 1M;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.MaxAmountToApprove = 1M;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			//The following properties are not checked in the IsEqual otherwise the AutoPosting process is failing with the error: "Can't post this request because source details have been modified since then"
			//In LevelAuthorizationWithApprovalRequest.PerformTransactionLevelAuthorization() it checks that requests are equals but the auto posting modify automatically some fields from the transaction or line. So I decided to not include any new fields.
			//Properties from the Header: Description, InvoiceTerm, InvoiceTermDays
			//Properties from the Line: Description, AccInvMsgPK, TaxDate

			a.Description = "desc";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			b.Description = "desc";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.InvoiceTerm = "CUS";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			b.InvoiceTerm = "CUS";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.InvoiceTermDays = 2;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			b.InvoiceTermDays = 2;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			var chargeA1 = a.Charges.AddNew();
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			var chargeB1 = b.Charges.AddNew();
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA1.JobNumber = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeB1.JobNumber = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			var chargeA2 = a.Charges.AddNew();
			var chargeA3 = a.Charges.AddNew();
			var chargeB2 = b.Charges.AddNew();
			var chargeB3 = b.Charges.AddNew();
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA2.JobNumber = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeB2.JobNumber = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA3.JobNumber = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeB3.JobNumber = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeB2.OSSellAmount = 1M;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeA2.OSSellAmount = 1M;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA3.InvoiceType = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeB3.InvoiceType = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA3.Description = "descCharge";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeB3.Description = "descCharge";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA3.AccInvMsgPK = ZGuid.BrettsGuid;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeB3.AccInvMsgPK = ZGuid.BrettsGuid;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA3.TaxDate = new ZDate(2020, 5, 5);
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeB3.TaxDate = new ZDate(2020, 5, 5);
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public override void TestCopyFromCore()
		{
			base.TestCopyFromCore();

			var postingRequest = (ARCreditNoteApprovalRequestDetails)GetNewBusinessObject();

			postingRequest.PostingOption = "PST";
			postingRequest.ApprovingOption = "ONE";
			postingRequest.MaxAuthorisationLevelRequired = 3;
			postingRequest.MaxAmountToApprove = 11M;
			postingRequest.InvoiceDate = new ZDateTime("2018-06-14");
			postingRequest.PostDate = new ZDateTime("2018-06-15");
			var charge = postingRequest.Charges.AddNew();
			charge.OSSellAmount = 5M;
			charge.SellAccount = "ORG";
			charge = postingRequest.Charges.AddNew();
			charge.LocalSellAmount = 35M;
			charge.SellCurrency = "AKL";

			var newPostingRequest = (ARCreditNoteApprovalRequestDetails)GetNewBusinessObject();

			newPostingRequest.CopyFrom(postingRequest);
			AssertEquals("PostingOption", "PST", newPostingRequest.PostingOption);
			AssertEquals("ApprovingOption", "ONE", newPostingRequest.ApprovingOption);
			AssertEquals("MaxAuthorisationLevelRequired", 3, newPostingRequest.MaxAuthorisationLevelRequired);
			AssertEquals("MaxAmountToApprove", 11M, newPostingRequest.MaxAmountToApprove);
			AssertEquals("InvoiceDate", new ZDateTime("2018-06-14"), newPostingRequest.InvoiceDate);
			AssertEquals("PostDate", new ZDateTime("2018-06-15"), newPostingRequest.PostDate);
			AssertEquals("Charges.Count", 2, newPostingRequest.Charges.Count);
			AssertEquals("Charges[0].OSSellAmount", 5M, newPostingRequest.Charges[0].OSSellAmount);
			AssertEquals("Charges[0].SellAccount", "ORG", newPostingRequest.Charges[0].SellAccount);
			AssertEquals("Charges[1].LocalSellAmount", 35M, newPostingRequest.Charges[1].LocalSellAmount);
			AssertEquals("Charges[1].SellCurrency", "AKL", newPostingRequest.Charges[1].SellCurrency);
		}

		public override void TestIsPostingActionTheSame()
		{
			var a = (ARCreditNoteApprovalRequestDetails)GetNewBusinessObject();
			var b = (ARCreditNoteApprovalRequestDetails)GetNewBusinessObject();

			a.MaxAmountToApprove = 10;
			a.Charges.AddNew();
			b.MaxAmountToApprove = 100;
			b.Charges.AddNew();
			b.Charges.AddNew();
			Assert(a.IsPostingActionTheSame(b));
			Assert(b.IsPostingActionTheSame(a));

			a.PostingOption = "PST";
			Assert(!a.IsPostingActionTheSame(b));
			Assert(!b.IsPostingActionTheSame(a));

			b.PostingOption = "PST";
			Assert(a.IsPostingActionTheSame(b));
			Assert(b.IsPostingActionTheSame(a));
		}

		public void TestIsEligibleToAutoPostAmendingARCreditNote()
		{
			var oldXML = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>COSTS</PostingOption><ApprovingOption>ONE</ApprovingOption><MaxAmountToApprove>100</MaxAmountToApprove><ArrayOfChargeDetails><ChargeDetails><JobNumber>JOB1</JobNumber><ChargeCode /><Branch /><Department>DEP1</Department><SellAccount>ORG</SellAccount><SellCurrency /><OSSellAmount>5</OSSellAmount><LocalSellAmount>0</LocalSellAmount><InvoiceType /></ChargeDetails><ChargeDetails><JobNumber>JOB2</JobNumber><ChargeCode /><Branch>BRN1</Branch><Department /><SellAccount /><SellCurrency>AKL</SellCurrency><OSSellAmount>0</OSSellAmount><LocalSellAmount>35</LocalSellAmount><InvoiceType /><OSTaxAmount>1.1</OSTaxAmount><LocalTaxAmount>1.2</LocalTaxAmount><ExchangeRate>3</ExchangeRate><TaxCode>CCC</TaxCode></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(oldXML)))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				var serializer = ZXmlSerializer.New(typeof(ARCreditNoteApprovalRequestDetails));
				var details = (ARCreditNoteApprovalRequestDetails)serializer.Deserialize(reader);
				Assert("Not Eligible", !details.IsEligibleToAutoPostAmendingARCreditNote);
			}

			var newXML = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>COSTS</PostingOption><ApprovingOption>ONE</ApprovingOption><InvoiceDate>15-Feb-18 00:00:00</InvoiceDate><PostDate>17-Feb-18 00:00:00</PostDate><MaxAmountToApprove>100</MaxAmountToApprove><ArrayOfChargeDetails><ChargeDetails><JobNumber>JOB1</JobNumber><ChargeCode /><Branch /><Department>DEP1</Department><SellAccount>ORG</SellAccount><SellCurrency /><OSSellAmount>5</OSSellAmount><LocalSellAmount>0</LocalSellAmount><InvoiceType /><OSTaxAmount>1</OSTaxAmount><LocalTaxAmount>2</LocalTaxAmount><ExchangeRate>3.5</ExchangeRate><TaxCode>AAA</TaxCode></ChargeDetails><ChargeDetails><JobNumber>JOB2</JobNumber><ChargeCode /><Branch>BRN1</Branch><Department /><SellAccount /><SellCurrency>AKL</SellCurrency><OSSellAmount>0</OSSellAmount><LocalSellAmount>35</LocalSellAmount><InvoiceType /><OSTaxAmount>1.1</OSTaxAmount><LocalTaxAmount>1.2</LocalTaxAmount><ExchangeRate>3</ExchangeRate><TaxCode>CCC</TaxCode></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(newXML)))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				var serializer = ZXmlSerializer.New(typeof(ARCreditNoteApprovalRequestDetails));
				var details = (ARCreditNoteApprovalRequestDetails)serializer.Deserialize(reader);
				Assert("Eligible", details.IsEligibleToAutoPostAmendingARCreditNote);
			}
		}

		protected override ARCreditNoteApprovalRequestDetails GetNewFullyPopulatedBusinessObjectForXMLTest()
		{
			var details = base.GetNewFullyPopulatedBusinessObjectForXMLTest();
			details.ApprovingOption = "ONE";
			details.MaxAuthorisationLevelRequired = 1;
			details.InvoiceDate = new ZDateTime("2018-02-15");
			details.PostDate = new ZDateTime("2018-02-17");
			var charge = details.Charges[0];
			charge.OSSellAmount = 5M;
			charge.SellAccount = "ORG";
			charge.OSTaxAmount = 1M;
			charge.LocalTaxAmount = 2M;
			charge.ExchangeRate = 3.5;
			charge.TaxCode = "AAA";
			charge = details.Charges[1];
			charge.LocalSellAmount = 35M;
			charge.SellCurrency = "AKL";
			charge.OSTaxAmount = 1.1M;
			charge.LocalTaxAmount = 1.2M;
			charge.ExchangeRate = 3;
			charge.TaxCode = "CCC";
			charge.SupplyType = "LOC";

			return details;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		protected override string GetExpectedXML(bool isReadTest)
		{
			return $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>COSTS</PostingOption><ApprovingOption>ONE</ApprovingOption><InvoiceDate>15-Feb-18 00:00:00</InvoiceDate><PostDate>17-Feb-18 00:00:00</PostDate><MaxAuthorisationLevelRequired>1</MaxAuthorisationLevelRequired><MaxAmountToApprove>100</MaxAmountToApprove><Description>HeaderDesc</Description><InvoiceTerm>CUS</InvoiceTerm><InvoiceTermDays>4</InvoiceTermDays><ArrayOfChargeDetails><ChargeDetails><JobNumber>JOB1</JobNumber><ChargeCode /><Branch /><Department>DEP1</Department><Description>LineDesc1</Description><AccInvMsgPK>{TaxMessage.PK}</AccInvMsgPK><TaxDate>{TaxDate.ToString("dd-MMM-yy")}</TaxDate><SellAccount>ORG</SellAccount><SellCurrency /><OSSellAmount>5</OSSellAmount><LocalSellAmount>0</LocalSellAmount><InvoiceType /><OSTaxAmount>1</OSTaxAmount><LocalTaxAmount>2</LocalTaxAmount><ExchangeRate>3.5</ExchangeRate><TaxCode>AAA</TaxCode><SupplyType /></ChargeDetails><ChargeDetails><JobNumber>JOB2</JobNumber><ChargeCode /><Branch>BRN1</Branch><Department /><Description>LineDesc2</Description><AccInvMsgPK>{ZGuid.Empty}</AccInvMsgPK><TaxDate>03-Jun-20</TaxDate><SellAccount /><SellCurrency>AKL</SellCurrency><OSSellAmount>0</OSSellAmount><LocalSellAmount>35</LocalSellAmount><InvoiceType /><OSTaxAmount>1.1</OSTaxAmount><LocalTaxAmount>1.2</LocalTaxAmount><ExchangeRate>3</ExchangeRate><TaxCode>CCC</TaxCode><SupplyType>LOC</SupplyType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
		}

		protected override string GetExpectedEmptyXML()
		{
			return "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption /><ApprovingOption /><InvoiceDate /><PostDate /><MaxAuthorisationLevelRequired>0</MaxAuthorisationLevelRequired><MaxAmountToApprove>0</MaxAmountToApprove><Description /><InvoiceTerm /><InvoiceTermDays>0</InvoiceTermDays><ArrayOfChargeDetails /></PostingRequest>";
		}

		protected override string GetLegacyXML()
		{
			return $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>COSTS</PostingOption><ApprovingOption>ONE</ApprovingOption><InvoiceDate>15-Feb-18 00:00:00</InvoiceDate><PostDate>17-Feb-18 00:00:00</PostDate><MaxAuthorisationLevelRequired>1</MaxAuthorisationLevelRequired><MaxAmountToApprove>100</MaxAmountToApprove><ArrayOfChargeDetails><ChargeDetails><JobNumber>JOB1</JobNumber><ChargeCode /><Branch /><Department>DEP1</Department><SellAccount>ORG</SellAccount><SellCurrency /><OSSellAmount>5</OSSellAmount><LocalSellAmount>0</LocalSellAmount><InvoiceType /><OSTaxAmount>1</OSTaxAmount><LocalTaxAmount>2</LocalTaxAmount><ExchangeRate>3.5</ExchangeRate><TaxCode>AAA</TaxCode></ChargeDetails><ChargeDetails><JobNumber>JOB2</JobNumber><ChargeCode /><Branch>BRN1</Branch><Department /><SellAccount /><SellCurrency>AKL</SellCurrency><OSSellAmount>0</OSSellAmount><LocalSellAmount>35</LocalSellAmount><InvoiceType /><OSTaxAmount>1.1</OSTaxAmount><LocalTaxAmount>1.2</LocalTaxAmount><ExchangeRate>3</ExchangeRate><TaxCode>CCC</TaxCode></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
		}

		protected override void AssertFullyPopulatedBusinessObjectForXMLTest(ARCreditNoteApprovalRequestDetails details, bool withNewFields = true)
		{
			base.AssertFullyPopulatedBusinessObjectForXMLTest(details, withNewFields);
			AssertEquals("ONE", details.ApprovingOption);
			AssertEquals(new ZDateTime("2018-02-15"), details.InvoiceDate);
			AssertEquals(new ZDateTime("2018-02-17"), details.PostDate);
			AssertEquals("Charges.Count", 2, details.Charges.Count);
			AssertEquals("Charges[0].OSSellAmount", 5M, details.Charges[0].OSSellAmount);
			AssertEquals("Charges[0].SellAccount", "ORG", details.Charges[0].SellAccount);
			AssertEquals("Charges[1].LocalSellAmount", 35M, details.Charges[1].LocalSellAmount);
			AssertEquals("Charges[1].SellCurrency", "AKL", details.Charges[1].SellCurrency);
		}
	}
}
