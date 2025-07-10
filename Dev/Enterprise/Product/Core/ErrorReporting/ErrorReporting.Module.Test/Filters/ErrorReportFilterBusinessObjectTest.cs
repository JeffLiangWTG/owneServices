using System;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ErrorReporting.Module.Filters;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace Enterprise.ErrorReporting.Module.Test
{
	[TestedType(typeof(ErrorReportFilterBusinessObject))]
	public class ErrorReportFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterForTransmitStatus()
		{
			var strip = (ErrorReportFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filters = strip.ModuleFilters;
			AssertNotNull(filters);

			var filter = filters.SingleOrDefault(f => f.Description == "Transmit Status");
			AssertNotNull(filter);
			AssertType<ModuleTextFilter>(filter);

			var mtf = (ModuleTextFilter)filter;

			AssertEquals(StmErrorReportSchema.QER_TransmitStatus, mtf.FilterColumn);

			AssertNotNull(mtf.MultilingualDescription);

			AssertEquals(FilterCategories.ModesAndTypes, mtf.Category);

			var list = mtf.List;
			AssertType<StmErrorReportTransmitStatus>(list);
			var cdp = (CodeDescriptionPairList)list;

			AssertEquals(3, cdp.Count);
			AssertEquals(StmErrorReportTransmitStatus.Codes.Failed, cdp[0].Code);
			AssertEquals(StmErrorReportTransmitStatus.Descriptions.Failed, cdp[0].Description);

			AssertEquals(StmErrorReportTransmitStatus.Codes.Queued, cdp[1].Code);
			AssertEquals(StmErrorReportTransmitStatus.Descriptions.Queued, cdp[1].Description);

			AssertEquals(StmErrorReportTransmitStatus.Codes.Sent, cdp[2].Code);
			AssertEquals(StmErrorReportTransmitStatus.Descriptions.Sent, cdp[2].Description);
		}

		public void TestFilterForReportType()
		{
			var strip = (ErrorReportFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filters = strip.ModuleFilters;
			AssertNotNull(filters);

			var filter = filters.SingleOrDefault(f => f.Description == "Report Type");
			AssertNotNull(filter);
			AssertType<ModuleTextFilter>(filter);

			var mtf = (ModuleTextFilter)filter;

			AssertNull(mtf.FilterColumn);

			AssertNotNull(mtf.MultilingualDescription);

			AssertEquals(FilterCategories.ModesAndTypes, mtf.Category);

			var list = mtf.List;
			AssertType<ErrorReportTypeList>(list);
			var cdp = (CodeDescriptionPairList)list;

			AssertEquals(Enum.GetValues(typeof(ErrorReportType)).Length, cdp.Count);

			foreach (ICodeDescription pair in cdp)
			{
				Assert(Enum.IsDefined(typeof(ErrorReportType), (ErrorReportType)int.Parse(pair.Code)));
				AssertEquals((ErrorReportType)int.Parse(pair.Code), Enum.Parse(typeof(ErrorReportType), pair.Description));
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ErrorReportFilterBusinessObject();
		}
	}
}
