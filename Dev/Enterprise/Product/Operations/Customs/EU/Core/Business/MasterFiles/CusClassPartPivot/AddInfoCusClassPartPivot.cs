using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class AddInfoCusClassPartPivot : AddInfo
	{
		public AddInfoCusClassPartPivot(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		public new AddInfoCusClassPartPivotValidation Validation
		{
			get { return (AddInfoCusClassPartPivotValidation)base.Validation; }
		}

		protected override EUAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusClassPartPivotValidation(this);
		}

		public new AddInfoCusClassPartPivotLookups Lookups
		{
			get { return (AddInfoCusClassPartPivotLookups)base.Lookups; }
		}

		protected override EUAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusClassPartPivotLookups(this);
		}
	}
}
