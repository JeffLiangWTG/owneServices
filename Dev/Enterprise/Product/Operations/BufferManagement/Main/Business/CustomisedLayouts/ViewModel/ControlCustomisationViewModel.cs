using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class ControlCustomisationViewModel : ITaskCardComponentParent
	{
		public ControlCustomisationViewModel(BMControlCustomisation customisation, IBizoCardContent cardContent)
		{
			Argument.NotNull(customisation, "customisation");
			Argument.NotNull(cardContent, "cardContent");

			Customisation = customisation;
			CardContent = cardContent;

			IsPreview = true;
			task = new Lazy<ProcessTask>(() => cardContent.Task);
		}

		public ControlCustomisationViewModel(BMControlCustomisation customisation, BMBoardSectionViewModel viewModel, ICardContent cardContent, CellContent cell, ProcessTask taskToModify = null)
		{
			Argument.NotNull(customisation, "customisation");
			Argument.NotNull(viewModel, "viewModel");
			Argument.NotNull(cardContent, "cardContent");
			Argument.NotNull(cell, "cell");

			Customisation = customisation;
			ViewModel = viewModel;
			CardContent = cardContent;
			Cell = cell;
			var factory = customisation.Factory;
			task = new Lazy<ProcessTask>(() => taskToModify ?? (cardContent as IBizoCardContent)?.Task ?? factory.Load<ProcessTask>(cardContent.TaskIdentifier));
			AssignedStaffCode = ZString.Empty;
		}

		public ICardContent CardContent { get; set; }
		public CellContent Cell { get; set; }
		public ProcessTask Task => task.Value;
		readonly Lazy<ProcessTask> task;

		public ZString AssignedStaffCode { get; set; }

		public BMControlCustomisation Customisation { get; }

		public BMBoardSectionViewModel ViewModel { get; }

		public bool IsPreview { get; }

		public InstalledFontCollection InstalledFonts => new InstalledFontCollection();

		public void SelectControl(IComponent sender, ControlCustomisationBase selectedControl)
		{
			SelectedControlChanged?.Invoke(sender, new SelectedControlChangedEventArgs(selectedControl));
		}

		public event EventHandler<SelectedControlChangedEventArgs> SelectedControlChanged;

		#region ITaskCardComponentParent Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This is a viewmodel")]
		public void Save(object sender = null)
		{
			DoOutsidePreviewMode(() =>
			{
				if (Task == null)
				{
					return;
				}
				var allBizos = BusinessObjectsRequiringValidation.Cast<IBusiness>().SelectDistinctRecursive(a => a.Children, new BusinessIdentifierComparer());
				if (allBizos.OfType<BusinessObject>().All(bo => !bo.HasErrors))
				{
					try
					{
						VisualBoardDataRefreshBusSubscriber.NotifyIsSavingDetailedTicket();

						if (!ViewModel.DataRefreshBusSubscriberRefreshesTickets)
						{
							ViewModel.BoardViewModel.SlideShowViewModel.RefreshServices(BoardServiceStalenessPolicy.StaleBeforeSavingTicket);
						}

						var tasksInFactory = ((IBusinessObjectFactoryInternals)Task.Factory).AllBusinessObjects.OfType<ProcessTask>().Where(t => t.HasChanges).ToArray();

						Task.Factory.Save();

						Saved?.Invoke(sender, new TasksSavedArgs(tasksInFactory));

						Cell.Channel?.ClearCacheAndReload();
						Close();
					}
					catch (ZSaveConcurrencyException)
					{
						Globals.Message.ShowWarning(Res.GetString("1d0a5efe-fa03-4dc2-98a2-b5f015fe4b16", "Another user has modified this task. Please make your changes and then try saving again.")); // This is a viewmodel
						Task.Reload();
					}
				}
				else
				{
					var errorMessages = GetFormattedErrorMessages(allBizos.OfType<BusinessObject>());
					Globals.Message.ShowError(errorMessages); // This is a viewmodel
				}
			});
		}

		IEnumerable<BusinessObject> BusinessObjectsRequiringValidation
		{
			get
			{
				if (Task != null)
				{
					var parent = Task.Parent as BusinessObject;
					if (parent != null)
					{
						yield return parent;
					}

					yield return Task;

					if (Task.ProcessHeader != null)
					{
						yield return (ProcessHeader)Task.ProcessHeader;

						if (Task.ProcessHeader.JobHeader != null)
						{
							yield return (ProcessJobHeader)Task.ProcessHeader.JobHeader;
						}
					}
				}
			}
		}

		#region Comparer

		class BusinessIdentifierComparer : IEqualityComparer<IBusiness>
		{
			public bool Equals(IBusiness x, IBusiness y)
			{
				return x == y && x.Identifier == y.Identifier;
			}

			public int GetHashCode(IBusiness obj)
			{
				return obj.GetHashCode() ^ obj.Identifier.GetHashCode();
			}
		}

		#endregion

		static string GetFormattedErrorMessages(IEnumerable<BusinessObject> bizos)
		{
			var message = new StringBuilder();
			foreach (var bizo in bizos)
			{
				var errors = bizo.Notifications.Where(n => n.Type == CargoWise.ComponentModel.NotificationType.Error).ToArray();
				if (errors.Length > 0)
				{
					var name = bizo.HumanReadableName;

					try
					{
						var description = DescriptionPropertyAttribute.DescriptionFromBusinessObject(bizo);

						message.AppendLine(Res.GetString("b44b8b3c-3671-4092-8d61-15e8ad0b94f7", "Errors on {0} [{1}]:", name, description));
					}
					catch (NoCodePropertyException)
					{
						// If there isn't a code, the table name is the best we can do.
						message.AppendLine(Res.GetString("e5cb81e8-7694-4823-b83e-50f42e940056", "Errors on {0}:", name));
					}

					foreach (var error in errors)
					{
						message.AppendLine(error.Message);
					}

					message.AppendLine();
				}
			}

			return message.ToString();
		}

		public void Close()
		{
			DoOutsidePreviewMode(() =>
			{
				Closed?.Invoke(this, EventArgs.Empty);
			});
		}

		public void ShowParent()
		{
			DoOutsidePreviewMode(() =>
			{
				if (ParentShown != null && Task != null)
				{
					var parent = CardContent.CardType == CardType.Task ? (BusinessObject)Task : Task.GetProcessHeader();
					ParentShown(this, new ParentShownEventArgs(parent));
				}
			});
		}

		public void VoteUp(bool jobCardsShown)
		{
			DoOutsidePreviewMode(() =>
			{
				if (Task != null)
				{
					Task.GetProcessHeaderForCardType(jobCardsShown).NudgeUp();
				}
			});
		}

		public void VoteDown(bool jobCardsShown)
		{
			DoOutsidePreviewMode(() =>
			{
				if (Task != null)
				{
					Task.GetProcessHeaderForCardType(jobCardsShown).NudgeDown();
				}
			});
		}

		public void UpdateStatus(string status)
		{
			var expectedAssignee = Cell.Channel != null && Cell.Channel.EntityType == ChannelTypeList.Codes.Resource && Cell.Channel.EntityPK.IsValid ? Cell.Channel.ChannelEntityCode : ZString.Empty;

			var task = Task;
			if (task == null)
			{
				return;
			}

			if (expectedAssignee != ZString.Empty)
			{
				if (!AssignedStaffCode.IsEmpty && AssignedStaffCode != expectedAssignee)
				{
					CannotUpdateStatus?.Invoke(this, EventArgs.Empty);

					ViewModel.RefreshAll(new WorkflowUpdatedOperation(new[] { CardContent.TaskIdentifier }, new[] { CardContent.WorkflowIdentifier }, Customisation.Factory));
					return;
				}

				task.P9_GS_NKAssignedStaffMember = expectedAssignee;
			}

			if (status != task.P9_Status || (!task.P9_ActualDuration.IsEmpty && status == ProcessTaskStatusCodeList.Codes.Closed))
			{
				using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
				{
					task.P9_Status = status;
				}

				StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(status));
			}
		}

		void DoOutsidePreviewMode(Action action)
		{
			if (!IsPreview)
			{
				action();
			}
		}

		public event EventHandler<StatusUpdatedEventArgs> StatusUpdated;
		public event EventHandler<TasksSavedArgs> Saved;
		public event EventHandler Closed;
		public event EventHandler<ParentShownEventArgs> ParentShown;
		public event EventHandler CannotUpdateStatus;

		#endregion
	}

	public class SelectedControlChangedEventArgs : EventArgs
	{
		public SelectedControlChangedEventArgs(ControlCustomisationBase selectedControl)
		{
			SelectedControl = selectedControl;
		}

		public ControlCustomisationBase SelectedControl { get; private set; }
	}

	public class TasksSavedArgs : EventArgs
	{
		public TasksSavedArgs(ProcessTask[] tasks)
		{
			tasksSaved = tasks;
		}

		readonly ProcessTask[] tasksSaved;

		public ProcessTask[] GetTasks()
		{
			return tasksSaved;
		}
	}

	public class StatusUpdatedEventArgs : EventArgs
	{
		public string NewStatus { get; }

		public StatusUpdatedEventArgs(string newStatus)
		{
			NewStatus = newStatus;
		}
	}
}
