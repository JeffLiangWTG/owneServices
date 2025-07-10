using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutPaymentDetailsForTest))]
	internal class CalloutPaymentDetailsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCalloutNoteIsReadonly()
		{
			PaymentDetails.TotalAmount = 150m;
			PaymentDetails.CreateCalloutPaymentNote();
			Assert("Newly created callout note should be readonly", ((StmNoteCollection)Callout.Notes.GetAllNotes())[0].ReadOnly);
		}

		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 150m;
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			AssertEquals("There should be a payment details note now", 1, Callout.Notes.GetAllNotes().Count);
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: Test
Total Amount: 150
my mum's credit card", note.ST_NoteDataAsText);
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
			return new CalloutPaymentDetailsForTest(Callout);
		}

		CalloutPaymentDetailsForTest PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutPaymentDetailsForTest)GetNewBusinessObject();
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

		CalloutPaymentDetailsForTest fPaymentDetails;
		Callout fCallout;
		#region CalloutPaymentDetailsForTest
		class CalloutPaymentDetailsForTest : CalloutPaymentDetails
		{
			public CalloutPaymentDetailsForTest(Callout callout) : base(callout)
			{
			}

			protected override string Reference
			{
				get
				{
					return "my mum's credit card";
				}
			}

			protected override string PaymentMethodAsText
			{
				get
				{
					return "Test";
				}
			}
		}
		#endregion
		#endregion
	}
}
