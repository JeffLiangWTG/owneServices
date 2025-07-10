using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaTransportMeans))]
	sealed class AsycudaTransportMeansTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCaption()
		{
			CombineAssertions(() =>
			{
				AssertCaption("Vehicle Registration", CusTransportMeans.TPM_IdentificationNumberInfo);
				AssertCaption("Trailer", CusTransportMeans.TPM_ReferenceNumberInfo);
				AssertCaption("Type of Identification", CusTransportMeans.TPM_TypeOfIdentificationInfo);
				AssertCaption("Type of means of Transport", CusTransportMeans.TPM_TypeOfTransportMeansInfo);
				AssertCaption("Nationality", CusTransportMeans.TPM_RN_NKTransportNationalityInfo);
			});
			void AssertCaption(string expectedValue, ZPropertyInfo propertyInfo) => AssertEquals(expectedValue, DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestMaxLength()
		{
			AssertEquals(15, CusTransportMeans.TPM_ReferenceNumberInfo.MaxLength);
		}

		public void TestShouldReturnAsycudaPackWhenTPMParentTableCodeIsPack()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var asycudaTransportMeans = pack.AsycudaTransportMeans.AddNew();

			AssertEquals(AsycudaPackSchema.Constants.Prefix, asycudaTransportMeans.TPM_ParentTableCode);
			AssertEquals(pack, asycudaTransportMeans.Pack);
		}

		public void TestShouldReturnAsycudaBillWhenTPMParentTableCodeIsBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaTransportMeans = bill.AsycudaTransportMeans.AddNew();

			AssertEquals(AsycudaBillSchema.Constants.Prefix, asycudaTransportMeans.TPM_ParentTableCode);
			AssertEquals(bill, asycudaTransportMeans.Bill);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObjectCore(factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectCore(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectCore(Factory);

		AsycudaTransportMeans GetNewBusinessObjectCore(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportModes.Road;
			var bill = header.Bills.AddNew();
			return bill.AsycudaTransportMeans.AddNew();
		}

		AsycudaTransportMeans CusTransportMeans => cusTransportMeans ??= GetNewBusinessObjectCore(Factory);
		AsycudaTransportMeans cusTransportMeans;
	}
}
