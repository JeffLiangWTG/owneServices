using System;
using CargoWise.Types;

namespace Enterprise.Client.TNT.Testing
{
	public class ConsignmentNoteRecordTest : IQDownBaseRecordTest
	{
		public override void TestFieldProperty()
		{
			ConsignmentNoteRecord record = new ConsignmentNoteRecord(DataString);
			AssertEquals("Record Type", "04", record.RecordType);
			AssertEquals("Consignment Number", "940432180 ", record.HouseBill);
			record.HouseBill = "Consignment Number";
			AssertEquals("Consignment Number", "Consignment Number", record.HouseBill);
			AssertEquals("Sequence Number", 1, record.Sequence);
			record.Sequence = 43;
			AssertEquals("Sequence Number", 43, record.Sequence);
			AssertEquals("Tariff Number", "123456789012345", record.TariffNumber);
			record.TariffNumber = "Tariff Number";
			AssertEquals("Tariff Number", "Tariff Number", record.TariffNumber);
			AssertEquals("Consignment NoteText", "DOCUMENTS AND DOCS.IN FOLDER DEscription 2 Description 3", record.NoteText);
			AssertEquals("Consignment Description Line 1", "DOCUMENTS AND DOCS.IN FOLDER".PadRight(78), record.Description1);
			record.Description1 = "Consignment Description Line 1";
			AssertEquals("Consignment Description Line 1", "Consignment Description Line 1", record.Description1);
			AssertEquals("Consignment Description Line 2", "DEscription 2".PadRight(78), record.Description2);
			record.Description2 = "Consignment Description Line 2";
			AssertEquals("Consignment Description Line 2", "Consignment Description Line 2", record.Description2);
			AssertEquals("Consignment Description Line 3", "Description 3".PadRight(78), record.Description3);
			record.Description3 = "Consignment Description Line 3";
			AssertEquals("Consignment Description Line 3", "Consignment Description Line 3", record.Description3);
			AssertEquals("Consignment NoteText", "Consignment Description Line 1 Consignment Description Line 2 Consignment Description Line 3", record.NoteText);
			AssertEquals("Consignment Origin", "MEL", record.Origin);
			record.Origin = "Consignment Origin";
			AssertEquals("Consignment Origin", "Consignment Origin", record.Origin);
			AssertEquals("Consignment Destination", "IAH", record.Destination);
			record.Destination = "Consignment Destination";
			AssertEquals("Consignment Destination", "Consignment Destination", record.Destination);
			AssertEquals("Consignment ECN/EDN", "EX2456789012 ", record.ECN);
			record.ECN = "Consignment ECN/EDN";
			AssertEquals("Consignment ECN/EDN", "Consignment ECN/EDN", record.ECN);
			AssertEquals("Spaces", ZString.Replicate(' ', 207), record.spaces);
			AssertEquals("Record Delimiter", ".", record.RecordDelimiter);
		}

#region TestHumanReadable
		public override void TestHumanReadable()
		{
			ConsignmentNoteRecord record = new ConsignmentNoteRecord(DataString);
			AssertEquals("HumanReadable", GenerateExpectedHumanReadable(record.RecordType, record.HouseBill, record.Sequence), record.HumanReadable);
			record.HouseBill = "BOB";
			record.Sequence = 32;
			AssertEquals("HumanReadable", GenerateExpectedHumanReadable(record.RecordType, "BOB", 32), record.HumanReadable);
		}

		ZString GenerateExpectedHumanReadable(ZString recordType, ZString houseBill, ZInt sequence)
		{
			return ZString.Format("Record {0} (Con Number={1}, Sequence={2})", recordType, houseBill, sequence);
		}

#endregion
		protected override IQDownBaseRecord GetRecord(ZString rawData)
		{
			return new ConsignmentNoteRecord(rawData);
		}

		protected override ZString DataString
		{
			get
			{
				return "04940432180 01123456789012345DOCUMENTS AND DOCS.IN FOLDER                                                  " + "DEscription 2                                                                 " + "Description 3                                                                 " + "MELIAHEX2456789012                                                                                                                                                                                                                .";
			}
		}

		protected override Type ExpectedRecordType
		{
			get
			{
				return typeof(ConsignmentNoteRecord);
			}
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return 12;
			}
		}
	}
}
