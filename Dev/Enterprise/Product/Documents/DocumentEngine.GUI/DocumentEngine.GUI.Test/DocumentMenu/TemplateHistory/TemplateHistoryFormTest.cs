using System.Windows.Forms;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing;

[TestedType(typeof(TemplateHistoryForm))]
public class TemplateHistoryFormTest  : ZFormBasherTest
{
	public void TestTemplateHistoryForm()
	{
		using (var form = GetFormToBashCore() as TemplateHistoryForm)
		{
			form.Show();
			Assert(form.TemplateHistoryDetailGrid.ReadOnly);
			Assert(!form.TemplateHistoryDetailGrid.Visible);
			Assert(form.EmptyLabel.Visible);
		}
		var templateData1 =  DocumentEngineTestHelper.CreateTemplateFromString(
			@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]");
		var templateData2 =  DocumentEngineTestHelper.CreateTemplateFromString(
			@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");

		var template = Factory.NewWithValidTestData<StmTemplateBase>();
		template.SO_Template = templateData1;
		template.SO_Name = "ATest";
		template.SO_ExcelTemplatePath = "Test.xls";
		Factory.Save();

		template.SO_Template = templateData2;
		Factory.Save();

		Assert(template.TemplateHistories != null);
		using (var form = new TemplateHistoryForm(template))
		{
			form.Show();
			Assert(form.TemplateHistoryDetailGrid.ReadOnly);
			Assert(form.TemplateHistoryDetailGrid.Visible);
			Assert(!form.EmptyLabel.Visible);
			var menuItem = form.TemplateHistoryDetailGrid.ContextMenu.MenuItems.FindByText("Save As...");
			AssertNotNull(menuItem);
			Assert(menuItem.Visible);
		}
	}

	protected override Form GetFormToBashCore()
	{
		return new TemplateHistoryForm(Factory.New<StmTemplateBase>());
	}
}