using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(DogHitXRay))]
	public class DogHitXRayTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRunPreSaveValidation()
		{
			DogHitXRay.RunPreSaveValidation();
			AssertEquals(true, DogHitXRay.TrackingNumberInfo.HasErrors());
			AssertEquals(false, DogHitXRay.MasterbillNumberInfo.HasErrors());
			AssertEquals(true, DogHitXRay.RemarksInfo.HasErrors());
			UPECusHAWB uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02755555555";
			uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02744444444";
			Factory.Save();
			DogHitXRay.TrackingNumber = "111";
			DogHitXRay.RunPreSaveValidation();
			AssertEquals(true, DogHitXRay.MasterbillNumberInfo.HasErrors());
		}

		public void TestTrackingNumber()
		{
			UPECusHAWB uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02755555555";
			uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02744444444";
			Factory.Save();
			DogHitXRay.TrackingNumber = "111";
			DogHitXRay.MasterbillNumber = "02744444444";
			DogHitXRay.TrackingNumber = "";
			AssertEquals("", DogHitXRay.MasterbillNumber);
			AssertNull(DogHitXRay.SelectedUPECusHAWB);
		}

		public void TestMasterbillNumberList()
		{
			UPECusHAWB uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02755555555";
			uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02744444444";
			Factory.Save();
			DogHitXRay.TrackingNumber = "111";
			AssertEquals(2, DogHitXRay.MasterbillNumberList.Count);
		}

		public void TestMasterbillNumberValidation()
		{
			UPECusHAWB uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02755555555";
			uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02744444444";
			Factory.Save();
			uPECusHAWB.MAWB.CM_MAWB = "02744444444";
			AssertEquals(false, DogHitXRay.MasterbillNumberInfo.HasErrors());
			DogHitXRay.MasterbillNumber = "TEST";
			AssertEquals(true, DogHitXRay.MasterbillNumberInfo.HasErrors());
		}

		public void TestRemarksValidation()
		{
			DogHitXRay.Remarks = "XXX";
			DogHitXRay.Remarks = "";
			AssertEquals(true, DogHitXRay.RemarksInfo.HasErrors());
			DogHitXRay.Remarks = "Test";
			AssertEquals(false, DogHitXRay.RemarksInfo.HasErrors());
		}

		public void TestSelectedUPECusHAWB()
		{
			UPECusHAWB uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02755555555";
			uPECusHAWB = CreateFirstUPECusHAWB();
			uPECusHAWB.MAWB.CM_MAWB = "02744444444";
			Factory.Save();
			AssertNull(DogHitXRay.SelectedUPECusHAWB);
			DogHitXRay.TrackingNumber = "111";
			AssertNull(DogHitXRay.SelectedUPECusHAWB);
			DogHitXRay.MasterbillNumber = "02744444444";
			AssertEquals("02744444444", DogHitXRay.SelectedUPECusHAWB.MAWB.CM_MAWB);
		}

		public void TestClear()
		{
			CreateFirstUPECusHAWB();
			DogHitXRay.TrackingNumber = "111";
			DogHitXRay.QuarantineHold = true;
			DogHitXRay.Remarks = "TEST";
			DogHitXRay.Clear();
			AssertEquals("", DogHitXRay.TrackingNumber);
			AssertEquals(true, DogHitXRay.CustomsHold);
			AssertEquals(false, DogHitXRay.QuarantineHold);
			AssertEquals("", DogHitXRay.Remarks);
			AssertEquals(false, DogHitXRay.HasErrors);
		}

		public void TestSetProcessQueueOnUPECusHAWB_QuarantineHoldSelected()
		{
			CreateFirstUPECusHAWB();
			DogHitXRay.TrackingNumber = "111";
			DogHitXRay.QuarantineHold = true;
			DogHitXRay.CustomsHold = false;
			DogHitXRay.Remarks = "TEST";
			DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_GS_NKTaskAssignedTo = "C";
			DogHitXRay.SetProcessQueueOnSelectedUPECusHAWB();
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_CustomsStatus);
			AssertEquals("TEST", DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_CustomsReason);
			AssertEquals("C", DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_GS_NKTaskAssignedTo);
		}

		public void TestSetProcessQueueOnUPECusHAWB_CustomsHoldSelected()
		{
			CreateFirstUPECusHAWB();
			DogHitXRay.TrackingNumber = "111";
			DogHitXRay.QuarantineHold = false;
			DogHitXRay.CustomsHold = true;
			DogHitXRay.Remarks = "TEST";
			DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_GS_NKTaskAssignedTo = "C";
			DogHitXRay.TrackingNumber = "111";
			DogHitXRay.QuarantineHold = false;
			DogHitXRay.CustomsHold = true;
			DogHitXRay.Remarks = "TEST";
			DogHitXRay.SetProcessQueueOnSelectedUPECusHAWB();
			AssertEquals(CargoReportQueueCodeDescriptionPairList.Codes.Hold, DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_CustomsQueue);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_CustomsStatus);
			AssertEquals("TEST", DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_CustomsReason);
			AssertEquals("C", DogHitXRay.SelectedUPECusHAWB.CurrentQueue.P4_GS_NKTaskAssignedTo);
		}

		[TestDate(2006, 1, 2, 10, 12, 13)]
		public void TestSendNotificationEmail()
		{
			DogHitXRayEmailTestClass dogHitXRay = new DogHitXRayEmailTestClass(Factory);
			dogHitXRay.SendNotificationEmail();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			CreateFirstUPECusHAWB();
			dogHitXRay.TrackingNumber = "111";
			dogHitXRay.QuarantineHold = false;
			dogHitXRay.CustomsHold = true;
			dogHitXRay.Remarks = "TEST";
			GlbGroup glbGroup = Factory.New<GlbGroup>();
			glbGroup.GG_Code = "XXX";
			UPEDataRegistry.Instance.DogHitXRayNotificationGroup = glbGroup.PK.ToGuid();
			GlbStaff glbStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			glbStaff.Groups.Add(glbGroup);
			glbStaff.GS_EmailAddress = "test@edi.com.au";
			Factory.Save();
			dogHitXRay.SendNotificationEmail();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Dog Hit Or X-Ray Hold for: 111", email.Subject);
			AssertEquals("Tracking Number : 111\nDate            : 02-Jan-06 10:12\nHold Type       : Customs Hold\nRemarks         : TEST", email.Body);
			AssertEquals("test@edi.com.au", email.Recipients[0]);
		}

		#region DogHitXRayEmailTestClass
		class DogHitXRayEmailTestClass : DogHitXRay
		{
			public DogHitXRayEmailTestClass(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		#endregion
		UPECusHAWB CreateFirstUPECusHAWB()
		{
			UPECusHAWB result = Factory.New<UPECusHAWB>();
			result.CS_CM = (Factory.New<CusMAWB>()).PK;
			result.CS_HAWB = "111";
			CreateJobRelatedWaybill(result.PK, "111", "222", JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			Factory.Save();
			return result;
		}

		void CreateJobRelatedWaybill(ZGuid parentID, string longTrackingNumber, string shortTrackingNumber, string waybillType)
		{
			JobRelatedWayBill jobRelatedWayBill = Factory.New<JobRelatedWayBill>();
			jobRelatedWayBill.EB_ParentID = parentID;
			jobRelatedWayBill.EB_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			jobRelatedWayBill.EB_WaybillNumber = longTrackingNumber;
			jobRelatedWayBill.EB_WaybillShortNumber = shortTrackingNumber;
			jobRelatedWayBill.EB_WaybillType = waybillType;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			DogHitXRay = new DogHitXRay(Factory);
		}

		DogHitXRay DogHitXRay;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DogHitXRay(Factory);
		}
	}
}
