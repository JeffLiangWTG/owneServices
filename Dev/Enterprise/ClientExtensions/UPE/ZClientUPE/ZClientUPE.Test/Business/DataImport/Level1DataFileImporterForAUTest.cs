using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterData.Business.Tests;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Level1DataFileImporterForAUTest : TestCaseWithFactory
	{
		[TestDate(2016, 6, 7, 16, 15, 19, 123)]
		public void TestLoadFile()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "23212345678";

			UPECusHAWB upeCusHAWB = Factory.New<UPECusHAWB>();
			upeCusHAWB.CS_HAWB = "1Z3824AR6640806327";
			upeCusHAWB.CS_CM = cusMAWB.PK;
			JobRelatedWayBill wayBill = Factory.NewWithValidTestData<JobRelatedWayBill>(TestBusinessObjectKind.MinimumRequiredToSave);
			wayBill.EB_ParentID = upeCusHAWB.PK;
			wayBill.EB_WaybillShortNumber = "3824ARFY9JH";
			wayBill.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;

			upeCusHAWB = Factory.New<UPECusHAWB>();
			upeCusHAWB.CS_HAWB = "1Z4R15V36659187478";
			upeCusHAWB.CS_CM = cusMAWB.PK;
			wayBill = Factory.NewWithValidTestData<JobRelatedWayBill>(TestBusinessObjectKind.MinimumRequiredToSave);
			wayBill.EB_ParentID = upeCusHAWB.PK;
			wayBill.EB_WaybillShortNumber = "4R15V3KYSM8";
			wayBill.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;

			Factory.Save();
			//run twice because running a second time should yield the same result
			//all the counters etc needs to be reset the second time
			AssertLoadFile();
			AssertEquals(7, notificationBuffer.Events.Length); //ProgressNotifications
			AssertLoadFile();
			AssertEquals(14, notificationBuffer.Events.Length); //ProgressNotifications
		}

		void AssertLoadFile()
		{
			Importer.LoadFile();

			AssertEquals(28, Importer.PercentageOfDuplicateHAWBs);

			var previousShipment1 = Importer.DuplicateShipmentDict.GetValueSafe("1Z3824AR6640806327");
			AssertEquals("previousShipment1.HouseBill", "1Z3824AR6640806327", previousShipment1.HouseBill);
			AssertEquals("previousShipment1.MasterBill", "23212345678", previousShipment1.MasterBill);

			var previousShipment2 = Importer.DuplicateShipmentDict.GetValueSafe("4R15V3KYSM8");
			AssertEquals("previousShipment2.HouseBill", "1Z4R15V36659187478", previousShipment2.HouseBill);
			AssertEquals("previousShipment2.MasterBill", "23212345678", previousShipment2.MasterBill);

			AssertEquals(3, Importer.IncorrectChildPacks.Count);
			Assert(Importer.IncorrectChildPacks.Contains("1Z3824AR6640806327: 0 childpack/s, 1 expected"));
			Assert(Importer.IncorrectChildPacks.Contains("1Z3824AR6641499935: 0 childpack/s, 2 expected"));
			Assert(Importer.IncorrectChildPacks.Contains("1Z3947806616452202: 2 childpack/s, 1 expected"));
		}

		public void TestLoadFile_InvalidFile()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1Sample That is Invalid.txt");
			Importer.LoadFile();

			string expectedErrorMessage = "Error Level 1 File is Invalid:\n" +
					"Input string was not in a correct format.\n" +
					"Approximate Line Number: 3\n\n" +
					"Line Value: US2795AU9639040422              C4A14T9J3YYD   4A14T9J3YYD           N 1 6    LBS US          USDNNNN           USDAKE32632QF    08695             USD4700     USD21089     USDN0 N   YEDI  18APR20046  LBS         USDD4    NNN NN  NN  N USD           USD    T1                      18APR20040000           26089      P/PNTF       6    LBSNNC0000051894QF12            N N 1   N \n\n" +
					"Previous Line Value: US2795AU9639040422              T4AX585GPRZC2000004AX585GPRZC           N 1 7    LBS US          USDNNNN N         USDAKE32632QF    09686             USD         USD5000      USDN0NN   NMAS1D19APR20047  LBS         AUDD3    NNN NN  NN  N USD           USD    T1                      19APR20041800           5000       P/PNDR            LBSNNC0000051894QF12            N N 1   N ";

			AssertMultilineEquals("", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.ToString(), '\n');
			ErrorReporter.Clear();//developer exception should not cause failure
		}

		public void TestLoadGCCShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCCLevel1Sample.txt");
			Importer.LoadFile();

			AssertEquals(1, Importer.TotalNoOfShipments);
			AssertEquals(173, Importer.TotalPiecesManifested);
			AssertEquals(35, Importer.TotalNoOfChildPackages);
			Assert("Port of Discharge/Destination discrepancy", ((ZString)Importer.IncorrectPortOfDestinationList[0]).EndsWith("9638"));
			AssertEquals(100, Importer.PercentageOfIncorrectPorts);
		}

		public void TestLoadWithEmptyShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SampleWithEmptyShipment_W3188389763.txt");
			Importer.LoadFile();

			AssertEquals(15, Importer.TotalNoOfShipments);
			AssertEquals(1, Importer.EmptyShipmentsList.Count);
			AssertEquals("W3188389763", Importer.EmptyShipmentsList[0]);

			//ensure that there is no double reporting
			Importer.LoadFile();

			AssertEquals(15, Importer.TotalNoOfShipments);
			AssertEquals(1, Importer.EmptyShipmentsList.Count);
			AssertEquals("W3188389763", Importer.EmptyShipmentsList[0]);
		}

		public void TestSaveDateImportEventLogged()
		{
			try
			{
				Importer.Save();
			}
			catch (Win32Exception ex)
			{
				//for amnesty fix WI00123536. really curious why this DLL cannot be loaded into memory sometimes
				if (ex.Message.Contains("RichEd20.DLL"))
				{
					Importer.Save();
					throw new InvalidOperationException("For WI00123536, failed to load RichEd20.DLL but worked on second save", ex);
				}
				else
				{
					throw;
				}
			}

			CusMAWB mAWB = GetCusMAWB("08122222222");

			foreach (StmALog log in mAWB.Logs.GetAllLogs())
			{
				if (log.SL_SE_NKEvent == Events.DataImport.Code)
				{
					AssertEquals("AU9639Level1Sample.txt", log.SL_Reference);
					break;
				}
			}
		}

		[TestDate(2016, 3, 7, 16, 15, 19, 123)]
		public void TestSplitShipment()
		{
			UPECusMAWB upeCusMAWB = Factory.New<UPECusMAWB>();
			upeCusMAWB.CM_MAWB = "08133333333";
			UPECusHAWB upeCusHAWB = (UPECusHAWB)upeCusMAWB.ChildBills.AddNew();
			upeCusHAWB.CS_HAWB = "1Z4AX5856644068241";
			upeCusHAWB.WayBillShort = "4AX585GPRZC";

			UPECusHAWB upeCusHAWB2 = (UPECusHAWB)upeCusMAWB.ChildBills.AddNew();
			upeCusHAWB2.CS_HAWB = "DONTCARE";
			upeCusHAWB2.WayBillShort = "4A14T9J3YYD";

			Factory.Save();

			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1FileToTestSplitShipments.txt");

			Importer.LoadFile();
			AssertEquals("duplicate shipment count", 2, Importer.DuplicateShipmentDict.Count);

			var duplicateShipment = Importer.DuplicateShipmentDict.GetValueSafe(upeCusHAWB.WayBillShort);
			AssertEquals(upeCusHAWB.CS_HAWB, duplicateShipment.HouseBill);
			AssertEquals(upeCusHAWB.CS_MasterBillNum, duplicateShipment.MasterBill);

			var duplicateShipment2 = Importer.DuplicateShipmentDict.GetValueSafe("1Z4A14T96650435917");
			AssertEquals(upeCusHAWB2.CS_HAWB, duplicateShipment2.HouseBill);
			AssertEquals(upeCusHAWB2.CS_MasterBillNum, duplicateShipment2.MasterBill);

			Importer.Save();

			// 1st duplicate shipment
			ZQuery hawbFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "1Z4AX5856644068241");
			hawbFilter.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, upeCusHAWB.PK);
			UPECusHAWB newUPECusHAWB = Factory.LoadTop1<UPECusHAWB>(hawbFilter);

			AssertNotNull("The house bill should be changed as the previous duplicate shipment", newUPECusHAWB);
			Assert("newUPECusHAWB should be split shipment", newUPECusHAWB.IsSplitShipment);

			ZQuery stmALogFilter = new ZQuery(StmALogSchema.SL_Parent, newUPECusHAWB.CurrentQueue.PK);
			stmALogFilter.AddToFilter(StmALogSchema.SL_Reference, "CUS\"\",\"DN\",\"\",\"\",\"\"");
			AssertEquals("There should be a DN in newUPECusHAWB", 1, Factory.GetDatabaseCount(typeof(StmALog), stmALogFilter));

			ZQuery jobRelatedWayBillFilter = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, newUPECusHAWB.PK);
			JobRelatedWayBill jobRelatedWayBill = Factory.LoadTop1<JobRelatedWayBill>(jobRelatedWayBillFilter);
			AssertEquals("JobRelatedWayBill.EB_WaybillNumber should be the same as newUPECusHAWB.CS_HAWB", newUPECusHAWB.CS_HAWB, jobRelatedWayBill.EB_WaybillNumber);

			// 2nd duplicate shipment
			hawbFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "DONTCARE");
			hawbFilter.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, upeCusHAWB2.PK);
			UPECusHAWB newUPECusHAWB2 = Factory.LoadTop1<UPECusHAWB>(hawbFilter);
			AssertNull("The house bill should not be changed", newUPECusHAWB2);

			hawbFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "1Z4A14T96650435917");
			newUPECusHAWB2 = Factory.LoadTop1<UPECusHAWB>(hawbFilter);

			AssertNotNull("The house bill should still be the one in level 1 file", newUPECusHAWB2);
			Assert("newUPECusHAWB2 should be split shipment", newUPECusHAWB2.IsSplitShipment);

			stmALogFilter = new ZQuery(StmALogSchema.SL_Parent, newUPECusHAWB2.CurrentQueue.PK);
			stmALogFilter.AddToFilter(StmALogSchema.SL_Reference, "CUS\"\",\"DN\",\"\",\"\",\"\"");
			AssertEquals("There should be a DN in newUPECusHAWB2", 1, Factory.GetDatabaseCount(typeof(StmALog), stmALogFilter));

			jobRelatedWayBillFilter = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, newUPECusHAWB2.PK);
			JobRelatedWayBill jobRelatedWayBill2 = Factory.LoadTop1<JobRelatedWayBill>(jobRelatedWayBillFilter);
			AssertEquals("JobRelatedWayBill.EB_WaybillNumber should be the same as newUPECusHAWB2.CS_HAWB", newUPECusHAWB2.CS_HAWB, jobRelatedWayBill2.EB_WaybillNumber);
		}

		[TestDate(2016, 3, 7, 16, 15, 19, 123)]
		public void TestSplitShipment_IfHAWBIsOutDated()
		{
			UPECusMAWB upeCusMAWB = Factory.New<UPECusMAWB>();
			upeCusMAWB.CM_MAWB = "08133333333";
			UPECusHAWB upeCusHAWB = (UPECusHAWB)upeCusMAWB.ChildBills.AddNew();
			upeCusHAWB.CS_HAWB = "1Z4AX5856644068241";
			upeCusHAWB.WayBillShort = "4AX585GPRZC";

			Factory.Save();

			TestDateAttribute.Date = new ZDateTime(2016, 6, 8, 16, 15, 19, 123).ToDateTime();
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1FileToTestSplitShipments.txt");
			Importer.LoadFile();
			AssertEquals("duplicate shipment count", 0, Importer.DuplicateShipmentDict.Count);

			Importer.Save();
			ZQuery hawbFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "1Z4AX5856644068241");
			hawbFilter.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, upeCusHAWB.PK);
			UPECusHAWB newUPECusHAWB = Factory.LoadTop1<UPECusHAWB>(hawbFilter);

			AssertNull("There should not be a HAWB with the same housebill", newUPECusHAWB);

			hawbFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "4AX585GPRZC");
			newUPECusHAWB = Factory.LoadTop1<UPECusHAWB>(hawbFilter);

			AssertNotNull("HAWB with the tracking number in level 1 file should be created", newUPECusHAWB);

			ZQuery stmALogFilter = new ZQuery(StmALogSchema.SL_Parent, upeCusHAWB.CurrentQueue.PK);
			stmALogFilter.AddToFilter(StmALogSchema.SL_Reference, "CUS\"\",\"DN\",\"\",\"\",\"\"");
			AssertEquals("It should not be a split shipment", 0, Factory.GetDatabaseCount(typeof(StmALog), stmALogFilter));
		}

		[TestDate(2016, 6, 7, 16, 15, 19, 123)]
		public void TestSplitShipment_IfShortNumberIsChild()
		{
			UPECusMAWB upeCusMAWB = Factory.New<UPECusMAWB>();
			upeCusMAWB.CM_MAWB = "08133333333";
			UPECusHAWB upeCusHAWB = (UPECusHAWB)upeCusMAWB.ChildBills.AddNew();
			upeCusHAWB.CS_HAWB = "1Z4AX5856644068241";
			JobRelatedWayBill wayBill = Factory.NewWithValidTestData<JobRelatedWayBill>(TestBusinessObjectKind.MinimumRequiredToSave);
			wayBill.EB_ParentID = upeCusHAWB.PK;
			wayBill.EB_WaybillShortNumber = "4AX585GPRZC";
			wayBill.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;

			Factory.Save();

			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1FileToTestSplitShipments.txt");
			Importer.LoadFile();
			AssertEquals("duplicate shipment count", 0, Importer.DuplicateShipmentDict.Count);

			Importer.Save();
			ZQuery hawbFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "1Z4AX5856644068241");
			hawbFilter.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, upeCusHAWB.PK);
			UPECusHAWB newUPECusHAWB = Factory.LoadTop1<UPECusHAWB>(hawbFilter);

			AssertNull("There should not be a HAWB with the same housebill", newUPECusHAWB);

			hawbFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "4AX585GPRZC");
			newUPECusHAWB = Factory.LoadTop1<UPECusHAWB>(hawbFilter);

			AssertNotNull("HAWB with the tracking number in level 1 file should be created", newUPECusHAWB);

			ZQuery stmALogFilter = new ZQuery(StmALogSchema.SL_Parent, upeCusHAWB.CurrentQueue.PK);
			stmALogFilter.AddToFilter(StmALogSchema.SL_Reference, "CUS\"\",\"DN\",\"\",\"\",\"\"");
			AssertEquals("It should not be a split shipment", 0, Factory.GetDatabaseCount(typeof(StmALog), stmALogFilter));
		}

		public void TestSavePostCMR_NotValidForSAC()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 30m);
			Importer.Save();

			CusMAWB mAWB = GetCusMAWB("08122222222");
			AssertMAWBDetails(Level1DataImport, mAWB);
			AssertEquals(7, mAWB.ChildBills.Count);

			UPECusHAWB uPECusHAWB = GetUPECusHAWB(mAWB, "1Z4A14T96650435917");
			AssertLevel1DataIsStored(uPECusHAWB);
			AssertHAWBDetails(uPECusHAWB);
			AssertEquals("Sanity check. Should not be valid. Goods Value is above the threshold", false, uPECusHAWB.IsValidForSAC);
			AssertNotNull("Not valid for SAC, should have formal declaration", uPECusHAWB.Declaration);
			AssertEquals("Goods value is above the threshold. Should not be valid as SAC", false, uPECusHAWB.CS_IsSelfAssessedClearance);
			AssertEquals("Should be valid for SAC. Message should be sent automatically", 1, uPECusHAWB.Messages.Count);
		}

		public void TestSavePostCMR_ValidForSAC()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 500m);
			Importer.Save();

			CusMAWB mAWB = GetCusMAWB("08122222222");
			AssertMAWBDetails(Level1DataImport, mAWB);
			AssertEquals(7, mAWB.ChildBills.Count);

			UPECusHAWB uPECusHAWB = GetUPECusHAWB(mAWB, "1Z4A14T96650435917");
			AssertLevel1DataIsStored(uPECusHAWB);
			AssertHAWBDetails(uPECusHAWB);
			AssertEquals("Sanity check. Should be valid. Goods Value is below the threshold", true, uPECusHAWB.IsValidForSAC);
			AssertEquals("Valid for SAC, formal declaration does not need to be created", false, uPECusHAWB.RequiresImporterOrConsigneeMatchApproval);
			AssertEquals("Should be valid for SAC", true, uPECusHAWB.CS_IsSelfAssessedClearance);
		}

		public void TestSavePostCMR_FDRequired_CRShouldBeSent()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);
			Importer.Save();

			CusMAWB mAWB = GetCusMAWB("08122222222");
			AssertMAWBDetails(Level1DataImport, mAWB);
			AssertEquals(7, mAWB.ChildBills.Count);

			ZQuery hAWBFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "1Z4A14T96650435917");
			CusHAWB[] hAWBs = (CusHAWB[])mAWB.ChildBills.Find(hAWBFilter);
			UPECusHAWB uPECusHAWB = (UPECusHAWB)hAWBs[0];
			AssertLevel1DataIsStored(uPECusHAWB);
			AssertHAWBDetails(uPECusHAWB);
			AssertNotNull("Declaration should be created", uPECusHAWB.Declaration);
			AssertEquals(1, uPECusHAWB.Messages.Count);
			AssertEquals(1, uPECusHAWB.ChildRelatedWayBills.Count);
			AssertEquals("1ZAT27736792092648", uPECusHAWB.ChildRelatedWayBills[0].EB_WaybillNumber);
			AssertEquals("", uPECusHAWB.ChildRelatedWayBills[0].EB_WaybillShortNumber);
		}

		public void TestFileWithoutLongTrackingNumberIsImportedFirst()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1FileToTestGenuineJob.txt");
			TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);
			Importer.Save();

			CusMAWB mAWB = GetCusMAWB("08122222222");

			// AA1235579412 is not a genuine job number because it doesn't match the pattern
			ZQuery hAWBFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "AA1235579412");
			CusHAWB[] hAWBs = (CusHAWB[])mAWB.ChildBills.Find(hAWBFilter);
			UPECusHAWB uPECusHAWB = (UPECusHAWB)hAWBs[0];

			AssertNull("Declaration should NOT be created", uPECusHAWB.Declaration);
			AssertEquals("No Message should be sent", 0, uPECusHAWB.Messages.Count);
			AssertEquals("AirCargo should be in INV queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);

			// B9876543210 is not a genuine job number because Manifested Pieces or Landed Pieces not eqauls to 1
			hAWBFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "B9876543210");
			hAWBs = (CusHAWB[])mAWB.ChildBills.Find(hAWBFilter);
			uPECusHAWB = (UPECusHAWB)hAWBs[0];

			AssertNull("Declaration should NOT be created", uPECusHAWB.Declaration);
			AssertEquals("No Message should be sent", 0, uPECusHAWB.Messages.Count);
			AssertEquals("AirCargo should be in INV queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);

			hAWBFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "A1234567890");
			hAWBs = (CusHAWB[])mAWB.ChildBills.Find(hAWBFilter);
			uPECusHAWB = (UPECusHAWB)hAWBs[0];

			AssertNotNull("Declaration should be created", uPECusHAWB.Declaration);
			AssertEquals("Message should be sent", 1, uPECusHAWB.Messages.Count);
			AssertNotEquals("AirCargo should not be in INV queue", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, uPECusHAWB.CurrentQueue.P4_CustomsQueue);
		}

		public void TestTranshipmentsAutomaticallySubmittedToCustoms()
		{
			TaxOrFeeTestHelper.SetDeminimus(Factory, 500m);
			Level1DataImport.PortOfDischarge = "NZAKL";
			Importer.Save();
			CusHAWB hAWB = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "1Z3824AR6641499935"));
			AssertEquals(1, hAWB.Messages.Count);
			Assert("SAC box should not be ticked for transhipments", !hAWB.CS_IsSelfAssessedClearance);
		}

		public void TestSaveGCCRecord()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCCLevel1Sample.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertMAWBDetails(Level1DataImport, cusMAWB);
			AssertEquals(1, cusMAWB.ChildBills.Count);

			AssertEquals((short)173, cusMAWB.ChildBills[0].CS_PiecesManifested);
			AssertEquals(46.6m, cusMAWB.ChildBills[0].CS_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, cusMAWB.ChildBills[0].CS_WeightUQ);
			AssertEquals(46.6m, cusMAWB.ChildBills[0].CS_ChargableWeight);
			AssertEquals(35, ((UPECusHAWB)cusMAWB.ChildBills[0]).ChildRelatedWayBills.Count);
			AssertEquals(136, ((UPECusHAWB)cusMAWB.ChildBills[0]).Level1Record._500000Lines.Count);
		}

		public void TestSaveGCCRecord_WithVirtualShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCCLevel1SampleWithV.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertMAWBDetails(Level1DataImport, cusMAWB);
			AssertEquals(3, cusMAWB.ChildBills.Count);

			ZQuery hAWBFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "1ZAY85140471016513");
			CusHAWB[] hAWBs = (CusHAWB[])cusMAWB.ChildBills.Find(hAWBFilter);
			UPECusHAWB uPECusHAWB = (UPECusHAWB)hAWBs[0];

			AssertEquals("1ZAY85140471016513 CS_Weight", 106.7m, uPECusHAWB.CS_Weight);
			AssertEquals("1ZAY85140471016513 CS_ChargableWeight", 106.7m, uPECusHAWB.CS_ChargableWeight);
			AssertEquals("1ZAY85140471016513 ChildRelatedWayBills.Count", 6, uPECusHAWB.ChildRelatedWayBills.Count);
			AssertEquals("1ZAY85140471016513 CS_PiecesManifested", (short)17, uPECusHAWB.CS_PiecesManifested);

			hAWBFilter = new ZQuery(CusHAWBSchema.CS_HAWB, "1ZAY85140472359286");
			hAWBs = (CusHAWB[])cusMAWB.ChildBills.Find(hAWBFilter);
			uPECusHAWB = (UPECusHAWB)hAWBs[0];

			AssertEquals("1ZAY85140472359286 CS_Weight", 3.746m, uPECusHAWB.CS_Weight);
			AssertEquals("1ZAY85140472359286 CS_ChargableWeight", 3.746m, uPECusHAWB.CS_ChargableWeight);
			AssertEquals("1ZAY85140472359286 ChildRelatedWayBills.Count", 1, uPECusHAWB.ChildRelatedWayBills.Count);
			AssertEquals("1ZAY85140472359286 CS_PiecesManifested", (short)2, uPECusHAWB.CS_PiecesManifested);
		}

		public void TestSaveWithEmptyShipment()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "SampleWithEmptyShipment_W3188389763.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertMAWBDetails(Level1DataImport, cusMAWB);
			AssertEquals(15, cusMAWB.ChildBills.Count);
		}

		public void TestConsigneeIsAutoMatched()
		{
			var orgImporter = Factory.New<UPEOrgHeader>();
			orgImporter.OH_IsConsignee = true;
			orgImporter.OH_FullName = "INSTRON PTY LIMITED";
			orgImporter.OH_RL_NKClosestPort = "AUMEL";
			orgImporter.OH_Code = "INSTROMEL";
			orgImporter.MainAddress.OA_City = "BAYSWATER";
			orgImporter.MainAddress.OA_Address1 = "15/15 STUD ROAD";

			var consignor = Factory.New<UPEOrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_FullName = "INSTRON CORP.";
			consignor.OH_RL_NKClosestPort = "USNDY";
			consignor.OH_Code = "INSTRONDY";
			consignor.MainAddress.OA_City = "NORWOOD";
			consignor.MainAddress.OA_Address1 = "825 UNIVERSITY AVE";
			consignor.MainAddress.OA_Phone = "17818282500";
			consignor.MainAddress.OA_PostCode = "02062";
			consignor.AccountNumber = "011968";

			var consignee = Factory.New<UPEOrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "FORD PRODUCT DEVELOP";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.OH_Code = "FORMOTMEL";
			consignee.MainAddress.OA_Address1 = "5-19 PRINCES HIGHWAY";
			consignee.MainAddress.OA_City = "NORLANE";
			consignee.MainAddress.OA_PostCode = "3214";

			consignee.CreatePatternMatchingAddressFromMainAddress(Factory);
			consignee.CreatePatternMatchingName(Factory);

			Factory.Save();

			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "TestAU8691T9.288");
			Importer.Save();

			var hawb = Factory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "1Z0119686756603912"));
			AssertNotNull("Declaration should be created", hawb.Declaration);
			AssertEquals("Factory used by the ValueObjectImportContext in ProcessHousebills might have been changed", consignee.PK, hawb.Declaration.Consignee.PK);

			var autoMatchImportFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code);
			autoMatchImportFilter.AddToFilter(StmALogSchema.SL_Reference, "Consignee: FORMOTMEL");
			AssertEquals("Should be an AutoMatchDone event for Importer", 1, hawb.Logs.GetAllLogs().Find(autoMatchImportFilter).Length);
		}

		public void TestInvalidOriginPortCodesIsImported()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCCLevel1Sample.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertEquals(1, cusMAWB.ChildBills.Count);
			AssertEquals("3296", cusMAWB.ChildBills[0].CS_RL_NKOrigin);
		}

		public void TestDestinationPortCodeIsTheSameAsDischarge()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCCLevel1Sample.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertEquals(1, cusMAWB.ChildBills.Count);
			AssertEquals("AUSYD", cusMAWB.ChildBills[0].CS_RL_NKDestination);
		}

		public void TestDestinationPortCodeIsWhenCountryOfDestinationIsNotAustralia()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadCurrencyNotFound.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertEquals("AUSYD", cusMAWB.CM_RL_NKDischargePort);
			AssertEquals("8699", cusMAWB.ChildBills[0].CS_RL_NKDestination);
		}

		public void TestStateIsNotSetFromUNLoco()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "GCCLevel1Sample.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertEquals(1, cusMAWB.ChildBills.Count);
			AssertEquals("", cusMAWB.ChildBills[0].CS_ConsigneeState);
		}

		public void TestIfCurrencyNotFoundThenDoNotConvertToAUD()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadCurrencyNotFound.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertEquals(1, cusMAWB.ChildBills.Count);
			AssertEquals("XYZ", cusMAWB.ChildBills[0].CS_RX_NKGoodsCurrency);
			AssertEquals(0m, cusMAWB.ChildBills[0].CS_GoodsValue);
		}

		public void TestConvertInvalidCurrencyAUS()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadCurrencyAUS.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertEquals(1, cusMAWB.ChildBills.Count);
			AssertEquals("AUD", cusMAWB.ChildBills[0].CS_RX_NKGoodsCurrency);
			AssertEquals(51m, cusMAWB.ChildBills[0].CS_GoodsValue);
		}

		public void TestConvertInvalidCurrencyRMB()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadCurrencyRMB.txt");
			Importer.Save();

			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, "08122222222");
			CusMAWB cusMAWB = GetCusMAWB("08122222222");
			AssertEquals(1, cusMAWB.ChildBills.Count);
			AssertEquals("AUD", cusMAWB.ChildBills[0].CS_RX_NKGoodsCurrency);
			AssertNotEquals("After conversion, the value is not equal to 0.", 0m, cusMAWB.ChildBills[0].CS_GoodsValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRemarksIsSetWithStopPhraseMessage()
		{
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "TOOL" };
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "CLEET" };
			Level1DataImport.FileName = Path.Combine(BaseSourcePath, UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1SampleFile.txt"));
			Importer.Save();

			UPECusHAWB hAWB = Factory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "1Z40E3726627413340"));
			AssertEquals("Remarks field", "ANY:TOOL,CLEET", hAWB.CurrentQueue.P4_CustomsReason);
		}
		public void TestPopulateVendorIdFieldForSAC()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadVendorId.txt");
			TaxOrFeeTestHelper.SetDeminimus(Factory, 500m);
			Importer.Save();

			CusMAWB cusMawb = GetCusMAWB("08122222222");
			AssertEquals(1, cusMawb.ChildBills.Count);
			AssertEquals("65744055555", cusMawb.ChildBills[0].CS_VendorIdentifier);
		}

		public void TestPopulateVendorIdFieldForNON_SAC()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "Level1LoadVendorId.txt");
			TaxOrFeeTestHelper.SetDeminimus(Factory, 30m);
			Importer.Save();

			CusMAWB cusMawb = GetCusMAWB("08122222222");
			AssertEquals(1, cusMawb.ChildBills.Count);
			AssertEquals("", cusMawb.ChildBills[0].CS_VendorIdentifier);
		}

		public void TestServiceLevel()
		{
			Level1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "AULevel1ServiceLevels.txt");
			Importer.Save();

			CusMAWB cusMawb = GetCusMAWB("08122222222");
			AssertEquals(4, cusMawb.ChildBills.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Bill 1Z3824AR6640806327 has Service Level 1", "1", cusMawb.ChildBills.Cast<CusHAWB>().Single(x => x.CS_HAWB == "1Z3824AR6640806327").CS_RS_NK_ServiceLevel);
				AssertEquals("Bill 1Z4R15V3665 has Service Level 5", "5", cusMawb.ChildBills.Cast<CusHAWB>().Single(x => x.CS_HAWB == "1Z4R15V3665").CS_RS_NK_ServiceLevel);
				AssertEquals("Bill 1Z3824AR6641499935 has Service Level 21", "21", cusMawb.ChildBills.Cast<CusHAWB>().Single(x => x.CS_HAWB == "1Z3824AR6641499935").CS_RS_NK_ServiceLevel);
				AssertEquals("Bill 1ZAE51680493200057 has Service Level 28", "28", cusMawb.ChildBills.Cast<CusHAWB>().Single(x => x.CS_HAWB == "1ZAE51680493200057").CS_RS_NK_ServiceLevel);
			});
		}

		#region Implementation

		protected CusMAWB GetCusMAWB(string masterBillNumber)
		{
			ZQuery mAWBFilter = new ZQuery(CusMAWBSchema.CM_MAWB, masterBillNumber);
			CusMAWB[] mAWBs = (CusMAWB[])Factory.Load(typeof(CusMAWB), mAWBFilter);
			AssertEquals(1, mAWBs.Length);
			return mAWBs[0];
		}

		UPECusHAWB GetUPECusHAWB(CusMAWB mAWB, ZString houseBillNumber)
		{
			ZQuery hAWBFilter = new ZQuery(CusHAWBSchema.CS_HAWB, houseBillNumber);
			CusHAWB[] hAWBs = (CusHAWB[])mAWB.ChildBills.Find(hAWBFilter);
			return (UPECusHAWB)hAWBs[0];
		}

		void AssertLevel1DataIsStored(UPECusHAWB uPECusHAWB)
		{
			string expected =
				"US2795AU9639040422              D4A14T9J3YYD2000004A14T9J3YYD           N 1 6    LBS US          USDNNNN           USDAKE32632QF    08695             USD4700     USD21089     USDN0 N   YEDI  18APR20046  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      18APR20040000           56089      P/PNTF       6    LBSNNC0000051894QF12            N N 1   N \n" +
				"US2795AU9639040422              D4A14T9J3YYD2020001Z4A14T96650435917                 6      6      2    N                                                            2491846057E                AU09639  S1AU9639TF.113B2004-04-21                              Y                                                                             AUDNNNNNBI                                  \n" +
				"US2795AU9639040422              D4A14T9J3YYD300000038694514A14T9    CAFEPRESS.COM                      26010 EDEN LANDING ROAD            SUITE 5                            HAYWARD                                                CA94545    US 18778091659                                                                               7906969                            ATTENTION S\n" +
				"US2795AU9639040422              D4A14T9J3YYD400000        8AU0596441MR C D MOUNTFORD                   MR C D MOUNTFORD         15 BOWER STREET                    HIGHGATE HILL                      HIGHGATE HILL                                          VI4101     AU 61421684220                                                                                      237           \n" +
				"US3295AU9639050704              NA0562F39CJN401000170596390000A46979W.L. GORE & ASSOCIATES (AUST)      ContactName              P.O. BOX 232                       ABN 90 002 134 465                 FRENCHS FOREST                                         NS1640     AU 123456789                                                                                                      \n" +
				"US2795AU9639040422              D4A14T9J3YYD5000001   EA THE MOST COMFORTABLE TSHIRT EVER! OUR 100% COTTON HANES BEEFYT IS PRESHRUNK DURABLE AND GUARANTEE       27399     USD7906969             US                                   AU7783461                                                                                                                                          \n" +
				"US2795AU9639040422              D4A14T9J3YYD5010001   EA OUR SUPER SOFT 100% COMBED COTTON RIBBED BABY DOLL TSHIRT FROM AMERICAN APPAREL WILL KEEP YOU IN ST     1699      USD7906969             US                                   AU7784328                                                                                                                                          \n" +
				"US2795AU9639040422              D4A14T9J3YYD5020001   EA OUR SUPER SOFT 100% COMBED COTTON RIBBED BABY DOLL TSHIRT FROM AMERICAN APPAREL WILL KEEP YOU IN ST     1699      USD7906969             US                                   AU7784328                                                                                                                                          \n" +
				"US2795AU9639040422              D4A14T9J3YYD5030001   EA THE MOST COMFORTABLE TSHIRT EVER! OUR 100% COTTON HANES BEEFYT IS PRESHRUNK DURABLE AND GUARANTEE       1499      USD7906969             US                                   AU7784083                                                                                                                                          \n" +
				"US2795AU9639040422              D4A14T9J3YYD5040001   EA THE MOST COMFORTABLE TSHIRT EVER! OUR 100% COTTON HANES BEEFYT IS PRESHRUNK DURABLE AND GUARANTEE       1499      USD7906969             US                                   AU7784083                                                                                                                                          \n" +
				"US2795AU9639040422              D4A14T9J3YYD5050001   EA ENJOY THE COMFORT OF OUR ROOMY 100% COTTON OPEN FLY BOXERS FROM ROBINSON APPAREL.  GREAT FOR UNDERWE    1299      USD7906969             US                                   AU10414834                                                                                                                                         \n" +
				"US2795AU9639040422              D4A14T9J3YYD5060001   EA THE PERFECT CASUAL WEAR FOR THE OFFICE OUR ANVIL GOLF SHIRTS ARE MADE OF 100% PRESHRUNK HEAVYWEIGHT     1699      USD7906969             US                                   AU6633334                                                                                                                                          \n" +
				"US2795AU9639040422              D4A14T9J3YYD5070001   EA THE PERFECT CASUAL WEAR FOR THE OFFICE OUR ANVIL GOLF SHIRTS ARE MADE OF 100% PRESHRUNK HEAVYWEIGHT     1999      USD7906969             US                                   AU6633334                                                                                                                                          \n" +
				"US2795AU9639040422              D4A14T9J3YYD5080001   EA DECORATE ANY ROOM IN YOUR HOME OR OFFICE WITH OUR 10 INCH WALL CLOCK.  BLACK PLASTIC CASE.  MADE IN T   1099      USD7906969             US                                   AU10414861                                                                                                                                         \n" +
				"US2795AU9639040422              D4A14T9J3YYD5090001   EA OUR RETRO SILVER LUNCHBOX BRINGS BACK MEMORIES OF CHILDHOOD WITH MODERN DAY FUNCTIONALITY.  THE IMAGE   1399      USD7906969             US                                   AU7989393                                                                                                                                          \n" +
				"US2795AU9639040422              D4A14T9J3YYD5100001   EA GET READY TO CRUISE THE URBAN JUNGLE IN OUR VINTAGE TRUCKER HAT.   <UL><LI>FOAM FRONT</LI>  <LI>PLAST   1099      USD7906969             US                                   AU7783497                                                                                                                                          \n" +
				"US3295AU9639050704              DAT2773T8Z9J60000099999999999    10 LBS         USD         USD          USD                             C0000092779UPS6901         NY1ZAT27736792092648                 10     AAY89758UPS                                                                                                                                                               \n" +
				"US2795AU9639040422              D4A14T9J3YYD900000                                     18APR2004EDI                  GIFT ITEM. NO RESALE VALUE                                                                                                                                                                                                                                           \n";

			AssertMultilineASCIIEquals("", expected, uPECusHAWB.Level1Record.ToString());
		}

		void AssertMAWBDetails(Level1DataImport level1DataImport, CusMAWB mAWB)
		{
			AssertEquals(level1DataImport.PortOfLoading, mAWB.CM_RL_NKLoadPort);
			AssertEquals(level1DataImport.PortOfDischarge, mAWB.CM_RL_NKDischargePort);
			AssertEquals(level1DataImport.FlightNumber, mAWB.CM_FlightNo);
			AssertEquals("08144444444", mAWB.CM_MasterHouseBill);

			AssertEquals("Summary Information From Load", mAWB.Notes.FindByDescription("LoadSummaryInformationNote")[0].ST_NoteDataAsText);
			AssertEquals("DuplicateHAWB Notes", mAWB.Notes.FindByDescription("DuplicateHAWBNote")[0].ST_NoteDataAsText);
			AssertEquals("MasterbillWarning Note", mAWB.Notes.FindByDescription(Level1DataImport.Schema.MasterbillWarningNote)[0].ST_NoteDataAsText);
			AssertEquals("SurplusIndicatedNote Notes", mAWB.Notes.FindByDescription(Level1DataImport.Schema.SurplusIndicatedNote)[0].ST_NoteDataAsText);
			AssertEquals("FlightNotInScheduleNote Notes", mAWB.Notes.FindByDescription(Level1DataImport.Schema.FlightNotInScheduleNote)[0].ST_NoteDataAsText);
			AssertEquals("ArrivalDateWarning Note", mAWB.Notes.FindByDescription(Level1DataImport.Schema.ArrivalDateWarningNote)[0].ST_NoteDataAsText);
			AssertEquals("UnmatchedFilename Note", mAWB.Notes.FindByDescription("FileNameWarningNote")[0].ST_NoteDataAsText);
		}

		void AssertHAWBDetails(UPECusHAWB hAWB)
		{
			AssertEquals("1Z4A14T96650435917", hAWB.CS_HAWB);
			AssertEquals("USLAX", hAWB.CS_RL_NKOrigin);
			AssertEquals(hAWB.MAWB.CM_RL_NKDischargePort, hAWB.CS_RL_NKDestination);
			AssertEquals("THE MOST COMFORTABLE TSHIRT EVER! OUR 100% COTTON HANES BEEFYT IS PRESHRUNK DURABLE AND GUARANTEE", hAWB.CS_GoodsDescription);
			AssertEquals("08144444444", hAWB.CS_MasterHouseBill);
			AssertEquals(369.71M, hAWB.CS_GoodsValue);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, hAWB.CS_RX_NKGoodsCurrency);
			AssertEquals(6M, hAWB.CS_Weight);
			AssertEquals("LB", hAWB.CS_WeightUQ);
			AssertEquals(2.722M, hAWB.CS_ChargableWeight);
			AssertEquals((ZShort)2, hAWB.CS_PiecesManifested);
			AssertEquals("1", hAWB.CS_RS_NK_ServiceLevel);

			AssertEquals("HIGHGATE HILL", hAWB.CS_ConsigneeCity);
			AssertEquals("MR C D MOUNTFORD", hAWB.CS_ConsigneeContactName);
			AssertEquals("MR C D MOUNTFORD", hAWB.CS_ConsigneeName);
			AssertEquals("61421684220", hAWB.CS_ConsigneePhone);
			AssertEquals("4101", hAWB.CS_ConsigneePostcode);
			AssertEquals("VI", hAWB.CS_ConsigneeState);
			AssertEquals("15 BOWER STREET", hAWB.CS_ConsigneeStreet);
			AssertEquals("HIGHGATE HILL", hAWB.CS_ConsigneeStreet2);
			AssertEquals("AU", hAWB.CS_RN_NKConsigneeCountry);

			AssertEquals("HAYWARD", hAWB.CS_ConsignorCity);
			AssertEquals("ATTENTION S", hAWB.CS_ConsignorContactName);
			AssertEquals("CAFEPRESS.COM", hAWB.CS_ConsignorName);
			AssertEquals("18778091659", hAWB.CS_ConsignorPhone.TrimStart('+'));
			AssertEquals("94545", hAWB.CS_ConsignorPostcode);
			AssertEquals("CA", hAWB.CS_ConsignorState);
			AssertEquals("26010 EDEN LANDING ROAD", hAWB.CS_ConsignorStreet);
			AssertEquals("SUITE 5", hAWB.CS_ConsignorStreet2);
			AssertEquals("US", hAWB.CS_RN_NKConsignorCountry);

			ZQuery importerFilter = new ZQuery(OrgPatternMatchAddressSchema.P3_ParentID, hAWB.PK);
			importerFilter.AddToFilter(OrgPatternMatchAddressSchema.P3_ParentTableCode, CusHAWBSchema.Constants.Prefix);
			importerFilter.AddToFilter(OrgPatternMatchAddressSchema.P3_AddressType, OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter);
			OrgPatternMatchAddress importer = Factory.LoadTop1<OrgPatternMatchAddress>(importerFilter);

			AssertEquals("W.L. GORE & ASSOCIATES (AUST)", importer.P3_CompanyName);
			AssertEquals("P.O. BOX 232", importer.P3_Address1);
			AssertEquals("ABN 90 002 134 465", importer.P3_Address2);
			AssertEquals("FRENCHS FOREST", importer.P3_City);
			AssertEquals("NS", importer.P3_State);
			AssertEquals("1640", importer.P3_PostCode);
			AssertEquals("123456789", importer.P3_Phone);
			AssertEquals("ContactName", importer.P3_ContactName);

			AssertEquals("CS_IsSurplus", true, hAWB.CS_IsSurplus);
			AssertEquals("ShipmentType", ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments, hAWB.ShipmentType);
			AssertEquals("DutyType", DutyTypeCodeDescriptionPairList.Codes.Dutiable, hAWB.DutyType);
			AssertEquals("BillingTerms", BillingTermsCodeDescriptionPairList.Codes.Prepaid, hAWB.BillingTerms);
			AssertEquals("CS_FreightPrepaidCollect", CMRMethodsOfPayment.Codes.PrepaidOnly, hAWB.CS_FreightPrepaidCollect);
			AssertEquals("CS_OtherSystemConsigneeCode", "8AU0596441", hAWB.CS_OtherSystemConsigneeCode);
			AssertEquals("CS_OtherSystemConsignorCode", "4A14T9", hAWB.CS_OtherSystemConsignorCode);

			ZQuery jobRelatedWayBillParentFilter = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, hAWB.PK);
			jobRelatedWayBillParentFilter.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			JobRelatedWayBill[] jobRelatedWayBillParents = (JobRelatedWayBill[])Factory.Load(typeof(JobRelatedWayBill), jobRelatedWayBillParentFilter);
			AssertEquals(1, jobRelatedWayBillParents.Length);
			AssertEquals("1Z4A14T96650435917", jobRelatedWayBillParents[0].EB_WaybillNumber);
			AssertEquals("4A14T9J3YYD", jobRelatedWayBillParents[0].EB_WaybillShortNumber);

			ZQuery jobRelatedWayBillChildFilter = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, hAWB.PK);
			jobRelatedWayBillChildFilter.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, JobRelatedWayBill.Constants.RelatedWayBillType.Child);
			JobRelatedWayBill[] jobRelatedWayBillChildren = (JobRelatedWayBill[])Factory.Load(typeof(JobRelatedWayBill), jobRelatedWayBillChildFilter);
			AssertEquals(1, jobRelatedWayBillChildren.Length);

			AssertEquals("1ZAT27736792092648", jobRelatedWayBillChildren[0].EB_WaybillNumber);
			AssertEquals("", jobRelatedWayBillChildren[0].EB_WaybillShortNumber);
		}

		protected Level1DataImport Level1DataImport
		{
			get
			{
				if (fLevel1DataImport == null)
				{
					fLevel1DataImport = new Level1DataImport(Factory);
					fLevel1DataImport.FlightNumber = "QF656";
					fLevel1DataImport.ArrivalDate = ZDateTime.Now;
					fLevel1DataImport.PortOfLoading = "SGSIN";
					fLevel1DataImport.PortOfDischarge = "AUSYD";
					fLevel1DataImport.IsSurplus = true;
					fLevel1DataImport.MasterBill = "08122222222";
					fLevel1DataImport.CoLoadMasterBill = "08144444444";
					fLevel1DataImport.SurplusIndicatedNote = "SurplusIndicatedNote Notes";
					fLevel1DataImport.FlightNotInScheduleNote = "FlightNotInScheduleNote Notes";
					fLevel1DataImport.DuplicateHAWBsNote = "DuplicateHAWB Notes";
					fLevel1DataImport.MasterbillWarningNote = "MasterbillWarning Note";
					fLevel1DataImport.ArrivalDateWarningNote = "ArrivalDateWarning Note";
					fLevel1DataImport.UnmatchedFilenameNote = "UnmatchedFilename Note";

					fLevel1DataImport.FileName = UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "AU9639Level1Sample.txt");
				}
				return fLevel1DataImport;
			}
		}
		Level1DataImport fLevel1DataImport;

		void PopulateRefLocoMap()
		{
			MasterFiles.Business.RefLocoMap uSLocoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
			uSLocoMap.RY_LocalPortCode = "2795";
			uSLocoMap.RY_RL_NKLocoPort = "USLAX";
			uSLocoMap.RY_RN = RefCountry.LoadFromCountryCode(Factory, "AU").PK;
			uSLocoMap.RY_SystemUsage = UPEDataLine.Constants.RefLocoSystemUsage;

			MasterFiles.Business.RefLocoMap sYDLocoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
			sYDLocoMap.RY_LocalPortCode = "9639";
			sYDLocoMap.RY_RL_NKLocoPort = "AUSYD";
			sYDLocoMap.RY_RN = RefCountry.LoadFromCountryCode(Factory, "AU").PK;
			sYDLocoMap.RY_SystemUsage = UPEDataLine.Constants.RefLocoSystemUsage;
			Factory.Save();
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();

			currentCompanyFactory = new BusinessObjectFactory();
			currentRegNo = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			currentCompany = currentCompanyFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			currentCompany.PrimaryRegistrationNumber.Number = "21 003 980 130 123";
			currentCompanyFactory.Save();

			PopulateRefLocoMap();
			notificationBuffer = new NotificationBuffer();
			Level1DataImport.LoadSummaryInformation = "Summary Information From Load";

			TaxOrFeeTestHelper.SetUp();
		}
		ZString currentRegNo;
		OrgHeader currentCompany;
		BusinessObjectFactory currentCompanyFactory;
		NotificationBuffer notificationBuffer;

		protected override void TearDown()
		{
			currentCompany.PrimaryRegistrationNumber.Number = currentRegNo;
			currentCompanyFactory.Save();

			base.TearDown();
			tempDir?.Dispose();
		}

		protected Level1DataFileImporterForAU Importer => importer ?? (importer = new Level1DataFileImporterForAU(Level1DataImport, notificationBuffer));
		Level1DataFileImporterForAU importer;

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		#endregion
	}
}
