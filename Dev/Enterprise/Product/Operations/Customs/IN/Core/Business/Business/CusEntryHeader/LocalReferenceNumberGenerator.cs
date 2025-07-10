using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.IN.Business;

public sealed class LocalReferenceNumberGenerator
{
	public LocalReferenceNumberGenerator(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	public string Generate(string shipmentType, ZDate date)
	{
		Argument.NotNullOrEmpty(shipmentType, nameof(shipmentType));
		if (date.IsEmpty)
		{
			throw new ArgumentException("date cannot be empty.", nameof(date));
		}

		var fountainKey = $"{shipmentType}_{Utils.GetIndianFinancialYear(date)}";
		return Env.NumberFountains.GetINLocalReferenceNumberFountain(fountainKey).GetNextFormatted(factory);
	}

	readonly BusinessObjectFactory factory;
}
