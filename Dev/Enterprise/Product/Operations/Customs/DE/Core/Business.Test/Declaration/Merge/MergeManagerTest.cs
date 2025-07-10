using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public void TestReasonCannotMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				var errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
				AssertEquals("There is no EntryInstruction linked to any line", "Cannot merge as no invoice line has an entry instruction assigned.", errorCondition);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;

				errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
				AssertEquals("There is now an EntryInstruction linked to one invoice line", ZString.Empty, errorCondition);
			});
		}

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Type GetLineMergerType() => typeof(LineMerger);
	}
}
