using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class LowValueShipmentsFormTest : JobDeclarationFormTest
	{
		public void TestDoMergeForLVSJobWithAttachedLVXShipments()
		{
			using (CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABCDEFGH"))
			using (CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var lvsJob = Factory.New<JobDeclaration>();
				lvsJob.JE_MessageType = Business.JobMessageTypeList.Codes.LowValueShipments;
				lvsJob.JE_GB = GlbBranch.CurrentBranch.PK;
				lvsJob.JE_EntryAuthorisationDate = new ZDateTime(2015, 7, 1);
				var invoice1 = lvsJob.Invoices.AddNew();
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				var lvxJob = Factory.New<JobDeclaration>();
				lvxJob.JE_MessageType = Business.JobMessageTypeList.Codes.LVSForConsolidation;
				lvxJob.JE_GB = GlbBranch.CurrentBranch.PK;
				lvxJob.JE_EntryAuthorisationDate = new ZDateTime(2015, 7, 1);
				var invoice2 = lvxJob.Invoices.AddNew();
				var invliceLine2 = invoice2.InvoiceLines.AddNew();
				LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, lvsJob);
				lvsJob.CA_RequiresMerge = false;
				lvxJob.CA_RequiresMerge = false;
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				lvsJob = otherFactory.Load<JobDeclaration>(lvsJob.PK);
				invoice2 = (JobComInvoiceHeader)lvsJob.Invoices.FindByPK(invoice2.PK);
				lvxJob = invoice2.JobDeclaration;
				lvxJob.CA_RequiresMerge = false;
				lvxJob.ResumeApportionment();

				using (var form = new JobDeclarationForm(lvsJob))
				{
					form.Show();
					Assert("Precondition:CA_RequiresMerge for the LVS Job", !lvsJob.CA_RequiresMerge);
					Assert("Precondition:ApportionmentDirty for the LVS Job", !lvsJob.ApportionmentDirty);
					Assert("Precondition:CA_RequiresMerge for the LVX Job", !lvxJob.CA_RequiresMerge);
					Assert("Precondition:ApportionmentDirty for the LVX Job", !lvxJob.ApportionmentDirty);
					Assert("Precondition:IsMergeDone for the LVX Job", !lvxJob.IsMergeDone);

					invoice2.InvoiceLines[0].JI_LinePrice = 10m;
					form.FireSaveButton();

					Assert("CA_RequiresMerge should be set to false on the LVS Job", !lvsJob.CA_RequiresMerge);
					Assert("Apportionment has been run on the LVS Job", !lvsJob.ApportionmentDirty);
					Assert("CA_RequiresMerge should be set to true on the LVX Job", lvxJob.CA_RequiresMerge);
					Assert("Apportionment has been run on the LVX Job", !lvxJob.ApportionmentDirty);
					Assert("No merge should be performed on the LVX Job", !lvxJob.IsMergeDone);
				}
			}
		}

		public override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.LowValueShipments;

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override string[] TransportCodesForFormBashingTest => new[] { Core.Constants.TransportModes.Road };

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		IDisposable asecSetup;
	}
}
