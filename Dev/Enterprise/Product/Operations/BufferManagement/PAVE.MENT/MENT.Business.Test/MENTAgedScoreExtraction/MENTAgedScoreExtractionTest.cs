using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(MENTAgedScoreExtraction))]
	class MENTAgedScoreExtractionTest : EnterpriseBusinessObjectTestCase
	{
		#region Clone

		public void TestCloneDefaultVisualisation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "FunnyBro");
			extraction.DefaultVisualisation.AllowArrowAnnotations = true;

			var copy = (MENTAgedScoreExtraction)extraction.Clone();

			AssertEquals(copy.PK, copy.DefaultVisualisation.MVI_MEX);
			AssertEquals(true, copy.DefaultVisualisation.AllowArrowAnnotations);
		}

		public void TestCloneVisualisationFlags()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "FunnyBro");
			extraction.DefaultVisualisation.AllowArrowAnnotations = true;
			extraction.DefaultVisualisation.ShowLegend = false;
			extraction.DefaultVisualisation.ShowHorizontalGridLines = false;
			extraction.DefaultVisualisation.ShowVerticalGridLines = true;

			var copy = (MENTAgedScoreExtraction)extraction.Clone();

			AssertEquals(copy.PK, copy.DefaultVisualisation.MVI_MEX);
			AssertEquals(true, copy.DefaultVisualisation.AllowArrowAnnotations);
			AssertEquals(false, copy.DefaultVisualisation.ShowLegend);
			AssertEquals(false, copy.DefaultVisualisation.ShowHorizontalGridLines);
			AssertEquals(true, copy.DefaultVisualisation.ShowVerticalGridLines);
		}

		public void TestCloneExtractionWithLargeName()
		{
			var name = new string('A', MENTAgedScoreExtractionSchema.MEX_Name.MaxLength);
			var extraction = MENTTestHelper.CreateExtraction(Factory, name);
			var copy = (MENTAgedScoreExtraction)extraction.Clone();

			AssertEquals(name.Substring(0, MENTAgedScoreExtractionSchema.MEX_Name.MaxLength - 5) + " Copy", copy.MEX_Name);
		}

		public void TestClone_WithEventSubscriptionAccessingFilter()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "JIMMY");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "JIMMY Extraction", query);

			var extractionList = new ActiveBusinessObjectCollection<MENTAgedScoreExtraction>(query);
			extractionList.CountChanged += (s, e) =>
			{
				foreach (var currentExtraction in extractionList)
				{
					AssertNotNull(currentExtraction.SeriesFilter);
				}
			};

			extraction.Clone();

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		public void TestDefaultVisualisationIsRegisteredOnCreation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Pandaman");
			Factory.Save();
			AssertEquals(false, extraction.HasChanges);
			AssertEquals(false, extraction.DefaultVisualisation.HasChanges);

			extraction.DefaultVisualisation.GraphTitle = "Blooop";

			AssertEquals(true, extraction.HasChanges);
			AssertEquals(true, extraction.DefaultVisualisation.HasChanges);

			var loadedExtraction = Factory.CreateNewFactory().Load<MENTAgedScoreExtraction>(extraction.PK);

			loadedExtraction.DefaultVisualisation.GraphTitle = "Blooop";

			AssertEquals(true, extraction.HasChanges);
			AssertEquals(true, extraction.DefaultVisualisation.HasChanges);
		}

		public void TestDontSetHasChangesOnExtractionWithoutEditingFilter()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Pandaman");

			AssertEquals(false, extraction.HasChanges);

			var filter = extraction.SeriesFilter;

			AssertEquals(false, extraction.HasChanges);
		}

		public void TestCreateDefaultVisualisation()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "bee");
			var extraction = Factory.New<MENTAgedScoreExtraction>();
			extraction.MEX_MAQ = query.PK;
			extraction.MEX_Name = "testing";

			Factory.Save();

			AssertEquals(true, extraction.DefaultVisualisation.IsInDatabase);
		}

		public void TestCreateDefaultVisualisationDoesNotRecreate()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "bee");
			var extraction = Factory.New<MENTAgedScoreExtraction>();
			extraction.MEX_MAQ = query.PK;
			extraction.MEX_Name = "testing";

			Factory.Save();

			AssertEquals(true, extraction.DefaultVisualisation.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var loadedExtraction = newFactory.Load<MENTAgedScoreExtraction>(extraction.PK);

			var defaultVisualisation = loadedExtraction.DefaultVisualisation;

			newFactory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedVisualisations = anotherFactory.Load<MENTAgedScoreVisualisation>(new ZQuery());
			var visualisation = loadedVisualisations.Single();

			AssertEquals("a second visualisation was not created or saved", extraction.DefaultVisualisation.PK, visualisation.PK);
			AssertEquals("a second visualisation was not created or saved", loadedExtraction.DefaultVisualisation.PK, visualisation.PK);
		}

		public void TestRelatedVisualisationIsRegisteredOnCreation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Pandaman");
			var visualisation = extraction.CreateNewRelatedVisualisation();
			Factory.Save();
			AssertEquals(false, extraction.HasChanges);
			AssertEquals(false, visualisation.HasChanges);

			visualisation.GraphTitle = "Blooop";

			AssertEquals(true, extraction.HasChanges);
			AssertEquals(true, visualisation.HasChanges);
		}

		public void TestSeriesFilter_IsProperlyPersisted()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Jimmy's Extraction");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var newExtraction = newFactory.Load<MENTAgedScoreExtraction>(extraction.PK);
			AssertEquals("Query loaded in new factory should have same FilterData", extraction.SeriesFilter.S9_FilterData, newExtraction.SeriesFilter.S9_FilterData);

			extraction.Delete();

			Assert(extraction.IsDeleted);

			ErrorReporter.Clear();
		}

		public void TestCreateANewExtractionAddsSeriesColumns()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_Code = "PEANUTS";
			var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();

			AssertEquals(6, extraction.SeriesColumns.Count);
			foreach (SQLColumnSpecification column in extraction.SeriesColumns)
			{
				column.Selected = true;
			}
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedExtraction = anotherFactory.Load<MENTAgedScoreExtraction>(extraction.PK);
			AssertEquals(6, loadedExtraction.SeriesColumns.Count);
			AssertEquals(true, loadedExtraction.SeriesColumns.Cast<SQLColumnSpecification>().All(sc => sc.Selected));
		}

		public void TestCreateNewExtractionAddsCategoryColumns()
		{
			var query = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_Code = "PEANUTS";
			var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();

			AssertEquals(6, extraction.CategoryColumns.Count);
			foreach (SQLColumnSpecification column in extraction.CategoryColumns)
			{
				column.Selected = true;
			}
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedExtraction = anotherFactory.Load<MENTAgedScoreExtraction>(extraction.PK);
			AssertEquals(6, loadedExtraction.CategoryColumns.Count);
			AssertEquals(true, loadedExtraction.CategoryColumns.Cast<SQLColumnSpecification>().All(sc => sc.Selected));
		}

		public void TestCreateNewExtractionCreatesDefaultVisualisation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "CHICK");
			extraction.DefaultVisualisation.LowerBoundAggregationSequence = 1;
			Factory.Save();

			AssertNotNull(extraction.DefaultVisualisation);
			var visualisationPK = extraction.DefaultVisualisation.PK;

			var anotherFactory = new BusinessObjectFactory();

			AssertEquals(true, anotherFactory.Load<MENTAgedScoreVisualisation>(visualisationPK).IsInDatabase);

			Factory.Save();
			var loadedVisualisation = anotherFactory.Load<MENTAgedScoreVisualisation>(visualisationPK);
			AssertNotNull(loadedVisualisation);
			AssertEquals(false, loadedVisualisation.MVI_IsCustomised);
			AssertEquals(true, loadedVisualisation.IsInDatabase);

			var newVisualisation = extraction.Visualisations.AddNew();
			AssertEquals(true, newVisualisation.MVI_IsCustomised);

			AssertEquals(2, extraction.Visualisations.Count);
		}

		public void TestCreateNewRelatedVisualisation()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Glange");

			var preCount = extraction.Visualisations.Count;

			var newVisualisation = extraction.CreateNewRelatedVisualisation();

			AssertNotNull(newVisualisation);
			AssertEquals(extraction.PK, newVisualisation.MVI_MEX);

			AssertEquals(preCount + 1, extraction.Visualisations.Count);
			AssertCollectionContains(newVisualisation, extraction.Visualisations);
		}

		public void TestGetAllExtractionsForExtractor()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "CarPark");
			MENTTestHelper.CreateAdditionalExtractionLink(extraction, extraction.RelatedQuery, extraction);
			MENTTestHelper.CreateAdditionalExtractionLink(extraction, extraction.RelatedQuery, extraction);

			AssertEquals(3, extraction.AllExtractionsForExtractor.Count());
		}

		public void TestHumanReadableName()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "CarPark");
			extraction.RelatedQuery.MAQ_QueryDescription = "Muery";
			AssertEquals("MENT Aged Score Query: Muery, Extraction: CarPark", extraction.HumanReadableName);
		}

		public void TestHumanReadableName_WhenRelatedQueryIsNull()
		{
			var extraction = base.GetNewBusinessObject();
			AssertEquals("Human readable name should be 'MENT Aged Score Query: , Extraction: '", extraction.HumanReadableName, "MENT Aged Score Query: , Extraction: ");
		}

		#region BusinessObjectTestCaseOverrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			var query = extraction.RelatedQuery;
			query.MAQ_Code = "CHARLIE";

			Factory.Save();

			return extraction;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var extraction = factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			var query = extraction.RelatedQuery;
			query.MAQ_Code = "SNOOPY";

			return extraction;
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "AdditionalExtractions";
				yield return "AggregationType";
				yield return "CategoryColumns";
				yield return "CollectionColumn";
				yield return "IsInstantaneous";
				yield return "SeriesColumns";
			}
		}

		#endregion
	}
}
