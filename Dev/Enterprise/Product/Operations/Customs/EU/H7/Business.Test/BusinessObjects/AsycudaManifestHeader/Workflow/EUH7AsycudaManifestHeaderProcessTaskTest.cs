using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(EUH7AsycudaManifestHeaderProcessTask))]
	sealed class EUH7AsycudaManifestHeaderProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
			return asycudaManifestHeader.WorkflowItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Factory.New<EUH7AsycudaManifestHeaderProcessTask>();
		}
	}
}
