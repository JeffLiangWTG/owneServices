using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public interface IJobNetwork : INetwork
	{
		#region Redefined Members

		new ShapeNetworkEntity DiagramEntity { get; }
		new NetworkEntityCollection Entities { get; }
		new IBMNetworkEntityController Controller { get; }

		#endregion

		IMultiActionButtonDialogWrapper<DeleteWorkflowOption> DeleteWorkflowDialogWrapper { get; set; }

		BMNCNShape DiagramShape { get; }
		BMNCNShapeCollection Shapes { get; }
		ShapeNetworkEntity AddNewShape(BMNCNShape shape);

		string EntityTypeDescription { get; }
		NetworkActions SupportedActions { get; }

		void FullRefresh();
		void RefreshSchedules(bool forceReCalculation = false);

		IEntityRelationship CreateRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity, BMNCNShape ownerShape, bool useAttachementValidation = true, bool refreshDiagram = true);
		bool LinkEntity(BMNCNShape shape, BusinessObject entityToLink);
		void LinkEntity(IProposedNetworkEntity entity, ModuleIdentifier moduleIDForLinkedEntityType);
		bool LinkEntity(ShapeNetworkEntity entity, BusinessObject entityToLink);
		void ShowNetworkDiagramsModule(INetworkEntity entity);
		void OpenLinkedEntity(BMNCNShape shape);
		void SwitchToScaled();
		void UnlinkEntity(IProposedNetworkEntity entity);

		IEnumerable<ShapeNetworkEntity> GetCriticalChain();
		TimeSpan GetCriticalChainDuration();

		bool ValidateAndCheckThereAreNoErrors();
		bool ValidateLoopsAndCheckThereAreNoErrors(string progressReporterCaption = null);

		event PropertyChangedEventHandler PropertyChanged;

#if DEBUG
		BMNCNShape this[string name] { get; }

		event EventHandler<RefreshArgs> Refreshed;
#endif
	}
}
