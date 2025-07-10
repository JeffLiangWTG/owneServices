using System.Linq;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
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

		public override string DuplicateAttributeForNoneHTI
		{
			get { return Res.GetString("1264420F-908C-47D1-AF65-70D2ADAD9682", "The combination of Classification Type and Related Organization should be unique for HTE and SHB classifications."); }
		}

		#region CheckCI_TariffNum

		protected override void CheckCI_TariffNum()
		{
			base.CheckCI_TariffNum();
			new TariffValidator(Parent.Factory).Validate(Parent.CI_TariffNumInfo, Parent.IsHTS, Parent.IsHTS ? 10 : 8, ZDateTime.Today, false, true);

			if (Parent.IsSIMADutyRequired && Parent.DutiesAndTaxes.Count == 0)
			{
				Parent.CI_TariffNumInfo.AddWarning(Res.GetString("FB8E3EF6-F52B-46C3-8597-E1A3BD055A23", "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes"));
			}
		}

		#endregion

		#region CheckCCA_SIMADumpingDesc

		protected void CheckCCA_SIMADumpingDesc()
		{
			if (Parent.CCA_SIMADumpingDesc.IsEmpty && Parent.DutiesAndTaxes.Find(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD || x.C1_TaxType == DutyAndTaxTypes.Codes.CVD).Any())
			{
				Parent.CCA_SIMADumpingDescInfo.AddMessageError(SimaMeasureIsMissingMessage);
			}
		}
		internal static string SimaMeasureIsMissingMessage => Res.GetString("3b3a3f2b-1989-4008-ab95-98471a796e9d", "SIMA Measure is missing – re-link to tariff number to re-instate.");

		public void ValidateCCA_SIMADumpingDesc()
		{
			ValidateCalculatedProperty(Parent.CCA_SIMADumpingDescInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCCA_SIMADumpingDesc();
		}
	}
}
