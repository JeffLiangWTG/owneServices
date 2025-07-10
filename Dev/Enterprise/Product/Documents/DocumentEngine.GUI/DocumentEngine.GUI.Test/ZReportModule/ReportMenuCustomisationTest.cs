using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(ReportMenuCustomisation))]
	sealed class ReportMenuCustomisationTest : MenuCustomisationTestCase<ReportMenuCustomisation>
	{
		public void TestSetAdditionalProperties()
		{
			var menu = Customisation.Menus.AddNew();
			menu.SU_IsSystemDefined = true;
			var template = Factory.New<StmTemplateBase>();
			template.FillWithValidTestData();

			var pivot = Customisation.AddTemplatePivot(menu, template);
			AssertEquals("DocType should be empty", ZGuid.Empty, pivot.SI_RT_DocType);

			menu = Customisation.Menus.AddNew();
			menu.SU_IsSystemDefined = false;
			pivot = Customisation.AddTemplatePivot(menu, template);
			var docType = Factory.Load<RefDocType>(pivot.SI_RT_DocType);
			AssertEquals("DocType should be SREP", "SREP", docType.RT_DocType);
		}

		public override void TestAvailableTemplates()
		{
			StmTemplateBaseCollection templates = new StmTemplateBaseCollection(Factory);
			templates.Load(new ZQuery(StmTemplateSchema.SO_DataContext, nameof(Core.Constants.DataContext.None)));

			Func<StmTemplateBase, string> templateAsString = (template) =>
				{
					return string.Format("{0},{1},{2},{3}", template.SO_Name, template.SO_TemplateType, template.SO_DataContext, template.SO_ExcelTemplatePath);
				};

			AssertContainsExactElementsInAnyOrder("AvailableTemplates",
				templates.Cast<StmTemplateBase>().Select(t => templateAsString(t)),
				Customisation.AvailableTemplates.Cast<StmTemplateBase>().Select(t => templateAsString(t)));
		}

		public override void TestGetPrintTask()
		{
			StmMenuItemBase menu = Customisation.Menus[0];
			ReportPrintSet printTask = (ReportPrintSet)Customisation.GetPrintTask(menu);
			AssertEquals("GetPrintTask().ParentMenuCommand", menu, printTask.ParentMenuCommand);
		}

		public override void TestMenus()
		{
			ReportCommandCollection reports = new ReportCommandCollection(Factory, "RepRefFilesReports");
			reports.Load();
			AssertCollectionEquals(reports, Customisation.Menus, "Menus");
		}

		#region Implementation

		protected override MenuCustomisationDocumentSupporter SupportedDocumentSupporter
		{
			get
			{
				return new ReportMenuCustomisationDocumentSupporter(Customisation);
			}
		}

		protected override Core.Constants.DataContext DataContext
		{
			get { return Core.Constants.DataContext.None; }
		}

		protected override string ValidTemplateFilePath
		{
			get { return Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\ReportTestFiles\UDF with defaults.xls"); }
		}

		protected override string ValidTemplateFilePathReleaseBuild
		{
			get { return "UDF with defaults.xls"; }
		}

		protected override KeyValuePair<string, string>[] GetInvalidTemplateFilePathAndValidationErrorMessagePairs()
		{
			return new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\DummyBusinessObjectAsDataSource.xls"), "This template cannot be added because it requires a specific data context but reports cannot have a data context.")
			};
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReportMenuCustomisation(Factory, "RepRefFilesReports");
		}

		#endregion
	}
}
