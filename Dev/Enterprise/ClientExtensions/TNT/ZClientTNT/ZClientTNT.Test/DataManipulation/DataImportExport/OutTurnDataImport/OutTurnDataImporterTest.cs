using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.OutTurnDataImport.Testing
{
	public class OutTurnDataImporterTest : TestCaseWithFactory
	{
		public void TestImportData_CMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			OutTurnDataImporter importer = new OutTurnDataImporter();
			NotificationBuffer buffer = new NotificationBuffer();
			CreateCusHAWBs();
			CreateTestFile();
			importer.ImportData(TestFileName, buffer, SourceInfo.EmptySourceInfo);
			AssertEquals("Buffer contain: " + System.Environment.NewLine + buffer.AsString, false, buffer.HasWarnings);
			AssertEquals("ShortLandedHAWB should not have Housebill's Underbond Movement", 0, ShortLandedHAWB.AllUnderbonds.Count);
			AssertEquals("NilDiscrepancyHAWB should not have Housebill's Underbond Movement", 0, NilDiscrepancyHAWB.AllUnderbonds.Count);
			AssertEquals("SurplusPackagesHAWB should not have Housebill's Underbond Movement", 0, SurplusPackagesHAWB.AllUnderbonds.Count);
			AssertEquals("MasterBill has 1 Masterbill Underbond Movement", 1, MasterBill.AllUnderbonds.Count);
			CusUnderbond underbond = MasterBill.AllUnderbonds[0];
			AssertEquals("Underbond's ParentID", MasterBill.PK, underbond.C4_ParentID);
			AssertEquals("Underbond's MovementReason", CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, underbond.C4_MovementReason);
			AssertEquals("Underbond's FlightNo", "QF1680", underbond.C4_FlightNo);
			AssertEquals("Underbond's ArrivalDate", new ZDateTime(2005, 8, 5), underbond.C4_ArrivalDate);
			AssertEquals("Underbond's Outturns count", 3, underbond.Outturns.Count);
			ZQuery shortLandedFilter = new ZQuery(CusOutturnSchema.C5_ParentID, ShortLandedHAWB.PK);
			shortLandedFilter.AddToFilter(CusOutturnSchema.C5_PackagesOutturned, 4);
			shortLandedFilter.AddToFilter(CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.ShortLanded);
			AssertCollectionContains(shortLandedFilter, underbond.Outturns);
			AssertEquals("ShortLandedHAWB's PiecesLanded", (short)5, ShortLandedHAWB.CS_PiecesLanded);
			ZQuery nilDiscrepancyFilter = new ZQuery(CusOutturnSchema.C5_ParentID, NilDiscrepancyHAWB.PK);
			nilDiscrepancyFilter.AddToFilter(CusOutturnSchema.C5_PackagesOutturned, 5);
			nilDiscrepancyFilter.AddToFilter(CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.NilDiscrepancy);
			AssertCollectionContains(nilDiscrepancyFilter, underbond.Outturns);
			AssertEquals("NilDiscrepancyHAWB's PiecesLanded", (short)5, NilDiscrepancyHAWB.CS_PiecesLanded);
			ZQuery surplusPackagesFilter = new ZQuery(CusOutturnSchema.C5_ParentID, SurplusPackagesHAWB.PK);
			surplusPackagesFilter.AddToFilter(CusOutturnSchema.C5_PackagesOutturned, 6);
			surplusPackagesFilter.AddToFilter(CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.SurplusPackages);
			AssertCollectionContains(surplusPackagesFilter, underbond.Outturns);
			AssertEquals("SurplusPackagesHAWB's PiecesLanded", (short)5, SurplusPackagesHAWB.CS_PiecesLanded);
		}

		public void TestNotifyingOfMissingData()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			OutTurnDataImporter importer = new OutTurnDataImporter();
			NotificationBuffer buffer = new NotificationBuffer();
			CreateCusHAWBs();
			CreateTestFile();
			string noMatchMessage = "No Match found For HAWB " + ShortLandedHAWB.CS_HAWB;
			ShortLandedHAWB.CS_HAWB = "MissingHAWB";
			string missingSectorInfo = NilDiscrepancyHAWB.UnderbondHumanReadableName + " is missing Sector Information";
			StmNote[] notes = NilDiscrepancyHAWB.Notes.FindByDescription(ConsignmentUpdator.OriginalQuantumSectorNoteDescription);
			AssertEquals("PreCondition: NilDiscrepancyHAWB should have only 1 Sector Note", 1, notes.Length);
			notes[0].ST_Description = "Not Sector Description";
			notes = SurplusPackagesHAWB.Notes.FindByDescription(ConsignmentUpdator.OriginalQuantumSectorNoteDescription);
			AssertEquals("PreCondition: SurplusPackagesHAWB should have only 1 Sector Note", 1, notes.Length);
			string notSectorInfo = string.Format("{0} does not contain Sector Info ({1})", SurplusPackagesHAWB.UnderbondHumanReadableName, notes[0].ST_NoteDataAsText);
			notes[0].ST_NoteDataAsText = "Not Sector Data";
			Factory.Save();
			importer.ImportData(TestFileName, buffer, SourceInfo.EmptySourceInfo);
			AssertEquals("Buffer contain: " + System.Environment.NewLine + buffer.AsString, true, buffer.HasWarnings);
			AssertEquals(string.Format("Buffer should contain '{0}': {1}{2}", noMatchMessage, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(noMatchMessage) >= 0);
			AssertEquals(string.Format("Buffer should contain '{0}': {1}{2}", missingSectorInfo, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(missingSectorInfo) >= 0);
			AssertEquals(string.Format("Buffer should contain '{0}': {1}{2}", notSectorInfo, System.Environment.NewLine, buffer.AsString), true, buffer.AsString.IndexOf(notSectorInfo) >= 0);
			AssertEquals("ShortLandedHAWB should not have Housebill's Underbond Movement", 0, ShortLandedHAWB.AllUnderbonds.Count);
			AssertEquals("NilDiscrepancyHAWB should not have Housebill's Underbond Movement", 0, NilDiscrepancyHAWB.AllUnderbonds.Count);
			AssertEquals("SurplusPackagesHAWB should not have Housebill's Underbond Movement", 0, SurplusPackagesHAWB.AllUnderbonds.Count);
			AssertEquals("MasterBill should have no Masterbill Underbond Movement in Legacy System", 0, MasterBill.AllUnderbonds.Count);
			AssertEquals("ShortLandedHAWB's PiecesLanded", (short)5, ShortLandedHAWB.CS_PiecesLanded);
			AssertEquals("NilDiscrepancyHAWB's PiecesLanded", (short)5, NilDiscrepancyHAWB.CS_PiecesLanded);
			AssertEquals("SurplusPackagesHAWB's PiecesLanded", (short)5, SurplusPackagesHAWB.CS_PiecesLanded);
		}

		public void TestUpdateExistingData()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			OutTurnDataImporter importer = new OutTurnDataImporter();
			NotificationBuffer buffer = new NotificationBuffer();
			CreateCusHAWBs();
			CreateTestFile();
			CusUnderbond noMatchUnderbond = MasterBill.AllUnderbonds.AddNew();
			noMatchUnderbond.C4_ParentID = MasterBill.PK;
			noMatchUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			noMatchUnderbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Air;
			noMatchUnderbond.C4_FlightNo = MasterBill.CM_FlightNo;
			noMatchUnderbond.C4_ArrivalDate = MasterBill.CM_ArrivalDate.AddDays(1);
			CusUnderbond matchUnderbond = MasterBill.AllUnderbonds.AddNew();
			matchUnderbond.C4_ParentID = MasterBill.PK;
			matchUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			matchUnderbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Rail;
			matchUnderbond.C4_FlightNo = MasterBill.CM_FlightNo;
			matchUnderbond.C4_ArrivalDate = MasterBill.CM_ArrivalDate;
			// Response Pending
			ShortLandedHAWB.CS_IsResponsePending = true;
			// Update existing Outturn
			Customs.Business.CusOutturn outTurn = matchUnderbond.Outturns.AddNew();
			outTurn.Parent = NilDiscrepancyHAWB;
			outTurn.C5_PackagesOutturned = 10;
			outTurn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;
			Factory.Save();
			importer.ImportData(TestFileName, buffer, SourceInfo.EmptySourceInfo);
			AssertEquals("Buffer contain: " + System.Environment.NewLine + buffer.AsString, false, buffer.HasWarnings);
			AssertEquals("ShortLandedHAWB should not have Housebill's Underbond Movement", 0, ShortLandedHAWB.AllUnderbonds.Count);
			AssertEquals("NilDiscrepancyHAWB should not have Housebill's Underbond Movement", 0, NilDiscrepancyHAWB.AllUnderbonds.Count);
			AssertEquals("SurplusPackagesHAWB should not have Housebill's Underbond Movement", 0, SurplusPackagesHAWB.AllUnderbonds.Count);
			AssertEquals("MasterBill has 2 Masterbill Underbond Movement", 2, MasterBill.AllUnderbonds.Count);
			AssertEquals("Masterbill should contain MatchUnderbond", true, MasterBill.AllUnderbonds.Contains(matchUnderbond.PK));
			AssertEquals("Underbond's ParentID", MasterBill.PK, matchUnderbond.C4_ParentID);
			AssertEquals("Underbond's MovementReason", CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, matchUnderbond.C4_MovementReason);
			AssertEquals("Underbond's ModeOfMovement", CMRUnderbondModeOfMovement.Codes.Rail, matchUnderbond.C4_ModeOfMovement);
			AssertEquals("Underbond's FlightNo", "QF1680", matchUnderbond.C4_FlightNo);
			AssertEquals("Underbond's ArrivalDate", new ZDateTime(2005, 8, 5), matchUnderbond.C4_ArrivalDate);
			AssertEquals("Underbond's Outturns count", 2, matchUnderbond.Outturns.Count);
			AssertEquals("ShortLandedHAWB's CS_IsResponsePending", true, ShortLandedHAWB.CS_IsResponsePending);
			AssertEquals("ShortLandedHAWB's PiecesLanded", (short)5, ShortLandedHAWB.CS_PiecesLanded);
			ZQuery nilDiscrepancyFilter = new ZQuery(CusOutturnSchema.C5_ParentID, NilDiscrepancyHAWB.PK);
			nilDiscrepancyFilter.AddToFilter(CusOutturnSchema.C5_PackagesOutturned, 5);
			nilDiscrepancyFilter.AddToFilter(CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.NilDiscrepancy);
			AssertCollectionContains(nilDiscrepancyFilter, matchUnderbond.Outturns);
			AssertEquals("NilDiscrepancyHAWB's PiecesLanded", (short)5, NilDiscrepancyHAWB.CS_PiecesLanded);
			AssertEquals("Existing Outturn", true, matchUnderbond.Outturns.Contains(outTurn.PK));
			AssertEquals("Outturn's C5_PackagesOutturned", 5, outTurn.C5_PackagesOutturned);
			AssertEquals("Outturn's C5_OutturnResultType", CMROutturnResultType.Codes.NilDiscrepancy, outTurn.C5_OutturnResultType);
			ZQuery surplusPackagesFilter = new ZQuery(CusOutturnSchema.C5_ParentID, SurplusPackagesHAWB.PK);
			surplusPackagesFilter.AddToFilter(CusOutturnSchema.C5_PackagesOutturned, 6);
			surplusPackagesFilter.AddToFilter(CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.SurplusPackages);
			AssertCollectionContains(surplusPackagesFilter, matchUnderbond.Outturns);
			AssertEquals("SurplusPackagesHAWB's PiecesLanded", (short)5, SurplusPackagesHAWB.CS_PiecesLanded);
		}

		public void TestCatchZSaveConcurrencyError()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			OutTurnDataImporterForTest importer = new OutTurnDataImporterForTest();
			NotificationBuffer buffer = new NotificationBuffer();
			CreateCusHAWBs();
			CreateTestFile();
			importer.ImportData(TestFileName, buffer, SourceInfo.EmptySourceInfo);
			AssertEquals("Buffer has errors", true, buffer.HasErrors);
			AssertEquals("Importer will retry 3 times to update the record", 3, importer.reTry);
			ZString expectedErrorMessage = "Air Cargo House Record (153630581) cannot be updated as it is currently modifying by other user or process";
			AssertEquals("Buffer have expected error message", true, buffer.AsString.IndexOf(expectedErrorMessage) != -1);
		}

		class OutTurnDataImporterForTest : OutTurnDataImporter
		{
			public OutTurnDataImporterForTest() : base()
			{
				reTry = 0;
			}

			internal int reTry;
			protected override void SetPiecesLandedAndSave(OutTurnFlatFileDataRow row, CusHAWB houseBill, NotificationBuffer buffer, BusinessObjectFactoryProvider factoryProvider)
			{
				if (row.HAWB == "153630581")
				{
					reTry++;
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, null), factoryProvider.Current);
				}
				else
				{
					base.SetPiecesLandedAndSave(row, houseBill, buffer, factoryProvider);
				}
			}
		}

		#region Setup
		void CreateTestFile()
		{
			DeleteIfExists(TestFileName);
			File.Copy(SourceFile, TestFileName);
			File.SetAttributes(TestFileName, FileAttributes.Normal);
		}

		void CreateCusHAWBs()
		{
			MasterBill = Factory.New<CusMAWB>();
			MasterBill.CM_MAWB = "08196106301";
			MasterBill.CM_RL_NKDischargePort = "AUSYD";
			MasterBill.CM_RL_NKLoadPort = "JPTYO";
			MasterBill.CM_FlightNo = "QF1680";
			MasterBill.CM_ArrivalDate = new ZDateTime(2005, 8, 5);
			ShortLandedHAWB = MasterBill.ChildBills.AddNew();
			ShortLandedHAWB.CS_PiecesManifested = (short)5;
			ShortLandedHAWB.CS_PiecesLanded = (short)5;
			ShortLandedHAWB.CS_RL_NKOrigin = "TYO";
			ShortLandedHAWB.CS_RL_NKDestination = "SYD";
			ShortLandedHAWB.CS_HAWB = "153630581";
			ShortLandedHAWB.Notes.AddNew(true, ConsignmentUpdator.OriginalQuantumSectorNoteDescription, "QF168008196106301 TYOSYD050805A");
			NilDiscrepancyHAWB = MasterBill.ChildBills.AddNew();
			NilDiscrepancyHAWB.CS_PiecesManifested = (short)5;
			NilDiscrepancyHAWB.CS_PiecesLanded = (short)5;
			NilDiscrepancyHAWB.CS_RL_NKOrigin = "TYO";
			NilDiscrepancyHAWB.CS_RL_NKDestination = "BNE";
			NilDiscrepancyHAWB.CS_HAWB = "153630582";
			NilDiscrepancyHAWB.Notes.AddNew(true, ConsignmentUpdator.OriginalQuantumSectorNoteDescription, "QF168008196106301 TYOSYD050805A");
			SurplusPackagesHAWB = MasterBill.ChildBills.AddNew();
			SurplusPackagesHAWB.CS_PiecesManifested = (short)5;
			SurplusPackagesHAWB.CS_PiecesLanded = (short)5;
			SurplusPackagesHAWB.CS_RL_NKOrigin = "TYO";
			SurplusPackagesHAWB.CS_RL_NKDestination = "MEL";
			SurplusPackagesHAWB.CS_HAWB = "153630583";
			SurplusPackagesHAWB.Notes.AddNew(true, ConsignmentUpdator.OriginalQuantumSectorNoteDescription, "QF168008196106301 TYOSYD050805A");
			Factory.Save();
		}

		CusMAWB MasterBill;
		CusHAWB ShortLandedHAWB;
		CusHAWB NilDiscrepancyHAWB;
		CusHAWB SurplusPackagesHAWB;
		string TestDirectory;
		string SourceFile;
		string TestFileName;
		EmbeddedResourceRetriever resourceRetriever;
		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			TestDirectory = Path.Combine(Env.TempPath, "OutTurnDirectory");
			SourceFile = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.DataImportExport.Testing.SYD.20051004.090432.out");
			TestFileName = Path.Combine(TestDirectory, "SYD.20050805.090432.out");
			TempDirectory.DeleteDirectory(TestDirectory);
			Directory.CreateDirectory(TestDirectory);
		}

		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(TestDirectory);
			base.TearDown();
			resourceRetriever.Dispose();
		}
		#endregion
	}
}
