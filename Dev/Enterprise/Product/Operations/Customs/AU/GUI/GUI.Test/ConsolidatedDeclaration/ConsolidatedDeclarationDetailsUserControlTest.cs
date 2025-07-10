using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class ConsolidatedDeclarationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ConsolidatedDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestEntryNumberTextBox()
		{
			CombineAssertions(() =>
			{
				var entryNumberControl = control.EntryNumberTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Upper, entryNumberControl.CharacterCasing);
				AssertEquals("BindTo", "DeclarationNumber", entryNumberControl.BindTo);
				AssertType<ZTextBox>("Type", entryNumberControl);
			});
		}

		public void TestEntryStatusTextBox()
		{
			CombineAssertions(() =>
			{
				var entryStatusControl = control.EntryStatusTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Upper, entryStatusControl.CharacterCasing);
				AssertEquals("BindTo", "CustomsStatusDescription", entryStatusControl.BindTo);
				AssertType<ZTextBox>("Type", entryStatusControl);
			});
		}

		public void TestMessageStatusTextBox()
		{
			CombineAssertions(() =>
			{
				var messageStatusControl = control.MessageStatusTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, messageStatusControl.CharacterCasing);
				AssertEquals("BindTo", "MessageStatusDescription", messageStatusControl.BindTo);
				AssertType<ZTextBox>("Type", messageStatusControl);
			});
		}

		public void TestMessageStatusTextBoxColor()
		{
			var consolidatedEntry = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 0);
			consolidatedEntry.Factory.Save();

			AssertEquals("PreCondition:There is no outstanding ConsolidatedEntry Change", false, consolidatedEntry.HasConsolidatedEntryChanges);

			using (var form = new ZForm(consolidatedEntry))
			using (var control = new ConsolidatedDeclarationDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var cecLog = consolidatedEntry.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);
				AssertEquals("PreCondition:There is an outstanding ConsolidatedEntry Change", true, consolidatedEntry.HasConsolidatedEntryChanges);
				consolidatedEntry.MessageStatusDescriptionInfo.RefreshBinding();
				AssertEquals("MessageStatusDescriptionTextBox Colour Pink", System.Drawing.Color.LightSalmon, control.MessageStatusTextBox.BackColor);

				cecLog.Cancel();
				AssertEquals("PreCondition:There is no ConsolidatedEntry Change", false, consolidatedEntry.HasConsolidatedEntryChanges);
				consolidatedEntry.MessageStatusDescriptionInfo.RefreshBinding();
				AssertEquals("MessageStatusDescriptionTextBox Colour Grey", System.Drawing.SystemColors.Control, control.MessageStatusTextBox.BackColor);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ConsolidatedDeclarationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ConsolidatedDeclarationDetailsUserControl control;
	}
}
