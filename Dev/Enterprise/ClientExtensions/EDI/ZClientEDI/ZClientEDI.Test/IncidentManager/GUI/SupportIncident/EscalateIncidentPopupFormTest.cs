using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(EscalateIncidentPopupForm))]
	public class EscalateIncidentPopupFormTest : BaseIncidentPopupFormTest
	{
		public void TestShowSourceModuleFinderPopupIfRequired()
		{
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "asd", true);
			var xxxMapping = product.ModuleMappings.AddNew("XXX", "XXX Description", "PA1", true);
			xxxMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("SourceModule1", "Menu Item A", "", ModuleListType.MenuSection, "PA1", true, true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			ZFormModaliser.ShowDialogsInTest = true;

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			var escalateAction = new SupportIncidentEscalateAction(incident, "");
			using (var form = new EscalateIncidentPopupForm(escalateAction))
			{
				form.Show();
				escalateAction.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
				escalateAction.SectionRequirementService = "XXX";
				escalateAction.ProductArea = "PA1";
				AssertNull("Should not show SourceModuleFInderPopup because Product Area is correct", ZFormModaliser.LastFormShownDialogForTest);
				escalateAction.ProductArea = "PA2";
				AssertNotNull("Should show SourceModuleFInderPopup because Product Area is incorrect", ZFormModaliser.LastFormShownDialogForTest);
				using (var shownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(SourceModuleFinderForm), shownForm);
					var sourceModuleFinderForm = (SourceModuleFinderForm)shownForm;
					var sourceModuleFinder = (SourceModuleFinder)sourceModuleFinderForm.LastDataSourceForTest;
					AssertNotNull("sourceModuleFinder", sourceModuleFinder);
					CombineAssertions("SourceModuleFinder properties should be correct", () =>
					{
						AssertEquals("ProductCode", ProductTypes.Codes.Enterprise, sourceModuleFinder.ProductCode);
						AssertEquals("ModuleType", ModuleListType.MenuSection, sourceModuleFinder.ModuleType);
						AssertMultilineASCIIEquals("FindReason", @"You have selected a 'Menu Section' that is shared by multiple Product Areas.
The current Menu Item does not belong to the currently selected Product Area (Product Area 2), please select the correct Menu Item to determine the correct Product Area.", sourceModuleFinder.FindReason);
						AssertEquals("ProductAreaFilter", "PA2", sourceModuleFinder.ProductAreaFilter);
						AssertEquals("ModuleFilter", "XXX", sourceModuleFinder.ModuleFilter);
					});

					shownForm.Close();
					AssertEquals("Product Area is reverted back as no source module is selected", "PA1", escalateAction.ProductArea);
				}

				ZFormModaliser.LastFormShownDialogForTest = null;
				incident.ProductArea = "";
				AssertNull("Should not show SourceModuleFInderPopup when Product Area is empty", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			return new EscalateIncidentPopupForm(action);
		}
	}
}
