using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECalloutQueue))]
	public class UPECalloutQueueTest : UPECargoReportQueueBizoTest
	{
		public void TestRaiseEIRProcessingTurnedOn()
		{
			Callout callout = Factory.New<Callout>();
			callout.CS_CM = Factory.New<UPECusMAWB>().PK;
			callout.CurrentQueue.P4_CustomsQueue = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			callout.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.AEIR.Codes.FF_RTSAuthorisationRequired;
			callout.CurrentQueue.P4_CustomsSubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			Factory.Save();
			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			Callout calloutReLoaded = factoryForTest.Load<Callout>(callout.PK);
			try
			{
				calloutReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnAskHasEIRBeenRaised);
				calloutReLoaded.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.AR;
				calloutReLoaded.CurrentQueue.P4_Reason = "Test1";
				calloutReLoaded.CS_GoodsDescription = "TEST1";
				factoryForTest.Save();
				AssertEquals("EIR has not been handled.", false, calloutReLoaded.Logs.AutoCreatedLog.SL_Reference.Contains("EIR"));
				calloutReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnAskHasEIRBeenRaised);
				factoryForTest = new BusinessObjectFactory();
				calloutReLoaded = factoryForTest.Load<Callout>(callout.PK);
				calloutReLoaded.CurrentQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnAskHasEIRBeenRaised);
				calloutReLoaded.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.EIR;
				calloutReLoaded.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.CEIR.Codes.BA_InadequateDescription;
				calloutReLoaded.CurrentQueue.P4_SubStatus = StatusCodeDescriptionPairList.EmptyStatus;
				calloutReLoaded.CurrentQueue.P4_Reason = "Test2";
				calloutReLoaded.CS_GoodsDescription = "TEST2";
				factoryForTest.Save();
				AssertEquals("EIR has been handled.", true, calloutReLoaded.Logs.AutoCreatedLog.SL_Reference.Contains("EIR"));
			}
			finally
			{
				calloutReLoaded.CurrentQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnAskHasEIRBeenRaised);
			}
		}

		public void TestAccountNumberAccountClass()
		{
			UPEOrgHeader uPEOrgHeader = Factory.New<UPEOrgHeader>();
			uPEOrgHeader.CompanyData.OB_OJ_ARDebtorGroup = Factory.New(typeof(OrgDebtorGroup)).PK;
			uPEOrgHeader.CompanyData.ARDebtorGroup.OJ_Code = "2";
			uPEOrgHeader.AccountNumber = "TEST";
			Queue.Parent = Factory.New<Callout>();
			Queue.P4_CustomAttrib8 = "TEST";
			AssertEquals("2", ((UPECalloutQueue)Queue).AccountNumberAccountClass);
		}

		protected override Type ExpectedValidationType
		{
			get
			{
				return typeof(UPECalloutQueueValidation);
			}
		}
	}
}
