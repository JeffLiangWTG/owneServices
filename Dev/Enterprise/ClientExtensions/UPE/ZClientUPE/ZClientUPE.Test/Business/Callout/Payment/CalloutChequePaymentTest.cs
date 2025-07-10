using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutChequePayment))]
	internal class CalloutChequePaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 99m;
			PaymentDetails.Note = "This is the cheque";
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: Cheque
Total Amount: 99
Note: This is the cheque", note.ST_NoteDataAsText);
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
			return new CalloutChequePayment(Callout);
		}

		CalloutChequePayment PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutChequePayment)GetNewBusinessObject();
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
		CalloutChequePayment fPaymentDetails;
		#endregion
	}
}
