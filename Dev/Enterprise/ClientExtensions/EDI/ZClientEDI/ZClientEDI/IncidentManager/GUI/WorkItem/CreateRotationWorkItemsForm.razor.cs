using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.ProcessManagement.Business;
using Microsoft.AspNetCore.Components;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class CreateRotationWorkItemsForm : IWinzorDispatcher
	{
		IEnumerable<ICodeDescription> rotationManagerOptions = Enumerable.Empty<ICodeDescription>();
		IEnumerable<ICodeDescription> WorkItemTypes => ProcessManagementRegistry.Instance.WorkItemTypeTree.Value.GetParents(true).ToArray();
		IEnumerable<ICodeDescription> WorkItemAreas => ProcessManagementRegistry.Instance.WorkItemTypeTree.Value.GetChildren(true, string.Empty).ToArray();
		IEnumerable<ICodeDescription> WorkItemActivityTypes => ProcessManagementRegistry.Instance.WorkItemTypeTree.Value.GetChildren(true, string.Empty, string.Empty).ToArray();
		IEnumerable<ICodeDescription> WorkItemActivitySubTypes => ProcessManagementRegistry.Instance.WorkItemTypeTree.Value.GetChildren(true, string.Empty, string.Empty, string.Empty).ToArray();
		IEnumerable<ICodeDescription> WorkItemPriorities => ProcessManagementRegistry.Instance.WorkItemTypeTree.Value.GetChildren(true, string.Empty, string.Empty, string.Empty, string.Empty).ToArray();
		IEnumerable<ICodeDescription> RotationManagerOptions { get => rotationManagerOptions; set { rotationManagerOptions = value; } }

		IWorkItemCreator workItemCreator;
		IWorkItemCreator WorkItemCreator => workItemCreator ??= new RotationWorkItemCreator();

		ICreateWorkItemsService service;
		ICreateWorkItemsService Service => service ??= new CreateWorkItemsService(this);

		async Task<IEnumerable<ICodeDescription>> GetRotationManagerOptionsAsync()
		{
			var rotationManagers = Enumerable.Empty<ICodeDescription>();
			if (WorkItemCreator is RotationWorkItemCreator rotationWorkItemCreator)
			{
				await InvokeAsync(() =>
				{
					rotationManagers = rotationWorkItemCreator.RotationManagers;
				});
			}
			return rotationManagers;
		}

		public CreateRotationWorkItemsForm(IWorkItemCreator workItemCreator, ICreateWorkItemsService service) : this()
		{
			this.workItemCreator = workItemCreator;
			this.service = service;
		}

		public CreateRotationWorkItemsForm()
		{
			MinimumSize = ControlDpiScalingHelper.NewScaledSize(2000, 200);
		}

		public string WorkItemDistinctBySelector(WorkItemDTO wi) => wi.StaffCode;

		public int MinWeeksInCoreSkillsTraining = 1;

		public void OnCoreSkillsTrainingChanged(WorkItemDTO workItem)
		{
			MinWeeksInCoreSkillsTraining = workItem.IsCoreSkillsTraining ? 1 : 0;
			StateHasChanged();
		}

		public string WorkItemDuplicateWarning(WorkItemDTO duplicate) => $"Duplicate workitems with staffCode '{duplicate.StaffCode}'";

		protected override async Task OnInitializedAsync()
		{
			rotationManagerOptions = await GetRotationManagerOptionsAsync();
		}

		string RegexPatternForHtmlSelect(IEnumerable<ICodeDescription> options)
		{
			var optionStrings = options?.Select(p =>
			{
				var regex = Regex.Escape(p.Code);
				// escaping # is invalid in html. Regex.Escape() turns # into \\#
				return Regex.Replace(regex, "\\\\#", "#");
			}) ?? Enumerable.Empty<string>();
			return @$"^({string.Join("|", optionStrings)})$";
		}

		public async Task InvokeAsync(Action action)
		{
			await InvokeWinzorDispatcherAsync(action);
		}
	}
}
