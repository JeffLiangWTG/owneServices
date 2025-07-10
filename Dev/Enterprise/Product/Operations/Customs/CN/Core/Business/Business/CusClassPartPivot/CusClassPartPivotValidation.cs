using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class CusClassPartPivotValidation : Customs.Business.BaseCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent)
			: base(parent)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDangerousGoodsDGSubs();
		}

		protected override void CheckCI_RN_NKCountryOfOrigin()
		{
			base.CheckCI_RN_NKCountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_RN_NKCountryOfOriginInfo);
		}

		protected override void CheckCI_RW_NKOriginState()
		{
			base.CheckCI_RW_NKOriginState();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_RW_NKOriginStateInfo);
		}

		protected override void CheckCI_RN_NKCountryOfExport()
		{
			base.CheckCI_RN_NKCountryOfExport();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_RN_NKCountryOfExportInfo);
		}

		protected override void CheckCI_TariffNum()
		{
			base.CheckCI_TariffNum();
			var targetInfo = Parent.CI_TariffNumInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			if (!Parent.CI_TariffNum.IsEmpty)
			{
				var tariff = Parent.UniversalTariff;
				if (tariff == null)
				{
					targetInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
				else
				{
					Parent.Details.Validation.ValidateCNC_GoodsSpecModel();
				}
			}
		}

		protected override void ValidateClassTariff(ZPropertyInfo info)
		{
		}

		public void ValidateDangerousGoodsDGSubs()
		{
			ValidateCalculatedProperty(Parent.DangerousGoodsDGSubsInfo);
		}
	}
}
