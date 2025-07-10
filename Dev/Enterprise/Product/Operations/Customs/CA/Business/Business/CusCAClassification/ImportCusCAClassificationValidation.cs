
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ImportCusCAClassificationValidation : CusCAClassificationValidation
	{
		public ImportCusCAClassificationValidation(AutoCusCAClassification parent)
			: base(parent)
		{
		}

		protected override void CheckCCA_AMMVPerUnit()
		{
			base.CheckCCA_AMMVPerUnit();
			if (!Parent.CCA_AMMVPercentage.IsEmpty && !Parent.CCA_AMMVPerUnit.IsEmpty)
			{
				Parent.CCA_AMMVPerUnitInfo.AddMessageError(Res.GetString("A6785B63-F7FE-47B0-9A52-2A46B9B16C86", "You can enter either Assist/AMMV Unit or Assist/AMMV Percent, not both."));
			}
		}

		protected override void CheckCCA_AMMVPercentage()
		{
			base.CheckCCA_AMMVPercentage();

			if (!Parent.CCA_AMMVPercentage.IsEmpty && !Parent.CCA_AMMVPerUnit.IsEmpty)
			{
				Parent.CCA_AMMVPercentageInfo.AddMessageError(Res.GetString("FE1E3E85-F469-4596-9BB3-61198688BC36", "You can enter either Assist/AMMV Unit or Assist/AMMV Percent, not both."));
			}
		}
	}
}
