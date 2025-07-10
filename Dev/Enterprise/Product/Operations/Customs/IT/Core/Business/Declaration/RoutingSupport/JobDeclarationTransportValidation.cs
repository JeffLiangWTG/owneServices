using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobDeclarationTransportValidation : EU.Business.Declaration.JobDeclarationTransportValidation
{
	public JobDeclarationTransportValidation(Transport transport) : base(transport)
	{
	}

	protected override void CheckJW_RL_NKDiscPort()
	{
		base.CheckJW_RL_NKDiscPort();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.JW_RL_NKDiscPortInfo);
		CheckConsecutiveDiscPortsHaveSameCountryCode();
	}

	protected override void CheckJW_LegOrder()
	{
		base.CheckJW_LegOrder();

		MandatoryValidation.MessageErrorIfIsZero(Parent.JW_LegOrderInfo);
		CheckDuplicateLegOrder();
		CheckLegOrderSequentiality();
	}

	#region Implementation

	void CheckDuplicateLegOrder()
	{
		if (OtherParentTransports.Any(x => x.JW_LegOrder == LegOrder))
		{
			Parent.JW_LegOrderInfo.AddMessageError(ValidationCaptions.TransportCaptions.LinesAreEnteredWithSameLegOrder);
		}
	}

	void CheckConsecutiveDiscPortsHaveSameCountryCode()
	{
		var discPortCountryCode = GetCountryCode(Parent.JW_RL_NKDiscPort);

		if (discPortCountryCode.IsEmpty || LegOrder == 1)
		{
			return;
		}

		if (OtherParentTransports.Any(x => IsPreceding(x) && HaveSameDiscPort(x)))
		{
			Parent.JW_RL_NKDiscPortInfo.AddMessageError(ValidationCaptions.TransportCaptions.ConsecutiveLinesAreEnteredWithSameCountry);
		}

		ZString GetCountryCode(ZString unlocoCode) => unlocoCode.SubstringSafe(0, 2);
		bool HaveSameDiscPort(Transport transport) => GetCountryCode(transport.JW_RL_NKDiscPort) == discPortCountryCode;
	}

	void CheckLegOrderSequentiality()
	{
		if (LegOrder > 1 && !OtherParentTransports.Any(x => IsPreceding(x)))
		{
			Parent.JW_LegOrderInfo.AddMessageError(ValidationCaptions.TransportCaptions.LegOrderShouldBeSequential);
		}
	}

	bool IsPreceding(Transport transport)
	{
		return transport.JW_LegOrder == LegOrder - 1;
	}

	IEnumerable<Transport> OtherParentTransports => (otherParentTransportsCached ?? (otherParentTransportsCached = new CachedProperty<IEnumerable<Transport>>(Parent.Factory, () => Parent.OtherParentTransports))).Value;
	CachedProperty<IEnumerable<Transport>> otherParentTransportsCached;

	byte LegOrder => Parent.JW_LegOrder;

	#endregion
}
