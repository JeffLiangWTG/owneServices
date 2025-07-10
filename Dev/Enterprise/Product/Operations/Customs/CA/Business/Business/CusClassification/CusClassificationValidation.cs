using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusClassificationValidation : Customs.Business.CusClassificationValidation
	{
		public CusClassificationValidation(CusClassification parent)
			: base(parent)
		{
		}

		public new CusClassification Parent
		{
			get { return (CusClassification)base.Parent; }
		}

		protected override void CheckCC_TariffNum()
		{
			base.CheckCC_TariffNum();
			new TariffValidator(Parent.Factory).Validate(Parent.CC_TariffNumInfo, Parent.IsHTS, Parent.IsHTS ? 10 : 8, ZDateTime.Today, false);

			if (Parent.IsSIMADutyRequired && Parent.DutiesAndTaxes.Count == 0)
			{
				Parent.CC_TariffNumInfo.AddWarning(Res.GetString("A0AEBDDE-3BA6-4994-8032-1D0603454713", "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes"));
			}
		}
	}
}
