using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	class DocumentFilterDateTime : DocumentFilterBase
	{
		public DocumentFilterDateTime(ComparisonOption dateComparisonOption)
		{
			DateComparisonOption = dateComparisonOption;
		}

		internal ZDateTime Value { get; private set; } = ZDateTime.Empty;

		public override void AddFilterValue(ZString value)
		{
			var newDate = Parse(value);
			if (IsDateMatch(newDate))
			{
				Value = newDate;
			}
		}

		public override bool IsMatch(IeDoc eDoc)
		{
			var docDate = eDoc.LastEdited;
			if (!docDate.IsValid)
			{
				docDate = eDoc.DateAdded;
			}
			return IsDateMatch(docDate);
		}

		ZDateTime Parse(ZString textValue)
		{
			if (textValue.Contains("Z"))
			{
				if (ZDateTime.TryParseISO8601Date(textValue, out var dateUtcFrom))
				{
					return dateUtcFrom;
				}
			}
			else if (ZDateTime.TryParseIgnoreTimezone(textValue, ObjectCache.CultureProvider.Culture, out var dateUtcFrom))
			{
				return dateUtcFrom;
			}
			throw new DataObjectReadFailureException($"Cannot parse date and time '{textValue}'.");
		}

		protected bool IsDateMatch(ZDateTime newDate)
		{
			if (!newDate.IsValid)
			{
				return false;
			}

			if (!Value.IsValid)
			{
				return true;
			}

			switch (DateComparisonOption)
			{
				case ComparisonOption.Equal:
					return Value.Equals(newDate);
				case ComparisonOption.From:
					return newDate >= Value;
				case ComparisonOption.To:
					return newDate <= Value;
				default:
					return false;
			}
		}

		ComparisonOption DateComparisonOption { get; }

		public enum ComparisonOption
		{
			Equal,
			From,
			To
		}
	}
}
