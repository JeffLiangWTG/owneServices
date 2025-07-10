using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.Testing
{
	public class QuantumFileTest : TestCaseWithFactory
	{
		public void TestGetFile()
		{
			string fileName = TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name);
			QuantumFile testFile = QuantumFile.GetFile(fileName, new NotificationBuffer(null));
			AssertNotNull("File should not be null", testFile);
			AssertNotNull("Segments should not be null", testFile.QuantumSegments);
			AssertEquals("Segment 0 - MasterBill", "08135025185", testFile.QuantumSegments[0].Consol.MasterBill);
			AssertEquals("Segment 1 - MasterBill", "19933462310", testFile.QuantumSegments[1].Consol.MasterBill);
			AssertEquals("Segment 2 - MasterBill", "08133462295", testFile.QuantumSegments[2].Consol.MasterBill);
			AssertEquals("Segment 3 - MasterBill", "08135025185", testFile.QuantumSegments[3].Consol.MasterBill);
			AssertEquals("Segment 4 - MasterBill", "07405320851", testFile.QuantumSegments[4].Consol.MasterBill);
			AssertEquals("Segment 5 - MasterBill", "70305321050", testFile.QuantumSegments[5].Consol.MasterBill);
			AssertEquals("Segment 0 - # of Shipments", 1, testFile.QuantumSegments[0].Shipments.Length);
			AssertEquals("Segment 1 - # of Shipments", 1, testFile.QuantumSegments[1].Shipments.Length);
			AssertEquals("Segment 2 - # of Shipments", 17, testFile.QuantumSegments[2].Shipments.Length);
			AssertEquals("Segment 3 - # of Shipments", 3, testFile.QuantumSegments[3].Shipments.Length);
			AssertEquals("Segment 4 - # of Shipments", 7, testFile.QuantumSegments[4].Shipments.Length);
			AssertEquals("Segment 5 - # of Shipments", 1, testFile.QuantumSegments[5].Shipments.Length);
		}

		public void TestFromData()
		{
			TNTTestUtils testUtils = new TNTTestUtils();
			string[] fileLines = testUtils.GetFileLines(TNTTestUtils.SampleX2Name);
			QuantumFile file = QuantumFile.FromData(fileLines, TNTTestUtils.SampleX2Name, new NotificationBuffer(null));
			AssertNotNull("File should not be null", file);
			AssertNotNull("Segments should not be null", file.QuantumSegments);
			AssertEquals("Segment 0 - MasterBill", "08135025185", file.QuantumSegments[0].Consol.MasterBill);
			AssertEquals("Segment 1 - MasterBill", "19933462310", file.QuantumSegments[1].Consol.MasterBill);
			AssertEquals("Segment 2 - MasterBill", "08133462295", file.QuantumSegments[2].Consol.MasterBill);
			AssertEquals("Segment 3 - MasterBill", "08135025185", file.QuantumSegments[3].Consol.MasterBill);
			AssertEquals("Segment 4 - MasterBill", "07405320851", file.QuantumSegments[4].Consol.MasterBill);
			AssertEquals("Segment 5 - MasterBill", "70305321050", file.QuantumSegments[5].Consol.MasterBill);
			AssertEquals("Segment 0 - # of Shipments", 1, file.QuantumSegments[0].Shipments.Length);
			AssertEquals("Segment 1 - # of Shipments", 1, file.QuantumSegments[1].Shipments.Length);
			AssertEquals("Segment 2 - # of Shipments", 17, file.QuantumSegments[2].Shipments.Length);
			AssertEquals("Segment 3 - # of Shipments", 3, file.QuantumSegments[3].Shipments.Length);
			AssertEquals("Segment 4 - # of Shipments", 7, file.QuantumSegments[4].Shipments.Length);
			AssertEquals("Segment 5 - # of Shipments", 1, file.QuantumSegments[5].Shipments.Length);
		}

		public void TestFromDataMergesDuplicatedMawbSegments()
		{
			TNTTestUtils testUtils = new TNTTestUtils();
			string[] fileLines = testUtils.GetFileLines(TNTTestUtils.SampleX2Name);
			QuantumFile file = QuantumFile.FromData(fileLines, TNTTestUtils.SampleX2Name, new NotificationBuffer(null));
			AssertNotNull("File should not be null", file);
			AssertNotNull("Segments should not be null", file.QuantumSegments);
			AssertEquals("Number of segments", 6, file.QuantumSegments.Length);
			// Not Merged Segment - Same MB but different flight/ports/etd/mode - Array item 0
			AssertEquals("Merged Segment 0 - MasterBill", "08135025185", file.QuantumSegments[0].Consol.MasterBill);
			AssertEquals("Merged Segment 0 - FlightNumber", "JL5772", file.QuantumSegments[0].Consol.FlightNumber);
			AssertEquals("Merged Segment 0 - PortOfLoading", "SYD", file.QuantumSegments[0].Consol.PortOfLoading);
			AssertEquals("Merged Segment 0 - PortOfDischarge", "NRT", file.QuantumSegments[0].Consol.PortOfDischarge);
			AssertEquals("Merged Segment 0 - DepartureDate", new ZDateTime(2004, 8, 14), file.QuantumSegments[0].Consol.DepartureDate);
			AssertEquals("Merged Segment 0 - # of Shipments", 1, file.QuantumSegments[0].Shipments.Length);
			AssertEquals("Merged Segment 0 - # of ShipmentNotes", 1, file.QuantumSegments[0].ShipmentsNotes.Length);
			// First Merged Segment - Array item 3
			AssertEquals("Merged Segment 3 - MasterBill", "08135025185", file.QuantumSegments[3].Consol.MasterBill);
			AssertEquals("Merged Segment 3 - FlightNumber", "PX004", file.QuantumSegments[3].Consol.FlightNumber);
			AssertEquals("Merged Segment 3 - PortOfLoading", "BNE", file.QuantumSegments[3].Consol.PortOfLoading);
			AssertEquals("Merged Segment 3 - PortOfDischarge", "POM", file.QuantumSegments[3].Consol.PortOfDischarge);
			AssertEquals("Merged Segment 3 - DepartureDate", new ZDateTime(2004, 8, 15), file.QuantumSegments[3].Consol.DepartureDate);
			AssertEquals("Merged Segment 3 - # of Shipments", 3, file.QuantumSegments[3].Shipments.Length);
			AssertEquals("Merged Segment 3 - # of ShipmentNotes", 3, file.QuantumSegments[3].ShipmentsNotes.Length);
			// Second Merged Segment - Array item 4
			AssertEquals("Merged Segment 4 - MasterBill", "07405320851", file.QuantumSegments[4].Consol.MasterBill);
			AssertEquals("Merged Segment 4 - FlightNumber", "PX5006", file.QuantumSegments[4].Consol.FlightNumber);
			AssertEquals("Merged Segment 4 - PortOfLoading", "SIN", file.QuantumSegments[4].Consol.PortOfLoading);
			AssertEquals("Merged Segment 4 - PortOfDischarge", "MEL", file.QuantumSegments[4].Consol.PortOfDischarge);
			AssertEquals("Merged Segment 4 - DepartureDate", new ZDateTime(2004, 8, 13), file.QuantumSegments[4].Consol.DepartureDate);
			AssertEquals("Merged Segment 4 - # of Shipments", 7, file.QuantumSegments[4].Shipments.Length);
			AssertEquals("Merged Segment 4 - # of ShipmentNotes", 7, file.QuantumSegments[4].ShipmentsNotes.Length);
		}

		#region Invalid File Name Tests
		public void TestValidFileName()
		{
			TestInvalidNameFormat("SYD.X1.00000000.000000.ok", false, TNTErrorType.InvalidFileName);
		}
		#endregion

		public void TestInvalidFileTypeForNZ()
		{
			TestInvalidNameFormat("AKL.X2.00000000.000000.ok", true, TNTErrorType.InvalidFileFormat);
			TestInvalidNameFormat("AKL.IND.20060713.150553.OK", false, TNTErrorType.InvalidFileFormat);
		}

		public void TestWrongBranchCode()
		{
			TestInvalidNameFormat("UUU.X2.20040404.140500.ok", true, TNTErrorType.InvalidFileName);
		}

		public void TestIncorrectFormat()
		{
			TestInvalidNameFormat("JJJ.ok", true, TNTErrorType.InvalidFileName);
		}

		public void TestInvalidFileType()
		{
			TestInvalidNameFormat("SYD.ID.20040817.180100.ok", true, TNTErrorType.InvalidFileName);
		}

		public void TestInvalidFileNameForNZ()
		{
			GlbCompany aKLCompany = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			aKLCompany.GC_Code = "NZC";
			aKLCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			GlbBranch aKLBranch = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			aKLBranch.GB_Code = "AKL";
			aKLBranch.GB_GC = aKLCompany.PK;
			aKLBranch.GB_RL_NKHomePort = "NZAKL";
			Factory.Save();
			TestInvalidNameFormat("AKL.IQDOWNI.20040817.180100.ok", true, TNTErrorType.InvalidFileName);
			TestInvalidNameFormat("akl.iqdowne.20060328.093016.ok", true, TNTErrorType.InvalidFileName);
			TestInvalidNameFormat("AKL.X2.20060713.150553.OK", false, TNTErrorType.InvalidFileName);
			TestInvalidNameFormat("AKL.IND.20060713.150553.OK", false, TNTErrorType.InvalidFileName);
		}

		protected void TestInvalidNameFormat(string fileName, bool expectingErrors, ErrorType expectedError)
		{
			var resourceBytes = new EmbeddedResourceRetriever().GetBytes(TNTTestUtils.ResourcePrefix + fileName);
			using (StreamReader fileData = new StreamReader(new MemoryStream(resourceBytes)))
			{
				NotificationBuffer buffer = new NotificationBuffer();
				ITransactionParticipant[] transactionActions;
				TNTDataImporter importer = new TNTDataImporter();
				importer.ImportDataToFactory(fileData, fileName, buffer, SourceInfo.EmptySourceInfo, out transactionActions);
				Factory.Save();
				if (expectingErrors)
				{
					AssertEquals("Expecting error in notify event buffer.", true, buffer.ContainsNotificationType(expectedError));
				}
				else
				{
					AssertEquals("Not expecting errors in notify event buffer.", false, buffer.ContainsNotificationType(expectedError));
				}
			}
		}

		protected TNTTestUtils TestUtils = new TNTTestUtils();
		protected override void TearDown()
		{
			TestUtils.DeleteTempDirectoryFiles();
			base.TearDown();
		}
	}
}
