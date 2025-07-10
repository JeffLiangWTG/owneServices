using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Netting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting.Testing
{
	public class HelperMethodTest : TestCaseWithFactory
	{
		public void TestTransactionReferenceHelperMethods()
		{
			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, "90881");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_1");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_2");

			Assert("Referece value set for JobInvoiceNumber", transaction.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber));
			Assert("Referece value not set for VesselVoyage", !transaction.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.VesselVoyage));
			Assert("Referece value set for ConsolContainerNumber", transaction.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber));

			AssertEquals("90881", transaction.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber));
			AssertEquals(2, transaction.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber).Length);
		}

		public void TestTransactionLineReferenceHelperMethods()
		{
			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			var line1 = (NettingReceivableTransactionLine)testObjectCreator.CreateNettingTransactionLine(transaction, "SH1", 300M, "USD");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_2");

			Assert("Referece value set for ShipmentNumber", line1.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ShipmentNumber));
			Assert("Referece value not set for HouseBill", !line1.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.HouseBill));
			Assert("Referece value set for ConsolContainerNumber", line1.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber));

			AssertEquals("SH1", line1.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.ShipmentNumber));
			AssertEquals(2, line1.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber).Length);
		}

		public void TestGetStatus()
		{
			var reference = ZString.Format("AR{0}INV{0}{1}", InvoiceAdditionalReference.Separator, InvoiceAdditionalReference.Posted);
			AssertEquals(InvoiceAdditionalReference.Posted, HelperMethods.GetStatus(reference));

			reference = ZString.Format("AP{0}CRD{0}{1}", InvoiceAdditionalReference.Separator, InvoiceAdditionalReference.FullyMatched);
			AssertEquals(InvoiceAdditionalReference.FullyMatched, HelperMethods.GetStatus(reference));

			reference = ZString.Format("AR{0}ADJ{0}{1}", InvoiceAdditionalReference.Separator, InvoiceAdditionalReference.UndoFullyMatched);
			AssertEquals(InvoiceAdditionalReference.UndoFullyMatched, HelperMethods.GetStatus(reference));

			reference = "ABC";
			AssertEquals(ZString.Empty, HelperMethods.GetStatus(reference));

			reference = "";
			AssertEquals(ZString.Empty, HelperMethods.GetStatus(reference));
		}

		NettingObjectCreator testObjectCreator;
		NettingSystem nettingSystem;
		NettingSystemPeriod period;
		NettingOrganisation issuer;
		OrgHeader localIssuer;
		NettingOrganisation recipient;
		OrgHeader localRecipient;

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new NettingObjectCreator(Factory);
			nettingSystem = testObjectCreator.CreateNettingSystem("NS1", "Test Netting System", GlbCompany.CurrentCompany);
			period = testObjectCreator.CreateNettingPeriod(nettingSystem, "201504", ZDateTime.Today, ZDateTime.Today.AddDays(30), ZDateTime.Today.AddDays(25)
				, ZDateTime.Today.AddDays(37), ZDateTime.Today.AddDays(40), ZDateTime.Today.AddDays(45), ZDate.Today.AddDays(30));
			localIssuer = testObjectCreator.CreateOrgHeader("Issuer1", true, true);
			issuer = CreateNettingOrgHeader(nettingSystem, localIssuer, "EDIISR001");
			localRecipient = testObjectCreator.CreateOrgHeader("Recipient1", true, true);
			recipient = CreateNettingOrgHeader(nettingSystem, localRecipient, "EDIRCT001");

			Factory.Save();
		}

		NettingOrganisation CreateNettingOrgHeader(NettingSystem ns, OrgHeader org, string eHubID)
		{
			org.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);
			return testObjectCreator.CreateNettingOrganisation(ns, org, "CUR");
		}
	}
}
