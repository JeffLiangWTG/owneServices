using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class DutyReductionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(Business.JobDeclaration), control.BindingSource.DataSourceType);
			var groupNumberDropEdit = control.FindSingle<ZDropEdit>("GroupNumberDropEdit");
			var seqNumberTextBox = control.FindSingle<ZTextBox>("SeqNumberTextBox");
			var itemNumberTextBox = control.FindSingle<ZTextBox>("ItemNumberTextBox");

			AssertEquals("FilteredInvoiceLines.DutyReductionGroupNumber", groupNumberDropEdit.BindTo);
			AssertEquals("FilteredInvoiceLines.DutyReductionSeqNumber", seqNumberTextBox.BindTo);
			AssertEquals("FilteredInvoiceLines.DutyReductionItemNumber", itemNumberTextBox.BindTo);

			Assert(!groupNumberDropEdit.ReadOnly);
			Assert(!seqNumberTextBox.ReadOnly);
			Assert(!itemNumberTextBox.ReadOnly);

			control.BindToMessageSendingObject();
			groupNumberDropEdit = control.FindSingle<ZDropEdit>("GroupNumberDropEdit");
			seqNumberTextBox = control.FindSingle<ZTextBox>("SeqNumberTextBox");
			itemNumberTextBox = control.FindSingle<ZTextBox>("ItemNumberTextBox");

			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.DutyReductionGroupNumber", groupNumberDropEdit.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.DutyReductionSeqNumber", seqNumberTextBox.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.DutyReductionItemNumber", itemNumberTextBox.BindTo);

			Assert(groupNumberDropEdit.ReadOnly);
			Assert(seqNumberTextBox.ReadOnly);
			Assert(itemNumberTextBox.ReadOnly);
		}
		protected override void SetUp()
		{
			base.SetUp();
			control = new DutyReductionDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DutyReductionDetailsUserControl control;
	}
}
