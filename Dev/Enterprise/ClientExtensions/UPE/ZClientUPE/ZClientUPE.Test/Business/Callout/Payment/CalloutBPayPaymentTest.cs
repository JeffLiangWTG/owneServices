using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutBPayPayment))]
	internal class CalloutBPayPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMandatoryFields()
		{
			PaymentDetails.AccountName = "blah";
			Assert("need a value", !PaymentDetails.AccountNameInfo.HasError("Please enter an Account Name"));
			PaymentDetails.ReceiptNumber = "blah";
			Assert("need a value", !PaymentDetails.ReceiptNumberInfo.HasError("Please enter a Receipt Number"));
			PaymentDetails.AccountName = "";
			Assert("need a value", PaymentDetails.AccountNameInfo.HasError("Please enter an Account Name"));
			PaymentDetails.ReceiptNumber = "";
			Assert("need a value", PaymentDetails.ReceiptNumberInfo.HasError("Please enter a Receipt Number"));
			PaymentDetails.DatePaid = ZDateTime.Invalid;
			Assert("need a valid date", PaymentDetails.DatePaidInfo.HasError("Please enter a valid date"));
			PaymentDetails.DatePaid = new ZDateTime(2000, 1, 1);
			Assert("need a valid date", !PaymentDetails.DatePaidInfo.HasError("Please enter a valid date"));
			PaymentDetails.DatePaid = ZDateTime.Empty;
			Assert("need a valid date", PaymentDetails.DatePaidInfo.HasError("Please enter a valid date"));
		}

		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 100m;
			PaymentDetails.DatePaid = new ZDateTime(2009, 1, 1);
			PaymentDetails.AccountName = "556892012";
			PaymentDetails.ReceiptNumber = "014789";
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: BPay
Total Amount: 100
Receipt Number: 014789
Account Name: 556892012
Date Paid: 01-Jan-09", note.ST_NoteDataAsText);
			Assert("should be read-only", note.ReadOnly);
			Assert("should be custom description", note.ST_IsCustomDescription);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CalloutBPayPayment(Callout);
		}

		CalloutBPayPayment PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutBPayPayment)GetNewBusinessObject();
				}

				return fPaymentDetails;
			}
		}

		Callout Callout
		{
			get
			{
				if (fCallout == null)
				{
					fCallout = Factory.New<Callout>();
				}

				return fCallout;
			}
		}

		Callout fCallout;
		CalloutBPayPayment fPaymentDetails;
		#endregion
	}
}
