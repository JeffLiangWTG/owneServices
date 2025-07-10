using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingVATCollection : NonPersistentBusinessObjectCollection<GuidedDecisionMakingVAT>
	{
		public GuidedDecisionMakingVATCollection(GuidedDecisionMakingBasic gDMBasic) : base(gDMBasic.Factory)
		{
			this.gDMBasic = Argument.NotNull(gDMBasic, nameof(gDMBasic));
			Load();
		}

		public override void Load()
		{
			RemoveAndDeleteAll();
			if (gDMBasic.EffectiveDate.IsValid && gDMBasic.Tariff is TariffView)
			{
				var vats = GetVATApplicabilities();
				var gdmVATs = new List<GuidedDecisionMakingVAT>();
				var matchedVATCodesNumber = vats.Count(v => v.ZX5_ZZF_NKTaxOrFeeCode == gDMBasic.CapturedVATCode);
				vats.ForEach(x =>
				{
					var gdmVAT = new GuidedDecisionMakingVAT(gDMBasic);
					var taxOrFee = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(gDMBasic.DataGrouping, x.ZX5_ZZF_NKTaxOrFeeCode, gDMBasic.EffectiveDate);
					gdmVAT.AdditionalCode = x.ZX5_AdditionalCode;
					gdmVAT.Category = x.ZX5_VATCategory;
					gdmVAT.VATCode = x.ZX5_ZZF_NKTaxOrFeeCode;
					gdmVAT.Description = taxOrFee?.ZZF_Description ?? ZString.Empty;
					gdmVAT.AdditionalCodeDescription = x.ZX5_Description;
					gdmVAT.VATRateValue = taxOrFee?.ZZF_Value ?? ZDecimal.Zero;
					gdmVAT.IsTicked = matchedVATCodesNumber != 0 && gdmVAT.VATCode == gDMBasic.CapturedVATCode && (matchedVATCodesNumber == 1 || gDMBasic.CapturedSupplementaryCodes.Contains(gdmVAT.AdditionalCode));
					gdmVATs.Add(gdmVAT);
				});

				AddExtraVATs(gdmVATs);

				gdmVATs.OrderBy(v => v.VATRateValue).ThenBy(v => v.VATCode).ThenBy(v => v.AdditionalCode).ForEach(v => this.Add(v));
			}
		}

		IEnumerable<VATApplicabilityView> GetVATApplicabilities() => GetVATApplicabilitiesCore();

		protected virtual IEnumerable<VATApplicabilityView> GetVATApplicabilitiesCore() => gDMBasic.Tariff.GetEffectiveVATApplicabilities(gDMBasic.EffectiveDate)
					.Where(v => v.ZX5_ZZZ_NKDataGrouping == gDMBasic.DataGrouping)
					.ToList();

		protected virtual void AddExtraVATs(List<GuidedDecisionMakingVAT> gdmVATs) { }

		protected readonly GuidedDecisionMakingBasic gDMBasic;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GuidedDecisionMakingVAT(gDMBasic);
		}
	}
}
