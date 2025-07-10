using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlFilterBusinessObject))]
	sealed class ExitControlFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestClearanceDateFilter()
		{
			CusExitHeader SetUpHeader(ZDateTime date)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				var entryNum = report.ClearanceEntryNumber;
				entryNum.CE_IssueDate = date;
				return header;
			}
			var header1 = SetUpHeader(new ZDateTime(2022, 1, 1, 10, 0, 0));
			var header2 = SetUpHeader(new ZDateTime(2022, 1, 1, 10, 0, 0));
			var header3 = SetUpHeader(new ZDateTime(2021, 2, 16, 15, 30, 0));

			Factory.Save();

			var moduleFilter = (ModuleDateFilter)filter[ExitControlFilterBusinessObject.ESFilterConstants.ClearanceDate];
			moduleFilter.IsActive = true;
			moduleFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			CombineAssertions(() =>
			{
				moduleFilter.Property1 = new ZDateTime(2022, 1, 1, 10, 0, 0);
				moduleFilter.Property2 = new ZDateTime(2022, 1, 1, 10, 0, 0);
				AssertExitHeadersMatchFilter("Date in 2022", (header1, true), (header2, true), (header3, false));

				moduleFilter.Property1 = new ZDateTime(2021, 2, 16, 15, 30, 0);
				moduleFilter.Property2 = new ZDateTime(2021, 2, 16, 15, 30, 0);
				AssertExitHeadersMatchFilter("Date in 2021", (header1, false), (header2, false), (header3, true));

				moduleFilter.Property1 = new ZDateTime(2020, 5, 26, 18, 15, 0);
				moduleFilter.Property2 = new ZDateTime(2020, 5, 26, 18, 15, 0);
				AssertExitHeadersMatchFilter("Date in 2020", (header1, false), (header2, false), (header3, false));
			});
		}

		public void TestCircuitFilter()
		{
			CusExitHeader SetUpHeader(string circuit)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				var entryNum = report.ClearanceEntryNumber;
				entryNum.CE_EntryStatus = circuit;
				return header;
			}
			var header1 = SetUpHeader(CircuitCodeList.Codes.GREEN);
			var header2 = SetUpHeader(CircuitCodeList.Codes.RED);
			var header3 = SetUpHeader(CircuitCodeList.Codes.ORANGE);

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.ESFilterConstants.Circuit];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "R";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "V";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, false), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "X";
				AssertExitHeadersMatchFilter("Contains", (header1, false), (header2, false), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "V";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "R";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "R";
				AssertExitHeadersMatchFilter("NotContain", (header1, true), (header2, false), (header3, true));
			});
		}

		void AssertExitHeadersMatchFilter(ZString message, params (CusExitHeader header, bool expected)[] headerMatches)
		{
			var headerIndex = 1;

			foreach (var (header, expected) in headerMatches)
			{
				AssertEquals($"{message}->header{headerIndex++}", expected, header.MatchesFilter(filter.Filter));
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ExitControlFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filter = new ExitControlFilterBusinessObject();
		}
		ExitControlFilterBusinessObject filter;
	}
}
