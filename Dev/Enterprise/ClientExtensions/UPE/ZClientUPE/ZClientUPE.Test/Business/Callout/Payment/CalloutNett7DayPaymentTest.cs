using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutNett7DayPayment))]
	internal class CalloutNett7DayPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 115;
			PaymentDetails.IsBusiness = true;
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: Nett 7 Day
Total Amount: 115
Nett 7 Day Type: Business", note.ST_NoteDataAsText);
			Assert("should be read-only", note.ReadOnly);
			Assert("should be custom description", note.ST_IsCustomDescription);
			PaymentDetails.IsResidential = true;
			Callout.Notes.GetAllNotes().RemoveAll();
			PaymentDetails.CreateCalloutPaymentNote();
			note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals(@"Payment Method: Nett 7 Day
Total Amount: 115
Nett 7 Day Type: Residential", note.ST_NoteDataAsText);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CalloutNett7DayPayment(Callout);
		}

		CalloutNett7DayPayment PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutNett7DayPayment)GetNewBusinessObject();
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
		CalloutNett7DayPayment fPaymentDetails;
		#endregion
	}
}
