namespace Enterprise.Customs.AU.Module
{
	public class CusCalculationRulesFilterBusinessObject : Customs.Module.CusCalculationRulesFilterBusinessObject
	{
		protected override Customs.Module.CusCalculationRulesFilterLookups GetNewLookups()
		{
			return new CusCalculationRulesFilterLookups(this);
		}
	}
}
