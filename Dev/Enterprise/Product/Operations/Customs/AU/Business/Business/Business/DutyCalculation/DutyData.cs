using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDutyData
	{
		BusinessObjectFactory Factory { get; }
		ZDecimal CustomsFactor { get; }
		ZDateTime DateOfValuation { get; }
		ZDateTime EffectiveDutyDate { get; }
		AUAddInfo AddInfo { get; }
		ZString TariffNumber { get; }
		ZString StatCode { get; }
		ZString TreatmentCode { get; }
		ZDecimal Quantity { get; }
		ZString UnitOfQuantity { get; }
		ZBool IsNature20 { get; }
		Money CustomsValue { get; }

		Money Price { get; }
		Money TransportAndInsurance { get; }
		CurrencyConverter CurrencyConverter { get; }
	}
}
