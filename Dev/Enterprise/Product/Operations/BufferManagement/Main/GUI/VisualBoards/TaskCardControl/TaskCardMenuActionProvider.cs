using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "It is better to construct the menu in one place.")]
	public class TaskCardMenuActionProvider
	{
		#region API

		public static void AddMenuItems(TaskCardControl control, ICardContent cardContent, CardLoadResult cardLoad, BMBoardSectionViewModel viewModel, KContextMenuStrip menuStrip)
		{
			var menuActionProvider = new TaskCardMenuActionProvider(control, cardContent, cardLoad, viewModel, menuStrip);
			menuActionProvider.AddItems();
		}

		#endregion

		#region Constructors

		TaskCardMenuActionProvider(TaskCardControl control, ICardContent card, CardLoadResult cardLoad, BMBoardSectionViewModel sectionViewModel, KContextMenuStrip menuStrip)
		{
			factory = cardLoad.Factory;
			cardContent = card;
			task = cardLoad.Task;
			workflow = cardLoad.Workflow;
			viewModel = sectionViewModel;
			taskCardControl = control;
			contextMenuStrip = menuStrip;
		}

		readonly BusinessObjectFactory factory;
		readonly TaskCardControl taskCardControl;
		readonly ICardContent cardContent;
		readonly ProcessTask task;
		readonly ProcessHeader workflow;
		readonly BMBoardSectionViewModel viewModel;
		readonly KContextMenuStrip contextMenuStrip;

		#endregion

		#region Create Toolstrips

		void AddItems()
		{
			if (workflow == null || task == null)
			{
				var message = ResString.GetMultilingualString("a5d5f18f-f4db-46fd-80d9-86f38954e087", "Task or Workflow no longer exists.");
				var messageContext = new DialogDefaultContext(new ZGuid("21e47faf-d30c-4570-bb58-da8bf5ddcfdc"), message, ZMessageBoxButtons.OK, ZMessageBoxIcon.Exclamation);
				Globals.Message.ShowOrDefault(messageContext, message);

				viewModel.RefreshGrid(cardContent);
			}
			else
			{
				contextMenuStrip.Items.AddRange(GetItems().ToArray());
			}
		}

		IEnumerable<ToolStripItem> GetItems()
		{
			var highlight = MakeMenuItem(Res.GetData("596a4be0-5b6c-49af-8781-c567d1490542", "Highlight Tasks in Workflow"), "HighlightRelatedTasksToolStripMenuItem", HighlightRelatedTasksToolStripMenuItem_Click);
			highlight.Checked = taskCardControl.AreTicketsInWorkflowHighlighted;
			yield return highlight;

			yield return new ToolStripSeparator();
			yield return MakeMenuItem(Res.GetData("dedb2bb2-608a-489a-9c31-9dfaae6da291", "Copy Job Hyperlink to Clipboard"), "CopyJobHyperlinkStripMenuItem", CopyJobHyperlinkStripMenuItem_Click);
			yield return MakeMenuItem(Res.GetData("ebae7e5f-dd68-4664-817c-8c1051a445c3", "Copy Job ID to Clipboard"), "CopyJobIDStripMenuItem", CopyJobIDStripMenuItem_Click);
			yield return MakeMenuItem(Res.GetData("90f88732-bd8c-40a9-aa34-70819ee5f14e", "Copy Job Name to Clipboard"), "CopyJobNameStripMenuItem", CopyJobNameStripMenuItem_Click);
			yield return new ToolStripSeparator();
			yield return MakeMenuItem(Res.GetData("5001acc6-5fa1-4fde-8308-f3da340f0f02", "Show Detailed Card"), "DetailedCardToolStripMenuItem", DetailedCardToolStripMenuItem_Click);
			yield return MakeMenuItem(Res.GetData("d280e332-48bd-4315-bced-fd51ffcec0ca", "Open Job"), "OpenJobToolStripMenuItem", OpenJobToolStripMenuItem_Click);

			if (!viewModel.ShowJobCards)
			{
				yield return MakeMenuItem(Res.GetData("4e1fa5b2-658a-4f66-a897-63efe249094b", "Task Details"), "TaskDetailsToolStripMenuItem", TaskDetailsToolStripMenuItem_Click);
			}

			if (viewModel.ShowWorkflowOrJobWorkflowCards)
			{
				yield return ConvertAndDisposeOriginal(new OperationalActionsMenuItem(workflow));
			}
			else
			{
				yield return ConvertAndDisposeOriginal(new OperationalActionsMenuItem(workflow, task));
			}

			yield return new ToolStripSeparator();
			yield return MakeMenuItem(Res.GetData("4e1fa5b2-658a-4f66-a897-63efe249194b", "Defer"), "DeferToolStripMenuItem", DeferToolStripMenuItem_Click);
			yield return CreateMoveToComponentItem();

			if (!viewModel.ShowJobCards)
			{
				yield return MakeMenuItem(Res.GetData("c9b288d4-d080-47d9-ae81-9bd4e3482427", "Transfer Diagnosis"), "TransferFailureReasonsToolStripMenuItem", TransferFailureReasonsToolStripMenuItem_Click);
			}

			yield return new ToolStripSeparator();

			foreach (var item in GetTaggingMenuItems())
			{
				yield return item;
			}

			yield return new ToolStripSeparator();

			yield return ConvertAndDisposeOriginal(new PreRequisitesMenuItem(workflow, saveAfterActions: true));
			yield return ConvertAndDisposeOriginal(new DependentWorkflowsMenuItem(workflow, saveAfterActions: true));
			yield return ConvertAndDisposeOriginal(new ParentWorkflowsMenuItem(workflow, saveAfterActions: true));
			yield return ConvertAndDisposeOriginal(new ChildWorkflowsMenuItem(workflow, saveAfterActions: true));

			if (!viewModel.ShowWorkflowOrJobWorkflowCards)
			{
				var shouldAddAssistWithThisTaskMenuItem = AssistWithThisTaskMenuItem.ShouldAddMenuItem(task);
				var shouldAddAddAssistanceTaskForMenuItemForTaskCard = AddAssistanceTaskForMenuItemForTaskCard.ShouldAddMenuItem(task);

				if (shouldAddAssistWithThisTaskMenuItem || shouldAddAddAssistanceTaskForMenuItemForTaskCard)
				{
					yield return new ToolStripSeparator();
				}

				if (shouldAddAssistWithThisTaskMenuItem)
				{
					yield return ConvertAndDisposeOriginal(new AssistWithThisTaskMenuItem(task, saveAfterActions: true));
				}

				if (shouldAddAddAssistanceTaskForMenuItemForTaskCard)
				{
					yield return ConvertAndDisposeOriginal(new AddAssistanceTaskForMenuItemForTaskCard(factory, task, workflow, GetPrimaryUserPKs(), GetPrimaryCapabilityPKs()));
				}
			}
		}

		ToolStripItem CreateMoveToComponentItem()
		{
			var item = MakeMenuItem(Res.GetData("04df7693-1f12-4c00-96c4-a65105299c2e", "Move To Component"), "MoveToComponentToolStripMenuItem", null, new ZToolStripMenuItem("-"));
			item.DropDownOpening += MoveToComponentToolStripMenuItem_DropDownOpening(item);

#if DEBUG
			if (taskCardControl.ShouldPreloadSubMenus_ForTest)
			{
				LoadMoveToComponentItem(item);
			}
#endif

			return item;
		}

		ToolStripItem ConvertAndDisposeOriginal(ZMenuItem menuItem)
		{
#if DEBUG
			if (taskCardControl.ShouldPreloadSubMenus_ForTest)
			{
				//ZLazyPopulatingMenuItem will not show in test if we don't do this
				menuItem.OnPopup(null);
			}
#endif

			var result = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem);

			menuItem.Dispose();

			return result;
		}

		IEnumerable<ToolStripItem> GetTaggingMenuItems()
		{
			var addTagMenuItem = new ZToolStripMenuItem(ResString.GetMultilingualString("223604b9-89fd-432d-8898-81a5f84d733e", "Add Tag"));
			var removeTagMenuItem = new ZToolStripMenuItem(ResString.GetMultilingualString("38c904b9-8b83-4776-82b1-0f9cd3433534", "Remove Tag"));

			var jobHeader = workflow.JobHeader;

			if (!viewModel.ShowJobCards)
			{
				CreateTagMenuItems(factory, addTagMenuItem, removeTagMenuItem, workflow, ResString.GetMultilingualString("9847c490-729a-4c3e-a3ed-96f3b0760f53", "Workflow [{0}]", ((string)workflow.FH_CompletionStatement).ShortenLineLengths(30)));
			}
			CreateTagMenuItems(factory, addTagMenuItem, removeTagMenuItem, jobHeader, ResString.GetMultilingualString("964c6671-e46f-4613-9be2-9a7a84836f15", "Job [{0}]", ((string)jobHeader.FH_CompletionStatement).ShortenLineLengths(30)));

			if (!viewModel.ShowWorkflowOrJobWorkflowCards)
			{
				CreateTagMenuItems(factory, addTagMenuItem, removeTagMenuItem, task, ResString.GetMultilingualString("25becbbb-eb1d-4e46-a533-c02aaa722761", "Task [{0}]", ((string)task.P9_Description).ShortenLineLengths(30)));
			}

			yield return addTagMenuItem;
			yield return removeTagMenuItem;
		}

		#region AddAssistanceTaskForMenuItem

		IReadOnlyCollection<ZGuid> GetPrimaryUserPKs()
		{
			return viewModel.PrimaryChannels.Where(c => c.EntityType == ChannelTypeList.Codes.Resource).Select(c => c.EntityPK).ToArray();
		}

		IReadOnlyCollection<ZGuid> GetPrimaryCapabilityPKs()
		{
			return viewModel.PrimaryChannels.Where(c => c.EntityType == ChannelTypeList.Codes.Capability).Select(c => c.EntityPK).ToArray();
		}

		#endregion

		#endregion

		#region Action Implementations

		#region Filters

		void HighlightRelatedTasksToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToggleSelectCardsInSameWorkflow();
		}

		void ToggleSelectCardsInSameWorkflow()
		{
			taskCardControl.CloseDetailedCardIfOpen();

			var filter = new HighlightTaskCardsInSameWorkflowFilter(workflow)
			{
				RestoreVisualStateAfterFilterRemovedAction = () =>
				{
					var form = taskCardControl.FindForm() as VisualBoardForm;
					if (form != null)
					{
						form.RefreshBoard();
					}
				}
			};

			viewModel.BoardViewModel.FilterManager.ToggleFilter(filter);
		}

		void OpenJobToolStripMenuItem_Click(object sender, EventArgs e)
		{
			taskCardControl.ShowParentWorkflow();
		}

		void DetailedCardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			taskCardControl.ToggleDetailedCard();
		}

		void MoveToComponentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MoveToComponent(((ZToolStripMenuItem)sender).Tag as BMComponent, workflow, cardContent, factory, viewModel);
		}

		public static void MoveToComponent(BMComponent component, ProcessHeader workflow, ICardContent content, BusinessObjectFactory factory, BMBoardSectionViewModel sectionViewModel)
		{
			if (component != null)
			{
				bool didMoveWorkflows;
				var jobHeader = workflow as ProcessJobHeader;

				if (jobHeader != null)
				{
					var moveResult = jobHeader.MoveJobToComponent(sectionViewModel.AllShownComponentPKs.ToArray(), component);
					didMoveWorkflows = moveResult.Item1 > 0;
				}
				else
				{
					didMoveWorkflows = workflow.MoveToComponent(component);
				}

				if (didMoveWorkflows)
				{
					factory.SaveHandlingZSaveExceptions();

					if (!sectionViewModel.DataRefreshBusSubscriberRefreshesTickets)
					{
						sectionViewModel.RefreshAll(new WorkflowUpdatedOperation(Array.Empty<ZGuid>(), new[] { content.WorkflowIdentifier }, factory));
					}
				}
			}
		}

		EventHandler MoveToComponentToolStripMenuItem_DropDownOpening(ZToolStripMenuItem item)
		{
			return (s, e) =>
			{
				LoadMoveToComponentItem(item);
			};
		}

		void LoadMoveToComponentItem(ZToolStripMenuItem item)
		{
			if (item.DropDownItems.Count == 1 && item.DropDownItems[0].Tag == null)
			{
				item.DropDownItems[0].Dispose();

				var components = workflow.GetBMSystem()?.Components?.AsEnumerable();

				if (components == null)
				{
					return;
				}

				if (!(workflow is ProcessJobHeader))
				{
					components = components.Where(x => x.PK != workflow.FH_FC_CurrentComponent);
				}

				foreach (var component in components.OrderBy(c => c.FC_DisplaySequence))
				{
					GetAndAddZToolStripMenuItem(item, component);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		ZToolStripMenuItem GetAndAddZToolStripMenuItem(ZToolStripMenuItem item, BMComponent component)
		{
			var menuItem = new ZToolStripMenuItem(component.FC_Name);
			try
			{
				if (component.FC_IsActive)
				{
					menuItem.Tag = component;
					menuItem.Click += MoveToComponentToolStripMenuItem_Click;
				}
				else
				{
					menuItem.Enabled = false;
				}

				item.DropDownItems.Add(menuItem);

				return menuItem;
			}
			catch
			{
				try
				{
					menuItem.Dispose();
				}
				catch { }
				throw;
			}
		}

		void TaskDetailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var edittedTask = cardContent.CardType == CardType.Task ? task : null;
			if (edittedTask != null)
			{
				taskCardControl.ShowTaskDetailsForm(edittedTask);
			}
		}

		void DeferToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var result = DeferWorkflowForm.ShowDeferForm(workflow, viewModel.AllShownComponentPKs);

			if (result.DialogResult == DialogResult.OK && !BMSRegistry.Instance.UpdateTicketsWithDataRefresh.Value) // Data refresh will update tickets when the business objects are updated. Otherwise do it here manually.
			{
				viewModel.RefreshAll(new WorkflowUpdatedOperation(Array.Empty<ZGuid>(), result.WorkflowsDeferred.Select(w => w.PK).ToArray(), workflow.Factory));
			}
		}

		void TransferFailureReasonsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var transferFailureFactory = new BusinessObjectFactory { NameForDebugging = "TaskCardControl.Transfer Failure Reasons" };
			var failedTransferWorkflow = cardContent.GetWorkflow(transferFailureFactory);
			if (failedTransferWorkflow != null)
			{
				WorkflowTransferDiagnosisForm.Show(failedTransferWorkflow);
			}
		}

		void CopyJobHyperlinkStripMenuItem_Click(object sender, EventArgs e)
		{
			var parent = task.Parent;
			var controllerId = WorkflowProviderHelper.GetControllerForWorkflowType(parent.WorkflowType)?.ID;
			var humanReadableName = ((BusinessObject)parent).HumanReadableName;
			var shortcut = CreateUrlForJob(controllerId, parent.PK);
			ZMenuStrategyHelper.CopyHyperlinkToClipboard(humanReadableName, shortcut);
		}

		string CreateUrlForJob(ControllerID controllerId, ZGuid jobGuid)
		{
			var useWebHyperlinks = ShortcutCreator.DoesUseWebHyperlinks();
			return useWebHyperlinks ? ShowEditFormUrlHandler.Instance.CreateWebTrampolineUri(controllerId, jobGuid) : ShowEditFormUrlHandler.Instance.Create(controllerId, jobGuid);
		}

		void CopyJobIDStripMenuItem_Click(object sender, EventArgs e)
		{
			ZMenuStrategyHelper.CopyIdToClipboard(task.Parent as BusinessObject);
		}

		void CopyJobNameStripMenuItem_Click(object sender, EventArgs e)
		{
			var parent = task.Parent;
			var humanReadableName = ((BusinessObject)parent).HumanReadableName;
			ShortcutCreator.CopyTextToClipboard(humanReadableName);
		}
	#endregion

	#endregion

	#region Utilities

	[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		ZToolStripMenuItem MakeMenuItem(ResourceStringData caption, string name, EventHandler handler, params ToolStripItem[] children)
		{
			var item = new ZToolStripMenuItem(caption, handler);

			try
			{
				item.Name = name;
				if (children.Length > 0)
				{
					item.DropDownItems.AddRange(children);
				}

				return item;
			}
			catch
			{
				try
				{
					item.Dispose();
				}
				catch { }
				throw;
			}
		}

		void CreateTagMenuItems(BusinessObjectFactory tagDefinitionFactory, ZToolStripMenuItem addTagMenuItem, ZToolStripMenuItem removeTagMenuItem, ITagable tagable, ResourceString name)
		{
			var trees = GetTagMenuItemTrees(tagDefinitionFactory, tagable, name, cardContent, viewModel);
			addTagMenuItem.DropDownItems.Add(trees[0]);
			removeTagMenuItem.DropDownItems.Add(trees[1]);
		}

		public static TagToolStripMenuTree[] GetTagMenuItemTrees(BusinessObjectFactory tagDefinitionFactory, ITagable tagable, ResourceString name, ICardContent cardContent, BMBoardSectionViewModel viewModel)
		{
			var definitions = cardContent.Definitions;
			var tagableType = tagable.GetType();
			var tagableToTag = new GetTagables((factoryForTagging, isForRemoving) => new[] { factoryForTagging == tagDefinitionFactory ? tagable : (ITagable)factoryForTagging.Load(tagableType, tagable.PK) });
			var addTagViewModel = new AddTagMenuItemViewModel(tagableToTag, tagableType, Lazy.Create(() => definitions), tagDefinitionFactory) { Name = name };
			var removeTagViewModel = new RemoveTagMenuItemViewModel(tagableToTag, Lazy.Create(() => definitions), tagDefinitionFactory) { Name = name };
			var addTagMenuItemTree = new TagToolStripMenuTree(addTagViewModel, viewModel, cardContent);
			var removeTagMenuItemTree = new TagToolStripMenuTree(removeTagViewModel, viewModel, cardContent);

			return new[] { addTagMenuItemTree, removeTagMenuItemTree };
		}

		#endregion
	}
}
