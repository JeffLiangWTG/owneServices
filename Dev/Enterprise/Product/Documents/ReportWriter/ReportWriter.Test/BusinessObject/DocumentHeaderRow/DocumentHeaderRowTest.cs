using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(DocumentHeaderRow))]
	sealed class DocumentHeaderRowTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentHeaderRow(ReportBizObj);
		}

		ReportBizObj ReportBizObj
		{
			get { return reportBizObj ?? (reportBizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj reportBizObj;
	}
}
