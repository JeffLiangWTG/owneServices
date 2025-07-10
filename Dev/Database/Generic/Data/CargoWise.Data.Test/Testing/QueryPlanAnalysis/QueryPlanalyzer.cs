using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;

namespace CargoWise.Data.Testing
{
	public class QueryPlanalyzer
	{
		public QueryPlanalyzer(string queryPlanXml)
		{
			Argument.NotNull(queryPlanXml, nameof(queryPlanXml));

			xml = XDocument.Parse(queryPlanXml);

			xmlNamespace = xml.Root.GetDefaultNamespace();
			Elements = new List<PlanElement>();
			decimal totalCost = 0;
			foreach (var e in xml.Descendants(xmlNamespace + "RelOp"))
			{
				var planElement = PlanElement.CreateElement(xmlNamespace, e);
				Elements.Add(planElement);
				totalCost += planElement.EstimateIO + planElement.EstimateCPU;
			}

			foreach (var e in Elements)
			{
				e.EstimateCostPercent = (int)Math.Round(((e.EstimateCPU + e.EstimateIO) / totalCost * 100));
			}
		}

		public QueryPlanalyzer(string sqlText, DbConnection connection)
			: this(connection.GetQueryPlanXml_ForTest(sqlText))
		{
			Argument.NotNull(sqlText, nameof(sqlText));
			Argument.NotNull(connection, nameof(connection));
		}

		readonly XDocument xml;
		readonly XNamespace xmlNamespace;
		public List<PlanElement> Elements { get; }

		public string SqlStatement => xml.Descendants(xmlNamespace + "StmtSimple").FirstOrDefault()?.Attribute("StatementText")?.Value;

		public IEnumerable<ColumnDetails> TableScans => GetTableScanDetails();
		public IEnumerable<ConstantScanDetails> ConstantScans => GetConstantScanDetails();
		public IEnumerable<IndexAccessDetails> IndexScans => GetIndexDetails("Index Scan");
		public IEnumerable<IndexAccessDetails> IndexSeeks => GetIndexDetails("Index Seek");
		public IEnumerable<RidLookupDetails> RowIDLookups => GetRowIDLookupDetails();
		public QueryPlanDetails QueryPlan => GetQueryPlans().First();
		public IEnumerable<QueryPlanDetails> OtherQueryPlans => GetQueryPlans().Skip(1);
		public IReadOnlyList<SortElement> Sorts => Elements.Where(e => e.PhysicalOperator == "Sort").Cast<SortElement>().ToList();

		#region Types

		[DebuggerDisplay("{PhysicalOperator} - Cost: {EstimateCostPercent} %")]
		public class PlanElement
		{
			public PlanElement() { }
			public PlanElement(XElement relOpElement)
			{
				PhysicalOperator = relOpElement.Attribute("PhysicalOp")?.Value;
				EstimateIO = ReadDeciaml(relOpElement.Attribute("EstimateIO")?.Value);
				EstimateCPU = ReadDeciaml(relOpElement.Attribute("EstimateCPU")?.Value);
				RootElement = relOpElement;
			}

			public static PlanElement CreateElement(XNamespace @namespace, XElement relOpElement)
			{
				var physicalOperator = relOpElement.Attribute("PhysicalOp")?.Value;
				if (physicalOperator == "Sort")
				{
					return new SortElement(@namespace, relOpElement);
				}
				return new PlanElement(relOpElement);
			}

			decimal ReadDeciaml(string value)
			{
				if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					return result;
				}
				return 0;
			}

			public string PhysicalOperator { get; internal set; }
			public decimal EstimateIO { get; internal set; }
			public decimal EstimateCPU { get; internal set; }
			public int EstimateCostPercent { get; internal set; }
			public XElement RootElement { get; internal set; }
		}

		public class SortElement : PlanElement
		{
			public SortElement()
			{
				SortColumns = new List<ColumnDetails>();
			}
			public SortElement(XNamespace @namespace, XElement relOpElement) : base(relOpElement)
			{
				SortColumns = relOpElement.Elements().Where(el => el.Name.LocalName.Contains("Sort")).Elements(@namespace + "OrderBy").Descendants(@namespace + "ColumnReference").Select(e => new ColumnDetails(e)).ToList();
			}

			public List<ColumnDetails> SortColumns { get; private set; }
		}

		[DebuggerDisplay("Table: {TableName}, Column: {ColumnName}")]
		public class ColumnDetails
		{
			public ColumnDetails() { }
			public ColumnDetails(XElement el)
			{
				TableName = el.Attribute("Table")?.Value?.Replace("[", "")?.Replace("]", "");
				ColumnName = el.Attribute("Column")?.Value;
			}
			public string TableName { get; internal set; }
			public string ColumnName { get; internal set; }
		}

		[DebuggerDisplay("Constant Value: {ConstantValue}")]
		public class ConstantScanDetails
		{
			public string ConstantValue { get; internal set; }
		}

		[DebuggerDisplay("Index: {IndexName}, Kind: {IndexKind}, Table: {TableName}, Column: {ColumnName}")]
		public class IndexAccessDetails : ColumnDetails
		{
			public string IndexName { get; internal set; }
			public string IndexKind { get; internal set; }

			public long? ActualScans { get; internal set; }
			public long? ActualLogicalReads { get; internal set; }
			public long? ActualRowsRead { get; internal set; }
			public long? ActualExecutions { get; internal set; }
		}

		[DebuggerDisplay("Columns: {OutputList.Count}")]
		public class RidLookupDetails
		{
			public List<ColumnDetails> OutputList { get; } = new List<ColumnDetails>();
		}

		[DebuggerDisplay("CachedPlanSize: {CachedPlanSize}, CompileCPU: {CompileCPU}, CompileMemory: {CompileMemory}, CompileTime: {CompileTime}")]
		public class QueryPlanDetails
		{
			public long CachedPlanSize { get; internal set; }
			public long CompileCPU { get; internal set; }
			public long CompileMemory { get; internal set; }
			public long CompileTime { get; internal set; }
		}
		#endregion Types

		#region Assertions

		/// <summary>
		/// Asserts that we have no RID lookups in the plan analyzer provided.
		/// Important to assure queries don't deprecate in quality - RID lookups are bad!
		/// </summary>
		public static void AssertNoRIDLookups(QueryPlanalyzer planalyzer)
		{
			NUnit.Framework.Assertion.AssertContainsExactElementsInAnyOrder(
				"We expected no RID lookups, and yet...",
				Array.Empty<Tuple<string, string>>(),
				planalyzer.RowIDLookups.SelectMany(x => x.OutputList).Select(c => Tuple.Create(c.TableName, c.ColumnName)));
		}

		/// <summary>
		/// Asserts that we have no TableScans in the plan analyzer provided.
		/// Important to assure queries don't deprecate in quality - tablescans are also bad!
		/// </summary>
		public static void AssertNoTableScans(QueryPlanalyzer planalyzer)
		{
			NUnit.Framework.Assertion.AssertContainsExactElementsInAnyOrder(
				"We expected no Table scans, and yet...",
				Array.Empty<Tuple<string, string>>(),
				planalyzer.TableScans.Select(c => Tuple.Create(c.TableName, c.ColumnName)));
		}

		/// <summary>
		/// An IndexScan is similar to a TableScan, in that the SQL server has to scan over the entirety of a part of an index to find required data
		/// These can be very detrimental for queries over large tables, as the server can take a lot of time to scan over even a subset of an index, if it's large enough.
		/// If you're looking to avoid IndexScans add better indexes, ones that include fields you're querying for!
		/// </summary>
		/// <param name="planalyzer"></param>
		public static void AssertNoIndexScans(QueryPlanalyzer planalyzer)
		{
			NUnit.Framework.Assertion.AssertContainsExactElementsInAnyOrder(
				"We expected no RID lookups, and yet...",
				Array.Empty<Tuple<string, string>>(),
				planalyzer.IndexScans.Select(c => Tuple.Create(c.TableName, c.ColumnName)));
		}

		#endregion

		#region Implementation

		IEnumerable<ColumnDetails> GetTableScanDetails()
		{
			foreach (var element in xml.Descendants(xmlNamespace + "TableScan"))
			{
				var objectElement = element.Descendants(xmlNamespace + "Object").Single();
				var columnElement = element.Descendants(xmlNamespace + "ColumnReference").FirstOrDefault();

				yield return new ColumnDetails(objectElement)
				{
					ColumnName = columnElement?.Attribute("Column").Value,
				};
			}
		}

		IEnumerable<ConstantScanDetails> GetConstantScanDetails()
		{
			foreach (var element in xml.Descendants(xmlNamespace + "ConstantScan"))
			{
				var constObject = element.Descendants(xmlNamespace + "Const").Single();

				yield return new ConstantScanDetails
				{
					ConstantValue = constObject.Attribute("ConstValue")?.Value
				};
			}
		}

		IEnumerable<IndexAccessDetails> GetIndexDetails(string indexType)
		{
			var elements = Elements.Where(e => e.PhysicalOperator.Contains(indexType));

			foreach (var element in elements)
			{
				var objectElement = element.RootElement.Descendants(xmlNamespace + "Object").Single();
				var columnElement = element.RootElement.Descendants(xmlNamespace + "ColumnReference").FirstOrDefault();
				var runTimeCounters = element.RootElement.Descendants(xmlNamespace + "RunTimeCountersPerThread");
				var totalCounters = runTimeCounters.Aggregate(new
				{
					ActualScans = (long?)null,
					ActualLogicalReads = (long?)null,
					ActualRowsRead = (long?)null,
					ActualExecutions = (long?)null,
				},
					(acc, e) => new
					{
						ActualScans = AccumulateLong(acc.ActualScans, e.Attribute("ActualScans")),
						ActualLogicalReads = AccumulateLong(acc.ActualLogicalReads, e.Attribute("ActualLogicalReads")),
						ActualRowsRead = AccumulateLong(acc.ActualRowsRead, e.Attribute("ActualRowsRead")),
						ActualExecutions = AccumulateLong(acc.ActualExecutions, e.Attribute("ActualExecutions")),
					}
				);

				yield return new IndexAccessDetails
				{
					TableName = objectElement.Attribute("Table").Value.Replace("[", "").Replace("]", ""),
					ColumnName = columnElement?.Attribute("Column").Value,
					IndexName = objectElement.Attribute("Index").Value.Replace("[", "").Replace("]", ""),
					IndexKind = objectElement.Attribute("IndexKind")?.Value,

					ActualScans = totalCounters.ActualScans,
					ActualLogicalReads = totalCounters.ActualLogicalReads,
					ActualRowsRead = totalCounters.ActualRowsRead,
					ActualExecutions = totalCounters.ActualExecutions,
				};
			}

			long? AccumulateLong(long? previous, XAttribute attr)
			{
				var attrValue = attr?.Value ?? String.Empty;
				if (long.TryParse(attrValue, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var number))
				{
					return previous.GetValueOrDefault(0L) + number;
				}
				else
				{
					return previous;
				}
			}
		}

		IEnumerable<RidLookupDetails> GetRowIDLookupDetails()
		{
			var elements =
				from e in xml.Descendants(xmlNamespace + "RelOp")
				from a in e.Attributes("LogicalOp")
				where a.Value == "RID Lookup"
				select e;

			foreach (var element in elements)
			{
				var outputListElement = element.Descendants(xmlNamespace + "OutputList").Single();
				var ridLookupDetails = new RidLookupDetails();

				foreach (var columnReferenceElement in outputListElement.Descendants(xmlNamespace + "ColumnReference"))
				{
					ridLookupDetails.OutputList.Add(new ColumnDetails(columnReferenceElement));
				}

				yield return ridLookupDetails;
			}
		}

		IEnumerable<QueryPlanDetails> GetQueryPlans()
		{
			foreach (var element in xml.Descendants(xmlNamespace + "QueryPlan"))
			{
				yield return new QueryPlanDetails
				{
					CachedPlanSize = ParseLong(element, "CachedPlanSize"),
					CompileTime = ParseLong(element, "CompileTime"),
					CompileCPU = ParseLong(element, "CompileCPU"),
					CompileMemory = ParseLong(element, "CompileMemory"),
				};
			}

			long ParseLong(XElement element, XName attr)
			{
				var attrValue = element.Attribute(attr)?.Value ?? string.Empty;
				if (long.TryParse(attrValue, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var number))
				{
					return number;
				}
				return 0L;
			}
		}

		#endregion
	}
}
