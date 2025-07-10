using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.Warehouse
{
	public class DocWarehouseAdHocServiceJob : DocBaseWrapperWithJobHeader
	{
		DocWarehouseAdHocServiceJob(WhsAdHocServiceJob adHocServiceJob, BusinessObjectFactory factory) : base(adHocServiceJob, factory)
		{
		}

		public static DocWarehouseAdHocServiceJob New(WhsAdHocServiceJob adHocServiceJob, BusinessObjectFactory factoryToWrap)
		{
			return (adHocServiceJob == null) ? null : new DocWarehouseAdHocServiceJob(adHocServiceJob, factoryToWrap);
		}

		WhsAdHocServiceJob AdHocServiceJob
		{
			get { return (WhsAdHocServiceJob)WrappedObject; }
		}

		public ZString CustomerReferenceNumber
		{
			get { return AdHocServiceJob.WSJ_CustomerReference; }
		}

		public override DocJobHeader JobHeader
		{
			get { return DocJobHeader.New(AdHocServiceJob.JobHeader, Factory); }
		}
	}
}
