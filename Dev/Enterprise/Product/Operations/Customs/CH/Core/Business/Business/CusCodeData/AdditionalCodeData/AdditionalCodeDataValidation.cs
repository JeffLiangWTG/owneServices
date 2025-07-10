using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class AdditionalCodeDataValidation : CusCodeDataValidation
{
	public AdditionalCodeDataValidation(AdditionalCodeData parent)
		: base(parent)
	{
	}

	public new AdditionalCodeData Parent => (AdditionalCodeData)base.Parent;

	protected override void CheckCY_Code()
	{
	}
}
