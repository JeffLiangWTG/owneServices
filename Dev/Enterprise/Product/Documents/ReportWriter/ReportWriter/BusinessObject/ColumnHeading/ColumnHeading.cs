using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.ReportWriter
{
	public class ColumnHeading : AutoColumnHeading, ICodeDescription
	{
		public ColumnHeading(ReportBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public ZString GetValue()
		{
			var result = new ZStringBuilder();
			AddData(result, DisplayLabelInfo);
			AddData(result, HeadingTextInfo);
			AddData(result, IsHidden, Constants.ConfigArea.ColumnHeading.Hidden);
			AddData(result, ShowPerformanceWarning, Constants.ConfigArea.ColumnHeading.ShowPerformanceWarning);
			AddData(result, HideIfDescriptionEmpty, Constants.ConfigArea.ColumnHeading.HideIfDescriptionEmpty);
			if (result.IsEmpty)
			{
				result.Append(UnknownValue);
			}
			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		public void PopulateFromExcel(string cellValue, int width)
		{
			if (RegexProvider.ColumnHeadingDisplayLabelRegex.IsMatch(cellValue))
			{
				var displayLabelMatch = RegexProvider.ColumnHeadingDisplayLabelRegex.Match(cellValue);
				string displayLabel = displayLabelMatch.Groups[1].Value;
				cellValue = cellValue.Replace(displayLabelMatch.Groups[0].Value, "");
				DisplayLabel = displayLabel;

				if (RegexProvider.ColumnHeadingDescriptionRegex.IsMatch(cellValue))
				{
					var descriptionMatch = RegexProvider.ColumnHeadingDescriptionRegex.Match(cellValue);
					Description = descriptionMatch.Groups[1].Value;
					cellValue = cellValue.Replace(descriptionMatch.Groups[0].Value, "");
				}
				else
				{
					Description = displayLabel;
				}

				if (RegexProvider.ColumnHeadingHeadingTextRegex.IsMatch(cellValue))
				{
					var headingTextMatch = RegexProvider.ColumnHeadingHeadingTextRegex.Match(cellValue);
					HeadingText = headingTextMatch.Groups[1].Value;
					cellValue = cellValue.Replace(headingTextMatch.Groups[0].Value, "");
				}
				else
				{
					HeadingText = displayLabel;
				}
				var splitValues = cellValue.Split(',').Select(x => x.Trim()).ToArray();

				IsHidden = splitValues.Any(x => x.Equals(Constants.ConfigArea.ColumnHeading.Hidden, StringComparison.OrdinalIgnoreCase));
				Width = width;
				ShowPerformanceWarning = splitValues.Any(x => x.Equals(Constants.ConfigArea.ColumnHeading.ShowPerformanceWarning, StringComparison.OrdinalIgnoreCase));
				HideIfDescriptionEmpty = splitValues.Any(x => x.Equals(Constants.ConfigArea.ColumnHeading.HideIfDescriptionEmpty, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				UnknownValue = cellValue;
			}
		}

		public override ZInt ColumnNumber
		{
			get { return base.ColumnNumber; }
			set
			{
				var oldValue = value;
				base.ColumnNumber = value;
				if (!IsCopying && oldValue != value)
				{
					parent.MarkAsNeedingRefreshRelatedDatas();
				}
			}
		}

		public override ZString DisplayLabel
		{
			get { return base.DisplayLabel; }
			set
			{
				var oldValue = value;
				base.DisplayLabel = value;
				if (!IsCopying && oldValue != value)
				{
					parent.MarkAsNeedingRefreshRelatedDatas();
				}
			}
		}

		void AddData(ZStringBuilder result, ZPropertyInfo info)
		{
			var value = info.Value;
			if (!value.IsEmpty)
			{
				result.AppendFormat(@"{0}=""{1}""", info.Name, value.ToString());
			}
		}

		void AddData(ZStringBuilder result, bool shouldAdd, ZString value)
		{
			if (shouldAdd)
			{
				result.Append(value);
			}
		}

		readonly ReportBizObj parent;

		string ICodeDescription.Code
		{
			get { return ColumnNumber.ToString(); }
		}

		string ICodeDescription.Description
		{
			get { return DisplayLabel; }
		}

		object ICodeDescription.PK
		{
			get { return PK; }
		}
	}
}
