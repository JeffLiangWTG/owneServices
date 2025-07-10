using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutPartPayment : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CalloutPartPayment(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateReasonType();
			ValidateAmountToCollect();
			ValidateRemarks();
		}

		public string NoteText
		{
			get
			{
				return
					"Reason Type       : " + CalloutPartPaymentCodeDescriptionPairList.GetDescriptionFromCode(ReasonType) + "\r\n" +
					"Amount To Collect : $" + AmountToCollect + "\r\n" +
					"Remarks           : " + Remarks;
			}
		}

		public bool MustAddNote
		{
			get { return fMustAddNote; }
			set { fMustAddNote = value; }
		}
		bool fMustAddNote;

		public bool IsControlNumberRequired
		{
			get { return ReasonType == CalloutPartPaymentCodeDescriptionPairList.Codes.BRK || ReasonType == CalloutPartPaymentCodeDescriptionPairList.Codes.OTH; }
		}

		public bool IsRefundRequired
		{
			get { return ReasonType == CalloutPartPaymentCodeDescriptionPairList.Codes.BRK; }
		}

		#region Email Sending

		public void SendNotificationEmail(string trackingNumber, string invoiceNumber, ZDecimal invoiceAmount, ZString controlNumber)
		{
			EmailDef emailToSend = new EmailDef();
			emailToSend.Subject = "Part Payment for: " + trackingNumber;
			if (!controlNumber.IsEmpty)
			{
				emailToSend.Subject += " Control Number: " + controlNumber;
			}

			emailToSend.Body =
				string.Format(
				"Date                  : {0}\n" +
				"User                  : {1}\n" +
				"Tracking Number       : {2}\n" +
				"Invoice Number        : {3}\n" +
				"Invoice Amount        : ${4}\n" +
				"Reason Type           : {5}\n" +
				"Amount For Collection : ${6}\n" +
				"Remarks               : {7}",
				ZDateTime.Now.ToLongTimeString(),
				GlbStaff.CurrentUser.GS_FullName,
				trackingNumber,
				invoiceNumber,
				invoiceAmount,
				CalloutPartPaymentCodeDescriptionPairList.GetDescriptionFromCode(ReasonType),
				AmountToCollect,
				Remarks);

			emailToSend.AddRecipientForUserCommunication(new EmailGroupUtility().GetGroupEmailCollection(UPEDataRegistry.Instance.PartPaymentNotificationGroup, false));
			if (emailToSend.Recipients.Count == 0)
			{
				emailToSend.AddRecipientForUserCommunication(new EmailGroupUtility().GetCompanyNotificationGroupEmails());
			}

			Env.OutgoingMailManager.CreateAndSave(emailToSend);
		}

		#endregion

		#region Reason Type

		[MaxLength(3)]
		public ZString ReasonType
		{
			get { return fReasonType; }
			set
			{
				if (fReasonType != value)
				{
					CheckMaximumLength(ReasonTypeInfo, value);
					SetNonPersistentPropertyValue(ReasonTypeInfo, ref fReasonType, value);
					if (!IsValidationSuspended)
					{
						ValidateReasonType();
					}
				}
			}
		}
		ZString fReasonType;

		public ZPropertyInfo ReasonTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ReasonType)); }
		}

		public void ValidateReasonType()
		{
			ReasonTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReasonTypeInfo);
			ListValidation.ErrorIfInvalidCode(ReasonTypeInfo, CalloutPartPaymentCodeDescriptionPairList);
		}

		#endregion

		#region AmountToCollect

		public ZDecimal AmountToCollect
		{
			get { return fAmountToCollect; }
			set
			{
				if (fAmountToCollect != value)
				{
					SetNonPersistentPropertyValue(AmountToCollectInfo, ref fAmountToCollect, value);
					if (!IsValidationSuspended)
					{
						ValidateAmountToCollect();
					}
				}
			}
		}
		ZDecimal fAmountToCollect;

		public ZPropertyInfo AmountToCollectInfo
		{
			get { return GetZPropertyInfo(nameof(AmountToCollect)); }
		}

		public void ValidateAmountToCollect()
		{
			AmountToCollectInfo.ClearAllNotifications();
			if (AmountToCollect < 0m)
			{
				AmountToCollectInfo.AddError("The Amount To Collect must be greater than 0.");
			}
		}

		#endregion

		#region Remarks

		[MaxLength(1000)]
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

		#region Bind To List

		public CalloutPartPaymentCodeDescriptionPairList CalloutPartPaymentCodeDescriptionPairList
		{
			get
			{
				if (fCalloutPartPaymentCodeDescriptionPairList == null)
				{
					fCalloutPartPaymentCodeDescriptionPairList = new CalloutPartPaymentCodeDescriptionPairList();
				}
				return fCalloutPartPaymentCodeDescriptionPairList;
			}
		}
		CalloutPartPaymentCodeDescriptionPairList fCalloutPartPaymentCodeDescriptionPairList;

		#endregion
	}
}
