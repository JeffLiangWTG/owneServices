using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Client.UPE.Business.ServiceTask.Testing
{
	[TestedType(typeof(ResolutionQueueServiceTask))]
	internal class ResolutionQueueBatchProcessorTest : ServiceTaskTestCase<ResolutionQueueServiceTask>
	{
		public void TestHumanReadableName()
		{
			var attributes = GetHostedServiceAttributes();
			AssertEquals("UPS Resolution Queue Processor", attributes[0].Description);
		}

		public void TestLogFilterIsWithoutNoLock()
		{
			ZQuery filter = Processor.ConstructFilter(ZDateTime.Now, ZDateTime.Now);
			AssertEquals("Filter should be without  so it doesnt process rolled back transaction records", false, filter.IsNoLock);
		}

		[TestDate(2005, 11, 2)]
		public void TestRunTask()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			Factory.Save();

			var cusHAWB = TestHelper.CreateCusHAWB("115", AutoCargoReportQueueCodeDescriptionPairList.Codes.Completed, AutoCommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding);
			cusHAWB.Declaration.JE_GB = branch.PK;
			cusHAWB.MAWB.CM_GB = branch.PK;
			TestHelper.MasterBill = null;
			Factory.Save();

			UPEDataRegistry.Instance.ProcessQueueProcessorHWM = ZDateTime.Now;
			TestHelper.SetHouseBillTestData();
			TestDateAttribute.Date = ZDateTime.Now.AddHours(2).ToDateTime();
			AssertTestCusHAWBsDoNotHaveResolutionCode(27);
			Processor.RunTask();
			UPECusHAWB[] houseBills = new BusinessObjectFactory().Load<UPECusHAWB>(TestHelper.HouseBillFilter);
			IList expectedFinalisedHAWBs = new ZString[] { "101", "108", "109", "110", "115", "124", "125" };
			AssertEquals(27, houseBills.Length);
			AssertEquals(7, Processor.AddedResolutionShipmentPKs.Count);
			foreach (UPECusHAWB houseBill in houseBills)
			{
				if (expectedFinalisedHAWBs.Contains(houseBill.CS_HAWB) && ((houseBill.CurrentQueue.P4_CustomsStatus != CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn
				|| (houseBill.CurrentQueue.FirstUPECusHAWB.Messages.LastIncomingMessage == null || houseBill.CurrentQueue.FirstUPECusHAWB.Messages.LastIncomingMessage.EM_MessageType != UPECargoReportQueue.EdiMessageTypes.Withdrawn))))
				{
					string expectedResolutionCode = (houseBill.Declaration != null && houseBill.Declaration.CurrentQueue.P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding)
						? ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms
						: ResolutionCodeDescriptionPairList.Codes.DA_Released;
					AssertEquals("Finalised shipment should have a resolution code", expectedResolutionCode, houseBill.CurrentQueue.ResolutionCode);
				}
				else
				{
					AssertEquals("Non-finalised shipments should have no resolution code", ZString.Empty, houseBill.CurrentQueue.ResolutionCode);
				}
			}
			AssertEquals("High water mark should be adjusted", new ZDateTime(2005, 11, 2, 1, 59, 0), UPEDataRegistry.Instance.ProcessQueueProcessorHWM);
		}

		[TestDate(2005, 11, 2)]
		public void TestRunTask_NotAU()
		{
			foreach (var br in Factory.Load<GlbBranch>(new ZQuery()))
			{
				br.SetCountry(CountryCodes.Taiwan);
			}
			Factory.Save();

			Processor.RunTask();

			AssertEquals("", Processor.Logger.ToString());
		}

		public void TestRun_ValidBranch()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			Processor.RunTask();

			AssertEquals("", Logger.ToString());

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Processor.RunTask();

			AssertNotNullOrEmpty(Logger.ToString());
		}

		[TestDate(2005, 11, 1, 9, 0, 0)]
		public void TestRunTask_TimeStampFilter()
		{
			UPEDataRegistry.Instance.ProcessQueueProcessorHWM = new ZDateTime(2005, 11, 1, 10, 0, 0);
			SetupTestDataForTimeStampFilterTest();
			AssertTestCusHAWBsDoNotHaveResolutionCode(4);
			Processor.RunTask();
			UPECusHAWB[] hAWBs = new BusinessObjectFactory().Load<UPECusHAWB>(TestHelper.HouseBillFilter);
			IList expectedFinalisedHAWBsInThe1stRun = new string[] { "101", "102", "103" };
			AssertEquals(4, hAWBs.Length);
			foreach (UPECusHAWB hAWB in hAWBs)
			{
				if (expectedFinalisedHAWBsInThe1stRun.Contains((string)hAWB.CS_HAWB))
				{
					AssertEquals("Finalised shipment should have a resolution code", ResolutionCodeDescriptionPairList.Codes.DA_Released, hAWB.CurrentQueue.ResolutionCode);
				}
				else
				{
					AssertEquals("Non-finalised shipments should have no resolution code", ZString.Empty, hAWB.CurrentQueue.ResolutionCode);
				}
			}

			TestDateAttribute.Date = ZDateTime.Now.AddMonths(1).ToDateTime();
			Processor.RunTask();
			hAWBs = new BusinessObjectFactory().Load<UPECusHAWB>(TestHelper.HouseBillFilter);
			AssertEquals(4, hAWBs.Length);
			foreach (UPECusHAWB hAWB in hAWBs)
			{
				AssertEquals("Should have a resolution code", ResolutionCodeDescriptionPairList.Codes.DA_Released, hAWB.CurrentQueue.ResolutionCode);
			}
		}

		void SetupTestDataForTimeStampFilterTest()
		{
			// Set Time Buffer to 8 hours
			UPEDataRegistry.Instance.ProcessQueueProcessorTimeBuffer = 8;

			UPECusHAWB hAWB1 = TestHelper.CreateCusHAWB("101", CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			UPECusHAWB hAWB2 = TestHelper.CreateCusHAWB("102", CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			UPECusHAWB hAWB3 = TestHelper.CreateCusHAWB("103", CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			UPECusHAWB hAWB4 = TestHelper.CreateCusHAWB("104", CargoReportQueueCodeDescriptionPairList.Codes.Completed, string.Empty, string.Empty);
			Factory.Save();

			// These logs should be included in the 1st run
			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 10, 0, 0).ToDateTime();
			hAWB1.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Chase;
			hAWB2.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			hAWB3.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();

			// These logs should not be included in 1st run
			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 18, 0, 0).ToDateTime();
			hAWB4.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.OnFile;
			Factory.Save();
		}

		[TestDate(2005, 11, 1, 9, 0, 0)]
		public void TestExecute_SplitShipment()
		{
			UPEDataRegistry.Instance.ProcessQueueProcessorHWM = new ZDateTime(2005, 11, 1, 9, 0, 0);
			UPEDataRegistry.Instance.ProcessQueueProcessorTimeBuffer = 8;

			UPECusHAWB cusHAWB1 = TestHelper.CreateCusHAWB("101", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Hold, string.Empty);
			UPECusHAWB cusHAWB2 = TestHelper.CreateCusHAWB("102", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			//one more split
			UPECusHAWB cusHAWB3 = TestHelper.CreateCusHAWB("201", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			cusHAWB1.WayBillShort = "FOO";
			cusHAWB2.WayBillShort = "FOO";
			cusHAWB3.WayBillShort = "BAR";
			Factory.Save();

			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 11, 0, 0).ToDateTime();
			AssertTestCusHAWBsDoNotHaveResolutionCode(3);
			Processor.RunTask();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			UPECusHAWB reloadedCusHAWB1 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "101"));
			UPECusHAWB reloadedCusHAWB2 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "102"));
			UPECusHAWB reloadedCusHAWB3 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "201"));
			AssertEquals("Non-finalised shipment 101 should have no resolution code", ZString.Empty, reloadedCusHAWB1.CurrentQueue.ResolutionCode);
			AssertEquals("Finalised shipment 102 should not have a resolution code until its split shipment 101 is finalised", ZString.Empty, reloadedCusHAWB2.CurrentQueue.ResolutionCode);
			AssertEquals("Finalised shipment 201 should have a resolution code", ResolutionCodeDescriptionPairList.Codes.DA_Released, reloadedCusHAWB3.CurrentQueue.ResolutionCode);

			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 18, 0, 0).ToDateTime();
			cusHAWB1.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			Processor.RunTask();

			reloadedCusHAWB1 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "101"));
			reloadedCusHAWB2 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "102"));
			AssertEquals("Finalised shipment 101 should have a resolution code as all shipments with the same consignment reference are finalised", ResolutionCodeDescriptionPairList.Codes.DA_Released, reloadedCusHAWB1.CurrentQueue.ResolutionCode);
			AssertEquals("Finalised shipment 102 should have a resolution code after shipment 101 is finalised", ResolutionCodeDescriptionPairList.Codes.DA_Released, reloadedCusHAWB2.CurrentQueue.ResolutionCode);
		}

		[TestDate(2005, 11, 1, 9, 0, 0)]
		public void TestExecute_DebugLog()
		{
			UPEDataRegistry.Instance.ProcessQueueProcessorHWM = new ZDateTime(2005, 11, 1, 9, 0, 0);
			UPEDataRegistry.Instance.ProcessQueueProcessorTimeBuffer = 8;
			UPECusHAWB cusHAWB3 = TestHelper.CreateCusHAWB("333", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed);

			Factory.Save();

			AssertNull(Factory.LoadTop1<EDIMessage>(new ZQuery()));
			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 11, 0, 0).ToDateTime();

			Processor.RunTask();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			UPECusHAWB reloadedCusHAWB = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "333"));
			AssertEquals("Finalised shipment 333 should have a resolution code", ResolutionCodeDescriptionPairList.Codes.DA_Released, reloadedCusHAWB.CurrentQueue.ResolutionCode);

			var logs = Logger.ToString();
			Assert("Should Contain Processing Log", logs.Contains("Processing Shipment with TrackingId '333'; Short TrackingId '333'; MAWB 'MAWB101'."));
			Assert("Should Contain Resolution Log", logs.Contains("Shipment is moved to Resolution Queue."));

			Logger.ClearLog();
			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 18, 0, 0).ToDateTime();

			UPECusHAWB cusHAWB4 = TestHelper.CreateCusHAWB("444", CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, CommercialQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			var cPLlog = cusHAWB4.CurrentQueue.CommercialQueueLogs.AddNew(CommercialQueueCodeDescriptionPairList.Codes.Completed, "", "", "", "");
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			Processor.RunTask();

			reloadedCusHAWB = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "444"));
			AssertEquals("Finalised shipment 444 should not have a resolution code", "", reloadedCusHAWB.CurrentQueue.ResolutionCode);

			logs = Logger.ToString();
			Assert("Should Contain Processing Log", logs.Contains($"====== Start Processing Queue Events (Login Branch: {Env.CurrentBranch.Code}) ======"));
			Assert("Should Contain Processing Log",logs.Contains($"Processing Queue Event with SL_PostedTimeUTC {cPLlog.SL_PostedTimeUtc}"));
			Assert("Should Contain current HWM Log", logs.Contains("Current HWM= 01-Nov-05 10:59:00"));
			Assert("Should Contain Processing Log", logs.Contains($"Queue Event with PK '{cPLlog.PK}' belongs to a non-finalised shipment."));
			Assert("Should Contain new HWM Log", logs.Contains($"New HWM={UPEDataRegistry.Instance.ProcessQueueProcessorHWM}"));
		}

		[TestDate(2005, 11, 1, 9, 0, 0)]
		public void TestExecute_Cancel()
		{
			UPEDataRegistry.Instance.ProcessQueueProcessorHWM = new ZDateTime(2005, 11, 1, 9, 0, 0);
			UPEDataRegistry.Instance.ProcessQueueProcessorTimeBuffer = 8;

			UPECusHAWB cusHAWB1 = TestHelper.CreateCusHAWB("101", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);

			Factory.Save();

			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 11, 0, 0).ToDateTime();
			var comLog = cusHAWB1.CurrentQueue.CommercialQueueLogs.AddNew(CommercialQueueCodeDescriptionPairList.Codes.Completed, "", "", "", "");
			Factory.Save();

			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 13, 0, 0).ToDateTime();
			cusHAWB1.CurrentQueue.CustomsQueueLogs.AddNew(CommercialQueueCodeDescriptionPairList.Codes.Completed, "", "", "", "");
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			Processor.RunTask(new CancellationTokenSource(), 5);

			var reloadedCusHAWB1 = new BusinessObjectFactory().LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "101"));

			AssertEquals("Finalised shipment 101 should have a resolution code", "DA", reloadedCusHAWB1.CurrentQueue.ResolutionCode);

			var logs = Logger.ToString();
			Assert("Should Contain HWM Log", logs.Contains($"Current HWM= 01-Nov-05 09:00:00"));
			Assert("Should Contain Cancellation Log", logs.Contains("Warning: Service Task Run is cancelled. HWM will be set to 01-Nov-05 11:00:00."));
		}

		[TestDate(2005, 11, 1, 9, 0, 0)]
		public void TestExecute_SplitShipmentsWithCusHawbLinkedToDeclaration()
		{
			UPEDataRegistry.Instance.ProcessQueueProcessorHWM = new ZDateTime(2005, 11, 1, 9, 0, 0);
			UPEDataRegistry.Instance.ProcessQueueProcessorTimeBuffer = 8;

			UPECusHAWB cusHAWB1 = TestHelper.CreateCusHAWB("101", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Hold, AutoDeclarationQueueCodeDescriptionPairList.Codes.Completed);
			UPECusHAWB cusHAWB2 = TestHelper.CreateCusHAWB("102", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			//one more split
			UPECusHAWB cusHAWB3 = TestHelper.CreateCusHAWB("201", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			cusHAWB1.WayBillShort = "FOO";
			cusHAWB2.WayBillShort = "FOO";
			cusHAWB3.WayBillShort = "BAR";

			var child1 = cusHAWB1.ChildRelatedWayBills.AddNew();
			child1.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;
			child1.EB_WaybillNumber = "AAA";

			child1 = cusHAWB1.ChildRelatedWayBills.AddNew();
			child1.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;
			child1.EB_WaybillNumber = "BBB";

			cusHAWB1.Logs.AddNew(Events.DataExport, "101|03|SR");
			cusHAWB1.Logs.AddNew(Events.DataExport, "AAA|03|SR");
			cusHAWB1.Logs.AddNew(Events.DataExport, "BBB|03|SR");
			cusHAWB1.Declaration.Logs.AddNew(Events.DataExport, "101|03|X2");
			cusHAWB1.Declaration.Logs.AddNew(Events.DataExport, "AAA|03|X2");
			cusHAWB1.Declaration.Logs.AddNew(Events.DataExport, "BBB|03|X2");

			cusHAWB2.Logs.AddNew(Events.DataExport, "102|03|NY");
			cusHAWB2.Declaration.Logs.AddNew(Events.DataExport, "102|03|BA");

			cusHAWB3.Logs.AddNew(Events.DataExport, "201|03|RJ");
			cusHAWB3.Declaration.Logs.AddNew(Events.DataExport, "201|03|ZZ");

			Factory.Save();

			AssertNull(Factory.LoadTop1<EDIMessage>(new ZQuery()));
			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 11, 0, 0).ToDateTime();
			AssertTestCusHAWBsDoNotHaveResolutionCode(3);
			AssertNull(Factory.LoadTop1<EDIMessage>(new ZQuery()));

			Processor.RunTask();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			UPECusHAWB reloadedCusHAWB1 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "101"));
			UPECusHAWB reloadedCusHAWB2 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "102"));
			UPECusHAWB reloadedCusHAWB3 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "201"));
			AssertEquals("Non-finalised shipment 101 should have no resolution code", ZString.Empty, reloadedCusHAWB1.CurrentQueue.ResolutionCode);
			AssertEquals("Finalised shipment 102 should not have a resolution code until its split shipment 101 is finalised", ZString.Empty, reloadedCusHAWB2.CurrentQueue.ResolutionCode);
			AssertEquals("Finalised shipment 201 should have a resolution code", ResolutionCodeDescriptionPairList.Codes.DA_Released, reloadedCusHAWB3.CurrentQueue.ResolutionCode);

			var edimessages = newFactory.Load<EDIMessage>(new ZQuery());

			AssertEquals("There should be 2 EDImessages", 2, edimessages.Length);

			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB3.PK, newFactory, "RJ", reloadedCusHAWB3.CS_HAWB);
			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB3.Declaration.PK, newFactory, "ZZ", reloadedCusHAWB3.CS_HAWB);

			edimessages.ForEach(m => m.DeleteFromTest());

			Logger.ClearLog();
			TestDateAttribute.Date = new ZDateTime(2005, 11, 1, 18, 0, 0).ToDateTime();
			cusHAWB2.CurrentQueue.AddResolutionCodeLog();
			cusHAWB3.CurrentQueue.AddResolutionCodeLog();
			cusHAWB1.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			Processor.RunTask();

			var logs = Logger.ToString();
			reloadedCusHAWB1 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "101"));
			reloadedCusHAWB2 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "102"));
			AssertEquals("Finalised shipment 101 should have a resolution code as all shipments with the same consignment reference are finalised", ResolutionCodeDescriptionPairList.Codes.DA_Released, reloadedCusHAWB1.CurrentQueue.ResolutionCode);
			AssertEquals("Finalised shipment 102 should have a resolution code after shipment 101 is finalised", ResolutionCodeDescriptionPairList.Codes.DA_Released, reloadedCusHAWB2.CurrentQueue.ResolutionCode);

			Assert("Should Contain Processing Log", logs.Contains("Processing Shipment with TrackingId '101'; Short TrackingId 'FOO'; MAWB 'MAWB101'."));
			Assert("Should Contain Processing Log", logs.Contains("All related Split Shipments of short TrackingID 'FOO' are ready to Release."));
			Assert("Should Contain Processing Log", logs.Contains($"Move related Split Shipment on MAWB 'MAWB101; ShipmentPK '{reloadedCusHAWB2.PK}' to Resolution Queue."));

			edimessages = newFactory.Load<EDIMessage>(new ZQuery());
			AssertEquals("There should be 8 EDImessages", 8, edimessages.Length);

			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB1.Declaration.PK, newFactory, "X2", "101");
			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB1.Declaration.PK, newFactory, "X2", "AAA");
			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB1.Declaration.PK, newFactory, "X2", "BBB");
			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB1.PK, newFactory, "SR", "101");
			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB1.PK, newFactory, "SR", "AAA");
			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB1.PK, newFactory, "SR", "BBB");

			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB2.PK, newFactory, "NY", "102");
			TestHelper.AssertContainsResolutionExportLog(reloadedCusHAWB2.Declaration.PK, newFactory, "BA", "102");
		}

		[TestDate(2005, 11, 1, 9, 0, 0)]
		public void TestExecute_SplitShipments_CompletedAtTheSameTime()
		{
			UPEDataRegistry.Instance.ProcessQueueProcessorHWM = new ZDateTime(2005, 11, 1, 9, 0, 0);
			UPEDataRegistry.Instance.ProcessQueueProcessorTimeBuffer = 8;

			UPECusHAWB cusHAWB1 = TestHelper.CreateCusHAWB("101", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			UPECusHAWB cusHAWB2 = TestHelper.CreateCusHAWB("102", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed);
			cusHAWB1.WayBillShort = "FOO";
			cusHAWB2.WayBillShort = "FOO";
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			Processor.RunTask();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			ZQuery encodedResolutionCodeReferenceFilter = new ZQuery(StmALogSchema.SL_Reference, Array.ConvertAll(new ResolutionCodeDescriptionPairList().ToArray(),
						resolution => ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, string.Empty, resolution.Code, string.Empty, string.Empty, string.Empty)));

			UPECusHAWB reloadedCusHAWB1 = newFactory.LoadTop1<UPECusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "101"));
			ZQuery resolutionCodeFilter = new ZQuery();
			resolutionCodeFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.QueueChanged.Code);
			resolutionCodeFilter.AddToFilter(StmALogSchema.SL_Parent, reloadedCusHAWB1.CurrentQueue.PK);
			resolutionCodeFilter.AddToFilter(encodedResolutionCodeReferenceFilter);
			AssertEquals(1, newFactory.Load<StmALog>(resolutionCodeFilter).Length);

			UPEDataRegistry.Instance.ProcessQueueProcessorHWM = new ZDateTime(2005, 11, 1, 9, 0, 0);
			Processor.RunTask();
			AssertEquals("Shipment is released already. It shouldn't release again", 1, newFactory.Load<StmALog>(resolutionCodeFilter).Length);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void AssertTestCusHAWBsDoNotHaveResolutionCode(int expectedHAWBCount)
		{
			UPECusHAWB[] houseBills = Factory.Load<UPECusHAWB>(TestHelper.HouseBillFilter);
			AssertEquals(expectedHAWBCount, houseBills.Length);
			foreach (UPECusHAWB houseBill in houseBills)
			{
				AssertEquals(ZString.Empty, houseBill.CurrentQueue.ResolutionCode);
			}
		}

		protected override void SetUpCore()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUpCore();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			Logger.ClearLog();
		}

		LoggerForTest Logger
		{
			get { return logger ?? (logger = new LoggerForTest()); }
		}
		LoggerForTest logger;

		ResolutionQueueServiceTaskForTest Processor
		{
			get { return processor ?? (processor = new ResolutionQueueServiceTaskForTest(Logger)); }
		}
		ResolutionQueueServiceTaskForTest processor;

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;

		class ResolutionQueueServiceTaskForTest : ResolutionQueueServiceTask
		{
			public ResolutionQueueServiceTaskForTest(ILogger logger)
				: base(logger)
			{
			}

			CancellationTokenSource cts;
			int cancelOnCount;
			int noOfRuns;

			public void RunTask(CancellationTokenSource cts, int cancelOnCount)
			{
				this.cts = cts;
				this.cancelOnCount = cancelOnCount;
				base.RunTask(cts.Token);
			}

			protected override void ReleaseShipmentAndAssociatedSplitShipments(UPECusHAWB finalisedCusHAWB, BusinessObjectFactory factory)
			{
				noOfRuns++;
				if (cts != null && noOfRuns == cancelOnCount)
				{
					cts.Cancel();
				}

				base.ReleaseShipmentAndAssociatedSplitShipments(finalisedCusHAWB, factory);
			}

			public new ZQuery ConstructFilter(ZDateTime lowerBound, ZDateTime upperBound)
			{
				return base.ConstructFilter(lowerBound, upperBound);
			}
		}
	}
}
