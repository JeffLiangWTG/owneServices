using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestPersons()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<CusPersonCollection<CusPerson, AsycudaManifestHeader>>(header.Persons);
		}

		public void TestDefaultCountryCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("Header.AMA_RN_NKCountry", "IE", header.AMA_RN_NKCountry);
		}

		public void TestCustomsReferenceCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(0, header.CustomsReferenceCollection.Count);
			var ncts = header.CustomsReferenceCollection.AddNew();
			AssertEquals(1, header.CustomsReferenceCollection.Count);
		}

		public void TestTransitReferenceCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(0, header.TransitDeclarationCollection.Count);
			var trans = header.TransitDeclarationCollection.AddNew();
			AssertEquals(1, header.TransitDeclarationCollection.Count);
		}

		public void TestDefaultAMA_ManifestType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("Header.AMA_ManifestType", "PBN", header.AMA_ManifestType);
		}

		public void TestIsEmptyVehicle()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.IsEmptyVehicle = true;
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			Assert(header.IsEmptyVehicle);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header;
		}
	}
}
