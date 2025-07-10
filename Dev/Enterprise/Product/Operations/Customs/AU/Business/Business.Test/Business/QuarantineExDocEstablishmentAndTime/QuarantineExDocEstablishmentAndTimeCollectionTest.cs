using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocEstablishmentAndTimeCollection))]
	sealed class QuarantineExDocEstablishmentAndTimeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			QuarantineExDocLine line = Factory.New<QuarantineExDocLine>();
			return new QuarantineExDocEstablishmentAndTimeCollection(line);
		}

		public void TestProcessCount()
		{
			QuarantineExDocEstablishmentAndTime process1 = line.Processes.AddNew();
			QuarantineExDocEstablishmentAndTime process2 = line.Processes.AddNew();
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			Assert("Harvest Process Count is true", line.Processes.ProcessCounts.Harvest);
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			AssertEquals("Catcher Process Count has been incremented", 1, line.Processes.ProcessCounts.CatcherVessel);
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			AssertEquals("Catcher Process Count has been incremented", 2, line.Processes.ProcessCounts.CatcherVessel);
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherBoat;
			AssertEquals("CatcherBoat Process Count has been incremented", 1, line.Processes.ProcessCounts.CatcherBoat);
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherBoat;
			AssertEquals("CatcherBoat Process Count has been incremented", 2, line.Processes.ProcessCounts.CatcherBoat);
			AssertEquals("No AquacultureFarm processes", 0, line.Processes.ProcessCounts.AquacultureFarm);
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			AssertEquals("AquacultureFarm Process Count has been incremented", 1, line.Processes.ProcessCounts.AquacultureFarm);
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			AssertEquals("AquacultureFarm Process Count has been incremented", 2, line.Processes.ProcessCounts.AquacultureFarm);
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			AssertEquals("Freezing Process Count has been incremented", 1, line.Processes.ProcessCounts.Freezing);
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			AssertEquals("Freezing Process Count has been incremented", 2, line.Processes.ProcessCounts.Freezing);
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			Assert("Packing Process Count is true", line.Processes.ProcessCounts.Packing);
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			Assert("Processing Process Count is true", line.Processes.ProcessCounts.Processing);
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			Assert("Slaughter Process Count is true", line.Processes.ProcessCounts.Slaughter);
			process1.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			Assert("Storage Process Count is true", line.Processes.ProcessCounts.Storage);
		}

		public void TestClone()
		{
			var collection = (QuarantineExDocEstablishmentAndTimeCollection)GetCollectionToTest();
			var exdocProcess = collection.AddNew();
			exdocProcess.EE_AuthorisationEstablishmentID = "89";
			exdocProcess.EE_ProcessingType = "PC";
			exdocProcess.EE_StartDate = new ZDateTime(2006, 12, 12);

			var clonedLine = Factory.New<QuarantineExDocLine>();
			var result = new QuarantineExDocEstablishmentAndTimeCollection(clonedLine);
			result.Clone(collection, null);

			var clonedExdocProcess = result[0];
			AssertEquals("Parent ID set", clonedLine.PK, clonedExdocProcess.EE_QL);
			AssertEquals("Cloned Authorisation Establishment ID", "89", clonedExdocProcess.EE_AuthorisationEstablishmentID);
			AssertEquals("Cloned Processing Type", "PC", clonedExdocProcess.EE_ProcessingType);
			AssertEquals("Cloned Start Date", new ZDateTime(2006, 12, 12), clonedExdocProcess.EE_StartDate);
		}

		public void TestFindByProcessTypeAndEstablishment()
		{
			QuarantineExDocEstablishmentAndTimeCollection collection = (QuarantineExDocEstablishmentAndTimeCollection)GetCollectionToTest();
			QuarantineExDocEstablishmentAndTime exdocProcess = collection.AddNew();
			exdocProcess.EE_AuthorisationEstablishmentID = "89";
			exdocProcess.EE_ProcessingType = "PC";
			exdocProcess.EE_StartDate = new ZDateTime(2006, 12, 12);
			AssertNull("Invalid Authorisation Establishment and Process type", collection.FindByProcessTypeAndEstablishment("SL", "2313"));
			AssertNull("Invalid Authorisation Establishment", collection.FindByProcessTypeAndEstablishment("PC", "2313"));
			AssertNull("Invalid Process type", collection.FindByProcessTypeAndEstablishment("SL", "89"));
			AssertEquals("Valid everything", exdocProcess, collection.FindByProcessTypeAndEstablishment("PC", "89"));
		}

		public void TestSynchroniseProcessAddresses()
		{
			var aqisEstablishment = Factory.NewWithValidTestData<OrgHeader>();
			aqisEstablishment.OH_Code = "TAQS";
			aqisEstablishment.OH_FullName = "AQIS SYDNEY";
			var mainAddress = aqisEstablishment.MainAddress;
			mainAddress.Address1 = "185 O'RIORDAN ST";
			mainAddress.City = "MASCOT";
			mainAddress.State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.Postcode = "2020";
			var esnCusCode = mainAddress.CustomsCodes.AddNew();
			esnCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			esnCusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			esnCusCode.OK_CustomsRegNo = "77";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineLine = line1.QuarantineExDocLine;
			var process = Factory.New<QuarantineExDocEstablishmentAndTime>();
			process.EE_AuthorisationEstablishmentID = "77";
			quarantineLine.Processes.Add(process);

			AssertEquals(ZGuid.Empty, process.EE_E2_Address);
			AssertNoExceptionThrown(() => quarantineLine.Processes.SynchroniseProcessAddresses());
			AssertNotEquals(ZGuid.Empty, process.EE_E2_Address);
			AssertEquals("Creates a JobDocAddress that maps to the AQIS Establishment for code 77", aqisEstablishment.PK, process.Address.OrganisationPK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			line = Factory.New<QuarantineExDocLine>();
		}

		QuarantineExDocLine line;
	}
}
