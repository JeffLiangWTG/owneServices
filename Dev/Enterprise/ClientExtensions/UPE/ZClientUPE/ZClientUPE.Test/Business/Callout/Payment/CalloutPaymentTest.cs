using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutPayment))]
	internal class CalloutPaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultIsNone()
		{
			AssertEquals("Should be defaulted to None", UPECargoPaymentMethod.None, Payment.PaymentMethod);
			Assert("Should be defaulted to None", Payment.IsNone);
		}

		public void TestIsCreditCard()
		{
			Payment.IsCreditCard = true;
			AssertEquals(UPECargoPaymentMethod.CreditCard, Payment.PaymentMethod);
			AssertEquals(true, Payment.IsCreditCardInfo.ReadOnly);
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			Assert("Should be false", !Payment.IsCreditCard);
			AssertEquals(false, Payment.IsCreditCardInfo.ReadOnly);
		}

		public void TestIsCheque()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				Payment.IsCheque = true;
				AssertEquals(UPECargoPaymentMethod.Cheque, Payment.PaymentMethod);
				AssertEquals(true, Payment.IsChequeInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, Payment.IsChequeInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				var zone = CODPostcodeTransportProvider.Zones.AddNew();
				var item = zone.Items.AddNew();
				var postCode1 = Factory.New<RefPostCode>();
				postCode1.RK_CityTownPostCode = "1";
				item.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
				var postCode2 = Factory.New<RefPostCode>();
				postCode2.RK_CityTownPostCode = "2";
				item.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
				Callout.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				Callout.Consignee.MainAddress.OA_PostCode = "2";
				Payment.PaymentMethod = UPECargoPaymentMethod.None;
				Assert("Should be false", !Payment.IsCheque);
				AssertEquals(false, Payment.IsChequeInfo.ReadOnly);
				CODPostcodeTransportProvider.Delete();
				AssertEquals("Not in COD Post code, should be true", true, Payment.IsChequeInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestIsPurchaseOrder()
		{
			Payment.IsPurchaseOrder = true;
			AssertEquals(UPECargoPaymentMethod.PurchaseOrder, Payment.PaymentMethod);
			AssertEquals(true, Payment.IsPurchaseOrderInfo.ReadOnly);
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			Assert("Should be false", !Payment.IsPurchaseOrder);
			AssertEquals(false, Payment.IsPurchaseOrderInfo.ReadOnly);
		}

		public void TestIsOther()
		{
			Payment.IsOther = true;
			AssertEquals(UPECargoPaymentMethod.Other, Payment.PaymentMethod);
			AssertEquals(true, Payment.IsOtherInfo.ReadOnly);
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			Assert("Should be false", !Payment.IsOther);
			AssertEquals(false, Payment.IsOtherInfo.ReadOnly);
		}

		public void TestIsAccount()
		{
			Payment.IsAccount = true;
			AssertEquals(UPECargoPaymentMethod.Account, Payment.PaymentMethod);
			AssertEquals(true, Payment.IsAccountInfo.ReadOnly);
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			Assert("Should be false", !Payment.IsAccount);
			AssertEquals(false, Payment.IsAccountInfo.ReadOnly);
		}

		public void TestIsNett7Day()
		{
			Payment.IsNett7Day = true;
			AssertEquals(UPECargoPaymentMethod.Nett7Day, Payment.PaymentMethod);
			AssertEquals(true, Payment.IsNett7DayInfo.ReadOnly);
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			Assert("Should be false", !Payment.IsNett7Day);
			AssertEquals(false, Payment.IsNett7DayInfo.ReadOnly);
		}

		public void TestIsBPay()
		{
			Payment.IsBPay = true;
			AssertEquals(UPECargoPaymentMethod.BPay, Payment.PaymentMethod);
			AssertEquals(true, Payment.IsBPayInfo.ReadOnly);
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			Assert("Should be false", !Payment.IsBPay);
			AssertEquals(false, Payment.IsBPayInfo.ReadOnly);
		}

		public void TestIsEFT()
		{
			Payment.IsEFT = true;
			AssertEquals(UPECargoPaymentMethod.EFT, Payment.PaymentMethod);
			AssertEquals(true, Payment.IsEFTInfo.ReadOnly);
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			Assert("Should be false", !Payment.IsEFT);
			AssertEquals(false, Payment.IsEFTInfo.ReadOnly);
		}

		public void TestIsNone()
		{
			Payment.IsNone = true;
			AssertEquals(UPECargoPaymentMethod.None, Payment.PaymentMethod);
			AssertEquals(false, Payment.IsNoneInfo.ReadOnly);
			Payment.PaymentMethod = UPECargoPaymentMethod.EFT;
			Assert("Should be false", !Payment.IsNone);
			AssertEquals(true, Payment.IsNoneInfo.ReadOnly);
		}

		public void TestPaymentMethod()
		{
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			Assert(Payment.IsNone);
			AssertEquals("PaymentMethodChanged event should not be fired", null, LastPaymentMethodChangedEventArgs);
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.Account;
			Assert(Payment.IsAccount);
			AssertNotNull("PaymentMethodChanged event should be fired", LastPaymentMethodChangedEventArgs);
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.BPay;
			Assert(Payment.IsBPay);
			AssertNotNull("PaymentMethodChanged event should be fired", LastPaymentMethodChangedEventArgs);
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.Cheque;
			Assert(Payment.IsCheque);
			AssertNotNull("PaymentMethodChanged event should be fired", LastPaymentMethodChangedEventArgs);
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.CreditCard;
			Assert(Payment.IsCreditCard);
			AssertNotNull("PaymentMethodChanged event should be fired", LastPaymentMethodChangedEventArgs);
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.EFT;
			Assert(Payment.IsEFT);
			AssertNotNull("PaymentMethodChanged event should be fired", LastPaymentMethodChangedEventArgs);
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.Nett7Day;
			Assert(Payment.IsNett7Day);
			AssertNotNull("PaymentMethodChanged event should be fired", LastPaymentMethodChangedEventArgs);
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.Other;
			Assert(Payment.IsOther);
			AssertNotNull("PaymentMethodChanged event should be fired", LastPaymentMethodChangedEventArgs);
			LastPaymentMethodChangedEventArgs = null;
			Payment.PaymentMethod = UPECargoPaymentMethod.PurchaseOrder;
			Assert(Payment.IsPurchaseOrder);
			AssertNotNull("PaymentMethodChanged event should be fired", LastPaymentMethodChangedEventArgs);
		}

		[TestDate(2005, 10, 3)]
		public void TestPaymentMethodAndPaymentDateIsStored()
		{
			AssertEquals((int)UPECargoPaymentMethod.None, Callout.CurrentQueue.P4_CustomDecimal2.ToZInt());
			AssertEquals(ZDateTime.Empty, Callout.PaymentDate);
			Payment.PaymentMethod = UPECargoPaymentMethod.Account;
			AssertEquals((int)UPECargoPaymentMethod.Account, Callout.CurrentQueue.P4_CustomDecimal2.ToZInt());
			AssertEquals(ZDateTime.Now, Callout.PaymentDate);
			Payment.PaymentMethod = UPECargoPaymentMethod.Cheque;
			AssertEquals((int)UPECargoPaymentMethod.Cheque, Callout.CurrentQueue.P4_CustomDecimal2.ToZInt());
			AssertEquals(ZDateTime.Now, Callout.PaymentDate);
			Payment.PaymentMethod = UPECargoPaymentMethod.None;
			AssertEquals("Delivery date should be empty again when payment method set to none", ZDateTime.Empty, Callout.PaymentDate);
		}

		public void TestPaymentMethodChanged_Cancel()
		{
			CancelNextPaymentMethodChangedEvent = false;
			Payment.PaymentMethod = UPECargoPaymentMethod.Account;
			AssertEquals("Should change to Account initially for the test", true, Payment.IsAccount);
			Callout.CurrentQueue.HasChanges = false;
			CancelNextPaymentMethodChangedEvent = true;
			Payment.PaymentMethod = UPECargoPaymentMethod.Nett7Day;
			AssertEquals("Setting the payment method should be cancelled", false, Payment.IsNett7Day);
			AssertEquals("Setting the payment method should be cancelled", true, Payment.IsAccount);
			AssertEquals("If cancelled, HasChanges has to be false", false, Callout.CurrentQueue.HasChanges);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Payment;
		}

		void CalloutPayment_PaymentMethodChanged(object sender, PaymentMethodChangedEventArgs e)
		{
			LastPaymentMethodChangedEventArgs = e;
			if (CancelNextPaymentMethodChangedEvent)
			{
				e.Cancel = true;
			}
		}

		bool CancelNextPaymentMethodChangedEvent;
		PaymentMethodChangedEventArgs LastPaymentMethodChangedEventArgs;
		CalloutPayment Payment
		{
			get
			{
				if (fPayment == null)
				{
					fPayment = new CalloutPayment(Callout);
					fPayment.PaymentMethodChanged += new PaymentMethodChangedEventHandler(CalloutPayment_PaymentMethodChanged);
				}

				return fPayment;
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

		UPERateTransportProvider CODPostcodeTransportProvider
		{
			get
			{
				UPERateTransportProvider result = UPERateTransportProvider.LoadCODPostcodeTransportProvider(Factory);
				if (result == null)
				{
					result = Factory.New<UPERateTransportProvider>();
					result.TP_OH_RelatedParty = CODPostcodeTransportOrg.PK;
				}

				return result;
			}
		}

		OrgHeader CODPostcodeTransportOrg
		{
			get
			{
				OrgHeader result = OrgHeader.LoadFromCode(Factory, UPERateTransportProvider.CODPostcodeTransportOrgCode);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<OrgHeader>();
					result.OH_Code = UPERateTransportProvider.CODPostcodeTransportOrgCode;
				}

				return result;
			}
		}

		CalloutPayment fPayment;
		Callout fCallout;
		#endregion
	}
}
