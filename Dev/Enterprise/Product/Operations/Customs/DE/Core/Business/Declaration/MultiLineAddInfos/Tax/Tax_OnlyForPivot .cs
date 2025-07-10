using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.DE.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
	public class Tax_OnlyForPivot : Tax_CusAddInfoOnlyForPIVOT
	{
		public Tax_OnlyForPivot(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new CusClassPartPivot Pivot => (CusClassPartPivot)base.Pivot;

		protected override EUAddInfoTaxLookups GetNewLookups() => new DEAddInfoTaxLookups(this);

		public new DEAddInfoTaxLookups Lookups => (DEAddInfoTaxLookups)base.Lookups;

		[MaxLength(5)]
		public override ZString G4_Type
		{
			get => base.G4_Type;
			set => base.G4_Type = value;
		}
	}
}
