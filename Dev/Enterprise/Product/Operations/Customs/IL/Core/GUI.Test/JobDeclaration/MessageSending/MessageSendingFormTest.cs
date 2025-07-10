using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : MessageSendingObjectFormTest
	{
		public void TestMessageSendingObjectsGridColumns()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);

			using (var testForm = new MessageSendingFormForTest(jobDeclarationMessageSendingObjectParent))
			{
				testForm.Show();

				AssertEquals(7, testForm.MessageSendingObjectsGrid.ColumnStyles.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "ShouldSend", "MessageType", "MessageTypeDescription", "DeclarationType", "EntryStatus", "Procedure", "EntryInstructionDescription" }, testForm.MessageSendingObjectsGrid.Columns.Select(x => x.ColumnName));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration));
		}

		class MessageSendingFormForTest : MessageSendingForm
		{
			public MessageSendingFormForTest(JobDeclarationMessageSendingObjectParent declarationWrapper)
				: base(declarationWrapper)
			{
			}

			public new ZGrid MessageSendingObjectsGrid => base.MessageSendingObjectsGrid;
		}
	}
}
