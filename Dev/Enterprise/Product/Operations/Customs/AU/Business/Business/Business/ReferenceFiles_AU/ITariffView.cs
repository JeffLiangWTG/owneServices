using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ITariffView
	{
		ZString ZZ1_TariffCode { get; }
		ZString ZZ1_TariffCodeForDisplay { get; }
		ZString ZZ1_Description { get; }
		ZString ZZ1_ZZ8_UQ1 { get; }
		ZString ZZ1_ZZ8_UQ2 { get; }

		// for compatability with old tariff classes
		bool HasChildren { get; }
	}
}
