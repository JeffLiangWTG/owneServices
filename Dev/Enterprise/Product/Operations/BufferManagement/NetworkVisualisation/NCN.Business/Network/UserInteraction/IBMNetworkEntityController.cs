using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public delegate ContinueWithSave NetworkControllerSaveAction();

	public interface IBMNetworkEntityController : INetworkEntityController
	{
		ContinueWithSave TriggerSaveAction();

		void ViewDiagram(INetworkEntity entity);
		void OpenLinkedEntity(BusinessObject linkedEntity, ControllerID controllerID);
		void EditEntity(IProposedNetworkEntity entity);

		BusinessObject PickEntity(ModuleIdentifier moduleID, bool shouldAllowDiagramShapesOnly = false);
		void ShowNetworkDiagramsModule(INetworkEntity entity);

		void ModifyAffinities(IDiagramEntity diagramEntity);

		ProcessJobHeader CreateJob(string workflowType, BusinessObjectFactory factory, string name = null);
		IEnumerable<BusinessObject> GetJobsFromClipboard(BusinessObjectFactory factory);
		void DeleteJob();

		ProcessJobHeader ShowNewFormAsDialogAndGetSaved();

		new IBMNetworkUserInteractionImplementor UserInteractionImplementor { get; }
	}
}
