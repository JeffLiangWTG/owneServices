using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class GroupByArea : DataArea
	{
		public enum Position { Top, Bottom }

		public GroupByArea(int start, int end, Report report, string parameterText)
			: base(start, end, report, parameterText)
		{
			var validParameter = true;
			breakPage = false;

			if (Parameters.Length > 1 && Parameters[1].Length > 2)
			{
				var value = report.MacroTranslator.GetValue(Parameters[1], Passes.FirstPass);
				if (value is string stringValue)
				{
					groupByColumns = stringValue.Split('+');

					var invalidGroupByColumn = groupByColumns.Where(o => o.EndsWith(".", StringComparison.CurrentCulture)).ToList();
					if (invalidGroupByColumn.Any())
					{
						throw new DocumentEngineException(string.Format(CultureInfo.InvariantCulture, "Malformed GroupByColumn provided: '{0}'. GroupByColumn {{{1}}} should not be defined end with '.'.", parameterText, invalidGroupByColumn.Aggregate((a, b) => $"{a}}},{{{b}")));
					}
				}
				else
				{
					validParameter = false;
				}
			}
			else
			{
				validParameter = false;
			}

			if (validParameter)
			{
				groupByPosition = ContainsParam("GroupTitle") ? Position.Top : Position.Bottom;
				breakPage = ContainsParam(Constants.CommonAreaParameters.PageBreakSignature);
				Sticky = ContainsParam(Constants.CommonAreaParameters.Sticky);
				KeepInSamePage = ContainsParam("KeepInSamePage");
				ClonedAreas.Add(this);
			}
			else
			{
				throw new DocumentEngineException(string.Format(CultureInfo.InvariantCulture, "Malformed GroupByColumn provided: '{0}'. Please specify at least one column to group by.", parameterText));
			}
		}

		GroupByArea() { }

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"#GroupBy:{GroupByColumns}[:GroupTitle[:Sticky]][:PageBreak][:KeepInSamePage]",
				ResString.GetMultilingualString("e419cca5-172c-46c9-8e3e-151761adc292",
					@"{0} Areas are used to group rows from the Data Row Source together where the fields specified in the {2} parameter are the same. You can specify multiple fields separated by + signs where required.

By default the contents of the {0} Area will show just after the group of {1} Areas where the {2} match. You can make a {0} Area appear before the matching group of {1} Areas by specifying the '{3}' parameter.

If you want one before and one after each group, specify a {0} Area with the '{3}' parameter and it contents, then another {0} Area without the '{3}' parameter, but make sure the {2} parameter is identical on both of them.

Optional Parameters:
{4} - Makes this {0} Area appear before the group it represents. Default is to appear after the group it represents.
{5} - Insert a page break just before rendering this Area.
{6} - Try and keep the {0} Area together with it's first {1} Area row. This only works when using the '{3}' optional parameter.
{7} - Try and keep the {0} Area together with it's {1} Area rows whenever possible. This only works when using the '{3}' optional parameter.",
					"#GroupBy", "#SectionBody", "GroupByColumns", ":GroupTitle", "GroupTitle",
					"PageBreak", "Sticky", "KeepInSamePage"));
		}

		protected override string[] SplitParameters(string parameterText)
		{
			List<string> result = new List<string>();
			ZStringBuilder currentParam = new ZStringBuilder();
			bool insideQuotes = false;
			foreach (char currentChar in parameterText)
			{
				if ((currentChar == ':' || currentChar == ',') && !insideQuotes)
				{
					result.Add(currentParam.ToString());
					currentParam = new ZStringBuilder();
				}
				else
				{
					currentParam.Append(currentChar.ToString());
					if (currentChar == '"')
					{
						insideQuotes = !insideQuotes;
					}
				}
			}
			result.Add(currentParam.ToString());
			return result.ToArray();
		}

		public override Area Clone(int rowNumber)
		{
			int end = rowNumber + fEnd - fStart;

			GroupByArea cloned = new GroupByArea(rowNumber, end, ParentReport, (NoResString)"#GroupBy:" + string.Join((NoResString)"+", GroupByColumns));
			CopyCommonMembers(cloned);

			cloned.groupByPosition = GroupByPosition;
			cloned.breakPage = BreakPage;
			cloned.instanceNumber = InstanceNumber + 1;
			cloned.Sticky = Sticky;
			cloned.KeepInSamePage = KeepInSamePage;

			return cloned;
		}

		public List<Area> ClonedAreas = new List<Area>();
		public List<Area> ParentAreaList = new List<Area>();
		public string[] GroupByColumns
		{
			get { return groupByColumns; }
		}

		public override bool SplitIfNotFitInAPage
		{
			get { return true; }
		}

		public Position GroupByPosition
		{
			get { return groupByPosition; }
		}

		public override bool BreakPage
		{
			get { return breakPage; }
			internal set { breakPage = value; }
		}

		public int InstanceNumber
		{
			get { return instanceNumber; }
		}

		public override List<Area> Parents
		{
			get
			{
				List<Area> result;
				if (DataParent is GroupByArea)
				{
					result = new List<Area>(((GroupByArea)DataParent).ClonedAreas);
				}
				else if (DataParent is SectionBodyArea)
				{
					result = new List<Area>(((SectionBodyArea)DataParent).AreasSplittedAcrossPages);
				}
				else
				{
					result = new List<Area>();
				}

				return result;
			}
		}

		protected override string GetTableName()
		{
			return DataSourceArea.TableName;
		}

		protected override Dictionary<string, List<string>> ReadAdditionalFields()
		{
			var result = new Dictionary<string, List<string>>();
			foreach (string groupByColumn in GroupByColumns)
			{
				string fieldNameToAdd = null;
				string dataSourceNameToAdd;
				if (!groupByColumn.Contains(".")) // You can have Field Names on Group By headers without specifying the table name.
				{
					fieldNameToAdd = groupByColumn;
					dataSourceNameToAdd = TableName.ToUpperInvariant().Trim();
				}
				else
				{
					var sourceNameAndField = groupByColumn.Split('.');
					dataSourceNameToAdd = string.IsNullOrEmpty(sourceNameAndField.First()) ? TableName.ToUpperInvariant().Trim() : sourceNameAndField.First().ToUpperInvariant();
					fieldNameToAdd = sourceNameAndField.Last().ToUpperInvariant();
				}

				List<string> list;
				if (!result.TryGetValue(dataSourceNameToAdd, out list))
				{
					list = new List<string>();
					result.Add(dataSourceNameToAdd, list);
				}

				if (!string.IsNullOrEmpty(fieldNameToAdd) && !list.Contains(fieldNameToAdd))
				{
					list.Add(fieldNameToAdd);
				}
			}
			return result;
		}

		public override bool FitsInPage(int heightAvailableInPage, Page page)
		{
			bool result = base.FitsInPage(heightAvailableInPage, page);

			if (result && ShouldForcePutInNextPage(heightAvailableInPage, page))
			{
				result = false;
			}

			if (result)
			{
				result = CalculateFitsInPageWhenKeepInSamePageFlagIsSet(heightAvailableInPage, page);
			}

			return result;
		}

		internal bool ShouldForcePutInNextPage(int heightAvailableInPage, Page page)
		{
			return Sticky && groupByPosition == Position.Top && OwnerSection.SectionBody.GetRowsToKeep(heightAvailableInPage - HeightInXls, page) == 0;
		}

		public override Area SplitAndReturnNewArea(int maxHeightAvailable, Page page, PageBreakProcessor pageBreakProcessor)
		{
			// When GroupBy+SectionBody cannot fit in the same page, rather than splitting the GroupBy area, we should split the SectionBody area
			// Moving GroupBy area to the next page forces the SectionBodyArea to be split instead. However, when the height of groupby area is even 
			// bigger than the available height of a new page, we will split is regardless if parameter KeepInSamePage is passed in or not.
			return (KeepInSamePage && FitsMaxHeightForReportRenderer(page) && page.Areas.Any(area => area is SectionBodyArea)) ? null : base.SplitAndReturnNewArea(maxHeightAvailable, page, pageBreakProcessor);
		}

		bool FitsMaxHeightForReportRenderer(Page page)
		{
			ReportRenderer renderer = ParentReport.Renderer as ReportRenderer;
			if (renderer != null)
			{
				return HeightInXls <= renderer.GetMaxAvailableHeightForArea(this, page);
			}
			else
			{
				return true;
			}
		}

		bool CalculateFitsInPageWhenKeepInSamePageFlagIsSet(int heightAvailableInPage, Page page)
		{
			bool result = true;

			if (KeepInSamePage && groupByPosition == Position.Top && page.Areas.Any(area => area is SectionBodyArea))
			{
				int groupByHeightIncludingParentsInXls = CalculateHeightInXlsIncludingParents(this);
				result = groupByHeightIncludingParentsInXls <= heightAvailableInPage;
			}

			return result;
		}

		int CalculateHeightInXlsIncludingParents(Area area)
		{
			int result = 0;

			if (area is SectionBodyArea || area is GroupByArea)
			{
				result = area.HeightInXls;
			}

			if (area is GroupByArea && area.Parents != null)
			{
				foreach (var parentArea in area.Parents)
				{
					result += CalculateHeightInXlsIncludingParents(parentArea);
				}
			}

			return result;
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public override SectionBodyArea DataSourceArea
		{
			get { return SectionBody; }
		}

		public SectionBodyArea SectionBody;
		public int RowCount;
		public bool Sticky;
		public bool KeepInSamePage
		{
			get;
			private set;
		}

		readonly string[] groupByColumns;

		Position groupByPosition;
		bool breakPage;
		int instanceNumber;
	}
}
