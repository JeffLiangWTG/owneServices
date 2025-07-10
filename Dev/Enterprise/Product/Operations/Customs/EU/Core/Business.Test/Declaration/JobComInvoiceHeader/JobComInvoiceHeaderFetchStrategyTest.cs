using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.FetchStrategies;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobComInvoiceHeaderFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoadChildEditableObjects()
		{
			var invoice = Factory.New<InvoiceHeaderWithDescription>();
			var strategy = new JobComInvoiceHeaderFetchStrategy(invoice);

			strategy.FetchForLoadChildEditableObjects();
			AssertNotEquals("Should have fetch hints on CusSupportingInfo.", 0, Factory.ActiveFetchHintsForTable(CusSupportingInfoSchema.Constants.TableName));
			AssertNotEquals("Should have fetch hints on InvoiceHeaderDescription.", 0, Factory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
		}

		public class InvoiceHeaderWithDescription : JobComInvoiceHeader
		{
			public InvoiceHeaderWithDescription(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZBool GetSupportHeaderDescriptionCore()
			{
				return true;
			}
		}
	}
}
