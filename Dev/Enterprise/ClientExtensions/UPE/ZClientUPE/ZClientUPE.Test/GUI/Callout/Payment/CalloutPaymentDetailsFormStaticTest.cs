using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class CalloutPaymentDetailsFormStaticTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestNew()
		{
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.Account, typeof(CalloutAccountPaymentForm), typeof(CalloutAccountPayment));
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.BPay, typeof(CalloutBPayPaymentForm), typeof(CalloutBPayPayment));
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.Cheque, typeof(CalloutOtherPaymentForm), typeof(CalloutChequePayment));
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.CreditCard, typeof(CalloutCreditCardPaymentForm), typeof(CalloutCreditCardPayment));
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.EFT, typeof(CalloutEFTPaymentForm), typeof(CalloutEFTPayment));
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.Nett7Day, typeof(CalloutNett7DayPaymentForm), typeof(CalloutNett7DayPayment));
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.None, null, null);
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.Other, typeof(CalloutOtherPaymentForm), typeof(CalloutOtherPayment));
			AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod.PurchaseOrder, typeof(CalloutPOPaymentForm), typeof(CalloutPOPayment));
		}

		void AssertCalloutPaymentDetailsForm(UPECargoPaymentMethod method, Type expectedFormType, Type expectedBusinessEntityType)
		{
			using (CalloutPaymentDetailsForm form = CalloutPaymentDetailsForm.New(Callout, method))
			{
				if (expectedFormType == null)
				{
					AssertNull("should return null", form);
				}
				else
				{
					AssertEquals("Incorrect type", expectedFormType, form.GetType());
					AssertEquals("Incorrect type", expectedBusinessEntityType, form.BusinessEntity.GetType());
				}
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
	}
}
