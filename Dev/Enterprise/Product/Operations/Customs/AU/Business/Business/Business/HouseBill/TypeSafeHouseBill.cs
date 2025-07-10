using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class TypeSafeBill : Customs.Business.Bill
	{
		protected TypeSafeBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new BillLookups Lookups
		{
			get { return (BillLookups)base.Lookups; }
		}

		public new BillValidation Validation
		{
			get { return (BillValidation)base.Validation; }
		}
	}
}
