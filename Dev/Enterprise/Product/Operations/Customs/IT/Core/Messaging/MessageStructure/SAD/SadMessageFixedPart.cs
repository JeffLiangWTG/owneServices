using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

[MessageFixedLength]
public class SadMessageFixedPart : ISadMessageFixedPart
{
	public SadMessageFixedPart(bool isHeader, ZString messageCode, ZString annualProgressiveNumber, ZInt progressiveNumber)
	{
		this.isHeader = isHeader;
		MessageCode = messageCode;
		AnnualProgressiveNumber = annualProgressiveNumber;
		ProgressiveNumber = progressiveNumber;
	}

	readonly bool isHeader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	public ZString RecordType => isHeader ? Constants.RecordType.HeaderPrefix : Constants.RecordType.ContinuationPrefix;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 8, true)]
	public ZString MessageCode { get; }

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 4, true)]
	public virtual ZString DeclarantTaxNumber => ZString.Empty;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	public ZString EmptyField => ZString.Empty;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 6, true)]
	public ZString AnnualProgressiveNumber { get; }

	[MessageLayout(Order = 5)]
	[MessageFieldIntegerRepresentation(2, true)]
	public ZInt ProgressiveNumber { get; }

	public static class Constants
	{
		public static class RecordType
		{
			public const string HeaderPrefix = "T";
			public const string ContinuationPrefix = "?";
		}

		public static class MessageCode
		{
			public static class AP
			{
				public const string HeaderAndContinuation = "AP";
			}

			public static class IM
			{
				public const string Header = "IM";
				public const string Continuation = "IM1";
			}

			public static class ET
			{
				public const string Header = "ET";
				public const string Continuation = "ET1";
			}

			public static class NB
			{
				public const string Header = "NB";
				public const string Continuation = "NB1";
			}
		}
	}
}
