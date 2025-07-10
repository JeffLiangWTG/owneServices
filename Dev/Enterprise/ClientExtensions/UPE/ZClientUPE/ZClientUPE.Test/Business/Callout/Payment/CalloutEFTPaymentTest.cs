using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutEFTPayment))]
	internal class CalloutEFTPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBSBNumber()
		{
			AssertEquals("Invalid max length", 6, PaymentDetails.BSBNumberInfo.MaxLength);
		}

		public void TestAccountNumber()
		{
			AssertEquals("Invalid max length", 9, PaymentDetails.AccountNumberInfo.MaxLength);
		}

		public void TestAccountName()
		{
			AssertEquals("Invalid max length", 35, PaymentDetails.AccountNameInfo.MaxLength);
		}

		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 100m;
			PaymentDetails.AccountName = "Jim Beam";
			PaymentDetails.AccountNumber = "556892012";
			PaymentDetails.BSBNumber = "014789";
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: Electronic Fund Transfer (EFT)
Total Amount: 100
BSB Number: 014789
Account Number: 556892012
Account Name: Jim Beam", note.ST_NoteDataAsText);
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
			return new CalloutEFTPayment(Callout);
		}

		CalloutEFTPayment PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutEFTPayment)GetNewBusinessObject();
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
		CalloutEFTPayment fPaymentDetails;
		#endregion
	}
}
