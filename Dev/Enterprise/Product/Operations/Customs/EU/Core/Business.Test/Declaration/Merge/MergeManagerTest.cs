using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public override void TestRequiresMerge()
		{
			var declaration = GetJobDeclaration();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(!declaration.IsDeclarationIntegrated, declaration.MergeManager.RequiresMerge);
		}

		public override void TestSupportsAutoMerge()
		{
			var declaration = GetJobDeclaration();
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(!declaration.IsDeclarationIntegrated, declaration.MergeManager.SupportsAutoMerge);
		}

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Type GetLineMergerType() => typeof(LineMerger);
	}
}
