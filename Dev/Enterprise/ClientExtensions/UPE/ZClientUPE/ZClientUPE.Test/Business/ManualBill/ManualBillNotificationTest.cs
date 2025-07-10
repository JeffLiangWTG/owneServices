using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class ManualBillNotificationTest : TestCaseWithFactory
	{
		public void TestShouldDoManualBillActivitiesOnSave()
		{
			SetHasChargesBeenAmendedSinceBISIUpload(false);
			AssertEquals(false, ManualBillNotification.ShouldDoManualBillActivitiesOnSave);
			SetHasChargesBeenAmendedSinceBISIUpload(true);
			AssertEquals(true, ManualBillNotification.ShouldDoManualBillActivitiesOnSave);
		}

		public void TestDoManualBillActivities_SendingEmail()
		{
			BillingNotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.ManualbillNotificationGroup = BillingNotificationGroup.PK.ToGuid();
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.CustomsCharges.Clear();
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 1000m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 1500m, 0m, false, creditor.PK));
			BISIShipmentDataAccessor accessor = new BISIShipmentDataAccessor(Factory);
			accessor.UpdateUploadData(CusHAWB);
			Factory.ClearQueryCache();
			declaration.CustomsCharges.Clear();
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 2000m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 3000m, 0m, false, creditor.PK));
			declaration.JE_HouseBill = "H12345";
			declaration.JE_MasterBill = "M12345";
			ManualBillNotification.DoManualBillActivities();
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Manual Bill - H12345", email.Subject);
			AssertEquals("clinty@edi.com.au", email.Recipients[0]);
			AssertEquals("Body", @"
Formal Declaration has been amended and manual billing is required.
A manual bill is required.

Tracking number: H12345
Master Air Waybill number: M12345

Charges:
201 = $2000.00
206 = $3000.00
", email.Body);
		}

		public void TestDoManualBillActivities_CreateNote()
		{
			BillingNotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.ManualbillNotificationGroup = BillingNotificationGroup.PK.ToGuid();
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.CustomsCharges.Clear();
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 2000m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 3000m, 0m, false, creditor.PK));
			declaration.JE_HouseBill = "H12345";
			declaration.JE_MasterBill = "M12345";
			ManualBillNotification.DoManualBillActivities();
			StmNote[] note = declaration.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.ManualBillNote.Description);
			AssertEquals("A note should be added", @"
Formal Declaration has been amended and manual billing is required.
A manual bill is required.

Tracking number: H12345
Master Air Waybill number: M12345

Charges:
201 = $2000.00
206 = $3000.00
", note[0].ST_NoteText);
		}

		public void TestDoManualBillActivities_CompleteCommercialQueues()
		{
			BillingNotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.ManualbillNotificationGroup = BillingNotificationGroup.PK.ToGuid();
			UPECusHAWB cusHAWB1 = Factory.NewWithValidTestData<UPECusHAWB>();
			UPECusHAWB cusHAWB2 = Factory.NewWithValidTestData<UPECusHAWB>();
			UPECusHAWB cusHAWB2_Split = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB1.CS_JE_CustomsFormalEntry = declaration.PK;
			cusHAWB2.CS_JE_CustomsFormalEntry = declaration.PK;
			cusHAWB2_Split.CS_JE_CustomsFormalEntry = declaration.PK;
			cusHAWB1.CS_HAWB = "HAWB1";
			cusHAWB2.CS_HAWB = "HAWB2";
			cusHAWB2_Split.CS_HAWB = "HAWB2";
			cusHAWB2_Split.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
			cusHAWB2_Split.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment;
			declaration.ResetRelatedCusHAWBs();
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.CustomsCharges.Clear();
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 2000m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 3000m, 0m, false, creditor.PK));
			declaration.JE_HouseBill = "HAWB1";
			ManualBillNotification.DoManualBillActivities();
			AssertEquals("Both shipments should be finance completed", CommercialQueueCodeDescriptionPairList.Codes.Completed, cusHAWB1.CurrentQueue.P4_QueueName);
			AssertEquals("Both shipments should be finance completed", CommercialQueueCodeDescriptionPairList.Codes.Completed, cusHAWB2.CurrentQueue.P4_QueueName);
			AssertEquals("The split shipment should still be marked as 'subsequent split shipment'", CommercialQueueCodeDescriptionPairList.Codes.Completed, cusHAWB2_Split.CurrentQueue.P4_QueueName);
			AssertEquals("The split shipment should still be marked as 'subsequent split shipment'", ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment, cusHAWB2_Split.CurrentQueue.P4_Status);
		}

		public void TestHasChargesBeenAmendedSinceBISIUpload()
		{
			UPECusHAWB cusHAWB = this.CusHAWB;
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			declaration.CustomsCharges.Clear();
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 1000m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 1500m, 0m, false, creditor.PK));
			BISIShipmentDataAccessor accessor = new BISIShipmentDataAccessor(Factory);
			accessor.UpdateUploadData(cusHAWB);
			Factory.ClearQueryCache();
			AssertEquals("Charges uploaded to BISI not amended", false, ManualBillNotification.HasChargesBeenAmendedSinceBISIUpload);
			declaration.CustomsCharges.Clear();
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 1500m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 1000m, 0m, false, creditor.PK));
			AssertEquals("Order of charges has changed, but amounts has not changed", false, ManualBillNotification.HasChargesBeenAmendedSinceBISIUpload);
			declaration.CustomsCharges.Clear();
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 1000m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 2000m, 0m, false, creditor.PK));
			AssertEquals("A charge amount has changed", true, ManualBillNotification.HasChargesBeenAmendedSinceBISIUpload);
			declaration.CustomsCharges.Clear();
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 1000m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 1500m, 0m, false, creditor.PK));
			declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.Woodlevy, 1m, 0m, false, creditor.PK));
			AssertEquals("A charge has been added", true, ManualBillNotification.HasChargesBeenAmendedSinceBISIUpload);
		}

		#region Test Classes

		class TestManualBillNotification : ManualBillNotification
		{
			public TestManualBillNotification(UPEJobDeclaration declaration) : base(declaration)
			{
			}

			public new bool HasChargesBeenAmendedSinceBISIUpload
			{
				get
				{
					return base.HasChargesBeenAmendedSinceBISIUpload;
				}
			}
		}

		#endregion
		#region Implementation
		GlbGroup BillingNotificationGroup
		{
			get
			{
				if (fBillingNotificationGroup == null)
				{
					fBillingNotificationGroup = Factory.New<GlbGroup>();
					fBillingNotificationGroup.GG_Code = "UPW";
					GlbStaff staff = fBillingNotificationGroup.Staff.AddNew();
					staff.GS_EmailAddress = "clinty@edi.com.au";
					staff.GS_Code = "ZAC";
				}

				return fBillingNotificationGroup;
			}
		}

		GlbGroup fBillingNotificationGroup;
		TestManualBillNotification ManualBillNotification
		{
			get
			{
				if (fManualBillNotification == null)
				{
					fManualBillNotification = new TestManualBillNotification(declaration);
				}

				return fManualBillNotification;
			}
		}

		TestManualBillNotification fManualBillNotification;
		UPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
					CusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
				}

				return fCusHAWB;
			}
		}

		UPECusHAWB fCusHAWB;
		UPEJobDeclarationWithDummyCharges declaration;
		OrgHeader creditor;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			declaration = Factory.New<UPEJobDeclarationWithDummyCharges>();
			base.SetUp();
			creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		void SetHasChargesBeenAmendedSinceBISIUpload(bool value)
		{
			if (value)
			{
				declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
				declaration.CustomsCharges.Clear();
				declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 1000m, 0m, false, creditor.PK));
				declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 1500m, 0m, false, creditor.PK));
				BISIShipmentDataAccessor accessor = new BISIShipmentDataAccessor(Factory);
				accessor.UpdateUploadData(CusHAWB);
				declaration.CustomsCharges.Clear();
				declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 2000m, 0m, false, creditor.PK));
				declaration.CustomsCharges.Add(new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 3000m, 0m, false, creditor.PK));
			}
			else
			{
				declaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			}

			declaration.RelatedCusHAWBs.Load();
			AssertEquals(value, ManualBillNotification.HasChargesBeenAmendedSinceBISIUpload);
		}
		#endregion
	}
}
