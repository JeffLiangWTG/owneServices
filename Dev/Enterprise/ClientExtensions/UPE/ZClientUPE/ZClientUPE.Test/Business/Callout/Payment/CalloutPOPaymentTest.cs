using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutPOPayment))]
	internal class CalloutPOPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPurchaseOrderNumber()
		{
			AssertEquals("Invalid max length", 35, PaymentDetails.PurchaseOrderNumberInfo.MaxLength);
		}

		public void TestPurchaseOrderName()
		{
			AssertEquals("Invalid max length", 35, PaymentDetails.PurchaseOrderNameInfo.MaxLength);
		}

		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 200m;
			PaymentDetails.PurchaseOrderNumber = "PO222255556987";
			PaymentDetails.PurchaseOrderName = "Randy West";
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: Purchase Order
Total Amount: 200
Purchase Order Number: PO222255556987
Purchase Order Name: Randy West", note.ST_NoteDataAsText);
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
			return new CalloutPOPayment(Callout);
		}

		CalloutPOPayment PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutPOPayment)GetNewBusinessObject();
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
		CalloutPOPayment fPaymentDetails;
		#endregion
	}
}
