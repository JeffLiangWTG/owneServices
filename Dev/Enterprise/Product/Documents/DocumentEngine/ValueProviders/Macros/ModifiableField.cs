using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	public class ModifiableField : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ModifiableField(\"{fieldidentifier}\"[, \"{defaultvalue}\"])>",
				ResString.GetMultilingualString("0affca48-217b-4755-8e93-9f907a50b426",
				@"Will allow the user to modify the content of the cell where there is no field to base the content on. 
You can optionally specify a default value where you want multiple values to make up the basis for one editable cell. The default value can contain any combination of macros and text where specified."),
				new List<(string example, object expectedResult)> { ((NoResString)"<ModifiableField(\"SignaturePrefix\", \"Yours Sincerely\")>", (NoResString)"Yours Sincerely") });
		}

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (report.Renderer?.CurrentAreaToProcess?.DataSourceArea?.DataRowSource is VisualiserDataSource bodyDataSource && report.Renderer.CurrentDataRow >= 0)
			{
				var bodyRowIndex = report.Renderer.CurrentDataRow;
				var columnName = VisualiserDataSet.GetColumnName(macro, VisualiserDataSet.GetCollectionName(bodyDataSource.TableName));
				if (bodyRowIndex < bodyDataSource.RowCount && bodyDataSource.ColumnExists(columnName))
				{
					var bodyDataRow = bodyDataSource.RowByIndex(bodyRowIndex);
					return bodyDataRow[columnName];
				}
				return string.Empty;
			}
			else
			{
				var match = regex.Match(macro);
				return match.Groups["defaultvalue"].Value;
			}
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		public static Regex MacroRegex
		{
			get { return regex; }
		}

		public static string GetFieldIdentifier(string macro)
		{
			var match = regex.Match(macro);
			return match.Groups["fieldidentifier"].Value;
		}

		static readonly Regex regex = new Regex(@"^<[\s]*ModifiableField[\s]*\(""(?<fieldidentifier>[^""]+)""(|,[\s]*""(?<defaultvalue>.*)"")[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
	}
}
