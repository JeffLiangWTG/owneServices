using System;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class CreateJobActionBase : CreateShapeActionBase
	{
		protected CreateJobActionBase(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true) : base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckShapeCanHaveChildren(shape));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return (!shape.IsDiagram ? BMNetworkActionAccessibilityHelper.CheckShapeIsNotLinkedToRealEntity(shape) : BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(shape));
		}

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entityToExecute)
		{
			var shape = entityToExecute.AsShape();

			if (!shape.IsDiagram)
			{
				if (shape.IsLinkedToRealEntity) // this should NOT be possible
				{
					throw new InvalidOperationException("This shape is already linked to a real entity.");
				}

				SetDefaultsForNewShapeCore(Network, shape, null);
				return null;
			}

			var result = base.ExecuteForEntityCore(entityToExecute);

			Network.FullRefresh();

			return result;
		}

		protected override ProcessHeader GetNewProcessHeaderForShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			if (newShape.BNS_JobType.IsEmpty)
			{
				if (GetUserConfirmation(Res.GetString("219114c8-070c-4ec3-9e18-da9bbd2cee49", "This shape does not have a Job Type specified. Open the shape properties form now?")))
				{
					Controller.EditEntity(Network.Entities.GetInstance(newShape));
				}
			}

			if (newShape.BNS_JobType.IsEmpty)
			{
				var message = Res.GetString("89aba57b-0f66-4de8-aa51-1ae7e9596eb2", "Cannot create a job without a Job Type specified.");
				UserInteractionImplementor.ShowMessage(message);
				return null;
			}

			return CreateJobForShape(newShape);
		}

		internal ProcessHeader CreateJobForShape(BMNCNShape newShape)
		{
			Controller.CreateJob(newShape.BNS_JobType, newShape.Factory.CreateNewFactory(), newShape.BNS_Name);
			var jobHeader = Controller.ShowNewFormAsDialogAndGetSaved();

			if (jobHeader != null)
			{
				jobHeader = newShape.Factory.Load<ProcessJobHeader>(jobHeader.PK);
				newShape.Name = ((IProposedNetworkEntity)jobHeader).JobName; // Set shape name again in case the user changed it while the job form was open.

				return jobHeader;
			}

			Controller.DeleteJob();
			return null;
		}
	}
}
