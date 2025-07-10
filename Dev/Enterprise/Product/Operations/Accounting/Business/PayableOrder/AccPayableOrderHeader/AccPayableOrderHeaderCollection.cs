using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.PayableOrder
{
	[ModuleID(ModuleId.AccPayableOrder)]
	public class AccPayableOrderHeaderCollection : ActiveBusinessObjectCollection<AccPayableOrderHeader>
	{
		public AccPayableOrderHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public AccPayableOrderHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

