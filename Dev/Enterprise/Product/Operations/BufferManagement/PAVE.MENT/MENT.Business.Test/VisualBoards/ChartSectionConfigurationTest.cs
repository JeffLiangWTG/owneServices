using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Shared;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(ChartSectionConfiguration))]
	class ChartSectionConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestModifyingExtraction()
		{
			var extraction1 = MENTTestHelper.CreateExtraction(Factory, "Extraction 1", isInstantaneous: true, queryCode: "TESTQUERY1");
			extraction1.DefaultVisualisation.GraphTitle = "Extraction 1";

			var extraction2 = MENTTestHelper.CreateExtraction(Factory, "Extraction 2", isInstantaneous: true, queryCode: "TESTQUERY2");
			extraction2.DefaultVisualisation.GraphTitle = "Extraction 2";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction1);
			configuration.OverrideDefaultVisualisation = true;
			AssertEquals("GIVEN configuration.OverrideDefaultVisualisation = TRUE", true, configuration.OverrideDefaultVisualisation);

			var oldVisualisationPK = configuration.VisualisationPK;

			CombineAssertions("WHEN modifying configuration.ExtractionPK", () =>
			{
				AssertEquals("old configuration.ExtractionPK", extraction1.PK, configuration.ExtractionPK);
				configuration.ExtractionPK = extraction2.PK;
				AssertEquals("new configuration.ExtractionPK", extraction2.PK, configuration.ExtractionPK);
			});

			AssertEquals("THEN configuration.OverrideDefaultVisualisation should be FALSE", false, configuration.OverrideDefaultVisualisation);
			AssertNotEquals("THEN configuration.Visualisation should not the old override", oldVisualisationPK, configuration.VisualisationPK);
		}

		#region Cloning

		#region Board Cloning

		public void TestCloneBoard_NoOverrideDefaultVisualisation()
		{
			var overrideDefaultVisualisation = false;
			var originalSection = CreateSectionForCloningTest(overrideDefaultVisualisation);

			Factory.Save();

			var clonedBoard = originalSection.Board.Clone() as BMBoard;
			var clonedSection = clonedBoard.Sections.First();

			AssertNotEquals("WHEN cloning board", originalSection.Board.PK, clonedSection.Board.PK);

			AssertCloning(originalSection, clonedSection, overrideDefaultVisualisation);
		}

		public void TestCloneBoard_OverrideDefaultVisualisation()
		{
			var overrideDefaultVisualisation = true;
			var originalSection = CreateSectionForCloningTest(overrideDefaultVisualisation);

			Factory.Save();

			var clonedBoard = originalSection.Board.Clone() as BMBoard;
			var clonedSection = clonedBoard.Sections.First();

			AssertNotEquals("WHEN cloning board", originalSection.Board.PK, clonedSection.Board.PK);

			AssertCloning(originalSection, clonedSection, overrideDefaultVisualisation);
		}

		#endregion

		#region Section Cloning

		public void TestCloneSection_NoOverrideDefaultVisualisation()
		{
			var overrideDefaultVisualisation = false;
			var originalSection = CreateSectionForCloningTest(overrideDefaultVisualisation);

			Factory.Save();

			var clonedSection = originalSection.Clone() as BMBoardSection;
			AssertCloning(originalSection, clonedSection, overrideDefaultVisualisation);
		}

		public void TestCloneSection_OverrideDefaultVisualisation()
		{
			var overrideDefaultVisualisation = true;
			var originalSection = CreateSectionForCloningTest(overrideDefaultVisualisation);

			Factory.Save();

			var clonedSection = originalSection.Clone() as BMBoardSection;
			AssertCloning(originalSection, clonedSection, overrideDefaultVisualisation);
		}

		#endregion

		#endregion

		public void TestRelatedVisualisationForDefaultVisualisation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Baallooogy are cauliflowers");

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory);

			AssertNull(configuration.RelatedVisualisation);

			configuration.ExtractionPK = extraction.PK;

			AssertNotNull(configuration.RelatedVisualisation);
			AssertEquals(false, configuration.RelatedVisualisation.MVI_IsCustomised);
		}

		public void TestOverrideCreatesNewVisualisation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Polka Pecker Peeter Booper Plocker Knocker Rocket");
			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);

			var defaultVisualisation = configuration.RelatedVisualisation;

			AssertEquals(ZGuid.Empty, configuration.VisualisationPK);

			configuration.OverrideDefaultVisualisation = true;

			AssertNotEquals(defaultVisualisation.PK, configuration.RelatedVisualisation.PK);
			AssertEquals(configuration.RelatedVisualisation.PK, configuration.VisualisationPK);
			AssertEquals(true, configuration.RelatedVisualisation.MVI_IsCustomised);
		}

		public void TestUnOverrideDeletesVisualisation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Esau would saw wood with a wood saw he saw Wood saw wood.");
			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory, extraction: extraction);

			configuration.OverrideDefaultVisualisation = true;
			AssertEquals(configuration.RelatedVisualisation.PK, configuration.VisualisationPK);

			var newVisualisation = configuration.RelatedVisualisation;

			configuration.OverrideDefaultVisualisation = false;
			AssertEquals(ZGuid.Empty, configuration.VisualisationPK);
			AssertEquals(true, newVisualisation.IsDeleted);

			AssertEquals(extraction.DefaultVisualisation, configuration.RelatedVisualisation);
		}

		public void TestPopulateSectionName()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Vader");
			extraction.DefaultVisualisation.GraphTitle = "Test1";

			var configuration = MENTTestHelper.CreateChartSectionConfiguration(Factory);

			AssertEquals("", configuration.SectionName);

			configuration.ExtractionPK = extraction.PK;

			configuration.OverrideDefaultVisualisation = true;
			var visualisation = configuration.RelatedVisualisation;
			AssertEquals("Test1", configuration.SectionName);

			visualisation.GraphTitle = "test2";
			AssertEquals("test2", configuration.SectionName);

			visualisation.GraphTitle = "";
			AssertEquals("Vader", configuration.SectionName);

			configuration.OverrideDefaultVisualisation = false;
			extraction.DefaultVisualisation.GraphTitle = "";
			AssertEquals("Vader", configuration.SectionName);
		}

		#region Implementation

		BMBoardSection CreateSectionForCloningTest(bool overrideDefaultVisualisation)
		{
			var system = BMSTestHelper.CreateSystem(Factory);

			var component = system.Components.AddNew();
			component.FC_Name = "Original Component";
			component.FC_Type = BMComponentTypeList.Codes.Bucket;

			var board = system.Boards.AddNew();
			board.MB_Name = "Original Board";

			var section = board.Sections.AddNew();
			section.MS_FC_Component = component.PK;
			section.MS_SectionType = MENTConstants.ChartSectionType;

			var extraction = MENTTestHelper.CreateExtraction(Factory, "Original Extraction");
			var sectionConfig = section.Configuration as ChartSectionConfiguration;
			sectionConfig.ExtractionPK = extraction.PK;
			sectionConfig.OverrideDefaultVisualisation = overrideDefaultVisualisation;
			sectionConfig.RelatedVisualisation.GraphTitle = "Original Graph Title";

			return section;
		}

		public void AssertCloning(BMBoardSection originalSection, BMBoardSection clonedSection, bool overrideDefaultVisualisation)
		{
			var originalBoard = originalSection.Board;
			var originalSectionConfig = originalSection.Configuration as ChartSectionConfiguration;

			AssertEquals("GIVEN section with MNT type", MENTConstants.ChartSectionType, originalSection.MS_SectionType);
			AssertEquals(string.Format("GIVEN section-configuration with OverrideDefaultVisualisation={0}", overrideDefaultVisualisation), overrideDefaultVisualisation, originalSectionConfig.OverrideDefaultVisualisation);

			CombineAssertions("WHEN cloning section", () =>
			{
				AssertNotNull("clonedSection exists", clonedSection);
				AssertNotEquals("has different PK", originalSection.PK, clonedSection.PK);
			});

			var clonedSectionConfig = clonedSection.Configuration as ChartSectionConfiguration;
			var originalExtraction = ((ChartSectionConfiguration)originalSection.Configuration).Extraction;

			AssertEquals("THEN cloned-section should have MNT type", MENTConstants.ChartSectionType, clonedSection.MS_SectionType);
			AssertEquals(string.Format("THEN cloned-section-config should have OverrideDefaultVisualisation={0}", overrideDefaultVisualisation), overrideDefaultVisualisation, clonedSectionConfig.OverrideDefaultVisualisation);
			AssertEquals("THEN cloned-section-config should have original-section-config value", originalSectionConfig.RelatedVisualisation.GraphTitle, clonedSectionConfig.RelatedVisualisation.GraphTitle);

			if (overrideDefaultVisualisation)
			{
				CombineAssertions("GIVEN overrideDefaultVisualisation = true THEN cloned-section-config should have new visualisation ", () =>
				{
					AssertNotEquals("Different from extraction-visualisation", originalExtraction.DefaultVisualisation.PK, clonedSectionConfig.VisualisationPK);
					AssertNotEquals("Different from original section-visualisation", originalSectionConfig.VisualisationPK, clonedSectionConfig.VisualisationPK);
				});

				clonedSectionConfig.RelatedVisualisation.GraphTitle = "Cloned Graph Title";
				clonedSection.Board.MB_Name = "Cloned Board";
				Factory.Save();
				AssertEquals("WHEN modifying cloned-section-config", "Cloned Graph Title", clonedSectionConfig.RelatedVisualisation.GraphTitle);

				originalSection = Factory.Load<BMBoardSection>(originalSection.PK);
				originalSectionConfig = originalSection.Configuration as ChartSectionConfiguration;

				CombineAssertions("SHOULD not modiyfing original-section-configuration", () =>
				{
					AssertEquals("Original-section-configuration values are not modified", "Original Graph Title", originalSectionConfig.RelatedVisualisation.GraphTitle);
					AssertNotEquals("original-section-configuration different from cloned-section-configuration value", clonedSectionConfig.RelatedVisualisation.GraphTitle, originalSectionConfig.RelatedVisualisation.GraphTitle);
				});
			}
			else
			{
				CombineAssertions("GIVEN overrideDefaultVisualisation = false THEN cloned-section-config have default visualisation", () =>
				{
					AssertNotEquals("Cloned visualisation same as default visualisation", originalExtraction.DefaultVisualisation.PK, clonedSectionConfig.VisualisationPK);
					AssertEquals("Cloned visualisation values same as original visualisation", originalSectionConfig.RelatedVisualisation.GraphTitle, clonedSectionConfig.RelatedVisualisation.GraphTitle);
				});
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChartSectionConfiguration(Factory.New<IBMBoardSection>());
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				// Moar pivot tables pls
				yield return "ExtractionPK";
				yield return "OverrideDefaultVisualisation";
				yield return "VisualisationPK";
			}
		}

		#endregion
	}
}
