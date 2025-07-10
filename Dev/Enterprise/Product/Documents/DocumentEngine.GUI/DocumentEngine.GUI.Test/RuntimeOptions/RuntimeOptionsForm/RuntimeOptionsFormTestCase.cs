using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class RuntimeOptionsFormTestCase : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestTypeInLanguageUpdatesInstruction()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), EmptyAndValidTemplate))
			{
				report.Parent.Language = Core.SharedConstants.Languages.EnglishAmerican;
				report.PrepareForRender();
				using (var form = GetNewForm(report))
				{
					form.Show();
					form.LanguageZDropEdit.CodeBox.Text = Core.SharedConstants.Languages.ChineseSimplified;
					form.ActionButton.Focus();
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, report.Parent.Language);
				}
			}
		}

		public void TestAutoScaleModeSetProperlyOnFormAndAllChildControls()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), CrowdedFilterForm))
			{
				report.PrepareForRender();
				using (RuntimeOptionsForm form = GetNewForm(report))
				{
					form.Show();

					var problems = new ZStringBuilder();

					if (form.AutoScaleMode != ControlDpiScalingHelper.DpiScaleMode)
					{
						problems.Append(string.Format(form.Name + ".AutoScaleMode should always be '{0}' so that Enterprise doesn't look like a dog's breakfast with Large Fonts.", ControlDpiScalingHelper.DpiScaleMode.ToString()));
						problems.Append("");
					}

					var containerControls = new List<ContainerControl>();
					foreach (Control childControl in form.Controls)
					{
						GetChildContainerControls(childControl, containerControls);
					}

					var childControlProblems = new ZStringBuilder();
					foreach (var containerControl in containerControls)
					{
						if (containerControl.AutoScaleMode != ControlDpiScalingHelper.DpiScaleMode)
						{
							childControlProblems.Append(containerControl.Name + " - " + containerControl.GetType().ToString() + " - " + containerControl.AutoScaleMode.ToString());
						}
					}
					if (!childControlProblems.IsEmpty)
					{
						problems.Append(string.Format("All Child Controls should have an AutoScaleMode of '{0}', but the following controls don't:", ControlDpiScalingHelper.DpiScaleMode));
						problems.Append("");
						problems.Append(childControlProblems.ToStringWithNewLineBetweenAppends());
						problems.Append("");
					}
					Assert("Found the following problems with 'AutoScaleMode' settings:\r\n\r\n" + problems.ToStringWithNewLineBetweenAppends(), problems.IsEmpty);
				}
			}
		}

		public void TestAutoScaleDoesntExceedScreenBoundaries()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SuperCrowdedFilterForm.xls", "SuperCrowdedFilterForm.xls");
			var template = new ExcelTemplateForUnitTesting("SuperCrowdedFilterForm.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			{
				report.PrepareForRender();
				using (var form = GetNewForm(report))
				{
					form.Show();

					var curerntScreen = CachedScreenInfo.Instance.FromControl(form);
					Assert(form.Height <= curerntScreen.Height);
					Assert(form.Width <= curerntScreen.Width);
				}
			}
		}

		public void TestDeserializeShowsValuesInControls()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TextFilter.xls", "TextFilter.xls");
			var template = new ExcelTemplateForUnitTesting("TextFilter.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			{
				report.PrepareForRender();
				((TextField)report.FilterCollection[1]).Value = "TestFilterValue";
				ScheduleTask.S5_ScheduleState = ScheduleTask.Serialize(report);
			}

			var pack = new DocumentPack(Factory.New<StmMenuItem>());
			using (var report = new Report(pack, template))
			using (var form = new RuntimeOptionsForm(report, ScheduleTask))
			{
				form.Show();
				AssertEquals("Should find the filter value.", true, ContainsTestFilterValue(form));
				AssertNotNull("Constructor for Scheduled reports should call base to set .BusinessEntity", form.BusinessEntity);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestUsingIFilter()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
			var template = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			{
				report.PrepareForRender();
				var filter = new FilterForTest();
				report.FilterCollection.Add(filter);

				GetNewForm(report).Dispose();
			}
		}

		public void TestGroupByGroupBoxIsVisible()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.GroupByPage.xls", "GroupByPage.xls");
			var template = new ExcelTemplateForUnitTesting("GroupByPage.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			{
				report.PrepareForRender();

				using (RuntimeOptionsForm testForm = GetNewForm(report))
				{
					testForm.Show();
					AssertEquals(true, testForm.GroupByGroupBox.Visible);
					AssertEquals(4, testForm.GroupByGroupBox.Controls.Count);
				}
			}
		}

		[RequiresSTA]
		public void TestPageBreakCheckBox()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), GroupByPageWithContinuousPageStyle))
			{
				report.PrepareForRender();

				using (RuntimeOptionsForm testForm = GetNewForm(report))
				{
					testForm.Show();
					AssertEquals("GroupBy GroupBox must be visible", true, testForm.GroupByGroupBox.Visible);
					AssertEquals("GroupBy GroupBox must have 5 controls", 5, testForm.GroupByGroupBox.Controls.Count);
					var pageBreakCheckBox = (ZCheckBox)testForm.GroupByGroupBox.Controls["PageBreakCheckBox"];
					pageBreakCheckBox.Checked = true;
					AssertEquals("GroupByCollection.BreakPageOverride must be true", true, report.GroupByCollection.BreakPageOverride);
				}
			}
		}

		public void TestPageBreakCheckBoxShouldHaveDefaultValueEqualsToBreakPageOverride()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), GroupByPageWithContinuousPageStyle))
			{
				report.PrepareForRender();
				report.GroupByCollection.BreakPageOverride = true;

				using (var testForm = GetNewForm(report))
				{
					testForm.Show();

					var pageBreakCheckBox = (ZCheckBox)testForm.GroupByGroupBox.Controls["PageBreakCheckBox"];

					Assert("pageBreakCheckBox should be checked.", pageBreakCheckBox.Checked);
				}
			}
		}

		public void TestFormGoesToTwoColumnsIfTooManyControlsAreInIt()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), CrowdedFilterForm))
			{
				report.PrepareForRender();
				using (RuntimeOptionsForm testForm = GetNewForm(report))
				{
					testForm.Show();
					AssertEquals("Primary Filters group should have two columns", 2, testForm.FilterTabControl.FilterGroupBoxes[""].NextHeights.Count);
					AssertEquals("Sort order group box should have two columns", 2, testForm.SortOrderGroupBox.NextHeights.Count);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestEmptyConstructor()
		{
			new RuntimeOptionsForm().Dispose();
		}

		[RequiresSTA]
		public void TestMakeGroupBoxesHaveTwoColumnsForDynamicFilterGroups()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), LotsOfFiltersWithGroups))
			using (RuntimeOptionsForm testForm = GetNewForm(report))
			{
				testForm.Show();
				foreach (AutoLayoutGroupBox groupBox in testForm.FilterTabControl.FilterGroupBoxes)
				{
					AssertEquals(2, groupBox.NextHeights.Count);
				}
			}
		}

		[RequiresSTA]
		public void TestMakeGroupBoxesHaveTwoColumnsWhenFormRequiresTwoButGroupsDoNot()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleFiltersWithGroupsAndGroupBysToForceTwoColumns.xls", "MultipleFiltersWithGroupsAndGroupBysToForceTwoColumns.xls");
			var template = new ExcelTemplateForUnitTesting("MultipleFiltersWithGroupsAndGroupBysToForceTwoColumns.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			{
				using (RuntimeOptionsForm testForm = GetNewForm(report))
				{
					testForm.Show();
					foreach (AutoLayoutGroupBox groupBox in testForm.FilterTabControl.FilterGroupBoxes)
					{
						AssertEquals("Dock style should not be fill as this causes an exception in this case", DockStyle.None, groupBox.Dock);
						AssertEquals(2, groupBox.NextHeights.Count);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestCalculateDesiredSizesIncludesAllFilterGroups()
		{
			const int mysteryPixelsHeight = 8;
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), MultipleFiltersWithGroups))
			using (var testForm = GetNewForm(report))
			{
				testForm.Show();
				var formSize = testForm.ClientSize;
				AssertEquals("height should be filter groups + footer + status panel + dockpadding top and bottom", testForm.DockPadding.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(mysteryPixelsHeight) + testForm.FilterTabControl.ItemSize.Height + testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredHeight + testForm.FooterPanel.Height + testForm.MainStatusBar.Height + testForm.DockPadding.Bottom, formSize.Height);
				AssertEquals("Width should fit it's content", formSize.Width, WidestGroupBoxInFilterGroups(testForm.FilterTabControl.FilterGroupBoxes) + testForm.TabControlPaddingFudge + testForm.DockPadding.Left + testForm.DockPadding.Right);
			}
		}

		[RequiresSTA]
		public void TestAddoptionalSheetsControls()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleTemplatesWithOptionalTemplates.xls", "MultipleTemplatesWithOptionalTemplates.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), excelTemplate))
			{
				using (RuntimeOptionsForm testForm = GetNewForm(report))
				{
					AssertEquals("Optional templates group should have two controls", 2, testForm.OptionalTemplatesGroupBox.Controls.Count);
					AssertControlsCollectionContainsControl(testForm.OptionalTemplatesGroupBox.Controls, "Sheet1");
					var checkBox = (ZCheckBox)AssertControlsCollectionContainsControl(testForm.OptionalTemplatesGroupBox.Controls, "Sheet3");
					AssertEquals("CheckBox should be bound", true, checkBox.DataBindings.Count > 0);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "Testing")]
		[RequiresSTA]
		public void TestClientSizeCorrectWithRegardToFilterScrolling()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), MultipleFiltersWithGroups))
			{
				using (RuntimeOptionsForm testForm = GetNewForm(report))
				{
					testForm.Show();
					Application.DoEvents();
					AssertEquals("Filter Group should not be scrolling", false, testForm.FilterTabControl.AreGroupsScrolling);
					AssertEquals("Width should fit it's controls", testForm.ClientSize.Width, testForm.FilterTabControl.DesiredWidth + testForm.DockPadding.Left + testForm.DockPadding.Right + testForm.TabControlPaddingFudge);

					var clientWidth = testForm.FilterTabControl.TabPages[0].ClientSize.Width;
					foreach (ZTabPage page in testForm.FilterTabControl.TabPages)
					{
						testForm.FilterTabControl.SelectedTab = page;
						AssertEquals("TabPages should all have same client with", clientWidth, page.ClientSize.Width);
					}

					foreach (AutoLayoutGroupBox groupBox in testForm.FilterTabControl.FilterGroupBoxes)
					{
						AssertEquals("Group " + groupBox.Name + "- Dock style should be fill", DockStyle.None, groupBox.Dock);
						AssertEquals("Group " + groupBox.Name + "- width should be filter tab page width", clientWidth, groupBox.Width);
					}
				}
			}

			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), LotsOfFiltersWithGroups))
			{
				using (RuntimeOptionsForm testForm = GetNewForm(report))
				{
					testForm.Show();
					AssertEquals("Filter Group should be scrolling", true, testForm.FilterTabControl.AreGroupsScrolling);

					var verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
					var screenHeight = Screen.PrimaryScreen.Bounds.Height;
					AssertEquals("Width should be filter groups max width plus scroll bar width (" + verticalScrollBarWidth + ") plus dock padding", testForm.FilterTabControl.DesiredWidth + verticalScrollBarWidth + testForm.TabControlPaddingFudge + testForm.DockPadding.Left + testForm.DockPadding.Right, testForm.ClientSize.Width);

					testForm.FilterTabControl.SelectedTab = (ZTabPage)testForm.FilterTabControl.TabPages[0];
					AssertEquals("Client width on page 1 should be Maxdesired width of groups + scroll bar width", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth + verticalScrollBarWidth, testForm.FilterTabControl.TabPages[0].ClientSize.Width);
					AssertEquals("Group '' width should Maxdesired width", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth + verticalScrollBarWidth, testForm.FilterTabControl.FilterGroupBoxes[""].Width);

					testForm.FilterTabControl.SelectedTab = (ZTabPage)testForm.FilterTabControl.TabPages[1];
					if (screenHeight >= testForm.Size.Height)
					{
						AssertEquals("Client width on page 2 should be Maxdesired width of groups + scroll bar width", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth + verticalScrollBarWidth, testForm.FilterTabControl.TabPages[1].ClientSize.Width);
						AssertEquals("Group One width should Maxdesired width + scroll bar width", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth + verticalScrollBarWidth, testForm.FilterTabControl.FilterGroupBoxes["One"].Width);
					}
					else
					{
						AssertEquals("Client width on page 2 should be Maxdesired width of groups", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth, testForm.FilterTabControl.TabPages[1].ClientSize.Width);
						AssertEquals("Group One width should Maxdesired width", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth, testForm.FilterTabControl.FilterGroupBoxes["One"].Width);
					}

					testForm.FilterTabControl.SelectedTab = (ZTabPage)testForm.FilterTabControl.TabPages[2];
					if (screenHeight >= testForm.Size.Height)
					{
						AssertEquals("Client width on page 3 should be Maxdesired width of groups", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth, testForm.FilterTabControl.TabPages[2].ClientSize.Width);
						AssertEquals("Group Two width should Maxdesired width", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth, testForm.FilterTabControl.FilterGroupBoxes["Two"].Width);
					}
					else
					{
						AssertEquals("Client width on page 3 should be Maxdesired width of groups - scroll bar width", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth - verticalScrollBarWidth, testForm.FilterTabControl.TabPages[2].ClientSize.Width);
						AssertEquals("Group Two width should Maxdesired width - scroll bar width", testForm.FilterTabControl.FilterGroupBoxes.MaxDesiredWidth - verticalScrollBarWidth, testForm.FilterTabControl.FilterGroupBoxes["Two"].Width);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPositionControlsHorizontally()
		{
			var values = new bool[] { true, false };

			foreach (var errorExists in values)
			{
				foreach (var groupByExists in values)
				{
					foreach (var sortExists in values)
					{
						foreach (var filterExists in values)
						{
							foreach (var optionsExist in values)
							{
								AssertGroupsDisplayedCorrectly(errorExists, groupByExists, sortExists, filterExists, optionsExist);
							}
						}
					}
				}
			}
		}

		public void TestAmpersandsNotNemonic()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), MultipleTemplatesWithAmpersand))
			{
				using (RuntimeOptionsForm testForm = GetNewForm(report))
				{
					testForm.Show();
					AssertNoLonleyAmpersands(GetCheckBoxFromCollection(testForm.OptionalTemplatesGroupBox.Controls, "Sheet & 1"));
				}
			}
		}

		[RequiresSTA]
		public void TestCreateShortcutLinkLabelClick()
		{
			AssertEquals("Precondition", false, Form.CreateShortcutContextMenu.Visible);
			InvokeLinkLabelClick(Form.CreateShortcutLinkLabel);
			AssertEquals("Create Shortcut link shows the Create Shortcut context menu", true, Form.CreateShortcutContextMenu.Visible);
		}

		public void TestCreateDesktopShortcutMenuItem()
		{
			ReportCommand.Factory.Save();
			Form.Show();
			Application.DoEvents();

			Form.CreateDesktopShortcutMenuItem.PerformClick();
			AssertEquals("Caption", "Template For Testing", Form.ShortcutCreator.LastCreatedDesktopShortcutCaption);
			AssertEquals("URL", true, Form.ShortcutCreator.LastCreatedDesktopShortcutURL.StartsWith("edient:Command=RunReport&LicenceCode=EDIEDIDAT&ReportPK=" + ReportCommand.PK));
		}

		public void TestCreateHyperlinkMenuItem()
		{
			ReportCommand.Factory.Save();
			Form.Show();
			Application.DoEvents();

			Form.CreateHyperlinkMenuItem.PerformClick();
			AssertEquals("Caption", "Template For Testing", Form.ShortcutCreator.LastCreatedHyperlinkCaption);
			AssertEquals("URL", true, Form.ShortcutCreator.LastCreatedHyperlinkURL.StartsWith("edient:Command=RunReport&LicenceCode=EDIEDIDAT&ReportPK=" + ReportCommand.PK));
		}

		public void TestShortcutMenuItems_NotAvailableIfReportCommandNotAvailable()
		{
			using (var report = new Report(new DocumentPack(), MultipleTemplatesWithAmpersand))
			using (var form = new RuntimeOptionsFormForTest(report))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Precondition: ReportCommand shouldn't be available", null, form.fReport.Parent.StmMenuCommand);
				AssertEquals("CreateDesktopShortcutMenuItem not available if the report doesnt have a ReportCommand", false, form.CreateDesktopShortcutMenuItem.Visible);
				AssertEquals("CreateHyperlinkMenuItem not available if the report doesnt have a ReportCommand", false, form.CreateHyperlinkMenuItem.Visible);
			}
		}

		[RequiresSTA]
		public void TestPreviewButtonOnRuntimeOptionsFormCausesValidation()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.DateFilterWithRequired.xls", "DateFilterWithRequired.xls");
			var template = new ExcelTemplateForUnitTesting("DateFilterWithRequired.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			using (var form = GetNewForm(report))
			{
				form.PreviewButton_Click(form, EventArgs.Empty);
				AssertEquals("Report should have errors.", true, report.HasErrors);
			}
		}

		[RequiresSTA]
		public void TestPreviewingUsesClones()
		{
			using (var documentPack = new DocumentPack(Factory.LoadTop1<ReportCommand>(new DocumentZQuery(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, "Rep"))))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				documentPack.Add(report);

				var originalInstructions = new DeliveryInstructions(documentPack);
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					using (var form = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, originalInstructions, Env.Security.None))
					{
						form.SuppressPreview = true;
						form.PreviewButton_Click(form, EventArgs.Empty);
						AssertNotNull("form.LastPreviewedDeliveryInstructions", form.LastPreviewedDeliveryInstructions);
						AssertNotEquals("Clone should be used", originalInstructions, form.LastPreviewedDeliveryInstructions);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestDeliveringUsesClones()
		{
			using (var pack = new DocumentPack(Factory.LoadTop1<ReportCommand>(new DocumentZQuery(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, "Rep"))))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				pack.Add(report);

				var originalInstructions = new DeliveryInstructions(pack);
				using (var optionsForm = new RuntimeOptionsFormForTest(report, AllowedDeliveryOptions.All, originalInstructions, Env.Security.None))
				{
					DeliveryInstructions instructions = null;
					optionsForm.DeliveryRequested += (deliverForm, deliveryOptions, innerInstructions, modifyDocumentCheckPoint) =>
						instructions = innerInstructions;
					optionsForm.SubmitButton_Click(optionsForm, EventArgs.Empty);
					Assert("Instructions shouldn't be null", instructions != null);
					AssertNotEquals("Clone should be used", originalInstructions, instructions);
				}
			}
		}

		public void TestShowMessageOfSecondaryServerConnectionException()
		{
			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			using (SystemDataRegistry.Instance.ReportingDbServerThresholdCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9999))
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), EmptyAndValidTemplate))
			using (var form = new RuntimeOptionsFormForTest(report))
			{
				var reportDb = new ReportDbForTestingWithoutOverrideAllReportServerNames(new string[] { Db.Connection.ServerName }, "InvalidDatabase", false, Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox);
				form.SetReportDbManagerForTesting(reportDb);
				reportDb.ReportConnectionException = SqlExceptionBuilder.CreateSqlException(4060, $"Cannot open database \"InvalidDatabase\" requested by the login. The login failed.\r\nLogin failed for user '{((IDbReconnectionHandling)Db.Connection).LoginName}'.");
				form.ReportDbManager_Exposed.SecondaryServerConnectionDetails.GetDelayBetweenPrimaryAndReportDatabaseInMinutes();

				AssertEquals($"If other Secondary Servers are set up in the registry setting under \"System -> Reports -> Reporting databases full server names\", the system will try to use them before reverting to the Primary Server.\r\nException details:\r\nCannot open database \"InvalidDatabase\" requested by the login. The login failed.\r\nLogin failed for user '{((IDbReconnectionHandling)Db.Connection).LoginName}'.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals($"Unable to connect to the Secondary Server {Db.Connection.ServerName}", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestShowCorrectServerForSecondaryServerConnectionException()
		{
			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			using (SystemDataRegistry.Instance.ReportingDbServerThresholdCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9999))
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), EmptyAndValidTemplate))
			using (var form = new RuntimeOptionsFormForTest(report))
			{
				var reportDb = new ReportDbForTesting(new string[] { "InvalidServer" }, "InvalidDatabase", false, Report.ShowMessageOfSecondaryServerConnectionExceptionByMessageBox);
				reportDb.ReportServerException = new ArgumentException("Dummy exception");
				form.SetReportDbManagerForTesting(reportDb);
				form.ReportDbManager_Exposed.SecondaryServerConnectionDetails.GetDelayBetweenPrimaryAndReportDatabaseInMinutes();

				AssertEquals($"Unable to connect to the Secondary Server InvalidServer", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestMustRunOnline()
		{
			var command = Factory.Load<ReportCommand>(new ZGuid("CFD5875F-1F58-4E39-A775-AB5FEFF9D5CC"));
			var testPack = new DocumentPack(command);

			using (var report = new Report(testPack, EmptyAndValidTemplate))
			{
				AssertEquals("Pre-Condition: report object is report", Report.Styles.Report, report.Style);

				command.SU_MustRunOnline = true;
				using (var form = new RuntimeOptionsFormForTest(report))
				{
					form.SetReportDbManagerForTesting(new ReportDbForTesting("TestServer", ""));
					form.Show();
					Application.DoEvents();
					form.CalculateOverrideReportDbOption();
					AssertEquals("OverrideReportDbOption when report MustRunOnline", true, report.OverrideReportDbOption);
				}

				// server not respond
				RuntimeOptionsFormForTest.ResetSessionOverrideReportDbOption();
				command.SU_MustRunOnline = false;
				report.OverrideReportDbOption = false;
				using (var form = new RuntimeOptionsFormForTest(report))
				{
					form.SetReportDbManagerForTesting(new ReportDbForTesting("TestServer", ""));
					((ReportDbForTesting)(form.ReportDbManager_Exposed.SecondaryServerConnectionDetails)).ReportServerRespondTime = long.MaxValue;
					form.Show();
					Application.DoEvents();
					form.CalculateOverrideReportDbOption();
					AssertEquals("OverrideReportDbOption when report MustRunOnline", true, report.OverrideReportDbOption);
				}

				// server respond with appropriate threshold
				RuntimeOptionsFormForTest.ResetSessionOverrideReportDbOption();
				report.OverrideReportDbOption = false;
				using (var form = new RuntimeOptionsFormForTest(report))
				{
					form.SetReportDbManagerForTesting(new ReportDbForTesting("TestServer", ""));
					((ReportDbForTesting)(form.ReportDbManager_Exposed.SecondaryServerConnectionDetails)).ReportServerRespondTime = 0;
					form.Show();
					Application.DoEvents();
					form.CalculateOverrideReportDbOption();
					AssertEquals("OverrideReportDbOption when report MustRunOnline", false, report.OverrideReportDbOption);
				}
			}
		}

		public void TestFormVerbIsEmpty()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), EmptyAndValidTemplate))
			using (var form = new RuntimeOptionsFormForTest(report))
			{
				AssertEquals(string.Empty, form.FormVerb);
			}
		}

		public void TestOKButtonDoesNotCloseForm()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), EmptyAndValidTemplate))
			using (var form = new RuntimeOptionsFormForTest(report))
			{
				form.Show();
				form.ActionButton.PerformClick();
				AssertEquals("Form should be not be closed.", false, form.IsFormClosed);
			}
		}

		[RequiresSTA]
		public void TestTabOrder()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.ErrorsFiltersSortsGroupBysUDFsOptionsNYYYYN.xls", "ErrorsFiltersSortsGroupBysUDFsOptionsNYYYYN.xls");
			var template = new ExcelTemplateForUnitTesting("ErrorsFiltersSortsGroupBysUDFsOptionsNYYYYN.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			using (var testForm = GetNewForm(report))
			{
				testForm.Show();
				Assert("ErrorsGroupBox tab order", testForm.ErrorsGroupBox.TabIndex < testForm.FilterTabControl.TabIndex);
				Assert("FilterTabControl tab order", testForm.FilterTabControl.TabIndex < testForm.SortOrderGroupBox.TabIndex);
				Assert("SortOrderGroupBox tab order", testForm.SortOrderGroupBox.TabIndex < testForm.GroupByGroupBox.TabIndex);
				Assert("GroupByGroupBox tab order", testForm.GroupByGroupBox.TabIndex < testForm.OptionalTemplatesGroupBox.TabIndex);
				Assert("OptionalTemplatesGroupBox tab order", testForm.OptionalTemplatesGroupBox.TabIndex < testForm.UserDefinedFieldsGroupBox.TabIndex);
				Assert("UserDefinedFieldsGroupBox tab order", testForm.UserDefinedFieldsGroupBox.TabIndex < testForm.FooterPanel.TabIndex);
			}
		}

		public void TestPreviewButtonSecurity()
		{
			using (var pack = new DocumentPack(Factory.LoadTop1<ReportCommand>(new DocumentZQuery(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, "Rep"))))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				pack.Add(report);
				var originalInstructions = new DeliveryInstructions(pack);
				// edit security right so its checks on for a button.
				using (var form = new RuntimeOptionsFormForTest(report, AllowedDeliveryOptions.All, originalInstructions, Env.Security.None))
				{
					form.Show();
					AssertEquals("Preview Button should be visible as user has all security rights granted", true, form.PreviewButton.Visible);
				}

				Env.Security.PreviewReportButton.IsAllowedForAllBranches = false;
				using (var form1 = new RuntimeOptionsFormForTest(report, AllowedDeliveryOptions.All, null, null))
				{
					AssertEquals("Preview Button should be unvisible, as security set to deny.", false, form1.PreviewButton.Visible);
				}
			}
		}

		public void TestPreviewButton()
		{
			using (var pack = new DocumentPack(Factory.LoadTop1<ReportCommand>(new DocumentZQuery(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, "Rep"))))
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				pack.Add(report);

				var originalInstructions = new DeliveryInstructions(pack);
				using (var form = new RuntimeOptionsFormForTest(report, AllowedDeliveryOptions.All, originalInstructions, Env.Security.None))
				{
					form.Show();
					AssertEquals("PreviewButton should be shown for this case.", true, form.PreviewButton.Visible);
				}

				using (var form = GetNewForm(report))
				{
					form.Show();
					AssertEquals("PreviewButton should be hidden for this case.", false, form.PreviewButton.Visible);
				}

				using (var form = new RuntimeOptionsForm(report, ScheduleTask))
				{
					form.Show();
					AssertEquals("PreviewButton should be hidden for this case.", false, form.PreviewButton.Visible);
				}
			}
		}

		public void TestLanguageZDropEdit()
		{
			var template = EmptyAndValidTemplate;
			template.ContainsCustomisedSections = false;
			using (var pack = new DocumentPack(Factory.LoadTop1<ReportCommand>(new DocumentZQuery(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, "Rep"))))
			using (var report = new Report(new DocumentPack(), template))
			{
				pack.Add(report);
				Assert("Precondition", !report.ContainsAnyCustomisation);
				var originalInstructions = new DeliveryInstructions(pack);
				using (var form = new RuntimeOptionsFormForTest(report, AllowedDeliveryOptions.All, originalInstructions, Env.Security.None))
				{
					form.Show();
					Assert("LabelReportLanguage should be shown for this case.", form.LabelReportLanguage.Visible);
					Assert("LanguageZDropEdit should be shown for this case.", form.LanguageZDropEdit.Visible);
					AssertNotNull("LanguageZDropEdit.List should not be null for this case", form.LanguageZDropEdit.List);
				}

				using (var form = new RuntimeOptionsForm(report, ScheduleTask))
				{
					form.Show();
					Assert("LabelReportLanguage should be shown for this case.", form.LabelReportLanguage.Visible);
					Assert("LanguageZDropEdit should be shown for this case.", form.LanguageZDropEdit.Visible);
					AssertNotNull("LanguageZDropEdit.List should not be null for this case", form.LanguageZDropEdit.List);
				}

				template.ContainsCustomisedSections = true;
				Assert("Precondition", report.ContainsAnyCustomisation);
				using (var form = new RuntimeOptionsFormForTest(report, AllowedDeliveryOptions.All, originalInstructions, Env.Security.None))
				{
					form.Show();
					Assert("LanguageZDropEdit should be hidden for this case.", !form.LabelReportLanguage.Visible);
					Assert("LanguageZDropEdit should be hidden for this case.", !form.LanguageZDropEdit.Visible);
				}
			}
		}

		[RequiresSTA]
		public void TestLanguageRestoredForScheduleReport()
		{
			var template = EmptyAndValidTemplate;
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			{
				report.PrepareForRender();
				report.Parent.Language = Core.SharedConstants.Languages.ChineseSimplified;
				ScheduleTask.S5_ScheduleState = ScheduleTask.Serialize(report);
			}
			using (var report = new Report(new DocumentPack(), template))
			using (var form = new RuntimeOptionsForm(report, ScheduleTask))
			{
				AssertEquals("BusinessEntity's language should be synchronized with serialized report", Core.SharedConstants.Languages.ChineseSimplified, form.BusinessEntity.Language);
				form.BusinessEntity.Parent.Language = Core.SharedConstants.Languages.Arabic;
				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load(report.FilterCollection, report.GroupByCollection, report.SortOrderCollection, report.OrientationManager, report.Parent);
				AssertEquals("BusinessEntity's language should be synchronized with current configurationmanager", Core.SharedConstants.Languages.EnglishAmerican, form.BusinessEntity.Language);
			}
		}

		[RequiresSTA]
		public void TestLanguageRestoredWhenLastSavedSettingWasSelected()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.FilterSortGroupby.xls", "FilterSortGroupby.xls");
			var template = new ExcelTemplateForUnitTesting("FilterSortGroupby.xls", Path.GetFullPath(tempFileName));

			var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());

			using (var report = new Report(new DocumentPack(menuItem), template))
			{
				report.PrepareForRender();
				report.ColumnHeadingManager.ConfigurationManagersForAllSavedConfigurations[1].Save(report.ColumnHeadingManager.LinkedFilterFields, report.GroupByCollection.SelectedGroupBy.DisplayName, report.SortOrderCollection.SelectedOrder.DisplayName, report.OrientationManager.Value, report.Parent.Language);

				((TextField)report.ColumnHeadingManager.LinkedFilterFields[2]).Value = "Test";
				report.Parent.Language = Core.SharedConstants.Languages.ChineseSimplified;
				report.GroupByCollection.SelectedGroupBy.DisplayName = "Branch";
				report.SortOrderCollection.SelectedOrder.DisplayName = "Objective";
				report.Orientation = "ORI";
				ScheduleTask.S5_ScheduleState = ScheduleTask.Serialize(report);
			}

			using (var report = new Report(new DocumentPack(menuItem), template))
			using (var form = new RuntimeOptionsForm(report, ScheduleTask))
			{
				report.ColumnHeadingManager.ConfigurationManagersForAllSavedConfigurations[2].Load(report);
				AssertEquals("Company default configuration value", "Hello", ((TextField)report.ColumnHeadingManager.LinkedFilterFields[2]).Value);
				AssertEquals("Company default configuration value", Core.SharedConstants.Languages.EnglishAmerican, form.BusinessEntity.Language);
				AssertEquals("Company default configuration value", "Period", report.GroupByCollection.SelectedGroupBy.DisplayName);
				AssertEquals("Company default configuration value", "Country", report.SortOrderCollection.SelectedOrder.DisplayName);
				AssertEquals("Company default configuration value", "DEF", report.Orientation);

				report.ColumnHeadingManager.ConfigurationManagersForAllSavedConfigurations[0].Load(report);
				AssertEquals("Last saved setting configuration value", "Test", ((TextField)report.ColumnHeadingManager.LinkedFilterFields[2]).Value);
				AssertEquals("Last saved setting configuration value", Core.SharedConstants.Languages.ChineseSimplified, form.BusinessEntity.Language);
				AssertEquals("Last saved setting configuration value", "Branch", report.GroupByCollection.SelectedGroupBy.DisplayName);
				AssertEquals("Last saved setting configuration value", "Objective", report.SortOrderCollection.SelectedOrder.DisplayName);
				AssertEquals("Last saved setting configuration value", "ORI", report.Orientation);
			}
		}

		public void TestColumnArrangementControlsAreOnlyShownForReports()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.LookupFilter.xls", "LookupFilter.xls");
			var template = new ExcelTemplateForUnitTesting("LookupFilter.xls", Path.GetFullPath(tempFileName));

			var documentPack = new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery()));
			using (var report = new Report(documentPack, template))
			using (var form = GetNewForm(report))
			{
				form.Show();
				AssertContainsColumnArrangementControls(form.FilterTabControl.TabPages, true);
			}

			using (var report = new Report(documentPack, template, new DocumentWrapperForTesting("value"), "Test Report", null, DocumentDirection.ANY, false))
			using (var form = GetNewForm(report))
			{
				form.Show();
				AssertContainsColumnArrangementControls(form.FilterTabControl.TabPages, false);
			}
		}

		public void TestShowFormForScheduleReport_ErrorControlsShouldNotBeShown()
		{
			using (var form = new RuntimeOptionsForm(Report, ScheduleTask))
			{
				AssertErrorControlsShown(form, false);
			}
		}

		public void TestShowFormForRunningReport_ErrorControlsShouldNotBeShown()
		{
			using (var form = new RuntimeOptionsForm(new PrintTask(), Report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
			{
				AssertErrorControlsShown(form, false);
			}
		}

		public void TestShowFormForReportErrors_ErrorControlsShouldBeShown()
		{
			var error = new Mock<IReportProcessingError>();

			error.Setup(m => m.Severity).Returns(ReportProcessingErrorSeverity.Error);
			error.Setup(m => m.Message).Returns("Blah blah blah");

			using (var reportWithError = new Report(new DocumentPack(ReportCommand), EmptyAndValidTemplate))
			{
				typeof(ReportErrorManager).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(reportWithError.ErrorManager, new[] { error.Object });
				using (var form = GetNewForm(reportWithError))
				{
					AssertErrorControlsShown(form, true);
				}
			}
			error.VerifyAll();
		}

		public void TestSortOrderTextIsNotEmpty()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), CrowdedFilterForm))
			{
				report.PrepareForRender();
				using (RuntimeOptionsForm form = GetNewForm(report))
				{
					form.Show();
					AssertEquals("option 1", form.SortOrderGroupBox.Controls[0].Text);
					AssertEquals("option 2", form.SortOrderGroupBox.Controls[1].Text);
				}
			}
		}

		public void TestGroupByTextIsNotEmptyForCustomizedReport()
		{
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), GroupByPageWithContinuousPageStyle))
			{
				report.PrepareForRender();
				using (RuntimeOptionsForm form = GetNewForm(report))
				{
					form.Show();
					AssertEquals("Only RL_HasAirport", form.GroupByGroupBox.Controls[0].Text);
					AssertEquals("All", form.GroupByGroupBox.Controls[1].Text);
					AssertEquals("None", form.GroupByGroupBox.Controls[2].Text);
					AssertEquals("Country", form.GroupByGroupBox.Controls[3].Text);
				}
			}
		}

		public void TestRuntimeOptionsFormComponentsAlignment()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TestRuntimeOptionsFormComponentsAlignment.xls", "TestRuntimeOptionsFormComponentsAlignment.xls");
			var template = new ExcelTemplateForUnitTesting("TestRuntimeOptionsFormComponentsAlignment.xls", Path.GetFullPath(tempFileName));

			using (var pack = new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())))
			using (var report = new Report(pack, template))
			{
				report.PrepareForRender();

				using (var testForm = GetNewForm(report))
				{
					testForm.Show();

					AssertEquals(4, report.SortOrderCollection.Count);
					AssertEquals(4, testForm.SortOrderGroupBox.Controls.Count);

					using (var defaultZRadioButton = new ZRadioButton())
					{
						foreach (ZRadioButton item in testForm.SortOrderGroupBox.Controls)
						{
							AssertEquals(defaultZRadioButton.Height, item.Height);
							AssertEquals(false, item.AutoSize);
						}
					}

					AssertEquals(5, report.OptionalTemplateSheetCollection.Count);
					AssertEquals(5, testForm.OptionalTemplatesGroupBox.Controls.Count);

					using (var defaultZCheckBox = new ZCheckBox())
					{
						foreach (ZCheckBox item in testForm.OptionalTemplatesGroupBox.Controls)
						{
							AssertEquals(defaultZCheckBox.Height, item.Height);
							AssertEquals(false, item.AutoSize);
						}
					}
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			form?.Dispose();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
			report?.Dispose();
		}

		void GetChildContainerControls(Control control, List<ContainerControl> containerControls)
		{
			var containerControl = control as ContainerControl;
			if (containerControl != null)
			{
				containerControls.Add(containerControl);
			}
			if (control.Controls != null)
			{
				foreach (Control childControl in control.Controls)
				{
					GetChildContainerControls(childControl, containerControls);
				}
			}
		}

		bool ContainsTestFilterValue(Control control)
		{
			foreach (Control subControl in control.Controls)
			{
				var textBox = subControl as TextBox;
				if (textBox != null && string.Compare(textBox.Text, "TestFilterValue", StringComparison.OrdinalIgnoreCase) == 0)
				{
					return true;
				}
				else if (ContainsTestFilterValue(subControl))
				{
					return true;
				}
			}
			return false;
		}

		int WidestGroupBoxInFilterGroups(AutoLayoutGroupBoxCollection filterGroups)
		{
			var result = 0;
			foreach (AutoLayoutGroupBox groupBox in filterGroups)
			{
				result = Math.Max(groupBox.DesiredWidth, result);
			}
			return result;
		}

		void AssertGroupsDisplayedCorrectly(bool errorExists, bool groupByExists, bool sortExists, bool filterExists, bool optionsExist)
		{
			var errorExistsAsString = errorExists ? "Y" : "N";
			var groupByExistsAsString = groupByExists ? "Y" : "N";
			var sortExistsAsString = sortExists ? "Y" : "N";
			var uDFExistsAsString = "N";
			var filterExistsAsString = filterExists ? "Y" : "N";
			var optionsExistAsString = optionsExist ? "Y" : "N";
			var templateName = "ErrorsFiltersSortsGroupBysUDFsOptions" + errorExistsAsString + filterExistsAsString + sortExistsAsString + groupByExistsAsString + uDFExistsAsString + optionsExistAsString + ".xls";

			var template = new ExcelTemplateForUnitTesting(templateName, TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template))
			using (RuntimeOptionsForm testForm = GetNewForm(report))
			{
				testForm.Show();
				if (errorExists)
				{
					AssertEquals("Errors group is visible - " + templateName, true, testForm.ErrorsGroupBox.Visible);
					AssertEquals("Sort group is invisible - " + templateName, false, testForm.SortOrderGroupBox.Visible);
					AssertEquals("Group by group is invisible - " + templateName, false, testForm.GroupByGroupBox.Visible);
					foreach (AutoLayoutGroupBox groupBox in testForm.FilterTabControl.FilterGroupBoxes)
					{
						AssertEquals("All filters should be invisible - " + templateName, false, groupBox.Visible);
					}
					AssertEquals("UDF group is invisible - " + templateName, false, testForm.UserDefinedFieldsGroupBox.Visible);
				}
				else
				{
					AssertEquals("Errors group is invisible - " + templateName, false, testForm.ErrorsGroupBox.Visible);
					if (filterExists)
					{
						AssertEquals("Filters group box should be invisible", true, testForm.FilterTabControl.FilterGroupBoxes[""].Visible);
					}
					else
					{
						AssertEquals("Filters group box should not exist ", 0, testForm.FilterTabControl.FilterGroupBoxes.Count);
					}
					AssertEquals("Filters tab control should be " + (filterExists ? "" : "in") + "visible", filterExists, testForm.FilterTabControl.Visible);
					AssertEquals("Sort group box should be " + (sortExists ? "" : "in") + "visible", sortExists, testForm.SortOrderGroupBox.Visible);
					AssertEquals("Group by group box should be " + (groupByExists ? "" : "in") + "visible", groupByExists, testForm.GroupByGroupBox.Visible);
					AssertEquals("Optional templates group should be " + (optionsExist ? "" : "in") + "visible", optionsExist, testForm.OptionalTemplatesGroupBox.Visible);
					if (testForm.NeedsToShow)
					{
						AssertEquals("Form should at least be as wide as the OK and Cancel buttons - " + templateName, true, testForm.ClientSize.Width > testForm.DockPadding.Left + testForm.CloseButton.Right - testForm.ActionButton.Left + testForm.DockPadding.Left);
					}
					var filtersTop = testForm.DockPadding.Top;
					var sortTop = filterExists ? testForm.FilterTabControl.Bottom : filtersTop;
					var groupByTop = sortExists ? testForm.SortOrderGroupBox.Bottom : sortTop;
					var optionsTop = groupByExists ? testForm.GroupByGroupBox.Bottom : groupByTop;
					var footerTop = optionsExist ? testForm.OptionalTemplatesGroupBox.Bottom : optionsTop;
					var statusTop = testForm.ClientSize.Height - (testForm.MainStatusBar.Height + testForm.DockPadding.Bottom);

					if (filterExists)
					{
						AssertEquals("group Should be correctly positoined", filtersTop, testForm.FilterTabControl.Top);
					}
					AssertEquals("group Should be correctly positoined for template - " + templateName, sortTop, testForm.SortOrderGroupBox.Top);
					AssertEquals("group Should be correctly positoined for template - " + templateName, groupByTop, testForm.GroupByGroupBox.Top);
					AssertEquals("group Should be correctly positoined for template - " + templateName, optionsTop, testForm.OptionalTemplatesGroupBox.Top);
					AssertEquals("group Should be correctly positoined for template - " + templateName, footerTop, testForm.FooterPanel.Top);
					AssertEquals("group Should be correctly positoined for template - " + templateName, statusTop, testForm.MainStatusBar.Top);
				}
			}
		}

		Control AssertControlsCollectionContainsControl(Control.ControlCollection controls, string controlName)
		{
			Control result = null;
			var containsControl = false;
			foreach (Control control in controls)
			{
				if (control.Name == controlName)
				{
					containsControl = true;
					result = control;
					break;
				}
			}
			AssertEquals("Control collection should contain control called " + controlName, true, containsControl);
			return result;
		}

		void InvokeLinkLabelClick(LinkLabel linkLabel)
		{
			linkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, linkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });
		}

		void AssertErrorControlsShown(RuntimeOptionsForm form, bool errorControlsShouldBeVisible)
		{
			form.Show();

			var errorControl = form.CopyButton;
			var nonErrorControl = form.ActionButton;
			if (errorControlsShouldBeVisible)
			{
				Assert(string.Format("Control {0} should be visible", errorControl.Name), errorControl.Visible);
				Assert(string.Format("Control {0} should NOT be visible", nonErrorControl.Name), !nonErrorControl.Visible);
			}
			else
			{
				Assert(string.Format("Control {0} should NOT be visible", errorControl.Name), !errorControl.Visible);
				Assert(string.Format("Control {0} should be visible", nonErrorControl.Name), nonErrorControl.Visible);
			}
		}

		ReportScheduleTask ScheduleTask
		{
			get
			{
				if (scheduleTask == null)
				{
					scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				}
				return scheduleTask;
			}
		}
		ReportScheduleTask scheduleTask;

		internal static RuntimeOptionsFormForTest GetNewForm(Report report, bool isErrorForm = false)
		{
			return new RuntimeOptionsFormForTest(report, AllowedDeliveryOptions.All, null, null, isErrorForm);
		}

		void AssertContainsColumnArrangementControls(ZTabControl.TabPageCollection tabPages, bool shouldContain)
		{
			var found = false;
			foreach (TabPage tabPage in tabPages)
			{
				if (tabPage.Text == "Configuration Management")
				{
					found = true;
					break;
				}
			}
			AssertEquals("Configuration Management controls visible", shouldContain, found);
		}

		RuntimeOptionsFormForTest form;
		Report report;

		RuntimeOptionsFormForTest Form => form ?? (form = GetNewForm(Report));

		Report Report => report ?? (report = new Report(new DocumentPack(ReportCommand), EmptyAndValidTemplate));

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly));

		ExcelTemplate crowdedFilterForm;
		ExcelTemplate CrowdedFilterForm
		{
			get
			{
				if (crowdedFilterForm == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.CrowdedFilterForm.xls", "CrowdedFilterForm.xls");
					crowdedFilterForm = new ExcelTemplateForUnitTesting("CrowdedFilterForm.xls", Path.GetFullPath(tempFileName));
				}
				return crowdedFilterForm;
			}
		}

		ExcelTemplate groupByPageWithContinuousPageStyle;
		ExcelTemplate GroupByPageWithContinuousPageStyle
		{
			get
			{
				if (groupByPageWithContinuousPageStyle == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.GroupByPageWithContinuousPageStyle.xls", "GroupByPageWithContinuousPageStyle.xls");
					groupByPageWithContinuousPageStyle = new ExcelTemplateForUnitTesting("GroupByPageWithContinuousPageStyle.xls", Path.GetFullPath(tempFileName));
				}
				return groupByPageWithContinuousPageStyle;
			}
		}

		ExcelTemplate lotsOfFiltersWithGroups;
		ExcelTemplate LotsOfFiltersWithGroups
		{
			get
			{
				if (lotsOfFiltersWithGroups == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.LotsOfFiltersWithGroups.xls", "LotsOfFiltersWithGroups.xls");
					lotsOfFiltersWithGroups = new ExcelTemplateForUnitTesting("LotsOfFiltersWithGroups.xls", Path.GetFullPath(tempFileName));
				}
				return lotsOfFiltersWithGroups;
			}
		}

		ExcelTemplate multipleFiltersWithGroups;
		ExcelTemplate MultipleFiltersWithGroups
		{
			get
			{
				if (multipleFiltersWithGroups == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleFiltersWithGroups.xls", "MultipleFiltersWithGroups.xls");
					multipleFiltersWithGroups = new ExcelTemplateForUnitTesting("MultipleFiltersWithGroups.xls", Path.GetFullPath(tempFileName));
				}
				return multipleFiltersWithGroups;
			}
		}

		ExcelTemplate multipleTemplatesWithAmpersand;
		ExcelTemplate MultipleTemplatesWithAmpersand
		{
			get
			{
				if (multipleTemplatesWithAmpersand == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleTemplatesWithAmpersand.xls", "MultipleTemplatesWithAmpersand.xls");
					multipleTemplatesWithAmpersand = new ExcelTemplateForUnitTesting("MultipleTemplatesWithAmpersand.xls", Path.GetFullPath(tempFileName));
				}
				return multipleTemplatesWithAmpersand;
			}
		}

		ExcelTemplate emptyAndValidTemplate;
		ExcelTemplate EmptyAndValidTemplate
		{
			get
			{
				if (emptyAndValidTemplate == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return emptyAndValidTemplate;
			}
		}

		StmTemplate template;
		StmTemplate Template
		{
			get
			{
				if (template == null)
				{
					template = Factory.New<StmTemplate>();
					template.SO_Template = EmptyAndValidTemplate.GetAsByteArray();
					template.SO_Name = "Template For Testing";
				}
				return template;
			}
		}

		ReportCommand reportCommand;
		ReportCommand ReportCommand
		{
			get
			{
				if (reportCommand == null)
				{
					reportCommand = Factory.New<ReportCommand>();
					reportCommand.SU_MenuName = "Template For Testing";

					var pivot = Factory.New<StmMenuTemplatePivot>();
					pivot.SI_SU = reportCommand.PK;
					pivot.SI_SO = Template.PK;
				}
				return reportCommand;
			}
		}

		CheckBox GetCheckBoxFromCollection(Control.ControlCollection controls, string name)
		{
			CheckBox checkBox = null;
			foreach (Control control in controls)
			{
				checkBox = control as CheckBox;
				if (checkBox != null)
				{
					if (checkBox.Name == name)
					{
						break;
					}
				}
			}
			return checkBox;
		}

		void AssertNoLonleyAmpersands(CheckBox checkBox)
		{
			AssertEquals("It shouldn't have space&space", -1, checkBox.Text.IndexOf(" & "));
			AssertEquals("It should have space&&space", 5, checkBox.Text.IndexOf(" && "));
		}
	}
}
