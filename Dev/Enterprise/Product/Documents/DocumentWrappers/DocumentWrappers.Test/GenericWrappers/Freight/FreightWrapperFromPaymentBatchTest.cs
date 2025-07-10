using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromPaymentBatch))]
	sealed class FreightWrapperFromPaymentBatchTest : FreightWrapperTest
	{
		protected override BusinessObject GetNewBusinessObjectToWrap() => GetNewAccPaymentBatch();
		protected override bool IsCarrierUsed => false;
		protected override GenericWrapper GetSetupWrapperForDefaultFormatting() => new FreightWrapperFromPaymentBatch(GetNewAccPaymentBatch(), Factory);

		public override void TestTrackingBusinessObjectPK()
		{
			var batch = GetNewAccPaymentBatch();
			var wrapper = new FreightWrapperFromPaymentBatch(batch, Factory);
			AssertEquals(batch.PK, wrapper.TrackingBusinessObjectPK);
		}

		AccPaymentBatch GetNewAccPaymentBatch() => Factory.New<AccPaymentBatch>();
	}
}
