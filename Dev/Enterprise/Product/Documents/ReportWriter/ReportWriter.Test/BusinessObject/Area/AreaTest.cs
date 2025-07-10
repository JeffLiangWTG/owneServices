using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(Area))]
	sealed class AreaTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new Area(ReportBizObj);
		}

		ReportBizObj ReportBizObj
		{
			get { return reportBizObj ?? (reportBizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj reportBizObj;
	}
}
