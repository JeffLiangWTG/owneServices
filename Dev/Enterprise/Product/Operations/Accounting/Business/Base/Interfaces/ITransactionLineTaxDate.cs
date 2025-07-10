using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface ITransactionLineTaxDate
	{
		ZGuid AL_AC { get; }
		AccChargeCode ChargeCode { get; }
		AccTaxRate TaxRate { get; }
		ZDate AL_TaxDate { get; }
	}
}
