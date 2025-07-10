using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSHOTFileConverterTest : TestCaseWithFactory
	{
		readonly NotificationTestHelper notificationTestHelper = new NotificationTestHelper();

		[TestDate(2015, 11, 05)]
		public void TestConverter()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("DCO Y17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618200000000002000000000300000000000000000000000400 K0002150MEX");
				writer.WriteLine("DCR N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					var header = new CASSCostHeader();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);

					AssertHeader(header, "CEXP", new ZDateTime(2008, 08, 01, 0, 0, 0), new ZDateTime(2008, 08, 31, 0, 0, 0), new ZDateTime(2008, 09, 10, 0, 0, 0), ZString.Empty, "AAA");
					AssertEquals(3, header.Lines.Count);

					var line = header.ExportLines[0];
					AssertEquals("line 1: RecordType", "AWM", line.RecordType);
					AssertEquals("line 1: AirlinePrefix", "160", line.AirlinePrefix);
					AssertEquals("line 1: AWBSerialNumber", "62144154", line.AWBSerialNumber);
					AssertEquals("line 1: AgentCode", "23470068510", line.AgentCode);
					AssertEquals("line 1: DateAWBExecution", new ZDateTime(2008, 08, 14), line.DateAWBExecution);
					AssertEquals("line 1: DateOfArrival", ZDateTime.Empty, line.DateOfArrival);
					AssertEquals("line 1: DateOfDelivery", ZDateTime.Empty, line.DateOfDelivery);
					AssertEquals("line 1: Origin", "DUS", line.Origin);
					AssertEquals("line 1: Destination", "HKG", line.Destination);
					AssertEquals("line 1: Weight", 1190.00M, line.Weight);
					AssertEquals("line 1: WeightUnit", "KG", line.WeightUnit);
					AssertEquals("line 1: Currency", "EUR", line.CurrencyCode);
					AssertEquals("line 1: WeightChargePP", 3462.90M, line.WeightChargePP);
					AssertEquals("line 1: ValuationChargePP", 30.00M, line.ValuationChargePP);
					AssertEquals("line 1: ChargesDueCarrierPP", 1808.80M, line.ChargesDueCarrierPP);
					AssertEquals("line 1: ChargesDueAgentCC", 20.00M, line.ChargesDueAgentCC);
					AssertEquals("line 1: Commission", 10.00M, line.Commission);
					AssertEquals("line 1: Discount", 1320.90M, line.Discount);
					AssertEquals("line 1: VATDueAirline", 100002.01M, line.VATDueAirline);
					AssertEquals("line 1: VATDueAgent", 0M, line.VATDueAgent);

					var line1 = header.ExportLines[1];
					AssertEquals("line 2: RecordType", "DCO", line1.RecordType);
					AssertEquals("line 2: AirlinePrefix", "172", line1.AirlinePrefix);
					AssertEquals("line 2: AWBSerialNumber", "67828073", line1.AWBSerialNumber);
					AssertEquals("line 2: AgentCode", "23470068510", line1.AgentCode);
					AssertEquals("line 2: DateAWBExecution", new ZDateTime(2008, 06, 07), line1.DateAWBExecution);
					AssertEquals("line 2: DateOfArrival", ZDateTime.Empty, line1.DateOfArrival);
					AssertEquals("line 2: DateOfDelivery", ZDateTime.Empty, line1.DateOfDelivery);
					AssertEquals("line 2: Origin", "LEJ", line1.Origin);
					AssertEquals("line 2: Destination", "MEX", line1.Destination);
					AssertEquals("line 2: Weight", 215.0M, line1.Weight);
					AssertEquals("line 2: WeightUnit", "KG", line1.WeightUnit);
					AssertEquals("line 2: Currency", "EUR", line1.CurrencyCode);
					AssertEquals("line 2: WeightChargePP", 473.00M, line1.WeightChargePP);
					AssertEquals("line 2: ValuationChargePP", 0M, line1.ValuationChargePP);
					AssertEquals("line 2: ChargesDueCarrierPP", 216.18M, line1.ChargesDueCarrierPP);
					AssertEquals("line 2: ChargesDueAgentCC", 2.00M, line1.ChargesDueAgentCC);
					AssertEquals("line 2: Commission", 3.00M, line1.Commission);
					AssertEquals("line 2: Discount", 4.00M, line1.Discount);
					AssertEquals("line 2: VATDueAirline", 2000000000.02M, line1.VATDueAirline);
					AssertEquals("line 2: VATDueAgent", 0M, line1.VATDueAgent);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestConverter_ImportRecords()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AA2JP054000004  10080110081510082301JPY");
				writer.WriteLine("IBI        00533186 NNJP054000004006A00670158700        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBO        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBR        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					var header = new CASSCostHeader();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);

					AssertHeader(header, "CIMP", new ZDateTime(2010, 08, 01, 0, 0, 0), new ZDateTime(2010, 08, 15, 0, 0, 0), new ZDateTime(2010, 08, 23, 0, 0, 0), "JPY", "AA2");
					AssertEquals(3, header.Lines.Count);

					var line = header.ImportLines[0];
					AssertEquals("Line 1: RecordType", "IBI", line.RecordType);
					AssertEquals("Line 1: AirlinePrefix", "006", line.AirlinePrefix);
					AssertEquals("Line 1: AWBSerialNumber", "70158700", line.AWBSerialNumber);
					AssertEquals("Line 1: AgentCode", "JP054000004", line.AgentCode);
					AssertEquals("Line 1: DateAWBExecution", ZDateTime.Empty, line.DateAWBExecution);
					AssertEquals("Line 1: DateOfArrival", new ZDateTime(2010, 08, 04), line.DateOfArrival);
					AssertEquals("Line 1: DateOfDelivery", new ZDateTime(2010, 08, 05), line.DateOfDelivery);
					AssertEquals("Line 1: Origin", "MGA", line.Origin);
					AssertEquals("Line 1: Destination", "NRT", line.Destination);
					AssertEquals("Line 1: Weight", 140.0M, line.Weight);
					AssertEquals("Line 1: WeightUnit", "KG", line.WeightUnit);
					AssertEquals("Line 1: Currency", "USD", line.CurrencyCode);
					AssertEquals("Line 1: WeightCharges", 415.60M, line.WeightCharges);
					AssertEquals("Line 1: ChargesDueAgentCC", 5000.00M, line.ChargesDueAgentCC);
					AssertEquals("Line 1: ChargesDueCarrierCC", 2300.00M, line.ChargesDueCarrierCC);
					AssertEquals("Line 1: FeeCharged", true, line.FeeCharged);
					AssertEquals("Line 1: FeeAmount", 30.00M, line.FeeAmount);
					AssertEquals("Line 1: HandlingCharges", 0.77M, line.HandlingCharges);
					AssertEquals("Line 1: StorageCharges", 4400000000.44M, line.StorageCharges);
					AssertEquals("Line 1: OtherCharge1Amount", 1200000000.21M, line.OtherCharge1Amount);
					AssertEquals("Line 1: OtherCharge2Amount", 6700000000.67M, line.OtherCharge2Amount);
					AssertEquals("Line 1: MiscellaneousChargesAmount", 1400000000.41M, line.MiscellaneousChargesAmount);

					var line1 = header.ImportLines[1];
					AssertEquals("Line 2: RecordType", "IBO", line1.RecordType);
					AssertEquals("Line 2: AirlinePrefix", "006", line1.AirlinePrefix);
					AssertEquals("Line 2: AWBSerialNumber", "70158756", line1.AWBSerialNumber);
					AssertEquals("Line 2: AgentCode", "JP054000004", line1.AgentCode);
					AssertEquals("Line 2: DateAWBExecution", ZDateTime.Empty, line1.DateAWBExecution);
					AssertEquals("Line 2: DateOfArrival", new ZDateTime(2010, 08, 04), line1.DateOfArrival);
					AssertEquals("Line 2: DateOfDelivery", new ZDateTime(2010, 08, 05), line1.DateOfDelivery);
					AssertEquals("Line 2: Origin", "MGA", line1.Origin);
					AssertEquals("Line 2: Destination", "NRT", line1.Destination);
					AssertEquals("Line 2: Weight", 140.0M, line1.Weight);
					AssertEquals("Line 2: WeightUnit", "KG", line1.WeightUnit);
					AssertEquals("Line 2: Currency", "USD", line1.CurrencyCode);
					AssertEquals("Line 2: WeightCharges", 415.60M, line1.WeightCharges);
					AssertEquals("Line 2: ChargesDueAgentCC", 5000.00M, line1.ChargesDueAgentCC);
					AssertEquals("Line 2: ChargesDueCarrierCC", 2300.00M, line1.ChargesDueCarrierCC);
					AssertEquals("Line 2: FeeCharged", true, line1.FeeCharged);
					AssertEquals("Line 2: FeeAmount", 30.00M, line1.FeeAmount);
					AssertEquals("Line 2: HandlingCharges", 0.77M, line1.HandlingCharges);
					AssertEquals("Line 2: StorageCharges", 4400000000.44M, line1.StorageCharges);
					AssertEquals("Line 2: OtherCharge1Amount", 1200000000.21M, line1.OtherCharge1Amount);
					AssertEquals("Line 2: OtherCharge2Amount", 6700000000.67M, line1.OtherCharge2Amount);
					AssertEquals("Line 2: MiscellaneousChargesAmount", 1400000000.41M, line1.MiscellaneousChargesAmount);

					var line2 = header.ImportLines[2];
					AssertEquals("Line 3: RecordType", "IBR", line2.RecordType);
					AssertEquals("Line 3: AirlinePrefix", "006", line2.AirlinePrefix);
					AssertEquals("Line 3: AWBSerialNumber", "70158756", line2.AWBSerialNumber);
					AssertEquals("Line 3: AgentCode", "JP054000004", line2.AgentCode);
					AssertEquals("Line 3: DateAWBExecution", ZDateTime.Empty, line2.DateAWBExecution);
					AssertEquals("Line 3: DateOfArrival", new ZDateTime(2010, 08, 04), line2.DateOfArrival);
					AssertEquals("Line 3: DateOfDelivery", new ZDateTime(2010, 08, 05), line2.DateOfDelivery);
					AssertEquals("Line 3: Origin", "MGA", line2.Origin);
					AssertEquals("Line 3: Destination", "NRT", line2.Destination);
					AssertEquals("Line 3: Weight", 140.0M, line2.Weight);
					AssertEquals("Line 3: WeightUnit", "KG", line2.WeightUnit);
					AssertEquals("Line 3: Currency", "USD", line2.CurrencyCode);
					AssertEquals("Line 3: WeightCharges", 415.60M, line2.WeightCharges);
					AssertEquals("Line 3: ChargesDueAgentCC", 5000.00M, line2.ChargesDueAgentCC);
					AssertEquals("Line 3: ChargesDueCarrierCC", 2300.00M, line2.ChargesDueCarrierCC);
					AssertEquals("Line 3: FeeCharged", true, line2.FeeCharged);
					AssertEquals("Line 3: FeeAmount", 30.00M, line2.FeeAmount);
					AssertEquals("Line 3: HandlingCharges", 0.77M, line2.HandlingCharges);
					AssertEquals("Line 3: StorageCharges", 4400000000.44M, line2.StorageCharges);
					AssertEquals("Line 3: OtherCharge1Amount", 1200000000.21M, line2.OtherCharge1Amount);
					AssertEquals("Line 3: OtherCharge2Amount", 6700000000.67M, line2.OtherCharge2Amount);
					AssertEquals("Line 3: MiscellaneousChargesAmount", 1400000000.41M, line2.MiscellaneousChargesAmount);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestConverter_MissingHeader_Import()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("IBI        00533186 NNJP054000004006A00670158700        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBO        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBR        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					var header = new CASSCostHeader();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);

					AssertHeader(header, "CIMP", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty);
					AssertEquals(3, header.Lines.Count);

					var line = header.ImportLines[0];
					AssertEquals("Line 1: RecordType", "IBI", line.RecordType);
					AssertEquals("Line 1: AirlinePrefix", "006", line.AirlinePrefix);
					AssertEquals("Line 1: AWBSerialNumber", "70158700", line.AWBSerialNumber);
					AssertEquals("Line 1: AgentCode", "JP054000004", line.AgentCode);
					AssertEquals("Line 1: DateAWBExecution", ZDateTime.Empty, line.DateAWBExecution);
					AssertEquals("Line 1: DateOfArrival", new ZDateTime(2010, 08, 04), line.DateOfArrival);
					AssertEquals("Line 1: DateOfDelivery", new ZDateTime(2010, 08, 05), line.DateOfDelivery);
					AssertEquals("Line 1: Origin", "MGA", line.Origin);
					AssertEquals("Line 1: Destination", "NRT", line.Destination);
					AssertEquals("Line 1: Weight", 140.0M, line.Weight);
					AssertEquals("Line 1: WeightUnit", "KG", line.WeightUnit);
					AssertEquals("Line 1: Currency", "USD", line.CurrencyCode);
					AssertEquals("Line 1: WeightCharges", 415.60M, line.WeightCharges);
					AssertEquals("Line 1: ChargesDueAgentCC", 5000.00M, line.ChargesDueAgentCC);
					AssertEquals("Line 1: ChargesDueCarrierCC", 2300.00M, line.ChargesDueCarrierCC);
					AssertEquals("Line 1: FeeCharged", true, line.FeeCharged);
					AssertEquals("Line 1: FeeAmount", 30.00M, line.FeeAmount);
					AssertEquals("Line 1: HandlingCharges", 0.77M, line.HandlingCharges);
					AssertEquals("Line 1: StorageCharges", 4400000000.44M, line.StorageCharges);
					AssertEquals("Line 1: OtherCharge1Amount", 1200000000.21M, line.OtherCharge1Amount);
					AssertEquals("Line 1: OtherCharge2Amount", 6700000000.67M, line.OtherCharge2Amount);
					AssertEquals("Line 1: MiscellaneousChargesAmount", 1400000000.41M, line.MiscellaneousChargesAmount);

					var line1 = header.ImportLines[1];
					AssertEquals("Line 2: RecordType", "IBO", line1.RecordType);
					AssertEquals("Line 2: AirlinePrefix", "006", line1.AirlinePrefix);
					AssertEquals("Line 2: AWBSerialNumber", "70158756", line1.AWBSerialNumber);
					AssertEquals("Line 2: AgentCode", "JP054000004", line1.AgentCode);
					AssertEquals("Line 2: DateAWBExecution", ZDateTime.Empty, line1.DateAWBExecution);
					AssertEquals("Line 2: DateOfArrival", new ZDateTime(2010, 08, 04), line1.DateOfArrival);
					AssertEquals("Line 2: DateOfDelivery", new ZDateTime(2010, 08, 05), line1.DateOfDelivery);
					AssertEquals("Line 2: Origin", "MGA", line1.Origin);
					AssertEquals("Line 2: Destination", "NRT", line1.Destination);
					AssertEquals("Line 2: Weight", 140.0M, line1.Weight);
					AssertEquals("Line 2: WeightUnit", "KG", line1.WeightUnit);
					AssertEquals("Line 2: Currency", "USD", line1.CurrencyCode);
					AssertEquals("Line 2: WeightCharges", 415.60M, line1.WeightCharges);
					AssertEquals("Line 2: ChargesDueAgentCC", 5000.00M, line1.ChargesDueAgentCC);
					AssertEquals("Line 2: ChargesDueCarrierCC", 2300.00M, line1.ChargesDueCarrierCC);
					AssertEquals("Line 2: FeeCharged", true, line1.FeeCharged);
					AssertEquals("Line 2: FeeAmount", 30.00M, line1.FeeAmount);
					AssertEquals("Line 2: HandlingCharges", 0.77M, line1.HandlingCharges);
					AssertEquals("Line 2: StorageCharges", 4400000000.44M, line1.StorageCharges);
					AssertEquals("Line 2: OtherCharge1Amount", 1200000000.21M, line1.OtherCharge1Amount);
					AssertEquals("Line 2: OtherCharge2Amount", 6700000000.67M, line1.OtherCharge2Amount);
					AssertEquals("Line 2: MiscellaneousChargesAmount", 1400000000.41M, line1.MiscellaneousChargesAmount);

					Assert(notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
					notificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "A header record with AA2 or AA3 Record ID is expected before the import records with IBI, IBR or IBO Record ID.");
				}
			}
		}

		[TestDate(2015, 11, 05)]
		public void TestConverter_MissingHeader_Export()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("DCO Y17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618200000000002000000000300000000000000000000000400 K0002150MEX");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					CASSCostHeader header = new CASSCostHeader();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, Factory);

					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);

					AssertHeader(header, "CEXP", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty);
					AssertEquals(2, header.Lines.Count);

					var line = header.ExportLines[0];
					AssertEquals("line 1: RecordType", "AWM", line.RecordType);
					AssertEquals("line 1: AirlinePrefix", "160", line.AirlinePrefix);
					AssertEquals("line 1: AWBSerialNumber", "62144154", line.AWBSerialNumber);
					AssertEquals("line 1: AgentCode", "23470068510", line.AgentCode);
					AssertEquals("line 1: DateAWBExecution", new ZDateTime(2008, 08, 14), line.DateAWBExecution);
					AssertEquals("line 1: DateOfArrival", ZDateTime.Empty, line.DateOfArrival);
					AssertEquals("line 1: DateOfDelivery", ZDateTime.Empty, line.DateOfDelivery);
					AssertEquals("line 1: Origin", "DUS", line.Origin);
					AssertEquals("line 1: Destination", "HKG", line.Destination);
					AssertEquals("line 1: Weight", 1190.00M, line.Weight);
					AssertEquals("line 1: WeightUnit", "KG", line.WeightUnit);
					AssertEquals("line 1: Currency", "EUR", line.CurrencyCode);
					AssertEquals("line 1: WeightChargePP", 3462.90M, line.WeightChargePP);
					AssertEquals("line 1: ValuationChargePP", 30.00M, line.ValuationChargePP);
					AssertEquals("line 1: ChargesDueCarrierPP", 1808.80M, line.ChargesDueCarrierPP);
					AssertEquals("line 1: ChargesDueAgentCC", 20.00M, line.ChargesDueAgentCC);
					AssertEquals("line 1: Commission", 10.00M, line.Commission);
					AssertEquals("line 1: Discount", 1320.90M, line.Discount);
					AssertEquals("line 1: VATDueAirline", 100002.01M, line.VATDueAirline);
					AssertEquals("line 1: VATDueAgent", 0M, line.VATDueAgent);

					var line2 = header.ExportLines[1];
					AssertEquals("line 2: RecordType", "DCO", line2.RecordType);
					AssertEquals("line 2: AirlinePrefix", "172", line2.AirlinePrefix);
					AssertEquals("line 2: AWBSerialNumber", "67828073", line2.AWBSerialNumber);
					AssertEquals("line 2: AgentCode", "23470068510", line2.AgentCode);
					AssertEquals("line 2: DateAWBExecution", new ZDateTime(2008, 06, 07), line2.DateAWBExecution);
					AssertEquals("line 2: DateOfArrival", ZDateTime.Empty, line2.DateOfArrival);
					AssertEquals("line 2: DateOfDelivery", ZDateTime.Empty, line2.DateOfDelivery);
					AssertEquals("line 2: Origin", "LEJ", line2.Origin);
					AssertEquals("line 2: Destination", "MEX", line2.Destination);
					AssertEquals("line 2: Weight", 215.0M, line2.Weight);
					AssertEquals("line 2: WeightUnit", "KG", line2.WeightUnit);
					AssertEquals("line 2: Currency", "EUR", line2.CurrencyCode);
					AssertEquals("line 2: WeightChargePP", 473.00M, line2.WeightChargePP);
					AssertEquals("line 2: ValuationChargePP", 0M, line2.ValuationChargePP);
					AssertEquals("line 2: ChargesDueCarrierPP", 216.18M, line2.ChargesDueCarrierPP);
					AssertEquals("line 2: ChargesDueAgentCC", 2.00M, line2.ChargesDueAgentCC);
					AssertEquals("line 2: Commission", 3.00M, line2.Commission);
					AssertEquals("line 2: Discount", 4.00M, line2.Discount);
					AssertEquals("line 2: VATDueAirline", 2000000000.02M, line2.VATDueAirline);
					AssertEquals("line 2: VATDueAgent", 0M, line2.VATDueAgent);

					Assert(notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestConverter_MultipleHeader()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AA2JP054000004  10080110081510082301JPY");
				writer.WriteLine("AA3JP054000004  10080110081510082301JPY");
				writer.WriteLine("IBI        00533186 NNJP054000004006A00670158700        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBO        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBR        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					var header = new CASSCostHeader();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);

					AssertHeader(header, "CIMP", new ZDateTime(2010, 08, 01, 0, 0, 0), new ZDateTime(2010, 08, 15, 0, 0, 0), new ZDateTime(2010, 08, 23, 0, 0, 0), "JPY", "AA2");
					AssertEquals(3, header.Lines.Count);

					var line = header.ImportLines[0];
					AssertEquals("Line 1: RecordType", "IBI", line.RecordType);
					AssertEquals("Line 1: AirlinePrefix", "006", line.AirlinePrefix);
					AssertEquals("Line 1: AWBSerialNumber", "70158700", line.AWBSerialNumber);
					AssertEquals("Line 1: AgentCode", "JP054000004", line.AgentCode);
					AssertEquals("Line 1: DateAWBExecution", ZDateTime.Empty, line.DateAWBExecution);
					AssertEquals("Line 1: DateOfArrival", new ZDateTime(2010, 08, 04), line.DateOfArrival);
					AssertEquals("Line 1: DateOfDelivery", new ZDateTime(2010, 08, 05), line.DateOfDelivery);
					AssertEquals("Line 1: Origin", "MGA", line.Origin);
					AssertEquals("Line 1: Destination", "NRT", line.Destination);
					AssertEquals("Line 1: Weight", 140.0M, line.Weight);
					AssertEquals("Line 1: WeightUnit", "KG", line.WeightUnit);
					AssertEquals("Line 1: Currency", "USD", line.CurrencyCode);
					AssertEquals("Line 1: WeightCharges", 415.60M, line.WeightCharges);
					AssertEquals("Line 1: ChargesDueAgentCC", 5000.00M, line.ChargesDueAgentCC);
					AssertEquals("Line 1: ChargesDueCarrierCC", 2300.00M, line.ChargesDueCarrierCC);
					AssertEquals("Line 1: FeeCharged", true, line.FeeCharged);
					AssertEquals("Line 1: FeeAmount", 30.00M, line.FeeAmount);
					AssertEquals("Line 1: HandlingCharges", 0.77M, line.HandlingCharges);
					AssertEquals("Line 1: StorageCharges", 4400000000.44M, line.StorageCharges);
					AssertEquals("Line 1: OtherCharge1Amount", 1200000000.21M, line.OtherCharge1Amount);
					AssertEquals("Line 1: OtherCharge2Amount", 6700000000.67M, line.OtherCharge2Amount);
					AssertEquals("Line 1: MiscellaneousChargesAmount", 1400000000.41M, line.MiscellaneousChargesAmount);

					var line1 = header.ImportLines[1];
					AssertEquals("Line 2: RecordType", "IBO", line1.RecordType);
					AssertEquals("Line 2: AirlinePrefix", "006", line1.AirlinePrefix);
					AssertEquals("Line 2: AWBSerialNumber", "70158756", line1.AWBSerialNumber);
					AssertEquals("Line 2: AgentCode", "JP054000004", line1.AgentCode);
					AssertEquals("Line 2: DateAWBExecution", ZDateTime.Empty, line1.DateAWBExecution);
					AssertEquals("Line 2: DateOfArrival", new ZDateTime(2010, 08, 04), line1.DateOfArrival);
					AssertEquals("Line 2: DateOfDelivery", new ZDateTime(2010, 08, 05), line1.DateOfDelivery);
					AssertEquals("Line 2: Origin", "MGA", line1.Origin);
					AssertEquals("Line 2: Destination", "NRT", line1.Destination);
					AssertEquals("Line 2: Weight", 140.0M, line1.Weight);
					AssertEquals("Line 2: WeightUnit", "KG", line1.WeightUnit);
					AssertEquals("Line 2: Currency", "USD", line1.CurrencyCode);
					AssertEquals("Line 2: WeightCharges", 415.60M, line1.WeightCharges);
					AssertEquals("Line 2: ChargesDueAgentCC", 5000.00M, line1.ChargesDueAgentCC);
					AssertEquals("Line 2: ChargesDueCarrierCC", 2300.00M, line1.ChargesDueCarrierCC);
					AssertEquals("Line 2: FeeCharged", true, line1.FeeCharged);
					AssertEquals("Line 2: FeeAmount", 30.00M, line1.FeeAmount);
					AssertEquals("Line 2: HandlingCharges", 0.77M, line1.HandlingCharges);
					AssertEquals("Line 2: StorageCharges", 4400000000.44M, line1.StorageCharges);
					AssertEquals("Line 2: OtherCharge1Amount", 1200000000.21M, line1.OtherCharge1Amount);
					AssertEquals("Line 2: OtherCharge2Amount", 6700000000.67M, line1.OtherCharge2Amount);
					AssertEquals("Line 2: MiscellaneousChargesAmount", 1400000000.41M, line1.MiscellaneousChargesAmount);

					var line3 = header.ImportLines[2];
					AssertEquals("Line 3: RecordType", "IBR", line3.RecordType);
					AssertEquals("Line 3: AirlinePrefix", "006", line3.AirlinePrefix);
					AssertEquals("Line 3: AWBSerialNumber", "70158756", line3.AWBSerialNumber);
					AssertEquals("Line 3: AgentCode", "JP054000004", line3.AgentCode);
					AssertEquals("Line 3: DateAWBExecution", ZDateTime.Empty, line3.DateAWBExecution);
					AssertEquals("Line 3: DateOfArrival", new ZDateTime(2010, 08, 04), line3.DateOfArrival);
					AssertEquals("Line 3: DateOfDelivery", new ZDateTime(2010, 08, 05), line3.DateOfDelivery);
					AssertEquals("Line 3: Origin", "MGA", line3.Origin);
					AssertEquals("Line 3: Destination", "NRT", line3.Destination);
					AssertEquals("Line 3: Weight", 140.0M, line3.Weight);
					AssertEquals("Line 3: WeightUnit", "KG", line3.WeightUnit);
					AssertEquals("Line 3: Currency", "USD", line3.CurrencyCode);
					AssertEquals("Line 3: WeightCharges", 415.60M, line3.WeightCharges);
					AssertEquals("Line 3: ChargesDueAgentCC", 5000.00M, line3.ChargesDueAgentCC);
					AssertEquals("Line 3: ChargesDueCarrierCC", 2300.00M, line3.ChargesDueCarrierCC);
					AssertEquals("Line 3: FeeCharged", true, line3.FeeCharged);
					AssertEquals("Line 3: FeeAmount", 30.00M, line3.FeeAmount);
					AssertEquals("Line 3: HandlingCharges", 0.77M, line3.HandlingCharges);
					AssertEquals("Line 3: StorageCharges", 4400000000.44M, line3.StorageCharges);
					AssertEquals("Line 3: OtherCharge1Amount", 1200000000.21M, line3.OtherCharge1Amount);
					AssertEquals("Line 3: OtherCharge2Amount", 6700000000.67M, line3.OtherCharge2Amount);
					AssertEquals("Line 3: MiscellaneousChargesAmount", 1400000000.41M, line3.MiscellaneousChargesAmount);

					Assert(!notificationBuffer.HasErrors);
					Assert(notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestConverter_MixedRecords()
		{
			var header = new CASSCostHeader();
			MemoryStream testStream = new MemoryStream();

			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AA2JP054000004  10080110081510082301JPY");
				writer.WriteLine("AWM  16070158700  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("DCO Y17270158756  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618200000000002000000000300000000000000000000000400 K0002150MEX");
				writer.WriteLine("DCR N17270158756  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX");
				writer.WriteLine("IBI        00533186 NNJP054000004006A00670158700        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBO        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBR        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);

					AssertHeader(header, "CIMP", new ZDateTime(2010, 08, 01), new ZDateTime(2010, 08, 15), new ZDateTime(2010, 08, 23), "JPY", "AA2");
					AssertEquals(3, header.Lines.Count);

					Assert(notificationBuffer.HasErrors);
				}
			}
		}

		public void TestConverter_Export_CurrencyISOSubUnitRatio()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001TWD 2347006");
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KTWD0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("DCO Y17267828073  LEJ23470068510808640TWD00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618200000000002000000000300000000000000000000000400 K0002150MEX");
				writer.WriteLine("DCR N17267828073  LEJ23470068510808640TWD00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					var header = new CASSCostHeader();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);

					AssertHeader(header, "CEXP", new ZDateTime(2008, 08, 01, 0, 0, 0), new ZDateTime(2008, 08, 31, 0, 0, 0), new ZDateTime(2008, 09, 10, 0, 0, 0), ZString.Empty, "AAA");
					AssertEquals(3, header.Lines.Count);

					var line = header.ExportLines[0];
					AssertEquals("line 1: RecordType", "AWM", line.RecordType);
					AssertEquals("line 1: AirlinePrefix", "160", line.AirlinePrefix);
					AssertEquals("line 1: AWBSerialNumber", "62144154", line.AWBSerialNumber);
					AssertEquals("line 1: AgentCode", "23470068510", line.AgentCode);
					AssertEquals("line 1: DateAWBExecution", new ZDateTime(2008, 08, 14), line.DateAWBExecution);
					AssertEquals("line 1: DateOfArrival", ZDateTime.Empty, line.DateOfArrival);
					AssertEquals("line 1: DateOfDelivery", ZDateTime.Empty, line.DateOfDelivery);
					AssertEquals("line 1: Origin", "DUS", line.Origin);
					AssertEquals("line 1: Destination", "HKG", line.Destination);
					AssertEquals("line 1: Weight", 1190.00M, line.Weight);
					AssertEquals("line 1: WeightUnit", "KG", line.WeightUnit);
					AssertEquals("line 1: Currency", "TWD", line.CurrencyCode);
					AssertEquals("line 1: WeightChargePP", 3462.90M, line.WeightChargePP);
					AssertEquals("line 1: ValuationChargePP", 30.00M, line.ValuationChargePP);
					AssertEquals("line 1: ChargesDueCarrierPP", 1808.80M, line.ChargesDueCarrierPP);
					AssertEquals("line 1: ChargesDueAgentCC", 20.00M, line.ChargesDueAgentCC);
					AssertEquals("line 1: Commission", 10.00M, line.Commission);
					AssertEquals("line 1: Discount", 1320.90M, line.Discount);
					AssertEquals("line 1: VATDueAirline", 100002.01M, line.VATDueAirline);
					AssertEquals("line 1: VATDueAgent", 0M, line.VATDueAgent);

					var line1 = header.ExportLines[1];
					AssertEquals("line 2: RecordType", "DCO", line1.RecordType);
					AssertEquals("line 2: AirlinePrefix", "172", line1.AirlinePrefix);
					AssertEquals("line 2: AWBSerialNumber", "67828073", line1.AWBSerialNumber);
					AssertEquals("line 2: AgentCode", "23470068510", line1.AgentCode);
					AssertEquals("line 2: DateAWBExecution", new ZDateTime(2008, 06, 07), line1.DateAWBExecution);
					AssertEquals("line 2: DateOfArrival", ZDateTime.Empty, line1.DateOfArrival);
					AssertEquals("line 2: DateOfDelivery", ZDateTime.Empty, line1.DateOfDelivery);
					AssertEquals("line 2: Origin", "LEJ", line1.Origin);
					AssertEquals("line 2: Destination", "MEX", line1.Destination);
					AssertEquals("line 2: Weight", 215.0M, line1.Weight);
					AssertEquals("line 2: WeightUnit", "KG", line1.WeightUnit);
					AssertEquals("line 2: Currency", "TWD", line1.CurrencyCode);
					AssertEquals("line 2: WeightChargePP", 473.00M, line1.WeightChargePP);
					AssertEquals("line 2: ValuationChargePP", 0M, line1.ValuationChargePP);
					AssertEquals("line 2: ChargesDueCarrierPP", 216.18M, line1.ChargesDueCarrierPP);
					AssertEquals("line 2: ChargesDueAgentCC", 2.00M, line1.ChargesDueAgentCC);
					AssertEquals("line 2: Commission", 3.00M, line1.Commission);
					AssertEquals("line 2: Discount", 4.00M, line1.Discount);
					AssertEquals("line 2: VATDueAirline", 2000000000.02M, line1.VATDueAirline);
					AssertEquals("line 2: VATDueAgent", 0M, line1.VATDueAgent);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestConverter_Import_CurrencyISOSubUnitRatio()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AA2JP054000004  10080110081510082301TWD");
				writer.WriteLine("IBI        00533186 NNJP054000004006A00670158700        MGANRTDL06191008041008050001400KTWD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBO        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KTWD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBR        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KTWD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					var header = new CASSCostHeader();
					CASSHOTFileConverter converter = new CASSHOTFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(header, new CASSHOTFileFormat(), reader);

					AssertHeader(header, "CIMP", new ZDateTime(2010, 08, 01, 0, 0, 0), new ZDateTime(2010, 08, 15, 0, 0, 0), new ZDateTime(2010, 08, 23, 0, 0, 0), "TWD", "AA2");
					AssertEquals(3, header.Lines.Count);

					var line = header.ImportLines[0];
					AssertEquals("Line 1: RecordType", "IBI", line.RecordType);
					AssertEquals("Line 1: AirlinePrefix", "006", line.AirlinePrefix);
					AssertEquals("Line 1: AWBSerialNumber", "70158700", line.AWBSerialNumber);
					AssertEquals("Line 1: AgentCode", "JP054000004", line.AgentCode);
					AssertEquals("Line 1: DateAWBExecution", ZDateTime.Empty, line.DateAWBExecution);
					AssertEquals("Line 1: DateOfArrival", new ZDateTime(2010, 08, 04), line.DateOfArrival);
					AssertEquals("Line 1: DateOfDelivery", new ZDateTime(2010, 08, 05), line.DateOfDelivery);
					AssertEquals("Line 1: Origin", "MGA", line.Origin);
					AssertEquals("Line 1: Destination", "NRT", line.Destination);
					AssertEquals("Line 1: Weight", 140.0M, line.Weight);
					AssertEquals("Line 1: WeightUnit", "KG", line.WeightUnit);
					AssertEquals("Line 1: Currency", "TWD", line.CurrencyCode);
					AssertEquals("Line 1: WeightCharges", 415.60M, line.WeightCharges);
					AssertEquals("Line 1: ChargesDueAgentCC", 5000.00M, line.ChargesDueAgentCC);
					AssertEquals("Line 1: ChargesDueCarrierCC", 2300.00M, line.ChargesDueCarrierCC);
					AssertEquals("Line 1: FeeCharged", true, line.FeeCharged);
					AssertEquals("Line 1: FeeAmount", 30.00M, line.FeeAmount);
					AssertEquals("Line 1: HandlingCharges", 0.77M, line.HandlingCharges);
					AssertEquals("Line 1: StorageCharges", 4400000000.44M, line.StorageCharges);
					AssertEquals("Line 1: OtherCharge1Amount", 1200000000.21M, line.OtherCharge1Amount);
					AssertEquals("Line 1: OtherCharge2Amount", 6700000000.67M, line.OtherCharge2Amount);
					AssertEquals("Line 1: MiscellaneousChargesAmount", 1400000000.41M, line.MiscellaneousChargesAmount);

					var line1 = header.ImportLines[1];
					AssertEquals("Line 2: RecordType", "IBO", line1.RecordType);
					AssertEquals("Line 2: AirlinePrefix", "006", line1.AirlinePrefix);
					AssertEquals("Line 2: AWBSerialNumber", "70158756", line1.AWBSerialNumber);
					AssertEquals("Line 2: AgentCode", "JP054000004", line1.AgentCode);
					AssertEquals("Line 2: DateAWBExecution", ZDateTime.Empty, line1.DateAWBExecution);
					AssertEquals("Line 2: DateOfArrival", new ZDateTime(2010, 08, 04), line1.DateOfArrival);
					AssertEquals("Line 2: DateOfDelivery", new ZDateTime(2010, 08, 05), line1.DateOfDelivery);
					AssertEquals("Line 2: Origin", "MGA", line1.Origin);
					AssertEquals("Line 2: Destination", "NRT", line1.Destination);
					AssertEquals("Line 2: Weight", 140.0M, line1.Weight);
					AssertEquals("Line 2: WeightUnit", "KG", line1.WeightUnit);
					AssertEquals("Line 2: Currency", "TWD", line1.CurrencyCode);
					AssertEquals("Line 2: WeightCharges", 415.60M, line1.WeightCharges);
					AssertEquals("Line 2: ChargesDueAgentCC", 5000.00M, line1.ChargesDueAgentCC);
					AssertEquals("Line 2: ChargesDueCarrierCC", 2300.00M, line1.ChargesDueCarrierCC);
					AssertEquals("Line 2: FeeCharged", true, line1.FeeCharged);
					AssertEquals("Line 2: FeeAmount", 30.00M, line1.FeeAmount);
					AssertEquals("Line 2: HandlingCharges", 0.77M, line1.HandlingCharges);
					AssertEquals("Line 2: StorageCharges", 4400000000.44M, line1.StorageCharges);
					AssertEquals("Line 2: OtherCharge1Amount", 1200000000.21M, line1.OtherCharge1Amount);
					AssertEquals("Line 2: OtherCharge2Amount", 6700000000.67M, line1.OtherCharge2Amount);
					AssertEquals("Line 2: MiscellaneousChargesAmount", 1400000000.41M, line1.MiscellaneousChargesAmount);

					var line2 = header.ImportLines[2];
					AssertEquals("Line 3: RecordType", "IBR", line2.RecordType);
					AssertEquals("Line 3: AirlinePrefix", "006", line2.AirlinePrefix);
					AssertEquals("Line 3: AWBSerialNumber", "70158756", line2.AWBSerialNumber);
					AssertEquals("Line 3: AgentCode", "JP054000004", line2.AgentCode);
					AssertEquals("Line 3: DateAWBExecution", ZDateTime.Empty, line2.DateAWBExecution);
					AssertEquals("Line 3: DateOfArrival", new ZDateTime(2010, 08, 04), line2.DateOfArrival);
					AssertEquals("Line 3: DateOfDelivery", new ZDateTime(2010, 08, 05), line2.DateOfDelivery);
					AssertEquals("Line 3: Origin", "MGA", line2.Origin);
					AssertEquals("Line 3: Destination", "NRT", line2.Destination);
					AssertEquals("Line 3: Weight", 140.0M, line2.Weight);
					AssertEquals("Line 3: WeightUnit", "KG", line2.WeightUnit);
					AssertEquals("Line 3: Currency", "TWD", line2.CurrencyCode);
					AssertEquals("Line 3: WeightCharges", 415.60M, line2.WeightCharges);
					AssertEquals("Line 3: ChargesDueAgentCC", 5000.00M, line2.ChargesDueAgentCC);
					AssertEquals("Line 3: ChargesDueCarrierCC", 2300.00M, line2.ChargesDueCarrierCC);
					AssertEquals("Line 3: FeeCharged", true, line2.FeeCharged);
					AssertEquals("Line 3: FeeAmount", 30.00M, line2.FeeAmount);
					AssertEquals("Line 3: HandlingCharges", 0.77M, line2.HandlingCharges);
					AssertEquals("Line 3: StorageCharges", 4400000000.44M, line2.StorageCharges);
					AssertEquals("Line 3: OtherCharge1Amount", 1200000000.21M, line2.OtherCharge1Amount);
					AssertEquals("Line 3: OtherCharge2Amount", 6700000000.67M, line2.OtherCharge2Amount);
					AssertEquals("Line 3: MiscellaneousChargesAmount", 1400000000.41M, line2.MiscellaneousChargesAmount);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		void AssertHeader(CASSCostHeader header, ZString expectedCASSType, ZDateTime expectedDatePeriodStart, ZDateTime expectedDatePeriodEnd, ZDateTime expectedDateOfBilling,
							ZString expectedBillingCurrency, ZString expectedRecordType)
		{
			AssertEquals("DatePeriodStart", expectedDatePeriodStart, header.DatePeriodStart);
			AssertEquals("DatePeriodStart", expectedDatePeriodEnd, header.DatePeriodEnd);
			AssertEquals("DateOfBilling", expectedDateOfBilling, header.DateOfBilling);
			AssertEquals("BillingCurrency", expectedBillingCurrency, header.BillingCurrency);
			AssertEquals("RecordType", expectedRecordType, header.RecordType);
		}
	}
}
