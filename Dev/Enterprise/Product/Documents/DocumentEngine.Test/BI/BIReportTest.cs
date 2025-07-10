using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	[TestedType(typeof(BiReport))]
	sealed class BIReportTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BiReport(new DocumentPack(), new ExcelTemplateForBi("BiReportTemplates.xls"));
		}
	}
}
