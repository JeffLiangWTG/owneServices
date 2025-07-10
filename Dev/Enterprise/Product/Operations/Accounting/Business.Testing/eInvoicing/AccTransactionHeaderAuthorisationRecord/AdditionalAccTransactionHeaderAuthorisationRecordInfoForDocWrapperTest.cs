using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	public class AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapperTest : TestCaseWithFactory
	{
		public virtual void TestProperties()
		{
			var additionalInfo = GetAdditionalInfoForDocWrapper();
			AssertBusinessName(additionalInfo);
			AssertLocationName(additionalInfo);
			AssertAddress(additionalInfo);
			AssertDistrict(additionalInfo);
			AssertEInvoicePaymentMethod(additionalInfo);
			AssertOriginalTransactionReferenceNumber(additionalInfo);
		}

		protected virtual void AssertBusinessName(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.BusinessName), string.Empty, info.BusinessName);
		protected virtual void AssertLocationName(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.LocationName), string.Empty, info.LocationName);
		protected virtual void AssertAddress(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.Address), string.Empty, info.Address);
		protected virtual void AssertDistrict(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.District), string.Empty, info.District);
		protected virtual void AssertEInvoicePaymentMethod(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.EInvoicePaymentMethod), string.Empty, info.EInvoicePaymentMethod);
		protected virtual void AssertOriginalTransactionReferenceNumber(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) =>
			AssertEquals(nameof(info.OriginalTransactionReferenceNumber), string.Empty, info.OriginalTransactionReferenceNumber);
		protected virtual AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper GetAdditionalInfoForDocWrapper() =>
			new AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper();
	}
}
