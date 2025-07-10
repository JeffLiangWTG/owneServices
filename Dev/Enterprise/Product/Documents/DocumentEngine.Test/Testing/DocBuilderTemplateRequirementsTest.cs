using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocBuilderTemplateRequirementsTest : TestCaseWithFactory
	{
		public void TestMakeSureExcelIndexedColorsHaveExpectedArgbValues()
		{
			foreach (var template in GetDocBuilderTemplates())
			{
				var templateName = template.SO_Name;

				using (var excelInterface = new ExcelInterface(template.SO_Template))
				{
					var excelFile = excelInterface.Xls;

					AssertIndexColor(string.Format("{0}: Document Heading", templateName), excelFile, 49, 0, 51, 102);
					AssertIndexColor(string.Format("{0}: Page Number Heading", templateName), excelFile, 46, 255, 102, 0);
					AssertIndexColor(string.Format("{0}: Primary Heading", templateName), excelFile, 48, 150, 150, 150);
					AssertIndexColor(string.Format("{0}: Secondary Heading", templateName), excelFile, 56, 51, 51, 51);
					AssertIndexColor(string.Format("{0}: Primary Body", templateName), excelFile, 24, 204, 204, 255);
					AssertIndexColor(string.Format("{0}: Secondary Body", templateName), excelFile, 40, 255, 204, 153);
				}
			}
		}

		void AssertIndexColor(string description, ExcelFile excelFile, int index, byte r, byte g, byte b)
		{
			var color = TExcelColor.FromIndex(index).ToColor(excelFile);

			AssertEquals(string.Format("{0} ({1}) Redcomponent", description, index), r, color.R);
			AssertEquals(string.Format("{0} ({1}) Green component", description, index), g, color.G);
			AssertEquals(string.Format("{0} ({1}) Blue component", description, index), b, color.B);
		}

		StmTemplate[] GetDocBuilderTemplates()
		{
			var query = new ZQuery(StmTemplateSchema.SO_IsUserConfigurable, ZBool.True);
			return Factory.Load<StmTemplate>(query);
		}
	}
}
