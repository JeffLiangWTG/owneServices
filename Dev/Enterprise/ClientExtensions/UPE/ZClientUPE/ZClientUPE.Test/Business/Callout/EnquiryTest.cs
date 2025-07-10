using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(Enquiry))]
	public class EnquiryTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.UPECusHAWB);
			}
		}

		public void TestForceIntoFinanceQueue()
		{
			EnquiryForTest.CurrentQueue.P4_QueueName = "";
			EnquiryForTest.ForceIntoFinanceQueue();
			AssertEquals(CommercialQueueCodeDescriptionPairList.Codes.Finance, EnquiryForTest.CurrentQueue.P4_QueueName);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, EnquiryForTest.CurrentQueue.P4_Status);
			AssertEquals("After forced to finance, it should be excluded from the warning report", true, EnquiryForTest.IsExcludedFromBISIWarning);
		}

		public void TestForceIntoFinanceQueue_LogReference()
		{
			EnquiryForTest.Factory.Save();
			EnquiryForTest.CS_ChargableWeight = 5;
			EnquiryForTest.CurrentQueue.P4_QueueName = "";
			EnquiryForTest.ForceIntoFinanceQueue();
			Factory.Save();
			AssertEquals("Excluded from BISI Warning Report; Forced to Finance", EnquiryForTest.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestOnCreateAutoAdminLogNRE()
		{
			EnquiryForTest.Factory.Save();
			EnquiryForTest.CS_ChargableWeight = 5;
			EnquiryForTest.CurrentQueue.P4_QueueName = "";
			EnquiryForTest.ForceIntoFinanceQueue();
			var edtAmount = EnquiryForTest.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count();
			//add another EDT log to try and cause NRE
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			EnquiryForTest.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.EditedARecord, "Foo Bar Baz");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			AssertEquals("Excluded from BISI Warning Report; Forced to Finance", EnquiryForTest.Logs.AutoCreatedLog.SL_Reference);
			AssertEquals(edtAmount + 2, EnquiryForTest.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
		}

		public void TestTypeOfProcessQueue()
		{
			AssertEquals("Should use cargo report queue instead of Finance because Finance requires a Finance queue which is not always available", typeof(UPECargoReportQueue), EnquiryForTest.TypeOfProcessQueue);
		}

		public void TestFinanceQueueNameNotRequired()
		{
			EnquiryForTest.CurrentQueue.P4_QueueName = "";
			EnquiryForTest.RunPreSaveValidation();
			AssertEquals("Finance queue name should not be mandatory when using Enquiry", false, EnquiryForTest.CurrentQueue.P4_QueueNameInfo.HasErrors());
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			Enquiry enquiry = (Enquiry)cusMAWB.ChildBills.AddNew(typeof(Enquiry));
			cusMAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			return enquiry;
		}

		TestEnquiry EnquiryForTest
		{
			get
			{
				if (fEnquiryForTest == null)
				{
					CusMAWB mAWB = Factory.New<CusMAWB>();
					fEnquiryForTest = (TestEnquiry)mAWB.ChildBills.AddNew(typeof(TestEnquiry));
				}

				return fEnquiryForTest;
			}
		}

		TestEnquiry fEnquiryForTest;
		#endregion
		#region Test Classes
		class TestEnquiry : Enquiry
		{
			public TestEnquiry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Type TypeOfProcessQueue
			{
				get
				{
					return base.TypeOfProcessQueue;
				}
			}
		}
		#endregion
	}
}
