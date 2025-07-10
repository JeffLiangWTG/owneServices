using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	public abstract class JobDeclarationMessageManagerTest : Customs.Business.Testing.BaseMessageManagerTest
	{
		public void TestMergeIfNecessary()
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("invoices not merged", 0, declaration.CustomsEntryHeaders.Count);
			manager.MergeIfNecessary(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			AssertNotEquals("invoices not merged", 0, declaration.CustomsEntryHeaders.Count);
		}

		#region Implementation
		protected void SetStatusAndSave(CusEntryHeader entry, ZString status)
		{
			entry.CH_Status = status;
			entry.Factory.Save();
		}

		protected JobDeclaration declaration;
		protected JobDeclarationMessageManager manager;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetNewJobDeclaration();
			manager = GetNewJobDeclarationMessageManager(declaration);
		}

		protected virtual JobDeclaration GetNewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		protected abstract JobDeclarationMessageManager GetNewJobDeclarationMessageManager(JobDeclaration declaration);
		#endregion Implementation
	}
}
