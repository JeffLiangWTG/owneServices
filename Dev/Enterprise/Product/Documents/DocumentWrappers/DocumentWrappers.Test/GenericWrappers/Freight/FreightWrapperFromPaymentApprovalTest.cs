using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromPaymentApproval))]
	sealed class FreightWrapperFromPaymentApprovalTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			var wrapper = new FreightWrapperFromPaymentApproval(paymentApproval, Factory);
			AssertEquals("TrackingBusinessObjectPK", paymentApproval.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromPaymentApproval(paymentApproval, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<APPaymentApprovalWithAuthorisation>();
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
		}

		PaymentApprovalWithAuthorisation paymentApproval;

		#endregion
	}
}
