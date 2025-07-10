using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class DogHitXRay : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DogHitXRay(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CustomsHold = true;
			QuarantineHold = false;
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateTrackingNumber();
			ValidateMasterbillNumber();
			ValidateRemarks();
		}

		#region Tracking Number

		[MaxLength(CusHAWB.Schema.CS_HAWBMaxLength)]
		public ZString TrackingNumber
		{
			get { return fTrackingNumber; }
			set
			{
				if (fTrackingNumber != value)
				{
					CheckMaximumLength(TrackingNumberInfo, value);
					SetNonPersistentPropertyValue(TrackingNumberInfo, ref fTrackingNumber, value);
					MasterbillNumber = "";
					ResetUPECusHAWBs();
					MasterbillNumberInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidateTrackingNumber();
					}
				}
			}
		}
		ZString fTrackingNumber;

		public ZPropertyInfo TrackingNumberInfo
		{
			get { return GetZPropertyInfo(nameof(TrackingNumber)); }
		}

		public void ValidateTrackingNumber()
		{
			TrackingNumberInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TrackingNumberInfo);
			if (UPECusHAWBs.Count == 0)
			{
				TrackingNumberInfo.AddError("Tracking number is not found.");
			}
		}

		#endregion

		#region Masterbill Number

		[MaxLength(CusMAWB.Schema.CM_MAWBMaxLength)]
		public ZString MasterbillNumber
		{
			get { return fMasterbillNumber; }
			set
			{
				if (fMasterbillNumber != value)
				{
					CheckMaximumLength(MasterbillNumberInfo, value);
					SetNonPersistentPropertyValue(MasterbillNumberInfo, ref fMasterbillNumber, value);
					if (!IsValidationSuspended)
					{
						ValidateMasterbillNumber();
					}
				}
			}
		}
		ZString fMasterbillNumber;

		public ZPropertyInfo MasterbillNumberInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(MasterbillNumber));
				((IZPropertyInfoObsolete)result).ReadOnly = UPECusHAWBs.Count < 2;
				return result;
			}
		}

		public void ValidateMasterbillNumber()
		{
			MasterbillNumberInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(MasterbillNumberInfo, MasterbillNumberList);
			if (!MasterbillNumberInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(MasterbillNumberInfo);
			}
		}

		#endregion

		#region Masterbill Number List

		public CodeDescriptionPairList MasterbillNumberList
		{
			get
			{
				if (fMasterbillNumberList == null)
				{
					fMasterbillNumberList = new CodeDescriptionPairList();
				}

				if (fMasterbillNumberList.Count == 0 && UPECusHAWBs.Count > 1)
				{
					foreach (UPECusHAWB uPECusHAWB in UPECusHAWBs)
					{
						fMasterbillNumberList.Add(new CodeDescriptionPair(uPECusHAWB.CS_MasterBillNum.ToString(), ""));
					}
				}
				return fMasterbillNumberList;
			}
		}
		CodeDescriptionPairList fMasterbillNumberList;

		#endregion

		#region Customs Hold

		public ZBool CustomsHold
		{
			get { return fCustomsHold; }
			set
			{
				SetNonPersistentPropertyValue(CustomsHoldInfo, ref fCustomsHold, value);
			}
		}
		ZBool fCustomsHold;

		public ZPropertyInfo CustomsHoldInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsHold)); }
		}

		#endregion

		#region Quarantine Hold

		public ZBool QuarantineHold
		{
			get { return fQuarantineHold; }
			set
			{
				SetNonPersistentPropertyValue(QuarantineHoldInfo, ref fQuarantineHold, value);
			}
		}
		ZBool fQuarantineHold;

		public ZPropertyInfo QuarantineHoldInfo
		{
			get { return GetZPropertyInfo(nameof(QuarantineHold)); }
		}

		#endregion

		#region Remarks

		[MaxLength(ProcessQueue.Schema.P4_ReasonMaxLength)]
		public ZString Remarks
		{
			get { return fRemarks; }
			set
			{
				if (fRemarks != value)
				{
					CheckMaximumLength(RemarksInfo, value);
					SetNonPersistentPropertyValue(RemarksInfo, ref fRemarks, value);
					if (!IsValidationSuspended)
					{
						ValidateRemarks();
					}
				}
			}
		}
		ZString fRemarks;

		public ZPropertyInfo RemarksInfo
		{
			get { return GetZPropertyInfo(nameof(Remarks)); }
		}

		public void ValidateRemarks()
		{
			RemarksInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RemarksInfo);
		}

		#endregion

		#region UPECusHAWBs

		public UPECusHAWB SelectedUPECusHAWB
		{
			get
			{
				UPECusHAWB result = null;

				if (UPECusHAWBs != null)
				{
					if (UPECusHAWBs.Count == 1)
					{
						result = UPECusHAWBs[0];
					}
					else
					{
						foreach (UPECusHAWB uPECusHAWB in UPECusHAWBs)
						{
							if (uPECusHAWB.CS_MasterBillNum == MasterbillNumber)
							{
								result = uPECusHAWB;
								break;
							}
						}
					}
				}

				return result;
			}
		}

		protected IReadOnlyList<UPECusHAWB> UPECusHAWBs
		{
			get
			{
				if (fUPECusHAWBs == null)
				{
					ZDBOnlyQuery uPECusHAWBQuery = new ZDBOnlyQuery(typeof(UPECusHAWB));

					ZDBOnlySubQuery relatedWaybillSubQuery = new ZDBOnlySubQuery(typeof(JobRelatedWayBill), JobRelatedWayBillSchema.EB_ParentID);
					relatedWaybillSubQuery.AddToFilter(JobRelatedWayBillSchema.EB_WaybillNumber, TrackingNumber);

					uPECusHAWBQuery.AddSubQuery(relatedWaybillSubQuery, JoinCondition.And);

					fUPECusHAWBs = (UPECusHAWB[])WorkingFactory.Load(typeof(UPECusHAWB), uPECusHAWBQuery);
				}
				return fUPECusHAWBs;
			}
		}
		UPECusHAWB[] fUPECusHAWBs;

		#region Working Factory

		BusinessObjectFactory WorkingFactory
		{
			get
			{
				if (fWorkingFactory == null)
				{
					fWorkingFactory = new BusinessObjectFactory();
				}
				return fWorkingFactory;
			}
		}
		BusinessObjectFactory fWorkingFactory;

		#endregion

		void ResetUPECusHAWBs()
		{
			fUPECusHAWBs = null;
			fMasterbillNumberList = null;
			fWorkingFactory = null;
		}

		public void SetProcessQueueOnSelectedUPECusHAWB()
		{
			if (CustomsHold)
			{
				SelectedUPECusHAWB.MoveCustomsQueueTo(
					CargoReportQueueCodeDescriptionPairList.Codes.Hold,
					ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease,
					ZString.Empty,
					"Dog Hit/X-Ray: CUSTOMS HOLD");

				SelectedUPECusHAWB.CurrentQueue.P4_CustomsReason = Remarks;
			}
			else if (QuarantineHold)
			{
				SelectedUPECusHAWB.MoveCustomsQueueTo(
					CargoReportQueueCodeDescriptionPairList.Codes.Quarantine,
					ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold,
					ZString.Empty,
					"Dog Hit/X-Ray: QUARANTINE HOLD");

				SelectedUPECusHAWB.CurrentQueue.P4_CustomsReason = Remarks;
			}

			SelectedUPECusHAWB.CurrentQueue.SetReadOnlyIncludingChildren(true);
		}

		#endregion

		public void Clear()
		{
			try
			{
				using (GetValidationSuspender())
				{
					SetDefaultValues();
					TrackingNumber = "";
					Remarks = "";
				}
			}
			finally
			{
				RefreshBinding();
			}
		}

		#region Email Sending

		public void SendNotificationEmail()
		{
			if (SelectedUPECusHAWB != null)
			{
				EmailDef emailToSend = new EmailDef();
				emailToSend.Subject = "Dog Hit Or X-Ray Hold for: " + SelectedUPECusHAWB.CS_HAWB;
				emailToSend.Body =
					string.Format(
					"Tracking Number : {0}\n" +
					"Date            : {1}\n" +
					"Hold Type       : {2}\n" +
					"Remarks         : {3}",
					SelectedUPECusHAWB.CS_HAWB,
					ZDateTime.Now.ToLongTimeString(),
					CustomsHold ? "Customs Hold" : "Quarantine Hold",
					Remarks);

				emailToSend.AddRecipientForUserCommunication(new EmailGroupUtility().GetGroupEmailCollection(UPEDataRegistry.Instance.DogHitXRayNotificationGroup, false));
				if (emailToSend.Recipients.Count == 0)
				{
					emailToSend.AddRecipientForUserCommunication(new EmailGroupUtility().GetCompanyNotificationGroupEmails());
				}

				Env.OutgoingMailManager.CreateAndSave(emailToSend);
			}
		}

		#endregion
	}
}
