using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeBill : AutoCusDecHouseBill
	{
		#region Constructor

		protected TypeSafeBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new CusDecHouseBillLookups Lookups
		{
			get { return (CusDecHouseBillLookups)base.Lookups; }
		}

		public new CusDecHouseBillValidation Validation
		{
			get { return (CusDecHouseBillValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		Bill HouseBill
		{
			get { return (Bill)this; }
		}

		protected override Customs.Business.CusDecHouseBillLookups GetNewLookups()
		{
			return new CusDecHouseBillLookups(HouseBill);
		}

		protected override Customs.Business.CusDecHouseBillValidation GetNewValidation()
		{
			return new CusDecHouseBillValidation(HouseBill);
		}

		#endregion

		#endregion
	}
}
