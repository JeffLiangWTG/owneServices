using System;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class TagToolStripMenuTree : ZToolStripMenuItem
	{
		public TagToolStripMenuTree(ITagMenuTreeViewModel model, BMBoardSectionViewModel viewModel, ICardContent content)
			: base(model.Name)
		{
			this.model = model;
			this.viewModel = viewModel;
			this.content = content;

			SetupMenu();
		}

		readonly ITagMenuTreeViewModel model;
		readonly BMBoardSectionViewModel viewModel;
		readonly ICardContent content;

		void SetupMenu()
		{
			DropDownItems.Add(new ZToolStripMenuItem("-"));

			DropDownOpening += (e, s) =>
			{
				DropDownItems.Clear();
				model.ReloadAllMagnitudes();

				foreach (var menuItem in model.GetValidDefinitionMenuItems())
				{
					var dropDownItem = AddTagDefinitionMenuItem(menuItem);
					if (dropDownItem.DropDownItems.Count > 0)
					{
						DropDownItems.Add(dropDownItem);
					}
				}

				if (model.ActionType == TagActionType.RemoveTag && DropDownItems.Count == 0)
				{
					DropDownItems.Add(new ZToolStripMenuItem(Res.GetString("1CB56565-E662-4515-8141-4A5C140370E1", "There are no tags applied")) { Enabled = false });
				}
			};
		}

		ZToolStripMenuItem AddTagDefinitionMenuItem(MenuItemDescriptor<TagDefinition> definitionMenuItem)
		{
			var definitionMenu = new ZToolStripMenuItem(definitionMenuItem.Text);

			foreach (var magnitudeMenuItem in model.GetValidMagnitudeMenuItems(definitionMenuItem.Payload))
			{
				if (magnitudeMenuItem.Text == ZMenuItem.Separator)
				{
					definitionMenu.DropDownItems.Add(new ToolStripSeparator());
				}
				else
				{
					definitionMenu.DropDownItems.Add(new ZToolStripMenuItem(magnitudeMenuItem.Text, OnClick(magnitudeMenuItem)));
				}
			}

			return definitionMenu;
		}

		EventHandler OnClick(MenuItemDescriptor<TagMagnitude> magnitudeMenuItem)
		{
			return (s, e) =>
			{
				DoClickEvent(s, magnitudeMenuItem);
			};
		}

		protected void DoClickEvent(object sender, MenuItemDescriptor<TagMagnitude> magnitudeMenuItem)
		{
			MainThreadRunner.RunOnMainThread(() =>
			{
				magnitudeMenuItem.ExecuteAndMaybeShowFormForPayload(sender, new MenuItemClickHandlerEventArgs(showPayloadFormModally: true));

				if (Owner != null && Owner.InvokeRequired)
				{
					Owner.BeginInvokeSafe(InvalidateCacheAndRefresh);
				}
				else
				{
					InvalidateCacheAndRefresh();
				}
			});
		}

		void InvalidateCacheAndRefresh()
		{
			ClearAppliedTagsCache();
			viewModel.RefreshGrid(content);
		}

		void ClearAppliedTagsCache()
		{
			if (content.CardType == CardType.Workflow)
			{
				RemoveCachedAppliedTags(content.WorkflowIdentifier);
			}
			else
			{
				var factory = new BusinessObjectFactory { NameForDebugging = nameof(TagToolStripMenuTree) + ".ClearAppliedTagsCache" };
				var workflow = content.GetWorkflow(factory);
				if (workflow != null)
				{
					foreach (var task in workflow.GetTasksWithoutAccessingWorkflowParent())
					{
						RemoveCachedAppliedTags(task.PK);
					}
				}
			}
		}

		void RemoveCachedAppliedTags(ZGuid keyPK)
		{
			viewModel.Cache.Remove(keyPK, TaskJobWorkflowCacheHelper.CacheConstants.ApplicableTags);
		}

#if DEBUG
		public void OnDropDownOpening_ForTest()
		{
			base.OnDropDownShow(EventArgs.Empty);
		}
#endif
	}
}
