using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWarehouseAdHocServiceJob))]
	sealed class DocWarehouseAdHocServiceJobTest : DocumentWrapperTestCase
	{
		protected override void SetUp()
		{
			AdHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			AdHocServiceJobWrapper = CreateWhsAdhocServiceJobWrapper(AdHocServiceJob);
			base.SetUp();
		}

		DocWarehouseAdHocServiceJob CreateWhsAdhocServiceJobWrapper(WhsAdHocServiceJob adHocServiceJob)
		{
			return DocWarehouseAdHocServiceJob.New(adHocServiceJob, Factory);
		}

		WhsAdHocServiceJob AdHocServiceJob;
		DocWarehouseAdHocServiceJob AdHocServiceJobWrapper;

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { AdHocServiceJobWrapper };
		}

		#endregion
	}
}
