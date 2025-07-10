using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(ColumnHeading))]
	sealed class ColumnHeadingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ColumnHeading(ReportBizObj);
		}

		ReportBizObj ReportBizObj
		{
			get { return reportBizObj ?? (reportBizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj reportBizObj;
	}
}
