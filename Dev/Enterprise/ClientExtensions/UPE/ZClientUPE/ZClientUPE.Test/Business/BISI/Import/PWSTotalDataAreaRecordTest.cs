using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class PWSTotalDataAreaRecordTest : TestCase
	{
		public void TestNew()
		{
			AssertNull("Should return null if array passed in is null", PWSTotalDataAreaRecord.New(null, Notifications));
			AssertNull("Should return null if array passed in does not have any items", PWSTotalDataAreaRecord.New(Array.Empty<PWSDetailRecord>(), Notifications));
			AssertEquals("Invalid details record count", 33, Record.NumberOfDetailRecords);
		}

		public void TestWayBillShortNumber()
		{
			AssertEquals("W6625665589", Record.WayBillShortNumber);
		}

		public void TestWayBillNumber()
		{
			AssertEquals("W66256655891234567890", Record.WayBillNumber);
		}

		public void TestInvoiceNumber()
		{
			AssertEquals("000000621397", Record.InvoiceNumber);
		}

		public void TestBillToAccount()
		{
			AssertEquals("8SG0157203", Record.BillToAccount);
			RecordContent[0] = new PWSDetailRecord("020W6625665589                        0001BW6625665589W66256655891234567890              00000062139709MAY2002      A59935    0SG0157203CREATIVE PRODUCTS CORP.      02562", Notifications);
			fRecord = null;
			AssertEquals("0SG0157203", Record.BillToAccount);
			RecordContent[0] = new PWSDetailRecord("020W6625665589                        0001BW6625665589W66256655891234567890              00000062139709MAY2002      A59935    00G0157203CREATIVE PRODUCTS CORP.      02562", Notifications);
			fRecord = null;
			AssertEquals("00G0157203", Record.BillToAccount);
			RecordContent[0] = new PWSDetailRecord("020W6625665589                        0001BW6625665589W66256655891234567890              00000062139709MAY2002      A59935    0000157203CREATIVE PRODUCTS CORP.      02562", Notifications);
			fRecord = null;
			AssertEquals("157203", Record.BillToAccount);
		}

		public void TestDimensionalWeight()
		{
			AssertEquals(30.4m, Record.DimensionalWeight);
			RecordContent[4] = new PWSDetailRecord("020W6625665589                        0005B           WW EXPRESS     X                     11.5/12.4                                   KGSA                               ", Notifications);
			fRecord = null;
			AssertEquals(12.4m, Record.DimensionalWeight);
			RecordContent[4] = new PWSDetailRecord("020W6625665589                        0005B           WW EXPRESS     X                     11.5                                        KGSA                               ", Notifications);
			fRecord = null;
			AssertEquals(0m, Record.DimensionalWeight);
			RecordContent[4] = new PWSDetailRecord("020W6625665589                        0005B           WW EXPRESS     X                     11.5/asdf                                   KGSA                               ", Notifications);
			fRecord = null;
			AssertEquals(0m, Record.DimensionalWeight);
		}

		public void TestCharges()
		{
			var sortedList = Record.Charges.ToList();
			sortedList.Sort(new ComparerForTest());
			AssertEquals("Last charge line should be excluded", 3, sortedList.Count);
			AssertChargeDetails(sortedList[0], "FREIGHT", 0m, 137m, 0m, 137m);
			AssertChargeDetails(sortedList[1], "FUEL SURCHARGE", 0m, 1.03m, 0m, 1.03m);
			AssertChargeDetails(sortedList[2], "THAILAND'S EXPORT", 0m, 2.07m, 0m, 2.07m);
		}

		#region Implementation
		void AssertChargeDetails(PWSChargeDetails chargeDetails, ZString expDesc, ZDecimal expTaxAmount, ZDecimal expNonTaxAmount, ZDecimal expDiscount, ZDecimal expNettAmount)
		{
			AssertEquals(expDesc, chargeDetails.ChargeDescription);
			AssertEquals(expTaxAmount, chargeDetails.TaxableAmount);
			AssertEquals(expNonTaxAmount, chargeDetails.NonTaxableAmount);
			AssertEquals(expDiscount, chargeDetails.Discount);
			AssertEquals(expNettAmount, chargeDetails.NettAmount);
		}

		PWSTotalDataAreaRecord Record
		{
			get
			{
				if (fRecord == null)
				{
					fRecord = PWSTotalDataAreaRecord.New(RecordContent, Notifications);
				}

				return fRecord;
			}
		}

		NotificationBuffer Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new NotificationBuffer();
				}

				return fNotifications;
			}
		}

		PWSTotalDataAreaRecord fRecord;
		NotificationBuffer fNotifications;
		PWSDetailRecord[] fRecordContent;
		PWSDetailRecord[] RecordContent
		{
			get
			{
				if (fRecordContent == null)
				{
					fRecordContent = new PWSDetailRecord[] { new PWSDetailRecord("020W6625665589                        0001BW6625665589W66256655891234567890              00000062139709MAY2002      A59935    8SG0157203CREATIVE PRODUCTS CORP.      02562", Notifications), new PWSDetailRecord("020W6625665589                        0002B        6/F PEAKSON BLDG. 1505 PRENCETON   ST.,CORNER SHAW                    MANDALUYONG                 1552      PHPH       ", Notifications), new PWSDetailRecord("020W6625665589                        0003BILIPPINES                      AIRTROPOLIS EXPRESS (S) PTE LTD    9 AIRLINE RD UNIT #01-20 (WRHS)    & UNIT 04-17 (OFC)        ", Notifications), new PWSDetailRecord("020W6625665589                        0004BCARGO AGENT     SINGAPORE                   819827    65 5431377     SGSINGAPORE      10MAY2002                                ", Notifications), new PWSDetailRecord("020W6625665589                        0005B           WW EXPRESS     X                     11.5/30.4                                   KGSA                               ", Notifications), new PWSDetailRecord("020W6625665589                        0006B           1    NON-DOCUMENTCYBORG KURO CHAN BETACAM                190.54           SGDIM2E005828 1.808000000                 ", Notifications), new PWSDetailRecord("020W6625665589                        0007B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0008B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0009B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0010B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0011B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0012B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0013B      140.10CASH ONLY           004GST-WAIVED (LOW CIF/FTZ)           005406 7279 9263                      00610052002        ", Notifications), new PWSDetailRecord("020W6625665589                        0014B                          00719                                 009UPS02051019                        0107PCS                  ", Notifications), new PWSDetailRecord("020W6625665589                        0015B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0016B                                                    FUEL SURCHARGE                                    1.03                     ", Notifications), new PWSDetailRecord("020W6625665589                        0017B                1.03FREIGHT                                         137.00                            137.00THAILAND'S E       ", Notifications), new PWSDetailRecord("020W6625665589                        0018BXPORT                                 2.07                              2.07                                                   ", Notifications), new PWSDetailRecord("020W6625665589                        0019B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0020B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0021B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0022B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0023B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0024B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0025B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0026B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0027B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0028B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0029B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0030B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0031B                                                                                                                               ", Notifications), new PWSDetailRecord("020W6625665589                        0032B                                                                    SOMETHING CHARGE                 2.39            12.       ", Notifications), new PWSDetailRecord("020W6625665589                        0033D45            56.90            34.89                                                                               SGD         ", Notifications) };
				}

				return fRecordContent;
			}
		}

		sealed class ComparerForTest : IComparer<PWSChargeDetails>
		{
			public int Compare(PWSChargeDetails x, PWSChargeDetails y)
			{
				return x.ChargeDescription.CompareTo(y.ChargeDescription);
			}
		}
		#endregion
	}
}
