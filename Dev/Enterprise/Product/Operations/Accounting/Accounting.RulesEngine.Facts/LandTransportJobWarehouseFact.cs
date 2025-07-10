using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Business.JobBillingDefaulting.Facts;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class LandTransportJobWarehouseFact : ILandTransportJobWarehouseFact
	{
		public LandTransportJobWarehouseFact(IJobInvoicingPlugIn jobInvoicingPlugIn, BusinessObject warehouseOrder)
		{
			Argument.NotNull(jobInvoicingPlugIn, nameof(jobInvoicingPlugIn));

			PK = Guid.NewGuid();
			Warehouse = new FactLeftJoin<IWarehouseFact>(null);

			if (!(warehouseOrder is IWhsOrder))
			{
				return;
			}

			var factory = jobInvoicingPlugIn.Factory;
			var warehouse = factory.Load<IWhsWarehouse>(((IWhsOrder)warehouseOrder).WD_WW_Whs);
			var transportJobDocAddress = JobDocAddress.Load(warehouseOrder, MasterFiles.Integration.DocAddressType.TransportCompanyDocumentaryAddress);
			var warehouseFact = warehouse == null ? null : new WarehouseFact(warehouse.PK.ToGuid(), warehouse.WW_WarehouseCode);

			Warehouse = new FactLeftJoin<IWarehouseFact>(warehouseFact);
			TransportClientCode = transportJobDocAddress?.Organisation?.OH_Code;
		}

		public Guid PK { get; }

		public FactLeftJoin<IWarehouseFact> Warehouse { get; }

		public string TransportClientCode { get; }
	}
}
