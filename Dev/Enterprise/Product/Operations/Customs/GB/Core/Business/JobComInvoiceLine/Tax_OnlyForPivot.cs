using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.MasterFiles;

namespace Enterprise.Customs.GB.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
	public class Tax_OnlyForPivot : Tax_CusAddInfoOnlyForPIVOT, Integration.Customs.GB.ITax_OnlyForPivot
	{
		public Tax_OnlyForPivot(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new CusClassPartPivot Pivot => (CusClassPartPivot)base.Pivot;

		[List(nameof(Lookups) + "." + nameof(GBAddInfoTaxLookups.RateDutyList))]
		public override ZString G4_RateDuty
		{
			get => base.G4_RateDuty;
			set
			{
				base.G4_RateDuty = value;
				if (G4_RateDuty == TaxRateExciseDutyListImport.Codes.ExciseACustomsDutyReliefCpcIsUsedAndASuspensionOfExciseDutyOnTiedHydrocarbonOilsIsAlsoClaimedExdMustBeEnteredAsTheTaxRateOverrideCodeAnd000EnteredInTheAmountColumn)
				{
					G4_RateOverride = TaxRateExciseOverrideListImport.Codes.TheExciseDutyPayableIsBeingCalculatedByTheTrader;
				}
			}
		}

		protected override EUAddInfoTaxLookups GetNewLookups()
		{
			return new GBAddInfoTaxLookups(this);
		}

		public new GBAddInfoTaxLookups Lookups => (GBAddInfoTaxLookups)base.Lookups;

		protected override EUAddInfoTaxValidation GetNewValidation()
		{
			return new GBAddInfoTaxValidation(this);
		}
	}
}
