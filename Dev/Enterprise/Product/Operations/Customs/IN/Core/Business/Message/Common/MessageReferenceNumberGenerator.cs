using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.IN.Business;

public sealed class MessageReferenceNumberGenerator
{
	public MessageReferenceNumberGenerator(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	public string Generate(ZString messageType, ZString messageSubType, ZString messageOwner, ZGuid companyPk, ZDate date)
	{
		Argument.NotNullOrEmpty(messageType, nameof(messageType));
		Argument.NotNullOrEmpty(messageSubType, nameof(messageSubType));
		Argument.NotNullOrEmpty(messageOwner, nameof(messageOwner));
		if (companyPk.IsEmpty)
		{
			throw new ArgumentException("companyPk cannot be empty.", nameof(companyPk));
		}
		if (date.IsEmpty)
		{
			throw new ArgumentException("date cannot be empty.", nameof(date));
		}

		var fountainKey = $"INCustomsMessageNumber_{messageType}_{messageSubType}_{messageOwner}_{companyPk}_{Utils.GetIndianFinancialYear(date)}";
		return Env.NumberFountains.GetINCustomsMessageNumberFountain(fountainKey).GetNextFormatted(factory);
	}

	readonly BusinessObjectFactory factory;
}
