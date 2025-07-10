using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutCreditCardPayment))]
	internal class CalloutCreditCardPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExpiryDate_Month_Year()
		{
			AssertEquals(20, PaymentDetails.ExpiryDate_Year_List.Count);
			AssertEquals((ZDateTime.Now.Year - 2000).ToString(), PaymentDetails.ExpiryDate_Year_List[0].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 1).ToString(), PaymentDetails.ExpiryDate_Year_List[1].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 2).ToString(), PaymentDetails.ExpiryDate_Year_List[2].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 3).ToString(), PaymentDetails.ExpiryDate_Year_List[3].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 4).ToString(), PaymentDetails.ExpiryDate_Year_List[4].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 5).ToString(), PaymentDetails.ExpiryDate_Year_List[5].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 6).ToString(), PaymentDetails.ExpiryDate_Year_List[6].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 7).ToString(), PaymentDetails.ExpiryDate_Year_List[7].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 8).ToString(), PaymentDetails.ExpiryDate_Year_List[8].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 9).ToString(), PaymentDetails.ExpiryDate_Year_List[9].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 10).ToString(), PaymentDetails.ExpiryDate_Year_List[10].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 11).ToString(), PaymentDetails.ExpiryDate_Year_List[11].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 12).ToString(), PaymentDetails.ExpiryDate_Year_List[12].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 13).ToString(), PaymentDetails.ExpiryDate_Year_List[13].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 14).ToString(), PaymentDetails.ExpiryDate_Year_List[14].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 15).ToString(), PaymentDetails.ExpiryDate_Year_List[15].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 16).ToString(), PaymentDetails.ExpiryDate_Year_List[16].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 17).ToString(), PaymentDetails.ExpiryDate_Year_List[17].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 18).ToString(), PaymentDetails.ExpiryDate_Year_List[18].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 19).ToString(), PaymentDetails.ExpiryDate_Year_List[19].Code);
		}

		public void TestExpiryDate_Month_List()
		{
			AssertEquals(12, PaymentDetails.ExpiryDate_Month_List.Count);
			AssertEquals("01", PaymentDetails.ExpiryDate_Month_List[0].Code);
			AssertEquals("02", PaymentDetails.ExpiryDate_Month_List[1].Code);
			AssertEquals("03", PaymentDetails.ExpiryDate_Month_List[2].Code);
			AssertEquals("04", PaymentDetails.ExpiryDate_Month_List[3].Code);
			AssertEquals("05", PaymentDetails.ExpiryDate_Month_List[4].Code);
			AssertEquals("06", PaymentDetails.ExpiryDate_Month_List[5].Code);
			AssertEquals("07", PaymentDetails.ExpiryDate_Month_List[6].Code);
			AssertEquals("08", PaymentDetails.ExpiryDate_Month_List[7].Code);
			AssertEquals("09", PaymentDetails.ExpiryDate_Month_List[8].Code);
			AssertEquals("10", PaymentDetails.ExpiryDate_Month_List[9].Code);
			AssertEquals("11", PaymentDetails.ExpiryDate_Month_List[10].Code);
			AssertEquals("12", PaymentDetails.ExpiryDate_Month_List[11].Code);
		}

		public void TestExpiryDate_Month()
		{
			AssertEquals("Invalid max length", 2, PaymentDetails.ExpiryDate_MonthInfo.MaxLength);
		}

		public void TestExpiryDate_Year()
		{
			AssertEquals("Invalid max length", 2, PaymentDetails.ExpiryDate_YearInfo.MaxLength);
		}

		public void TestCardNumber()
		{
			AssertEquals("Invalid max length", 16, PaymentDetails.CardNumberInfo.MaxLength);
		}

		public void TestNameOnCard()
		{
			AssertEquals("Invalid max length", 35, PaymentDetails.NameOnCardInfo.MaxLength);
		}

		public void TestSecurityCode()
		{
			AssertEquals("Invalid max length", 6, PaymentDetails.SecurityCodeInfo.MaxLength);
		}

		public void TestCreateCalloutPaymentNote()
		{
			PaymentDetails.TotalAmount = 650m;
			PaymentDetails.CardNumber = "1234567890123456";
			PaymentDetails.NameOnCard = "Jack Daniels";
			PaymentDetails.VPOSCode = "VPOS";
			PaymentDetails.SecurityCode = "123456";
			PaymentDetails.ExpiryDate_Month = "12";
			PaymentDetails.ExpiryDate_Year = "06";
			AssertEquals("Pre-condition", 0, Callout.Notes.GetAllNotes().Count);
			PaymentDetails.CreateCalloutPaymentNote();
			StmNote note = ((StmNoteCollection)Callout.Notes.GetAllNotes())[0];
			AssertEquals("Payment Details", note.ST_Description);
			AssertEquals(@"Payment Method: Credit Card
Total Amount: 650
Card Number: XXXX XXXX XXXX 3456
Expiry Date: 12/06
Security Code: 123 456
Name: Jack Daniels
VPOS: VPOS", note.ST_NoteDataAsText);
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
			return new CalloutCreditCardPayment(Callout);
		}

		CalloutCreditCardPayment PaymentDetails
		{
			get
			{
				if (fPaymentDetails == null)
				{
					fPaymentDetails = (CalloutCreditCardPayment)GetNewBusinessObject();
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
		CalloutCreditCardPayment fPaymentDetails;
		#endregion
	}
}
