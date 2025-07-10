using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutOtherPayment))]
	internal class CalloutOtherPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAccountNumber()
		{
			AssertEquals("Invalid max length", 300, PaymentDetails.NoteInfo.MaxLength);
		}

		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 105;
			PaymentDetails.Note = "this is to be cleared on Monday";
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: Other
Total Amount: 105
Note: this is to be cleared on Monday", note.ST_NoteDataAsText);
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
			return new CalloutOtherPayment(Callout);
		}

		CalloutOtherPayment PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutOtherPayment)GetNewBusinessObject();
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
		CalloutOtherPayment fPaymentDetails;
		#endregion
	}
}
