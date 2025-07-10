using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(ColumnData))]
	sealed class ColumnDataTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ColumnData(ReportBizObj);
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				if (!result.ContainsKey(ColumnData.Schema.ColumnNumberString))
				{
					result.Add(ColumnData.Schema.ColumnNumberString, new ZString("2"));
				}
				return result;
			}
		}

		ReportBizObj ReportBizObj
		{
			get { return reportBizObj ?? (reportBizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj reportBizObj;
	}
}
