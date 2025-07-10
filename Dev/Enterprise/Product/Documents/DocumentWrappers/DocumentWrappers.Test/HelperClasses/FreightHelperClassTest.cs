using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class FreightHelperClassTest : TestCaseWithFactory
	{
		public void TestAddNoteWithAndWithoutNoteContext()
		{
			DummyEnterpriseBusinessObject dummy1 = Factory.New<DummyEnterpriseBusinessObject>();
			FreightHelperClass.AddNote(dummy1, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Data without Context");
			AssertEquals("One note expected to be on the bizo.", 1, dummy1.GetNotes().GetAllNotes().Count);
			AssertEquals(StmNoteContextUtils.StmNoteContextsAllToString, ((StmNoteCollection)dummy1.GetNotes().GetAllNotes())[0].ST_NoteContext);

			DummyEnterpriseBusinessObject dummy2 = Factory.New<DummyEnterpriseBusinessObject>();
			StmNoteContexts airStmNoteContext = new StmNoteContexts();
			airStmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			airStmNoteContext.Module |= StmNoteContextModule.A;
			airStmNoteContext.Direction |= StmNoteContextDirection.A;
			FreightHelperClass.AddNote(dummy2, PredefinedNoteTypes.Instance.LoadListInstructions.Description, "Data with Context", airStmNoteContext);
			AssertEquals("One note expected to be on the bizo.", 1, dummy2.GetNotes().GetAllNotes().Count);
			AssertEquals(airStmNoteContext.Module.ToString() + airStmNoteContext.Direction.ToString() + airStmNoteContext.FreightMode.ToString(), ((StmNoteCollection)dummy2.GetNotes().GetAllNotes())[0].ST_NoteContext);
		}

		public void TestFormatBillAndIssueDate()
		{
			AssertEquals("Only bill number", "12345", FreightHelperClass.FormatBillAndIssueDate("12345", ZDateTime.Empty));

			ZDateTime date = ZDateTime.Today;
			AssertEquals("Only date", " / " + date.ToShortDateString(), FreightHelperClass.FormatBillAndIssueDate("", date));
			AssertEquals("Bill # and date", "12345 / " + date.ToShortDateString(), FreightHelperClass.FormatBillAndIssueDate("12345", date));
		}

		public void TestFormatBillAndIssueHeading()
		{
			AssertEquals("Only bill number", "HEADING", FreightHelperClass.FormatBillAndIssueHeading("HEADING", ZDateTime.Empty));

			ZDateTime date = ZDateTime.Today;
			AssertEquals("Only date", " / ISSUE", FreightHelperClass.FormatBillAndIssueHeading("", date));
			AssertEquals("Bill # and date", "HEADING / ISSUE", FreightHelperClass.FormatBillAndIssueHeading("HEADING", date));
		}

		public void TestMergePackMarksAndNumbers()
		{
			string marksString1 = "Marks";
			string marksString2 = "DifferentMarks";

			CommonShipment shipment = Factory.New<CommonShipment>();

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_MarksAndNumbers = marksString1;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_MarksAndNumbers = marksString2;

			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_MarksAndNumbers = marksString1;

			ZString result = FreightHelperClass.MergePackMarksAndNumbers(
				new PackLine[] { packLine1, packLine2, packLine3 }
			);

			AssertEquals(marksString1 + System.Environment.NewLine + marksString2, result);
		}
	}
}
