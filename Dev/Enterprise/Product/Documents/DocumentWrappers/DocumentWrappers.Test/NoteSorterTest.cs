using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class NoteSorterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestInvalidNoteDoesntThrowException()
		{
			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.AddNew();
			collection.AddNew();
			collection.Sort(new NoteSorter());
		}

		public void TestCompareMarksAndNumbers()
		{
			NoteA.ST_Description = MarksAndNumbers;
			NoteB.ST_Description = GoodsDescription;

			StmNoteCollection collection = new StmNoteCollection(Factory.New<ForwardingShipment>(), Factory);
			collection.Add(NoteB);
			collection.Add(NoteA);
			collection.Sort(new NoteSorter());
			AssertEquals(MarksAndNumbers, collection[0].ST_Description);
			AssertEquals(GoodsDescription, collection[1].ST_Description);

			NoteA.ST_Description = BookingNotes;
			NoteB.ST_Description = MarksAndNumbers;
			NoteC.ST_Description = LoadListInstructions;
			NoteD.ST_Description = GoodsDescription;
			collection.Add(NoteC);
			collection.Add(NoteD);
			collection.Sort(new NoteSorter());
			AssertEquals(MarksAndNumbers, collection[0].ST_Description);
			AssertEquals(GoodsDescription, collection[1].ST_Description);
			AssertEquals(BookingNotes, collection[2].ST_Description);
			AssertEquals(LoadListInstructions, collection[3].ST_Description);
		}

		StmNote NoteA;
		StmNote NoteB;
		StmNote NoteC;
		StmNote NoteD;
		readonly ZString MarksAndNumbers = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
		readonly ZString GoodsDescription = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
		readonly ZString LoadListInstructions = PredefinedNoteTypes.Instance.LoadListInstructions.Description;
		readonly ZString BookingNotes = PredefinedNoteTypes.Instance.BookingNotes.Description;

		protected override void SetUp()
		{
			NoteA = Factory.New<StmNote>();
			NoteB = Factory.New<StmNote>();
			NoteC = Factory.New<StmNote>();
			NoteD = Factory.New<StmNote>();
			base.SetUp();
		}
	}
}
