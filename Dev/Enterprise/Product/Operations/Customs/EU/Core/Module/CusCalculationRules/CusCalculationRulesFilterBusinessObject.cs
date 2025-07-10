namespace Enterprise.Customs.EU.Module
{
	public class CusCalculationRulesFilterBusinessObject : Customs.Module.CusCalculationRulesFilterBusinessObject
	{
		protected override Customs.Module.CusCalculationRulesFilterLookups GetNewLookups()
		{
			return new CusCalculationRulesFilterLookups(this);
		}

		public new CusCalculationRulesFilterLookups Lookups => (CusCalculationRulesFilterLookups)base.Lookups;
	}
}
