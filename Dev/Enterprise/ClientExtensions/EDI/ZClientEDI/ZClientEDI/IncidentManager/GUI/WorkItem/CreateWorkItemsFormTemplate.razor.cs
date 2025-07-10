using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Module;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.GUI;

public partial class CreateWorkItemsFormTemplate<TWorkItemDistinctByKey>
{
	[Parameter]
	public IWorkItemCreator WorkItemCreator { get; set; }

	[Parameter]
	public WinzorDispatcher WinzorDispatcher { get; set; }

	[Parameter]
	public ICreateWorkItemsService Service { get; set; }

	public bool IsCreateRotationWorkitemEnabled { get; set; } = true;

	[Inject]
	public IJSRuntime JS { get; set; }

	public BindingList<WorkItemDTO> workItems { get; } = new();

	IEnumerable<CreateWorkItemResult> createdItemsResults = Enumerable.Empty<CreateWorkItemResult>();

	IEnumerable<string> warnings = Enumerable.Empty<string>();

	public CreateWorkItemsFormTemplate()
	{
		workItems.ListChanged += OnWorkItemChange;
	}

	IEnumerable<string> CreateItemErrors => createdItemsResults
		.Where((r) => r.StatusCode != CreateWorkItemStatusCode.Success)
		.Select((r) => r.StatusCode switch
		{
			CreateWorkItemStatusCode.InvalidStartDate => Res.GetString("AD178CF5-E5D6-49E5-B0D9-54EF33EEFCBE", "Start Date is invalid. No work item created."),
			CreateWorkItemStatusCode.StaffNotFound => Res.GetString("590961A6-5AAB-4B67-B2D9-7C0C9D6EEC50", $"Staff with code \"{r.WorkItemDTO.StaffCode}\" could not be found. No work item created."),
			CreateWorkItemStatusCode.RotationManagerNotFound =>
				Res.GetString("322AC9FC-8990-4D4E-8DA1-1D304B3AB7A1", $"Rotation manager with code \"{r.WorkItemDTO.RotationManager}\" could not be found. No work item created."),
			CreateWorkItemStatusCode.InvalidRotationManager =>
				Res.GetString("9b860b5e-9f7e-4f99-9c98-5fee05c81c5e", $"Staff with code \"{r.WorkItemDTO.RotationManager}\" is not a rotation manager. No work item created."),
			CreateWorkItemStatusCode.InvalidWeeksPerRotation =>
				Res.GetString("CA3C4F13-403D-4F21-8EF9-11425C6F345F", $"Weeks per rotation value is invalid. Each rotation must be a minimum of 1 week and a maximum of 17 weeks. No work item created."),
			CreateWorkItemStatusCode.InvalidWeeksInCoreSkillsTraining =>
				Res.GetString("d19db603-f638-4256-80a4-fd669a633ea0", $"Weeks In Core Skills Training value is invalid. Core Skills Training weeks must be between 1 and 6. No work item created."),
			CreateWorkItemStatusCode.InvalidLocation =>
				Res.GetString("AE045336-668B-40F5-858F-639AB3188FE7", $"Location code \"{r.WorkItemDTO.LocationCode}\" is invalid. Location code must be 0, 2 or 5 characters. No work item created."),
			CreateWorkItemStatusCode.WorkItemPreviouslyCreated =>
				Res.GetString("0A36097F-D148-4B93-A295-F2EF8EB864C5", $"Work item for row previously created. No new work item created."),
			_ when r.CreatedWorkItem is null => Res.GetString("DF5F67D3-D18E-492E-8021-55A48FBE77BE", "Unknown error. No work item created."),
			_ => Res.GetString("C81145D5-133C-4983-9547-89BCD9F8D9C3", "Unknown error. Created work item: {0}", r.CreatedWorkItem.HumanReadableName),
		});

	IEnumerable<string> CreatedItemNames => createdItemsResults
		.Where((r) => r.StatusCode == CreateWorkItemStatusCode.Success)
		.Select((r) => r.CreatedWorkItem.HumanReadableName);

	string Emails => string.Join(",", createdItemsResults.Where(r => !r.StaffEmailAddress.IsNullOrEmpty()).Select(r => r.StaffEmailAddress));

	public string ImageUrl => Enterprise.Client.EDI.Properties.Resources.CopyIcon.ToBase64DataUrl();

	async Task OnClickCreateAsync(EventArgs e)
	{
		await WinzorDispatcher.InvokeAsync(() =>
		{
			createdItemsResults = WorkItemCreator.CreateWorkItems(Enumerable.DistinctBy(workItems, WorkItemDistinctBySelector));
			if (CreateItemErrors.Any())
			{
				StringBuilder errorMessageBuilder = new StringBuilder();
				foreach (var errorMessage in CreateItemErrors)
				{
					errorMessageBuilder.AppendLine(errorMessage);
				}
				string errorMessageString = errorMessageBuilder.ToString();
				Service.ShowMessage(errorMessageString);
			}
		});
		checkforWorkItemCreated();
		StateHasChanged();
	}

	void checkforWorkItemCreated()
	{
		bool allWorkItemsCreated = workItems.All(wi => wi.WorkItemCreated);
		if (allWorkItemsCreated)
		{
			IsCreateRotationWorkitemEnabled = false;
		}
		else
		{
			IsCreateRotationWorkitemEnabled = true;
		}
		StateHasChanged();
	}

	public void OnWorkItemChange(object sender, ListChangedEventArgs e)
	{
		warnings = workItems
			.Where((wi) => !wi.StaffCode.IsNullOrEmpty())
			.GroupBy(WorkItemDistinctBySelector)
			.Where((g) => g.Count() >= 2)
			.Select((g) => WorkItemDuplicateWarning(g.First()));
		checkforWorkItemCreated();
		StateHasChanged();
	}

	void AddItem()
	{
		workItems.Add(WorkItemCreator.CreateDefaultWorkItemDTO());
		checkforWorkItemCreated();
		StateHasChanged();
	}

	void RemoveItem()
	{
		if (workItems.Count > 1)
		{
			workItems.RemoveAt(workItems.Count - 1);
			checkforWorkItemCreated();
			StateHasChanged();
		}
	}

	protected override void OnInitialized()
	{
		AddItem();
		base.OnInitialized();
	}

	public void CopyLastRow()
	{
		if (workItems.Any())
		{
			var lastItem = workItems.Last();
			var newItem = new WorkItemDTO(
				lastItem.Summary,
				lastItem.Type,
				lastItem.Area,
				lastItem.ActivityType,
				lastItem.ActivitySubType,
				lastItem.Priority,
				string.Empty, // Set StaffCode to blank
				lastItem.LocationCode)
			{
				StartDate = lastItem.StartDate,
				Rotation1Team = lastItem.Rotation1Team,
				RotationManager = lastItem.RotationManager,
				WeeksPerRotation = lastItem.WeeksPerRotation,
				WeeksInCoreSkillsTraining = lastItem.WeeksInCoreSkillsTraining,
				IsCoreSkillsTraining = lastItem.IsCoreSkillsTraining,
				IsDeveloper = lastItem.IsDeveloper,
				WorkItemCreated = false
			};
			workItems.Add(newItem);
			StateHasChanged();
		}
	}

	async void CopyEmail() => await WorkItemCreator.CopyEmails(JS.InvokeVoidAsync("navigator.clipboard.writeText", Emails));
}
