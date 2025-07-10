using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Business;

namespace Enterprise.Client.IFC
{
	public class IFCStorageDocsValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			IFCStorageDocsValueObjectDataAdapterTestClass dataAdapter = new IFCStorageDocsValueObjectDataAdapterTestClass(Factory.New(typeof(CommonShipment)));
			AssertEquals("Export Format Type should be PDF", OutputFormatType.PDF, dataAdapter.ExportFormatTypeExposed);
		}

		class IFCStorageDocsValueObjectDataAdapterTestClass : IFCStorageDocsValueObjectDataAdapter
		{
			public IFCStorageDocsValueObjectDataAdapterTestClass(BusinessObject parentBizObj) : base(parentBizObj)
			{
			}

			public OutputFormatType ExportFormatTypeExposed
			{
				get
				{
					return base.ExportFormatType;
				}
			}
		}
	}
}
