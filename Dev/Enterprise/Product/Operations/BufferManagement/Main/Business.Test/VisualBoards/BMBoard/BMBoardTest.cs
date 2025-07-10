using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.PAVE.Common.Model;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoard))]
	public class BMBoardTest : EnterpriseBusinessObjectTestCase
	{
		#region Clone

		public void TestClone_ShouldCloneCustomisedLayoutLinks()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);

			var layout1 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var layout2 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var layout3 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard);
			var layout4 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard);

			BMSTestHelper.CreateControlCustomisationLink(Factory, board, layout1);
			BMSTestHelper.CreateControlCustomisationLink(Factory, board, layout2);
			BMSTestHelper.CreateControlCustomisationLink(Factory, board, layout3);
			BMSTestHelper.CreateControlCustomisationLink(Factory, board, layout4);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBoard = newFactory.Load<BMBoard>(board.PK);
			var clone = (BMBoard)loadedBoard.Clone();

			AssertEquals(4, clone.CustomisedLayoutLinks.Count);

			AssertCollectionContains(clone.CustomisedLayoutLinks, l => l.FML_FM_ControlCustomisation == layout1.PK);
			AssertCollectionContains(clone.CustomisedLayoutLinks, l => l.FML_FM_ControlCustomisation == layout2.PK);
			AssertCollectionContains(clone.CustomisedLayoutLinks, l => l.FML_FM_ControlCustomisation == layout3.PK);
			AssertCollectionContains(clone.CustomisedLayoutLinks, l => l.FML_FM_ControlCustomisation == layout4.PK);
		}

		#endregion

		public void TestHumanReadableName()
		{
			var board = Factory.New<BMBoard>();
			board.MB_Name = "Dat board";

			AssertEquals("Dat board", board.HumanReadableName);
		}

		public void TestIsGlobal()
		{
			var board = Factory.New<BMBoard>();

			board.IsGlobal = true;
			AssertEquals(ZGuid.Empty, board.MB_GC_Company);

			board.IsGlobal = false;
			AssertEquals(GlbCompany.CurrentCompany.PK, board.MB_GC_Company);
		}

		public void TestDefaultValues_ShouldAssignToSelf()
		{
			var board = Factory.New<BMSystem>().Boards.AddNew();
			AssertEquals(GlbStaff.CurrentUser.GS_Code, board.MB_GS_NKStaffCode);
		}

		public void TestSections()
		{
			var board = Factory.New<BMBoard>();
			var section1 = Factory.New<BMBoardSection>();
			var section2 = Factory.New<BMBoardSection>();
			section1.MS_MB_Board = board.PK;

			AssertEquals(1, board.Sections.Count);
			AssertCollectionContains(section1, board.Sections);
		}

		public void TestSlideshows()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var board = BMSTestHelper.CreateBoard(system);
			AssertEquals(0, board.SlideshowPivots.Count);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board);

			AssertEquals(1, board.SlideshowPivots.Count);
			AssertEquals(slideshow.PK, board.SlideshowPivots.Single().MC_MD_Slideshow);
			AssertEquals(board.PK, board.SlideshowPivots.Single().MC_MB_Board);
		}

		public void TestCodeDescriptions()
		{
			var board = Factory.New<BMBoard>();
			board.MB_Name = "Mai Board";
			board.MB_Description = "All your board are belong to us";

			AssertEquals("Mai Board", ((ICodeDescription)board).Code);
			AssertEquals("All your board are belong to us", ((ICodeDescription)board).Description);
		}

		public void TestBufferManagementVisualBoardProvider()
		{
			var board = Factory.New<BMBoard>();
			board.MB_Name = "Rylan's Board";
			board.MB_Description = "Zayden will be angry that i named this after his brother";

			var boardProvider = (IVisualBoardProvider)board;
			AssertEquals(board.MB_Name, boardProvider.Name);
			AssertEquals(board.MB_Description, boardProvider.Description);
			AssertEquals(board.PK, boardProvider.PK);
			AssertEquals(1, boardProvider.Boards.Count());
			AssertEquals(board.PK, boardProvider.Boards.First().BoardPK);
		}

		public void TestDetailedCardPK()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var customisedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, board, customisedCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(newFactory.Load<BMBoardSection>(section.PK).SectionConfiguration, string.Empty);
			AssertNotNull(loadedDetailedCard);
			AssertEquals(customisedCard.PK, loadedDetailedCard.PK);
		}

		public void TestCustomisedCards_Workflow()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var workflowDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			var workflowSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, board, workflowDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, board, workflowSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var loadedWorkflowDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedWorkflowSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedWorkflowDetailedCard);
			AssertNotNull(loadedWorkflowSummaryCard);

			AssertEquals(workflowDetailedCard.PK, loadedWorkflowDetailedCard.PK);
			AssertEquals(workflowSummaryCard.PK, loadedWorkflowSummaryCard.PK);
		}

		public void TestCustomisedCards_Task()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			var taskDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			taskDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			var taskSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			taskSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, board, taskDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, board, taskSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var loadedTaskDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedTaskSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedTaskDetailedCard);
			AssertNotNull(loadedTaskSummaryCard);

			AssertEquals(taskDetailedCard.PK, loadedTaskDetailedCard.PK);
			AssertEquals(taskSummaryCard.PK, loadedTaskSummaryCard.PK);
		}

		[TestDate(2014, 1, 02)]
		public void TestOnChannelAddBoardEditDateUpdates()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "systemname";
			var section = system.Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, channel1.Section.Board.MB_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			BMSTestHelper.CreatePrimaryChannelForSection(section);

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, channel1.Section.Board.MB_SystemLastEditTimeUtc);
		}

		[TestDate(2014, 1, 02)]
		public void TestOnSectionAddBoardEditDateUpdates()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "systemname2";
			var board = system.Boards.AddNew();

			Factory.Save();
			var section = board.Sections.AddNew();

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, section.Board.MB_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			system.Boards.FirstOrDefault().Sections.AddNew();

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, section.Board.MB_SystemLastEditTimeUtc);
		}

		[TestDate(2014, 1, 02)]
		public void TestOnChannelDeleteBoardEditDateUpdates()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "systemname";
			var section = system.Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, channel1.Section.Board.MB_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			BMBoardSectionTestHelper.RemoveAndDeleteAllPrimaryAxisChannels(section);

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, section.Board.MB_SystemLastEditTimeUtc);
		}

		[TestDate(2014, 1, 02)]
		public void TestOnSectionDeleteBoardEditDateUpdates()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "systemname";
			var section = system.Boards.AddNew().Sections.AddNew();

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, section.Board.MB_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			system.Boards.FirstOrDefault().Sections.DeleteAll();

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, system.Boards.FirstOrDefault().MB_SystemLastEditTimeUtc);
		}

		public void TestHasChangesOnSectionConfigChange()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			Factory.Save();

			AssertEquals(false, board.HasChanges);

			section.SectionConfiguration.BufferZone0Color = ColorList.NameFromColor(Color.Blue);

			AssertEquals(true, board.HasChanges);

			//Test that there is nothing wrong with regular ActiveBusinessObjectCollection.
			var collection = new ActiveBusinessObjectCollection<BMBoardSection>(Factory.CreateNewFactory());
			var loadedSection1 = collection.Single(t => t.PK == section.PK);
			AssertEquals(false, ((IBusiness)collection).HasChanges);
			loadedSection1.SectionConfiguration.BufferZone0Color = ColorList.NameFromColor(Color.Beige);
			AssertEquals(true, ((IBusiness)collection).HasChanges);

			//Test that there is nothing wrong with regular ActiveBusinessObjectCollection xtended constructor.
			var factoryX = Factory.CreateNewFactory();
			var collection2 = new BMBoardSectionCollection(factoryX.Load<BMBoard>(board.PK));
			var loadedSection21 = collection2.Single(t => t.PK == section.PK);
			AssertEquals(false, ((IBusiness)collection2).HasChanges);
			loadedSection21.SectionConfiguration.BufferZone0Color = ColorList.NameFromColor(Color.Beige);
			AssertEquals(true, loadedSection21.HasChanges);
			loadedSection21.MS_SectionType = "TOO";
			loadedSection21.HasChanges = true;
			AssertEquals(true, ((IBusiness)collection2).HasChanges);

			//Test that there is nothing wrong with custom ActiveBusinessObjectCollection
			var factory = Factory.CreateNewFactory();
			var loadedBoard = factory.Load<BMBoard>(board.PK);
			AssertEquals(false, loadedBoard.HasChanges);

			var loadedSection2 = loadedBoard.Sections.First();
			loadedSection2.RowHeightPercent = 7;
			loadedSection2.SectionConfiguration.BufferZone0Color = ColorList.NameFromColor(Color.Beige);
			loadedSection2.MS_LayoutData = "sh";

			AssertEquals(true, ((IBusiness)loadedBoard.Sections).HasChanges);
			AssertEquals(true, loadedBoard.HasChanges);
		}

		#region Delete

		[ExpectNoExceptions]
		public void TestDelete()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket2, board);
			section.MS_SectionType = "CMP";

			Factory.Save();
			board.Delete();
		}

		[ExpectNoExceptions]
		public void TestDelete_WhenSectionsNotAlreadyLoaded()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BufferBoard.PK;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBoard = newFactory.Load<BMBoard>(config.BufferBoard.PK);

			loadedBoard.Delete();
			newFactory.Save();
		}

		public void TestDelete_ShouldDeleteLayoutLinks()
		{
			var board = BMSTestHelper.CreateBoard(BMSTestHelper.CreateSystem(Factory));
			var layout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var link = BMSTestHelper.CreateControlCustomisationLink(Factory, board, layout);

			Factory.Save();

			var loadedBoard = Factory.CreateNewFactory().Load<BMBoard>(board.PK);
			loadedBoard.Delete();
			loadedBoard.Factory.Save();

			AssertEquals(true, link.IsDeleted);
		}

		public void TestDelete_ShouldDeleteSlideshowPivots()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "First Board");
			var board2 = BMSTestHelper.CreateBoard(system, "Second Board");
			var slideshow1 = BMSTestHelper.CreateSlideshow(Factory, board1);
			var slideshow2 = BMSTestHelper.CreateSlideshow(Factory, board1);
			slideshow1.MD_Name = "First Slideshow";
			slideshow2.MD_Name = "Second Slideshow";

			var board1Slideshow1Pivot = slideshow1.BoardPivots.Single();
			var board1Slideshow2Pivot = slideshow2.BoardPivots.Single();

			var board2Slideshow1Pivot = Factory.New<BMBoardSlideshowPivot>();
			board2Slideshow1Pivot.MC_MB_Board = board2.PK;
			board2Slideshow1Pivot.MC_MD_Slideshow = slideshow1.PK;

			Factory.Save();
			board1.Delete();

			Assert("The board was deleted so its link to slideshow 1 should have been deleted as well.", board1Slideshow1Pivot.IsDeleted);
			Assert("The board was deleted so its link to slideshow 2 should have been deleted as well.", board1Slideshow2Pivot.IsDeleted);
			Assert("Slideshow 1 should not be deleted becasuse there is an undeleted board linked.", !slideshow1.IsDeleted);
			Assert("Slideshow 2 should have been deleted because its last board was deleted.", slideshow2.IsDeleted);
			Assert("The link between board 2 and slideshow 1 should not be deleted just because another board linked to the slideshow was deleted.", !board2Slideshow1Pivot.IsDeleted);
		}

		#endregion

		public void TestOnSaving_ErrorWhenBoardBelongsToOtherCompanies()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");

			var myGlobalBoard = BMSTestHelper.CreateBoard(system, "My global board");
			myGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myGlobalBoard.MB_GC_Company = ZGuid.Empty;

			var myNonGlobalBoard = BMSTestHelper.CreateBoard(system, "My non-global board");
			myNonGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myNonGlobalBoard.MB_GC_Company = GlbCompany.CurrentCompany.PK;

			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			var otherDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_GB_HomeBranch = otherBranch.PK;
			otherStaff.GS_GE_HomeDepartment = otherDepartment.PK;

			Factory.Save();

			BMBoard otherStaffNonGlobalBoard;
			using (Env.SetTemporaryUserContext(otherStaff.PK.ToGuid(), otherBranch.PK.ToGuid(), otherDepartment.PK.ToGuid()))
			{
				otherStaffNonGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's non-global board");
				otherStaffNonGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffNonGlobalBoard.MB_GC_Company = otherBranch.GB_GC;

				Factory.Save();
			}

			myGlobalBoard.MB_Description = myGlobalBoard.MB_Name;
			AssertNoExceptionThrown("This board is global.", () => Factory.Save());

			myNonGlobalBoard.MB_Description = myNonGlobalBoard.MB_Name;
			AssertNoExceptionThrown("This board belongs to the current company.", () => Factory.Save());

			otherStaffNonGlobalBoard.MB_Description = otherStaffNonGlobalBoard.MB_Name;
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			Factory.Save();
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals($"User {GlbStaff.CurrentUser.PK} should not update non-global Board {otherStaffNonGlobalBoard.PK} belonging to another Company {(ZGuid)otherStaffNonGlobalBoard.MB_GC_CompanyInfo.OriginalValue}. (Current Company: {GlbCompany.CurrentCompany.PK})", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestOnSaving_ErrorWhenCompanyIsNotEmptyOrCurrentCompany()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var board = BMSTestHelper.CreateBoard(system);
			var company = Factory.NewWithValidTestData<GlbCompany>();

			board.MB_GC_Company = ZGuid.Empty;
			AssertNoExceptionThrown("This board is global.", () => Factory.Save());

			board.MB_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertNoExceptionThrown("This board belongs to the current company.", () => Factory.Save());

			board.MB_GC_Company = company.PK;
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			Factory.Save();
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals($"Board {board.PK} should not belong to Company {board.MB_GC_Company}. (Current Company: {GlbCompany.CurrentCompany.PK}, Current User: {GlbStaff.CurrentUser.PK})", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSystem_ChangingSystemWillNotInvalidateBoardSectionComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var anotherSystem = BMSTestHelper.CreateSystem(Factory, "ANT");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);

			Factory.Save();

			AssertEquals(0, board.Sections.Count);

			var originalSystemPK = board.MB_FS_System;
			board.MB_FS_System = ZGuid.NewZGuid();

			AssertNoErrors(board);

			board.MB_FS_System = originalSystemPK;
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			Factory.Save();

			AssertEquals(1, board.Sections.Count);

			board.MB_FS_System = anotherSystem.PK;

			AssertNoErrors(board);
		}

		#region Centralized Cache

		public void TestToModel()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var board = system.Boards.AddNew();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = BMSTestHelper.CreateCapability(Factory, description: "Capability");
			var tagMag = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(Factory, "FLP"), "TAG", description: "Tag Magnitude");
			var staff = BMSTestHelper.CreateStaff(Factory, "STF", "Staff");

			var bufferSection = BMSTestHelper.CreateBoardSection(buffer, board, customReleaseGroupPK: group.PK, row: 0, col: 0, rowHeightPercent: 90, colWidthPercent: 80, cellsPerSubsection: 10);
			bufferSection.BackgroundColor = Color.Yellow.ToString();
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staff.PK, displaySequence: 5);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Group, group.PK, displaySequence: 4);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Capability, capability.PK, displaySequence: 3);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Tag, tagMag.PK, displaySequence: 2);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.NotChanneled, Guid.Empty, displaySequence: 1);
			bufferSection.SectionConfiguration.BufferZone0Color = Color.CadetBlue.ToString();
			bufferSection.SectionConfiguration.BufferZone1Color = Color.Gainsboro.ToString();
			bufferSection.SectionConfiguration.BufferZone2Color = Color.DarkOrange.ToString();
			bufferSection.SectionConfiguration.BufferZone3Color = Color.Honeydew.ToString();

			var bucketSection = BMSTestHelper.CreateBoardSection(bucket, board, customReleaseGroupPK: group.PK, row: 0, col: 1, rowHeightPercent: 50, colWidthPercent: 40, cellsPerSubsection: 4);
			bucketSection.BackgroundColor = Color.Purple.ToString();
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Resource, staff.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Group, group.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Capability, capability.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Tag, tagMag.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.NotChanneled, Guid.Empty);

			BMSTestHelper.CreateAdditionalComponent(bucketSection, BMSTestHelper.CreateBucket(bucket.System, "additionalBucket"));

			Factory.Save();

			var boardModel = board.ToModel();

			CombineAssertions(() =>
			{
				AssertEquals(board.PK.ToGuid(), boardModel.PK);
				AssertEquals(board.Sections.Count, boardModel.Sections.Count());

				foreach (var section in board.Sections)
				{
					var sectionModel = boardModel.Sections.First(s => s.PK == section.PK.ToGuid());

					AssertEquals(board.PK.ToGuid(), sectionModel.BoardPK);
					AssertEquals(section.MS_SectionType, sectionModel.SectionType);
					AssertEquals(section.SectionName, sectionModel.Name);

					AssertEquals(section.Row, sectionModel.Layout.Row);
					AssertEquals(section.Column, sectionModel.Layout.Column);
					AssertEquals(section.RowSpan, sectionModel.Layout.RowSpan);
					AssertEquals(section.ColSpan, sectionModel.Layout.ColSpan);
					AssertEquals(section.RowHeightPercent, sectionModel.Layout.RowHeightPercent);
					AssertEquals(section.ColWidthPercent, sectionModel.Layout.ColWidthPercent);

					AssertType<ComponentSection>(sectionModel);
					var componentSectionModel = sectionModel;

					AssertEquals(section.AllComponents.Count(), componentSectionModel.Components.Count());

					foreach (var component in section.AllComponents)
					{
						var componentModel = componentSectionModel.Components.First(c => c.PK == component.PK.ToGuid());

						AssertEquals(component.FC_Name, componentModel.Name);
						AssertEquals(component.FC_BufferTimespanInMinutes, componentModel.SizeInMinutes);
						AssertEquals(component.FC_OffsetInMinutes, componentModel.OffsetInMinutes);
					}

					IBranchDepartmentProvider branchDepartmentProvider = section;
					AssertEquals(branchDepartmentProvider.GetBranch(Factory).PK.ToGuid(), componentSectionModel.BranchPK);
					AssertEquals((branchDepartmentProvider.GetDepartment(Factory) as BusinessObject).PK.ToGuid(), componentSectionModel.DepartmentPK);

					var sectionConfiguration = section.SectionConfiguration;

					AssertEquals(sectionConfiguration.IsBucket, componentSectionModel.IsBucket);
					AssertEquals(sectionConfiguration.IsBuffer, componentSectionModel.IsBuffer);
					AssertEquals(sectionConfiguration.ChannelBy, componentSectionModel.ChannelBy);
					AssertEquals(sectionConfiguration.OverrideChannels, componentSectionModel.OverrideChannels);
					AssertEquals(sectionConfiguration.OrderedPrimaryAxisChannels.Count(), componentSectionModel.Channels.Count());

					AssertEquals(sectionConfiguration.Subsections, componentSectionModel.SubSections);
					AssertEquals(sectionConfiguration.CellsPerSubsection, componentSectionModel.CellsPerSubSection);
					AssertEquals(sectionConfiguration.FlowDirection, componentSectionModel.FlowDirection);
					AssertEquals(sectionConfiguration.LastCell, componentSectionModel.LastCellPosition);
					AssertEquals(sectionConfiguration.MaxOverdueSlots, componentSectionModel.MaxOverdueSlots);
					AssertEquals(sectionConfiguration.FadeBackgroundAtPercentage, componentSectionModel.FadeBackgroundAtPercentage);
					AssertEquals(sectionConfiguration.IsReleaseScheduler, componentSectionModel.IsReleaseScheduler);
					AssertEquals(sectionConfiguration.EnableShowCurrentItemsFilterByDefault, componentSectionModel.EnableShowCurrentItemsFilterByDefault);
					AssertEquals(sectionConfiguration.ShowZones, componentSectionModel.ShowZones);
					AssertEquals(section.BackgroundColorValue.ToHex(), componentSectionModel.BackgroundColor);

					if (componentSectionModel.IsBuffer)
					{
						AssertEquals(4, componentSectionModel.BufferZonesColors.Count);
						AssertEquals(section.GetZoneColor(0).ToHex(), componentSectionModel.BufferZonesColors[0]);
						AssertEquals(section.GetZoneColor(1).ToHex(), componentSectionModel.BufferZonesColors[1]);
						AssertEquals(section.GetZoneColor(2).ToHex(), componentSectionModel.BufferZonesColors[2]);
						AssertEquals(section.GetZoneColor(3).ToHex(), componentSectionModel.BufferZonesColors[3]);
					}

					if (componentSectionModel.IsBucket)
					{
						AssertEquals(0, componentSectionModel.BufferZonesColors.Count);
					}

					AssertEquals(sectionConfiguration.HideCapabilityTasksFromResourceChannels, componentSectionModel.HideCapabilityTasksFromResourceChannels);
					AssertEquals(sectionConfiguration.HideResourceTasksFromCapabilityChannels, componentSectionModel.HideResourceTasksFromCapabilityChannels);
					AssertEquals(sectionConfiguration.ReleaseGroupPK, componentSectionModel.ReleaseGroupPK);
					AssertEquals(sectionConfiguration.ShowWorkInReleaseGroupOnly, componentSectionModel.ShowWorkInReleaseGroupOnly);
					AssertEquals(sectionConfiguration.HideResourceTasksFromCapabilityChannels, componentSectionModel.HideResourceTasksFromCapabilityChannels);
					AssertEquals(sectionConfiguration.AcceptabilityBands.Count, componentSectionModel.AcceptabilityBands.Count());
				}
			});
		}

		public void TestToModel_ShouldNotThrowException_ForBoardWithNoComponentSections()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			board.MB_Name = "Board";

			var moduleGridSection = board.Sections.AddNew();
			moduleGridSection.MS_SectionType = BMConstants.ModuleGridSectionType;
			moduleGridSection.Row = 0;
			moduleGridSection.Column = 1;
			moduleGridSection.RowSpan = 2;
			moduleGridSection.ColSpan = 3;
			moduleGridSection.RowHeightPercent = 4;
			moduleGridSection.ColWidthPercent = 5;

			var webBrowserSection = board.Sections.AddNew();
			webBrowserSection.MS_SectionType = MENTConstants.WebBrowserSectionType;
			webBrowserSection.Row = 6;
			webBrowserSection.Column = 7;
			webBrowserSection.RowSpan = 8;
			webBrowserSection.ColSpan = 9;
			webBrowserSection.RowHeightPercent = 10;
			webBrowserSection.ColWidthPercent = 11;

			Factory.Save();

			Board boardModel = null;

			AssertNoExceptionThrown(() => boardModel = board.ToModel());
			AssertNotNull(board.MB_Name, boardModel.Name);
			AssertEquals(2, boardModel.Sections.Count());

			var moduleGridSectionModel = boardModel.Sections.First(s => s.PK == moduleGridSection.PK);

			AssertNotNull(moduleGridSectionModel.Layout);

			AssertEquals(moduleGridSection.Row, moduleGridSectionModel.Layout.Row);
			AssertEquals(moduleGridSection.Column, moduleGridSectionModel.Layout.Column);
			AssertEquals(moduleGridSection.RowSpan, moduleGridSectionModel.Layout.RowSpan);
			AssertEquals(moduleGridSection.ColSpan, moduleGridSectionModel.Layout.ColSpan);
			AssertEquals(moduleGridSection.RowHeightPercent, moduleGridSectionModel.Layout.RowHeightPercent);
			AssertEquals(moduleGridSection.ColWidthPercent, moduleGridSectionModel.Layout.ColWidthPercent);

			var webBrowserSectionModel = boardModel.Sections.First(s => s.PK == webBrowserSection.PK);

			AssertEquals(webBrowserSection.Row, webBrowserSectionModel.Layout.Row);
			AssertEquals(webBrowserSection.Column, webBrowserSectionModel.Layout.Column);
			AssertEquals(webBrowserSection.RowSpan, webBrowserSectionModel.Layout.RowSpan);
			AssertEquals(webBrowserSection.ColSpan, webBrowserSectionModel.Layout.ColSpan);
			AssertEquals(webBrowserSection.RowHeightPercent, webBrowserSectionModel.Layout.RowHeightPercent);
			AssertEquals(webBrowserSection.ColWidthPercent, webBrowserSectionModel.Layout.ColWidthPercent);
		}

		public void TestToModel_ShouldNotThrowException_ForComponentModelWithNoCells()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var board = system.Boards.AddNew();

			var bufferSection = BMSTestHelper.CreateBoardSection(buffer, board, row: 0, col: 0, rowHeightPercent: 90, colWidthPercent: 80, cellsPerSubsection: 0);
			bufferSection.BackgroundColor = Color.Yellow.ToString();
			bufferSection.SectionConfiguration.BufferZone0Color = Color.CadetBlue.ToString();
			bufferSection.SectionConfiguration.BufferZone1Color = Color.Gainsboro.ToString();
			bufferSection.SectionConfiguration.BufferZone2Color = Color.DarkOrange.ToString();
			bufferSection.SectionConfiguration.BufferZone3Color = Color.Honeydew.ToString();

			Factory.Save();

			var boardModel = board.ToModel();

			CombineAssertions(() =>
			{
				AssertEquals(1, boardModel.Sections.Count());
				AssertEquals(0, boardModel.Sections.First().CellsPerSubSection);
			});
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var board = Factory.NewWithValidTestData<BMBoard>();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, board.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			board.MB_Name = "New name";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			board.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion
	}
}
