using System;
using CargoWise.Types;

namespace Enterprise.Client.TNT.Testing
{
	public class FlightRecordTest : IQDownBaseRecordTest
	{
		public override void TestFieldProperty()
		{
			FlightRecord record = new FlightRecord(DataString);
			AssertEquals("Record Type", "01", record.RecordType);
			AssertEquals("Record Key", "BA015112527013486 SINSYD150705A", record.RecordKey);
			AssertEquals("Flight Number", "BA0151", record.FlightNumber);
			AssertEquals("Carrier", "BA", record.Carrier);
			AssertEquals("Carrier Number", "0151", record.CarrierNumber);
			record.FlightNumber = "GD3423";
			AssertEquals("Flight Number", "GD3423", record.FlightNumber);
			AssertEquals("Carrier", "GD", record.Carrier);
			AssertEquals("Carrier Number", "3423", record.CarrierNumber);
			AssertEquals("Record Key", "GD342312527013486 SINSYD150705A", record.RecordKey);
			record.Carrier = "DD";
			AssertEquals("Carrier", "DD", record.Carrier);
			AssertEquals("Flight Number", "DD3423", record.FlightNumber);
			AssertEquals("Record Key", "DD342312527013486 SINSYD150705A", record.RecordKey);
			record.CarrierNumber = "5612";
			AssertEquals("Carrier Number", "5612", record.CarrierNumber);
			AssertEquals("Flight Number", "DD5612", record.FlightNumber);
			AssertEquals("Record Key", "DD561212527013486 SINSYD150705A", record.RecordKey);
			AssertEquals("Port Of Loading", "SIN", record.PortOfLoading);
			record.PortOfLoading = "Port Of Loading";
			AssertEquals("Port Of Loading", "Port Of Loading", record.PortOfLoading);
			AssertEquals("Record Key", "DD561212527013486 PorSYD150705A", record.RecordKey);
			AssertEquals("Port Of Discharge", "SYD", record.PortOfDischarge);
			record.PortOfDischarge = "Port Of Discharge";
			AssertEquals("Port Of Discharge", "Port Of Discharge", record.PortOfDischarge);
			AssertEquals("Record Key", "DD561212527013486 PorPor150705A", record.RecordKey);
			AssertEquals("Flight Date", new ZDateTime(2005, 7, 15), record.FlightDate);
			AssertEquals("Flight Date", "150705", record[FlightRecord.Schema.FlightDate.Name]);
			record.FlightDate = new ZDateTime(2005, 9, 30);
			AssertEquals("Flight Date", new ZDateTime(2005, 9, 30), record.FlightDate);
			AssertEquals("Flight Date", "300905", record[FlightRecord.Schema.FlightDate.Name]);
			AssertEquals("Record Key", "DD561212527013486 PorPor300905A", record.RecordKey);
			record[FlightRecord.Schema.FlightDate.Name] = "221104";
			AssertEquals("Flight Date", new ZDateTime(2004, 11, 22), record.FlightDate);
			AssertEquals("Flight Date", "221104", record[FlightRecord.Schema.FlightDate.Name]);
			AssertEquals("Record Key", "DD561212527013486 PorPor221104A", record.RecordKey);
			AssertEquals("Mode", "A", record.Mode);
			record.Mode = "Mode";
			AssertEquals("Mode", "Mode", record.Mode);
			AssertEquals("Record Key", "DD561212527013486 PorPor221104M", record.RecordKey);
			AssertEquals("MasterBill", "12527013486 ", record.MasterBill);
			record.MasterBill = "MasterBill";
			AssertEquals("MasterBill", "MasterBill", record.MasterBill);
			AssertEquals("Record Key", "DD5612MasterBill  PorPor221104M", record.RecordKey);
			AssertEquals("MBag Number", "M0000151  ", record.MBagNo);
			record.MBagNo = "MBag Number";
			AssertEquals("MBag Number", "MBag Number", record.MBagNo);
			AssertEquals("MBag Origin", "SIN", record.MBagOrigin);
			record.MBagOrigin = "MBag Origin";
			AssertEquals("MBag Origin", "MBag Origin", record.MBagOrigin);
			AssertEquals("Mbag Destination", "SYD", record.MBagDestination);
			record.MBagDestination = "Mbag Destination";
			AssertEquals("Mbag Destination", "Mbag Destination", record.MBagDestination);
			AssertEquals("MBag Type", "TP", record.MBagType);
			record.MBagType = "MBag Type";
			AssertEquals("MBag Type", "MBag Type", record.MBagType);
			AssertEquals("MBag Weight", 231.423m, record.MBagWeight);
			AssertEquals("MBag Weight", "   231.423", record[FlightRecord.Schema.MBagWeight.Name]);
			record.MBagWeight = 4534.45311m;
			AssertEquals("MBag Weight", 4534.453m, record.MBagWeight);
			AssertEquals("MBag Weight", "4534.453", record[FlightRecord.Schema.MBagWeight.Name]);
			record[FlightRecord.Schema.MBagWeight.Name] = "521.23123";
			AssertEquals("MBag Weight", 521.231m, record.MBagWeight);
			AssertEquals("MBag Weight", "521.23123", record[FlightRecord.Schema.MBagWeight.Name]);
			AssertEquals("Spaces", ZString.Replicate(' ', 427), record.spaces);
			AssertEquals("Record Delimiter", ".", record.RecordDelimiter);
		}

#region TestHumanReadable
		public override void TestHumanReadable()
		{
			FlightRecord record = new FlightRecord(DataString);
			AssertEquals("HumanReadable", GenerateExpectedHumanReadable(record.RecordType, record.MasterBill), record.HumanReadable);
			record.MasterBill = "MASTERTEST";
			AssertEquals("HumanReadable", GenerateExpectedHumanReadable(record.RecordType, "MASTERTEST"), record.HumanReadable);
		}

		ZString GenerateExpectedHumanReadable(ZString recordType, ZString masterBill)
		{
			return ZString.Format("Record {0} (MasterBill={1})", recordType, masterBill);
		}

#endregion
		protected override IQDownBaseRecord GetRecord(ZString rawData)
		{
			return new FlightRecord(rawData);
		}

		protected override ZString DataString
		{
			get
			{
				return "01BA0151SINSYD150705A12527013486 M0000151  SINSYDTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
			}
		}

		protected override Type ExpectedRecordType
		{
			get
			{
				return typeof(FlightRecord);
			}
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return 15;
			}
		}
	}
}
