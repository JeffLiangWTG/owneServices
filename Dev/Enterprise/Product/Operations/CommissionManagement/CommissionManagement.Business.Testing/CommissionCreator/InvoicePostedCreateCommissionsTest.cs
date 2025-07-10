using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class InvoicePostedCreateCommissionsTest : TestCaseWithFactory
	{
		public void TestAddCommissionCalculationQueueItem()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var invoice1999 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1999.AH_PostDate = new ZDateTime(1999, 1, 1);

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.RunCommissionCreationInBackground.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var createCommissions = new InvoicePostedCreateCommissions(invoice1999);
				createCommissions.PostQueueItemOrCreateCommissions();
			}

			var queueItem = Factory.LoadTop1<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_AH, invoice1999.PK));

			AssertNotNull("Should be a queue item for the job", queueItem);
			AssertEquals("Operation on Queue item", OrgCommissionCalculationQueueOperationCodeList.Codes.Creation, queueItem.CAQ_Operation);
		}
	}
}
