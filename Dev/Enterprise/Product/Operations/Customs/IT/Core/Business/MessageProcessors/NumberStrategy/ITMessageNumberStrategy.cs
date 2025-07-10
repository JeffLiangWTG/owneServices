using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

public class ITMessageNumberStrategy : IMessageNumberStrategy
{
	public ITMessageNumberStrategy(ICustomsMessageFountainProvider fountainProvider)
	{
		this.fountainProvider = Argument.NotNull(fountainProvider, nameof(fountainProvider));
	}

	readonly ICustomsMessageFountainProvider fountainProvider;

	public string GetMessageReferenceNumber()
	{
		CheckMatchingSystemAccount();

		var numberFountain = TryGetNumberFountain();
		var messageReferenceNumber = GenerateMessageReferenceNumber(numberFountain);
		return FormatMessageReferenceNumber(messageReferenceNumber);
	}

	#region Implementation

	string FormatMessageReferenceNumber(long number) => number.ToString().PadLeft(ProgressiveCodeLength, PaddingChar);

	void CheckMatchingSystemAccount()
	{
		if (fountainProvider.DeclarantTaxNumber.IsEmpty)
		{
			throw new InvalidOperationException(Res.GetString("0AB36059-FCF3-49C5-9ABC-152D8847F964", "Cannot get message reference number. Declarant node [{0}] does not match any registered account.", fountainProvider.Node));
		}
	}

	INumberFountainProxy TryGetNumberFountain() => fountainProvider.TryGetNumberFountain() ?? throw new NumberRangeNotSetUpException(Res.GetString("41216A49-3497-401A-8F93-02BC74AAB2E0", "Company {0}", GlbCompany.CurrentCompany.GC_Code), fountainProvider.FountainType, fountainProvider.FountainPrefix);

	long GenerateMessageReferenceNumber(INumberFountainProxy numberFountain)
	{
		try
		{
			return numberFountain.GetNext(Db.Connection);
		}
		catch (NumberFountainMaximumValueReachedException)
		{
			throw new NumberFountainMaximumValueReachedException(Res.GetString("381262D0-C715-45BD-B2A7-566D31CC6B87", "For the account {0} the maximum limit of annual number (maximum: {1}) has been reached", fountainProvider.DeclarantTaxNumber, fountainProvider.Wrapper.SN_MaximumValue));
		}
	}

	const int ProgressiveCodeLength = 6;
	const char PaddingChar = '0';

	#endregion
}
