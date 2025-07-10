using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class ChannelHeaderControlTest : BMSGUITestCase
	{
		#region Thread Test

		public void TestChannelHasFactoryFromBackgroundThread()
		{
			var buffer = CreateBuffer(system, "Mai Buffer");
			var section = CreateBoardSection(buffer);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.ProfileImage = new Bitmap(800, 800);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var cell = new CellContent(0, 0, CellContentType.ChannelHeading) { Channel = viewModel.CreateChannelForTest(resource) };
			var newFactory = Factory.CreateNewFactory();
			cell.Channel.ClearChannelCache();

			newFactory.ThreadSentry.RelinquishThreadOwnership();

			var thread = new Thread(() =>
			{
				newFactory.ThreadSentry.TakeThreadOwnership();
			});
			thread.Start();
			thread.Join();

			AssertNoExceptionThrown(() =>
			{
				using (var control = new ChannelHeaderControlForTest(cell, viewModel))
				{
					Assert(true);
				}
			});
		}

		public void TestRefreshHeading_WhenParentFormAlreadyClosed_ShouldNotBotherRefreshingChannel()
		{
			var buffer = CreateBuffer(system, "Buffer");
			var section = CreateBoardSection(buffer);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.ProfileImage = new Bitmap(800, 800);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateViewModel(section);
			var cell = new CellContent(0, 0, CellContentType.ChannelHeading) { Channel = sectionViewModel.CreateChannelForTest(resource) };

			using (var control = new ChannelHeaderControl(cell, sectionViewModel).WrapWithShownForm())
			{
				var factory = new BusinessObjectFactory();
				var form = control.FindForm();

				form.Close();

				AssertEquals(true, form.IsDisposed);
				AssertNoExceptionThrown(() => control.RefreshHeading(control, new HeadingsRefreshedEventArgs(null)));
			}
		}

		#endregion

		#region Channel Load Scale

		[TestDate(2014, 1, 28)]
		public void TestChannelLoadScale_WhenResourceHasNoCapacity_AndNothingAssigned()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(30), resource.PK);

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var cell = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertFadePanelDetails("Should be no fade since there is no available or utilised capacity. Also shouldn't throw div/0 exception.", fadePanel, 0f, 0f, 0.1f, cell.BackColor ?? control.BackColor, cell.BackColor ?? control.BackColor);
			}
		}

		[TestDate(2014, 1, 28)]
		public void TestChannelLoadScale_WhenResourceHasNoCapacity_AndSomethingAssigned()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(30), resource.PK);

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.BackgroundColor = ZString.Empty;
			section.ForegroundColor = ZString.Empty;
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });
			var cell = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertFadePanelDetails("Fade should be full since there is no available capacity, but some has been utilised. Also shouldn't throw div/0 exception.", fadePanel, 0f, 0.95f, 0.1f, BMConstants.GoodBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control);
			}
		}

		[TestDate(2014, 1, 28)]
		public void TestChannelLoadScale_WhenUtilisedCapacityPercentChangedToZero()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(30), resource.PK);

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.BackgroundColor = ZString.Empty;
			section.ForegroundColor = ZString.Empty;
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });
			var cell = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertFadePanelDetails("Fade should be full since there is no available capacity, but some has been utilised. Also shouldn't throw div/0 exception.", fadePanel, 0f, 0.95f, 0.1f, BMConstants.GoodBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control);

				task.P9_Status = "CLS";

				Factory.Save();

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertFadePanelDetails("Should be no fade since there is no available or utilised capacity. Also shouldn't throw div/0 exception.", fadePanel, 0f, 0f, 0.1f, SystemColors.Control, SystemColors.Control);
			}
		}

		[TestDate(2014, 1, 28)]
		public void TestChannelLoadScale_HorizontalOrientation_NoFadeAtPercentage()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.BackgroundColor = ZString.Empty;
			section.ForegroundColor = ZString.Empty;
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 0;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * buffer.FC_BufferLoadLimitPercent / 100.0 / 4.0)); // 1/4 of relevant buffer timespan

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });
			var cell = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertFadePanelDetails("", fadePanel, 270f, 0.32f, 0.1f, Color.FromArgb(255, 180, 180, 180), SystemColors.Control);
			}
		}

		[TestDate(2014, 1, 28)]
		public void TestChannelLoadScale_OverloadedChannel()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.BackgroundColor = ZString.Empty;
			section.ForegroundColor = ZString.Empty;
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, buffer.FC_BufferTimespanInMinutes);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });
			var cell = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.Channel.ClearCacheAndReload();
				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertFadePanelDetails("Overloaded channel should be high risk", fadePanel, 0f, 1f, 0.1f, BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control);
			}
		}

		#endregion

		#region AvailableForBMSWorkMenuItem

		public void TestAvailableForBMSWorkMenuItem()
		{
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system, "Mai Buffer");
			LinkComponents(bucket, buffer);

			var section = CreateBoardSection(buffer);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "Frodo Baggins";
			resource.GS_Code = "FRO";
			resource.Holidays.RemoveAndDeleteAll();
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(resource, system, section, viewModel))
			{
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);

				Assert("Should be at least three context menu items", control.ContextMenuStrip.Items.Count >= 3);
				var availabilityMenuItem = control.ContextMenuStrip.Items.Find("Availability", false).FirstOrDefault();
			}
		}

		#endregion

		#region CCR Functionality

		public void TestMarkAsCCRContextMenuItem_WithNULLCardAllocationMap_ShouldAvoidException()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.Board)))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();
				var control = FindChannelHeaderControl(form, config.NonCCR1);

				AssertExceptionThrown<NullReferenceException>(
					"Simulate CardAllocationMap=NULL in ChannelHeaderControl.RefreshHeadingAsync",
					"Object reference not set to an instance of an object.", () =>
					{
						control.ViewModel_ForTest.ComponentGrid.RefreshComponent(Factory, control.ViewModel_ForTest, null, requiresFullRedraw: true);
					});

				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);
				Assert("Should be at least two context menu items", control.ContextMenuStrip.Items.Count >= 2);

				var menuItem = control.ContextMenuStrip.Items[2];
				AssertEquals("Mark as capacity constrained", menuItem.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertNoExceptionThrown(menuItem.PerformClick);
			}
		}

		public void TestMarkAsCCRContextMenuItem()
		{
			Factory.RefreshEnabled = false;

			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system, "Mai Buffer");
			LinkComponents(bucket, buffer);

			var board = CreateBoard(system, "Mai Board");
			var section = CreateBoardSection(buffer, board);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "Frodo Baggins";
			resource.GS_Code = "FRO";

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			Factory.ServiceContainer.AddService(new CapacityConstrainedResourcesCacheService(board));
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(resource, system, section))
			{
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);

				Assert("Should be at least two context menu items", control.ContextMenuStrip.Items.Count >= 2);

				var menuItem = control.ContextMenuStrip.Items[1];
				AssertEquals("Mark as capacity constrained", menuItem.Text);

				menuItem.PerformClick();
				AssertEquals("Mark as non-capacity constrained", menuItem.Text);
				AssertMultilineASCIIEquals("", "Mark Frodo Baggins as capacity constrained?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Mark as capacity constrained", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, buffer));

				buffer.FC_BufferTimeCapacityConstraintThresholdMultiple = 2;
				Factory.Save();

				menuItem.PerformClick();
				AssertEquals("Mark as capacity constrained", menuItem.Text);
				AssertMultilineASCIIEquals("", "Marked Frodo Baggins as non-capacity constrained.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Caption);

				buffer = new BusinessObjectFactory().Load<BMComponent>(buffer.PK);
				AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, buffer));

				var resourceComponentLink = buffer.GetOrCreateResourceLink(resource.GS_Code);
				resourceComponentLink.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow.AddDays(-8);
				resourceComponentLink.FD_IsPersistentlyOverloaded = true;
				buffer.Factory.Save();

				menuItem.PerformClick();
				AssertEquals("Mark as non-capacity constrained", menuItem.Text);
				AssertEquals("Mark Frodo Baggins as capacity constrained?\r\nFrodo Baggins was detected as capacity constrained over 1 week ago.", UnitTestUserNotification.Instance.LastMessage.Text);

				buffer = new BusinessObjectFactory().Load<BMComponent>(buffer.PK);
				AssertEquals(true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, buffer));

				menuItem.PerformClick();
				AssertEquals("Mark as capacity constrained", menuItem.Text);
				AssertEquals("Marked Frodo Baggins as non-capacity constrained.", UnitTestUserNotification.Instance.LastMessage.Text);

				buffer = new BusinessObjectFactory().Load<BMComponent>(buffer.PK);
				AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, buffer));

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				menuItem.PerformClick();
				AssertEquals("Mark as capacity constrained", menuItem.Text);
				AssertEquals("Mark Frodo Baggins as capacity constrained?\r\nFrodo Baggins was detected as capacity constrained over 1 week ago.", UnitTestUserNotification.Instance.LastMessage.Text);

				buffer = new BusinessObjectFactory().Load<BMComponent>(buffer.PK);
				AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, buffer));
			}
		}

		public void TestMarkAsCCRContextMenuItem_ShouldBePresentOnlyForResourceChannels()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, shouldUseExistingSystem: true);
			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var tag = Factory.NewWithValidTestData<TagMagnitude>();

			Factory.Save();

			var channels = new Dictionary<string, IVisualBoardChannel>();

			// New channel types will need to be added here
			channels[ChannelTypeList.Codes.Resource] = viewModel.CreateChannelForTest(resource);
			channels[ChannelTypeList.Codes.Capability] = viewModel.CreateChannelForTest(capability);
			channels[ChannelTypeList.Codes.Group] = viewModel.CreateChannelForTest(group);
			channels[ChannelTypeList.Codes.Tag] = viewModel.CreateChannelForTest(tag);

			var relevantChannelTypes =
				from ICodeDescription cdp in new ChannelTypeList()
				where !cdp.Code.In(ChannelTypeList.Codes.NotChanneled,
								ChannelTypeList.Codes.CurrentUser,
								ChannelTypeList.Codes.ReleaseSchedulerChannels)
				select cdp.Code;

			foreach (var channelType in relevantChannelTypes)
			{
				var cell = new CellContent(0, 0, CellContentType.ChannelHeading) { Channel = channels[channelType] };

				using (var control = new ChannelHeaderControl(cell, viewModel))
				{
					((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);

					var ccrMenuItem = control.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(t => t.Text == "Mark as capacity constrained");
					var availabilityMenuItem = control.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(t => t.Text == "Availability");

					if (channelType == ChannelTypeList.Codes.Resource)
					{
						AssertNotNull(ccrMenuItem);
						AssertNotNull(availabilityMenuItem);
					}
					else
					{
						AssertNull(string.Format("ChannelType {0} should NOT have a CCR menu item", channelType), ccrMenuItem);
						AssertNull(string.Format("ChannelType {0} should NOT have an availability menu item", channelType), availabilityMenuItem);
					}
				}
			}
		}

		public void TestCCRPictureBoxHasNoMargin()
		{
			var buffer = CreateBuffer(system, "Mai Buffer");

			var section = CreateBoardSection(buffer);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "Frodo Baggins";
			resource.GS_Code = "FRO";

			var resourceComponentLink = buffer.ResourceLinks.AddNew();
			resourceComponentLink.FD_GS_NKResource = resource.GS_Code;
			resourceComponentLink.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow.AddDays(-8);
			resourceComponentLink.FD_IsPersistentlyOverloaded = true;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(resource, system, section, viewModel))
			{
				var pictureBox = control.Controls.Find("CCRPictureBox", true).FirstOrDefault();

				AssertNotNull(pictureBox);

				AssertEquals(0, pictureBox.Margin.Left);
				AssertEquals(0, pictureBox.Margin.Top);
				AssertEquals(0, pictureBox.Margin.Right);
				AssertEquals(0, pictureBox.Margin.Bottom);
			}
		}

		public void TestCCRPictureBox_WhenInPreview_ShouldNotHitDb()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			group.Staff.Add(resource);

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			resource.DesignateAsCCR(buffer);

			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource.GS_Code, 60);
			var task2 = CreateTask(workflow, resource.GS_Code, 60);
			var task3 = CreateTask(workflow, string.Empty, 60, capability: capability);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var loadedResource = newFactory.Load<GlbStaff>(resource.PK);

			var viewModel = BMSTestHelper.CreateViewModel(loadedSection, isPreview: true);
			var cell = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading);

			using (var control = new ChannelHeaderControl(cell, viewModel))
			{
				var status = control.ChannelStatus;
				var ccrImage = control.CcrPictureBox.Image;

				CombineAssertions("Should not check real status in preview - no point hitting the db just for this", () =>
				{
					foreach (var table in VisualBoardChannelTest.TablesNotAllowedToBeHitInPreview)
					{
						if (table != BMComponentResourceLinkSchema.Constants.TableName) //WI0007974: ComponentGridBuilder indirectly acceing it to check CCR status
						{
							AssertEquals(table + " hit count", 0, newFactory.GetTableHitCount(table));
						}
					}
				});

				AssertEquals("Working", status);
				AssertNull("Even though the resource is a CCR candidate, don't calculate this in preview - no point hitting the db for this", ccrImage);
			}
		}

		public void TestCCRWhichIsNotPersistentlyOverloaded_HasDifferentTooltip()
		{
			var testconfig = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var resourceLink = testconfig.Buffer.ResourceLinks.Single();
			resourceLink.FD_IsPersistentlyOverloaded = true;
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(testconfig.Board)))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();
				var headerControl = FindChannelHeaderControl(form, testconfig.CCR);
				AssertEquals("This resource is designated as a Capacity Constrained Resource. They have been detected to be persistently overloaded.", ToolTipService.GetToolTip(headerControl.CcrPictureBox));

				var originalCcrImage = headerControl.CcrPictureBox.Image;
				AssertNotNull("Make sure the hourglass is being displayed", originalCcrImage);

				resourceLink.FD_IsPersistentlyOverloaded = false;
				Factory.Save();
				form.ReloadBoard();
				headerControl = FindChannelHeaderControl(form, testconfig.CCR);

				AssertEquals("This resource is designated as a Capacity Constrained Resource.", ToolTipService.GetToolTip(headerControl.CcrPictureBox));
				Assert("The image should have changed from a hourglass-icon to a hourglass-icon-green and yet...", !headerControl.CcrPictureBox.Image.Equals(originalCcrImage));
			}
		}

		public void TestCCRIcon_WhenSavingDetailedTicket_ShouldContinueToDisplayIcon()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var board = config.BufferAndReleaseSchedulerBoard;
			var bufferSection = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, config.CCR.PK);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Returney", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(board))
			{
				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == bufferSection.PK);
				var channelHeading = sectionControl.FindSingle<ChannelHeaderControl>();

				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
				AssertNotNull("CCR icon should be shown originally", channelHeading.CcrPictureBox.Image);

				PlayTask(task, config.CCR, sectionControl);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
				AssertNotNull("CCR icon should not be changed since nothing has caused the resource to no longer be a CCR", channelHeading.CcrPictureBox.Image);

				form.RefreshNow_ForTest();

				AssertNotNull("CCR icon should not be changed since nothing has caused the resource to no longer be a CCR", channelHeading.CcrPictureBox.Image);
			}
		}

		#endregion

		#region RefreshHeadings

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestRefreshHeadings_ForUnChanneledChannel_ShouldNotThrowException()
		{
			var section = CreateBoardSection(CreateBuffer(system));
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;

			AssertEquals(1, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var unchanneledChannel = viewModel.PrimaryChannels.Single();
			var channelCell = viewModel.ComponentGrid.Cells.First(c => c.Channel == unchanneledChannel);

			using (var control = new ChannelHeaderControl(channelCell, viewModel))
			{
				AssertNoExceptionThrown(() => control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap));
				AssertEquals("No need tooltip for unchanneled channel", string.Empty, ToolTipService.GetToolTip(control.ChannelStatusLabel));
			}
		}

		public void TestRefreshHeadingsAsync_StatusImage_ShouldNotThrowException()
		{
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.OverrideChannels = true;

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "AAA");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "BBB");
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK, overrideChannels: true);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel1 = viewModel.PrimaryChannels.Single(x => x.EntityPK == staff1.PK);
			var channel2 = viewModel.PrimaryChannels.Single(x => x.EntityPK == staff2.PK);
			var cell1 = viewModel.ComponentGrid.Cells.First(x => x.Channel == channel1);
			var cell2 = viewModel.ComponentGrid.Cells.First(x => x.Channel == channel2);

			Factory.Save();

			using (var control1 = new ChannelHeaderControlForTest(cell1, viewModel))
			using (var control2 = new ChannelHeaderControlForTest(cell2, viewModel))
			{
				var image = control1.Channel.StatusImage.Image;
				var maxSize = new Size(40, 40);

				var task1 = new Task(() =>
				{
					for (var i = 0; i < 100; i++)
					{
						control1.GetImageAtMaxSize_Exposed(image, maxSize);
					}
				});

				var task2 = new Task(() =>
				{
					for (var i = 0; i < 100; i++)
					{
						control2.GetImageAtMaxSize_Exposed(image, maxSize);
					}
				});

				AssertNoExceptionThrown("Getting a resized image should not throw an 'Object is currently in use elsewhere.' exception, and yet... check that the image is locked properly before creating a new bitmap from it.", () =>
				{
					task1.Start();
					task2.Start();
					task1.Wait();
					task2.Wait();
				});
			}
		}

		#endregion

		#region ChannelHeaderText

		public void TestChannelHeaderText()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Cooking Plot";
			staff.GS_FriendlyName = "Hatch";
			staff.GS_Code = "THC";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			// without PreferredName ChannelHeader
			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Cooking Plot", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Hatche", control.ChannelNameLabel.Font).Width;
				AssertEquals("Hatch", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Hatc", control.ChannelNameLabel.Font).Width;
				AssertEquals("THC", control.ChannelNameLabel.Text);
			}

			BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			// with PreferredName ChannelHeader
			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Hatch", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("THC", control.ChannelNameLabel.Font).Width;
				AssertEquals("THC", control.ChannelNameLabel.Text);
			}
		}

		public void TestChannelHeaderText_ForTagChannel()
		{
			var tag = Factory.NewWithValidTestData<TagMagnitude>();
			tag.TGM_Description = "X-Wing @Alicioussness";
			tag.TGM_Code = "XW@";

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, shouldUseExistingSystem: true);
			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(tag, system, config.BufferSection, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("XW@ - X-Wing @Alicioussness", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("XW@ - X-Wing @Alicioussn", control.ChannelNameLabel.Font).Width;
				AssertEquals("X-Wing @Alicioussness", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("XW@", control.ChannelNameLabel.Font).Width;
				AssertEquals("XW@", control.ChannelNameLabel.Text);
			}
		}

		public void TestChannelHeaderText_NoFriendlyName()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Johnny X. Knoxville";
			staff.GS_Code = "JK";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			// without PreferredName ChannelHeader
			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Johnny X. Knoxville", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Johnny", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Joh", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("J", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);
			}

			BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			// with PreferredName ChannelHeader
			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Johnny X. Knoxville", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Johnny", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Joh", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("J", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);
			}
		}

		public void TestChannelHeaderText_VerticalLabels()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Johnny X. Knoxville";
			staff.GS_FriendlyName = "John";
			staff.GS_Code = "JK";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			// without PreferredName ChannelHeader
			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Johnny X. Knoxville", control.ChannelNameLabel.Text);

				control.Height = TextRenderer.MeasureText("Johnny", control.ChannelNameLabel.Font).Width;
				AssertEquals("John", control.ChannelNameLabel.Text);

				control.Height = TextRenderer.MeasureText("Joh", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);

				control.Height = TextRenderer.MeasureText("J", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);
			}

			BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			// with PreferredName ChannelHeader
			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Should be two vertical labels", 2, control.FindAll<DirectionalLabel>().Count(c => c.IsVertical));

				AssertEquals("John", control.ChannelNameLabel.Text);

				control.Height = TextRenderer.MeasureText("Johnny", control.ChannelNameLabel.Font).Width;
				AssertEquals("John", control.ChannelNameLabel.Text);

				control.Height = TextRenderer.MeasureText("Joh", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);

				control.Height = TextRenderer.MeasureText("J", control.ChannelNameLabel.Font).Width;
				AssertEquals("JK", control.ChannelNameLabel.Text);
			}
		}

		public void TestChannelHeaderText_WithLongSingularName()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Guðmundsdóttir";
			staff.GS_FriendlyName = "Björk";
			staff.GS_Code = "BG";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.Width = TextRenderer.MeasureText("Guðmundsdó", control.ChannelNameLabel.Font).Width;
				AssertEquals("Breaking up the Full Name won't help here as there are no smaller parts", "Björk", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Guð", control.ChannelNameLabel.Font).Width;
				AssertEquals("BG", control.ChannelNameLabel.Text);

				BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				control.Width = TextRenderer.MeasureText("Guðmundsdó", control.ChannelNameLabel.Font).Width;
				AssertEquals("Breaking up the Full Name won't help here as there are no smaller parts", "Björk", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Guðmu", control.ChannelNameLabel.Font).Width;
				AssertEquals("Björk", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Guð", control.ChannelNameLabel.Font).Width;
				AssertEquals("BG", control.ChannelNameLabel.Text);
			}
		}

		public void TestChannelHeaderText_WithLongFriendlyname()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Björkstensteinsson";
			staff.GS_FriendlyName = "Björksten";
			staff.GS_Code = "BG";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.Width = TextRenderer.MeasureText("Björkstensteinsson", control.ChannelNameLabel.Font).Width;
				AssertEquals("Björksten", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Björkstenst", control.ChannelNameLabel.Font).Width;
				AssertEquals("Björksten", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Björ", control.ChannelNameLabel.Font).Width;
				AssertEquals("BG", control.ChannelNameLabel.Text);

				BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				control.Width = TextRenderer.MeasureText("Björkstensteins", control.ChannelNameLabel.Font).Width;
				AssertEquals("Björksten", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Björkste", control.ChannelNameLabel.Font).Width;
				AssertEquals("BG", control.ChannelNameLabel.Text);
			}
		}

		public void TestChannelHeaderText_WithDisplayName()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Frodo";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				AssertEquals("Frodo", control.ChannelNameLabel.Text);
				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertEquals(control.ChannelStatus, ToolTipService.GetToolTip(control.ChannelStatusLabel));

				var pictureBox = (PictureBox)control.FindAll<TableLayoutPanel>().Single().GetControlFromPosition(0, 0);
				AssertNotNull(pictureBox);
				AssertNull("PictureBox image should be removed when no profile pic present", pictureBox.Image);
			}
		}

		public void TestChannelHeaderText_ShowsFullNameWhenSpaceAndNoPreferredName()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Björkstensteinsson";
			staff.GS_Code = "BG";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.Width = TextRenderer.MeasureText("Björkstensteinssonba", control.ChannelNameLabel.Font).Width;
				AssertEquals("Björkstensteinsson", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Björkstensteinsso", control.ChannelNameLabel.Font).Width;
				AssertEquals("BG", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("B", control.ChannelNameLabel.Font).Width;
				AssertEquals("BG", control.ChannelNameLabel.Text);
			}
		}

		public void TestChannelHeaderText_ShouldIgnoreFriendlyNameOnCruelRobotCapabilities()
		{
			var capableBot = Factory.NewWithValidTestData<GlbCapability>();
			capableBot.G4_Description = "Not a robotist assassin";
			capableBot.G4_Code = "ASS";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(capableBot, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.Width = TextRenderer.MeasureText("Not a robotist assassina", control.ChannelNameLabel.Font).Width;
				AssertEquals("Not a robotist assassin", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Not a robotist assassi", control.ChannelNameLabel.Font).Width;
				AssertEquals("ASS", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("AS", control.ChannelNameLabel.Font).Width;
				AssertEquals("ASS", control.ChannelNameLabel.Text);
			}
		}

		public void TestChannelHeaderText_ShouldIgnoreFriendlyNameOnCruelRobotGroups()
		{
			var groupableBot = Factory.NewWithValidTestData<GlbGroup>();
			groupableBot.GG_Desc = "Not a robotist assassin";
			groupableBot.GG_Code = "ASS";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(groupableBot, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.Width = TextRenderer.MeasureText("Not a robotist assassina", control.ChannelNameLabel.Font).Width;
				AssertEquals("Not a robotist assassin", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("Not a robotist assassi", control.ChannelNameLabel.Font).Width;
				AssertEquals("ASS", control.ChannelNameLabel.Text);

				control.Width = TextRenderer.MeasureText("AS", control.ChannelNameLabel.Font).Width;
				AssertEquals("ASS", control.ChannelNameLabel.Text);
			}
		}

		#endregion

		#region Channel Status

		[TestDate(2016, 9, 5)]
		public void TestChannelStatus_Fade_ForCCR()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true, makeResourcesPartOfReleaseGroup: false); // Because this test is highly reliant on the incorrect behaviour that CCR/non-CCR resources are not actually part of the constrained mode release group.

			Factory.Save();

			var ccrWorkflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "ccr-workflow",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-8),
				staffCode: config.CCR.GS_Code,
				lowEstMinutes: 3 * 8 * 60,
				sequence: 2,
				description: "ccr-workflow - ccr-task");
			config.Workflows.Add(ccrWorkflow);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.Section.Board)))
			{
				form.Show();

				var control = FindChannelHeaderControl(form, config.CCR);
				var viewModel = form.BoardViewModel.GetSections().Cast<BMBoardSectionViewModel>().Single();

				control.RefreshHeading_ForTest(config.Section, viewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("WHEN CCR's primary-channel is Zone 1 but inside CCR target area, THEN its channel-header status should not at risk", "Idle", control.ChannelStatus);

				var fadePanel = control.FindAll<ZFadePanel>().Single();
				var cell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading);

				AssertFadePanelDetails(
					"WHEN CCR's primary-channel is Zone 1 but inside CCR target area, THEN its channel-header fade should be blue/GoodColor (not at risk)",
					fadePanel,
					gradientAngle: 0f,
					gradientStartPercent: 0.47f,
					gradientSizePercent: 0.1f,
					fadeStartColor: BMConstants.GoodBoardColor.FadeTowardsWhite(fadeFactor: 2.5f),
					fadeEndColor: cell.BackColor.Value);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestChannelStatusText_WhenDisabledInRegistry()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, shouldUseExistingSystem: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var section = config.BufferSection;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Antidisestablishmenwhatever", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var control = FindChannelHeaderControl(form, resource);
				var viewModel = form.BoardViewModel.GetSections().Cast<BMBoardSectionViewModel>().Single();

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Zone 3, Working for 10 minutes", control.ChannelStatus);

				BMSRegistry.Instance.ShowIdleTimeOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Zone 3, Working for 10 minutes", control.ChannelStatus);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				Factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(15);

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Zone 3, Idle", control.ChannelStatus);

				BMSRegistry.Instance.ShowIdleTimeOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Zone 3, Idle for 15 minutes", control.ChannelStatus);
			}
		}

		[TestDate(2017, 11, 30)]
		public void TestStatusChannelTooltipText_NonCCRsContributingToCCRRisk_Up()
		{
			StatusChannelTooltipText_NonCCRsContributingToCCRRisk(FlowDirectionList.Codes.Up);
		}

		[TestDate(2017, 11, 30)]
		public void TestStatusChannelTooltipText_NonCCRsContributingToCCRRisk_Down()
		{
			StatusChannelTooltipText_NonCCRsContributingToCCRRisk(FlowDirectionList.Codes.Down);
		}

		[TestDate(2017, 11, 30)]
		public void TestStatusChannelTooltipText_NonCCRsContributingToCCRRisk_Left()
		{
			StatusChannelTooltipText_NonCCRsContributingToCCRRisk(FlowDirectionList.Codes.Left);
		}

		[TestDate(2017, 11, 30)]
		public void TestStatusChannelTooltipText_NonCCRsContributingToCCRRisk_Right()
		{
			StatusChannelTooltipText_NonCCRsContributingToCCRRisk(FlowDirectionList.Codes.Right);
		}

		void StatusChannelTooltipText_NonCCRsContributingToCCRRisk(string flowDirection)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "floccinaucinihilipilification", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, lowEstMinutes: 60, sequence: 10, description: "of a modern");
			var task2 = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, lowEstMinutes: 60, sequence: 20, description: "The very model");
			var task3 = BMSTestHelper.CreateTask(workflow, config.NonCCR2.GS_Code, lowEstMinutes: 60, sequence: 30, description: "major general");

			config.Section.SectionConfiguration.FlowDirection = flowDirection;

			Factory.Save();

			AssertEquals("Pre-Constraint", config.PreConstraintBuffer.FC_Name);
			AssertEquals("Post-Constraint", config.PostConstraintBuffer.FC_Name);
			AssertEquals(80, config.Section.SectionConfiguration.FadeBackgroundAtPercentage);
			AssertEquals(13, config.Section.SectionConfiguration.CellsPerSubsection);

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(config.Board))
			{
				AssertTooltipText(config, workflow, form, 0, "Zone 3, Idle", "Zone 3, Idle");
				AssertTooltipText(config, workflow, form, 1, "Zone 3, Idle", "Zone 3, Idle");
				AssertTooltipText(config, workflow, form, 2, "Zone 3, Idle", "Zone 3, Idle");
				AssertTooltipText(config, workflow, form, 3, "Zone 3, Idle", "Zone 3, Idle");
				AssertTooltipText(config, workflow, form, 4, "Zone 2, Idle", "Zone 2, Idle");

				AssertTooltipText(config, workflow, form, 5,
					string.Format(ToolTipFullText, 1, "Pre-Constraint"),
					"Zone 2, Idle");

				AssertTooltipText(config, workflow, form, 6,
					string.Format(ToolTipFullText, 1, "Pre-Constraint"),
					"Zone 2, Idle");

				AssertTooltipText(config, workflow, form, 7,
					string.Format(ToolTipFullText, 1, "Pre-Constraint"),
					"Zone 2, Idle");

				AssertTooltipText(config, workflow, form, 8,
					string.Format(ToolTipFullText, 0, "Pre-Constraint"),
					"Zone 1, Idle");

				AssertTooltipText(config, workflow, form, 9,
					string.Format(ToolTipFullText, 0, "Pre-Constraint"),
					"Zone 1, Idle");

				AssertTooltipText(config, workflow, form, 10,
					string.Format(ToolTipFullText, 0, "Pre-Constraint"),
					"Zone 1, Idle");

				AssertTooltipText(config, workflow, form, 11,
					string.Format(ToolTipFullText, 0, "Pre-Constraint"),
					string.Format(ToolTipFullText, 1, "Post-Constraint"));

				AssertTooltipText(config, workflow, form, 12,
					string.Format(ToolTipFullText, 0, "Pre-Constraint"),
					string.Format(ToolTipFullText, 0, "Post-Constraint"));
			}
		}

		[TestDate(2017, 11, 30)]
		public void TestStatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation_Up()
		{
			StatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation(FlowDirectionList.Codes.Up);
		}

		[TestDate(2017, 11, 30)]
		public void TestStatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation_Down()
		{
			StatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation(FlowDirectionList.Codes.Down);
		}

		[TestDate(2017, 11, 30)]
		public void TestStatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation_Left()
		{
			StatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation(FlowDirectionList.Codes.Left);
		}

		[TestDate(2017, 11, 30)]
		public void TestStatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation_Right()
		{
			StatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation(FlowDirectionList.Codes.Right);
		}

		void StatusChannelTooltipText_NonCCRsContributingToCCRRisk_SwitchingComponentVisualisation(string flowDirection)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "floccinaucinihilipilification", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, lowEstMinutes: 60, sequence: 10, description: "of a modern");
			var task2 = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, lowEstMinutes: 60, sequence: 20, description: "The very model");
			var task3 = BMSTestHelper.CreateTask(workflow, config.NonCCR2.GS_Code, lowEstMinutes: 60, sequence: 30, description: "major general");
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-12); // Go to zone 0, go directly to zone 0, do not pass GO, do not collect $200.

			config.Section.SectionConfiguration.FlowDirection = flowDirection;

			Factory.Save();

			AssertEquals("Pre-Constraint", config.PreConstraintBuffer.FC_Name);
			AssertEquals("Post-Constraint", config.PostConstraintBuffer.FC_Name);
			AssertEquals(80, config.Section.SectionConfiguration.FadeBackgroundAtPercentage);

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(config.Board))
			{
				var componentControl = form.FindAll<BMComponentControl>().Single();

				var controlNonCCR1 = FindChannelHeaderControl(form, config.NonCCR1);
				var controlCCR = FindChannelHeaderControl(form, config.CCR);
				var controlNonCCR2 = FindChannelHeaderControl(form, config.NonCCR2);

				var nonCCR1OriginalWidth = controlNonCCR1.Width;
				var nonCCR2OriginalWidth = controlNonCCR2.Width;
				var ccrOriginalWidth = controlCCR.Width;

				var taskCards = GetTaskCards(componentControl, task1.PK, task2.PK, task3.PK);

				CombineAssertions("default view", () =>
				{
					AssertEquals(string.Format(ToolTipFullText, 0, "Pre-Constraint"),
						ToolTipService.GetToolTip(controlNonCCR1.ChannelStatusLabel));
					AssertEquals(string.Format(ToolTipFullText, 0, "Post-Constraint"),
						ToolTipService.GetToolTip(controlNonCCR2.ChannelStatusLabel));
					AssertEquals("WHEN n-th % is outside target zone (higher in buffer penetration) THEN should show outside target zone status", "Outside target zone, Idle", ToolTipService.GetToolTip(controlCCR.ChannelStatusLabel));

					AssertNotNull("All cards should be visible", taskCards[0]);
					AssertNotNull("All cards should be visible", taskCards[1]);
					AssertNotNull("All cards should be visible", taskCards[2]);
				});

				AssertEquals(false, componentControl.ViewModel.FilterManager.IsApplied(typeof(ComponentViewFilter)));

				AssertNotNull(controlNonCCR1.StatusLabelClickHandler);
				controlNonCCR1.StatusLabelClickHandler.Invoke(controlNonCCR1.StatusLabelClickHandler, null);
				Application.DoEvents();

				AssertEquals(true, componentControl.ViewModel.FilterManager.IsApplied(typeof(ComponentViewFilter)));
				AssertEquals("Showing items in component: Pre-Constraint", componentControl.ViewModel.FilterManager.AppliedFilters.OfType<ComponentViewFilter>().FirstOrDefault().FilterName);

				var menuItem = componentControl.ContextMenuStrip.Items.OfType<MultiChildCheckedFilterMenuItem<BMComponent>>().FirstOrDefault();
				menuItem.DropDownOpeningHandler.Invoke(menuItem.DropDownOpeningHandler, null);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Pre-Constraint", true);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Post-Constraint", false);

				taskCards = GetTaskCards(componentControl, task1.PK, task2.PK, task3.PK);

				CombineAssertions("pre constraint view", () =>
				{
					AssertEquals("80 percent of work in this channel falls within zone 0 of the Pre-Constraint buffer.",
						ToolTipService.GetToolTip(controlNonCCR1.ChannelStatusLabel));
					AssertEquals(string.Format(ToolTipFullText, 0, "Post-Constraint"),
						ToolTipService.GetToolTip(controlNonCCR2.ChannelStatusLabel));
					AssertEquals("WHEN n-th % is outside target zone (higher in buffer penetration) THEN should show outside target zone status", "Outside target zone, Idle", ToolTipService.GetToolTip(controlCCR.ChannelStatusLabel));

					AssertEquals("Non CCR1's channel width should remain the same after clicking on the label", nonCCR1OriginalWidth, controlNonCCR1.Width);
					AssertEquals("Non CCR2's channel width should remain the same after clicking on the label", nonCCR2OriginalWidth, controlNonCCR2.Width);
					AssertEquals("CCR's channel width should remain the same after clicking on the label", ccrOriginalWidth, controlCCR.Width);

					AssertNotNull("Only pre-req card should be visible", taskCards[0]);
					AssertNull("Only pre-req card should be visible", taskCards[1]);
					AssertNull("Only pre-req card should be visible", taskCards[2]);
				});

				AssertNotNull(controlNonCCR2.StatusLabelClickHandler);
				controlNonCCR2.StatusLabelClickHandler.Invoke(controlNonCCR2.StatusLabelClickHandler, null);
				Application.DoEvents();

				AssertEquals(true, componentControl.ViewModel.FilterManager.IsApplied(typeof(ComponentViewFilter)));
				AssertEquals("Showing items in component: Post-Constraint", componentControl.ViewModel.FilterManager.AppliedFilters.OfType<ComponentViewFilter>().FirstOrDefault().FilterName);
				menuItem = componentControl.ContextMenuStrip.Items.OfType<MultiChildCheckedFilterMenuItem<BMComponent>>().FirstOrDefault();
				menuItem.DropDownOpeningHandler.Invoke(menuItem.DropDownOpeningHandler, null);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Pre-Constraint", false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Post-Constraint", true);

				taskCards = GetTaskCards(componentControl, task1.PK, task2.PK, task3.PK);

				CombineAssertions("post constraint view", () =>
				{
					AssertEquals(string.Format(ToolTipFullText, 0, "Pre-Constraint"),
						ToolTipService.GetToolTip(controlNonCCR1.ChannelStatusLabel));
					AssertEquals("80 percent of work in this channel falls within zone 0 of the Post-Constraint buffer.",
						ToolTipService.GetToolTip(controlNonCCR2.ChannelStatusLabel));
					AssertEquals("WHEN n-th % is outside target zone (higher in buffer penetration) THEN should show outside target zone status", "Outside target zone, Idle", ToolTipService.GetToolTip(controlCCR.ChannelStatusLabel));

					AssertEquals("Non CCR1's channel width should remain the same after clicking on the label", nonCCR1OriginalWidth, controlNonCCR1.Width);
					AssertEquals("Non CCR2's channel width should remain the same after clicking on the label", nonCCR2OriginalWidth, controlNonCCR2.Width);
					AssertEquals("CCR's channel width should remain the same after clicking on the label", ccrOriginalWidth, controlCCR.Width);

					AssertNull("Only post-req card should be visible", taskCards[0]);
					AssertNull("Only post-req card should be visible", taskCards[1]);
					AssertNotNull("Only post-req card should be visible", taskCards[2]);
				});
			}
		}

		void AssertTooltipText(ComplexConstrainedSchematicTestConfig config, ProcessHeader workflow, VisualBoardForm form, int agingDays, params string[] labelText)
		{
			var controlNonCCR1 = FindChannelHeaderControl(form, config.NonCCR1);
			var controlCCR = FindChannelHeaderControl(form, config.CCR);
			var controlNonCCR2 = FindChannelHeaderControl(form, config.NonCCR2);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-agingDays);
			Factory.Save();
			form.RefreshNow_ForTest();

			CombineAssertions($"aging days: {agingDays}", () =>
			{
				AssertEquals("Tooltip for Non CCR 1 should differ", labelText[0], ToolTipService.GetToolTip(controlNonCCR1.ChannelStatusLabel));
				AssertEquals("Tooltip for Non CCR 2 should differ", labelText[1], ToolTipService.GetToolTip(controlNonCCR2.ChannelStatusLabel));

				if (agingDays < 9)
				{
					AssertEquals("Not enough work, Idle", ToolTipService.GetToolTip(controlCCR.ChannelStatusLabel));
				}
				else
				{
					AssertEquals("WHEN aging >= 9 THEN the n-th % is outside target zone (higher in buffer penetration) THEN should show outside target zone status", "Outside target zone, Idle", ToolTipService.GetToolTip(controlCCR.ChannelStatusLabel));
				}
			});
		}

		List<TaskCardControl> GetTaskCards(BMComponentControl componentControl, params ZGuid[] taskPKs)
		{
			var taskCards = new List<TaskCardControl>();
			var cardControls = BMSGUITestCase.FindTaskCardControls(componentControl);
			taskCards.Add(cardControls.FirstOrDefault(c => c.CardContent.Identifier == taskPKs[0]));
			taskCards.Add(cardControls.FirstOrDefault(c => c.CardContent.Identifier == taskPKs[1]));
			taskCards.Add(cardControls.FirstOrDefault(c => c.CardContent.Identifier == taskPKs[2]));
			return taskCards;
		}

		const string ToolTipFullText = "80 percent of work in this channel falls within zone {0} of the {1} buffer. Use the Component Filter context menu to switch to this buffer, or click this message.";

		#endregion

		#region ChannelHeaderImage

		public void TestChannelHeaderImage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.ProfileImage = new Bitmap(20, 20);

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				AssertNotNull(control.ThumbnailPhotoPictureBox.Image);
			}
		}

		#endregion

		#region Back/Fore Colors

		public void TestBackAndForeColors_WhenExpandingChannel_TagChannel()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory, shouldUseExistingSystem: true);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);

			using (var control = ChannelHeaderControlTestHelpers.GetControl(config.RedTag, system, config.BufferSection, viewModel).WrapWithShownForm())
			{
				var channelHeaderControl = control.Controls.Cast<ChannelHeaderControl>().Single();

				channelHeaderControl.RefreshHeading_ForTest(config.BufferSection, viewModel.ComponentGrid.CardAllocationMap);

				AssertEquals(Color.Transparent, channelHeaderControl.BackColor);
				AssertEquals(SystemColors.ControlText, channelHeaderControl.ForeColor);

				channelHeaderControl.OnChannelExpansionToggled(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

				AssertEquals(Color.White, channelHeaderControl.BackColor);
				AssertEquals(SystemColors.ControlText, channelHeaderControl.ForeColor);
			}
		}

		#endregion

		#region ShowChannelForm

		public void TestShowChannelForm_Resource()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FRO";
			staff.GS_FullName = "Frodo Baggins";

			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(staff, system, section, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				control.ShowChannelForm();
				control.ShowChannelForm();

				using (var staffForm = OpenedFormCache.GetInstance().GetForm(staff.PK.ToGuid(), ModuleIDs.GlbStaff.Name))
				{
					AssertNotNull(staffForm);
				}
			}
		}

		public void TestShowChannelForm_Group()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "FRO";

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(group, system))
			{
				form.Controls.Add(control);
				form.Show();

				control.ShowChannelForm();
				control.ShowChannelForm();

				using (var groupForm = OpenedFormCache.GetInstance().GetForm(group.PK.ToGuid(), ModuleIDs.GlbGroup.Name))
				{
					AssertNotNull(groupForm);
				}
			}
		}

		public void TestShowChannelForm_Capability()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "FRO";

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(capability, system))
			{
				form.Controls.Add(control);
				form.Show();

				control.ShowChannelForm();
				control.ShowChannelForm();

				using (var capabilityForm = OpenedFormCache.GetInstance().GetForm(capability.PK.ToGuid(), ModuleIDs.GlbCapability.Name))
				{
					AssertNotNull(capabilityForm);
				}
			}
		}

		public void TestShowChannelForm_Tag()
		{
			var tag = Factory.NewWithValidTestData<TagMagnitude>();
			tag.TGM_Code = "FRO";

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(tag, system))
			{
				form.Controls.Add(control);
				form.Show();

				control.ShowChannelForm();
				control.ShowChannelForm();

				using (var tagForm = OpenedFormCache.GetInstance().GetForm(tag.PK.ToGuid(), ModuleIDs.BMTagMagnitude.Name))
				{
					AssertNotNull(tagForm);
				}
			}
		}

		public void TestShowChannelForm_WorkQueue()
		{
			var queue = Factory.NewWithValidTestData<WorkQueue>();
			queue.TGM_Code = "FRO";

			Factory.Save();

			using (var form = new ZForm())
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(queue, system))
			{
				form.Controls.Add(control);
				form.Show();

				control.ShowChannelForm();
				control.ShowChannelForm();

				using (var queueForm = OpenedFormCache.GetInstance().GetForm(queue.PK.ToGuid(), ModuleIDs.WorkQueues.Name))
				{
					AssertNotNull(queueForm);
				}
			}
		}

		public static void AssertFadePanelDetails(string message, ZFadePanel fadePanel, float gradientAngle, float gradientStartPercent, float gradientSizePercent, Color fadeStartColor, Color fadeEndColor)
		{
			fadePanel.ForceCompleteAnimation();

			CombineAssertions(message, () =>
			{
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				AssertEquals("GradientAngle", gradientAngle, fadePanel.GradientAngle);

				var actualGradientStartPercent = fadePanel.GradientStartPercent != null ? fadePanel.GradientStartPercent.Value : 0f;
				AssertCloseEnough("GradientStartPercent", Convert.ToInt32(Math.Round(gradientStartPercent, 2) * 100), Convert.ToInt32(Math.Round(actualGradientStartPercent, 2) * 100)); // Why you no take in floats for, Utilities.Round??

				var actualGradientSizePercent = fadePanel.GradientSizePercent != null ? fadePanel.GradientSizePercent.Value : 0f;
				AssertEquals("GradientSizePercent", gradientSizePercent, actualGradientSizePercent);
				AssertEquals("FadeStartColor", fadeStartColor, fadePanel.FadeStartColor);
				AssertEquals("FadeEndColor", fadeEndColor, fadePanel.FadeEndColor);
			});
		}

		#endregion

		#region ShowChannelCapacity

		[TestDate(2013, 4, 16)]
		public void TestShowCapacity_UnchanneledResourceChannel_Buffer()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var buffer = CreateBuffer(system);

			var section = CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task1 = CreateTask(workflow, string.Empty, 3 * 60); // 4.5 hours std est

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var cell = viewModel.ComponentGrid.Cells.First(c => c.ContentType == CellContentType.ChannelHeading);

			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });

			using (var control = (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, null, viewModel, () => new ChannelHeaderControl(cell, viewModel)))
			{
				AssertNoExceptionThrown(control.ShowChannelCapacity);
				AssertEquals("Un-channeled", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals("Capacity balloon text", "Total Assigned: 4.5 hours", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCapacity_ResourceChannel_Bucket()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "Frodo Baggins";
			resource.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			resource.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			system.FS_Description = "WiseTech Global Development";
			var buffer = BMSTestHelper.CreateBuffer(system, "Dis Buffer");
			var bucket = BMSTestHelper.CreateBucket(system, "Dis Bucket");

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			task1.P9_EstDuration = new ZDateTime(2013, 1, 1, 3, 0, 0); // 4.5 hours std est

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;
			task2.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			task2.P9_EstDuration = new ZDateTime(2013, 1, 1, 1, 0, 0); // 1.5 hours std est

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });

			using (var control = ChannelHeaderControlTestHelpers.GetControl(resource, system, section, viewModel))
			{
				control.ShowChannelCapacity();
				AssertEquals("Frodo Baggins", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals("Capacity balloon text",
					@"Total Assigned: 4.5 hours
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCapacity_ForDisposedControl()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var bucket = system.Components.AddNew();
			bucket.FC_Name = "Dis Bucket";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			task1.P9_EstDuration = new ZDateTime(2013, 1, 1, 3, 0, 0); // 4.5 hours std est

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });

			using (var control = ChannelHeaderControlTestHelpers.GetControl(resource, system, section, viewModel))
			{
				control.Dispose();
				control.ShowChannelCapacity();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		[TestDate(2013, 4, 16)]
		public void TestShowCapacity_ShouldNotRecalculateChannelFade()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, buffer.FC_BufferTimespanInMinutes, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });

			using (DisableAsyncBehaviour())
			using (var control = ChannelHeaderControlTestHelpers.GetControl(resource, system, section, viewModel).WrapWithShownForm())
			{
				var channelHeaderControl = control.Controls.Cast<ChannelHeaderControl>().Single();
				channelHeaderControl.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);

				var fadePanel = control.FindAll<ZFadePanel>().Single();
				fadePanel.ForceCompleteAnimation();
				AssertEquals(1f, fadePanel.GradientStartPercent.Value);
				AssertEquals("Overloaded, Working", channelHeaderControl.ChannelStatus);
				AssertEquals(channelHeaderControl.ChannelStatus, ToolTipService.GetToolTip(channelHeaderControl.ChannelStatusLabel));

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				control.ShowChannelCapacity();
				AssertEquals("Should not have recalculated channel fade when showing capacity bubble - that's a big slowdown", 1f, fadePanel.GradientStartPercent.Value);
				AssertEquals("Overloaded, Working", channelHeaderControl.ChannelStatus);
				AssertEquals(channelHeaderControl.ChannelStatus, ToolTipService.GetToolTip(channelHeaderControl.ChannelStatusLabel));
			}
		}

		#endregion

		#region High Risk Fade

		[TestDate(2014, 1, 28)]
		public void TestChannelHeaderFade_HighRisk()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			var buffer = section.Component;
			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC2", "Non CCR Resource 1");

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			group.Staff.AddRange(resourceCCR1);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK, overrideChannels: true);
			resourceCCR1.DesignateAsCCR(buffer);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, config.Buffer, releaseDateTime: ZDateTime.Now, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 15, description: "Workflow 1");

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, config.Buffer, releaseDateTime: ZDateTime.Now, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 1500, sequence: 100, description: "Workflow 2");
			var task1 = CreateTask(workflow2, resourceNonCCR1.GS_Code, lowEstMinutes: 1500, sequence: 1, description: "Workflow 2 - task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { workflow1.Parent.WorkflowItems[0], workflow2.Parent.WorkflowItems[0], task1 });
			var cell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.Channel.ClearCacheAndReload();
				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertFadePanelDetails("High-risk channel should have red colour", fadePanel, 0f, 0.34f, 0.1f, BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), cell.BackColor.Value);
			}
		}

		[TestDate(2014, 1, 6)]
		public void TestCCRChannelHeaderFade_OnPrimaryBufferZone1And0_NonHighRiskChannel()
		{
			TestCCRChannelHeaderFade_OnPrimaryBufferZone1And0(message: "Non-high-risk CCR-channel that fades on zone 1 or 0 primary-buffer: no red heading", isHighRiskCCRQueueTooShort: false);
		}

		[TestDate(2014, 1, 6)]
		public void TestCCRChannelHeaderFade_OnPrimaryBufferZone1And0_HighRiskChannel()
		{
			TestCCRChannelHeaderFade_OnPrimaryBufferZone1And0(message: "High-risk CCR-channel that fades on zone 1 or 0 primary-buffer's zone 1 and 0: red heading", isHighRiskCCRQueueTooShort: true);
		}

		#endregion

		#region Implementation

		void TestCCRChannelHeaderFade_OnPrimaryBufferZone1And0(string message, bool isHighRiskCCRQueueTooShort)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true, makeResourcesPartOfReleaseGroup: false); // Because these tests are highly reliant on the incorrect behaviour that CCR/non-CCR resources are not actually part of the constrained mode release group.
			var section = config.Section;

			var ccrWorkflow = config.Workflows[0];
			ccrWorkflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-5);
			var taskDurationInMinutes = isHighRiskCCRQueueTooShort ? 15 : 3 * 8 * 60;
			ccrWorkflow.Parent.WorkflowItems[0].P9_EstDuration = new ZInt(taskDurationInMinutes).GetDateTimeFromMinutes();

			Factory.Save();
			var viewModel = config.SectionViewModel;

			var cell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();
				control.Channel.ClearCacheAndReload();

				AssertEquals(message, isHighRiskCCRQueueTooShort, control.Channel.IsHighRisk);

				if (isHighRiskCCRQueueTooShort)
				{
					AssertFadePanelDetails(message, fadePanel, gradientAngle: 0f, gradientStartPercent: 0f, gradientSizePercent: 0.1f, fadeStartColor: BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), fadeEndColor: cell.BackColor.Value);
				}
				else
				{
					AssertFadePanelDetails(message, fadePanel, gradientAngle: 0f, gradientStartPercent: 0.46f, gradientSizePercent: 0.1f, fadeStartColor: BMConstants.GoodBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), fadeEndColor: cell.BackColor ?? control.BackColor);
				}
			}
		}

		#endregion

		#region High Risk Zone

		[TestDate(2014, 1, 6)]
		public void TestChannelHeaderFade_HighRiskZone()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Workflows[0].FH_ReleaseDateTime = ZDateTime.Now.AddYears(-1);
			Factory.Save();
			var viewModel = config.SectionViewModel;

			var section = config.Section;
			var cell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.Channel.ClearCacheAndReload();
				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);

				AssertFadePanelDetails("High-risk-zone channel should have red colour", fadePanel, gradientAngle: 0f, gradientStartPercent: 0f, gradientSizePercent: 0.1f, fadeStartColor: BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), fadeEndColor: cell.BackColor.Value);
			}
		}

		#endregion

		#region Header Alert Click Option

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnAlert_CursorShouldChangeToHand()
		{
			var helper = new AlertStatusTestHelper(Factory, system);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				var headerControl = helper.GetHeaderControl(form);

				var picture = headerControl.FindAll<ChannelStatusPictureBox>().Single();
				AssertEquals(Cursors.Hand, picture.Cursor);
			}
		}

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnAlert_HeaderShouldNotExpandWhenPictureIsClicked()
		{
			var helper = new AlertStatusTestHelper(Factory, system);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				var headerControl = helper.GetHeaderControl(form);

				var picture = headerControl.FindAll<ChannelStatusPictureBox>().Single();

				var initialColumnWidth = headerControl.Width;
				picture.Select();
				AssertEquals("Column should not expand when the alert picture is clicked", initialColumnWidth, headerControl.Width);
			}
		}

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnAlert_ToolTip()
		{
			var visualBoard = new AlertStatusTestHelper(Factory, system).VisualBoard;

			AssertStartsWith("Tool Tip should start with 'Alert'.", "ALERT", visualBoard.StatusImage.ImageTooltip);
			AssertEndsWith("Tool Tip should end with 'Click to open the task's job.'.", "Click to open the task's job.", visualBoard.StatusImage.ImageTooltip);
		}

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnAlert_ShouldOpenJobTask()
		{
			var helper = new AlertStatusTestHelper(Factory, system);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				var headerControl = helper.GetHeaderControl(form);

				AssertNotNull("StatusImageClickHandler should be initialised", headerControl.StatusImageClickHandler);
				headerControl.StatusImageClickHandler.Invoke(headerControl.StatusImageClickHandler, null);
				Application.DoEvents();

				var jobForm = GetFirstFormThatStartsWith("Edit Organization");
				AssertNotNull("Task form should have opened", jobForm);

				var taskControl = jobForm.FindAll<TaskDetailsUserControl>().Single();
				AssertEquals(helper.Task2.PK, taskControl.TasksGrid.GetCurrentPK());

				jobForm.Close();

				helper.ChangeTask1Status(ProcessTaskStatusCodeList.Codes.Working);
				Factory.Save();

				form.RefreshBoard();
				Application.DoEvents();

				AssertNotEquals("Alert", headerControl.ChannelStatus);

				AssertNull("StatusImageClickHandler should be null because the channel is not in alert mode", headerControl.StatusImageClickHandler);
			}
		}

		Form GetFirstFormThatStartsWith(string formText)
		{
			foreach (Form form in Application.OpenForms)
			{
				if (form.Text.StartsWith(formText))
				{
					return form;
				}
			}
			return null;
		}

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnAlert_DeletedTask_ShouldShowErrorMessage()
		{
			var helper = new AlertStatusTestHelper(Factory, system);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				helper.Task2.Delete();
				Factory.Save();

				var headerControl = helper.GetHeaderControl(form);
				headerControl.StatusImageClickHandler.Invoke(headerControl.StatusImageClickHandler, null);

				AssertEquals(@"Task  (XVBQP68SIYXQ), task2
In component: bucket, has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestTaskFormLinkOnAlert_ForStandaloneTask_ShouldOpenTaskForm()
		{
			var helper = new AlertStatusTestHelper(Factory, system);
			var standaloneTask = BMSTestHelper.CreateStandaloneTask(Factory, helper.Resource.GS_Code, ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				var headerControl = helper.GetHeaderControl(form);
				headerControl.StatusImageClickHandler.Invoke(headerControl.StatusImageClickHandler, null);

				using (var taskForm = GetFirstFormThatStartsWith("Edit Task"))
				{
					AssertType<TaskManagementForm>(taskForm);
				}
			}
		}

		[TestDate(2016, 7, 29)]
		public void TestAlertStatusTestHelper_ShouldCreateAlertStatus()
		{
			var helper = new AlertStatusTestHelper(Factory, system);
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();
				AssertEquals("Zone 3, " + nameof(RoadRunnerStatus.Alert), helper.GetHeaderControl(form).ChannelStatus);
			}
		}

		#endregion

		#region Header WorkingOutOfBufferWithNoPendingTasks Click Option

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnWorkingOutOfBufferWithNoPendingTasks_CursorShouldChangeToHand()
		{
			var helper = new AlertStatusTestHelper(Factory, system);
			helper.Task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				var headerControl = helper.GetHeaderControl(form);

				var picture = headerControl.FindAll<ChannelStatusPictureBox>().Single();
				AssertEquals(Cursors.Hand, picture.Cursor);
			}
		}

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnWorkingOutOfBufferWithNoPendingTasks_HeaderShouldNotExpandWhenPictureIsClicked()
		{
			var helper = new AlertStatusTestHelper(Factory, system);
			helper.Task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				var headerControl = helper.GetHeaderControl(form);

				var picture = headerControl.FindAll<ChannelStatusPictureBox>().Single();

				var initialColumnWidth = headerControl.Width;
				picture.Select();
				AssertEquals("Column should not expand when the road runner picture is clicked", initialColumnWidth, headerControl.Width);
			}
		}

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnWorkingOutOfBufferWithNoPendingTasks_ShouldOpenJobTask()
		{
			var helper = new AlertStatusTestHelper(Factory, system);
			helper.Task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				var headerControl = helper.GetHeaderControl(form);
				headerControl.StatusImageClickHandler.Invoke(headerControl.StatusImageClickHandler, null);
				Application.DoEvents();

				var jobForm = GetFirstFormThatStartsWith("Edit Organization");
				AssertNotNull("Task form should have opened", jobForm);

				var taskControl = jobForm.FindAll<TaskDetailsUserControl>().Single();
				AssertEquals(helper.Task2.PK, taskControl.TasksGrid.GetCurrentPK());

				jobForm.Close();

				helper.ChangeTask1Status(ProcessTaskStatusCodeList.Codes.Working);
				Factory.Save();

				form.RefreshBoard();
				Application.DoEvents();

				AssertNull("StatusImageClickHandler should be null because the resource is working on the board", headerControl.StatusImageClickHandler);
			}
		}

		[TestDate(2016, 7, 29)]
		public void TestTaskFormLinkOnWorkingOutOfBufferWithNoPendingTasks_DeletedTask_ShouldShowErrorMessage()
		{
			var helper = new AlertStatusTestHelper(Factory, system);
			helper.Task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				helper.Task2.Delete();
				Factory.Save();

				var headerControl = helper.GetHeaderControl(form);
				headerControl.StatusImageClickHandler.Invoke(headerControl.StatusImageClickHandler, null);

				AssertEquals(@"Task  (XVBQP68SIYXQ), task2
In component: bucket, has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Performance

		public void TestExpandChannel_ShouldNotHitDatabase()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, shouldUseExistingSystem: true);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource3.PK);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Your words", config.Buffer);
			BMSTestHelper.CreateTask(workflow, resource1.GS_Code);
			BMSTestHelper.CreateTask(workflow, resource2.GS_Code);
			BMSTestHelper.CreateTask(workflow, resource3.GS_Code);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();
				var tickets = BMSGUITestCase.FindTaskCardControls(control);

				AssertEquals(3, tickets.Length);

				using (Db.Connection.TrackExecutedCommands())
				{
					for (var i = 0; i < 3; i++)
					{
						control.ExpandNextHeader();
						Application.DoEvents();
					}

					AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), Db.Connection.ExecutedCommands);
				}

				AssertEquals(false, tickets[0].IsDisposed);
				AssertEquals(false, tickets[1].IsDisposed);
				AssertEquals(false, tickets[2].IsDisposed);
			}
		}

		#endregion

		#region Turned off in registry

		[TestDate(2022, 5, 9)]
		public void TestChannelHeaderFade_WhenDisableCapacityCalculationsEnabled_ShouldHaveNoFade()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			Factory.Save();

			var viewModel = config.SectionViewModel;
			var section = config.Section;
			var cell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.Channel.ClearCacheAndReload();
				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);

				control.Channel.ClearCacheAndReload();
				AssertEquals("Idle", control.ChannelStatus);
				AssertEquals(Color.Transparent, control.BackColor);
				AssertEquals(SystemColors.ControlText, control.ForeColor);
			}
		}

		[TestDate(2022, 5, 9)]
		public void TestChannelHeaderFade_WhenDisableCapacityCalculationsEnabled_ShouldHaveNoFade_StaffWorkingOvertime()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var workflow = config.Workflows[0];
			var task = BMSTestHelper.CreateTask(workflow, config.Staffs.First().GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			TestDateAttribute.AddHours(1);
			Factory.Save();

			var viewModel = config.SectionViewModel;
			var section = config.Section;
			var cell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading);

			using (DisableAsyncBehaviour())
			using (var control = new ChannelHeaderControl(cell, viewModel).WrapWithShownForm())
			{
				var fadePanel = control.FindAll<ZFadePanel>().Single();

				control.Channel.ClearCacheAndReload();
				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);

				control.Channel.ClearCacheAndReload();
				AssertEquals("Working for 1 hour", control.ChannelStatus);
				AssertEquals("We still want to show the yellow colour when the staff is working over the high estimate on the task, since this has nothing to do with capacity.", BMConstants.CautionBoardColor, control.BackColor);
				AssertEquals(SystemColors.ControlText, control.ForeColor);
			}
		}

		public void TestShowCapacity_WhenDisableCapacityCalculationsEnabled_ShouldNotShowAnyValues_AndShouldShowHelpfulMessageInstead()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, shouldUseExistingSystem: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 10);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			viewModel.ComponentGrid.AllocateTasks_ForTest(config.BufferSection, viewModel, new[] { task });

			using (var control = ChannelHeaderControlTestHelpers.GetControl(resource, system, config.BufferSection, viewModel))
			{
				control.ShowChannelCapacity();
				AssertEquals("Frodo Baggins", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals(@"Capacity unavailable because the [Workflow Manager -> Buffer Management -> Release Gate -> Disable Capacity Calculations] registry item is enabled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region ShowChannelCapacity

		[TestDate(2013, 4, 16)]
		public virtual void TestShowCapacityMenuItem()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, config.CCR.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, 60 * 3);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Bucket);
			var task2 = BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			viewModel.ComponentGrid.AllocateTasks_ForTest(config.BufferSection, viewModel, new[] { task1, task2 });

			using (var control = (ChannelHeaderControl)ChannelHeaderControlTestHelpers.GetControl(config.CCR, system, config.BufferSection, viewModel).Controls[0])
			{
				((LazyContextMenuStrip)control.ContextMenuStrip).AddItems_ForTest(control);
				var menuItem = (ZToolStripMenuItem)control.ContextMenuStrip.Items[0];
				AssertEquals("Show Capacity Details", menuItem.Text);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("CCR", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals(@"Total Capacity: 64 hours

Allocated Capacity in buffer: 4.5 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 4.5 hours

Available Capacity: 59.5 hours

Calculated at: 16-Apr-2013 10:00:00", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2013, 4, 16)]
		public void TestShowCapacity_ResourceChannel_Buffer()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			resource1.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			resource1.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			system.FS_Name = "WTGDEV";
			system.FS_Description = "WiseTech Global Development";
			var buffer = BMSTestHelper.CreateBuffer(system, "Dis Buffer");
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task1.P9_EstDuration = new ZDateTime(2013, 1, 1, 3, 0, 0); // 4.5 hours std est

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;
			task2.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_EstDuration = new ZDateTime(2013, 1, 1, 1, 0, 0); // 1.5 hours std est

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2 });

			using (var control = ChannelHeaderControlTestHelpers.GetControl(resource1, system, section, viewModel))
			{
				control.ShowChannelCapacity();
				AssertEquals("Frodo Baggins", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals(@"Total Capacity: 48 hours

Allocated Capacity in Dis Buffer: 4.5 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 4.5 hours

Available Capacity: 43.5 hours

Calculated at: 16-Apr-2013 10:00:00", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Channel Load Scale

		[TestDate(2014, 1, 28)]
		public void TestChannelLoadScale_VerticalOrientation()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			section.BackgroundColor = ZString.Empty;
			section.ForegroundColor = ZString.Empty;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, resource.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * buffer.FC_BufferLoadLimitPercent / 100.0 / 4.0)); // 1/4 of relevant buffer timespan 

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new MockAsyncStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();

				AssertNoOverlaidLabel(form);
				AssertFadePanelAndTaskCardDetails("", form, 0f, 0.32f, 0.1f, BMConstants.GoodBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control, Tuple.Create(task1, 3));

				workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-14);
				Factory.Save();

				form.RefreshBoardAndWait();
				AssertNoOverlaidLabel(form);
				AssertFadePanelAndTaskCardDetails("Should now be high-risk - in zone 0", form, 0f, 0.32f, 0.1f, BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control, Tuple.Create(task1, 1));

				var task2 = BMSTestHelper.CreateTask(workflow, resource.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * buffer.FC_BufferLoadLimitPercent / 100.0 / 4.0)); // 1/4 of relevant buffer timespan

				Factory.Save();
				form.AwaitAll(); // Allow data refresh bus to run.
				BufferCapacityCache.Clear();

				form.RefreshBoardAndWait();
				AssertNoOverlaidLabel(form);
				AssertFadePanelAndTaskCardDetails("Still in zone 0, also allocation scale has increased", form, 0f, 0.70f, 0.1f, BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control, Tuple.Create(task1, 0), Tuple.Create(task2, 0));
			}
		}

		[TestDate(2014, 1, 28)]
		public void TestChannelLoadScale_HorizontalOrientation()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			section.BackgroundColor = ZString.Empty;
			section.ForegroundColor = ZString.Empty;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, resource.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * buffer.FC_BufferLoadLimitPercent / 100.0 / 4.0)); // 1/4 of relevant buffer timespan

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new MockAsyncStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();

				AssertNoOverlaidLabel(form);
				AssertFadePanelAndTaskCardDetails("", form, 270f, 0.32f, 0.1f, BMConstants.GoodBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control, Tuple.Create(task1, 3));

				workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-14);
				Factory.Save();

				form.RefreshBoardAndWait();
				AssertNoOverlaidLabel(form);
				AssertFadePanelAndTaskCardDetails("Should now be high-risk - in zone 0", form, 270f, 0.32f, 0.1f, BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control, Tuple.Create(task1, 1));

				var task2 = BMSTestHelper.CreateTask(workflow, resource.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * buffer.FC_BufferLoadLimitPercent / 100.0 / 4.0)); // 1/4 of relevant buffer timespan

				Factory.Save();
				form.AwaitAll(); // Allow data refresh bus to run.
				BufferCapacityCache.Clear();

				form.RefreshBoardAndWait();
				AssertNoOverlaidLabel(form);
				AssertFadePanelAndTaskCardDetails("Still in zone 0, also allocation scale has increased", form, 270f, 0.70f, 0.1f, BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f), SystemColors.Control, Tuple.Create(task1, 0), Tuple.Create(task2, 0));
			}
		}

		static void AssertFadePanelAndTaskCardDetails(string message, VisualBoardForm form, float gradientAngle, float gradientStartPercent, float gradientSizePercent, Color fadeStartColor, Color fadeEndColor, params Tuple<ProcessTask, int>[] tasksAndZones)
		{
			var control = form.FindSingle<ChannelHeaderControl>();
			var fadePanel = control.FindSingle<ZFadePanel>();

			foreach (var tuple in tasksAndZones)
			{
				var taskCard = BMSGUITestCase.FindTaskCardControl(form, tuple.Item1);
				AssertNotNull(taskCard);
				AssertEquals("Task card has been placed in the correct zone", tuple.Item2, taskCard.Cell.Zone);
			}

			ChannelHeaderControlTest.AssertFadePanelDetails(message, fadePanel, gradientAngle, gradientStartPercent, gradientSizePercent, fadeStartColor, fadeEndColor);
		}

		#endregion

		static void AssertNoOverlaidLabel(VisualBoardForm form)
		{
			var componentControl = form.FindSingle<BMComponentControl>();
			var label = componentControl.FindSingleOrDefault<ZLabel>(l => l.Name == "LoadFailedLabel");
			AssertNull($"There should not be a label overlaid on the component grid, but there is, and it says: {label?.Text}", label);
		}

		protected BMSystem system;

		protected override void SetUp()
		{
			base.SetUp();
			system = BMSTestHelper.CreateSystem(Factory, "ORG");
		}
	}

	#region AlertStatusTestHelper

	class AlertStatusTestHelper
	{
		public BoardSlideshowViewModel ViewModel { get; set; }
		public IVisualBoardChannel VisualBoard { get; set; }
		public GlbStaff Resource { get; set; }
		public OrgHeader Job { get; set; }
		public ProcessTask Task1 { get; set; }
		public ProcessTask Task2 { get; set; }

		public AlertStatusTestHelper(BusinessObjectFactory factory, BMSystem system)
		{
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			ViewModel = BMSTestHelper.CreateSlideshowViewModel(board);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var capability1 = factory.NewWithValidTestData<GlbCapability>();
			Resource = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "MIC", "Mickey Mouse", capability1);
			VisualBoard = sectionViewModel.CreateChannelForTest(Resource);

			Job = factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(Job, factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			Task1 = BMSTestHelper.CreateTask(workflow1, Resource.GS_Code, 60, description: "task1");
			Task2 = BMSTestHelper.CreateTask(workflow2, Resource.GS_Code, 60, description: "task2");
			var task3 = BMSTestHelper.CreateTask(workflow2, Resource.GS_Code, 50, description: "task3");

			factory.Save();

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, "RES", Resource.PK, overrideChannels: true);
			Task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			BMSTestHelper.CacheTasksStartability(sectionViewModel, Task1, Task2, task3);

			factory.Save();
		}

		internal void ChangeTask1Status(string newStatus)
		{
			Task1.P9_Status = newStatus;
		}

		internal ChannelHeaderControl GetHeaderControl(VisualBoardForm form)
		{
			return (BMSGUITestCase.FindChannelHeaderControl(form, Resource));
		}
	}

	public static class ChannelHeaderControlTestHelpers
	{
		public static void RefreshHeading_ForTest(this ChannelHeaderControl control, BMBoardSection section, CardAllocationMap allocationMap)
		{
			var viewModel = control.ViewModel_ForTest;
			control.Channel.ClearChannelCache();
			section.Factory.ServiceContainer.AddService(new CapacityConstrainedResourcesCacheService(section.Board));
			var viewModels = new[] { control.Channel }.MakeViewModelSet(section.Factory, viewModel, allocationMap);

			control.BeginInvokeSafe(() => control.RefreshHeading(control, new HeadingsRefreshedEventArgs(viewModels)));
		}

		#region Implementation

		internal static BMBoardSectionViewModel GetViewModel(BusinessObjectFactory factory, BMSystem system)
		{
			var board = system.Boards.AddNew();

			return BMSTestHelper.CreateViewModel(board.Sections.AddNew());
		}

		internal static HeaderControlProvider.HeaderWrapperControl GetControl(BusinessObject bizo, BMSystem system, BMBoardSection section = null, BMBoardSectionViewModel viewModel = null)
		{
			var cell = new CellContent(0, 0, CellContentType.ChannelHeading) { Channel = viewModel.CreateChannelForTest(bizo) };
			if (viewModel == null)
			{
				viewModel = GetViewModel(bizo.Factory, system);
			}
			return (HeaderControlProvider.HeaderWrapperControl)HeaderControlProvider.GetHeaderControl(cell, null, viewModel, () => new ChannelHeaderControl(cell, viewModel));
		}

		internal static ChannelHeaderControlForTest GetChannelHeaderControl(BusinessObject bizo, BMSystem system, BMBoardSection section = null, BMBoardSectionViewModel viewModel = null, BusinessObjectFactory factory = null)
		{
			if (viewModel == null)
			{
				if (section != null)
				{
					viewModel = BMSTestHelper.CreateViewModel(section);
				}
				else
				{
					viewModel = GetViewModel(factory ?? bizo.Factory, system);
				}
			}

			var cell = new CellContent(0, 0, CellContentType.ChannelHeading) { Channel = viewModel.CreateChannelForTest(bizo) };
			return new ChannelHeaderControlForTest(cell, viewModel);
		}

		#endregion
	}

	#endregion

	#region ChannelHeaderControlForTest

	class ChannelHeaderControlForTest : ChannelHeaderControl
	{
		internal ChannelHeaderControlForTest(CellContent cell, BMBoardSectionViewModel viewModel)
			: base(cell, viewModel)
		{
			asyncDisabler = BMSTestCaseWithFactory.DisableAsyncBehaviour();
		}

		public Image GetImageAtMaxSize_Exposed(Image image, Size maxSize) => GetImageAtMaxSize(image, maxSize);

		readonly IDisposable asyncDisabler;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				asyncDisabler.Dispose();
			}
		}
	}

	#endregion

	#region Non-Transactioned Test

	public class ChannelHeaderControlNonTransactionedTest : NonTransactionedTestCase
	{
		[TestDate(2014, 3, 14)]
		public void TestChannelHeaderDoesNotThrowWhenMapIsNull()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, workflowType: "INQ");

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var buffer = config.Buffer;
			var section = config.Section;
			var sectionConfiguration = section.SectionConfiguration;
			section.SectionConfiguration.CellsPerSubsection = 13;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			section.BackgroundColor = ZString.Empty;
			section.ForegroundColor = ZString.Empty;

			var workflow = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, resource.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * buffer.FC_BufferLoadLimitPercent / 100.0 / 4.0)); // 1/4 of relevant buffer timespan 

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();

				var channelHeaderControl = form.FindAll<ChannelHeaderControl>().First(c => !c.Channel.IsHighRisk);
				AssertNoExceptionThrown(() => channelHeaderControl.RefreshHeading_ForTest(section, null));
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestStatusUpdates()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "JON";
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "JIM", "Jimmy", capability);
			group.Staff.Add(resource);

			Factory.Save();

			BMSTestHelper.CreateReleaseGroup(system, group);
			var buffer = VisualBoardsTestCase.CreateBuffer(system);
			var section = VisualBoardsTestCase.CreateBoardSection(buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "complete");
			var bufferTask = VisualBoardsTestCase.CreateTask(workflow, resource.GS_Code, 60);
			bufferTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateViewModel(section);

			var cell = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading);

			using (var formWrapper = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				formWrapper.AwaitAll();

				ChannelHeaderControl control = null;

				try
				{
					control = formWrapper.FindAll<ChannelHeaderControl>().SingleOrDefault();
				}
				catch (Exception ex) // Exceptions thrown in non transactioned test case cause no rollback
				{
					Fail("an exception was thrown" + ex);
				}
				AssertNotNull(control);

				VisualBoardsTestCase.AssertImagePixelsEqual((Bitmap)control.Channel.StatusImage.Image, Business.Properties.Resources.resource_active);

				var thread2Factory = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};
				var workingTask = thread2Factory.Load<ProcessTask>(bufferTask.PK);
				workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				thread2Factory.Save();

				formWrapper.RefreshBoard();
				formWrapper.AwaitAll();

				Assert(!control.Channel.StatusImage.Image.IsDisposed());
				VisualBoardsTestCase.AssertImagePixelsEqual((Bitmap)control.Channel.StatusImage.Image, Business.Properties.Resources.resource_inactive);
			}
		}

		[TestDate(2014, 3, 14)]
		public void TestHeadingStatusShouldUpdateAsContentInChannelsAreUpdated()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Complete!");
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var cell = new CellContent(0, 0, CellContentType.ChannelHeading);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

			using (var control = new ChannelHeaderControlForTest(cell, sectionViewModel).WrapWithShownForm())
			{
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);

				AssertEquals("Working", control.ChannelStatus);
				AssertEquals(control.ChannelStatus, ToolTipService.GetToolTip(control.ChannelStatusLabel));

				var taskCardCell = new CellContent(0, 0, CellContentType.Cards);
				taskCardCell.Channel = cell.Channel;

				var cardViewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), taskCardCell, task);
				cardViewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Suspended);
				cardViewModel.Save();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);

				AssertEquals("Idle", control.ChannelStatus);
				AssertEquals(control.ChannelStatus, ToolTipService.GetToolTip(control.ChannelStatusLabel));
			}
		}

		[TestDate(2015, 1, 5, 2, 0, 0)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestHeadingIdleTimeShouldBeShown()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Complete!");
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var cell = new CellContent(0, 0, CellContentType.ChannelHeading);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

			using (var control = new ChannelHeaderControlForTest(cell, sectionViewModel).WrapWithShownForm())
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				Factory.Save();

				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Idle", control.ChannelStatus);
				AssertEquals(control.ChannelStatus, ToolTipService.GetToolTip(control.ChannelStatusLabel));

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

				TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Idle for 1 hour", control.ChannelStatus);
				AssertEquals(control.ChannelStatus, ToolTipService.GetToolTip(control.ChannelStatusLabel));

				TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(30);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(15);

				task.Factory.Save();

				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Idle for 15 minutes", control.ChannelStatus);

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(4);

				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Idle for 4 days", control.ChannelStatus);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestRoadRunnerImage_ShouldUpdateImageWhenRefreshingHeadings()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Complete!");
			workflow.FH_FC_CurrentComponent = bucket.PK;
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(resource, system, section, viewModel).WrapWithShownForm())
			{
				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);

				var statusImage = control.StatusPictureBox.Image;
				AssertNotNull(statusImage);

				workflow.FH_FC_CurrentComponent = buffer.PK;
				Factory.Save();

				control.RefreshHeading_ForTest(section, viewModel.ComponentGrid.CardAllocationMap);
				AssertNotNull(control.StatusPictureBox.Image);
				AssertNotEquals(statusImage, control.StatusPictureBox.Image);
			}
		}

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestRefreshHeadings_ShouldReloadChannelBusinessEntity()
		{
			Factory.RefreshEnabled = false;

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");

			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			BMSRegistry.Instance.ChannelHeadingsUsePreferredName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new ZForm { Width = 800, Height = 600 })
			using (var control = ChannelHeaderControlTestHelpers.GetChannelHeaderControl(resource, system, section, viewModel))
			{
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertEquals("Frodo Baggins", control.ChannelNameLabel.Text);

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedResource = newFactory.Load<GlbStaff>(resource.PK);
				loadedResource.GS_FullName = "Frodo Underhill";
				var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

				newFactory.Save();

				control.Channel.ClearCacheAndReload();
				Application.DoEvents();

				AssertEquals("Frodo Underhill", control.ChannelNameLabel.Text);
			}
		}

		[TestDate(2013, 10, 26)]
		public void TestChannelHeaderImage_IsCCRCandidate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var control = form.FindAll<ChannelHeaderControl>().Single();

				section = viewModel.FactoryProvider.GetBoardGUIThreadFactory().Load<BMBoardSection>(section.PK); // Ensures section is loaded in a factory that has access to CCR status service.

				AssertNull(control.ThumbnailPhotoPictureBox.Image);
				AssertNull(control.CcrPictureBox.Image);

				form.RefreshBoardAndWait();

				AssertNull(control.ThumbnailPhotoPictureBox.Image);
				AssertNull(control.CcrPictureBox.Image);

				var link = buffer.ResourceLinks.AddNew();
				link.FD_GS_NKResource = staff.GS_Code;
				link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow;
				Factory.Save();
				form.RefreshBoardAndWait();

				AssertNull(control.ThumbnailPhotoPictureBox.Image);
				AssertNull("Resource has not been persistently overloaded yet - should not add CCR candidate image yet", control.CcrPictureBox.Image);

				link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow.AddMinutes(-buffer.FC_BufferTimespanInMinutes);
				Factory.Save();
				form.RefreshBoardAndWait();

				AssertNull(control.ThumbnailPhotoPictureBox.Image);
				AssertNull("Resource has not been persistently overloaded yet - should not add CCR candidate image yet", control.CcrPictureBox.Image);

				link.FD_IsPersistentlyOverloaded = true;
				Factory.Save();
				form.RefreshBoardAndWait();

				AssertNull(control.ThumbnailPhotoPictureBox.Image);
				AssertNotNull("Is now a persistently-overloaded CCR candidate - should show the image", control.CcrPictureBox.Image);

				link.FD_IsCapacityConstrained = true;
				Factory.Save();
				form.RefreshBoardAndWait();

				AssertNull(control.ThumbnailPhotoPictureBox.Image);
				AssertNotNull("Should show Designated CCR image", control.CcrPictureBox.Image);

				link.FD_IsCapacityConstrained = false;
				Factory.Save();
				form.RefreshBoardAndWait();

				AssertNull(control.ThumbnailPhotoPictureBox.Image);
				AssertNotNull("Should show Candidate CCR image", control.CcrPictureBox.Image);

				link.FD_CapacityConstraintDetectedUtc = ZDateTime.Empty;
				link.FD_IsCapacityConstrained = false;
				Factory.Save();
				form.RefreshBoardAndWait();

				AssertNull(control.ThumbnailPhotoPictureBox.Image);
				AssertNull(control.CcrPictureBox.Image);
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2015, 01, 16, 6, 21, 0)]
		public void TestHeadingActivityTimeShouldBeShown()
		{
			TestDateAttribute.UseUNLOCO = true;
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Complete!");
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			var cell = new CellContent(0, 0, CellContentType.ChannelHeading);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

			using (var control = new ChannelHeaderControlForTest(cell, sectionViewModel).WrapWithShownForm())
			{
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Working", control.ChannelStatus);

				TestDateAttribute.Date = new DateTime(2015, 01, 16, 6, 29, 0);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				Factory.Save();
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Idle", control.ChannelStatus);

				TestDateAttribute.Date = new DateTime(2015, 01, 18, 21, 42, 0);
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Idle for 2 days", control.ChannelStatus);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Working for 8 minutes", control.ChannelStatus);

				TestDateAttribute.Date = new DateTime(2015, 01, 18, 22, 1, 0);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				Factory.Save();

				TestDateAttribute.Date = new DateTime(2015, 01, 18, 22, 55, 30);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Working for 27 minutes", control.ChannelStatus);

				TestDateAttribute.Date = new DateTime(2015, 01, 18, 23, 14, 0);
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Working for 46 minutes", control.ChannelStatus);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertMultilineASCIIEquals("Idle", control.ChannelStatus);

				TestDateAttribute.AddMinutes(2);
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertMultilineASCIIEquals("Idle for 2 minutes", control.ChannelStatus);
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2015, 04, 28, 16, 40, 0)]
		public void TestHeadingActivityTimeShouldBeShown_MaxRollupInHoursForWorkingStatus()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Complete!");
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			var cell = new CellContent(0, 0, CellContentType.ChannelHeading);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

			using (var control = new ChannelHeaderControlForTest(cell, sectionViewModel).WrapWithShownForm())
			{
				TestDateAttribute.Date = new DateTime(2015, 05, 01, 9, 40, 0);
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Working for 21.5 hours", control.ChannelStatus);

				TestDateAttribute.Date = new DateTime(2015, 05, 05, 9, 40, 0);
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Working for 40.5 hours", control.ChannelStatus);

				TestDateAttribute.Date = new DateTime(2016, 05, 05, 9, 40, 0);
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Working for 2529.5 hours", control.ChannelStatus);

				TestDateAttribute.Date = new DateTime(2016, 05, 05, 9, 40, 0);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				Factory.Save();
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Idle", control.ChannelStatus);

				TestDateAttribute.Date = new DateTime(2017, 05, 05, 9, 40, 0);
				control.Channel.ClearChannelCache();
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);
				AssertEquals("Idle for 1 year", control.ChannelStatus);
			}
		}

		[TestDate(2014, 3, 14)]
		public void TestHeadingStatusShouldNotUpdateWhenCapabilityFilterIsApplied()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Complete!");
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow, ZString.Empty, 3000);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_G4_RequiredCapability = capability.PK;

			var task2 = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 2);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			resource.Capabilities.Add(capability);

			Factory.Save();

			var cell = new CellContent(0, 0, CellContentType.ChannelHeading);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

			var filter = new CapabilityTaskFilter();
			sectionViewModel.FilterManager.ApplyFilter(filter);

			using (var control = new ChannelHeaderControlForTest(cell, sectionViewModel).WrapWithShownForm())
			{
				filter.TaskOption = TaskCapabilityFilter.ShowOnlyUnassignedCapabilityTasks;
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);

				AssertEquals("Overloaded, Working", control.ChannelStatus);
				AssertEquals(control.ChannelStatus, ToolTipService.GetToolTip(control.ChannelStatusLabel));

				cell.Channel.ClearChannelCache();

				filter.TaskOption = TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks;
				control.RefreshHeading_ForTest(section, sectionViewModel.ComponentGrid.CardAllocationMap);

				AssertEquals("Overloaded, Working", control.ChannelStatus);
				AssertEquals(control.ChannelStatus, ToolTipService.GetToolTip(control.ChannelStatusLabel));
			}
		}

		#region Implementation

		protected BMSystem system;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			system = BMSTestHelper.CreateSystem(Factory, "ORG");
		}

		#endregion
	}

	#endregion
}
