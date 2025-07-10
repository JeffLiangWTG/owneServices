using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	public class ConsignmentNoteRecordCollectionTest : TestCase
	{
		public void TestConstructor()
		{
			ConsignmentNoteRecordCollection collection = new ConsignmentNoteRecordCollection();
			AssertEquals(0, collection.Count);
		}

		public void TestAdd()
		{
			ConsignmentNoteRecordCollection collection = new ConsignmentNoteRecordCollection();
			ConsignmentNoteRecord record = new ConsignmentNoteRecord(Line2);
			collection.Add(record);
			AssertEquals(1, collection.Count);
			ConsignmentNoteRecord chkRecord = collection[0];
			AssertEquals(record, chkRecord);
			record = new ConsignmentNoteRecord(Line3);
			collection.Add(record);
			AssertEquals("No new record added as a record with same HouseBill and Sequence already exists", 1, collection.Count);
			record = new ConsignmentNoteRecord(Line1);
			collection.Add(record);
			AssertEquals(2, collection.Count);
		}

		public void TestIndexer_UsingInteger()
		{
			ConsignmentNoteRecordCollection collection = new ConsignmentNoteRecordCollection();
			ConsignmentNoteRecord record = new ConsignmentNoteRecord(Line1);
			collection.Add(record);
			AssertEquals(1, collection.Count);
			object chkRecord = collection[0];
			AssertNotNull(chkRecord);
			AssertEquals(record, chkRecord);
		}

		public void TestIndexer_UsingHouseBillAndSequence()
		{
			ConsignmentNoteRecordCollection collection = new ConsignmentNoteRecordCollection();
			ConsignmentNoteRecord record1 = new ConsignmentNoteRecord(Line1);
			collection.Add(record1);
			ConsignmentNoteRecord record2 = new ConsignmentNoteRecord(Line2);
			collection.Add(record2);
			ConsignmentNoteRecord record3 = new ConsignmentNoteRecord(Line3);
			record3.Sequence = 2;
			collection.Add(record3);
			AssertEquals(3, collection.Count);
			object chkRecord = collection[record1.HouseBill, record1.Sequence];
			AssertNotNull(chkRecord);
			AssertEquals(record1, chkRecord);
			chkRecord = collection[record2.HouseBill, record2.Sequence];
			AssertNotNull(chkRecord);
			AssertEquals(record2, chkRecord);
			chkRecord = collection[record3.HouseBill, record3.Sequence];
			AssertNotNull(chkRecord);
			AssertEquals(record3, chkRecord);
			chkRecord = collection["balas", 12];
			AssertNull(chkRecord);
		}

		#region Implementation
		const string Line1 = "04940432180 01123456789012345DOCUMENTS AND DOCS.IN FOLDER                                                  " + "DEscription 2                                                                 " + "Description 3                                                                 " + "MELIAHEX2456789012                                                                                                                                                                                                                .";
		const string Line2 = "04435423434 01123456789012345DOCUMENTS AND DOCS.IN FOLDER                                                  " + "DEscription 2                                                                 " + "Description 3                                                                 " + "SINSYDEX2434549012                                                                                                                                                                                                                .";
		const string Line3 = "04435423434 01123456789012345DOCUMENTS AND DOCS.IN FOLDER   dsfdsfdafasdfsfasdfsadfsadfsa                  " + "DEscription 2 asdfdsfsafasdfsadfsdfsadfsa                                     " + "Description 3 asfsafdasfasdfsadfadfsaasdfsd                                   " + "SINSYDEX2434549012                                                                                                                                                                                                                .";
		#endregion
	}
}
