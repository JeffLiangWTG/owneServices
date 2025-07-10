using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;

namespace Enterprise.Customs.IT.Business;

public class CustomsMessageFilenameProvider : ICustomsMessageFilenameProvider
{
	public CustomsMessageFilenameProvider(Account account, ZString messageType, BusinessObjectFactory factory)
	{
		this.account = Argument.NotNull(account, nameof(account));
		this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	readonly Account account;
	readonly ZString messageType;
	readonly BusinessObjectFactory factory;

	ZString ICustomsMessageFilenameProvider.GenerateFilename()
	{
		var shortDate = ZDate.Today.ToString(DateStringFormat, CultureInfo.InvariantCulture);
		var dailySequenceNumberGenerator = new DailySequenceNumberGenerator(account, factory);
		return FormattableString.Invariant($"{account.AccountNode}{shortDate}.{messageType}{dailySequenceNumberGenerator.Generate()}");
	}

	const string DateStringFormat = "MMdd";
}
