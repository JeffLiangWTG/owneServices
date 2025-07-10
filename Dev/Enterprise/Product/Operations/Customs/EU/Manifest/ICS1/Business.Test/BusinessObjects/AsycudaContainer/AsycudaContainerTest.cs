using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Latvia, EUManifestTypes.Codes.ICS, ApplicationCodeTypeList.Codes.ShippingLine);
			header.SuspendCheckBusinessObjectType();
			return header.Containers.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			applicationContext = ApplicationBusinessProviderTestContext.CreateDefaultIcsContext();
		}

		protected override void TearDown()
		{
			base.TearDown();

			applicationContext.Dispose();
		}

		ApplicationBusinessProviderTestContext applicationContext;
	}
}
