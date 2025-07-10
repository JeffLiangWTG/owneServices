using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceChargesApprovalRequestDetails))]
	public class APInvoiceChargesApprovalRequestDetailsTest : ApprovalRequestDetailsWithChargesTest<APInvoiceChargesApprovalRequestDetails, APInvoiceChargesApprovalRequestChargeDetails>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new APInvoiceChargesApprovalRequestDetails(Factory);
		}

		public override void TestOpertorEqualCore()
		{
			base.TestOpertorEqualCore();

			APInvoiceChargesApprovalRequestDetails a = null;
			APInvoiceChargesApprovalRequestDetails b = null;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();
			b = null;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			a = null;
			b = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			a = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();
			b = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.MaxAmountToApprove = 1M;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.MaxAmountToApprove = 1M;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.PostingOption = "COSTS";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.PostingOption = "COSTS";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.Creditor = "Creditor";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.Creditor = "Creditor";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.TransactionNumber = "TransactionNumber";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.TransactionNumber = "TransactionNumber";
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

			chargeB2.OSCostAmount = 1M;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeA2.OSCostAmount = 1M;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA3.CostCurrency = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeB3.CostCurrency = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);
		}

		public override void TestCopyFromCore()
		{
			base.TestCopyFromCore();

			var postingRequest = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();

			postingRequest.MaxAmountToApprove = 11M;
			postingRequest.PostingOption = "COSTS";
			postingRequest.Creditor = "ORG";
			postingRequest.TransactionNumber = "Number";
			var charge = postingRequest.Charges.AddNew();
			charge.OSCostAmount = 5M;
			charge.CostCurrency = "ORG";
			charge = postingRequest.Charges.AddNew();
			charge.LocalCostAmount = 35M;
			charge.Branch = "AKL";

			var newPostingRequest = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();

			newPostingRequest.CopyFrom(postingRequest);

			AssertEquals("MaxAmountToApprove", 11M, newPostingRequest.MaxAmountToApprove);
			AssertEquals("PostingOption", "COSTS", newPostingRequest.PostingOption);
			AssertEquals("Creditor", "ORG", newPostingRequest.Creditor);
			AssertEquals("TransactionNumber", "Number", newPostingRequest.TransactionNumber);

			AssertEquals("Charges.Count", 2, newPostingRequest.Charges.Count);
			AssertEquals("Charges[0].OSCostAmount", 5M, newPostingRequest.Charges[0].OSCostAmount);
			AssertEquals("Charges[0].CostAccount", "ORG", newPostingRequest.Charges[0].CostCurrency);
			AssertEquals("Charges[1].LocalCostAmount", 35M, newPostingRequest.Charges[1].LocalCostAmount);
			AssertEquals("Charges[1].Branch", "AKL", newPostingRequest.Charges[1].Branch);
		}

		public override void TestIsPostingActionTheSame()
		{
			var a = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();
			var b = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();
			a.MaxAmountToApprove = 10;
			a.PostingOption = "COSTS";
			a.Charges.AddNew();
			b.MaxAmountToApprove = 100;
			b.PostingOption = "COSTS";
			b.Charges.AddNew();
			b.Charges.AddNew();
			Assert(a.IsPostingActionTheSame(b));
			Assert(b.IsPostingActionTheSame(a));

			b.Creditor = "ORG";
			Assert("Posting action is not the same for any invoice and charge details", !a.IsPostingActionTheSame(b));
			Assert("Posting action is not the same for any invoice and charge details", !b.IsPostingActionTheSame(a));

			a.Creditor = "ORG";
			a.TransactionNumber = "1";
			Assert("Posting action is not the same for any invoice and charge details", !a.IsPostingActionTheSame(b));
			Assert("Posting action is not the same for any invoice and charge details", !b.IsPostingActionTheSame(a));

			b.TransactionNumber = "1";
			Assert("Posting action is the same for any invoice and charge details", a.IsPostingActionTheSame(b));
			Assert("Posting action is the same for any invoice and charge details", b.IsPostingActionTheSame(a));
		}

		protected override APInvoiceChargesApprovalRequestDetails GetNewFullyPopulatedBusinessObjectForXMLTest()
		{
			var details = base.GetNewFullyPopulatedBusinessObjectForXMLTest();

			details.Creditor = "PST";
			details.TransactionNumber = "111";

			var charge = details.Charges[0];
			charge.OSCostAmount = 5M;
			charge.CostCurrency = "ORG";

			charge = details.Charges[1];
			charge.LocalCostAmount = 35M;
			charge.CostCurrency = "AKL";

			return details;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		protected override string GetExpectedXML(bool isReadTest)
		{
			if (isReadTest)
			{
				return $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><APInvoiceChargesApprovalRequestDetails><PostingOption>COSTS</PostingOption><Creditor>PST</Creditor><TransactionNumber>111</TransactionNumber><DetailsXmlNode><RequisitionStatus>TST</RequisitionStatus><RequisitionDate>2016-03-14T00:00:00</RequisitionDate></DetailsXmlNode><MaxAmountToApprove>100</MaxAmountToApprove><Description>HeaderDesc</Description><InvoiceTerm>CUS</InvoiceTerm><InvoiceTermDays>4</InvoiceTermDays><ArrayOfAPInvoiceChargesApprovalRequestChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>JOB1</JobNumber><ChargeCode /><Branch /><Department>DEP1</Department><Description>LineDesc1</Description><AccInvMsgPK>{TaxMessage.PK}</AccInvMsgPK><TaxDate>{TaxDate.ToString("dd-MMM-yy")}</TaxDate><CostCurrency>ORG</CostCurrency><OSCostAmount>5</OSCostAmount><LocalCostAmount>0</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>JOB2</JobNumber><ChargeCode /><Branch>BRN1</Branch><Department /><Description>LineDesc2</Description><AccInvMsgPK>{ZGuid.Empty}</AccInvMsgPK><TaxDate>03-Jun-20</TaxDate><CostCurrency>AKL</CostCurrency><OSCostAmount>0</OSCostAmount><LocalCostAmount>35</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails></ArrayOfAPInvoiceChargesApprovalRequestChargeDetails></APInvoiceChargesApprovalRequestDetails>";
			}
			else
			{
				return $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><APInvoiceChargesApprovalRequestDetails><PostingOption>COSTS</PostingOption><Creditor>PST</Creditor><TransactionNumber>111</TransactionNumber><MaxAmountToApprove>100</MaxAmountToApprove><Description>HeaderDesc</Description><InvoiceTerm>CUS</InvoiceTerm><InvoiceTermDays>4</InvoiceTermDays><ArrayOfAPInvoiceChargesApprovalRequestChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>JOB1</JobNumber><ChargeCode /><Branch /><Department>DEP1</Department><Description>LineDesc1</Description><AccInvMsgPK>{TaxMessage.PK}</AccInvMsgPK><TaxDate>{TaxDate.ToString("dd-MMM-yy")}</TaxDate><CostCurrency>ORG</CostCurrency><OSCostAmount>5</OSCostAmount><LocalCostAmount>0</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>JOB2</JobNumber><ChargeCode /><Branch>BRN1</Branch><Department /><Description>LineDesc2</Description><AccInvMsgPK>{ZGuid.Empty}</AccInvMsgPK><TaxDate>03-Jun-20</TaxDate><CostCurrency>AKL</CostCurrency><OSCostAmount>0</OSCostAmount><LocalCostAmount>35</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails></ArrayOfAPInvoiceChargesApprovalRequestChargeDetails></APInvoiceChargesApprovalRequestDetails>";
			}
		}

		protected override string GetExpectedEmptyXML()
		{
			return "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><APInvoiceChargesApprovalRequestDetails><PostingOption /><Creditor /><TransactionNumber /><MaxAmountToApprove>0</MaxAmountToApprove><Description /><InvoiceTerm /><InvoiceTermDays>0</InvoiceTermDays><ArrayOfAPInvoiceChargesApprovalRequestChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" /></APInvoiceChargesApprovalRequestDetails>";
		}

		protected override string GetLegacyXML()
		{
			return $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><APInvoiceChargesApprovalRequestDetails><PostingOption>COSTS</PostingOption><Creditor>PST</Creditor><TransactionNumber>111</TransactionNumber><DetailsXmlNode><RequisitionStatus>TST</RequisitionStatus><RequisitionDate>2016-03-14T00:00:00</RequisitionDate></DetailsXmlNode><MaxAmountToApprove>100</MaxAmountToApprove><ArrayOfAPInvoiceChargesApprovalRequestChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>JOB1</JobNumber><ChargeCode /><Branch /><Department>DEP1</Department><CostCurrency>ORG</CostCurrency><OSCostAmount>5</OSCostAmount><LocalCostAmount>0</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>JOB2</JobNumber><ChargeCode /><Branch>BRN1</Branch><Department /><CostCurrency>AKL</CostCurrency><OSCostAmount>0</OSCostAmount><LocalCostAmount>35</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails></ArrayOfAPInvoiceChargesApprovalRequestChargeDetails></APInvoiceChargesApprovalRequestDetails>";
		}

		protected override void AssertFullyPopulatedBusinessObjectForXMLTest(APInvoiceChargesApprovalRequestDetails details, bool withNewFields = true)
		{
			base.AssertFullyPopulatedBusinessObjectForXMLTest(details, withNewFields);
			AssertVariousFields(details);
		}

		void AssertVariousFields(APInvoiceChargesApprovalRequestDetails details)
		{
			AssertEquals("Creditor", "PST", details.Creditor);
			AssertEquals("TransactionNumber", "111", details.TransactionNumber);

			AssertEquals("Charges.Count", 2, details.Charges.Count);
			AssertEquals("Charges[0].OSCostAmount", 5M, details.Charges[0].OSCostAmount);
			AssertEquals("Charges[0].CostAccount", "ORG", details.Charges[0].CostCurrency);
			AssertEquals("Charges[1].LocalCostAmount", 35M, details.Charges[1].LocalCostAmount);
			AssertEquals("Charges[1].CostCurrency", "AKL", details.Charges[1].CostCurrency);
		}

		[TestDate(2014, 11, 12)]
		public void TestReadXMLBackwardCompatibleWithNoPostingOptionAndRequisitionStatusAndDate()
		{
			var legacyXML = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><APInvoiceChargesApprovalRequestDetails><Creditor>PST</Creditor><TransactionNumber>111</TransactionNumber><MaxAmountToApprove>100</MaxAmountToApprove><ArrayOfAPInvoiceChargesApprovalRequestChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>JOB1</JobNumber><ChargeCode /><Branch /><Department>DEP1</Department><CostCurrency>ORG</CostCurrency><OSCostAmount>5</OSCostAmount><LocalCostAmount>0</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>JOB2</JobNumber><ChargeCode /><Branch>BRN1</Branch><Department /><CostCurrency>AKL</CostCurrency><OSCostAmount>0</OSCostAmount><LocalCostAmount>35</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails></ArrayOfAPInvoiceChargesApprovalRequestChargeDetails></APInvoiceChargesApprovalRequestDetails>";

			using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(legacyXML)))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				var serializer = ZXmlSerializer.New(typeof(APInvoiceChargesApprovalRequestDetails));
				var details = (APInvoiceChargesApprovalRequestDetails)serializer.Deserialize(reader);
				AssertEquals(ZString.Empty, details.PostingOption);
				AssertVariousFields(details);
			}
		}

		public void TestDBHitsForAllowedToLogin()
		{
			var factory1 = new BusinessObjectFactory();
			var staff = factory1.NewWithValidTestData<GlbStaff>();
			factory1.Save();

			GlbStaff.CurrentUser.Factory.ResetDatabaseLoadCount();

			UserLoginPermissions(factory1);

			var factory2 = new BusinessObjectFactory();

			UserLoginPermissions(factory2);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(new Dictionary<string, int>
			{
			}, GlbStaff.CurrentUser.Factory);

			AssertDbHits(new Dictionary<string, int>
			{
			}, factory2);

			void UserLoginPermissions(BusinessObjectFactory factory)
			{
				using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					var details = (APInvoiceChargesApprovalRequestDetails)GetNewBusinessObject();

					Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = false;
					details.Charges.AddNew();
					details.Charges[0].Branch = "SYD";
					details.Charges[0].Department = "FEA";

					details.FilteredCharges.AddNew();
				}
			}
		}
	}
}
