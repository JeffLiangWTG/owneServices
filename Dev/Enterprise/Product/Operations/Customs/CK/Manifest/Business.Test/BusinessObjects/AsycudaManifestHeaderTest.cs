using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.CK.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestValidation()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<AsycudaManifestHeaderValidation>(header.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = CreateBusinessObject(factory);
			header.SuspendCheckBusinessObjectType();
			return header;
		}

		AsycudaManifestHeader CreateBusinessObject(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "C1234";
			return manifestHeader;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<ASYCUDAManifest.Business.AsycudaContainer, ASYCUDAManifest.Business.AsycudaManifestHeader>);
	}
}
