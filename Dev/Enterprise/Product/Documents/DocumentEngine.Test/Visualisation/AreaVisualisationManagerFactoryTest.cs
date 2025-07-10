using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class AreaVisualisationManagerFactoryTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastPageFooterIsVisibleIfTheresJustANormalFooterAndALastPageFooter()
		{
			AssertFooterOfRightTypeIsNotDeletedOnTemplate(typeof(LastPageFooterArea), "PageFooter_LastAndDefaultFooters.xls");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefaultFooterIsVisibleWhenNoOtherFootersArePresent()
		{
			AssertFooterOfRightTypeIsNotDeletedOnTemplate(typeof(PageFooterArea), "PageFooter_OneFooterOnly.xls");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnlyOnePageFooterAreaIsTheOnlyFooterVisibleEvenIfOthersAreDefined()
		{
			AssertFooterOfRightTypeIsNotDeletedOnTemplate(typeof(OnlyOnePageFooterArea), "PageFooter_AllFooters.xls");
		}

		public void TestSectionPageHeaderAreaIsVisibleOnlyIfBodySectionDoesNotHaveSectionHeaderArea()
		{
			var report = new Report(null, null);
			var analyserAreas = new List<Area>();

			analyserAreas.Add(new SectionHeaderArea(1, 2, report, ""));
			analyserAreas.Add(new SectionPageHeaderArea(3, 4, report, ""));
			analyserAreas.Add(new SectionBodyArea(5, 6, report, ""));

			analyserAreas.Add(new SectionPageHeaderArea(10, 11, report, ""));
			analyserAreas.Add(new SectionBodyArea(12, 13, report, ""));

			var areaManagers = AreaVisualisationManagerFactory.NewList(analyserAreas);
			areaManagers.Sort((x, y) => (x.Area.StartingRow.CompareTo(y.Area.StartingRow)));

			AssertEquals("areaManagers.Count", 5, areaManagers.Count);

			AssertEquals("areaManagers[0].ShouldShowInVisualiser", true, areaManagers[0].ShouldShowInVisualiser);
			AssertEquals("areaManagers[1].ShouldShowInVisualiser", false, areaManagers[1].ShouldShowInVisualiser);
			AssertEquals("areaManagers[2].ShouldShowInVisualiser", true, areaManagers[2].ShouldShowInVisualiser);

			AssertEquals("areaManagers[3].ShouldShowInVisualiser", true, areaManagers[3].ShouldShowInVisualiser);
			AssertEquals("areaManagers[4].ShouldShowInVisualiser", true, areaManagers[4].ShouldShowInVisualiser);
		}

		public void TestPageFooterAreaWithHighestRankIsVisualised()
		{
			var report = new Report(null, null);
			var analyserAreas = new List<Area>();

			analyserAreas.Add(new OnlyOnePageFooterArea(1, 3, report, ""));
			analyserAreas.Add(new LastPageFooterArea(4, 6, report, ""));

			var areaManagers = AreaVisualisationManagerFactory.NewList(analyserAreas);

			AssertAreaManager(areaManagers[0], typeof(OnlyOnePageFooterArea), true);
			AssertAreaManager(areaManagers[1], typeof(LastPageFooterArea), false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
		}

		protected override void TearDown()
		{
			base.TearDown();
			temporarilyUseMainConnection?.Dispose();
		}

		IDisposable temporarilyUseMainConnection;

		void AssertAreaManager(AreaVisualisationManager manager, Type managedAreaType, bool shouldShowInVisualiser)
		{
			CombineAssertions("AssertAreaManager", () =>
			{
				Assert(string.Format("Area manager should be of type [{0}] but was [{1}]", managedAreaType, manager.Area.GetType()), managedAreaType == manager.Area.GetType());
				AssertEquals(string.Format("Should {0}visualise {1}", shouldShowInVisualiser ? "" : "NOT ", managedAreaType), shouldShowInVisualiser, manager.ShouldShowInVisualiser);
			});
		}

		void AssertFooterOfRightTypeIsNotDeletedOnTemplate(Type typeOfFooterThatShouldBeActive, string templateFileName)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(templateFileName, TestFilesSubFolder.ReportTestFiles);
			var pack = new DocumentPack();
			using (var rpt = new Report(pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();//calls analyser.Analyse()
				var analyser = rpt.Analyser;

				AreaVisualisationManager footerManagerThatShouldHaveActiveFooter = null;

				var areaManagers = AreaVisualisationManagerFactory.NewList(rpt.Analyser.Areas);

				foreach (AreaVisualisationManager areaManager in areaManagers)
				{
					var footerArea = areaManager.Area as FooterArea;
					if (footerArea != null)
					{
						if (footerArea.GetType() == typeOfFooterThatShouldBeActive)
						{
							if (footerManagerThatShouldHaveActiveFooter != null)
							{
								Fail("Cannot have 2 Footer Sections of the same type - " + typeOfFooterThatShouldBeActive.ToString());
							}
							footerManagerThatShouldHaveActiveFooter = areaManager;
						}
						else
						{
							AssertEquals("footerArea.AlwaysShowInVisualiser for footer type - " + footerArea.GetType().ToString(), false, areaManager.ShouldShowInVisualiser);
						}
					}
				}
				AssertNotNull("Couldn't find a Manager with a footer of type - " + typeOfFooterThatShouldBeActive.ToString(), footerManagerThatShouldHaveActiveFooter);
				AssertEquals("footerManagerThatShouldHaveActiveFooter.ShouldDelete", true, footerManagerThatShouldHaveActiveFooter.ShouldShowInVisualiser);
			}
		}
	}
}
