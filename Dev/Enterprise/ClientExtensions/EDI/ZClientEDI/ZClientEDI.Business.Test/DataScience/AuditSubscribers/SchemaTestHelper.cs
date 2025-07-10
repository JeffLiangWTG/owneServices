using System.Linq;
using CargoWise.Bi.ConfigLoader;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(BiAutomationConfigLoader))]

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests
{
	class SchemaTestHelper : TestCase
	{
		/// <summary>
		/// This method can be used to generate test code from <code>IDataScienceSubscripberToKafka</code> implementations.
		/// </summary>
		public static string GenerateTestDataSchemaCode(IDataScienceSubscriberToKafka subscriber)
		{
			return string.Join(
				null,
				new[]
				{
					new[]
					{
						"// Arrange / Act",
						"var subscriber = SubscriberUnderTest;",
						"",
						"// Assert",
						$"AssertEquals({subscriber.DataSchema.DataSchemaVersion}, subscriber.DataSchema.DataSchemaVersion);",
						$"CombineAssertions(",
						$@"	@""
The schema of the table {subscriber.Table.TableName} required by {subscriber.GetType().Name} has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team."",",
						"	() =>",
						"	{",
						$"		AssertEquals({subscriber.ColumnInfos.Count}, subscriber.ColumnInfos.Count);",
						"",
					},
					subscriber.ColumnInfos.Select((columnInfo, i) =>
					new[]
					{
						$@"		AssertEquals(""{columnInfo.ColumnName}"", subscriber.ColumnInfos[{i}].ColumnName);",
						$@"		AssertEquals(""{columnInfo.SqlType}"", subscriber.ColumnInfos[{i}].SqlType);",
						$@"		AssertEquals({columnInfo.IsNullable.ToString().ToLower()}, subscriber.ColumnInfos[{i}].IsNullable);",
					}.AsEnumerable())
						.Aggregate((linesSoFar, columnInfoLines) => linesSoFar.Concat(new[] { "" }).Concat(columnInfoLines)),
					new[]
					{
						"	});"
					}
				}
					.SelectMany(lines => lines)
					.Select(line => line + "\n"));
		}
	}
}
