using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public static class TriageAssistHelper
	{
		public static void TriageAssistOnSaved(TriageAssistBusinessObject assistObject, BusinessObject parentObject)
		{
			var triage = assistObject.Parent.IncidentTriage;
			if (triage != null && (!assistObject.Parent.Module.IsEmpty || !assistObject.Parent.Product.IsEmpty || !assistObject.Parent.ProductArea.IsEmpty))
			{
				if (triage.IMT_Type.Equals(IncidentTriageTypes.Codes.Support) && !assistObject.Parent.nonTriageOverridableCriticalities.Contains(assistObject.Parent.Priority))
				{
					OverrideCriticalityAndStage(Constants.CustomerService.CriticalityCodes.CR5_Training, assistObject.Parent, SupportIncidentCategoriesList.Codes.Support);
				}
				else if (triage.IMT_Type.Equals(IncidentTriageTypes.Codes.Compliance))
				{
					OverrideCriticalityAndStage(Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, assistObject.Parent, SupportIncidentCategoriesList.Codes.Support);
				}
				else if (triage.IMT_Type.Equals(IncidentTriageTypes.Codes.Service))
				{
					OverrideCriticalityAndStage(Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, assistObject.Parent, SupportIncidentCategoriesList.Codes.CustomerServiceRequest);
				}
				OverrideProductClassification(triage, assistObject.Parent, parentObject);
			}
		}

		static void OverrideCriticalityAndStage(ZString criticalityCode, ITriageAssistParent assistParent, ZString stage)
		{
			assistParent.Priority = criticalityCode;
			assistParent.Category = stage;
		}

		static void OverrideProductClassification(IncidentTriage triage, ITriageAssistParent assistParent, BusinessObject parentObject)
		{
			if (!triage.IMT_SetProductAreaByMenuItem)
			{
				OverrideProductClassification(triage.IMT_Product, triage.IMT_ProductArea, triage.IMT_Module, "", assistParent, parentObject);
			}
			else
			{
				if (!assistParent.SourceModuleId.IsEmpty && !assistParent.SourceModuleId.EqualsIgnoringCase(IncidentApproval.NotAvailableActiveModuleID) && !assistParent.IsSourceModuleOverriden)
				{
					if (Globals.Message.Show($@"The following Menu Item already exists on the eRequest and was populated via the client using the F1 Help functionality:

{assistParent.SourceModuleWithPath}

Do you want to finalize this triage node using the existing Menu Item to determine Product Area (selecting no will allow you to override)?", "", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						return;
					}
				}

				var product = new[] { triage.IMT_Product, assistParent.Product }.FirstOrDefault(x => !x.IsEmpty);
				var module = triage.IMT_Module;
				var finder = new SourceModuleFinder(product, triage.ModuleType, "Please select the appropriate Menu Item to complete finalization of this Triage Node\r\nYour selection will determine the Product Area applied.", triage.Factory);
				finder.ModuleFilter = module;
				finder.RefreshSourceModules();

				if (finder.SourceModules.Any())
				{
					var form = new SourceModuleFinderForm(finder) { ShowDescriptionFilterOnly = true, Text = "Select Menu Item" };
					form.ModuleMappingWithSourceModuleSelected += (_, e) =>
					{
						OverrideProductClassification(product, e.ModuleMappingWithSourceModule.ProductArea, module, e.ModuleMappingWithSourceModule.SourceModuleCode, assistParent, parentObject);
					};

					ZFormModaliser.ShowDialogAndDispose(form);
				}
				else
				{
					var defaultMapping = EDIDataRegistry.Instance.SystemProductMappings.Value.GetProductByCode(product)?
						.ModuleMappings.OfType<ProductAreaModuleMapping>()
						.FirstOrDefault(x => x.ModuleCode.EqualsIgnoringCase(module) && x.IsEnabled);

					if (defaultMapping != null)
					{
						OverrideProductClassification(product, defaultMapping.ProductArea, module, IncidentApproval.NotAvailableActiveModuleID, assistParent, parentObject);
					}
				}
			}
		}

		static void OverrideProductClassification(ZString product, ZString productArea, ZString module, ZString sourceModuleId, ITriageAssistParent assistParent, BusinessObject parentObject)
		{
			if (new[] { product, productArea, module }.All(x => !x.IsEmpty))
			{
				using (parentObject.SetTempContext(SupportIncident.Context.OnOverrideProductClassification))
				{
					assistParent.Product = product;
					assistParent.ProductArea = productArea;
					assistParent.Module = module;

					if (!sourceModuleId.IsEmpty)
					{
						assistParent.SourceModuleId = sourceModuleId;
					}
				}
			}
		}
	}
}
