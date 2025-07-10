using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutAccountPayment))]
	internal class CalloutAccountPaymentTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestAccountNumber()
		{
			AssertEquals("Invalid max length", 35, PaymentDetails.AccountNumberInfo.MaxLength);
		}

		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 333m;
			PaymentDetails.AccountNumber = "XXX1920382";
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: Account
Total Amount: 333
Account Number: XXX1920382", note.ST_NoteDataAsText);
			Assert("should be read-only", note.ReadOnly);
			Assert("should be custom description", note.ST_IsCustomDescription);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CalloutAccountPayment(Callout);
		}

		CalloutAccountPayment PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutAccountPayment)GetNewBusinessObject();
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
		CalloutAccountPayment fPaymentDetails;
		#endregion
	}
}
