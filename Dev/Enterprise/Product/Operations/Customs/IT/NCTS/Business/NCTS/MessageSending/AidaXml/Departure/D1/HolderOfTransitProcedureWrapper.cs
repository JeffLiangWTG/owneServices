using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class HolderOfTransitProcedureWrapper : IHolderOfTransitProcedure
{
	public HolderOfTransitProcedureWrapper(JobDocAddress jobDocAddress, string declarationType)
	{
		this.jobDocAddress = Argument.NotNull(jobDocAddress, nameof(JobDocAddress));
		this.orgHeader = Argument.NotNull(jobDocAddress.Organisation, nameof(jobDocAddress.Organisation));
		this.declarationType = declarationType;

		lazyIdentificationNumber = new Lazy<string>(GetIdentificationNumber);
		lazyTirHolderIdentificationNumber = new Lazy<string>(GetTirHolderIdentificationNumber);
		lazyAddress = new Lazy<IAddress>(GetAddress);
		lazyEoriOrTcuTraderWrapper = new Lazy<ITrader>(GetEoriOrTcuTraderWrapper);
	}

	string IHolderOfTransitProcedure.IdentificationNumber => lazyIdentificationNumber.Value;
	readonly Lazy<string> lazyIdentificationNumber;

	string IHolderOfTransitProcedure.TirHolderIdentificationNumber => lazyTirHolderIdentificationNumber.Value;
	readonly Lazy<string> lazyTirHolderIdentificationNumber;

	IAddress IHolderOfTransitProcedure.Address => lazyAddress.Value;
	readonly Lazy<IAddress> lazyAddress;

	string GetTirHolderIdentificationNumber()
	{
		if (declarationType != NctsPhase5DeclarationTypeList.Codes.TIR)
		{
			return null;
		}
		var tirCusCode = orgHeader.GetCusCode(NctsPhase5DeclarationTypeList.Codes.TIR);

		return tirCusCode == null
			? orgHeader.GetEoriCode(countryCode: true)
			: tirCusCode.OK_CustomsRegNo;
	}

	string GetIdentificationNumber()
	{
		var trader = lazyEoriOrTcuTraderWrapper.Value;
		return trader.IdentificationNumber;
	}

	IAddress GetAddress()
	{
		var trader = lazyEoriOrTcuTraderWrapper.Value;
		return trader.Address;
	}

	ITrader GetEoriOrTcuTraderWrapper()
	{
		ITrader trader = new EoriOrTcuTraderWrapper(jobDocAddress);
		return trader;
	}

	readonly JobDocAddress jobDocAddress;
	readonly OrgHeader orgHeader;
	readonly string declarationType;
	readonly Lazy<ITrader> lazyEoriOrTcuTraderWrapper;
}
