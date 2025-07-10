using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using AccGenericConsol = Enterprise.Accounting.Business.GenericConsol.GenericConsol;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[ModuleID(ModuleId.GenericConsol)]
	public class APInvoiceConsolCollection : MainFormGenericConsolCollection<AccGenericConsol>
	{
		public APInvoiceConsolCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ManyToManyShipmentCollection GetShipmentFromBusinessObject(AccGenericConsol businessObject)
		{
			ManyToManyShipmentCollection result = null;
			IJobCostingPlugIn consol = AccGenericConsol.LoadConsolBOFromGenericConsol(Factory, businessObject);
			if (consol != null && consol.CostSupporter != null)
			{
				result = (ManyToManyShipmentCollection)consol.CostSupporter.Shipments;
			}
			return result;
		}
	}
}
