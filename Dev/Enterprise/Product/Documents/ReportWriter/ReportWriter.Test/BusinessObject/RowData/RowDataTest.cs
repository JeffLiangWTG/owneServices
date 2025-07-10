using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(RowData))]
	sealed class RowDataTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RowData(ReportBizObj);
		}

		ReportBizObj ReportBizObj
		{
			get { return reportBizObj ?? (reportBizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj reportBizObj;
	}
}
