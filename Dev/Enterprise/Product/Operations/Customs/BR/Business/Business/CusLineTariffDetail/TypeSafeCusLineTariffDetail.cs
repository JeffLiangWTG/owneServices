namespace Enterprise.Customs.BR.Business;

public partial class CusLineTariffDetail : AutoBRCusLineTariffDetail
{
	#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

	public new CusLineTariffDetail Clone() => (CusLineTariffDetail)base.Clone();

	public new CusLineTariffDetailLookups Lookups => (CusLineTariffDetailLookups)base.Lookups;

	public new CusLineTariffDetailValidation Validation => (CusLineTariffDetailValidation)base.Validation;

	#endregion

	#region Implementation

	#region protected override

	protected override Customs.Business.CusLineTariffDetailLookups GetNewLookups() => new CusLineTariffDetailLookups(this);

	protected override Customs.Business.CusLineTariffDetailValidation GetNewValidation() => new CusLineTariffDetailValidation(this);

	#endregion

	#endregion
}
