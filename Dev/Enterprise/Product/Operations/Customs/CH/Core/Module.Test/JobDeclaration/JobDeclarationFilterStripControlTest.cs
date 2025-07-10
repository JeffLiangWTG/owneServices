using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(JobDeclarationFilterStripControl))]
sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
{
	public void TestAddedColumns() => CombineAssertions(() =>
	{
		var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		using (var module = new JobDeclarationModule())
		using (var userControl = new JobDeclarationFilterStripControl(module, declarations, new JobDeclarationFilterBusinessObject()))
		{
			var grid = userControl.FilteredGrid;
			grid.SetDataBinding(declarations, "");
			var addedColumns = new List<(string, string)>()
			{
				(JobDeclaration.Schema.PhaseStatus, "Phase Status"),
				(JobDeclaration.Schema.PhaseStatusDescription, "Phase Status Description"),
				(JobDeclaration.Schema.SelectionResult, "Selection Result"),
				(JobDeclaration.Schema.SelectionResultDescription, "Selection Result Description"),
			};

			foreach (var (column, caption) in addedColumns)
			{
				AssertEquals(caption, grid.GetColumnCaption(column));
			}
		}
	});
}
