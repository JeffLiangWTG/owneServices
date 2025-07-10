using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	#region IApportionedCharge Interface

	public interface IApportionedCharge
	{
		Job InvoicingJob { get; }
		ZDecimal ChargeableUnits { get; }
		ZDecimal GrossWeight { get; }
		ZDecimal GrossVolume { get; }
		ZBool IsUsedForApportionment { get; }
		ZInt ContainerCount { get; }
		ZDecimal TEUCount { get; }
		ZInt OuterPackTotal { get; }
		ZString CurrencyCode { get; set; }
		ZDecimal GetContainersCostShare();
		ZDecimal ExcessActualVolumeWeight { get; }
		ZDecimal ExcessChargeableVolumeWeight { get; }
	}

	public interface IApportionedChargesHeader
	{
		IApportionedCharge[] Charges { get; }
		AccChargeCode ChargeCode { get; }
		ZString ApportionmentMethod { get; }
		RefCurrency Currency { get; }
		bool IsChargeReadyToPost(IApportionedCharge charge);
		ZDecimal FreeSpace { get; }
	}

	public interface IApportionedChargesHeaderList : IFactoryProvider, ISecurityOverrideProviderSource
	{
		IApportionedChargesHeader[] Headers { get; }
	}
	#endregion
}
