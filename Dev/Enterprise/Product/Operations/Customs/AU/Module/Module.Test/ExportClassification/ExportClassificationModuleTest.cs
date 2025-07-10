using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ExportClassificationModuleForTest))]
	class ExportClassificationModuleTest : Customs.Module.Testing.ExportClassificationModuleTest
	{
		public void TestGetNewFilterControl()
		{
			using (ExportClassificationModuleForTest module = new ExportClassificationModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AUExportClassificationFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (ExportClassificationModuleForTest module = new ExportClassificationModuleForTest())
			{
				IBusinessObjectCollection collection = module.NewGridCollection;
				Assert("Invalid type", collection is ExportClassificationCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (ExportClassificationModuleForTest module = new ExportClassificationModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AUExportClassificationFilterBusinessObject);
			}
		}

		public void TestCSVImportMenuItemHasBeenAdded()
		{
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestExportClassificationModule.EmbeddedControl);
				form.Show();
				MenuItem result = null;
				foreach (MenuItem item in TestExportClassificationModule.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems)
				{
					if (item.Text.Equals("Import From CSV"))
					{
						result = item;
						break;
					}
				}

				AssertNotNull(result);
				result.PerformClick();
				ImportClassificationsFromCSVForm popupForm = ZFormModaliser.ActiveForm as ImportClassificationsFromCSVForm;
				AssertNotNull(popupForm);
				popupForm.Dispose();
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override Customs.Module.ExportClassificationModule GetNewExportClassificationModule() => new ExportClassificationModule();

		ExportClassificationModule TestExportClassificationModule => (ExportClassificationModule)testExportClassificationModule;

		sealed class ExportClassificationModuleForTest : ExportClassificationModule
		{
			public ExportClassificationModuleForTest()
			{
			}

			public IFilterControl NewFilterControl => GetNewFilterControl();

			public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

			public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
		}
	}
}
