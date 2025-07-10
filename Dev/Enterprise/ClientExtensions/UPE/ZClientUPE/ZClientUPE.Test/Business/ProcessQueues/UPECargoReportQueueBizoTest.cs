using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECargoReportQueue))]
	public class UPECargoReportQueueBizoTest : UPEProcessQueueTestCase
	{
		public override void TestReferenceCode()
		{
			Assert("COM Reference code", Queue.ReferenceCode == "COM");
		}

		public void TestParentBranch()
		{
			AssertNull("CusHAWB", Queue.ParentBusinessObject);
			UPECusHAWB.CS_CM = ZGuid.Empty;
			AssertNull("CusMAWB", UPECusHAWB.MAWB);
			Queue.Parent = UPECusHAWB;
			AssertNull("Parent Branch", Queue.ParentBranch);
			UPECusHAWB.CS_CM = UPECusMAWB.PK;
			AssertEquals("Parent Branch", UPECusMAWB.Branch.PK, Queue.ParentBranch.PK);
		}

		public void TestUPECusHAWB()
		{
			Queue.Parent = UPECusHAWB;
			AssertEquals(UPECusHAWB, Queue.FirstUPECusHAWB);
		}

		public void TestResolutionCode_CustomsQueueLogsNotLoaded()
		{
			AddCommercialQueueLogDirectlyToFactory(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty, string.Empty, string.Empty);
			AddCustomsProcessQueueLogDirectlyToFactory("CPL", ResolutionCodeDescriptionPairList.Codes.DA_Released, "RR", "SS", "123");
			AddCustomsProcessQueueLogDirectlyToFactory("BLA", ResolutionCodeDescriptionPairList.Codes.DA_Released, "BB", "D", "T");
			AddCustomsProcessQueueLogDirectlyToFactory("TST", ResolutionCodeDescriptionPairList.Codes.DA_Released, "LL", "B", "T");
			AddCustomsProcessQueueLogDirectlyToFactory(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty, string.Empty, "T");
			AddCustomsProcessQueueLogDirectlyToFactory("S", ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty, string.Empty, string.Empty);
			Factory.Save();
			AssertEquals("Sanity check", false, Queue.IsCustomsQueueLogsLoaded);
			AssertEquals(ZString.Empty, Queue.ResolutionCode);
			AssertEquals("Should not be loading CustomsQueueLogs if it has not been loaded", false, Queue.IsCustomsQueueLogsLoaded);
			ProcessQueueLog logWithResolutionCode = AddCustomsProcessQueueLogDirectlyToFactory(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty, string.Empty, string.Empty);
			Factory.Save();
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.DA_Released, Queue.ResolutionCode);
			AssertEquals("Should not be loading CustomsQueueLogs if it has not been loaded", false, Queue.IsCustomsQueueLogsLoaded);
			Queue.ResetResolutionCodeForTest();
			Factory.Save();
			TestCaseHelper.ClearTable(StmALogSchema.Constants.TableName);
			BusinessObjectFactory newFactory1 = new BusinessObjectFactory();
			UPECargoReportQueue newQueue1 = newFactory1.Load<UPECargoReportQueue>(Queue.PK);
			logWithResolutionCode = newFactory1.New<UPEProcessQueueLog>();
			logWithResolutionCode.SL_Parent = Queue.PK;
			logWithResolutionCode.SL_Table = ProcessQueueSchema.Constants.TableName;
			logWithResolutionCode.SetQueueDetails(ProcessQueueType.Enum.Customs, string.Empty, ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker, string.Empty, string.Empty, string.Empty);
			newFactory1.Save();
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker, newQueue1.ResolutionCode);
			AssertEquals("Should not be loading CustomsQueueLogs if it has not been loaded", false, newQueue1.IsCustomsQueueLogsLoaded);
			newQueue1.ResetResolutionCodeForTest();
			newFactory1.Save();
			TestCaseHelper.ClearTable(StmALogSchema.Constants.TableName);
			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			UPECargoReportQueue newQueue2 = newFactory2.Load<UPECargoReportQueue>(Queue.PK);
			logWithResolutionCode = newFactory2.New<UPEProcessQueueLog>();
			logWithResolutionCode.SL_Parent = Queue.PK;
			logWithResolutionCode.SL_Table = ProcessQueueSchema.Constants.TableName;
			logWithResolutionCode.SetQueueDetails(ProcessQueueType.Enum.Customs, string.Empty, ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms, string.Empty, string.Empty, string.Empty);
			newFactory2.Save();
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms, newQueue2.ResolutionCode);
			AssertEquals("Should not be loading CustomsQueueLogs if it has not been loaded", false, newQueue2.IsCustomsQueueLogsLoaded);
			newQueue2.ResetResolutionCodeForTest();
			newFactory2.Save();
			TestCaseHelper.ClearTable(StmALogSchema.Constants.TableName);
			BusinessObjectFactory newFactory3 = new BusinessObjectFactory();
			UPECargoReportQueue newQueue3 = newFactory3.Load<UPECargoReportQueue>(Queue.PK);
			logWithResolutionCode = newFactory3.New<UPEProcessQueueLog>();
			logWithResolutionCode.SL_Parent = Queue.PK;
			logWithResolutionCode.SL_Table = ProcessQueueSchema.Constants.TableName;
			logWithResolutionCode.SetQueueDetails(ProcessQueueType.Enum.Customs, string.Empty, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, string.Empty, string.Empty, string.Empty);
			newFactory3.Save();
			AssertEquals(ZString.Empty, newQueue3.ResolutionCode);
			AssertEquals("Should not be loading CustomsQueueLogs if it has not been loaded", false, newQueue3.IsCustomsQueueLogsLoaded);
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestResolutionCode_CustomsQueueLogsLoaded()
		{
			AddCustomsProcessQueueLog("CPL", ResolutionCodeDescriptionPairList.Codes.DA_Released, "RR", "SS", "123");
			AddCustomsProcessQueueLog("BLA", ResolutionCodeDescriptionPairList.Codes.DA_Released, "BB", "D", "T");
			AddCustomsProcessQueueLog("TST", ResolutionCodeDescriptionPairList.Codes.DA_Released, "LL", "B", "T");
			AddCustomsProcessQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, "bc", "TT", "T");
			AddCommercialQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty, string.Empty, string.Empty);
			AssertEquals("Sanity check", true, Queue.IsCustomsQueueLogsLoaded);
			int loadCountBefore = Factory.DatabaseLoadCount;
			AssertEquals(ZString.Empty, Queue.ResolutionCode);
			AssertEquals("Should not be loading from database if CustomsQueueLogs have been loaded", loadCountBefore, Factory.DatabaseLoadCount);
			loadCountBefore = Factory.DatabaseLoadCount;
			ProcessQueueLog logWithResolutionCode = AddCustomsProcessQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty, string.Empty, string.Empty);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.DA_Released, Queue.ResolutionCode);
			AssertEquals("Should not be loading from database if CustomsQueueLogs have been loaded", loadCountBefore, Factory.DatabaseLoadCount);
			loadCountBefore = Factory.DatabaseLoadCount;
			Queue.ResetResolutionCodeForTest();
			logWithResolutionCode = AddCustomsProcessQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker, string.Empty, string.Empty, string.Empty);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker, Queue.ResolutionCode);
			AssertEquals("Should not be loading from database if CustomsQueueLogs have been loaded", loadCountBefore, Factory.DatabaseLoadCount);
			loadCountBefore = Factory.DatabaseLoadCount;
			Queue.ResetResolutionCodeForTest();
			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			logWithResolutionCode = AddCustomsProcessQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms, string.Empty, string.Empty, string.Empty);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms, Queue.ResolutionCode);
			AssertEquals("Should not be loading from database if CustomsQueueLogs have been loaded", loadCountBefore, Factory.DatabaseLoadCount);
			loadCountBefore = Factory.DatabaseLoadCount;
			Queue.ResetResolutionCodeForTest();
			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			AddCustomsProcessQueueLog(string.Empty, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, string.Empty, string.Empty, string.Empty);
			AssertEquals(ZString.Empty, Queue.ResolutionCode);
			AssertEquals("Should not be loading from database if CustomsQueueLogs have been loaded", loadCountBefore, Factory.DatabaseLoadCount);
		}

		public void TestResolutionUploadDateTime()
		{
			AssertEquals(ZDateTime.Empty, Queue.ResolutionUploadDateTime);
			ProcessQueueLog logWithResolutionCode = AddCustomsProcessQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, string.Empty, string.Empty, string.Empty, new ZDateTime(2004, 11, 1));
			AssertEquals(new ZDateTime(2004, 11, 1), Queue.ResolutionUploadDateTime);
			Queue.ResetResolutionCodeForTest();
			logWithResolutionCode = AddCustomsProcessQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker, string.Empty, string.Empty, string.Empty, new ZDateTime(2004, 11, 2));
			AssertEquals(new ZDateTime(2004, 11, 2), Queue.ResolutionUploadDateTime);
			Queue.ResetResolutionCodeForTest();
			logWithResolutionCode = AddCustomsProcessQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.BZ_Abandoned, string.Empty, string.Empty, string.Empty, new ZDateTime(2004, 11, 3));
			AssertEquals(new ZDateTime(2004, 11, 3), Queue.ResolutionUploadDateTime);
			Queue.ResetResolutionCodeForTest();
			logWithResolutionCode = AddCustomsProcessQueueLog(string.Empty, ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker, string.Empty, string.Empty, string.Empty, new ZDateTime(2004, 11, 4));
			AssertEquals(new ZDateTime(2004, 11, 4), Queue.ResolutionUploadDateTime);
			Queue.ResetResolutionCodeForTest();
			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			AddCustomsProcessQueueLog(string.Empty, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, string.Empty, string.Empty, string.Empty, new ZDateTime(2004, 11, 8));
			AssertEquals(ZDateTime.Empty, Queue.ResolutionUploadDateTime);
		}

		public void TestAddResolutionCodeLog()
		{
			Queue.Parent = UPECusHAWB;
			Queue.P4_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
			EDIMessage message = Queue.FirstUPECusHAWB.Messages.AddNew();
			message.EM_MessageType = UPECalloutQueue.EdiMessageTypes.Withdrawn;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Queue.AddResolutionCodeLog();
			AssertEquals("Default release code is DA", ZString.Empty, Queue.ResolutionCode);
			message.EM_MessageType = ZString.Empty;
			Queue.AddResolutionCodeLog();
			AssertEquals("witheld shipment therefore set to empty", ResolutionCodeDescriptionPairList.Codes.DA_Released, Queue.ResolutionCode);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes._R1_Abandon;
			Queue.AddResolutionCodeLog();
			AssertEquals("Should create a new log even if a resolution log already exist", ResolutionCodeDescriptionPairList.Codes.BZ_Abandoned, Queue.ResolutionCode);
			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			Queue.ResetResolutionCodeForTest();
			Queue.AddResolutionCodeLog();
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.BZ_Abandoned, Queue.ResolutionCode);
			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			Queue.ResetResolutionCodeForTest();
			Queue.P4_QueueName = string.Empty;
			Queue.Parent = UPECusHAWB;
			Queue.FirstUPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			Queue.FirstUPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding;
			Queue.AddResolutionCodeLog();
			AssertEquals("Sanity check", true, Queue.FirstUPECusHAWB.Declaration.IsInCustomsBondingQueue);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms, Queue.ResolutionCode);
			Queue.CustomsQueueLogs.RemoveAndDeleteAll();
			Queue.ResetResolutionCodeForTest();
			Queue.FirstUPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = string.Empty;
			Queue.FirstUPECusHAWB.Declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			Queue.FirstUPECusHAWB.Declaration.Importer.SetRelatedParty(Factory.New<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			Queue.AddResolutionCodeLog();
			AssertEquals("Sanity check", true, Queue.FirstUPECusHAWB.Declaration.HasAlternateBroker);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker, Queue.ResolutionCode);
		}

		public void TestResolutionCode_AddedAndLoadedCorrectly()
		{
			NotificationBuffer notification = new NotificationBuffer();
			Queue.AddResolutionCodeLog();
			Queue.ResetResolutionCodeForTest();
			AssertEquals("Not yet in the database, should be empty", ZString.Empty, Queue.ResolutionCode);
			Factory.Save();
			AssertEquals("Pre-condition", false, Queue.IsCustomsQueueLogsLoaded);
			object lazyLoadCustomsQueueLogs = Queue.CustomsQueueLogs;
			int loadCountBefore = Factory.DatabaseLoadCount;
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.DA_Released, Queue.ResolutionCode);
			AssertEquals("Should not be loading from database if CustomsQueueLogs have been loaded", loadCountBefore, Factory.DatabaseLoadCount);
			Queue.ResetResolutionCodeForTest();
			Factory.Save();
			TestCaseHelper.ClearTable(StmALogSchema.Constants.TableName);
			BusinessObjectFactory newFactory1 = new BusinessObjectFactory();
			UPECargoReportQueue newQueue1 = newFactory1.Load<UPECargoReportQueue>(Queue.PK);
			AssertEquals(ZString.Empty, newQueue1.ResolutionCode);
			AssertEquals(0, newQueue1.CustomsQueueLogs.Count);
			newQueue1.AddResolutionCodeLog();
			AssertEquals("There should be a resolution code log in the CustomsQueueLogs", 1, newQueue1.CustomsQueueLogs.Count);
			newFactory1.Save();
			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			UPECargoReportQueue newQueue2 = newFactory2.Load<UPECargoReportQueue>(Queue.PK);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.DA_Released, newQueue2.ResolutionCode);
		}

		public void TestCustomsAndCommercialTaskAssignedTosReadOnly()
		{
			AssertEquals(true, Queue.P4_GS_NKCustomsTaskAssignedToInfo.ReadOnly);
			AssertEquals(true, Queue.P4_GS_NKTaskAssignedToInfo.ReadOnly);
		}

		public void TestP4_Status()
		{
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals(Queue.P4_CustomDecimal1, 4M);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes._R1_Abandon;
			AssertEquals(Queue.P4_CustomDecimal1, 3M);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes._R2_Transhipment;
			AssertEquals(Queue.P4_CustomDecimal1, 0M);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes._R3_RTS;
			AssertEquals(Queue.P4_CustomDecimal1, 4M);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes._R1_Abandon;
			AssertEquals(Queue.P4_CustomDecimal1, 3M);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Queue.P4_Status = "";
			AssertEquals(Queue.P4_CustomDecimal1, 0M);
		}

		#region Implementation
		protected override Type ExpectedParentBusinessObjectType
		{
			get
			{
				return typeof(UPECusHAWB);
			}
		}

		protected override Type ExpectedLookupsType
		{
			get
			{
				return typeof(UPECargoReportQueueLookups);
			}
		}

		protected override Type ExpectedValidationType
		{
			get
			{
				return typeof(UPECargoReportQueueValidation);
			}
		}

		protected new UPECargoReportQueue Queue
		{
			get
			{
				return (UPECargoReportQueue)base.Queue;
			}
		}

		UPEProcessQueueLog AddCustomsProcessQueueLogDirectlyToFactory(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			return AddProcessQueueLogDirectlyToFactory(ProcessQueueType.Enum.Customs, queueName, status, subStatus, reason, assignedTo);
		}

		UPEProcessQueueLog AddCustomsProcessQueueLog(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo, ZDateTime eventTime)
		{
			UPEProcessQueueLog result = AddCustomsProcessQueueLog(queueName, status, subStatus, reason, assignedTo);
			result.SL_EventTime = eventTime;
			return result;
		}

		UPEProcessQueueLog AddCustomsProcessQueueLog(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			return (UPEProcessQueueLog)Queue.CustomsQueueLogs.AddNew(queueName, status, subStatus, reason, assignedTo);
		}

		UPEProcessQueueLog AddCommercialQueueLogDirectlyToFactory(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			return AddProcessQueueLogDirectlyToFactory(ProcessQueueType.Enum.Commercial, queueName, status, subStatus, reason, assignedTo);
		}

		UPEProcessQueueLog AddProcessQueueLogDirectlyToFactory(ProcessQueueType.Enum queueType, ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			UPEProcessQueueLog result = Factory.New<UPEProcessQueueLog>();
			result.SL_Parent = Queue.PK;
			result.SL_Table = ProcessQueueSchema.Constants.TableName;
			result.SetQueueDetails(queueType, queueName, status, subStatus, reason, assignedTo);
			return result;
		}

		UPEProcessQueueLog AddCommercialQueueLog(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			return (UPEProcessQueueLog)Queue.CommercialQueueLogs.AddNew(queueName, status, subStatus, reason, assignedTo);
		}

		UPECusHAWB UPECusHAWB
		{
			get
			{
				if (fUPECusHAWB == null)
				{
					fUPECusHAWB = (UPECusHAWB)UPECusMAWB.ChildBills.AddNew();
					fUPECusHAWB.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
				}

				return fUPECusHAWB;
			}
		}

		UPECusMAWB UPECusMAWB
		{
			get
			{
				if (fUPECusMAWB == null)
				{
					fUPECusMAWB = Factory.New<UPECusMAWB>();
				}

				return fUPECusMAWB;
			}
		}

		UPECusHAWB fUPECusHAWB;
		UPECusMAWB fUPECusMAWB;
		#endregion
	}
}
