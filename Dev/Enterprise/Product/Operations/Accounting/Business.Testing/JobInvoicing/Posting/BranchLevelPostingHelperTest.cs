using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	class BranchLevelPostingHelperTest : TestCaseWithFactory
	{
		public void TestIsUsedBy()
		{
			AssertType<BranchLevelPostingHelper>(ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper());
		}

		public void TestGetHeaderBranch()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("CCC", GlbCompany.CurrentCompany);
			var branch4 = TestObjectCreator.CreateBranch("DDD", GlbCompany.CurrentCompany);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "2322", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);

			var invoiceLine1 = invoice.Lines[0];
			invoiceLine1.AL_GB = branch1.PK;

			//adding another line to the invoice and choosing the same branch as in line1
			var invoiceLine2 = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine2.AL_GB = branch1.PK;

			var lineBranchPKs = invoice.Lines.Cast<DependentTransactionLine>().Select(x => x.AL_GB).OrderBy(y => y).ToHashSet();
			var actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, ZGuid.Empty, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);
			AssertEquals("No configuration is setup for branch goruping and invoice has only one branch, header branch is simply the line branch", branch1.PK, actualHeaderbranch);

			//choose a different branch in line2 than in line1
			invoiceLine2.AL_GB = branch2.PK;

			lineBranchPKs = invoice.Lines.Cast<DependentTransactionLine>().Select(x => x.AL_GB).OrderBy(y => y).ToHashSet();
			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, ZGuid.Empty, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			//With EnforceBranchLevelPosting registry turned on and no branch grouping setup, this is an invalid state
			//BranchLevelPostingHelper.GetHeaderBranch should not return any result
			AssertEquals("No configuration is setup for branch goruping and invoice lines with different branches, no header branch is found", ZGuid.Empty, actualHeaderbranch);

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);

			AssertEquals("No configuration is setup encompassing both the branches, no header branch is found", ZGuid.Empty, actualHeaderbranch);

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, ZGuid.Empty, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, ZGuid.Empty, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			AssertEquals("branch goruping is setup with branch 2 as the parent branch and invoice lines with different branches, branch 2 is selected as the header branch as per config", branch2.PK, actualHeaderbranch);

			//job branch is passed to the helper method, job branch should get precedence over fallback logic in branch grouping fallback
			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, branch1.PK, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			AssertEquals("branch goruping is setup with branch 2 as the parent branch and invoice lines with different branches. But branch job is used in one of the lines so branch job is selected as the header branch", branch1.PK, actualHeaderbranch);

			//a third branch is set as the job branch and it is not part of the branch grouping
			//This is not a valid condition hence no header branch should be selected
			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, branch3.PK, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			AssertEquals("Job branch is provided as a branch which is not part of the branch grouping, hence job branch will be ignored and parent branch form the configuration will be chosen", branch2.PK, actualHeaderbranch);

			var settings3 = new BranchGroupSettings();
			settings3.BranchPK = branch3.PK;
			settings3.GroupNumber = 2;
			settings3.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings3);

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			//branch3 is set to be part of a different group than branch1 and branch2
			//so no improvement on the previous invalid condition
			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, branch3.PK, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			AssertEquals("Job branch is provided as a branch which is not part of the branch grouping, hence job branch will be continued to be ignored and parent branch from the configuration will be chosen", branch2.PK, actualHeaderbranch);

			//branch3 is made part of the same branch group as branch1 and branch2
			//But the header branch should be the overriden branch from the registry
			settings3.GroupNumber = 1;
			settings3.IsParentBranch = false;

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, branch3.PK, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			AssertEquals("Job branch is provided as a branch which is not used in any of the lines though is in the same branch group config, job branch will be continued to be ignored and branch from cofig will be chosen", branch2.PK, actualHeaderbranch);

			//Added a new settings for branch4 and it is made as the parent branch
			settings2.IsParentBranch = false;

			var settings4 = new BranchGroupSettings();
			settings4.BranchPK = branch4.PK;
			settings4.GroupNumber = 1;
			settings4.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings4);

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, branch3.PK, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			AssertEquals("Job branch is provided as a branch which is not any of the line branches, the header branch should be decided from the registry fallback, in this case Branch4 should be the header branch even though it is not used in any of the lines", branch4.PK, actualHeaderbranch);

			var invoiceLine3 = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine3.AL_GB = branch3.PK;

			lineBranchPKs = invoice.Lines.Cast<DependentTransactionLine>().Select(x => x.AL_GB).OrderBy(y => y).ToHashSet();
			actualHeaderbranch = BranchLevelPostingHelper.GetHeaderBranchForTestOnly(lineBranchPKs, branch3.PK, AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting);

			AssertEquals("Job branch is provided as a branch which is one of the line branches finally, so header branch is picked as the job branch", branch3.PK, actualHeaderbranch);
		}

		public void TestGetTransactionHeaderBranchForOverrideBranchAndDepartment()
		{
			var originalBranch = TestObjectCreator.CreateBranch("OBR", GlbCompany.CurrentCompany);
			var branch1 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", GlbCompany.CurrentCompany);
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1122", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			invoice.AH_GB = originalBranch.PK;

			invoice.Lines.AddNew();
			invoice.Lines[0].AL_AG = invoice.Lines[1].AL_AG = TestObjectCreator.GLHeader1.PK;

			//lines have different branches but in the same group
			invoice.Lines[0].AL_GB = branch1.PK;
			invoice.Lines[1].AL_GB = branch2.PK;

			Factory.Save();

			//Setup branch grouping and defaulting configuration
			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = true;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = false;

			invoice.AH_GB = branch2.PK;

			IBranchLevelPostingHelper branchLevelPostingHelper = new BranchLevelPostingHelper();

			//Both registry - turned off
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var apBranchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, apBranchLevelPostingConfiguration);
			AssertGetTransactionHeaderBranchForOverrideBranchAndDepartment("Invoice Header Branch - Original Value is picked to set invoice header branch", originalBranch.PK);

			//Post to login branch registry - turned on & AP branch level posting registry - turned off
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertGetTransactionHeaderBranchForOverrideBranchAndDepartment("Current Login Branch is picked to set invoice header branch", GlbBranch.CurrentBranch.PK);

			//Both registry - turned on
			apBranchLevelPostingConfiguration.EnableBranchLevelPosting = true;
			apBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			apBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, apBranchLevelPostingConfiguration);
			AssertGetTransactionHeaderBranchForOverrideBranchAndDepartment("Invoice Line Branch is picked to set invoice header branch", branch1.PK);

			//Post to login Branch registry - turned off & AP branch level posting registry - turned on
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertGetTransactionHeaderBranchForOverrideBranchAndDepartment("Invoice Line branch is picked to set invoice header branch as per the logic", branch1.PK);

			var shipment = TestObjectCreator.CreateShipment("S0010011");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			job.JH_GB = branch2.PK;

			var charges = new IReceivablesPostingChargeCollection();
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, "110", TestObjectCreator.AUD, 120M, TestObjectCreator.ABIGAS);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, "110", TestObjectCreator.AUD, 120M, TestObjectCreator.ABIGAS);
			charges.Add(charge1);
			charges.Add(charge2);

			invoice.AH_JH = job.PK;

			//When invoice is Job related (AH_JH NOT empty) + invoice has more than one lines + EnforceBranchLevelPosting registry - yes + job branch- NOT empty & any invoicelines contains the Job branch, then Job branch is picked.
			AssertGetTransactionHeaderBranchForOverrideBranchAndDepartment("Invoice Job Header branch is picked to set invoice header branch", branch2.PK);

			void AssertGetTransactionHeaderBranchForOverrideBranchAndDepartment(string expectedMessage, ZGuid expectedBranch)
			{
				var actualBranch = branchLevelPostingHelper.GetTransactionHeaderBranchForOverrideBranchAndDepartment(invoice);

				AssertEquals(expectedMessage, expectedBranch, actualBranch);
			}
		}

		public void TestSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment()
		{
			var originalBranch = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", GlbCompany.CurrentCompany);
			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1122", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "2244", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);

			invoice1.AH_GB = invoice2.AH_GB = originalBranch.PK;
			invoice1.Lines[0].AL_GB = invoice2.Lines[0].AL_GB = branch2.PK;

			invoice2.SetContext(BusinessContext.OverrideTransactionBranchAndDepartment);

			Assert("PreCondition: invoice1 NOT set to Context", !invoice1.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment));
			Assert("PreCondition: invoice2 set to Context", invoice2.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment));

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var singleActionMock = new Mock<ISingleActionPerTransactionOnDifferentLevels>();

			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

			IBranchLevelPostingHelper branchLevelPostingHelper = new BranchLevelPostingHelper();
			var invoiceLevel = InvoiceProcessingLevelIsAllowingToResetBranch.Saving;

			//Both registry - turned on
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var apBranchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, apBranchLevelPostingConfiguration);
			AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment("Invoice1 branch is set as per the logic", invoice1, branch2.PK);
			AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment("Invoice2 branch does not change", invoice2, originalBranch.PK);

			invoice1.AH_GB = originalBranch.PK;

			//Post to login branch registry - turned off & AP branch level posting registry - turned on
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment("Invoice1 branch is set as per the logic", invoice1, branch2.PK);
			AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment("Invoice2 branch does not change", invoice2, originalBranch.PK);

			invoice1.AH_GB = originalBranch.PK;

			//Both registry - turned off
			apBranchLevelPostingConfiguration.EnableBranchLevelPosting = false;
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, apBranchLevelPostingConfiguration);
			AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment("Invoice1 branch is header branch as per the logic inside SetTransactionHeaderBranch, so remains same", invoice1, originalBranch.PK);
			AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment("Invoice2 branch does not change", invoice2, originalBranch.PK);

			invoice1.AH_GB = originalBranch.PK;

			//Post to login Branch registry - turned on & AP branch level posting registry - turned off
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment("Invoice1 branch is set as per the logic", invoice1, GlbBranch.CurrentBranch.PK);
			AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment("Invoice2 branch does not change", invoice2, originalBranch.PK);

			void AssertSetTransactionHeaderBranch_Context_OverrideTransactionBranchAndDepartment(string expectedMessage, InvoicingBase invoicingBase, ZGuid expectedBranch)
			{
				mockIAccountingDependencyFactory.Reset();
				singleActionMock.Reset();

				mockIAccountingDependencyFactory.Setup(x => x.GetSingleActionPerTransactionOnDifferentLevels()).Returns(singleActionMock.Object);
				singleActionMock.Setup(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(invoicingBase, It.IsAny<string>(), It.IsAny<Action>(), (int)invoiceLevel))
					.Callback<BusinessObject, string, Action, int>((bizo, actionIdUniqueOnFactoryLevel, actionToRun, invoiceProcessingLevel) => actionToRun());

				branchLevelPostingHelper.SetTransactionHeaderBranch(invoicingBase, invoiceLevel);

				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(It.IsAny<InvoicingBase>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<int>()), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(invoicingBase, actionIdUniqueOnFactoryLevel, It.IsAny<Action>(), (int)invoiceLevel));

				AssertEquals(expectedMessage, expectedBranch, invoicingBase.AH_GB);
			}
		}

		public void TestSetTransactionHeaderBranch_Transaction()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			var originalBranchPK = TestObjectCreator.NonCurrentBranch.PK;

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var singleActionMock = new Mock<ISingleActionPerTransactionOnDifferentLevels>();

			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

			IBranchLevelPostingHelper branchLevelPostingHelper = new BranchLevelPostingHelper();
			var invoiceLevel = InvoiceProcessingLevelIsAllowingToResetBranch.Creation;

			//Setup branch grouping and defaulting configuration
			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = true;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = false;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "2322", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			var invoiceLine1 = invoice.Lines[0];
			var invoiceLine2 = (InvoicingLineBase)invoice.Lines.AddNew();

			//lines have different branches but in the same group
			invoiceLine1.AL_GB = branch1.PK;
			invoiceLine2.AL_GB = branch2.PK;

			var uaInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(UAInvoice), "3434", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			uaInvoice.Lines[0].AL_GB = branch1.PK;

			var directReceipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Today, 200M, 0M, 200M, 0M);
			var drLine1 = directReceipt.Lines[0];
			var drLine2 = directReceipt.Lines[1];

			//lines have different branches but in the same group
			drLine1.AL_GB = branch1.PK;
			drLine2.AL_GB = branch2.PK;

			invoice.AH_GB = uaInvoice.AH_GB = directReceipt.AH_GB = originalBranchPK;

			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var apBranchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
			var arBranchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, apBranchLevelPostingConfiguration);
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, arBranchLevelPostingConfiguration);

			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(invoice, "Invoice branch - all registry turned off", TestObjectCreator.NonCurrentBranch.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(uaInvoice, "UAInvoice branch - all registry turned off", TestObjectCreator.NonCurrentBranch.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(directReceipt, "DirectReceipt branch - all registry turned off", TestObjectCreator.NonCurrentBranch.PK);

			invoice.AH_GB = uaInvoice.AH_GB = directReceipt.AH_GB = originalBranchPK;

			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(invoice, "Invoice branch - post to login company registry turned on", GlbBranch.CurrentBranch.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(uaInvoice, "UAInvoice branch - post to login company registry turned on", TestObjectCreator.NonCurrentBranch.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(directReceipt, "DirectReceipt branch - post to login company registry turned on", GlbBranch.CurrentBranch.PK);

			invoice.AH_GB = uaInvoice.AH_GB = directReceipt.AH_GB = originalBranchPK;

			arBranchLevelPostingConfiguration.EnableBranchLevelPosting = true;
			arBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			arBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, arBranchLevelPostingConfiguration);

			//Setting ReceivableEnforceBranchLevelPosting does not have any impact on AP invoices. it only impacts AR transaction. 
			//Branch1 is picked as the header branch according to the fallback logic
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(invoice, "Invoice branch - post to login company registry + AR branch level posting registry turned on", GlbBranch.CurrentBranch.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(uaInvoice, "UAInvoice branch - post to login company registry + AR branch level posting registry turned on", TestObjectCreator.NonCurrentBranch.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(directReceipt, "DirectReceipt branch - post to login company registry + AR branch level posting registry turned on", branch1.PK);

			invoice.AH_GB = uaInvoice.AH_GB = directReceipt.AH_GB = originalBranchPK;

			apBranchLevelPostingConfiguration.EnableBranchLevelPosting = true;
			apBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			apBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, apBranchLevelPostingConfiguration);

			//Setting PayableEnforceBranchLevelPosting only impacts applicable AP invoices
			//When invoice is NOT Job related (AH_JH empty) + invoice has more than one lines + EnforceBranchLevelPosting registry - yes + then the branch is which is Parent Branch from the invoice line branches is picked.
			//Branch1 is picked as the header branch according to the fallback logic
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(invoice, "Invoice branch - post to login company registry + AR/AP branch level posting registry turned on", branch1.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(uaInvoice, "UAInvoice branch - post to login company registry + AR/AP branch level posting registry turned on", TestObjectCreator.NonCurrentBranch.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(directReceipt, "DirectReceipt branch - post to login company registry + AR/AP branch level posting registry turned on", branch1.PK);

			invoice.AH_GB = uaInvoice.AH_GB = directReceipt.AH_GB = originalBranchPK;

			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			//PostJobInvoicingTransactionsToLoginBranch does not have precedence over EnforceBranchLevelPosting
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(invoice, "Invoice branch - post to login company registry off + AR/AP branch level posting registry turned on", branch1.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(uaInvoice, "UAInvoice branch - post to login company registry off + AR/AP branch level posting registry turned on", TestObjectCreator.NonCurrentBranch.PK);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(directReceipt, "DirectReceipt branch - post to login company registry off + AR/AP branch level posting registry turned on", branch1.PK);

			void AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(TransactionHeaderWithLines transactionHeaderWithLines, string message, ZGuid expectedBranchPK)
			{
				mockIAccountingDependencyFactory.Reset();
				singleActionMock.Reset();

				mockIAccountingDependencyFactory.Setup(x => x.GetSingleActionPerTransactionOnDifferentLevels()).Returns(singleActionMock.Object);
				singleActionMock.Setup(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(transactionHeaderWithLines, It.IsAny<string>(), It.IsAny<Action>(), (int)invoiceLevel));

				branchLevelPostingHelper.SetTransactionHeaderBranch(transactionHeaderWithLines, invoiceLevel);

				mockIAccountingDependencyFactory.Verify(x => x.GetSingleActionPerTransactionOnDifferentLevels(), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(It.IsAny<TransactionHeaderWithLines>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<int>()), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(transactionHeaderWithLines, actionIdUniqueOnFactoryLevel, It.IsAny<Action>(), (int)invoiceLevel));

				AssertEquals(originalBranchPK, transactionHeaderWithLines.AH_GB);

				mockIAccountingDependencyFactory.Reset();
				singleActionMock.Reset();

				mockIAccountingDependencyFactory.Setup(x => x.GetSingleActionPerTransactionOnDifferentLevels()).Returns(singleActionMock.Object);
				singleActionMock.Setup(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(transactionHeaderWithLines, It.IsAny<string>(), It.IsAny<Action>(), (int)invoiceLevel))
					.Callback<BusinessObject, string, Action, int>((bizo, actionIdUniqueOnFactoryLevel, actionToRun, invoiceProcessingLevel) => actionToRun());

				branchLevelPostingHelper.SetTransactionHeaderBranch(transactionHeaderWithLines, invoiceLevel);

				mockIAccountingDependencyFactory.Verify(x => x.GetSingleActionPerTransactionOnDifferentLevels(), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(It.IsAny<TransactionHeaderWithLines>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<int>()), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(transactionHeaderWithLines, actionIdUniqueOnFactoryLevel, It.IsAny<Action>(), (int)invoiceLevel));

				AssertEquals(message, expectedBranchPK, transactionHeaderWithLines.AH_GB);
			}
		}

		public void TestSetTransactionHeaderBranch_Charges()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			var originalBranchPK = TestObjectCreator.NonCurrentBranch.PK;

			//Setup branch grouping and defaulting configuration
			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = true;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = false;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			job.JH_GB = branch1.PK;
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			apInvoice.AH_JH = job.PK;
			arInvoice.AH_JH = job.PK;

			Assert("Precondition: We do not expect invoice lines as we use charges instead of invoice lines.", !apInvoice.Lines.Any());
			Assert("Precondition: We do not expect invoice lines as we use charges instead of invoice lines.", !arInvoice.Lines.Any());

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, "110", TestObjectCreator.AUD, 120M, TestObjectCreator.ABIGAS);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, "110", TestObjectCreator.AUD, 120M, TestObjectCreator.ABIGAS);
			charge1.JR_GB = branch2.PK;
			charge2.JR_GB = branch2.PK;

			var receivablesPostingCharges = new IReceivablesPostingChargeCollection();
			var payablesInvoiceCharges = new APInvoiceCharges(charge1.CostAccount.OH_Code, charge1.JR_APInvoiceNum, job.PK, job.TablePrefix, null);
			payablesInvoiceCharges = new APInvoiceCharges(charge2.CostAccount.OH_Code, charge2.JR_APInvoiceNum, job.PK, job.TablePrefix, null);

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var singleActionMock = new Mock<ISingleActionPerTransactionOnDifferentLevels>();

			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

			IBranchLevelPostingHelper branchLevelPostingHelper = new BranchLevelPostingHelper();
			var invoiceLevel = InvoiceProcessingLevelIsAllowingToResetBranch.Creation;

			apInvoice.AH_GB = arInvoice.AH_GB = originalBranchPK;

			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var apBranchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
			var arBranchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, apBranchLevelPostingConfiguration);
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, arBranchLevelPostingConfiguration);

			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(apInvoice, "all registry turned off", TestObjectCreator.NonCurrentBranch.PK, payablesInvoiceCharges);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(arInvoice, "all registry turned off", TestObjectCreator.NonCurrentBranch.PK, receivablesPostingCharges);

			apInvoice.AH_GB = arInvoice.AH_GB = originalBranchPK;

			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(apInvoice, "post to login company registry turned on", GlbBranch.CurrentBranch.PK, payablesInvoiceCharges);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(arInvoice, "post to login company registry turned on", GlbBranch.CurrentBranch.PK, receivablesPostingCharges);

			apInvoice.AH_GB = arInvoice.AH_GB = originalBranchPK;

			apBranchLevelPostingConfiguration.EnableBranchLevelPosting = true;
			arBranchLevelPostingConfiguration.EnableBranchLevelPosting = true;
			Assert("Precondition", !payablesInvoiceCharges.Charges.Any());
			Assert("Precondition", !receivablesPostingCharges.Any());
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(apInvoice, "post to login company registry + AP branch level posting registry turned on, but charges collection is empty", GlbBranch.CurrentBranch.PK, payablesInvoiceCharges);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(arInvoice, "post to login company registry + AR branch level posting registry turned on, but charges collection is empty", GlbBranch.CurrentBranch.PK, receivablesPostingCharges);

			apBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			apBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);
			arBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			arBranchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);
			payablesInvoiceCharges.Charges.Add(charge1);
			payablesInvoiceCharges.Charges.Add(charge2);
			receivablesPostingCharges.Add(charge1);
			receivablesPostingCharges.Add(charge2);

			apInvoice.AH_GB = arInvoice.AH_GB = originalBranchPK;

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, apBranchLevelPostingConfiguration);
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, arBranchLevelPostingConfiguration);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(apInvoice, "post to login company registry + AP branch level posting registry turned on", branch2.PK, payablesInvoiceCharges);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(arInvoice, "post to login company registry + AR branch level posting registry turned on", branch2.PK, receivablesPostingCharges);

			apInvoice.AH_GB = arInvoice.AH_GB = originalBranchPK;

			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(apInvoice, "post to login company registry + AP branch level posting registry turned on", branch2.PK, payablesInvoiceCharges);
			AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(arInvoice, "post to login company registry + AR branch level posting registry turned on", branch2.PK, receivablesPostingCharges);

			void AssertInvoicingBaseBranchLevelPostingHelper_SetTransactionHeaderBranch(TransactionHeaderWithLines transactionHeaderWithLines, string message, ZGuid expectedBranchPK, ITransactionBranchCalculationDataProviderFromJobCharge receivablesOrPayablesCharges)
			{
				mockIAccountingDependencyFactory.Reset();
				singleActionMock.Reset();

				mockIAccountingDependencyFactory.Setup(x => x.GetSingleActionPerTransactionOnDifferentLevels()).Returns(singleActionMock.Object);
				singleActionMock.Setup(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(transactionHeaderWithLines, It.IsAny<string>(), It.IsAny<Action>(), (int)invoiceLevel));

				branchLevelPostingHelper.SetTransactionHeaderBranch(transactionHeaderWithLines, invoiceLevel, receivablesOrPayablesCharges);

				mockIAccountingDependencyFactory.Verify(x => x.GetSingleActionPerTransactionOnDifferentLevels(), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(It.IsAny<TransactionHeaderWithLines>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<int>()), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(transactionHeaderWithLines, actionIdUniqueOnFactoryLevel, It.IsAny<Action>(), (int)invoiceLevel));

				AssertEquals(originalBranchPK, transactionHeaderWithLines.AH_GB);

				mockIAccountingDependencyFactory.Reset();
				singleActionMock.Reset();

				mockIAccountingDependencyFactory.Setup(x => x.GetSingleActionPerTransactionOnDifferentLevels()).Returns(singleActionMock.Object);
				singleActionMock.Setup(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(transactionHeaderWithLines, It.IsAny<string>(), It.IsAny<Action>(), (int)invoiceLevel))
					.Callback<BusinessObject, string, Action, int>((bizo, actionIdUniqueOnFactoryLevel, actionToRun, invoiceProcessingLevel) => actionToRun());

				branchLevelPostingHelper.SetTransactionHeaderBranch(transactionHeaderWithLines, invoiceLevel, receivablesOrPayablesCharges);

				mockIAccountingDependencyFactory.Verify(x => x.GetSingleActionPerTransactionOnDifferentLevels(), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(It.IsAny<TransactionHeaderWithLines>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<int>()), Times.Once);
				singleActionMock.Verify(mock => mock.RunSingleActionPerTransactionOnDifferentLevels(transactionHeaderWithLines, actionIdUniqueOnFactoryLevel, It.IsAny<Action>(), (int)invoiceLevel));

				AssertEquals(message, expectedBranchPK, transactionHeaderWithLines.AH_GB);
			}
		}

		public void TestGetParentBranch()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("CCC", GlbCompany.CurrentCompany);

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch2.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch3.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			AssertEquals("Branch1 does not have any branch goruping configuraion setup, so Branch1 is its own parent", branch1.PK, BranchLevelPostingHelper.GetParentBranchPK(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, branch1.PK));
			AssertEquals("Parent of Branch2 is Branch3", branch3.PK, BranchLevelPostingHelper.GetParentBranchPK(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, branch2.PK));
			AssertEquals("Parent of Branch3 is Branch3", branch3.PK, BranchLevelPostingHelper.GetParentBranchPK(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, branch3.PK));
		}

		public void TestGetAssociatedBranches()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("CCC", GlbCompany.CurrentCompany);

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch2.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch3.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			AssertContainsExactElementsInAnyOrder("Branch1 is not part of any group", new List<ZGuid> { branch1.PK }, BranchLevelPostingHelper.GetAssociatedBranches(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, branch1.PK));
			AssertContainsExactElementsInAnyOrder("Branch2 and Branch3 are part of same group", new List<ZGuid> { branch2.PK, branch3.PK }
			, BranchLevelPostingHelper.GetAssociatedBranches(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, branch2.PK));
			AssertContainsExactElementsInAnyOrder("Branch2 and Branch3 are part of same group", new List<ZGuid> { branch2.PK, branch3.PK }
			, BranchLevelPostingHelper.GetAssociatedBranches(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, branch3.PK));
		}

		public void TestDoesAllBranchesBelongToSamePostingGroup()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany); //Does not have any group configuration
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany); //Branch2 and Branch3 is part of same group
			var branch3 = TestObjectCreator.CreateBranch("CCC", GlbCompany.CurrentCompany); //Branch2 and Branch3 is part of same group
			var branch4 = TestObjectCreator.CreateBranch("DDD", GlbCompany.CurrentCompany); //Branch4 forms its own group
			var branch5 = TestObjectCreator.CreateBranch("EEE", GlbCompany.CurrentCompany); //Does not have any group configuration

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch2.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch3.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			var settings3 = new BranchGroupSettings();
			settings3.BranchPK = branch4.PK;
			settings3.GroupNumber = 2;
			settings3.IsParentBranch = true;

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			Assert("Branch1 is not part of any Group, call to DoesAllBranchesBelongToSamePostingGroup() should return true"
				, BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, new HashSet<ZGuid> { branch1.PK }));

			Assert("Branch1 and Branch2 is part of 2 different groups, so DoesAllBranchesBelongToSamePostingGroup() should return false"
				, !BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, new HashSet<ZGuid> { branch1.PK, branch2.PK }));

			Assert("Branch2 and Branch3 is part of the same group, so DoesAllBranchesBelongToSamePostingGroup() should return true"
				, BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, new HashSet<ZGuid> { branch2.PK, branch3.PK }));

			Factory.Save();

			Assert("Order of the the PKs should not matter"
				, BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, new HashSet<ZGuid> { branch3.PK, branch2.PK }));

			Assert("Branch2 and Branch4 is part of 2 different groups, so DoesAllBranchesBelongToSamePostingGroup() should return false"
				, !BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, new HashSet<ZGuid> { branch2.PK, branch4.PK }));

			Assert("Branch4 forms its own group, so DoesAllBranchesBelongToSamePostingGroup() should return true"
				, BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, new HashSet<ZGuid> { branch4.PK }));

			Assert("Branch1 and Branch4 does not have any group settings, that means they should not be posted in the same invoice"
				, !BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, new HashSet<ZGuid> { branch1.PK, branch4.PK }));
		}

		TestObjectCreator TestObjectCreator => (fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)));
		TestObjectCreator fTestObjectCreator;

		const string actionIdUniqueOnFactoryLevel = "Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper";
	}
}
