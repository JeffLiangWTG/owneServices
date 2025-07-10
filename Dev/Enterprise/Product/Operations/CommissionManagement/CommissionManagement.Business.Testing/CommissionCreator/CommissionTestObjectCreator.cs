using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionTestObjectCreator
	{
		public TestObjectCreator TestObjectCreator;
		readonly BusinessObjectFactory Factory;

		public CommissionTestObjectCreator(BusinessObjectFactory factory)
		{
			this.Factory = factory;
			this.TestObjectCreator = new TestObjectCreator(Factory);
		}

		public static (AccCommissionHeader AccCommissionHeader, RecipientRatePair RecipientRatePair, AccTransactionHeader AccTransactionHeader) CreateNewTransactionWithCommissionableLines(BusinessObjectFactory factory,
			int numLines,
			bool createLinesOnHeader,
			string ledgerType,
			AccChargeCode chargeCode = null,
			string transactionNum = null,
			AccTransactionHeader existingHeader = null,
			Job job = null,
			RecipientRatePair recipientRatePair = null)
		{
			recipientRatePair = recipientRatePair ?? GetNewRecipientRatePair(factory);

			var transactionHeader = existingHeader ?? factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_PostDate = ZDate.Today;

			if (existingHeader == null)
			{
				transactionHeader.AH_TransactionNum = transactionNum;
			}

			var commissionHeader = factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			commissionHeader.CH0_GroupingSourceID = job != null ? job.PK : transactionHeader.PK;
			commissionHeader.CH0_GroupingSourceTableCode = job != null ? job.TablePrefix : transactionHeader.TablePrefix;
			if (job != null)
			{
				commissionHeader.CH0_JobNumber = job.JH_JobNum;
			}

			if (createLinesOnHeader)
			{
				Enumerable.Range(0, numLines).ForEach(line => SetLineValuesFromRecipientRatePair(commissionHeader.Lines.AddNew(), recipientRatePair));
			}
			else
			{
				var lineGroup = commissionHeader.LineGroups.AddNew();
				lineGroup.CLG_AC = chargeCode != null ? chargeCode.PK : ZGuid.Empty;
				Enumerable.Range(0, numLines).ForEach(line => SetLineValuesFromRecipientRatePair(lineGroup.Lines.AddNew(), recipientRatePair));
			}

			factory.Save();

			return (commissionHeader, recipientRatePair, transactionHeader);
		}

		public static void SetLineValuesFromRecipientRatePair(AccCommissionLine line, RecipientRatePair recipientRatePair)
		{
			line.CL0_GS_NKStaff = recipientRatePair.Recipient.CAR_GS_NKStaff;
			line.CL0_OH_Party = recipientRatePair.Recipient.CAR_OH_Party;
			line.CL0_RX_NKTransactionCurrency = recipientRatePair.Rate.CAT_RX_NKCommissionCurrency;
		}

		public static RecipientRatePair GetNewRecipientRatePair(BusinessObjectFactory factory, string staffCode = "CW1")
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;

			var recipient = factory.NewWithValidTestData<OrgCommissionAgreementRecipient>();
			recipient.CAR_GS_NKStaff = staff.GS_Code;

			var rate = factory.NewWithValidTestData<OrgCommissionAgreementRecipientRate>();
			rate.CAT_RX_NKCommissionCurrency = "AUD";

			factory.Save();

			return new RecipientRatePair(recipient, rate);
		}

		public static List<ViewCommissionLineGrouping> GetGroupings(BusinessObjectFactory factory)
		{
			var viewLineGroupings = new List<ViewCommissionLineGrouping>();
			var viewLines = factory.Load<ViewCommissionLine>(new ZQuery());
			var grouper = new ViewCommissionLineRecipientAndLocalCompanyAndSourceGrouper<ViewCommissionLine>();
			var viewLinesGroups = grouper.GetGroupings(viewLines);

			foreach (var viewLineGroup in viewLinesGroups)
			{
				var viewLineGrouping = new ViewCommissionLineGrouping(factory);
				viewLineGrouping.Init(viewLineGroup);

				viewLineGroupings.Add(viewLineGrouping);
			}

			return viewLineGroupings;
		}

		public OrgHeader Org;
		public Job Job;
		public OrgCommissionAgreement CommissionAgreement1;
		public OrgCommissionAgreement CommissionAgreement2;
		public InvoicingBase JobInvoice;
		public InvoicingBase Invoice;
		public InvoicingLineBase JobLine1;
		public InvoicingLineBase JobLine2;
		public InvoicingLineBase InvoiceLine;

		public void SetupTransactionsAndCommissions(bool createLinesForJobOnly = false, bool closeJob = false)
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Org.Addresses.AddNew();
			address1.OA_Address1 = "72 O'Riordan St";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ACL";

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();

			CommissionAgreement1 = opportunity1.ApprovedCommissionAgreements.AddNew();
			CommissionAgreement1.CA0_OH_Customer = Org.PK;
			CommissionAgreement1.FillWithValidTestData();
			CommissionAgreement1.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			CommissionAgreement1.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			CommissionAgreement1.CA0_EffectiveDate = ZDate.Today;

			CommissionAgreement2 = opportunity1.ApprovedCommissionAgreements.AddNew();
			CommissionAgreement2.CA0_OH_Customer = Org.PK;
			CommissionAgreement2.FillWithValidTestData();
			CommissionAgreement2.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			CommissionAgreement2.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			CommissionAgreement2.CA0_EffectiveDate = ZDate.Today;

			var item1 = CommissionAgreement1.ProductItems.AddNew(true, "SHP");
			item1.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;

			var item2 = CommissionAgreement2.ProductItems.AddNew(true, "ALL");
			item2.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;

			var agreementPctRecipient1 = CommissionAgreement1.Recipients.AddNew();
			agreementPctRecipient1.CAR_GS_NKStaff = "ACL";
			agreementPctRecipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient1.CAR_Share = 1;

			var agreementPctRecipient1Rate = agreementPctRecipient1.Rates.AddNew();
			agreementPctRecipient1Rate.CAT_CommissionPercentage = 10;

			var agreementPctRecipient2 = CommissionAgreement2.Recipients.AddNew();
			agreementPctRecipient2.CAR_GS_NKStaff = "ACL";
			agreementPctRecipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient2.CAR_Share = 1;

			var agreementPctRecipient2Rate = agreementPctRecipient2.Rates.AddNew();
			agreementPctRecipient2Rate.CAT_CommissionPercentage = 10;

			Factory.Save();

			TestObjectCreator.CC1.AC_IsCommissionable = true;
			TestObjectCreator.CC2.AC_IsCommissionable = true;

			var shipment1 = TestObjectCreator.CreateShipment("1001");

			Job = TestObjectCreator.CreateJob(shipment1);
			Job.JH_OA_LocalChargesAddr = address1.PK;

			JobInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			JobInvoice.AH_InvoiceAmount = 1000;
			JobInvoice.AH_OH = Org.PK;

			JobLine1 = TestObjectCreator.CreateInvoiceLine(JobInvoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			JobLine1.AL_LineType = TransactionLineTypes.Revenue;
			JobLine1.AL_JH = Job.PK;
			JobLine1.AL_AC = TestObjectCreator.CC1.PK;

			JobLine2 = TestObjectCreator.CreateInvoiceLine(JobInvoice, TestObjectCreator.AUD, 1, 200, 10, 0);
			JobLine2.AL_LineType = TransactionLineTypes.Revenue;
			JobLine2.AL_JH = Job.PK;
			JobLine2.AL_AC = TestObjectCreator.CC2.PK;

			if (!createLinesForJobOnly)
			{
				Invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1);
				Invoice.AH_InvoiceAmount = 500;
				Invoice.AH_OH = Org.PK;

				InvoiceLine = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 500, 10, 0);
				InvoiceLine.AL_LineType = TransactionLineTypes.Revenue;
				InvoiceLine.AL_AC = TestObjectCreator.CC1.PK;
			}

			TestObjectCreator.CreateCharge(JobLine1);
			TestObjectCreator.CreateCharge(JobLine2);

			Factory.Save();

			if (closeJob)
			{
				Job.Close(null, null);
				Factory.Save();
			}
		}
	}
}
