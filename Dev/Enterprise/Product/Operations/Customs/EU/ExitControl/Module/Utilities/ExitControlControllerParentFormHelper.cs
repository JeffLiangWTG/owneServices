using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Module;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	internal static class ExitControlControllerParentFormHelper
	{
		internal static IZForm ShowLoadedFormForParentJob(BusinessObject parent)
		{
			IZForm shownForm = null;
			if (parent is ForwardingShipment shipment)
			{
				shownForm = ShowShipmentForm(shipment);
			}
			else if (parent is ForwardingConsol consol)
			{
				shownForm = ShowConsolForm(consol);
			}
			else if (parent is JobDeclaration declaration)
			{
				if (declaration.Shipment is ForwardingShipment declarationParent)
				{
					shownForm = ShowShipmentForm(declarationParent);
				}
				else
				{
					var declarationController = (JobDeclarationController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
					declarationController.ShowEditForm(declaration, true);
					shownForm = declarationController.LastShownForm;
				}
			}
			else if (parent is CusExitHeader exitHeader)
			{
				var exitHeaderController = (ExitControlController)ZControllerFactory.Create(ControllerIDs.Customs.EU.ExitControl);
				exitHeaderController.ShowEditForm(exitHeader);
				shownForm = exitHeaderController.LastShownForm;
			}
			return shownForm;
		}

		static IZForm ShowShipmentForm(ForwardingShipment shipment)
		{
			IZForm shownForm;
			var shipmentController = (JobDeclarationShipmentController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
			shipmentController.ShowEditForm(shipment, true);
			shownForm = shipmentController.LastShownForm;
			return shownForm;
		}

		static IZForm ShowConsolForm(ForwardingConsol consol)
		{
			IZForm shownForm;
			var consolController = (JobConsolController)ZControllerFactory.Create(ControllerIDs.JobConsol);
			consolController.ShowEditForm(consol);
			shownForm = consolController.LastShownForm;
			return shownForm;
		}

		internal static void SelectExitControlTabPageInParentForm(IZForm form)
		{
			var zForm = (ZForm)form;

			if (zForm is ShipmentForm shipmentForm)
			{
				shipmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.Customs.EU.ExitSummaryController);
			}
			else if (zForm is ConsolForm consolForm)
			{
				consolForm.PlugIns.SelectPlugInTabPage(ControllerIDs.Customs.EU.ExitSummaryController);
			}
			else if (zForm is BaseJobDeclarationForm declarationForm)
			{
				declarationForm.PlugIns.SelectPlugInTabPage(ControllerIDs.Customs.EU.ExitSummaryController);
			}
		}

		internal static void AddToRecentItems(ZController controller, BusinessObject parent, IBusiness sourceEntity)
		{
			ZString description;
			if (parent is ForwardingShipment shipment)
			{
				description = $"{sourceEntity.HumanReadableName} ({shipment.JS_UniqueConsignRef})";
			}
			else if (parent is ForwardingConsol consol)
			{
				description = $"{sourceEntity.HumanReadableName} ({consol.JK_UniqueConsignRef})";
			}
			else if (parent is JobDeclaration declaration)
			{
				if (declaration.Shipment is ForwardingShipment declarationParent)
				{
					description = $"{sourceEntity.HumanReadableName} ({declarationParent.JS_UniqueConsignRef})";
				}
				else
				{
					description = $"{sourceEntity.HumanReadableName} ({declaration.JE_DeclarationReference})";
				}
			}
			else
			{
				description = sourceEntity.HumanReadableName;
			}

			AddToRecentItems(controller, sourceEntity.Identifier, description);
		}

		internal static IZForm ShowLoadedFormSelectExitControlAndAddToRecent(ZController controller, BusinessObject parent, IBusiness sourceEntity)
		{
			var result = ShowLoadedFormForParentJob(parent);
			SelectExitControlTabPageInParentForm(result);
			AddToRecentItems(controller, parent, sourceEntity);

			return result;
		}

		static void AddToRecentItems(ZController controller, ZGuid key, string description)
		{
			var urlHandler = ShowEditFormUrlHandler.Instance.Create(controller.ID, key);
			var linkWrapper = new LinkWrapper(controller.ModuleID.Name, key.ToGuid(), urlHandler, description);
			ObjectFactory.Get<IFavoriteProvider>().AddToRecentItems(linkWrapper);
		}
	}
}
