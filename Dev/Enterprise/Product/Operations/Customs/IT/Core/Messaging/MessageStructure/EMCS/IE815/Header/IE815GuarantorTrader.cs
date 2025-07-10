using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815GuarantorTrader
{
	const int MaxIterationNumber = 2;
	public IE815GuarantorTrader(IGuarantorTrader guarantorTrader, int numberIteration)
	{
		this.guarantorTrader = Argument.NotNull(guarantorTrader, "guarantorTrader");
		var numberIterationName = nameof(numberIteration);
		NumberIteration = Argument.GreaterThanOrEqualToZero(numberIteration, numberIterationName);
		if (numberIteration > MaxIterationNumber)
		{
			throw new ArgumentOutOfRangeException(numberIterationName, FormattableString.Invariant($"{numberIterationName} must be less than {MaxIterationNumber}"));
		}
	}
	readonly IGuarantorTrader guarantorTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldRules("C", "C053")]
	public ZString IterationType => "H";

	[MessageLayout(Order = 1)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("C", "C053")]
	public ZInt NumberIteration { get; }

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 13, true)]
	[MessageFieldRules("C", "C053", "C018", "R014")]
	public ZString TraderExciseNumber => guarantorTrader.TraderExciseNumber;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldRules("C", "C053", "C018", "R015")]
	public ZString VatNumber => guarantorTrader.VatNumber;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 182, false)]
	[MessageFieldRules("C", "C053", "C018", "R015")]
	public ZString TraderName => guarantorTrader.TraderName;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 65, false)]
	[MessageFieldRules("C", "C053", "C020", "C019")]
	public ZString StreetName => guarantorTrader.StreetName;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 11, false)]
	[MessageFieldRules("O", "C053", "C021")]
	public ZString StreetNumber => guarantorTrader.StreetNumber;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	[MessageFieldRules("C", "C053", "C021")]
	public ZString Postcode => guarantorTrader.Postcode;

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 50, false)]
	[MessageFieldRules("C", "C053", "C020", "C019")]
	public ZString City => guarantorTrader.City;

	[MessageLayout(Order = 9)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C053", "C012")]
	public ZString LanguageDescriptions => guarantorTrader.LanguageDescriptions;
}
