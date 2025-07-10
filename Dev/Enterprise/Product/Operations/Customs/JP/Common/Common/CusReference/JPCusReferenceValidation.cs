using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Common;

public class JPCusReferenceValidation(CusReference parent) : CusReferenceValidation(parent)
{
	protected override void CheckCFR_Reference()
	{
		base.CheckCFR_Reference();
		MandatoryValidation.CheckEntered(Parent.CFR_ReferenceInfo);
	}

	new CusReference Parent => (CusReference)base.Parent;
}
