namespace Enterprise.Customs.JP.AFR.Business
{
	public class OtherRelevantLawLookups : Customs.Business.CusCodeDataLookups
	{
		public OtherRelevantLawLookups(OtherRelevantLaw parent)
			: base(parent)
		{
		}

		public OtherRelevantLawsAndOrdinancesCodeList OtherRelevantLawsAndOrdinancesCodeList
		{
			get { return Factory.GetCachedValue<OtherRelevantLawsAndOrdinancesCodeList>(); }
		}
	}
}
