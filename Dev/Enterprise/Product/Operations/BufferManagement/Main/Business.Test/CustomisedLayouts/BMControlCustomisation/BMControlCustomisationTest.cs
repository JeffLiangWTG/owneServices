using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMControlCustomisation))]
	class BMControlCustomisationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAllPropertiesOnDefaultLayouts_ShouldResolveToRealProperties()
		{
			CombineAssertions(() =>
			{
				foreach (ICodeDescription cdp in new CustomisedControlTypeList())
				{
					var defaultLayout = BMControlCustomisation.GetNewDefaultCardLayout(Factory, cdp.Code);

					foreach (BMControlCustomisationLine field in defaultLayout.CustomisationLines)
					{
						var message = string.Format("Default control layout [{0}] has a property line named [{1}] which cannot be located on property source [{2}].", cdp.Description, field.PropertyName, field.PropertySource);
						AssertNotNull(message, field.Property);
					}
				}
			});
		}

		public void TestBackgroundImage()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.BackgroundImage = new ZBlob(new byte[] { 1, 2, 3, 4 });

			Factory.Save();

			var loadedCustomisation = new BusinessObjectFactory().Load<BMControlCustomisation>(customisation.PK);
			AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), loadedCustomisation.BackgroundImage);
		}

		public void TestLineItems_ShouldBeSerialisedWithCustomisation()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var line1 = customisation.CustomisationLines.AddNew();
			line1.PropertyName = ProcessTasksSchema.Constants.P9_Sequence;
			line1.Label = "Sequence";
			line1.AutoSize = true;

			var line2 = customisation.CustomisationLines.AddNew();
			line2.PropertyName = ProcessTasksSchema.Constants.P9_Status;
			line2.Label = "Status";

			var control1 = customisation.CustomisedControls.AddNew();
			control1.ControlType = StaticControlTypeList.Codes.CloseButton;

			Factory.Save();

			var loadedCustomisation = new BusinessObjectFactory().Load<BMControlCustomisation>(customisation.PK);

			AssertEquals(2, loadedCustomisation.CustomisationLines.Count);
			AssertEquals(ProcessTasksSchema.Constants.P9_Sequence, loadedCustomisation.CustomisationLines[0].PropertyName);
			AssertEquals("Sequence", loadedCustomisation.CustomisationLines[0].Label);
			AssertEquals(true, loadedCustomisation.CustomisationLines[0].AutoSize);

			AssertEquals(ProcessTasksSchema.Constants.P9_Status, loadedCustomisation.CustomisationLines[1].PropertyName);
			AssertEquals("Status", loadedCustomisation.CustomisationLines[1].Label);
			AssertEquals(false, loadedCustomisation.CustomisationLines[1].AutoSize);

			AssertEquals(1, loadedCustomisation.CustomisedControls.Count);
			AssertEquals(StaticControlTypeList.Codes.CloseButton, loadedCustomisation.CustomisedControls[0].ControlType);
		}

		public void TestLineItems_ShouldBeCloned()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			customisation.BackgroundColor = Color.Yellow.Name;

			var line1 = customisation.CustomisationLines.AddNew();
			line1.PropertyName = ProcessTasksSchema.Constants.P9_Sequence;
			line1.Label = "Sequence";

			var line2 = customisation.CustomisationLines.AddNew();
			line2.PropertyName = ProcessTasksSchema.Constants.P9_Status;
			line2.Label = "Status";

			var control1 = customisation.CustomisedControls.AddNew();
			control1.ControlType = StaticControlTypeList.Codes.CloseButton;

			var clonedCustomisation = (BMControlCustomisation)customisation.Clone();
			AssertEquals(Color.Yellow.Name, clonedCustomisation.BackgroundColor);

			AssertEquals(2, clonedCustomisation.CustomisationLines.Count);
			AssertEquals(ProcessTasksSchema.Constants.P9_Sequence, clonedCustomisation.CustomisationLines[0].PropertyName);
			AssertEquals("Sequence", clonedCustomisation.CustomisationLines[0].Label);
			AssertEquals(ProcessTasksSchema.Constants.P9_Status, clonedCustomisation.CustomisationLines[1].PropertyName);
			AssertEquals("Status", clonedCustomisation.CustomisationLines[1].Label);

			AssertEquals(1, clonedCustomisation.CustomisedControls.Count);
			AssertEquals(StaticControlTypeList.Codes.CloseButton, clonedCustomisation.CustomisedControls[0].ControlType);
		}

		public void TestPreview()
		{
			var expectedCustomisationCount = 0;
			var receiver = new DummyPreviewReceiver();
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.PreviewReceiver = receiver;

			AssertEquals(expectedCustomisationCount, receiver.PreviewUpdatedCount);

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			AssertEquals(++expectedCustomisationCount, receiver.PreviewUpdatedCount);

			var line1 = customisation.CustomisationLines.AddNew();
			expectedCustomisationCount += 2;
			AssertEquals(expectedCustomisationCount, receiver.PreviewUpdatedCount);

			line1.PropertySource = PropertySourceList.Codes.Workflow;
			AssertEquals(++expectedCustomisationCount, receiver.PreviewUpdatedCount);

			line1.PropertyName = ProcessHeaderSchema.FH_CompletionStatement.Name;
			AssertEquals("Setting property name also sets label and property type", expectedCustomisationCount += 3, receiver.PreviewUpdatedCount);

			line1.PropertyName = ProcessHeaderSchema.FH_CompletionStatement.Name;
			AssertEquals("Should not refresh preview when same value set", expectedCustomisationCount, receiver.PreviewUpdatedCount);
		}

		public void TestPreview_ValidationErrors_ShouldHaveErrorsAtTimeOfUpdate()
		{
			var expectedCustomisationCount = 0;
			var receiver = new DummyPreviewReceiverWithErrorVerification();
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.PreviewReceiver = receiver;

			var line1 = customisation.CustomisationLines.AddNew();
			AssertEquals(++expectedCustomisationCount, receiver.PreviewUpdatedCount);

			receiver.Property = line1.PropertySourceInfo;
			receiver.ExpectedPropertyError = "Enter a valid Source.";

			line1.PropertySource = "ZZZ";
			AssertEquals(++expectedCustomisationCount, receiver.PreviewUpdatedCount);

			receiver.ExpectedPropertyError = null;

			line1.PropertySource = PropertySourceList.Codes.Workflow;
			AssertEquals(++expectedCustomisationCount, receiver.PreviewUpdatedCount);
		}

		public void TestAppendNameForClone()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_Name = "Boo";

			var clone1 = (BMControlCustomisation)customisation.Clone();
			AssertEquals("Boo - Copy", clone1.FM_Name);

			var clone2 = (BMControlCustomisation)customisation.Clone();
			AssertEquals("Boo - Copy 1", clone2.FM_Name);

			var clone3 = (BMControlCustomisation)customisation.Clone();
			AssertEquals("Boo - Copy 2", clone3.FM_Name);
		}

		public void TestGetCustomisedDetailedCard_ShouldNotHitBMSystemTable()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var buffer = config.BufferSection;
			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Test Factory" };
			var loadedSection = newFactory.Load<BMBoardSection>(buffer.PK);

			using (AssertDbHitsForAllFactories(new Dictionary<string, int>
			{
				{ BMSystemSchema.Constants.TableName, 0 },
			}, ignoreUnspecified: true, thresholdForUnspecified: 50))
			{
				BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, "INQ");
			}
		}

		#region Detailed card

		public void TestGetDefaultDetailedTaskCard()
		{
			var defaultCard = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);

			AssertEquals(Color.White, defaultCard.BackgroundColorValue);
			AssertEquals(350, defaultCard.Width);
			AssertEquals(238, defaultCard.Height);

			defaultCard.Validation.ValidateAll();

			var lines = new Dictionary<(string, string), BMControlCustomisationLine>();
			foreach (BMControlCustomisationLine line in defaultCard.CustomisationLines)
			{
				line.Validation.ValidateAll();
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Line error on {0}", line.PropertyName), false, line.HasErrors);

				if (line.PropertySource != PropertySourceList.Codes.Job)
				{
					var property = line.Property;
					AssertNotNull(string.Format("Should be a valid property for {0}.{1}", line.PropertySource, line.PropertyName), property);
				}

				lines.Add((line.PropertySource, line.PropertyName), line);
			}

			var controls = new Dictionary<string, StaticControlCustomisation>();
			foreach (StaticControlCustomisation control in defaultCard.CustomisedControls)
			{
				control.Validation.ValidateAll();
				AssertEquals(false, control.HasErrors);

				controls.Add(control.ControlType, control);
			}

			AssertEquals(false, defaultCard.HasErrors);

			var taskStatusIndicator = controls[StaticControlTypeList.Codes.TaskStatusIndicator];
			AssertEquals(3, taskStatusIndicator.Top);

			var jobNumberField = lines[(PropertySourceList.Codes.Workflow, "ProviderJobNumber")];
			AssertEquals(30, jobNumberField.Left);
			AssertEquals(true, jobNumberField.AutoSize);

			var jobDescriptionField = lines[(PropertySourceList.Codes.Workflow, "ProviderJobDescription")];
			AssertEquals(110, jobDescriptionField.Left);

			var closeButton = controls[StaticControlTypeList.Codes.CloseButton];
			AssertEquals(323, closeButton.Left);

			var statusField = lines[(PropertySourceList.Codes.ProcessTask, "StatusDescription")];
			AssertEquals("Status", statusField.Label);
			AssertEquals(25, statusField.Top);

			AssertEquals("Should not use CardStatusDescription", false, lines.ContainsKey((PropertySourceList.Codes.ProcessTask, "CardStatusDescription")));

			var workflowField = lines[(PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement)];
			AssertEquals("Workflow", workflowField.Label);
			AssertEquals(50, workflowField.Top);

			var taskField = lines[(PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_Description)];
			AssertEquals("Task:", taskField.Label);
			AssertEquals(75, taskField.Top);

			var estimateField = lines[(PropertySourceList.Codes.ProcessTask, "EstimateDescription")];
			AssertEquals("Estimate", estimateField.Label);
			AssertEquals(100, estimateField.Top);

			var handoverField = lines[(PropertySourceList.Codes.ProcessTask, "EstimatedHandoverTimeLocal")];
			AssertEquals("Handover:", handoverField.Label);
			AssertEquals(125, handoverField.Top);

			AssertEquals("Should not have Time Left field", false, lines.ContainsKey((PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_EstimatedTimeToComplete)));

			AssertEquals("Should not have Delivery Date field", false, lines.ContainsKey((PropertySourceList.Codes.Workflow, "AgreedDeliveryDateLocal")));

			AssertEquals("Should not have Nudge Controls", false, controls.ContainsKey(StaticControlTypeList.Codes.NudgeControls));

			AssertEquals("Should not have Date Acceptability Picture", false, controls.ContainsKey(StaticControlTypeList.Codes.DateAcceptabilityPicture));

			var reminderField = lines[(PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_ScheduledDate)];
			AssertEquals("Reminder:", reminderField.Label);
			AssertEquals(150, reminderField.Top);

			var reminderCheckbox = lines[(PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_IsCalendarItem)];
			AssertEquals("Add to Calendar", reminderCheckbox.Label);
			AssertEquals(150, reminderCheckbox.Top);
			AssertEquals(200, reminderCheckbox.Left);

			var cardNoteField = lines[(PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_CardNote)];
			AssertEquals("Card Note:", cardNoteField.Label);
			AssertEquals(175, cardNoteField.Top);

			var tagLabel = controls[StaticControlTypeList.Codes.Label];
			AssertEquals("Tag:", tagLabel.Label);
			AssertEquals(ControlAlignmentList.Codes.Right, tagLabel.Alignment);

			var tagIndicator = controls[StaticControlTypeList.Codes.AttachedTagsIndicator];
			AssertEquals(200, tagIndicator.Top);

			var claimButton = controls[StaticControlTypeList.Codes.CapabilityAssignmentButton];
			AssertEquals(214, claimButton.Top);
			AssertEquals(4, claimButton.Left);

			var statusButtons = controls[StaticControlTypeList.Codes.StatusButtons];
			AssertEquals(214, statusButtons.Top);

			var openButton = controls[StaticControlTypeList.Codes.OpenJobButton];
			AssertEquals("Open", openButton.Label);
			AssertEquals(214, openButton.Top);
			AssertEquals(215, openButton.Left);

			var saveButton = controls[StaticControlTypeList.Codes.SaveButton];
			AssertEquals("Save", saveButton.Label);
			AssertEquals(214, saveButton.Top);
			AssertEquals(280, saveButton.Left);
		}

		public void TestGetCustomisedDetailedCard()
		{
			var group = Factory.New<GlbGroup>();

			var system = Factory.New<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var systemDetailedCard = Factory.New<BMControlCustomisation>();
			var releaseGroupDetailedCard = Factory.New<BMControlCustomisation>();
			var boardDetailedCard = Factory.New<BMControlCustomisation>();
			var sectionDetailedCard = Factory.New<BMControlCustomisation>();
			systemDetailedCard.FM_ControlType = releaseGroupDetailedCard.FM_ControlType = boardDetailedCard.FM_ControlType = sectionDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, system, systemDetailedCard);
			AssertEquals(systemDetailedCard, BMControlCustomisation.GetCustomisedDetailedCard(section.SectionConfiguration, string.Empty));

			BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, releaseGroupDetailedCard);
			AssertEquals(releaseGroupDetailedCard, BMControlCustomisation.GetCustomisedDetailedCard(section.SectionConfiguration, string.Empty));

			BMSTestHelper.CreateControlCustomisationLink(Factory, board, boardDetailedCard);
			AssertEquals(boardDetailedCard, BMControlCustomisation.GetCustomisedDetailedCard(section.SectionConfiguration, string.Empty));

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, sectionDetailedCard);
			AssertEquals(sectionDetailedCard, BMControlCustomisation.GetCustomisedDetailedCard(section.SectionConfiguration, string.Empty));
		}

		public void TestGetCustomisedDetailedCard_ForNonExistentReleaseGroup()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.SectionConfiguration.ReleaseGroupPK = ZGuid.NewZGuid();

			AssertNull(BMControlCustomisation.GetCustomisedDetailedCard(section.SectionConfiguration, string.Empty));
		}

		public void TestGetCustomisedSummaryCard()
		{
			var group = Factory.New<GlbGroup>();

			var system = Factory.New<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var systemSummaryCard = Factory.New<BMControlCustomisation>();
			var releaseGroupSummaryCard = Factory.New<BMControlCustomisation>();
			var boardSummaryCard = Factory.New<BMControlCustomisation>();
			var sectionSummaryCard = Factory.New<BMControlCustomisation>();
			systemSummaryCard.FM_ControlType = releaseGroupSummaryCard.FM_ControlType = boardSummaryCard.FM_ControlType = sectionSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, system, systemSummaryCard);
			AssertEquals(systemSummaryCard, BMControlCustomisation.GetCustomisedSummaryCard(section.SectionConfiguration, string.Empty));

			BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, releaseGroupSummaryCard);
			AssertEquals(releaseGroupSummaryCard, BMControlCustomisation.GetCustomisedSummaryCard(section.SectionConfiguration, string.Empty));

			BMSTestHelper.CreateControlCustomisationLink(Factory, board, boardSummaryCard);
			AssertEquals(boardSummaryCard, BMControlCustomisation.GetCustomisedSummaryCard(section.SectionConfiguration, string.Empty));

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, sectionSummaryCard);
			AssertEquals(sectionSummaryCard, BMControlCustomisation.GetCustomisedSummaryCard(section.SectionConfiguration, string.Empty));
		}

		#endregion

		#region Indexes

		public void TestFM_ControlType_ForSystemWideCustomisation_ShouldUseNaturalKeyCache()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_IsSystemWide = true;
			customisation.FM_JobType = "WKI";
			customisation.FM_ControlType = "TSK";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedCustomisation = newFactory.LoadFromNaturalKey<BMControlCustomisation>(BMControlCustomisationSchema.FM_ControlType, "TSK");
			AssertNotNull(loadedCustomisation);

			loadedCustomisation.FM_ControlType = "WFL";

			var isNkInCache = newFactory.IsInNaturalKeyCache_ForTest(typeof(BMControlCustomisation), BMControlCustomisationSchema.FM_ControlType, new ZString("WFL"));
			AssertEquals("The customisation is system-wide, so the NK cache should be updated if the control type changes. SAD!", true, isNkInCache);
		}

		public void TestFM_ControlType_ForNonSystemWideCustomisation_ShouldNotAffectNaturalKeyCache()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_IsSystemWide = false;
			customisation.FM_JobType = "WKI";
			customisation.FM_ControlType = "TSK";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedCustomisation = newFactory.LoadFromNaturalKey<BMControlCustomisation>(BMControlCustomisationSchema.FM_ControlType, "TSK");
			AssertNotNull(loadedCustomisation);

			loadedCustomisation.FM_ControlType = "WFL";

			var isNkInCache = newFactory.IsInNaturalKeyCache_ForTest(typeof(BMControlCustomisation), BMControlCustomisationSchema.FM_ControlType, new ZString("WFL"));
			AssertEquals("The customisation is not system-wide, so the NK cache should not be updated if the control type changes. SAD!", false, isNkInCache);
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var layout = Factory.NewWithValidTestData<BMControlCustomisation>();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, layout.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			layout.FM_Name = "New name";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			layout.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		class DummyPreviewReceiver : IPreviewReceiver
		{
			public virtual void UpdatePreview()
			{
				PreviewUpdatedCount++;
			}

			internal int PreviewUpdatedCount { get; private set; }
		}

		class DummyPreviewReceiverWithErrorVerification : DummyPreviewReceiver
		{
			internal ZPropertyInfo Property { get; set; }
			internal string ExpectedPropertyError { get; set; }

			public override void UpdatePreview()
			{
				base.UpdatePreview();

				if (Property != null && !string.IsNullOrEmpty(ExpectedPropertyError))
				{
					Assert(Property.HasError(ExpectedPropertyError));
				}
			}
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "BackgroundColor";
				yield return "BackgroundImage";
				yield return "CustomisationLines";
				yield return "CustomisedControls";
				yield return "Height";
				yield return "Width";
				yield return "RenderOnTheWeb";
			}
		}

		#endregion
	}
}
