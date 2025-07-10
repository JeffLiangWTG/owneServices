using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
[AttributeUsage(AttributeTargets.Property)]
public class MessageFieldDateTimeRepresentationAttribute : MessageFieldRepresentationAttribute
{
	public string FormatDate { get; }

	protected MessageFieldDateTimeRepresentationAttribute(string formatDate, int length)
		: base(length, true)
	{
		FormatDate = Argument.NotNullOrEmpty(formatDate, "formatDate");
	}

	public override ZString SerializeValue(IZType value)
	{
		Argument.NotNull(value, nameof(value));

		if (!(value is ZDate) && !(value is ZDateTime))
		{
			throw new InvalidCastException(FormattableString.Invariant($"The expected types are: ZDateTime or ZDate, but was: {value.GetType().Name}"));
		}
		var castedValue = new ZDateTime(value);
		return castedValue.ToString(FormatDate, CultureInfo.InvariantCulture);
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldDateYYYYMMDDRepresentationAttribute : MessageFieldDateTimeRepresentationAttribute
{
	const string YYYYMMDD = "yyyyMMdd";

	public MessageFieldDateYYYYMMDDRepresentationAttribute()
		: base(YYYYMMDD, YYYYMMDD.Length)
	{
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldDateDDMMYYYYRepresentationAttribute : MessageFieldDateTimeRepresentationAttribute
{
	const string DDMMYYYY = "ddMMyyyy";

	public MessageFieldDateDDMMYYYYRepresentationAttribute()
		: base(DDMMYYYY, DDMMYYYY.Length)
	{
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldDateHHMMSSRepresentationAttribute : MessageFieldDateTimeRepresentationAttribute
{
	const string HHMMSS = "HHmmss";

	public MessageFieldDateHHMMSSRepresentationAttribute()
		: base(HHMMSS, HHMMSS.Length)
	{
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldDateDDMMYYRepresentationAttribute : MessageFieldDateTimeRepresentationAttribute
{
	const string DDMMYY = "ddMMyy";

	public MessageFieldDateDDMMYYRepresentationAttribute()
		: base(DDMMYY, DDMMYY.Length)
	{
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class MessageFieldDateYYYYMMDDHHMMRepresentationAttribute : MessageFieldDateTimeRepresentationAttribute
{
	const string YYYYMMDDHHMM = "yyyyMMddHHmm";

	public MessageFieldDateYYYYMMDDHHMMRepresentationAttribute()
		: base(YYYYMMDDHHMM, YYYYMMDDHHMM.Length)
	{
	}
}
