using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class JobNetworkActionBase : DynamicNetworkAction, INotifyPropertyChanged
	{
		protected JobNetworkActionBase(INetworkViewModel networkViewModel, INetworkActionExecutionStrategy executionStrategy = null, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, executionStrategy, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Accessors

		public IJobNetwork Network => NetworkViewModel.GetJobNetwork();

		public IBMNetworkEntityController Controller => NetworkViewModel.GetJobController();

		public IBMNetworkUserInteractionImplementor UserInteractionImplementor => Controller?.UserInteractionImplementor;

		#endregion

		#region Testing
#if DEBUG
		public bool CanPerformOnApprovedShape_ExposedForTesting => CanPerformOnApprovedShape;
#endif
		#endregion

		#region Implementation

		#region Network Action Overrides

		#region Main Network Action Attributes

		protected override sealed ResourceString GetNameCore(INetworkEntity activeEntity) => GetNameCore(activeEntity.AsShape());

		protected override sealed ResourceString GetDescriptionCore(INetworkEntity activeEntity) => GetDescriptionCore(activeEntity.AsShape());

		protected override sealed bool IsActivatedCore(INetworkEntity activeEntity) => IsActivatedCore(activeEntity.AsShape());

		protected override sealed IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity) => GetChildActionsCore(activeEntity.AsShape());

		#endregion

		#region Accessibility

		protected override sealed INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			return BMNetworkActionAccessibilityHelper.CheckEntityIsNotDeleted(Network.Entities.GetInstance(entity)).UnionIfAllowed(() => IsApplicableCore(entity.AsShape()));
		}

		protected override sealed INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => IsEnabledCore(entity.AsShape());

		protected override sealed INetworkActionAccessibility PerformPreExecutionChecksForEntityCore(INetworkEntity entity)
		{
			var shape = entity.AsShape();

			if (shape.IsApproved && !CanPerformOnApprovedShape)
			{
				var message = Res.GetString("5d91bdc3-c798-4623-b0ae-b4e7045b59d3", "This action cannot be performed on an approved {0}.",
					shape.PK == ((IDiagramEntity)Network.DiagramEntity).EntityPK ? Res.GetString("a15c7378-4a41-485c-a756-ac0f5b20bae3", "diagram") : Res.GetString("bd134d61-1f09-4063-a42f-3ecd441607eb", "shape"));
				return new NetworkActionAccessibility(entity, message);
			}

			return PerformPreExecutionChecksForShape(shape);
		}

		#endregion

		#endregion

		#region Abstract and Virtual Methods

		protected virtual ResourceString GetNameCore(BMNCNShape shape) => GetDefaultNameCore();

		protected virtual ResourceString GetDescriptionCore(BMNCNShape shape) => GetDefaultDescriptionCore();

		protected virtual bool IsActivatedCore(BMNCNShape shape) => GetDefaultIsActivatedCore();

		protected virtual IEnumerable<INetworkAction> GetChildActionsCore(BMNCNShape shape) => GetDefaultChildActionsCore();

		protected abstract INetworkActionAccessibility IsApplicableCore(BMNCNShape shape);

		protected virtual INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return NetworkActionAccessibility.Allowed;
		}

		protected virtual INetworkActionAccessibility PerformPreExecutionChecksForShape(BMNCNShape shape)
		{
			var shapeMessage = GetUserConfirmationMessageForShape(shape);

			if (string.IsNullOrEmpty(shapeMessage) || GetUserConfirmation(shapeMessage))
			{
				return NetworkActionAccessibility.Allowed;
			}

			return NetworkActionAccessibility.GetCancelledByUserWithoutNotification(Network.DiagramShape);
		}

		#endregion

		#region Execution Preconditions

		protected override INetworkActionAccessibility CheckCanStartExecutionForNetworkCore()
		{
			if (RequiresValidateBeforeExecute && !Network.ValidateAndCheckThereAreNoErrors())
			{
				return new NetworkActionAccessibility(new NetworkActionDenialReason(Network.DiagramShape, Res.GetString("bb5bcf97-23aa-409f-99c2-666235d717b4", "There are validation errors. Please fix these first."), needsNotification: true));
			}

			var diagramNeedsSave = RequiresSaveBeforeExecute && (!Network.DiagramShape.IsInDatabase || Network.DiagramShape.HasChanges);
			var requireUserConfirmation = RequiresUserConfirmation || diagramNeedsSave;
			if (requireUserConfirmation && !GetUserConfirmation(GetUserConfirmationMessage(diagramNeedsSave)))
			{
				return NetworkActionAccessibility.GetCancelledByUserWithoutNotification(Network.DiagramShape);
			}

			var savedResult = ContinueWithSave.Yes;

			if (diagramNeedsSave)
			{
				savedResult = Controller.TriggerSaveAction();
			}

			if (savedResult == ContinueWithSave.No)
			{
				return new NetworkActionAccessibility(new NetworkActionDenialReason(Network.DiagramShape, Res.GetString("E0498153-6387-48E5-89CA-1C8C82093C6F", "The diagram was not saved."), needsNotification: false));
			}

			return NetworkActionAccessibility.Allowed;
		}

		protected bool GetUserConfirmation(string message)
		{
			var caption = Res.GetString("151e5fcd-84b3-46b7-9b2c-853ec94c3f16", "Confirmation Required");
			return UserInteractionImplementor.HasUserConfirmed(message, caption);
		}

		string GetUserConfirmationMessage(bool diagramNeedsSave)
		{
			if (diagramNeedsSave && RequiresUserConfirmation)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", SaveConfirmationMessage, UserConfirmationMessage);
			}
			else if (diagramNeedsSave)
			{
				return SaveConfirmationMessage;
			}
			else if (RequiresUserConfirmation)
			{
				return UserConfirmationMessage;
			}
			else
			{
				throw new InvalidOperationException("No message body could be constructed.");
			}
		}

		protected virtual string UserConfirmationMessage { get => throw new InvalidOperationException("Should override this in descendants."); }

		protected virtual string GetUserConfirmationMessageForShape(BMNCNShape shape)
		{
			return null;
		}

		protected virtual string SaveConfirmationMessage
		{
			get { return Res.GetString("1f4bf2f7-33e0-43aa-8170-3507adf14cd2", "The form will attempt to save before performing this operation."); }
		}

		protected virtual bool RequiresValidateBeforeExecute
		{
			get { return false; }
		}

		protected virtual bool RequiresSaveBeforeExecute
		{
			get { return false; }
		}

		protected virtual bool RequiresUserConfirmation
		{
			get { return false; }
		}

		protected virtual bool CanPerformOnApprovedShape
		{
			get { return true; }
		}

		#endregion

		#region INotifyPropertyChanged Members

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#endregion
	}
}
