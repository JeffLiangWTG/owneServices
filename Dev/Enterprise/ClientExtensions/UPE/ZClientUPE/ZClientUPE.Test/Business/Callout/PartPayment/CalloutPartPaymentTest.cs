using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutPartPayment))]
	public class CalloutPartPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMustAddNote()
		{
			AssertEquals(false, CalloutPartPayment.MustAddNote);
			CalloutPartPayment.MustAddNote = true;
			AssertEquals(true, CalloutPartPayment.MustAddNote);
		}

		public void TestIsControlNumberRequired()
		{
			AssertEquals(false, CalloutPartPayment.IsControlNumberRequired);
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.OTH;
			AssertEquals(true, CalloutPartPayment.IsControlNumberRequired);
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.COD;
			AssertEquals(false, CalloutPartPayment.IsControlNumberRequired);
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.BRK;
			AssertEquals(true, CalloutPartPayment.IsControlNumberRequired);
		}

		public void TestIsRefundRequired()
		{
			AssertEquals(false, CalloutPartPayment.IsRefundRequired);
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.OTH;
			AssertEquals(false, CalloutPartPayment.IsRefundRequired);
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.BRK;
			AssertEquals(true, CalloutPartPayment.IsRefundRequired);
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.COD;
			AssertEquals(false, CalloutPartPayment.IsRefundRequired);
		}

		public void TestRunPreSaveValidation()
		{
			CalloutPartPayment.ReasonType = "XYZ";
			CalloutPartPayment.AmountToCollect = -1m;
			CalloutPartPayment.Remarks = "XYZ";
			CalloutPartPayment.Remarks = "";
			AssertEquals(true, CalloutPartPayment.ReasonTypeInfo.HasErrors());
			Assert(CalloutPartPayment.AmountToCollectInfo.HasError("The Amount To Collect must be greater than 0."));
			AssertEquals(true, CalloutPartPayment.RemarksInfo.HasErrors());
		}

		public void TestNoteText_BRK()
		{
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.BRK;
			CalloutPartPayment.AmountToCollect = 10.34m;
			CalloutPartPayment.Remarks = "TEST";
			string expected = "Reason Type       : " + CalloutPartPaymentCodeDescriptionPairList.Descriptions.BRK + "\r\n" + "Amount To Collect : $10.34\r\n" + "Remarks           : TEST";
			AssertEquals(expected, CalloutPartPayment.NoteText);
		}

		public void TestNoteText_COD()
		{
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.COD;
			CalloutPartPayment.AmountToCollect = 10.34m;
			CalloutPartPayment.Remarks = "TEST";
			string expected = "Reason Type       : " + CalloutPartPaymentCodeDescriptionPairList.Descriptions.COD + "\r\n" + "Amount To Collect : $10.34\r\n" + "Remarks           : TEST";
			AssertEquals(expected, CalloutPartPayment.NoteText);
		}

		public void TestNoteText_OTH()
		{
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.OTH;
			CalloutPartPayment.AmountToCollect = 10.34m;
			CalloutPartPayment.Remarks = "TEST";
			string expected = "Reason Type       : " + CalloutPartPaymentCodeDescriptionPairList.Descriptions.OTH + "\r\n" + "Amount To Collect : $10.34\r\n" + "Remarks           : TEST";
			AssertEquals(expected, CalloutPartPayment.NoteText);
		}

		public void TestReasonTypeValidation()
		{
			CalloutPartPayment.ReasonType = "XYZ";
			AssertEquals(true, CalloutPartPayment.ReasonTypeInfo.HasErrors());
			CalloutPartPayment.ReasonType = "";
			AssertEquals(true, CalloutPartPayment.ReasonTypeInfo.HasErrors());
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.BRK;
			AssertEquals(false, CalloutPartPayment.ReasonTypeInfo.HasErrors());
		}

		public void TestAmountToCollectValidation()
		{
			CalloutPartPayment.AmountToCollect = 10m;
			AssertEquals(false, CalloutPartPayment.AmountToCollectInfo.HasErrors());
			CalloutPartPayment.AmountToCollect = 0m;
			AssertEquals(false, CalloutPartPayment.AmountToCollectInfo.HasErrors());
			CalloutPartPayment.AmountToCollect = -1m;
			Assert(CalloutPartPayment.AmountToCollectInfo.HasError("The Amount To Collect must be greater than 0."));
		}

		public void TestRemarksValidation()
		{
			CalloutPartPayment.Remarks = "XYZ";
			AssertEquals(false, CalloutPartPayment.RemarksInfo.HasErrors());
			CalloutPartPayment.Remarks = "";
			AssertEquals(true, CalloutPartPayment.RemarksInfo.HasErrors());
		}

		public void TestBindToList()
		{
			AssertEquals(3, CalloutPartPayment.CalloutPartPaymentCodeDescriptionPairList.Count);
			Assert(CalloutPartPayment.CalloutPartPaymentCodeDescriptionPairList.ContainsCode(CalloutPartPaymentCodeDescriptionPairList.Codes.BRK));
			Assert(CalloutPartPayment.CalloutPartPaymentCodeDescriptionPairList.ContainsCode(CalloutPartPaymentCodeDescriptionPairList.Codes.COD));
			Assert(CalloutPartPayment.CalloutPartPaymentCodeDescriptionPairList.ContainsCode(CalloutPartPaymentCodeDescriptionPairList.Codes.OTH));
		}

		[TestDate(2006, 1, 2, 10, 12, 13)]
		public void TestSendNotificationEmail()
		{
			GlbStaff glbStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			glbStaff.GS_FullName = "Developer";
			CalloutPartPayment.ReasonType = CalloutPartPaymentCodeDescriptionPairList.Codes.OTH;
			CalloutPartPayment.AmountToCollect = 10.34m;
			CalloutPartPayment.Remarks = "TEST";
			string expected = "Date                  : 02-Jan-06 10:12\n" + "User                  : Developer\n" + "Tracking Number       : TrackingNumber\n" + "Invoice Number        : InvoiceNumber\n" + "Invoice Amount        : $21.22\n" + "Reason Type           : Other\n" + "Amount For Collection : $10.34\n" + "Remarks               : TEST";
			GlbGroup glbGroup = Factory.New<GlbGroup>();
			glbGroup.GG_Code = "XXX";
			UPEDataRegistry.Instance.PartPaymentNotificationGroup = glbGroup.PK.ToGuid();
			glbStaff.Groups.Add(glbGroup);
			glbStaff.GS_EmailAddress = "test@edi.com.au";
			Factory.Save();
			CalloutPartPayment.SendNotificationEmail("TrackingNumber", "InvoiceNumber", 21.22m, "060005");
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Part Payment for: TrackingNumber Control Number: 060005", email.Subject);
			AssertEquals(expected, email.Body);
			AssertEquals("test@edi.com.au", email.Recipients[0]);
			CalloutPartPayment.SendNotificationEmail("TrackingNumber", "InvoiceNumber", 21.22m, "");
			email = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals("Part Payment for: TrackingNumber", email.Subject);
		}

		CalloutPartPayment CalloutPartPayment
		{
			get
			{
				if (fCalloutPartPayment == null)
				{
					fCalloutPartPayment = new CalloutPartPayment(Factory);
				}

				return fCalloutPartPayment;
			}
		}

		CalloutPartPayment fCalloutPartPayment;
	}
}
