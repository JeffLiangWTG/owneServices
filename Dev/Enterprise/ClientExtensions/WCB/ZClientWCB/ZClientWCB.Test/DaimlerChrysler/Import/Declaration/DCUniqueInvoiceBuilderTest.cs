using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.WCB.DaimlerChrysler.Testing
{
	public class DCUniqueInvoiceBuilderTest : TestCase
	{
		public void TestAddNewHeaderOnly()
		{
			DCUniqueInvoiceBuilder.UniqueInvoices = new Dictionary<ZString, DCUniqueInvoice>();
			AssertEquals("Precondition - no header rows", 0, DCUniqueInvoiceBuilder.UniqueInvoices.Count);
			DCUniqueInvoiceBuilder.AddNewHeaderOnly("key1", Header);
			AssertEquals("One header row", 1, DCUniqueInvoiceBuilder.UniqueInvoices.Count);
			AssertEquals("Correct header row", Header, DCUniqueInvoiceBuilder.UniqueInvoices["key1"].InvoiceHeader);
			DCUniqueInvoiceBuilder.AddNewHeaderOnly("key1", Header);
			AssertEquals("One header row", 1, DCUniqueInvoiceBuilder.UniqueInvoices.Count);
			AssertEquals("Correct header row", Header, DCUniqueInvoiceBuilder.UniqueInvoices["key1"].InvoiceHeader);
			DCUniqueInvoiceBuilder.AddNewHeaderOnly("key2", Header);
			AssertEquals("One header row", 2, DCUniqueInvoiceBuilder.UniqueInvoices.Count);
		}

		[ExpectException(typeof(WCBException))]
		public static void TestAddLineOnlyWithException()
		{
			DCUniqueInvoiceBuilder.UniqueInvoices = new Dictionary<ZString, DCUniqueInvoice>();
			AssertEquals("Precondition - no header rows", 0, DCUniqueInvoiceBuilder.UniqueInvoices.Count);
			DCUniqueInvoiceBuilder.AddLineOnly("key1", new DecInvoiceLineDataRow());
		}

		[ExpectNoExceptions]
		public void TestAddLineOnlySansException()
		{
			DCUniqueInvoiceBuilder.UniqueInvoices = new Dictionary<ZString, DCUniqueInvoice>();
			AssertEquals("Precondition - no header rows", 0, DCUniqueInvoiceBuilder.UniqueInvoices.Count);
			DCUniqueInvoiceBuilder.AddNewHeaderOnly("key1", Header);
			DCUniqueInvoiceBuilder.AddNewHeaderOnly("key2", Header);
			DCUniqueInvoiceBuilder.AddLineOnly("key2", Line);
			DecInvoiceLineDataRow line2 = new DecInvoiceLineDataRow();
			DCUniqueInvoiceBuilder.AddLineOnly("key2", line2);
			AssertEquals("Header rows", 2, DCUniqueInvoiceBuilder.UniqueInvoices.Count);
			AssertEquals("2nd header line rows", 2, DCUniqueInvoiceBuilder.UniqueInvoices["key2"].InvoiceLineCollection.Count);
			AssertEquals("Correct first line", Line, DCUniqueInvoiceBuilder.UniqueInvoices["key2"].InvoiceLineCollection[0]);
			AssertEquals("Correct second line", line2, DCUniqueInvoiceBuilder.UniqueInvoices["key2"].InvoiceLineCollection[1]);
		}

		DecInvoiceHeaderDataRow Header
		{
			get
			{
				if (header == null)
				{
					header = new DecInvoiceHeaderDataRow();
					header.RecordType = "D1";
					header.VesselCode = "MAN003";
					header.VesselName = "MANANON";
					header.Voyage = "SF223344";
					header.RegionalAllocation = "987564";
					header.CountOfVehicles = 10;
					header.FOBAmount = 6.40m;
					header.OceanBill = "OBL1234567";
				}

				return header;
			}
		}

		DecInvoiceHeaderDataRow header;
		DecInvoiceLineDataRow Line
		{
			get
			{
				if (line == null)
				{
					line = new DecInvoiceLineDataRow();
					line.RecordType = "D2";
					line.InvoiceOrderNo = "1234/7895462";
					line.CommissionNo = "COM38372";
					line.ChassisNo = "CCN34223344";
					line.Model = "987564";
					line.Colour = "456";
					line.FOBAmount = 6.40m;
				}

				return line;
			}
		}

		DecInvoiceLineDataRow line;
	}
}
