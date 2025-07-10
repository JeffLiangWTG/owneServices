using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Utility.Testing
{
	public class DummyJobParent : CommonShipment, IJobInvoicingPlugIn
	{
		public DummyJobParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new DummyParentJobInvoicingSupporter(this);
		}
	}

	class DummyParentJobInvoicingSupporter : CommonShipmentInvoicingSupporter
	{
		public DummyParentJobInvoicingSupporter(DummyJobParent parent)
			: base(parent)
		{
		}

		protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
		{
			return Env.Security.AgencyBillContainers;
		}

		public override bool EditSecurityLock
		{
			get { return true; }
		}

		public override ZString EditSecurityMessage
		{
			get { return "Boop Boop Be Doop"; }
		}
	}
}
