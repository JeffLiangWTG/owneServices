using System;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business
{
	public class ClientRefund : AutoClientRefund, IObsoleteValidation
	{
		public ClientRefund(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Schema

		public new class Schema : AutoClientRefund.Schema
		{
			public const string T10_IsRefundApproved = "T10_IsRefundApproved";
		}

		#endregion

		#region Related Business Objects

		public UPEJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.Load<UPEJobDeclaration>(T10_JE)); }
		}
		UPEJobDeclaration declaration;

		public Callout Callout
		{
			get { return callout ?? (callout = Factory.Load<Callout>(T10_CS)); }
		}
		Callout callout;

		#endregion

		#region Override

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();

			if (T10_IsRefundRejected)
			{
				Validation.ValidateT10_RefundRejectedDetails();
			}
			else
			{
				Validation.ValidateT10_RefundProcessingFee();
				Validation.ValidateT10_RefundAmount();
				Validation.ValidateT10_WriteOffAmount();
				Validation.ValidateT10_AdditionalCharges();
				Validation.ValidateT10_RefundReason();
				Validation.ValidateT10_AmountRefundedToUPS();
				Validation.ValidateT10_GS_NKAtFaultUser();

				ValidateRemarks();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (PostRefund && EnquiryDetailsValid)
			{
				T10_DateCreated = ZDateTime.Now;
				T10_ControlNumber = NewControlNumber();

				SetIsRefundEnquiry(Callout, true);
				SetIsRefundEnquiry(Declaration, true);

				PostRefund = false;
				SendEmailOnSaved = true;

				AddRefundNote(Callout);
				AddRefundNote(Declaration);
			}

			if (ProcessRefund && !T10_ControlNumber.IsEmpty)
			{
				SwitchRefundFlags(Declaration);
				T10_DateProcessed = ZDateTime.Now;

				ProcessRefund = false;
				SendRejectedEmailOnSaved = T10_IsRefundRejected;

				AddProcessedRefundNote(Declaration);
				AddRefundNote(Declaration);
			}
		}

		void SetIsRefundEnquiry(IRefundEnquiry owner, bool val)
		{
			if (owner != null)
			{
				owner.IsRefundEnquiry = val;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (SendEmailOnSaved)
				{ SendEmailOnSaved = !TrySendEmail(SendNotificationEmail); }
				if (SendRejectedEmailOnSaved)
				{ SendRejectedEmailOnSaved = !TrySendEmail(SendRejectedNotificationEmail); }
			}
		}

		bool TrySendEmail(Action action)
		{
			bool result = true;
			try
			{
				action();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("IRefundEnquiry.SendNotificationEmail Failed", "Notification Email sending failed for declaration " + (Declaration != null ? Declaration.JE_HouseBill : ZString.Empty), ex);
				result = false;
			}
			return result;
		}

		void SwitchRefundFlags(IRefundEnquiry refundEnquiry)
		{
			if (refundEnquiry != null)
			{
				refundEnquiry.IsRefundEnquiry ^= refundEnquiry.IsRefundEnquiry;
				refundEnquiry.IsRefundProcessed = !refundEnquiry.IsRefundEnquiry;
			}
		}

		protected virtual void AddRefundNote(IRefundEnquiry refundEnquiry)
		{
			if (refundEnquiry != null)
			{
				refundEnquiry.AddRefundNote();
			}
		}

		public override ZPropertyInfo T10_AmountRefundedToUPSInfo
		{
			get
			{
				ZPropertyInfo result = base.T10_AmountRefundedToUPSInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region Binding Lists

		#region RaisedByList

		RaisedByPairList raisedByList;

		public RaisedByPairList RaisedByList
		{
			get { return raisedByList ?? (raisedByList = new RaisedByPairList()); }
		}

		#endregion

		#region AtFaultList

		public RefundUsersPairList AtFaultList
		{
			get { return GetAtFaultList(); }
		}

		RefundUsersPairList GetAtFaultList()
		{
			RefundUsersPairList result = new RefundUsersPairList();
			foreach (UPEGlbGroupsRegistryObject group in UPEDataRegistry.Instance.AtFaultGroupsItem.Value)
			{
				result.AddRange(Factory.Load<GlbGroup>(group.Group).Staff);
			}
			result.AddPair("CUS", "Customer");
			result.AddPair("GDW", "Goodwill");
			return result;
		}

		#endregion

		#region ReasonTypesPairList

		public RefundProcessingReasonTypesPairList ReasonTypesPairList
		{
			get { return refundProcessingReasonTypesPairList ?? (refundProcessingReasonTypesPairList = new RefundProcessingReasonTypesPairList()); }
		}

		RefundProcessingReasonTypesPairList refundProcessingReasonTypesPairList;

		#endregion

		#endregion

		#region Properties

		#region T10_IsRefundApproved

		public ZBool T10_IsRefundApproved
		{
			get { return !T10_IsRefundRejected; }
			set { T10_IsRefundRejected = !value; }
		}

		public ZPropertyInfo T10_IsRefundApprovedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.T10_IsRefundApproved, x => T10_IsRefundRejectedInfo); }
		}

		#endregion

		#region Remarks

		[MaxLength(1000)]
		public ZString Remarks
		{
			get { return remarks; }
			set
			{
				if (remarks != value)
				{
					CheckMaximumLength(RemarksInfo, value);
					SetNonPersistentPropertyValue(RemarksInfo, ref remarks, value);
					if (!IsValidationSuspended)
					{
						ValidateRemarks();
					}

					RemarksInfo.RefreshBinding();
				}
			}
		}
		ZString remarks;

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

		#region WriteOffAndRefund

		public ZDecimal WriteOffAndRefund
		{
			get { return (T10_WriteOffAmount + T10_RefundAmount); }
		}

		public ZPropertyInfo WriteOffAndRefundInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(WriteOffAndRefund));
				((IZPropertyInfoObsolete)info).ReadOnly = true;
				return info;
			}
		}

		#endregion

		internal ZBool ProcessRefund { get; set; }
		internal ZBool PostRefund { get; set; }
		ZBool SendEmailOnSaved { get; set; }
		ZBool SendRejectedEmailOnSaved { get; set; }

		#region Enqury Details Validation

		public virtual void ValidateEnquiryDetailsAll()
		{
			Validation.ValidateT10_EnquiryContact();
			Validation.ValidateT10_EnquiryPhoneNumber();
			Validation.ValidateT10_EnquiryDetails();
			Validation.ValidateT10_EnquiryRaisedBy();
		}

		public ZBool EnquiryDetailsValid
		{
			get
			{
				ValidateEnquiryDetailsAll();
				return
					!T10_EnquiryContactInfo.HasErrors() &&
					!T10_EnquiryPhoneNumberInfo.HasErrors() &&
					!T10_EnquiryDetailsInfo.HasErrors() &&
					!T10_EnquiryRaisedByInfo.HasErrors();
			}
		}

		#endregion

		#endregion

		#region Implementation

		public string NoteText
		{
			get
			{
				return ZString.Format(
@"REFUND ENQUIRY

USER                          : {0}
DATE                          : {1}
CONTACT                       : {2}
PHONE NUMBER                  : {3}
RAISED BY                     : {4}
ENQUIRY DETAILS               : {5}
REFUND ENQUIRY CONTROL NUMBER : {6}",
									GlbStaff.CurrentUser.GS_FullName,
									ZDateTime.Now.ToLongTimeString(),
									T10_EnquiryContact,
									T10_EnquiryPhoneNumber,
									T10_EnquiryRaisedBy,
									T10_EnquiryDetails,
									T10_ControlNumber);
			}
		}

		public string ProcessedRefundNoteText
		{
			get
			{
				string headerNote = ZString.Format(
@"User                                            : {0}
Date                                            : {1}
Control Number                                  : {2}
Refund Enquiry                                  : {3}

", GlbStaff.CurrentUser.GS_FullName,
	ZDateTime.Now.ToString(),
	T10_ControlNumber,
	(T10_IsRefundRejected ? "REJECTED" : "APPROVED"));

				string refundRejectedNote = ZString.Format(
				@"Reason: 
{0}"
				, T10_RefundRejectedDetails);

				string refundApprovedNote = ZString.Format(
				@"Refund Processing Fee                           : ${0}

AMOUNT TO BE CREDITED
Write off                                       : ${1}
Refund                                          : ${2}
Write & Refund                                  : ${3}
Additional Charges for Invoicing to Customer    : ${4}
Amount Refund to UPS                            : ${5}
At Fault                                        : {6}
Remarks                                         : {7}"
				, T10_RefundProcessingFee,
					T10_WriteOffAmount,
					T10_RefundAmount,
					T10_WriteOffAmount + T10_RefundAmount,
					T10_AdditionalCharges,
					T10_AmountRefundedToUPS,
					T10_GS_NKAtFaultUser,
					Remarks);

				return headerNote + (T10_IsRefundRejected ? refundRejectedNote : refundApprovedNote);
			}
		}

		void AddProcessedRefundNote(BusinessObject notifyObject)
		{
			if (notifyObject != null)
			{
				notifyObject.GetNotes().AddNew(false, UPEPredefinedNoteTypes.Instance.RefundNote.Description, ProcessedRefundNoteText);
			}
		}

		internal ZString NewControlNumber()
		{
			return ZDateTime.Now.Year.ToString().Substring(2) + UPENumberFountains.Instance.RefundControlNumberFountain.GetNextFormatted(Factory).PadLeft(4, '0');
		}

		public ZDecimal ExtractRefundAmountFromCustoms(JobDeclaration respondee)
		{
			ZDecimal result = 0.0m;
			if (respondee != null)
			{
				foreach (CusEntryHeader header in respondee.CustomsEntryHeaders)
				{
					if (header.PaymentStatus == CMREntryPaymentStatusList.Descriptions.Refunded)
					{
						EDIMessage message = respondee.Messages.GetLastMessage(CMRMessage.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.IMD, EDIMessage.Direction.Receive);
						if (message != null &&
							message.EM_MessageSubType == CMRIMDRMessage.ManifestResponseSubTypes.Clear)
						{
							EdiMessageWrapper ediWrapper = new EdiMessageWrapper(message);
							result = new OutstandingAmountRetriever(ediWrapper).OutstandingAmount;
							break;
						}
					}
				}
			}
			return result;
		}

		#region Email Sending

		public void SendNotificationEmail(ZString recipient, string trackingNumber, string invoiceNumber, ZDecimal invoiceAmount)
		{
			EmailDef email = new EmailDef();
			email.Subject = "Refund Enquiry for: " + trackingNumber;
			email.Subject += !T10_ControlNumber.IsEmpty ? ", Control Number: " + T10_ControlNumber.ToString() : string.Empty;
			email.Body = string.Format(NoteText +
@"
TRACKING NUMBER               : {0}
INVOICE NUMBER                : {1}
INVOICE AMOUNT                : {2}",
									trackingNumber,
									invoiceNumber,
									invoiceAmount);

			if (!recipient.IsEmpty)
			{
				email.AddRecipientForUserCommunication(recipient);
			}
			if (email.Recipients.Count == 0)
			{
				email.AddRecipientForUserCommunication(EmailGroupUtility.GetCompanyNotificationGroupEmails());
			}
			Env.OutgoingMailManager.CreateAndSave(email);
		}

		protected virtual EmailGroupUtility EmailGroupUtility
		{
			get { return new EmailGroupUtility(); }
		}

		void SendRejectedNotificationEmail()
		{
			EmailDef email = new EmailDef();
			email.Body = ProcessedRefundNoteText;
			email.Subject = "Refund Enquiry Rejected " + (Declaration != null ? Declaration.JE_HouseBill.ToString() : string.Empty);
			email.Subject += ", Control Number: " + T10_ControlNumber;
			Env.OutgoingMailManager.CreateAndSave(email, UPEDataRegistry.Instance.RefundNotificationGroup, GroupSourceLocator.GetFromRegistryItem(UPEDataRegistry.Instance.RefundNotificationGroupItem));
		}

		protected virtual void SendNotificationEmail()
		{
			if (Declaration != null)
			{
				((IRefundEnquiry)Declaration).SendNotificationEmail();
			}
			if (Callout != null)
			{
				((IRefundEnquiry)Callout).SendNotificationEmail();
			}
		}

		#endregion

		#endregion
	}

	class EdiMessageWrapper : IOutstandingPaymentInfoProvider
	{
		public EdiMessageWrapper(EDIMessage message)
		{
			this.message = message;
		}

		public SegmentGroup5MessageSection Group5Section
		{
			get
			{
				SegmentGroup5MessageSection result = null;
				if (message != null)
				{
					IOutstandingPaymentInfoProvider provider = message as IOutstandingPaymentInfoProvider;
					if (provider != null)
					{
						result = provider.Group5Section;
					}
					else
					{
						CUSRESMessage cusres = message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet()) as CUSRESMessage;
						if (cusres != null)
						{
							result = cusres.Group5;
						}
					}
				}
				return result;
			}
		}

		public GISSegmentMessageSection GISSection
		{
			get
			{
				GISSegmentMessageSection result = null;
				if (message != null)
				{
					IOutstandingPaymentInfoProvider provider = message as IOutstandingPaymentInfoProvider;
					if (provider != null)
					{
						result = provider.GISSection;
					}
					else
					{
						CUSRESMessage cusres = message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet()) as CUSRESMessage;
						if (cusres != null)
						{
							result = cusres.GIS;
						}
					}
				}
				return result;
			}
		}

		readonly EDIMessage message;
	}
}
