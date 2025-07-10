using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	public abstract class UPEProcessQueueTestCase : EnterpriseBusinessObjectTestCase
	{
		protected abstract Type ExpectedParentBusinessObjectType { get; }

		protected abstract Type ExpectedLookupsType { get; }

		protected abstract Type ExpectedValidationType { get; }

		public void TestIsInspectIndicatorAndNewLog()
		{
			TestCusHAWB hawb = Factory.NewWithValidTestData<TestCusHAWB>();
			hawb.CS_JE_CustomsFormalEntry = Factory.New<UPEJobDeclaration>().PK;
			UPEProcessQueue queue = hawb.CurrentQueue;
			queue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Intervention;
			queue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease;
			Factory.Save();
			AssertEquals(false, queue.IsInspectIndicator);
			queue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Quarantine;
			queue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold;
			Factory.Save();
			AssertEquals(true, queue.IsInspectIndicator);
			AssertEquals(true, queue.CustomsQueueLogs.ContainsQueue(ZString.Empty, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, ZString.Empty));
			queue = hawb.Declaration.CurrentQueue;
			queue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.BCA;
			queue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker;
			Factory.Save();
			AssertEquals(false, Queue.IsInspectIndicator);
			queue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold;
			Factory.Save();
			AssertEquals(true, queue.IsInspectIndicator);
			AssertEquals(true, queue.CustomsQueueLogs.ContainsQueue(ZString.Empty, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, ZString.Empty));
		}

		public void TestSetDefaultForCustomsStatusAndCustomsSubStatus()
		{
			if (Queue.P4_CustomsQueue == CargoReportQueueCodeDescriptionPairList.Codes.Quarantine)
			{
				Queue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Pending;
				Factory.Save();
			}

			Queue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Quarantine;
			Factory.Save();
			AssertEquals("it should default to DI", ReasonCodeDescriptionPairList.AQUA.Codes.DI_DocInspect, Queue.P4_CustomsStatus);
		}

		public void TestEnsureGetFinalisedCusHAWBsMethodDoesNotReturnNull()
		{
			AssertNotNull("Should not return null, return an empty array instead", Queue.GetFinalisedCusHAWBs());
		}

		#region Business Object Overrides
		public void TestLookups()
		{
			AssertEquals(ExpectedLookupsType, Queue.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(ExpectedValidationType, Queue.Validation.GetType());
		}

		public void TestCusHAWB_OnCusHAWBOrDeclarationProcessQueueSaving_CalledOnSaving()
		{
			TestCusHAWB cusHAWB = Factory.NewWithValidTestData<TestCusHAWB>();
			cusHAWB.CurrentQueue.P4_CustomAttrib1 = "changed";
			Factory.Save();
			AssertEquals("Should have called OnCusHAWBOrDeclarationProcessQueueSaving after a change was made", true, cusHAWB.OnCusHAWBOrDeclarationProcessQueueSavingCalled);
			cusHAWB.OnCusHAWBOrDeclarationProcessQueueSavingCalled = false;
			cusHAWB.CS_ConsigneeCity = "changed";
			Factory.Save();
			AssertEquals("Should not have called OnCusHAWBOrDeclarationProcessQueueSaving if UPEProcessQueue was not changed", false, cusHAWB.OnCusHAWBOrDeclarationProcessQueueSavingCalled);
		}

		[ExpectNoExceptions]
		public void TestSaveConcurrencyException()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Factory.RefreshEnabled = false;
			newFactory.RefreshEnabled = false;
			UPEProcessQueue queue = (UPEProcessQueue)Factory.New(GetExpectedBusinessObjectType());
			Factory.Save();
			UPEProcessQueue newQueue = (UPEProcessQueue)Factory.Load(GetExpectedBusinessObjectType(), queue.PK);
			queue.P4_CustomAttrib1 = "TEst";
			newQueue.P4_CustomAttrib2 = "BLAH";
			Factory.Save();
			newFactory.Save();
		}

		#endregion
		#region Queued Dates
		public void TestCustomsQueuedDate()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 1, 16);
			Queue.P4_CustomDate6 = expectedDate;
			AssertEquals(expectedDate, Queue.CustomsQueuedDate);
			ZDateTime newExpectedDate = new ZDateTime(2005, 2, 16);
			Queue.CustomsQueuedDate = newExpectedDate;
			AssertEquals(newExpectedDate, Queue.P4_CustomDate6);
			AssertEquals("Inner Info has to be " + Queue.P4_CustomDate6Info.Name, Queue.P4_CustomDate6Info, ((ZWrappedPropertyInfo)Queue.CustomsQueuedDateInfo).InnerInfo);
			AssertEquals("Has to be read-only", true, Queue.CustomsQueuedDateInfo.ReadOnly);
		}

		public void TestCommercialQueuedByUser()
		{
			ZString expectedUser = "ME";
			Queue.P4_CustomAttrib5 = expectedUser;
			AssertEquals(expectedUser, Queue.CommercialQueuedByUser);
			ZString newExpectedUser = "YOU";
			Queue.CommercialQueuedByUser = newExpectedUser;
			AssertEquals(newExpectedUser, Queue.P4_CustomAttrib5);
			AssertEquals("Inner Info has to be " + Queue.P4_CustomAttrib5Info.Name, Queue.P4_CustomAttrib5Info, ((ZWrappedPropertyInfo)Queue.CommercialQueuedByUserInfo).InnerInfo);
			AssertEquals("Has to be read-only", true, Queue.CommercialQueuedByUserInfo.ReadOnly);
		}

		public void TestCommercialQueuedDate()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 1, 16);
			Queue.P4_CustomDate7 = expectedDate;
			AssertEquals(expectedDate, Queue.CommercialQueuedDate);
			ZDateTime newExpectedDate = new ZDateTime(2005, 2, 17);
			Queue.CommercialQueuedDate = newExpectedDate;
			AssertEquals(newExpectedDate, Queue.P4_CustomDate7);
			AssertEquals("Inner Info has to be " + Queue.P4_CustomDate7Info.Name, Queue.P4_CustomDate7Info, ((ZWrappedPropertyInfo)Queue.CommercialQueuedDateInfo).InnerInfo);
			AssertEquals("Has to be read-only", true, Queue.CommercialQueuedDateInfo.ReadOnly);
		}

		[TestDate(2000, 1, 1)]
		public void TestCommercialReleasedDate()
		{
			TestDateAttribute.Date = new DateTime(2005, 1, 1);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
			Factory.Save();
			AssertEquals("When the commercial queue is not yet released, it should not be marked with a date", true, Queue.CommercialReleasedDate.IsEmpty);
			TestDateAttribute.Date = new DateTime(2005, 2, 2);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Rebill;
			Factory.Save();
			AssertEquals("When the commercial queue is moved to Rebill", new ZDateTime(2005, 2, 2), Queue.CommercialReleasedDate);
			TestDateAttribute.Date = new DateTime(2005, 3, 3);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();
			AssertEquals("When the commercial queue is moved from Rebill to Completed, date should not change", new ZDateTime(2005, 2, 2), Queue.CommercialReleasedDate);
			TestDateAttribute.Date = new DateTime(2005, 3, 3);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.AR;
			Factory.Save();
			AssertEquals("When the commercial queue is moved from Completed to something else, ReleasedDate should be empty", true, Queue.CommercialReleasedDate.IsEmpty);
		}

		#endregion
		#region Maintaining Queued Dates
		[TestDate(2005, 11, 1)]
		public void TestQueuedDateAndUserSetOnSaving()
		{
			AssertEquals("Pre-condition", ZDateTime.Empty, Queue.CustomsQueuedDate);
			AssertEquals("Pre-condition", ZDateTime.Empty, Queue.CommercialQueuedDate);
			AssertEquals("Pre-condition", ZString.Empty, Queue.CommercialQueuedByUser);
			Factory.Save();
			AssertEquals("No queue name or statuses change, should still be empty", ZDateTime.Empty, Queue.CustomsQueuedDate);
			AssertEquals("No queue name or statuses change, should still be empty", ZDateTime.Empty, Queue.CommercialQueuedDate);
			AssertEquals("No queue name or statuses change, should still be empty", ZString.Empty, Queue.CommercialQueuedByUser);
			Queue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Hold;
			Factory.Save();
			AssertEquals(new ZDateTime(2005, 11, 1), Queue.CustomsQueuedDate);
			AssertEquals("No commercial queue name or statuses change, should still be empty", ZDateTime.Empty, Queue.CommercialQueuedDate);
			AssertEquals("No commercial queue name or statuses change, should still be empty", ZString.Empty, Queue.CommercialQueuedByUser);
			ZDateTime testDate1 = new ZDateTime(2005, 11, 2);
			TestDateAttribute.Date = testDate1.ToDateTime();
			Queue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice;
			Queue.P4_Reason = "Test 12345";
			Factory.Save();
			AssertEquals(testDate1, Queue.CustomsQueuedDate);
			AssertEquals("Should only set QueuedDate if QueueName, Status, or SubStatus has changes", ZDateTime.Empty, Queue.CommercialQueuedDate);
			AssertEquals("Should only set QueuedDate if QueueName, Status, or SubStatus has changes", ZString.Empty, Queue.CommercialQueuedByUser);
			ZDateTime testDate2 = new ZDateTime(2005, 11, 3);
			TestDateAttribute.Date = testDate2.ToDateTime();
			Queue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			Queue.P4_GS_NKTaskAssignedTo = "TES";
			Factory.Save();
			AssertEquals(testDate2, Queue.CustomsQueuedDate);
			AssertEquals("Should only set QueuedDate if QueueName, Status, or SubStatus has changes", ZDateTime.Empty, Queue.CommercialQueuedDate);
			AssertEquals("Should only set QueuedDate if QueueName, Status, or SubStatus has changes", ZString.Empty, Queue.CommercialQueuedByUser);
			ZDateTime testDate3 = new ZDateTime(2005, 11, 4);
			TestDateAttribute.Date = testDate3.ToDateTime();
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Chase;
			Factory.Save();
			AssertEquals("No new customs name or statuses change", testDate2, Queue.CustomsQueuedDate);
			AssertEquals(testDate3, Queue.CommercialQueuedDate);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Queue.CommercialQueuedByUser);
			ZDateTime testDate4 = new ZDateTime(2005, 11, 5);
			TestDateAttribute.Date = testDate4.ToDateTime();
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;
			Queue.P4_CustomsReason = "BLABLABLA";
			Factory.Save();
			AssertEquals("Should only set QueuedDate if QueueName, Status, or SubStatus has changes", testDate2, Queue.CustomsQueuedDate);
			AssertEquals(testDate4, Queue.CommercialQueuedDate);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Queue.CommercialQueuedByUser);
			ZDateTime testDate5 = new ZDateTime(2005, 11, 6);
			TestDateAttribute.Date = testDate5.ToDateTime();
			Queue.P4_SubStatus = StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber;
			Queue.P4_GS_NKCustomsTaskAssignedTo = "HAH";
			Factory.Save();
			AssertEquals("Should only set QueuedDate if QueueName, Status, or SubStatus has changes", testDate2, Queue.CustomsQueuedDate);
			AssertEquals(testDate5, Queue.CommercialQueuedDate);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Queue.CommercialQueuedByUser);
		}

		[TestDate(2005, 11, 1)]
		public void TestQueuedDateShouldBeSetEvenIfQueueIsNotInDatabase()
		{
			Queue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Hold;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;
			Factory.Save();
			ZDateTime expectedDate = new ZDateTime(2005, 11, 1);
			AssertEquals(expectedDate, Queue.CustomsQueuedDate);
			AssertEquals(expectedDate, Queue.CommercialQueuedDate);
		}

		#endregion
		#region ParentBusinessObject
		public void TestParentBusinessObject_ParentIsNull()
		{
			IProcessQueueParent parent = (IProcessQueueParent)Factory.New(ExpectedParentBusinessObjectType);
			Queue.P4_ParentTableCode = parent.TablePrefix;
			Queue.P4_ParentID = parent.PK;
			AssertNull("Sanity check, Parent is null", Queue.Parent);
			AssertEquals("Should not be null and type should be as expected", ExpectedParentBusinessObjectType, Queue.ParentBusinessObject.GetType());
		}

		[ExpectNoExceptions]
		public void TestParentBusinessObject_ParentIsNullAndParentIDNotSpecified()
		{
			object notUsed = Queue.ParentBusinessObject;
		}

		public void TestParentBusinessObject_ParentIsAssigned()
		{
			UPEJobDeclaration dec = Factory.New<UPEJobDeclaration>();
			Queue.Parent = dec;
			AssertEquals("Should be the same object reference", dec, Queue.ParentBusinessObject);
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			Queue.Parent = uPECusHAWB;
			AssertEquals("Should be the same object reference", uPECusHAWB, Queue.ParentBusinessObject);
		}

		#endregion
		#region Queue Summary
		public void TestIsCustomsQueueCompleted()
		{
			CustomsQueueCodeDescriptionPairList pairList = new CustomsQueueCodeDescriptionPairList();
			foreach (CodeDescriptionPair pair in pairList)
			{
				bool expected = ((IList)CustomsQueueCodeDescriptionPairList.CompletedQueueNames).Contains(pair.Code);
				Queue.P4_CustomsQueue = pair.Code;
				AssertEquals(expected, Queue.IsCustomsQueueCompleted);
			}
		}

		public void TestHasCustomsQueueBeenCompleted()
		{
			Queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.CompletedQueueNames[0];
			AssertEquals("Not in database", false, Queue.HasCustomsQueueBeenCompleted);
			Factory.Save();
			AssertEquals(true, Queue.HasCustomsQueueBeenCompleted);
			Queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.Hold;
			AssertEquals("Should be looking at the original value in DB", true, Queue.HasCustomsQueueBeenCompleted);
			Factory.Save();
			AssertEquals("Original value is held", false, Queue.HasCustomsQueueBeenCompleted);
			Queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.CompletedQueueNames[0];
			AssertEquals("Should be looking at the original value in DB", false, Queue.HasCustomsQueueBeenCompleted);
		}

		public void TestCustomsQueueHeldOrCompleted()
		{
			Queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.CompletedQueueNames[0];
			AssertEquals("Not in database", "HELD", Queue.CustomsQueueHeldOrCompleted);
			Factory.Save();
			AssertEquals("COMPLETED", Queue.CustomsQueueHeldOrCompleted);
			Queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.Unknown;
			AssertEquals("Should be looking at the original value in DB", "COMPLETED", Queue.CustomsQueueHeldOrCompleted);
			Factory.Save();
			AssertEquals("HELD", Queue.CustomsQueueHeldOrCompleted);
			Queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.CompletedQueueNames[0];
			AssertEquals("Should be looking at the original value in DB", "HELD", Queue.CustomsQueueHeldOrCompleted);
		}

		public void TestCustomsQueueSummary()
		{
			Queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.EIR;
			Queue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient;
			AssertEquals("Not in database, should be empty", ZString.Empty, Queue.CustomsQueueSummary);
			Factory.Save();
			string expected = Queue.P4_CustomsQueue + " / " + ReasonCodeDescriptionPairList.Descriptions.S1_ShipperConsigneeDetailsInsufficient;
			AssertEquals(expected, Queue.CustomsQueueSummary);
			Queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.Pending;
			Queue.P4_CustomsStatus = "";
			AssertEquals("Should be looking at the original values", expected, Queue.CustomsQueueSummary);
			Factory.Save();
			AssertEquals(CustomsQueueCodeDescriptionPairList.Codes.Pending, Queue.CustomsQueueSummary);
		}

		public void TestIsCommercialQueueCompleted()
		{
			CommercialQueueCodeDescriptionPairList pairList = new CommercialQueueCodeDescriptionPairList();
			foreach (CodeDescriptionPair pair in pairList)
			{
				bool expected = ((IList)CommercialQueueCodeDescriptionPairList.CompletedQueueNames).Contains(pair.Code);
				Queue.P4_QueueName = pair.Code;
				AssertEquals(expected, Queue.IsCommercialQueueCompleted);
			}
		}

		public void TestHasCommercialQueueBeenCompleted()
		{
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.CompletedQueueNames[0];
			AssertEquals("Not in database", false, Queue.HasCommercialQueueBeenCompleted);
			Factory.Save();
			AssertEquals(true, Queue.HasCommercialQueueBeenCompleted);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Hold;
			AssertEquals("Should be looking at the original value in DB", true, Queue.HasCommercialQueueBeenCompleted);
			Factory.Save();
			AssertEquals("Original value is held", false, Queue.HasCommercialQueueBeenCompleted);
			Queue.P4_CustomsQueue = CommercialQueueCodeDescriptionPairList.CompletedQueueNames[0];
			AssertEquals("Should be looking at the original value in DB", false, Queue.HasCommercialQueueBeenCompleted);
		}

		public void TestCommercialQueueHeldOrCompleted()
		{
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.CompletedQueueNames[0];
			AssertEquals("Not in database", "HELD", Queue.CommercialQueueHeldOrCompleted);
			Factory.Save();
			AssertEquals("COMPLETED", Queue.CommercialQueueHeldOrCompleted);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.EIR;
			AssertEquals("Should be looking at the original value in DB", "COMPLETED", Queue.CommercialQueueHeldOrCompleted);
			Factory.Save();
			AssertEquals("HELD", Queue.CommercialQueueHeldOrCompleted);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.CompletedQueueNames[0];
			AssertEquals("Should be looking at the original value in DB", "HELD", Queue.CommercialQueueHeldOrCompleted);
		}

		public void TestCommercialQueueSummary()
		{
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
			Queue.P4_Status = ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;
			AssertEquals("Not in database, should be empty", ZString.Empty, Queue.CommercialQueueSummary);
			Factory.Save();
			string expected = Queue.P4_QueueName + " / " + ReasonCodeDescriptionPairList.Descriptions.OQ_HeldForPayment;
			AssertEquals(expected, Queue.CommercialQueueSummary);
			Queue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			Queue.P4_Status = "";
			AssertEquals("Should be looking at the original values", expected, Queue.CommercialQueueSummary);
			Factory.Save();
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Completed, Queue.CommercialQueueSummary);
		}

		#endregion
		#region EIR Raised
		public void TestMovedToEIRQueue_NoAction()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = Factory.New<UPECusMAWB>().PK;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(uPECusHAWB.PK);
			EIRBeenRaisedAnswer = false;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnAskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
				uPECusHAWBReLoaded.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.FF_RTSAuthorisationRequired;
				uPECusHAWBReLoaded.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
				factoryForTest.Save();
				AssertEquals("", uPECusHAWBReLoaded.CurrentQueue.EIRRaisedLog);
				AssertEquals(ZDateTime.Empty, uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnAskHasEIRBeenRaised);
			}
		}

		public void TestOpenInEIRQueueAndSaveInEIRQueue_EIRNotRaisedSelected()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = Factory.New<UPECusMAWB>().PK;
			uPECusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			uPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.FF_RTSAuthorisationRequired;
			uPECusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(uPECusHAWB.PK);
			EIRBeenRaisedAnswer = false;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnAskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CurrentQueue.P4_Reason = "Test";
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals("EIR Not Raised", uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(ZDateTime.Empty, uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnAskHasEIRBeenRaised);
			}
		}

		[TestDate(2006, 5, 5)]
		public void TestOpenInEIRQueueAndSaveInEIRQueue_EIRRaisedSelected()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = Factory.New<UPECusMAWB>().PK;
			uPECusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			uPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.FF_RTSAuthorisationRequired;
			uPECusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(uPECusHAWB.PK);
			EIRBeenRaisedAnswer = true;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnAskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CurrentQueue.P4_Reason = "Test";
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals("EIR Raised", uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(new ZDateTime(2006, 5, 5), uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnAskHasEIRBeenRaised);
			}
		}

		public void TestOpenInEIRQueueAndSaveInAnotherQueue()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = Factory.New<UPECusMAWB>().PK;
			uPECusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			uPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.FF_RTSAuthorisationRequired;
			uPECusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(uPECusHAWB.PK);
			EIRBeenRaisedAnswer = true;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnAskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Intervention;
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals("", uPECusHAWBReLoaded.CurrentQueue.EIRRaisedLog);
				AssertEquals(ZDateTime.Empty, uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnAskHasEIRBeenRaised);
			}
		}

		[TestDate(2006, 5, 5)]
		public void TestOpenInEIRQueueAndMultipleSaves()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = Factory.New<UPECusMAWB>().PK;
			uPECusHAWB.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			uPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.FF_RTSAuthorisationRequired;
			uPECusHAWB.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			UPECusHAWB uPECusHAWBReLoaded = factoryForTest.Load<UPECusHAWB>(uPECusHAWB.PK);
			EIRBeenRaisedAnswer = false;
			try
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnAskHasEIRBeenRaised);
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST";
				factoryForTest.Save();
				AssertEquals(Events.EditedARecord.Code, uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_SE_NKEvent);
				AssertEquals("EIR Not Raised", uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(ZDateTime.Empty, uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
				EIRBeenRaisedAnswer = true;
				uPECusHAWBReLoaded.CS_GoodsDescription = "TEST1";
				factoryForTest.Save();
				AssertEquals("EIR Raised", uPECusHAWBReLoaded.Logs.AutoCreatedLog.SL_Reference);
				AssertEquals(new ZDateTime(2006, 5, 5), uPECusHAWBReLoaded.CurrentQueue.P4_CustomDate4);
			}
			finally
			{
				uPECusHAWBReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnAskHasEIRBeenRaised);
			}
		}

		protected void OnAskHasEIRBeenRaised(object sender, CancelEventArgs eventArgs)
		{
			eventArgs.Cancel = !EIRBeenRaisedAnswer;
		}

		protected bool EIRBeenRaisedAnswer;
		#endregion
		public abstract void TestReferenceCode();
		#region Test Classes
		class TestCusHAWB : UPECusHAWB
		{
			public TestCusHAWB(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool OnCusHAWBOrDeclarationProcessQueueSavingCalled;
			internal override void OnCusHAWBOrDeclarationProcessQueueSaving()
			{
				OnCusHAWBOrDeclarationProcessQueueSavingCalled = true;
			}
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected UPEProcessQueue Queue
		{
			get
			{
				if (fQueue == null)
				{
					fQueue = (UPEProcessQueue)Factory.New(GetExpectedBusinessObjectType());
				}

				return fQueue;
			}
		}

		UPEProcessQueue fQueue;
		#endregion
	}
}
