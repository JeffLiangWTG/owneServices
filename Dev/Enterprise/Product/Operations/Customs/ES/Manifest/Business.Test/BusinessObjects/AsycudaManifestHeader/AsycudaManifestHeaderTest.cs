using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestGetDefaultCountryCode()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
			AssertEquals(Core.Constants.CountryCodes.Spain, manifestHeader.GetDefaultCountryCode());
		}

		public void TestBills()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(manifestHeader.Bills);
		}

		public void TestPackedItemRelationship()
		{
			var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals("Should be RelationshipType.Many", ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many, manifestHeader.PackedItemRelationship);
		}

		protected override BusinessObject GetNewBusinessObject() => CreateBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = CreateBusinessObject(factory);
			header.SuspendCheckBusinessObjectType();
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		AsycudaManifestHeader CreateBusinessObject(BusinessObjectFactory factory) => factory.NewWithValidTestData<AsycudaManifestHeader>();

		sealed class AsycudaManifestHeaderForTest : AsycudaManifestHeader
		{
			public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZString GetDefaultCountryCode() => base.GetDefaultCountryCode();
		}
	}
}
