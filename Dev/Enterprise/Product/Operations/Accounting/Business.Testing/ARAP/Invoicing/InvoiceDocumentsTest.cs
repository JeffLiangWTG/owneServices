using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(Invoice))]
	public class InvoiceDocumentsTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			return invoice;
		}

		public void TestBusinessContextDependsOnLedger()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			AssertEquals("Should be the ARInvoice business context", CargoWise.Definitions.BusinessContext.ARInvoice, aRInv.DocumentSupporter.BusinessContext);
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			AssertEquals("Should be the APInvoice business context", CargoWise.Definitions.BusinessContext.APInvoice, aPInv.DocumentSupporter.BusinessContext);
			UAInvoice uAInv = Factory.NewWithValidTestData<UAInvoice>();
			AssertEquals("Should be the APInvoice business context", CargoWise.Definitions.BusinessContext.APInvoice, uAInv.DocumentSupporter.BusinessContext);
		}

		public void TestTransactionNumberShouldIncludeBranchCodeWhenPostingAtBranchLevel()
		{
			var creator = new TestObjectCreator(Factory);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "COM";
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "GB1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "GB2";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "AAA";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				TransactionNumberSequenceCustomisationCollection collection = new TransactionNumberSequenceCustomisationCollection();
				AddTransactionNumberSequenceCustomisation(collection, "Transaction Header Branch Code", ZString.Empty, 3, 1, true, false);
				AddTransactionNumberSequenceCustomisation(collection, "Transaction Header Department Code", ZString.Empty, 3, 2, true, true);
				AddTransactionNumberSequenceCustomisation(collection, "Custom Element 1", "AB", 2, 3, false, false);
				AddTransactionNumberSequenceCustomisation(collection, "Custom Element 2", "XY", 2, 4, false, false);
				AddTransactionNumberSequenceCustomisation(collection, "Sequence Number", ZString.Empty, 8, 5, true, true);
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true });

				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
				var line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
				line1.AL_AG = creator.GLHeader1.PK;
				line2.AL_AG = creator.GLHeader1.PK;

				AssertEquals("GB1", line1.Branch.GB_Code);
				AssertEquals("GB1", line2.Branch.GB_Code);

				line2.AL_GB = branch2.PK;
				Factory.Save();

				AssertEquals("GB1AAA00000001", invoice1.AH_TransactionNum);
				AssertEquals("GB2AAA00000002", invoice2.AH_TransactionNum);

				var copiedInvocie = (ARInvoice)invoice1.CopyTransaction_ForTestOnly();
				AssertEquals(1, copiedInvocie.Lines.Count);
				var line3 = copiedInvocie.Lines[0];
				AssertEquals("GB1", line3.Branch.GB_Code);
				line3.AL_GB = branch2.PK;
				Factory.Save();

				AssertEquals("GB2AAA00000003", copiedInvocie.AH_TransactionNum);
			}
		}

		public void TestTransactionNumberShouldBeBasedOnFountain()
		{
			var creator = new TestObjectCreator(Factory);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "COM";
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "GB1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "GB2";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "AAA";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				TransactionNumberSequenceCustomisationCollection collection = new TransactionNumberSequenceCustomisationCollection();
				AddTransactionNumberSequenceCustomisation(collection, "Transaction Header Branch Code", ZString.Empty, 3, 1, true, true);
				AddTransactionNumberSequenceCustomisation(collection, "Transaction Header Department Code", ZString.Empty, 3, 2, true, true);
				AddTransactionNumberSequenceCustomisation(collection, "Custom Element 1", "AB", 2, 3, false, false);
				AddTransactionNumberSequenceCustomisation(collection, "Custom Element 2", "XY", 2, 4, false, false);
				AddTransactionNumberSequenceCustomisation(collection, "Sequence Number", ZString.Empty, 8, 5, true, true);
				AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true });

				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
				line1.AL_AG = creator.GLHeader1.PK;
				Factory.Save();
				AssertEquals("GB1AAA00000001", invoice1.AH_TransactionNum);

				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				var line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
				line2.AL_AG = creator.GLHeader1.PK;
				line2.AL_GB = branch2.PK;
				Factory.Save();

				AssertEquals(invoice2.AH_GB, branch2.PK);
				AssertEquals("GB2AAA00000001", invoice2.AH_TransactionNum);
			}
		}

		protected TransactionNumberSequenceCustomisation AddTransactionNumberSequenceCustomisation(TransactionNumberSequenceCustomisationCollection collection, ZString elementName, ZString code, ZInt length, ZByte order, ZBool include, ZBool fountain)
		{
			TransactionNumberSequenceCustomisation customisation = collection.AddNew();
			customisation.ElementName = elementName;
			customisation.Code = code;
			customisation.Include = include;
			customisation.Fountain = fountain;
			if (elementName == "Sequence Number")
			{
				customisation.Length = length;
			}
			customisation.Order = order;
			return customisation;
		}
	}
}
