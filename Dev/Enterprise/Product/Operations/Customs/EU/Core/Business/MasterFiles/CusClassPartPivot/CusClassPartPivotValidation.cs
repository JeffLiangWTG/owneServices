using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class CusClassPartPivotValidation : Customs.Business.BaseCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent)
		: base(parent)
		{
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		protected override void CheckCI_ZZF_NKTaxType()
		{
			base.CheckCI_ZZF_NKTaxType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_ZZF_NKTaxTypeInfo);
		}

		protected override void CheckCI_PrimaryPreference()
		{
			base.CheckCI_PrimaryPreference();
			ListValidation.MessageErrorIfInvalidCode(Parent.PreferenceCodeInfo);
		}

		protected override void ValidateClassTariff(ZPropertyInfo info)
		{
		}
	}
}
