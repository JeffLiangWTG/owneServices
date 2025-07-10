using System;
using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using Enterprise.Accounting.Business.ComplianceReport;
	using NUnit.Framework;

	[TestedType(typeof(AccComplianceReportLine))]
	public class AccComplianceReportLineTest : AccComplianceReportLineBaseTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccComplianceReportLine(Factory, GetDataRow(Factory));
		}

		internal static DataRow GetDataRow(BusinessObjectFactory factory)
		{
			var report = factory.NewWithValidTestData<AccComplianceReport>();
			var table = AccComplianceReportLine.GetDataTable(report);
			var result = table.NewRow();

			result[AccComplianceReportLineBase.Schema.PK] = Guid.NewGuid();
			table.Rows.Add(result);
			result.AcceptChanges();

			return result;
		}

		public void TestPreCalculatedProperties()
		{
			var line = (AccComplianceReportLine)GetNewBusinessObject();

			var row = (line as INeedRow)?.Row;
			AssertNotNull(row);

			var testCases = new []
			{
				new { Input = "", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "100.99|100", Amount = new ZDecimal(100.99), Count =  new ZInt(100) },
				new { Input = "-100.00|-1", Amount = new ZDecimal(-100.00), Count = new ZInt(-1) },
				new { Input = ".01|-5.0", Amount = new ZDecimal(0.01), Count =  ZInt.Zero },
				new { Input = " 100.01| 10", Amount = new ZDecimal(100.01), Count =  new ZInt(10) },
				new { Input = "1A|2", Amount = ZDecimal.Zero, Count = new ZInt(2) },
				new { Input = "1A|2B", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "ABC|D", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "A1|B2", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "||50", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "99999999999999999.99", Amount = new ZDecimal(99999999999999999.99m), Count = ZInt.Zero },
				new { Input = "|" + int.MaxValue.ToString(), Amount = ZDecimal.Zero, Count = new ZInt(int.MaxValue) },
				new { Input = "|9999999999999999999", Amount = ZDecimal.Zero, Count = ZInt.Zero },
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					row[AccComplianceReportLine.Schema.ReportSubCode] = testCase.Input;
					AssertEquals("PreCalculatedAmount for ReportSubCode = " + testCase.Input, testCase.Amount, line.PreCalculatedAmount);
					AssertEquals("PreCalculatedCount for ReportSubCode = " + testCase.Input, testCase.Count, line.PreCalculatedCount);
				}
			});
		}

		public void TestPreCalculatedCount2()
		{
			var line = (AccComplianceReportLine)GetNewBusinessObject();

			var row = (line as INeedRow)?.Row;
			AssertNotNull(row);

			var testCases2 = new[]
			{
				new { Input = "", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "100.99|100|50", Amount = new ZDecimal(100.99), Count =  new ZInt(50) },
				new { Input = "-100.00|-1| -5", Amount = new ZDecimal(-100.00), Count = new ZInt(-5) },
				new { Input = ".01|-5.0", Amount = new ZDecimal(0.01), Count =  ZInt.Zero },
				new { Input = " 100.01| 10| 7", Amount = new ZDecimal(100.01), Count =  new ZInt(7) },
				new { Input = "1A|2|56", Amount = ZDecimal.Zero, Count = new ZInt(56) },
				new { Input = "1A|2B|3C", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "ABC|D|EF", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "A1|B2|C3", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "||50", Amount = ZDecimal.Zero, Count = new ZInt(50) },
				new { Input = "99999999999999999.99", Amount = new ZDecimal(99999999999999999.99m), Count = ZInt.Zero },
				new { Input = "||" + int.MaxValue.ToString(), Amount = ZDecimal.Zero, Count = new ZInt(int.MaxValue) },
				new { Input = "||9999999999999999999", Amount = ZDecimal.Zero, Count = ZInt.Zero },
			};

			CombineAssertions(() =>
			{ 
				foreach (var testCase2 in testCases2)
				{
					row[AccComplianceReportLine.Schema.ReportSubCode] = testCase2.Input;
					AssertEquals("PreCalculatedCount for ReportSubCode = " + testCase2.Input, testCase2.Count, line.PreCalculatedCount2);
				}
			});
		}

		public void TestPreCalculatedCount3()
		{
			var line = (AccComplianceReportLine)GetNewBusinessObject();

			var row = (line as INeedRow)?.Row;
			AssertNotNull(row);

			var testCases = new[]
			{
				new { Input = "", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "100.99|100|50|76", Amount = new ZDecimal(100.99), Count =  new ZInt(76) },
				new { Input = "-100.00|-1| -5| -3", Amount = new ZDecimal(-100.00), Count = new ZInt(-3) },
				new { Input = ".01|-5.0|9", Amount = new ZDecimal(0.01), Count =  ZInt.Zero },
				new { Input = " 100.01| 10| 7 |89", Amount = new ZDecimal(100.01), Count =  new ZInt(89) },
				new { Input = "1A|2|56|42", Amount = ZDecimal.Zero, Count = new ZInt(42) },
				new { Input = "1A|2B|3C|4D", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "ABC|D|EF|GHI", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "A1|B2|C3|D4", Amount = ZDecimal.Zero, Count = ZInt.Zero },
				new { Input = "|||50", Amount = ZDecimal.Zero, Count = new ZInt(50) },
				new { Input = "9999999999999999.99", Amount = new ZDecimal(99999999999999999.99m), Count = ZInt.Zero },
				new { Input = "|||" + int.MaxValue.ToString(), Amount = ZDecimal.Zero, Count = new ZInt(int.MaxValue) },
				new { Input = "|||9999999999999999999", Amount = ZDecimal.Zero, Count = ZInt.Zero },
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					row[AccComplianceReportLine.Schema.ReportSubCode] = testCase.Input;
					AssertEquals("PreCalculatedCount for ReportSubCode = " + testCase.Input, testCase.Count, line.PreCalculatedCount3);
				}
			});
		}

		public void TestDaysRange()
		{
			var line = (AccComplianceReportLine)GetNewBusinessObject();

			var row = (line as INeedRow)?.Row;
			AssertNotNull(row);

			var testCases = new[]
			{
				new { Expected = "", CountFrom = int.MinValue, CountTo = int.MinValue },
				new { Expected = "", CountFrom = -10, CountTo = -1 },
				new { Expected = "0-20 Days", CountFrom = 0, CountTo = 20 },
				new { Expected = "21-30 Days", CountFrom = 21, CountTo = 30 },
				new { Expected = "31-60 Days", CountFrom = 31, CountTo = 60 },
				new { Expected = "61-90 Days", CountFrom = 61, CountTo = 90 },
				new { Expected = "91-120 Days", CountFrom = 91, CountTo = 120 },
				new { Expected = "121+ Days", CountFrom = 121, CountTo = 200 },
				new { Expected = "", CountFrom = int.MaxValue, CountTo = int.MaxValue },
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					if (testCase.CountFrom == testCase.CountTo) // To avoid Overflow for int.MaxValue
					{
						assertIndex(testCase.CountFrom, testCase.Expected);
					}
					else
					{
						for (int i = testCase.CountFrom; i <= testCase.CountTo; i++)
						{
							assertIndex(i, testCase.Expected);
						}
					}
				}

				void assertIndex(int i, string expected)
				{
					row[AccComplianceReportLine.Schema.ReportSubCode] = "123|" + i.ToString();
					AssertEquals("PreCalculatedCount", i, line.PreCalculatedCount);
					AssertEquals("DaysRange for PreCalculatedCount = " + i.ToString(), expected, line.DaysRange);
				}
			});
		}

		public void TestDaysRange30_60()
		{
			var line = (AccComplianceReportLine)GetNewBusinessObject();

			var row = (line as INeedRow)?.Row;
			AssertNotNull(row);

			var testCases = new[]
			{
				new { Expected = "", CountFrom = int.MinValue, CountTo = int.MinValue },
				new { Expected = "", CountFrom = -10, CountTo = -1 },
				new { Expected = "0-30 Days", CountFrom = 0, CountTo = 30 },
				new { Expected = "31-60 Days", CountFrom = 31, CountTo = 60 },
				new { Expected = "61+ Days", CountFrom = 61, CountTo = 100 },
				new { Expected = "", CountFrom = int.MaxValue, CountTo = int.MaxValue },
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					if (testCase.CountFrom == testCase.CountTo) // To avoid Overflow for int.MaxValue
					{
						assertIndex(testCase.CountFrom, testCase.Expected);
					}
					else
					{
						for (int i = testCase.CountFrom; i <= testCase.CountTo; i++)
						{
							assertIndex(i, testCase.Expected);
						}
					}
				}

				void assertIndex(int i, string expected)
				{
					row[AccComplianceReportLine.Schema.ReportSubCode] = "123|" + i.ToString();
					AssertEquals("PreCalculatedCount", i, line.PreCalculatedCount);
					AssertEquals("DaysRange30_60 for PreCalculatedCount = " + i.ToString(), expected, line.DaysRange30_60);
				}
			});
		}

		public void TestIsSmallBusiness()
		{
			var line = (AccComplianceReportLine)GetNewBusinessObject();

			var row = (line as INeedRow)?.Row;
			AssertNotNull(row);

			var testCases = new[]
			{
				new { Expected = ZBool.False, CountFrom = int.MinValue, CountTo = int.MinValue },
				new { Expected = ZBool.False, CountFrom = -10, CountTo = 1 },
				new { Expected = ZBool.True, CountFrom = 2, CountTo = 3 },
				new { Expected = ZBool.False, CountFrom = 4, CountTo = 10 },
				new { Expected = ZBool.False, CountFrom = int.MaxValue, CountTo = int.MaxValue },
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					if (testCase.CountFrom == testCase.CountTo) // To avoid Overflow for int.MaxValue
					{
						assertIndex(testCase.CountFrom, testCase.Expected);
					}
					else
					{
						for (int i = testCase.CountFrom; i <= testCase.CountTo; i++)
						{
							assertIndex(i, testCase.Expected);
						}
					}
				}

				void assertIndex(int i, ZBool expected)
				{
					row[AccComplianceReportLine.Schema.ReportSubCode] = "123|" + i.ToString();
					AssertEquals("PreCalculatedCount", i, line.PreCalculatedCount);
					AssertEquals("IsSmallBusiness = " + i.ToString(), expected, line.IsSmallBusiness);
				}
			});
		}

		public void TestIsFullyPaid()
		{
			var line = (AccComplianceReportLine)GetNewBusinessObject();

			var row = (line as INeedRow)?.Row;
			AssertNotNull(row);

			var testCases = new[]
			{
				new { Expected = ZBool.False, CountFrom = int.MinValue, CountTo = int.MinValue },
				new { Expected = ZBool.False, CountFrom = -10, CountTo = 0 },
				new { Expected = ZBool.True, CountFrom = 1, CountTo = 1 },
				new { Expected = ZBool.False, CountFrom = 2, CountTo = 2 },
				new { Expected = ZBool.True, CountFrom = 3, CountTo = 3 },
				new { Expected = ZBool.False, CountFrom = 4, CountTo = 10 },
				new { Expected = ZBool.False, CountFrom = int.MaxValue, CountTo = int.MaxValue },
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					if (testCase.CountFrom == testCase.CountTo) // To avoid Overflow for int.MaxValue
					{
						assertIndex(testCase.CountFrom, testCase.Expected);
					}
					else
					{
						for (int i = testCase.CountFrom; i <= testCase.CountTo; i++)
						{
							assertIndex(i, testCase.Expected);
						}
					}
				}

				void assertIndex(int i, ZBool expected)
				{
					row[AccComplianceReportLine.Schema.ReportSubCode] = "123|" + i.ToString();
					AssertEquals("PreCalculatedCount", i, line.PreCalculatedCount);
					AssertEquals("IsFullyPaid = " + i.ToString(), expected, line.IsFullyPaid);
				}
			});
		}
	}
}

