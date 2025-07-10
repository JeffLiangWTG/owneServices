using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;
using AsycudaContainer = Enterprise.Customs.ASYCUDAManifest.Business.AsycudaContainer;

namespace Enterprise.Customs.FJ.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	public class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
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

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, ASYCUDAManifest.Business.AsycudaManifestHeader>);
	}
}
