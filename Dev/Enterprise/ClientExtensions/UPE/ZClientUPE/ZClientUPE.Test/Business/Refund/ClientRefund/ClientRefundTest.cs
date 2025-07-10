using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ClientRefund))]
	public class ClientRefundTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAtFaultList()
		{
			AssertContains("CUS should be visible", "CUS", Refund.AtFaultList.ElementsAsString);
			AssertContains("GDW should be visible", "GDW", Refund.AtFaultList.ElementsAsString);
			AssertNotContains("BOB should not be visible", "BOB", Refund.AtFaultList.ElementsAsString);
			TestHelper.SetupAtFaultRegistry("BOB", string.Empty, Factory);
			AssertContains("CUS should be visible", "CUS", Refund.AtFaultList.ElementsAsString);
			AssertContains("GDW should be visible", "GDW", Refund.AtFaultList.ElementsAsString);
			AssertContains("BOB should be visible", "BOB", Refund.AtFaultList.ElementsAsString);
		}

		[TestDate(2008, 8, 8)]
		public void TestPostRefund()
		{
			Refund.PostRefund = false;
			Assert(!Refund.PostRefund);
			Factory.Save();
			Assert("DateCreated can be set only when Refund was Posted", Refund.T10_DateCreated.IsEmpty);
			Assert("ControlNumber can be generated only when Refund was Posted", Refund.T10_ControlNumber.IsEmpty);
			Assert("Email was not sent", !Refund.SendNotificationEmailCalled);
			Assert("Notes were not Added", !Refund.AddRefundNoteCalled);
			ClientRefundDummy emptyRefund = Factory.NewWithValidTestData<ClientRefundDummy>();
			emptyRefund.PostRefund = true;
			Factory.Save();
			Assert("DateCreated can be set only when Refund was Posted", emptyRefund.T10_DateCreated.IsEmpty);
			Assert("ControlNumber can be generated only when Refund was Posted", emptyRefund.T10_ControlNumber.IsEmpty);
			Assert("Email was not sent", !emptyRefund.SendNotificationEmailCalled);
			Assert("Notes were not Added", !emptyRefund.AddRefundNoteCalled);
			Refund.PostRefund = true;
			Assert(Refund.PostRefund);
			Factory.Save();
			Assert("DateCreated is set when Refund Posted", !Refund.T10_DateCreated.IsEmpty);
			AssertEquals("DateCreated is set when Refund Posted", TestDateAttribute.Date, Refund.T10_DateCreated.ToDateTime());
			Assert("ControlNumber is generated when Refund Posted", !Refund.T10_ControlNumber.IsEmpty);
			Assert("Notification email was sent", Refund.SendNotificationEmailCalled);
			Assert("Notes Added after Post", Refund.AddRefundNoteCalled);
			Assert("Refund alredy Posted", !Refund.PostRefund);
		}

		[TestDate(2008, 8, 8)]
		public void TestProcessRefund()
		{
			ZString note = "AMOUNT TO BE CREDITED";
			RefundManager.CreateClientRefund();
			AssertNotNull("RefundManager.Refund", RefundManager.Refund);
			AssertNotNull("Owner can't be null", RefundManager.Refund.Declaration);
			ZBool isRefundEnquiry = RefundManager.Refund.Declaration.IsRefundEnquiry;
			ZBool isRefundProcessed = RefundManager.Refund.Declaration.IsRefundProcessed;
			RefundManager.Refund.PostRefund = true;
			Factory.Save();
			AssertNotContainsNote("Shouldn't contain Processed note: " + note, note, RefundManager.Refund.Declaration.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.RefundNote.Description));
			Assert("Refund was posted", RefundManager.Refund.Declaration.IsRefundEnquiry);
			Assert("Refund wasn't processed", !RefundManager.Refund.Declaration.IsRefundProcessed);
			Assert("Refund wasn't processed", RefundManager.Refund.T10_DateProcessed.IsEmpty);
			Assert(!RefundManager.Refund.PostRefund);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(10);
			RefundManager.Refund.ProcessRefund = true;
			TestHelper.SetupRefundNotifcationGroupRegistry(Factory);
			Factory.Save();
			AssertContainsNote("Should contain Processed note: " + note, note, RefundManager.Refund.Declaration.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.RefundNote.Description));
			Assert("IsRefundEnquiry marked off", !RefundManager.Refund.Declaration.IsRefundEnquiry);
			Assert("Refund was processed", RefundManager.Refund.Declaration.IsRefundProcessed);
			AssertEquals("Refund was processed", TestDateAttribute.Date, RefundManager.Refund.T10_DateProcessed.ToDateTime());
			AssertEquals("10 days diffrense", RefundManager.Refund.T10_DateProcessed, RefundManager.Refund.T10_DateCreated.AddDays(10));
			Assert(!RefundManager.Refund.ProcessRefund);
		}

		void AssertContainsNote(string message, ZString expected, StmNote[] original)
		{
			AssertNotNull(message, Array.Find(original, n => n.ST_NoteDataAsText.Contains(expected)));
		}

		void AssertNotContainsNote(string message, ZString expected, StmNote[] original)
		{
			AssertNull(message, Array.Find(original, n => n.ST_NoteDataAsText.Contains(expected)));
		}

		public void TestIsRefundRejected()
		{
			Assert("Refund is not rejected by default", !Refund.T10_IsRefundRejected);
		}

		[TestDate(2008, 8, 8)]
		public void TestControlNumber()
		{
			ZString controlNumber = string.Empty;
			BusinessObjectFactory emptyFactory = new BusinessObjectFactory();
			emptyFactory.Saving += delegate
			{
				controlNumber = Refund.GetNewControlNumber();
			};
			emptyFactory.Save();
			AssertEquals("080001", controlNumber);
		}

		public void TestEnquiryDetailsValid()
		{
			Assert("EnquiryDetailsValid", !Factory.NewWithValidTestData<ClientRefund>().EnquiryDetailsValid);
			Assert("EnquiryDetailsValid", Refund.EnquiryDetailsValid);
		}

		public void TestBindingLists()
		{
			ClientRefund refund = Factory.NewWithValidTestData<ClientRefund>();
			AssertNotNull(refund.RaisedByList);
			AssertNotNull(refund.AtFaultList);
			AssertNotNull(refund.ReasonTypesPairList);
		}

		public void TestRemarks()
		{
			ClientRefund refund = Factory.NewWithValidTestData<ClientRefund>();
			refund.RunPreSaveValidation();
			Assert(refund.RemarksInfo.HasErrors());
			refund.Remarks = "TEST";
			refund.RunPreSaveValidation();
			Assert(!refund.RemarksInfo.HasErrors());
		}

		public void TestWriteOffAndRefund()
		{
			Refund.T10_RefundAmount = 111.111m;
			Refund.T10_WriteOffAmount = 222.222m;
			AssertEquals(333.333m, Refund.WriteOffAndRefund);
		}

		[TestDate(2008, 8, 8)]
		public void TestNoteText()
		{
			Refund.T10_ControlNumber = "#1234567890";
			AssertEquals("NoteText", BuildNoteText(noteTextFormat, Refund), Refund.NoteText);
		}

		public void TestExtractRefundAmountFromCustoms()
		{
			RefundManager.CreateClientRefund();
			AssertNotNull("RefundManager.Refund", RefundManager.Refund);
			AssertNotNull("RefundManager.Refund.Declaration", RefundManager.Refund.Declaration);
			AddRefundedCustomsMessage(RefundManager.Refund.Declaration, 123.12m);
			Assert("RefundAmount is empty", RefundManager.Refund.T10_RefundAmount.IsEmpty);
			ZDecimal refundAmountFromCustoms = RefundManager.Refund.ExtractRefundAmountFromCustoms(RefundManager.Refund.Declaration);
			AssertEquals("RefundAmount is extracted", 123.12m, refundAmountFromCustoms);
		}

		public void TestSendNotificationEmail()
		{
			ZString trackingNumber = "TrackingNumber";
			ZString invoiceNumber = "InvoiceNumber";
			ZDecimal invoiceAmount = 123.123m;
			GlbStaff staff = TestHelper.NewStaff(Factory);
			Refund.T10_ControlNumber = "#1234567890";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Refund.SendNotificationEmail(staff.GS_EmailAddress, trackingNumber, invoiceNumber, invoiceAmount);
			AssertEquals("1 email should be send", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(staff.GS_EmailAddress, ZString.Format("Refund Enquiry for: {0}, Control Number: {1}", trackingNumber, Refund.T10_ControlNumber), BuildNoteText(noteTextFormat, Refund) + ZString.Format(emailBodyFormat, trackingNumber, invoiceNumber, invoiceAmount), Env.OutgoingMailManager.EmailsCreated[0]);
		}

		public void TestSendNotificationEmailDefault()
		{
			TestHelper.SetCompanyNotificationGroup(Refund.EmailUtility, Factory).Staff.Add(TestHelper.NewStaff("Bill Gates", "billyboy@microsoft.com", Factory));
			Factory.Save();
			ZString trackingNumber = "trackingNumber";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Refund.SendNotificationEmail(string.Empty, trackingNumber, string.Empty, 0);
			AssertEquals("1 email should be send", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail("billyboy@microsoft.com", "Refund Enquiry for: " + trackingNumber, BuildNoteText(noteTextFormat, Refund) + ZString.Format(emailBodyFormat, trackingNumber, string.Empty, 0), Env.OutgoingMailManager.EmailsCreated[0]);
		}

		public void TestSendNotificationEmailWithException()
		{
			Assert("No error messages", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			Refund.PostRefund = true;
			Factory.Save();
			AssertNotContains("IRefundEnquiry.SendNotificationEmail Failed: ", ErrorReporter.LastMessageReported);
			Assert(!Refund.PostRefund);
			Refund.PostRefund = true;
			Refund.ThrowExceptionOnEmailSending = true;
			Factory.Save();
			AssertContains("Notification Email sending failed for declaration", ErrorReporter.LastMessageReported);
			Refund.ThrowExceptionOnEmailSending = false;
			ExceptionReporterTestListener.Instance.Clear();
		}

		[TestDate(2008, 8, 8)]
		public void TestSendNotificationRejectedEmail()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			UPEDataRegistry.Instance.RefundNotificationGroup = group.PK.ToGuid();
			group.Staff.Add(TestHelper.NewStaff(Factory));
			Refund.T10_IsRefundRejected = true;
			Refund.PostRefund = true;
			Refund.ProcessRefund = true;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(group.Staff[0].GS_EmailAddress, "Refund Enquiry Rejected , Control Number: " + Refund.T10_ControlNumber, Refund.ProcessedRefundNoteText, Env.OutgoingMailManager.EmailsCreated[0]);
		}

		void AssertEmail(ZString expectedRecipient, ZString expectedSubject, ZString expectedBody, EmailDef email)
		{
			Assert("email has recipients", email.Recipients.Count == 1);
			AssertEquals("Email Recipient", expectedRecipient.ToString(), email.Recipients[0]);
			AssertEquals("Email Subject", expectedSubject.ToString(), email.Subject);
			AssertEquals("Email body", expectedBody.ToString(), email.Body);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		ZString BuildNoteText(ZString format, ClientRefund refund)
		{
			return ZString.Format(format, GlbStaff.CurrentUser.GS_FullName, ZDateTime.Now.ToLongTimeString(), refund.T10_EnquiryContact, refund.T10_EnquiryPhoneNumber, refund.T10_EnquiryRaisedBy, refund.T10_EnquiryDetails, refund.T10_ControlNumber);
		}

		const string noteTextFormat = @"REFUND ENQUIRY

USER                          : {0}
DATE                          : {1}
CONTACT                       : {2}
PHONE NUMBER                  : {3}
RAISED BY                     : {4}
ENQUIRY DETAILS               : {5}
REFUND ENQUIRY CONTROL NUMBER : {6}";

		const string emailBodyFormat = @"
TRACKING NUMBER               : {0}
INVOICE NUMBER                : {1}
INVOICE AMOUNT                : {2}";

		internal ClientRefundDummy Refund
		{
			get
			{
				return refund ?? (refund = (ClientRefundDummy)TestHelper.PopulatedRefund(Factory));
			}
		}

		ClientRefundDummy refund;
		RefundManagerDummy<UPEJobDeclaration> RefundManager
		{
			get
			{
				return manager ?? (manager = new RefundManagerDummy<UPEJobDeclaration>(Factory.NewWithValidTestData<UPEJobDeclaration>()));
			}
		}

		RefundManagerDummy<UPEJobDeclaration> manager;
		#region Test Helper
		public static class TestHelper
		{
			#region AtFault
			public static GlbStaff SetupAtFaultRegistry(BusinessObjectFactory factory)
			{
				return SetupAtFaultRegistry(factory.Load<GlbGroup>(Core.Constants.Groups.AllPK), NewStaff(factory));
			}

			public static void SetupAtFaultRegistry(GlbStaff staff)
			{
				SetupAtFaultRegistry(staff.Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK), staff);
			}

			public static GlbStaff SetupAtFaultRegistry(string staffName, string staffEmail, BusinessObjectFactory factory)
			{
				return SetupAtFaultRegistry(factory.Load<GlbGroup>(Core.Constants.Groups.AllPK), staffName, staffEmail);
			}

			public static GlbStaff SetupAtFaultRegistry(GlbGroup group, string staffName, string staffEmail)
			{
				return SetupAtFaultRegistry(group, NewStaff(staffName, staffEmail, group.Factory));
			}

			public static GlbStaff SetupAtFaultRegistry(GlbGroup group, GlbStaff staff)
			{
				UPEGlbGroupsRegistryObjectCollection collection = new UPEGlbGroupsRegistryObjectCollection();
				collection.AddNew().Group = group.PK;
				group.Staff.Add(staff);
				UPEDataRegistry.Instance.AtFaultGroupsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
				group.Factory.Save();
				return staff;
			}

			public static GlbStaff NewStaff(BusinessObjectFactory factory)
			{
				return NewStaff("BOB", "bob@bob.com", factory);
			}

			public static GlbStaff NewStaff(string name, string email, BusinessObjectFactory factory)
			{
				GlbStaff staff = factory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = name;
				staff.GS_EmailAddress = email;
				return staff;
			}

			#endregion
			#region RefundNotification
			public static GlbGroup SetupRefundNotifcationGroupRegistry(BusinessObjectFactory factory)
			{
				GlbGroup group = factory.NewWithValidTestData<GlbGroup>();
				group.Staff.Add(NewStaff(factory));
				SetupRefundNotifcationGroupRegistry(group);
				factory.Save();
				return group;
			}

			public static void SetupRefundNotifcationGroupRegistry(GlbGroup group)
			{
				UPEDataRegistry.Instance.RefundNotificationGroup = group.PK.ToGuid();
				group.Factory.Save();
			}

			#endregion
			public static ClientRefund PopulatedRefund(BusinessObjectFactory factory)
			{
				return PopulateRefund(factory.NewWithValidTestData<ClientRefundDummy>());
			}

			public static ClientRefund PopulateRefund(ClientRefund refund)
			{
				GlbStaff staff = NewStaff("Mr Bob", "email@fortest.com", refund.Factory);
				refund.T10_RefundReason = refund.ReasonTypesPairList[0].Code;
				refund.T10_GS_NKAtFaultUser = staff.GS_Code;
				refund.T10_EnquiryContact = staff.GS_FullName;
				refund.T10_EnquiryPhoneNumber = "123123123";
				refund.T10_EnquiryDetails = "T10_EnquiryDetails";
				refund.T10_EnquiryRaisedBy = refund.RaisedByList[0].Code;
				refund.Remarks = "Remarks";
				refund.Factory.Save();
				return refund;
			}

			public static GlbGroup SetCompanyNotificationGroup(EmailGroupUtility utility, BusinessObjectFactory factory)
			{
				GlbGroup group = factory.NewWithValidTestData<GlbGroup>();
				utility.SetNotificationGroup(group.PK.ToGuid());
				return group;
			}
		}

		#endregion
		void AddRefundedCustomsMessage(JobDeclaration declaration, ZDecimal refundAmount)
		{
			CusEntryHeader header = declaration.CustomsEntryHeaders.AddNew();
			header.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Refunded;
			//CMRIMDRMessage message = (CMRIMDRMessage)declaration.Messages.AddNew(typeof(CMRIMDRMessage));
			EDIMessage message = declaration.Messages.AddNew();
			message.EM_ApplicationCode = CMRMessage.ApplicationCodes.CMR;
			message.EM_MessageType = CMRMessage.CMRMessageTypes.IMD;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			ZString amount = refundAmount.Round(2).ToString().PadLeft(16, '0');
			message.EM_MessageText = ZString.Format(responseFormat, amount);
		}

		readonly string responseFormat = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+4A1G 4H8A 89G5:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++AUSTRALIAN CUSTOMS SERVICE'
NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'
RFF+ABO:B00148580/1/SYD2::12'
RFF+ABT:AAAA9JARY::3'
RFF+ABQ:373'
RFF+ADU:B00148580/1'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
TAX+3'
MOA+39:0000000001100.00'
TAX+3'
MOA+40:0000000001100.00'
TAX+3'
MOA+55:0000000000000.54'
TAX+3'
MOA+369:0000000000126.55'
TAX+3'
MOA+68:0000000000165.00'
TAX+3'
MOA+128:{0}'
DOC+1+1'
CST+2+N10::95'
FTX+AAF+++5% ?+ $0.05449/LITRE'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001100.00'
TAX+1'
MOA+55:0000000000000.54'
TAX+1'
MOA+369:0000000000126.55'
TAX+1'
MOA+56:0000000001265.54'
TAX+1'
MOA+68:0000000000165.00'
ERP+::218'
ERC+2::95'
FTX+ABS+++GOODS (CHEMICALS) MAY BE REGULATED BY NICNAS. RING 1800638528'
CNT+5:1'
UNT+51+000001'".Replace("\r\n", "");
		#endregion
	}
}
