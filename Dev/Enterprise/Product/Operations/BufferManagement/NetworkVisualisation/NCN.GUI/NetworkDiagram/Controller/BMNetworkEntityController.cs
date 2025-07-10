using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	//<summary>
	//Performs GUI-specific operations for JobNetwork and network actions. Should not contain business logic.
	//</summary>
	public class BMNetworkEntityController : NetworkEntityController, IBMNetworkEntityController
	{
		public BMNetworkEntityController(INetworkRefresher refresher, ZForm zForm)
			: this(new BMNetworkUserInteractionImplementor(new DefaultProgressReporterProvider(zForm)), refresher, zForm)
		{
		}

		BMNetworkEntityController(IBMNetworkUserInteractionImplementor userInteractionImplementor, INetworkRefresher refresher, ZForm zForm)
			: this(userInteractionImplementor, MakeFormSaveAction(userInteractionImplementor, zForm, refresher))
		{
			if (zForm != null)
			{
				void zForm_Disposed(object sender, EventArgs e)
				{
					// remove all strong references to the form on form disposal
					zForm.Disposed -= zForm_Disposed;
					Deactivate();
				}

				zForm.Disposed += zForm_Disposed;
			}
		}

		public BMNetworkEntityController(IBMNetworkUserInteractionImplementor userInteractionImplementor, NetworkControllerSaveAction saveAction)
			: base(userInteractionImplementor)
		{
			this.saveAction = saveAction;
		}

		protected override void Deactivate()
		{
			base.Deactivate();
			saveAction = null;
		}

		public new IBMNetworkUserInteractionImplementor UserInteractionImplementor => (IBMNetworkUserInteractionImplementor)base.UserInteractionImplementor;

		public WorkflowParentJobCreator JobCreator { get; private set; }

		#region Save Action

		NetworkControllerSaveAction saveAction;

		static NetworkControllerSaveAction MakeFormSaveAction(IBMNetworkUserInteractionImplementor userInteractionImplementor, ZForm zForm, INetworkRefresher refresher)
		{
			if (zForm != null)
			{
				EventHandler disposedHandler = null;
				EventHandler refresh = (s, e) => refresher.Refresh(RefreshType.Saving);

				disposedHandler = (s, e) =>
				{
					zForm.Disposed -= disposedHandler;
					zForm.BeforePerformValidation -= refresh;
				};

				zForm.BeforePerformValidation += refresh;
				zForm.Disposed += disposedHandler;

				return () => zForm.FireSaveButton();
			}
			else
			{
				return () =>
				{
					var message = Res.GetString("4a891fa9-d249-4131-b9a4-d327e8f0df2b", "Please save the form first.");
					userInteractionImplementor.ShowMessage(message);
					return ContinueWithSave.No;
				};
			}
		}

		public ContinueWithSave TriggerSaveAction() => saveAction != null ? saveAction() : ContinueWithSave.No;

		#endregion

		public void ViewDiagram(INetworkEntity entity)
		{
			var shape = entity.AsShape();
			var controller = ZControllerFactory.Create(ControllerIDs.NetworkDiagram);

			if (shape.IsInDatabase)
			{
				if (shape.HasChanges)
				{
					throw new InvalidOperationException("Entity should be saved first.");
				}

				controller.ShowEditForm(shape);
			}
			else
			{
				controller.ShowFormForNewEntity(shape);
			}
		}

		public void OpenLinkedEntity(BusinessObject linkedEntity, ControllerID controllerID)
		{
			if (linkedEntity == null)
			{
				UserInteractionImplementor.ShowMessage(Res.GetString("ed19edca-b5da-4c94-9c2b-59fade05f314", "This shape is not linked to a job, workflow or another diagram. You can link this shape from the Linked Entity option, or by copying and pasting a hyperlink for an entity directly onto the shape."));
			}
			else
			{
				ZControllerFactory.Create(controllerID).ShowEditForm(linkedEntity);
			}
		}

		public void ShowNetworkDiagramsModule(INetworkEntity entity)
		{
			ZQuery query = entity.AsShape().BNS_BNS_RootShape == ZGuid.Empty ?
				new ZQuery(BMNCNShapeSchema.BNS_BNS_RootShape, entity.EntityPK) :
				ZQuery.NoResultQuery;

			BusinessObjectModulePicker.ShowModuleScreen(ModuleIDs.NetworkDiagram, module =>
				{
					module.AddAdditionalDisplayFilter = q => q.AddToFilter(query);
				},
				BusinessObjectModulePicker.FilterLayoutStrategy.Empty, isReadOnly: false);
		}

		public BusinessObject PickEntity(ModuleIdentifier moduleID, bool shouldAllowDiagramShapesOnly = false)
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleID))
			{
				var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(PickEntity) };
				var emptyLayout = factory.New<StmModuleFilter>();
				if (shouldAllowDiagramShapesOnly && module.GridCollection is DiagramShapeCollection collection)
				{
					var filterDefault = new FilterBusinessObjectDefault(BMNCNShape.ModuleFilterConstants.DiagramType, nameof(ModuleTextFilter.Property), new ZString(ShapeTypeList.Codes.Diagram), isRemovable: false);
					collection.FilterBusinessObjectDefaults.Add(filterDefault);
					return BusinessObjectModulePicker.PickOneRecordFromModuleScreen<BusinessObject>(collection, moduleID, emptyLayout, shouldLoadLayoutEvenWhenUnsaved: true);
				}

				return BusinessObjectModulePicker.PickOneRecordFromModuleScreen<BusinessObject>(module.GridCollection, moduleID, emptyLayout, shouldLoadLayoutEvenWhenUnsaved: true);
			}
		}

		public void EditEntity(IProposedNetworkEntity entity)
		{
			ZFormModaliser.ShowDialogAndDispose(new ShapeEntityDetailsForm((ShapeNetworkEntity)entity));
		}

		public void ModifyAffinities(IDiagramEntity diagramEntity)
		{
			var form = new ModifyAffinitiesForm(diagramEntity.AsShape());

			ZFormModaliser.ShowDialogAndDispose(form);
		}

		public ProcessJobHeader CreateJob(string workflowType, BusinessObjectFactory factory, string name = null)
		{
			JobCreator = new WorkflowParentJobCreator(workflowType);
			return JobCreator.CreateJob(factory, name);
		}

		public void DeleteJob()
		{
			if (JobCreator != null)
			{
				JobCreator.DeleteJob();
			}
		}

		public ProcessJobHeader ShowNewFormAsDialogAndGetSaved()
		{
			if (JobCreator != null)
			{
				return JobCreator.ShowNewFormAsDialogAndGetSaved();
			}
			else
			{
				throw new InvalidOperationException("Must create job first");
			}
		}

		public IEnumerable<BusinessObject> GetJobsFromClipboard(BusinessObjectFactory factory)
		{
#if !WINZOR

			var hyperlink = ObjectFactory.Get<IClipboard>().GetDataObject();
			var hyperlinkData = hyperlink?.GetData(DataFormats.Html) as string ?? hyperlink?.GetData(DataFormats.Text) as string;
#else
			var hyperlink = SafeClipboardHelper.Instance.GetDataObjectDict();
			var hyperlinkData = hyperlink.GetValueOrDefault(System.Net.Mime.MediaTypeNames.Text.Html) ?? hyperlink.GetValueOrDefault(System.Net.Mime.MediaTypeNames.Text.Plain);
#endif

			if (hyperlinkData is null)
			{
				yield break;
			}

			var urls = ZMenuStrategyHelper.ShortcutCreator.TryGetUrlsFromHyperlinkBadly(hyperlinkData);
			foreach (var url in urls)
			{
				if (url is null)
				{
					continue;
				}

				var tuple = ZFormUtilities.GetControllerIDsFromURLSafe(url);

				if (tuple is null)
				{
					continue;
				}

				var controller = ZControllerFactory.Create(tuple.Item1);
				var result = factory.Load(controller.TypeOfTopLevelBusinessObject, tuple.Item2);

				if (result != null)
				{
					yield return result;
				}
			}
		}
	}
}
