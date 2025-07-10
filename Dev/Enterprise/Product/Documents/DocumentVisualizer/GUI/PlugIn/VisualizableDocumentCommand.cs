using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class VisualizableDocumentCommand : IVisualizableDocumentCommand
	{
		public VisualizableDocumentCommand(BusinessObject bizObj, IStmMenuItem menuItem, ModuleIdentifier moduleID)
		{
			Argument.NotNull(bizObj, nameof(bizObj));
			Argument.NotNull(menuItem, nameof(menuItem));

			this.bizObj = bizObj;
			this.moduleID = moduleID;
			this.menuItem = menuItem;
		}

		readonly BusinessObject bizObj;
		readonly ModuleIdentifier moduleID;
		readonly IStmMenuItem menuItem;

		public string Name => menuItem.SU_MenuNameMultilingual;

		public bool IsApplicable => menuItem.SU_MenuType == Enterprise.Core.Constants.StmMenuItemTypes.Forms
			&& bizObj.IsApplicable(menuItem.SU_FilterList)
			&& menuItem.Documents?.Count > 0;

		public void Execute()
		{
			if (menuItem.SU_MenuType != Enterprise.Core.Constants.StmMenuItemTypes.Forms
					|| !bizObj.IsApplicable(menuItem.SU_FilterList))
			{
				Globals.Message.Show(
					ResString.GetMultilingualString("2322c5e4-4413-4ff0-aaf9-6773306e8e9d",
						"'{0}' form cannot be shown because the filter doesn't match current execution context.", menuItem.SU_MenuNameMultilingual));
				return;
			}

			if (menuItem.Documents == null || menuItem.Documents.Count == 0)
			{
				Globals.Message.Show(
					ResString.GetMultilingualString("0fcb68ef-89e7-445b-8672-214a9aab0158",
						"'{0}' form cannot be shown because there is no document linked.", menuItem.SU_MenuName));
				return;
			}

			if (bizObj.HasChanges || !bizObj.IsInDatabase)
			{
				Globals.Message.Show(
					ResString.GetMultilingualString("11970b05-31f5-41f8-8df7-2cd0d0eec5a8", "Please save before opening '{0}' form.", menuItem.SU_MenuNameMultilingual));
				return;
			}

			var supporter = bizObj.GetSupporter();

			var additionalData = supporter?.GetAdditionalData(bizObj, menuItem)
				?? new Either<string, object>((object)null);

			if (additionalData.IsLeft)
			{
				if (!string.IsNullOrWhiteSpace(additionalData.Left))
				{
					Globals.Message.Show(additionalData.Left);
				}
				return;
			}

			ShowForm(additionalData.Right);
		}

		void ShowForm(object additionalData)
		{
			Argument.NotNull(menuItem, nameof(menuItem));
			Argument.NotNull(bizObj, nameof(bizObj));

			var securityService = new DocumentSecurityService(menuItem, moduleID);

			if (!securityService.CanView)
			{
				securityService.ShowViewError();
				return;
			}

			var pivots = menuItem
				.Documents
				.OfType<IStmMenuTemplatePivot>()
				.ToArray();

			if (!pivots.Any())
			{
				var message = Res.GetString("be00c781-a019-47c6-b5c3-5b6952275c3b", "Document '{0}' cannot be shown because it has no link to a template.", menuItem.SU_MenuNameMultilingual);
				Globals.Message.ShowError(message);
				return;
			}

			var matchingPivots = pivots
				.Where(pivot => bizObj.IsApplicable(pivot.SI_MenuTemplateFilter))
				.ToArray();

			if (!matchingPivots.Any())
			{
				var message = Res.GetString("fe6d06cb-d6d4-46e6-b615-5a22333632d3", "None of the documents matches the filtering criteria.");
				Globals.Message.ShowError(message);
				return;
			}

			if (!CheckDeliveryRestrictionType(menuItem, bizObj))
			{
				return;
			}

			var factory = new BusinessObjectFactory();
			factory.NameForDebugging = "VisualizableDocumentCommandFactory";

			var supporter = bizObj.GetSupporter();

			var bizO = (BusinessObject)supporter?.GetBusinessObjectInAnotherFactory(factory, bizObj)
				?? factory.ImportFromAnotherFactory(bizObj);

			if (bizO == null)
			{
				return;
			}

			var orderedPivots = matchingPivots
				.OrderBy(p => p.PK == menuItem.SU_PrimaryDocPackItemId ? -1 : p.SI_Index)
				.ToArray();

			var broker = new EventBroker();
			var documentDeliveryService = new DocumentDeliveryService(broker, menuItem);
			var services = GetServices(broker, securityService, documentDeliveryService);
			var pack = new DocumentInfoPack(services, bizO, additionalData, menuItem.SU_MenuNameMultilingual, orderedPivots);

			DocumentVisualizerForm form;

			using (var progressManager = ObjectFactory.Get<IProgressManager>())
			{
				progressManager.Start();

				using (broker.GetEvent<ProgressInfoEvent>().Subscribe(info => progressManager.UpdateStatus(info.Message)))
				{
					form = DocumentVisualizerForm.CreateView(pack);

					IDisposable disposableSubscription = null;
					disposableSubscription = broker.GetEvent<DocumentHardRefreshEvent>().Subscribe(_ =>
					{
						disposableSubscription.Dispose();

						form.Visible = false;
						form.Close();
						form.Dispose();

						ShowForm(additionalData);
					});
				}
			}

			if (form != null)
			{
				using (factory.AddDisposableService())
				{
					ZFormModaliser.ShowDialogAndDispose(form);
				}
			}
		}

		static bool CheckDeliveryRestrictionType(IStmMenuItem menuItem, BusinessObject bizObj)
		{
			if (menuItem.SU_DeliveryRestrictionType != nameof(DeliveryRestrictionType.CNH)
				&& menuItem.SU_DeliveryRestrictionType != nameof(DeliveryRestrictionType.UDF))
			{
				return true;
			}

			if (!(bizObj is ICreditControlledBusinessObject creditControlledObj))
			{
				return true;
			}

			using (var guiManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
			{
				guiManager.Initialise(creditControlledObj);

				var caption = Res.GetString("e067dd1a-74a6-4e98-8fd5-8acb86e68966", "Unable To Run This Form");

				if (menuItem.SU_DeliveryRestrictionType == nameof(DeliveryRestrictionType.CNH))
				{
					var errorMessage = ObjectFactory.Get<IDocumentDeliveryCreditControlManager>()
						.GetDocumentDeliveryStatusForCreditManagement(bizObj,
							Res.GetString("a4dece1d-7b62-4aaa-9d9e-2f591cb6a4a5", "form"), menuItem.PK);

					if (!string.IsNullOrEmpty(errorMessage))
					{
						if (!errorMessage.Contains(SecurityLogin.CancelledText, StringComparison.CurrentCulture))
						{
							Globals.Message.ShowError(errorMessage, caption);
						}

						return false;
					}
				}
				else
				{
					var restrictionDescription = string.IsNullOrEmpty(menuItem.SU_DeliveryRestrictionDescription)
						? string.Empty
						: Res.GetString("3c942be7-f83f-47c0-8a86-7a3f41ede335", "Condition description: {0}",
							menuItem.SU_DeliveryRestrictionDescription);
					var errorMessage = IsUserDefinedConditionMet(bizObj, menuItem.SU_DeliveryRestrictionMacro)
						? ZString.Empty
						: (ZString)Res.GetString("9af7c102-2395-4f52-bc28-b6094e4432d5",
							"User defined delivery restriction condition is not met. {0}", restrictionDescription);
					if (!errorMessage.IsEmpty)
					{
						Globals.Message.ShowError(errorMessage, caption);
						return false;
					}
				}
			}

			return true;
		}

		static bool IsUserDefinedConditionMet(BusinessObject businessObject, string macro)
		{
			using (var scope = businessObject.CreateFilterEvaluatorScope())
			{
				var expr = macro
					.With(FilterExtensions.FilterContext)
					.CreateExpression();

				var result = expr.Evaluate(scope);
				return !expr.HasErrors() && result is bool && (bool)result;
			}
		}

		static IServiceContainer GetServices(IEventBroker broker, IDocumentSecurityService securityService, IDocumentDeliveryService documentDeliveryService)
		{
			IServiceContainer container = new ServiceContainer();

			container.Register<IPageViewBuildService>(() => new PageViewBuildService());
			container.Register<INotificationViewBuildService>(() => new NotificationViewBuildService());
			container.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			container.Register<IUserNotificationService>(() => new UserNotificationService());
			container.Register<IDocumentToolsService>(() => new DocumentToolsService());
			container.Register<IConsoleService>(new ConsoleService());
			container.Register<IEventBroker>(broker);
			container.Register<IResourceProvider>(new EmbeddedResourcesProvider());
			container.Register<IDocumentDeliveryService>(documentDeliveryService);
			container.Register<IDocumentSecurityService>(securityService);

			return container;
		}
	}
}
