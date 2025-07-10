using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.Declaration;

[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
public class Tax_OnlyForPivot : Tax_CusAddInfoOnlyForPIVOT
{
	public Tax_OnlyForPivot(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
	{
	}

	public new CusClassPartPivot Pivot => (CusClassPartPivot)base.Pivot;

	protected override EUAddInfoTaxLookups GetNewLookups() => new ITAddInfoTaxLookups(this);

	public new ITAddInfoTaxLookups Lookups => (ITAddInfoTaxLookups)base.Lookups;

	protected override EUAddInfoTaxValidation GetNewValidation() => new ITAddInfoTaxValidation(this);

	public override ZString G4_Type
	{
		get => base.G4_Type;
		set
		{
			var oldValue = G4_Type;
			base.G4_Type = value;
			if (!IsCopying && oldValue != G4_Type)
			{
				EmptyPortRateIfNecessary();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(ITAddInfoTaxLookups.PortTaxRateList))]
	[ReadOnlyMember(nameof(PortTaxRateReadOnly))]
	public override ZString G4_PortTaxRate
	{
		get => base.G4_PortTaxRate;
		set => base.G4_PortTaxRate = value;
	}

	public bool PortTaxRateReadOnly => !IsPortTax;

	public bool IsPortTax => Factory.GetValue(ref isPortTaxCached, () =>
	{
		var portTaxRateCodesLoader = new PortTaxRateCodesLoader(Factory, ZDateTime.Now);
		var portTaxRateCodeResolver = new PortTaxRateCodeResolver(portTaxRateCodesLoader);
		return portTaxRateCodeResolver.IsPortTaxRateCode(G4_Type);
	});

	CachedProperty<bool> isPortTaxCached;

	void EmptyPortRateIfNecessary()
	{
		if (!IsPortTax)
		{
			G4_PortTaxRate = ZString.Empty;
		}
	}
}
