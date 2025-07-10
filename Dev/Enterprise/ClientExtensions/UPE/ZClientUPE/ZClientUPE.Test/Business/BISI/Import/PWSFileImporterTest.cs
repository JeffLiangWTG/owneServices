using System;
using System.Collections;
using System.IO;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class PWSFileImporterTest : TestCaseWithClientSpecificDocuments
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportingWithChargesThatDoNotHaveExistingShipments()
		{
			bool result = Importer.Import(UPETestHelper.TestFiles.BISI.Import.Folder + "CODFileForNonExistingShipments.txt", Notifications);
			AssertEquals("Should be imported successfully", true, result);
			ClientPWSHeader[] headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert(headers.Length == 1);
			ClientPWSHeader header = headers[0];
			Assert(header.U1_WayBillNumber == "1Z3F1A740459640777");
			Assert(header.U1_WayBillShortNumber == "3F1A74L4FNN");
			Assert(header.U1_InvoiceNumber == "200001467417");
			Assert(header.U1_BillToAccount == "A44266");
			Assert(header.U1_BillableWeight == 2.1m);
			Assert(header.Charges.Count == 3);
			header.Charges.Sort(ClientPWSChargeSchema.U2_ChargeDescription.Name, System.ComponentModel.ListSortDirection.Ascending);
			ClientPWSCharge charge = header.Charges[0];
			Assert(charge.U2_ChargeDescription == "FREIGHT");
			Assert(charge.U2_TaxableAmount == 0m);
			Assert(charge.U2_NonTaxableAmount == 178.00m);
			Assert(charge.U2_Discount == 105.02m);
			Assert(charge.U2_NetAmount == 72.98m);
			charge = header.Charges[1];
			Assert(charge.U2_ChargeDescription == "FUEL SURCHARGE");
			Assert(charge.U2_TaxableAmount == 0m);
			Assert(charge.U2_NonTaxableAmount == 19.58m);
			Assert(charge.U2_Discount == 11.55m);
			Assert(charge.U2_NetAmount == 8.03m);
			charge = header.Charges[2];
			Assert(charge.U2_ChargeDescription == "SECURITY FEE");
			Assert(charge.U2_TaxableAmount == 0m);
			Assert(charge.U2_NonTaxableAmount == 9.95m);
			Assert(charge.U2_Discount == 9.95m);
			Assert(charge.U2_NetAmount == 0m);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 12, 4)]
		public void TestImport()
		{
			SetupCusHAWBsForImportTest();
			Callout callout1 = Factory.Load<Callout>(CalloutPK1);
			Callout callout2 = Factory.Load<Callout>(CalloutPK2);
			Callout callout3 = Factory.Load<Callout>(CalloutPK3);
			Callout callout4 = Factory.Load<Callout>(CalloutPK4);
			AssertCalloutEmpty(callout1, "WAA25665589", "WAA25665589", "Pre-condition, Callout details should not be populated");
			AssertCalloutEmpty(callout2, "", "298E68FX8HF", "Pre-condition, Callout details should not be populated");
			AssertCalloutEmpty(callout3, "WZZ25665589", "WZZ25665589", "Pre-condition, Callout details should not be populated");
			AssertCalloutEmpty(callout4, "", "WY5665589", "Pre-condition, Callout details should not be populated");
			bool result = Importer.Import(CODFilePath, Notifications);
			AssertEquals("Should be imported successfully", true, result);
			callout1.CurrentQueue.Reload();
			callout2.CurrentQueue.Reload();
			callout3.CurrentQueue.Reload();
			callout4.CurrentQueue.Reload();
			AssertCallout(callout1, 3, "WAA25665589", "WAA25665589", "AAAAA", "000000621101", 1m, 3m, 34.9m);
			AssertCallout(callout2, 3, "", "298E68FX8HF", "BBBBB", "000000621102", 2m, 7m, 900.2m);
			AssertCallout(callout3, 3, "WZZ25665589", "WZZ25665589", "CCCCC", "000000621103", 3m, 9m, 0m);
			AssertCallout(callout4, 3, "", "WY5665589", "DDDDD", "000000621104", 4m, 12m, 0m);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 12, 4)]
		public void TestImport_BillToAccountFallbackToImporterIfEmpty()
		{
			SetupCusHAWBsForImportTest();
			Callout callout = Factory.Load<Callout>(CalloutPK1);
			callout.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<UPEOrgHeader>().MainAddress.PK;
			callout.Consignee.AccountNumber = "ImporterAccountNum";
			Factory.Save();
			AssertCalloutEmpty(callout, "WAA25665589", "WAA25665589", "Pre-condition, Callout details should not be populated");
			bool result = Importer.Import(CODFileNoBillToAccount, Notifications);
			AssertEquals("Should be imported successfully", true, result);
			callout.CurrentQueue.Reload();
			AssertCallout(callout, 3, "WAA25665589", "WAA25665589", "ImporterAccountNum", "000000621101", 1m, 3m, 34.9m);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2005, 12, 4)]
		public void TestImport_TwiceChargesNotDuplicated()
		{
			SetupCusHAWBsForImportTest();
			Callout callout1 = Factory.Load<Callout>(CalloutPK1);
			Callout callout2 = Factory.Load<Callout>(CalloutPK2);
			Callout callout3 = Factory.Load<Callout>(CalloutPK3);
			Callout callout4 = Factory.Load<Callout>(CalloutPK4);
			AssertCalloutEmpty(callout1, "WAA25665589", "WAA25665589", "Pre-condition, Callout details should not be populated");
			AssertCalloutEmpty(callout2, "", "298E68FX8HF", "Pre-condition, Callout details should not be populated");
			AssertCalloutEmpty(callout3, "WZZ25665589", "WZZ25665589", "Pre-condition, Callout details should not be populated");
			AssertCalloutEmpty(callout4, "", "WY5665589", "Pre-condition, Callout details should not be populated");
			bool result = Importer.Import(CODFilePath, Notifications);
			AssertEquals("Should be imported successfully", true, result);
			callout1.CurrentQueue.Reload();
			callout2.CurrentQueue.Reload();
			callout3.CurrentQueue.Reload();
			callout4.CurrentQueue.Reload();
			AssertCallout(callout1, 3, "WAA25665589", "WAA25665589", "AAAAA", "000000621101", 1m, 3m, 34.9m);
			AssertCallout(callout2, 3, "", "298E68FX8HF", "BBBBB", "000000621102", 2m, 7m, 900.2m);
			AssertCallout(callout3, 3, "WZZ25665589", "WZZ25665589", "CCCCC", "000000621103", 3m, 9m, 0m);
			AssertCallout(callout4, 3, "", "WY5665589", "DDDDD", "000000621104", 4m, 12m, 0m);
			result = Importer.Import(CODFilePath, Notifications);
			AssertEquals("Should be imported successfully", true, result);
			callout1.CurrentQueue.Reload();
			callout2.CurrentQueue.Reload();
			callout3.CurrentQueue.Reload();
			callout4.CurrentQueue.Reload();
			// Should not add more charges to the Callout
			AssertCallout(callout1, 3, "WAA25665589", "WAA25665589", "AAAAA", "000000621101", 1m, 3m, 34.9m);
			AssertCallout(callout2, 3, "", "298E68FX8HF", "BBBBB", "000000621102", 2m, 7m, 900.2m);
			AssertCallout(callout3, 3, "WZZ25665589", "WZZ25665589", "CCCCC", "000000621103", 3m, 9m, 0m);
			AssertCallout(callout4, 3, "", "WY5665589", "DDDDD", "000000621104", 4m, 12m, 0m);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_ReturnsFalseWhenThereIsException()
		{
			var mock = new Mock<PWSFileImporter>();
			mock.Protected()
				.Setup("ImportToFactory", ItExpr.IsAny<PWSTotalDataAreaRecord>())
				.Throws(new Exception("BLAH"));
			var importer = mock.Object;
			bool result = importer.Import(CODFilePath, Notifications);
			AssertEquals("There is an exception. Result should be false.", false, result);
			ErrorReporter.Clear();
			mock.VerifyAll();
		}

		public void TestImport_ReturnsFalseWhenReaderNotValid()
		{
			bool result = Importer.Import("FileDoesNotExistMeh.Meh", Notifications);
			AssertEquals("Should return false, file does not exist", false, result);
		}

		public void TestFactoryIsSavedAndResetIfReachesMaxImportCounter()
		{
			var maxFactoryImportCounterField = Importer.GetType().GetField("MaxFactorySaveCounter", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertEquals("Default Max Import counter", 50, maxFactoryImportCounterField.GetValue(Importer));
			var tempFile = Env.GetTempFileName();
			try
			{
				maxFactoryImportCounterField.SetValue(Importer, 10);
				SetupImportFileAndCusHAWBsForFactoryProviderTest(tempFile);
				var result = Importer.Import(tempFile, Notifications);
				AssertEquals("Should be imported successfully", true, result);
				var callouts = Importer.GetLastImportedCalloutsForTest();
				SortCalloutsByTrackingNumber(callouts);
				AssertEquals("Same batch. Should be using the same factory", callouts[0].Logs.MostRecentLog.SL_PostedTimeUtc, callouts[1].Logs.MostRecentLog.SL_PostedTimeUtc);
				Assert("Different batch. Should be using different factory", callouts[1].Logs.MostRecentLog.SL_PostedTimeUtc != callouts[10].Logs.MostRecentLog.SL_PostedTimeUtc);
				AssertEquals("Should be saved by the importer", false, callouts[0].HasChanges && callouts[10].HasChanges && callouts[20].HasChanges);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
		}

		void AssertCalloutEmpty(Callout callout, ZString expHAWBNumber, ZString expWayBillShort, string message)
		{
			AssertEquals(message, expHAWBNumber, callout.CS_HAWB);
			AssertEquals(message, expWayBillShort, callout.WayBillShort);
			AssertEquals(message, "", callout.BillToAccountNumber);
			AssertEquals(message, "", callout.InvoiceNumber);
			AssertNull(message, callout.JobHeader);
			AssertEquals(message, ZDateTime.Empty, callout.BisiDownloadDate);
			AssertEquals(message, 0m, callout.TotalAmountDue);
			AssertEquals(message, 0m, callout.CS_ChargableWeight);
		}

		void AssertCallout(Callout callout, int expectedChargesNumber, ZString expHAWBNumber, ZString expWayBillShort, ZString expBillToAccount, ZString expInvoiceNumber, ZDecimal expAmount, ZDecimal expTotalAmountDue, ZDecimal expDimensionalWeight)
		{
			callout.Reload();
			AssertEquals(expHAWBNumber, callout.CS_HAWB);
			AssertEquals(expWayBillShort, callout.WayBillShort);
			AssertEquals(expBillToAccount, callout.BillToAccountNumber);
			AssertEquals(expInvoiceNumber, callout.InvoiceNumber);
			AssertEquals("Last charge line should not be included", expectedChargesNumber, callout.JobHeader.Charges.Count);
			AssertEquals(expAmount, callout.JobHeader.Charges[0].NonTaxableAmount);
			AssertEquals(ZDateTime.Now, callout.BisiDownloadDate);
			AssertEquals(expTotalAmountDue, callout.TotalAmountDue);
			AssertEquals(expDimensionalWeight, callout.CS_ChargableWeight);
		}

		UPECusHAWB AddHouseBillByTrackingNumber(ZString trackingNumber)
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			UPECusHAWB houseBill = (UPECusHAWB)mAWB.ChildBills.AddNew();
			houseBill.CS_HAWB = trackingNumber;
			return houseBill;
		}

		UPECusHAWB AddHouseBillByShortTrackingNumber(ZString shortTrackingNumber)
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			UPECusHAWB houseBill = (UPECusHAWB)mAWB.ChildBills.AddNew();
			houseBill.WayBillShort = shortTrackingNumber;
			return houseBill;
		}

		void SetupCusHAWBsForImportTest()
		{
			CalloutPK1 = AddHouseBillByTrackingNumber("WAA25665589").PK;
			CalloutPK2 = AddHouseBillByShortTrackingNumber("298E68FX8HF").PK;
			CalloutPK3 = AddHouseBillByTrackingNumber("WZZ25665589").PK;
			CalloutPK4 = AddHouseBillByShortTrackingNumber("WY5665589").PK;
			Factory.Save();
		}

		void SetupImportFileAndCusHAWBsForFactoryProviderTest(string tempFile)
		{
			using (StreamWriter writer = File.CreateText(tempFile))
			{
				for (int i = 0; i < 30; i++)
				{
					string indexAsString = i.ToString("00");
					string trackingNumber = "S" + indexAsString;
					string invoiceNumber = "INV" + indexAsString;
					AddHouseBillByTrackingNumber(trackingNumber);
					writer.WriteLine(string.Format(CODShipmentRecordTemplate, trackingNumber, invoiceNumber).Trim());
				}
			}

			Factory.Save();
		}

		void SortCalloutsByTrackingNumber(Callout[] callouts)
		{
			Array.Sort(callouts, new CalloutComparer());
		}

		PWSFileImporter Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = new PWSFileImporter();
				}

				return fImporter;
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

		string CODFilePath
		{
			get
			{
				return UPETestHelper.TestFiles.BISI.Import.Folder + "CODFileForImport.txt";
			}
		}

		string CODFileNoBillToAccount
		{
			get
			{
				return UPETestHelper.TestFiles.BISI.Import.Folder + "CODFileNoBillToAccount.txt";
			}
		}

		const string CODShipmentRecordTemplate = @"
020{0,-35}0001B{0,-11}{0,-35}{1,-12}09MAY2002      A59935    8SG0157203CREATIVE PRODUCTS CORP.      02562
020{0,-35}0002B        6/F PEAKSON BLDG. 1505 PRENCETON   ST.,CORNER SHAW                    MANDALUYONG                 1552      PHPH
020{0,-35}0003BILIPPINES                      AIRTROPOLIS EXPRESS (S) PTE LTD    9 AIRLINE RD UNIT #01-20 (WRHS)    & UNIT 04-17 (OFC)
020{0,-35}0004BCARGO AGENT     SINGAPORE                   819827    65 5431377     SGSINGAPORE      10MAY2002
020{0,-35}0005B           WW EXPRESS     X                     11.5                                        KGSA
020{0,-35}0006B           1    NON-DOCUMENTCYBORG KURO CHAN BETACAM                190.54           SGDIM2E005828 1.808000000
020{0,-35}0007B
020{0,-35}0008B
020{0,-35}0009B
020{0,-35}0010B
020{0,-35}0011B
020{0,-35}0012B
020{0,-35}0013B      140.10CASH ONLY           004GST-WAIVED (LOW CIF/FTZ)           005406 7279 9263                      00610052002
020{0,-35}0014B                          00719                                 009UPS02051019                        0107PCS
020{0,-35}0015B
020{0,-35}0016B                                                    FUEL SURCHARGE                                    1.03
020{0,-35}0017B                1.03FREIGHT                                         137.00                            137.00THAILAND'S E
020{0,-35}0018BXPORT                                 2.07                              2.07
020{0,-35}0019B
020{0,-35}0020B
020{0,-35}0021B
020{0,-35}0022B
020{0,-35}0023B
020{0,-35}0024B
020{0,-35}0025B
020{0,-35}0026B
020{0,-35}0027B
020{0,-35}0028B
020{0,-35}0029B
020{0,-35}0030B
020{0,-35}0031B
020{0,-35}0032B                                                                    10MAY2002
020{0,-35}0033D                                                                                                                   SGD         ";
		ZGuid CalloutPK1;
		ZGuid CalloutPK2;
		ZGuid CalloutPK3;
		ZGuid CalloutPK4;
		PWSFileImporter fImporter;
		NotificationBuffer fNotifications;
		#region class CalloutComparer
		class CalloutComparer : IComparer
		{
			#region IComparer Members
			public int Compare(object x, object y)
			{
				return Compare(x as Callout, y as Callout);
			}

			int Compare(Callout x, Callout y)
			{
				return x.CS_HAWB.CompareTo(y.CS_HAWB);
			}
			#endregion
		}
		#endregion
		#endregion
	}
}
