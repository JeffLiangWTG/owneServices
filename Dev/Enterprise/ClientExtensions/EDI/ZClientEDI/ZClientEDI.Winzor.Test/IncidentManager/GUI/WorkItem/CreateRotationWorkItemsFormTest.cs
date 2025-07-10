using System.Text.RegularExpressions;
using System.Windows.Forms;
using AngleSharp.Dom;
using Bunit;
using Bunit.Extensions;
using CargoWise.Blazor.Controls;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace ZClientEDI.Winzor.Test;
class CreateRotationWorkItemsFormTest : Bunit.TestContext
{
	[Test]
	public async Task TableHeadingsRenderedInOrderAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			return new CreateRotationWorkItemsForm();
		});
		cut.WaitForState(() => !cut.Markup.IsNullOrEmpty());
		var table = cut.Find("table");
		Assert.That(table, Is.Not.Null);

		var headings = table.QuerySelectorAll("label");
		Assert.That(headings.Length, Is.EqualTo(15));
		Assert.That(headings.ElementAt(0).TextContent, Is.EqualTo("Start Date"));
		Assert.That(headings.ElementAt(1).TextContent, Is.EqualTo("Staff Code"));
		Assert.That(headings.ElementAt(2).TextContent, Is.EqualTo("Product"));
		Assert.That(headings.ElementAt(3).TextContent, Is.EqualTo("Product Area"));
		Assert.That(headings.ElementAt(4).TextContent, Is.EqualTo("Module"));
		Assert.That(headings.ElementAt(5).TextContent, Is.EqualTo("Change Type"));
		Assert.That(headings.ElementAt(6).TextContent, Is.EqualTo("Priority"));
		Assert.That(headings.ElementAt(7).TextContent, Is.EqualTo("Location Code"));
		Assert.That(headings.ElementAt(8).TextContent, Is.EqualTo("Rotation 1 Team"));
		Assert.That(headings.ElementAt(9).TextContent, Is.EqualTo("Rotation Manager Staff Code"));
		Assert.That(headings.ElementAt(10).TextContent, Is.EqualTo("Weeks per rotation"));
		Assert.That(headings.ElementAt(11).TextContent, Is.EqualTo("Weeks in Core Skills Training"));
		Assert.That(headings.ElementAt(12).TextContent, Is.EqualTo("Core Skills Training"));
		Assert.That(headings.ElementAt(13).TextContent, Is.EqualTo("Developer"));
		Assert.That(headings.ElementAt(14).TextContent, Is.EqualTo("Work item created"));
	}

	[Test]
	public async Task TableDataRenderedInOrderAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{
				StartDate = new DateOnly(2020, 2, 2),
				Rotation1Team = "team",
				RotationManager = "manager",
				WeeksPerRotation = 5,
				WeeksInCoreSkillsTraining = 4
			};
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});
		var table = cut.Find("table");
		Assert.That(table, Is.Not.Null);

		var inputs = table.QuerySelectorAll("input");
		Assert.That(inputs.Length, Is.EqualTo(14));
		Assert.That(inputs.ElementAt(0).GetAttribute("value"), Is.EqualTo("2020-02-02"));
		Assert.That(inputs.ElementAt(1).GetAttribute("value"), Is.EqualTo("sta"));
		Assert.That(inputs.ElementAt(2).GetAttribute("value"), Is.EqualTo("typ"));
		Assert.That(inputs.ElementAt(3).GetAttribute("value"), Is.EqualTo("are"));
		Assert.That(inputs.ElementAt(4).GetAttribute("value"), Is.EqualTo("act"));
		Assert.That(inputs.ElementAt(5).GetAttribute("value"), Is.EqualTo("ast"));
		Assert.That(inputs.ElementAt(6).GetAttribute("value"), Is.EqualTo("pri"));
		Assert.That(inputs.ElementAt(7).GetAttribute("value"), Is.EqualTo("locat"));
		Assert.That(inputs.ElementAt(8).GetAttribute("value"), Is.EqualTo("team"));
		Assert.That(inputs.ElementAt(9).GetAttribute("value"), Is.EqualTo("manager"));
		Assert.That(inputs.ElementAt(10).GetAttribute("value"), Is.EqualTo("5"));
		Assert.That(inputs.ElementAt(11).GetAttribute("value"), Is.EqualTo("4"));

		var cells = table.QuerySelectorAll("td");
		Assert.That(cells.ElementAt(15).TextContent, Is.EqualTo("No"));
	}

	[Test]
	public async Task EditValuesUpdatesWorkItemDTOAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
		{ StartDate = new DateOnly(2020, 2, 2), Rotation1Team = "team", RotationManager = "manager", WeeksPerRotation = 5, WeeksInCoreSkillsTraining = 4 };
		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var startDateInput = cut.Find("#start_date");
		var staffCodeInput = cut.Find("#staff_code");
		var productInput = cut.Find("#product");
		var productAreaInput = cut.Find("#product_area");
		var moduleInput = cut.Find("#module");
		var changeTypeInput = cut.Find("#change_type");
		var priorityInput = cut.Find("#priority");
		var locationCodeInput = cut.Find("#location_code");
		var rotationOneInput = cut.Find("#rotation_one");
		var rotationManagerInput = cut.Find("#rotation_manager");
		var rotationWeeksInput = cut.Find("#rotation_weeks");
		var coreSkillsWeeksInput = cut.Find("#core_skills_weeks");

		startDateInput.Input("2021-10-01");
		staffCodeInput.Input("AAA");
		productInput.Input("BBB");
		productAreaInput.Input("CCC");
		moduleInput.Input("DDD");
		changeTypeInput.Input("EEE");
		priorityInput.Input("FFF");
		locationCodeInput.Input("sydney");
		rotationOneInput.Input("modernization");
		rotationManagerInput.Input("person");
		rotationWeeksInput.Input("2");
		coreSkillsWeeksInput.Input("1");

		Assert.That(workItem.StartDate, Is.EqualTo(new DateOnly(2021, 10, 1)));
		Assert.That(workItem.StaffCode, Is.EqualTo("AAA"));
		Assert.That(workItem.Type, Is.EqualTo("BBB"));
		Assert.That(workItem.Area, Is.EqualTo("CCC"));
		Assert.That(workItem.ActivityType, Is.EqualTo("DDD"));
		Assert.That(workItem.ActivitySubType, Is.EqualTo("EEE"));
		Assert.That(workItem.Priority, Is.EqualTo("FFF"));
		Assert.That(workItem.LocationCode, Is.EqualTo("sydney"));
		Assert.That(workItem.Rotation1Team, Is.EqualTo("modernization"));
		Assert.That(workItem.RotationManager, Is.EqualTo("person"));
		Assert.That(workItem.WeeksPerRotation, Is.EqualTo(2));
		Assert.That(workItem.WeeksInCoreSkillsTraining, Is.EqualTo(1));
	}

	[TestCase(false, true, "No", TestName = "InputsEnabledBeforeWorkItemCreatedAsync")]
	[TestCase(true, false, "Yes", TestName = "InputsDisabledAfterWorkItemCreatedAsync")]
	public async Task TestInputsStateAfterWorkItemCreatedAsync(bool shouldCreateWorkItem, bool shouldBeNull, string expectedTextContent)
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{
				StartDate = new DateOnly(2020, 2, 2),
				Rotation1Team = "team",
				RotationManager = "manager",
				WeeksPerRotation = 5,
				WorkItemCreated = shouldCreateWorkItem
			};
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});
		var table = cut.Find("table");
		Assert.That(table, Is.Not.Null);

		var inputs = table.QuerySelectorAll("input");
		Assert.That(inputs.Length, Is.EqualTo(14));

		for (int i = 0; i < inputs.Length; i++)
		{
			var element = inputs[i];
			string? disabledValue = element.GetAttribute("disabled");
			Assert.That(disabledValue == null, Is.EqualTo(shouldBeNull), $"Failed at index {i}");
		}

		var cell = cut.Find(".work-item__created");
		Assert.That(cell.TextContent, Is.EqualTo(expectedTextContent));
	}

	[Test]
	public async Task CoreSkillsTrainingAndDeveloperCustomFieldIsCheckedAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{ StartDate = new DateOnly(2020, 2, 2), Rotation1Team = "team", RotationManager = "manager", WeeksPerRotation = 5 };
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var cstByID = cut.Find("#core_skills_training");
		var developerByID = cut.Find("#developer");

		Assert.That(cstByID.HasAttribute("checked"), Is.EqualTo(true));
		Assert.That(developerByID.HasAttribute("checked"), Is.EqualTo(true));
	}

	[Test]
	public async Task CopyLastRowAddsNewRowWithSameValuesAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "", "locat")
			{ StartDate = new DateOnly(2020, 2, 2), Rotation1Team = "", RotationManager = "", WeeksPerRotation = 5, WeeksInCoreSkillsTraining = 4, IsCoreSkillsTraining = true, IsDeveloper = false };
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});
		var table = cut.Find("table");
		Assert.That(table, Is.Not.Null);

		var button = cut.Find("[data-test='copy-last-row']");
		button.Click();

		var rows = table.QuerySelectorAll("tr");

		var lastRowInputs = rows.Last().QuerySelectorAll("input");
		Assert.That(rows.Length, Is.EqualTo(3));
		Assert.That(lastRowInputs.ElementAt(0).GetAttribute("value"), Is.EqualTo("2020-02-02"));
		Assert.That(lastRowInputs.ElementAt(1).GetAttribute("value"), Is.EqualTo(""));
		Assert.That(lastRowInputs.ElementAt(2).GetAttribute("value"), Is.EqualTo("typ"));
		Assert.That(lastRowInputs.ElementAt(3).GetAttribute("value"), Is.EqualTo("are"));
		Assert.That(lastRowInputs.ElementAt(4).GetAttribute("value"), Is.EqualTo("act"));
		Assert.That(lastRowInputs.ElementAt(5).GetAttribute("value"), Is.EqualTo("ast"));
		Assert.That(lastRowInputs.ElementAt(6).GetAttribute("value"), Is.EqualTo("pri"));
		Assert.That(lastRowInputs.ElementAt(7).GetAttribute("value"), Is.EqualTo("locat"));
		Assert.That(lastRowInputs.ElementAt(8).GetAttribute("value"), Is.EqualTo(""));
		Assert.That(lastRowInputs.ElementAt(9).GetAttribute("value"), Is.EqualTo(""));
		Assert.That(lastRowInputs.ElementAt(10).GetAttribute("value"), Is.EqualTo("5"));
		Assert.That(lastRowInputs.ElementAt(11).GetAttribute("value"), Is.EqualTo("4"));

		// Find the checkboxes for the boolean values
		var developerCheckBox = cut.Find("#developer");
		var coreSkillsCheckBox = cut.Find("#core_skills_training");
		Assert.That(coreSkillsCheckBox.IsChecked, Is.True); // Check for IsCoreSkillsTraining
		Assert.That(developerCheckBox.IsChecked, Is.False); // Check for IsDeveloper
	}
	[Test]
	public async Task TestClickingCopyIconCopiesEmailToCipboardAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var creatorMock = new Mock<IWorkItemCreator>();
		var cut = await ctx.RenderFormAsync(() =>
		{
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "QWE", "locat")
			{
				StartDate = new DateOnly(2020, 2, 2),
				Rotation1Team = "best team",
				RotationManager = "manager",
				WeeksPerRotation = 5,
				WeeksInCoreSkillsTraining = 4,
				IsCoreSkillsTraining = true,
				IsDeveloper = false
			};
			// set up			
			var creator = new RotationWorkItemCreator();
			var workItemDto = creator.CreateDefaultWorkItemDTO();
			workItemDto.StaffCode = "QWE";
			workItemDto.StartDate = new DateOnly(2023, 11, 13);
			workItemDto.Rotation1Team = "best team";
			workItemDto.WeeksPerRotation = 7;
			workItemDto.LocationCode = "AUSYD";

			var rotationWorkItem = new Mock<IRotationWorkItem>();

			var workItemResult = new CreateWorkItemResult()
			{
				CreatedWorkItem = rotationWorkItem.Object,
				StaffEmailAddress = "xyz@gmail.com",
				StatusCode = CreateWorkItemStatusCode.Success,
				WorkItemDTO = workItem,
			};
			var results = new List<CreateWorkItemResult> { workItemResult };
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			creatorMock.Setup(m => m.CreateWorkItems(It.IsAny<IEnumerable<WorkItemDTO>>())).Returns(results);
			creatorMock.Setup(m => m.CopyEmails(It.IsAny<ValueTask>())).Returns(ValueTask.CompletedTask);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var table = cut.Find("table");
		Assert.That(table, Is.Not.Null);
		
		cut.Find("[data-test='create-items']").Click();
		cut.Find("[data-test='copy-email']").Click();
		
		creatorMock.Verify(m => m.CopyEmails(It.IsAny<ValueTask>()), Times.Once);
	}

	[Test]
	public async Task EmptyEmailAddressesDoesNotShowEmailAddressElementAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{ StartDate = new DateOnly(2020, 2, 2), Rotation1Team = "team", RotationManager = "manager", WeeksPerRotation = 5 };
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		cut.Find("[data-test='create-items']").Click();
		Assert.That(cut.FindAll("[data-test='email-address']").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task WarningsAreNotDisplayedWhenNoDuplicateWorkItemsAsync()
	{
		// set up
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{ StartDate = new DateOnly(2020, 2, 2), Rotation1Team = "team", RotationManager = "manager", WeeksPerRotation = 5 };
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		// assert
		var table = cut.Find("table");
		Assert.That(table, Is.Not.Null);

		var inputs = table.QuerySelectorAll("input");
		inputs.ElementAt(1).Input("AAA");

		var form = cut.Find(".form");
		Assert.That(form.TextContent, Does.Not.Contain("Warning"));
	}

	[Test]
	public async Task WarningIsDisplayedWhenDuplicateWorkItemsAsync()
	{
		// set up
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(() => new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat"));
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		Assert.That(() => cut.FindAll("input"), Has.Count.EqualTo(14));
		cut.FindAll("input")[1].Input("AAA");
		Assert.That(cut.Find(".form").TextContent, Does.Not.Contain("Warning"));

		// act
		cut.Find("[data-test='add-item']").Click();
		cut.FindAll("input")[15].Input("AAA");

		cut.Find("[data-test='add-item']").Click();
		cut.FindAll("input")[29].Input("AAA");

		cut.Find("[data-test='add-item']").Click();
		cut.FindAll("input")[43].Input("BBB");

		cut.Find("[data-test='add-item']").Click();
		cut.FindAll("input")[57].Input("BBB");

		// assert
		Assert.That(() => Regex.Matches(cut.Find(".form").TextContent, "Duplicate workitems with staffCode 'AAA'"), Has.Count.EqualTo(1).After(3000, 100));
		Assert.That(() => Regex.Matches(cut.Find(".form").TextContent, "Duplicate workitems with staffCode 'BBB'"), Has.Count.EqualTo(1).After(3000, 100));
	}

	[Test]
	public async Task CreateWorkItemsUpdatesWorkItemAndDisplaysWorkItemCreatedAsync()
	{
		// setup
		using var ctx = new EnterpriseTestContext();

		var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
		{ StartDate = new DateOnly(2020, 2, 2), Rotation1Team = "team", RotationManager = "manager", WeeksPerRotation = 5, WeeksInCoreSkillsTraining = 4 };
		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			creatorMock.Setup(m => m.CreateWorkItems(It.IsAny<IEnumerable<WorkItemDTO>>())).Returns<IEnumerable<WorkItemDTO>>((items) =>
			{
				foreach (var item in items)
				{
					item.WorkItemCreated = true;
				}
				return Enumerable.Empty<CreateWorkItemResult>();
			});
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var startDateInput = cut.Find("#start_date");
		var staffCodeInput = cut.Find("#staff_code");
		var productInput = cut.Find("#product");
		var productAreaInput = cut.Find("#product_area");
		var moduleInput = cut.Find("#module");
		var changeTypeInput = cut.Find("#change_type");
		var priorityInput = cut.Find("#priority");
		var locationCodeInput = cut.Find("#location_code");
		var rotationOneInput = cut.Find("#rotation_one");
		var rotationManagerInput = cut.Find("#rotation_manager");
		var rotationWeeksInput = cut.Find("#rotation_weeks");
		var coreSkillsWeeksInput = cut.Find("#core_skills_weeks");
		var coreSkillsTrainingInput = cut.Find("#core_skills_training");
		var developerInput = cut.Find("#developer");

		startDateInput.Input("2021-10-01");
		staffCodeInput.Input("AAA");
		productInput.Input("BBB");
		productAreaInput.Input("CCC");
		moduleInput.Input("DDD");
		changeTypeInput.Input("EEE");
		priorityInput.Input("FFF");
		locationCodeInput.Input("sydney");
		rotationOneInput.Input("modernization");
		rotationManagerInput.Input("person");
		rotationWeeksInput.Input("2");
		coreSkillsWeeksInput.Input("1");
		coreSkillsTrainingInput.Input("advanced");
		developerInput.Input("XYZ");

		Assert.That(workItem.StartDate, Is.EqualTo(new DateOnly(2021, 10, 1)));
		Assert.That(workItem.StaffCode, Is.EqualTo("AAA"));
		Assert.That(workItem.Type, Is.EqualTo("BBB"));
		Assert.That(workItem.Area, Is.EqualTo("CCC"));
		Assert.That(workItem.ActivityType, Is.EqualTo("DDD"));
		Assert.That(workItem.ActivitySubType, Is.EqualTo("EEE"));
		Assert.That(workItem.Priority, Is.EqualTo("FFF"));
		Assert.That(workItem.LocationCode, Is.EqualTo("sydney"));
		Assert.That(workItem.Rotation1Team, Is.EqualTo("modernization"));
		Assert.That(workItem.RotationManager, Is.EqualTo("person"));
		Assert.That(workItem.WeeksPerRotation, Is.EqualTo(2));
		Assert.That(workItem.WeeksInCoreSkillsTraining, Is.EqualTo(1));

		// act
		cut.Find("[data-test='create-items']").Click();

		// assert
		Assert.That(cut.Find("[data-test='work-item-created']").TextContent, Is.EqualTo("Yes"));
	}

	[Test]
	public async Task SelectingDateUpdatesStartDateForRotationWorkItemAsync()
	{
		// setup
		using var ctx = new EnterpriseTestContext();

		var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
		{ StartDate = new DateOnly(2020, 2, 2), Rotation1Team = "team", RotationManager = "manager", WeeksPerRotation = 5, WeeksInCoreSkillsTraining = 4 };
		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			creatorMock.Setup(m => m.CreateWorkItems(It.IsAny<IEnumerable<WorkItemDTO>>())).Returns<IEnumerable<WorkItemDTO>>((items) =>
			{
				foreach (var item in items)
				{
					item.WorkItemCreated = true;
				}
				return Enumerable.Empty<CreateWorkItemResult>();
			});
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var startDateInput = cut.Find("#start_date");

		startDateInput.Input("2021-10-01");

		cut.Find("[data-test='create-items']").Click();

		Assert.That(workItem.StartDate, Is.EqualTo(new DateOnly(2021, 10, 1)));
	}

	[Test]
	public async Task CssClassesAreAppliedToElementsAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{
				StartDate = new DateOnly(2020, 2, 2),
				Rotation1Team = "team",
				RotationManager = "manager",
				WeeksPerRotation = 5,
				WeeksInCoreSkillsTraining = 4
			};
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var inputs = cut.FindAll(".form__input");
		var datagrid = cut.FindAll(".data-grid");
		var workItemForm = cut.FindAll(".form");
		var buttons = cut.FindAll(".button-container button");

		Assert.Multiple(() =>
		{
			Assert.That(inputs.Count, Is.EqualTo(12));
			Assert.That(datagrid.Count, Is.EqualTo(1));
			Assert.That(workItemForm.Count, Is.EqualTo(1));
			Assert.That(buttons.Count, Is.EqualTo(4));
		});
	}

	[Test, WithPlaywrightPage()]
	public async Task ComponentRendersWithProperStylesAsync()
	{
			await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
			{
				var form = new Form() { Width = 1250, Height = 700 };
				var creatorMock = new Mock<IWorkItemCreator>();
				var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
				{
					StartDate = new DateOnly(2020, 2, 2),
					Rotation1Team = "team",
					RotationManager = "manager",
					WeeksPerRotation = 5,
					WeeksInCoreSkillsTraining = 4
				};
				creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);

				var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
				return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
			});

		var staffCode = await page.WaitForSelectorAsync("#staff_code");
		await staffCode!.ClickAsync();

		var form = await page.WaitForSelectorAsync(".form");
		var form_button = await page.WaitForSelectorAsync(".button-container__button");
		var formInput = await page.WaitForSelectorAsync(".form__input");
		var formInputInvalid = await page.WaitForSelectorAsync(".form__input:invalid");
		var formInputFocus = await page.WaitForSelectorAsync(".form__input:focus");
		var dataGrid = await page.WaitForSelectorAsync(".data-grid");
		var td = await page.WaitForSelectorAsync(".data-grid__cell");
		var button_container = await page.WaitForSelectorAsync(".button-container");

		Assert.Multiple(() =>
		{
			AssertCssPropertyContainsValue(form!, "font-family", "Tahoma, sans-serif");
			AssertCssPropertyContainsValue(form!, "table-layout", "auto");
			AssertCssPropertyContainsValue(form_button!, "margin-right", "5px");
			AssertCssPropertyContainsValue(formInput!, "border", "none");
			AssertCssPropertyContainsValue(formInput!, "font-size", "10.6667px");
			AssertCssPropertyContainsValue(formInput!, "max-width", "100%");
			AssertCssPropertyContainsValue(formInputInvalid!, "background", "rgb(255, 179, 179)");
			AssertCssPropertyContainsValue(formInputFocus!, "background", "rgb(255, 255, 225)");
			AssertCssPropertyContainsValue(td!, "padding", "0px");
			AssertCssPropertyContainsValue(button_container!, "display", "flex");
			AssertCssPropertyContainsValue(button_container!, "padding", "5px");
		});
	}

	void AssertCssPropertyContainsValue(IElementHandle element, string propertyName, string expectedValue)
	{
		Assert.That(async () => await element.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('{propertyName}')"), Does.Contain(expectedValue));
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollBarsArePresentWhenContentExceedsBoundsAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var cut = await ctx.LoadFormAsync(() =>
		{
			return new CreateRotationWorkItemsForm();
		});

		var scrollableDiv = await cut.WaitForSelectorAsync(".scrollable-div");
		await scrollableDiv!.ClickAsync();

		Assert.That(scrollableDiv, Is.Not.Null, "The .scrollable-div element should exist.");

		var viewportHeight = await cut.EvaluateAsync<int>("() => window.innerHeight");
		var expectedMaxHeight = $"{viewportHeight - 20}px";

		var viewportWidth = await cut.EvaluateAsync<int>("() => window.innerWidth");
		var expectedMaxWidth = $"{viewportWidth}px";

		Assert.Multiple(() =>
		{
			 AssertCssPropertyContainsValue(scrollableDiv!, "max-height", expectedMaxHeight);
			 AssertCssPropertyContainsValue(scrollableDiv!, "max-width", expectedMaxWidth);
			 AssertCssPropertyContainsValue(scrollableDiv!, "overflow-y", "auto");
			 AssertCssPropertyContainsValue(scrollableDiv!, "overflow-x", "auto");
		});
	}

	[Test]
	public async Task ErrorIsGeneratedWhenStaffCodeIsBlankAsync()
	{
		//setup
		using var ctx = new EnterpriseTestContext();
		string myLocalVal = string.Empty;

		var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
		createWorkItemServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>())).Callback((string p) => myLocalVal = p);

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{
				StartDate = new DateOnly(2020, 2, 2),
				Rotation1Team = "team",
				RotationManager = "manager",
				WeeksPerRotation = 5
			};
			var results = new List<CreateWorkItemResult>();
			var result = new CreateWorkItemResult
			{
				StatusCode = CreateWorkItemStatusCode.StaffNotFound,
				WorkItemDTO = workItem
			};
			results.Add(result);
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			creatorMock.Setup(m => m.CreateWorkItems(It.IsAny<IEnumerable<WorkItemDTO>>())).Returns(results);
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});
		//act
		var staffCodeInput = cut.Find("#staff_code");
		staffCodeInput.Input(""); // Set staff code to blank

		var button = cut.Find("[data-test='create-items']");
		button.Click();

		//assert
		string expectedErrorMessage = "Staff with code \"\" could not be found. No work item created.\r\n";
		Assert.That(myLocalVal, Is.EqualTo(expectedErrorMessage));
	}

	[Test]
	public async Task NoErrorIsGeneratedWhenStaffCodeIsRightAsync()
	{
		//setup
		using var ctx = new EnterpriseTestContext();

		var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
		createWorkItemServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>()));

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{
				StartDate = new DateOnly(2020, 2, 2),
				Rotation1Team = "team",
				RotationManager = "manager",
				WeeksPerRotation = 5
			};
			var rotationWorkItem = new Mock<IRotationWorkItem>();
			var results = new List<CreateWorkItemResult>();
			var result = new CreateWorkItemResult
			{
				StatusCode = CreateWorkItemStatusCode.Success,
				WorkItemDTO = workItem,
				CreatedWorkItem = rotationWorkItem.Object,
			};
			results.Add(result);
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			creatorMock.Setup(m => m.CreateWorkItems(It.IsAny<IEnumerable<WorkItemDTO>>())).Returns(results);
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});
		//act
		var button = cut.Find("[data-test='create-items']");
		button.Click();

		//check
		createWorkItemServiceMock.Verify(m => m.ShowMessage(It.IsAny<String>()), Times.Never);
	}

	[Test]
	public async Task TestForCreateButtonWhenWorkItemCreatedIsYesAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
		createWorkItemServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>()));

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{
				StartDate = new DateOnly(2020, 2, 27),
				Rotation1Team = "team",
				RotationManager = "manager",
				WeeksPerRotation = 5,
				WeeksInCoreSkillsTraining = 4,
				WorkItemCreated = true
			};
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var createButton = cut.Find("[data-test='create-items']");

		Assert.That(createButton, Is.Not.Null);
		Assert.That(createButton.TextContent, Is.EqualTo("Create Work Items"));
		Assert.That(createButton.IsDisabled(), Is.True, "The button should be disabled");
	}

	[Test]
	public async Task TestForCreateButtonWhenWorkItemCreatedIsNoAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
		createWorkItemServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>()));

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{
				StartDate = new DateOnly(2020, 2, 27),
				Rotation1Team = "team",
				RotationManager = "manager",
				WeeksPerRotation = 5,
				WeeksInCoreSkillsTraining = 4,
				WorkItemCreated = false
			};
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var createButton = cut.Find("[data-test='create-items']");

		Assert.That(createButton, Is.Not.Null);
		Assert.That(createButton.TextContent, Is.EqualTo("Create Work Items"));
		Assert.That(createButton.IsDisabled(), Is.False, "The button should not be disabled");
	}

	[Test]
	public async Task TestForCreateButtonWhenWorkItemCreatedIsYesAndNoBothAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
		createWorkItemServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>()));

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();

			var workItem1 = new WorkItemDTO("summary1", "type1", "area1", "activity1", "asset1", "priority1", "state1", "location1")
			{
				StartDate = new DateOnly(2020, 4, 2),
				Rotation1Team = "team1",
				RotationManager = "manager1",
				WeeksPerRotation = 5,
				WeeksInCoreSkillsTraining = 4,
				WorkItemCreated = false
			};

			var workItem2 = new WorkItemDTO("summary2", "type2", "area2", "activity2", "asset2", "priority2", "state2", "location2")
			{
				StartDate = new DateOnly(2021, 6, 15),
				Rotation1Team = "team2",
				RotationManager = "manager2",
				WeeksPerRotation = 6,
				WeeksInCoreSkillsTraining = 3,
				WorkItemCreated = true
			};

			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem1).Callback(() => creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem2));

			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});
		
		var createButton = cut.Find("[data-test='create-items']");

		Assert.That(createButton, Is.Not.Null);
		Assert.That(createButton.TextContent, Is.EqualTo("Create Work Items"));
		Assert.That(createButton.IsDisabled(), Is.False, "The button should not be disabled");
	}

	[TestCase(false, "0", TestName = "{m}_MinValueForCoreSkillsWeeks_WhenCoreSkillsTrainingIsUnchecked_ExpectMinValueZero")]
	[TestCase(true, "1", TestName = "{m}_MinValueForCoreSkillsWeeks_WhenCoreSkillsTrainingIsChecked_ExpectMinValueOne")]
	public async Task Test_MinValueForCoreSkillsWeeksAsync(bool isCoreSkillsTraining, string expectedMinValue)
	{
		using var ctx = new EnterpriseTestContext();

		var createWorkItemServiceMock = new Mock<ICreateWorkItemsService>();
		createWorkItemServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>()));

		var cut = await ctx.RenderFormAsync(() =>
		{
			var creatorMock = new Mock<IWorkItemCreator>();
			var workItem = new WorkItemDTO("summary", "typ", "are", "act", "ast", "pri", "sta", "locat")
			{
				StartDate = new DateOnly(2020, 2, 27),
				Rotation1Team = "team",
				RotationManager = "manager",
				WeeksPerRotation = 5,
				WorkItemCreated = false
			};
			creatorMock.Setup(m => m.CreateDefaultWorkItemDTO()).Returns(workItem);
			return new CreateRotationWorkItemsForm(creatorMock.Object, createWorkItemServiceMock.Object);
		});

		var isCoreSkillsTrainingCheckbox = cut.Find("#core_skills_training");

		isCoreSkillsTrainingCheckbox.Input(isCoreSkillsTraining);

		var weeksInCoreSkillsTraining = cut.Find("#core_skills_weeks");
		var minAttribute = weeksInCoreSkillsTraining.GetAttribute("min");

		Assert.That(minAttribute, Is.EqualTo(expectedMinValue));
	}
}
