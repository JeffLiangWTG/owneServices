using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class SuppressField : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<SuppressField({ShouldSuppressField})>",
				ResString.GetMultilingualString("ed02b172-7070-4ab1-a192-251b90e9de94", @"Returns Boolean value indicating whether the field should be suppressed. List of possible parameters:
{0}",
				GetListOfFieldTypes("\r\n")),
				new List<(string example, object expectedResult)> { ("<SuppressField(MasterBill)>", (NoResString)"true") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			IControlFlightDetailsSuppression flightSuppressionDetailsProvider = report.BODocDataProvider as IControlFlightDetailsSuppression;

			if (flightSuppressionDetailsProvider == null)
			{
				ReportMacroError(report, (NoResString)"You can only use this macro on Document with a DataSource type of GenericFreightWrapper.");
				return "";
			}

			string groupByField = Regex.Match(macro).Groups[1].Value;

			SuppressFields type;
			try
			{
				type = (SuppressFields)Enum.Parse(typeof(SuppressFields), groupByField, true);
			}
			catch (ArgumentException)
			{
				ReportMacroError(report, string.Format((NoResString)"{0} is not a correct field to suppress.", groupByField));
				return "";
			}

			if (!Enum.IsDefined(typeof(SuppressFields), type) | type.ToString().Contains(","))
			{
				ReportMacroError(report, string.Format((NoResString)"{0} is not a correct field to suppress.", groupByField));
				return "";
			}

			ContactType contactType = report.TypeOfContact ?? (report.MenuItem != null ? ContactType.Find(report.MenuItem.SU_ContactType) : null);
			return flightSuppressionDetailsProvider.ShouldSuppressFlightDetails(type, contactType) ? (NoResString)"true" : (NoResString)"false";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)SuppressField(?:[\s]*)\((?:[\s]*)(" + GetListOfFieldTypes("|") + @")(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		static string GetListOfFieldTypes(string delimeter)
		{
			return new ZStringBuilder(Enum.GetNames(typeof(SuppressFields))).ToStringWithDelimiterBetweenAppends(delimeter);
		}
	}

	public interface IControlFlightDetailsSuppression
	{
		bool ShouldSuppressFlightDetails(SuppressFields fieldType, ContactType contactTypeCode);
	}
}
