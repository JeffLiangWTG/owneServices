using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Testing;

namespace Enterprise.Client.UPE.Business.AirCargo.Testing
{
	internal class Level1RecordNoteTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
		}

		public void TestText()
		{
			UPECusHAWB houseBill = TestHelper.HouseBill;
			Level1RecordNote note = new Level1RecordNote(houseBill);

			note.Text = "Note Text";
			AssertEquals("Note text should be stored correctly", "Note Text", note.Text);
			AssertEquals("The note should exist when the text exists", 1, houseBill.Notes.GetAllNotes().Count);

			note.Text = string.Empty;
			AssertEquals("Note text should be deleted", "", note.Text);
			AssertEquals("The note should be deleted when the text is emptied", 0, houseBill.Notes.GetAllNotes().Count);
		}

		public void TestLongText()
		{
			UPECusHAWB houseBill = TestHelper.HouseBill;
			Level1RecordNote note = new Level1RecordNote(houseBill);
			note.Text = new string('x', 200000);
			AssertEquals("Note text should be stored correctly", new string('x', 200000), note.Text);
		}

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper()); }
		}
		UPETestHelper testHelper;
	}
}
