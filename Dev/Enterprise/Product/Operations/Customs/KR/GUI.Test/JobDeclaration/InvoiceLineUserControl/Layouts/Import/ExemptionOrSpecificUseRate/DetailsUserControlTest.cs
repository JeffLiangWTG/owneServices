using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class DetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(Business.JobDeclaration), control.BindingSource.DataSourceType);
			var dutyReductionTypeDropEdit = control.FindSingle<ZDropEdit>("DutyReductionTypeDropEdit");
			var specificUseCheckBox = control.FindSingle<ZCheckBox>("SpecificUseCheckBox");
			var dutyReductionCodeFindBox = control.FindSingle<ZCodeFindBox>("DutyReductionCodeFindBox");
			var instalmentCodeFindBox = control.FindSingle<ZCodeFindBox>("InstalmentCodeFindBox");
			var remarkTextBox = control.FindSingle<ZTextBox>("RemarkTextBox");

			AssertEquals("FilteredInvoiceLines.DutyReductionClassificationCode", dutyReductionTypeDropEdit.BindTo);
			AssertEquals("FilteredInvoiceLines.JI_IsSpecificUseCode", specificUseCheckBox.BindTo);
			AssertEquals("FilteredInvoiceLines.JI_SecondaryPreference", dutyReductionCodeFindBox.BindTo);
			AssertEquals("FilteredInvoiceLines.JI_InstallmentCode", instalmentCodeFindBox.BindTo);
			AssertEquals("FilteredInvoiceLines.AdditionalInformationContent", remarkTextBox.BindTo);

			Assert(!dutyReductionTypeDropEdit.ReadOnly);
			Assert(specificUseCheckBox.ReadOnly);
			Assert(dutyReductionCodeFindBox.ReadOnly);
			Assert(instalmentCodeFindBox.ReadOnly);
			Assert(!remarkTextBox.ReadOnly);

			control.BindToMessageSendingObject();
			dutyReductionTypeDropEdit = control.FindSingle<ZDropEdit>("DutyReductionTypeDropEdit");
			specificUseCheckBox = control.FindSingle<ZCheckBox>("SpecificUseCheckBox");
			dutyReductionCodeFindBox = control.FindSingle<ZCodeFindBox>("DutyReductionCodeFindBox");
			instalmentCodeFindBox = control.FindSingle<ZCodeFindBox>("InstalmentCodeFindBox");
			remarkTextBox = control.FindSingle<ZTextBox>("RemarkTextBox");

			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.DutyReductionClassificationCode", dutyReductionTypeDropEdit.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_IsSpecificUseCode", specificUseCheckBox.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_SecondaryPreference", dutyReductionCodeFindBox.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_InstallmentCode", instalmentCodeFindBox.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.AdditionalInformationContent", remarkTextBox.BindTo);

			Assert(dutyReductionTypeDropEdit.ReadOnly);
			Assert(specificUseCheckBox.ReadOnly);
			Assert(dutyReductionCodeFindBox.ReadOnly);
			Assert(instalmentCodeFindBox.ReadOnly);
			Assert(remarkTextBox.ReadOnly);
		}
		protected override void SetUp()
		{
			base.SetUp();
			control = new DetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DetailsUserControl control;
	}
}
