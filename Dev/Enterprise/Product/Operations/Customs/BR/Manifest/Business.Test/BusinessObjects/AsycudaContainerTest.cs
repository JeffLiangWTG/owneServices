using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	public class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTareWeight()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_TareWeight = 1M;
			refContainer.RC_Code = "CNTR01";

			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "DANU1234567";
			container.ACN_RC_ContainerType = refContainer.PK;
			AssertEquals("TareWeight should be", 1M, container.TareWeight);
		}

		public void TestIAsycudaContainer()
		{
			var asycudaContainer = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.BRManifest.IAsycudaContainer>();
			AssertType<AsycudaContainer>(asycudaContainer);
			AssertType<AsycudaContainer>(Factory.Load(asycudaContainer.TablePrefix, asycudaContainer.PK));
		}

		public void TestHeader()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(container.Header);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.SuspendCheckBusinessObjectType();
			return header.Containers.AddNew();
		}

		#endregion
	}
}
