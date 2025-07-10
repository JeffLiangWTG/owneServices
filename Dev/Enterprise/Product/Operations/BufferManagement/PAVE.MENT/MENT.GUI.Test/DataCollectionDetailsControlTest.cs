using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.Core.Forms;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	public class DataCollectionDetailsControlTest : TestCaseWithFactory
	{
		public void TestFormDontSaveQueryUntilTicked()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(component, 0, 0, 0, 0, 0, 0, "ab");
			band.BAB_FiltersBySection = false;

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(1);
				var activeCheckbox = form.FindAll<ZCheckBox>().First(f => f.Text == "MENT Enabled");
				AssertEquals(false, activeCheckbox.Checked);
				form.FireSaveButton();

				var factoryStatistic = new BusinessObjectFactoryStatistic(Factory);
				AssertEquals(1, factoryStatistic.FactoryInternals.AllBusinessObjects.OfType<MENTAgedScoreQuery>().Count());

				var newFactory = Factory.CreateNewFactory();

				var query = new ZQuery(MENTAgedScoreQuerySchema.MAQ_BAB_RelatedAcceptabilityBand, SQLComparisonOperator.Equal, band.PK);
				var agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);
				AssertNotNull(agedScoreQuery);
				AssertEquals(0, newFactory.Load<MENTAgedScoreQuery>(new ZQuery()).Length);

				activeCheckbox.Checked = true;
				Assert(activeCheckbox.Checked);
				agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);
				agedScoreQuery.MAQ_Code = "ACB123";

				form.FireSaveButton();
				AssertNoErrors(band);

				Application.DoEvents();

				var loadedQuery = newFactory.LoadTop1<MENTAgedScoreQuery>(query);
				AssertNotNull(loadedQuery);
				AssertEquals("ACB123", loadedQuery.MAQ_Code);

				form.FireSaveButton();
				var newFactory2 = newFactory.CreateNewFactory();
				loadedQuery = newFactory2.Load<MENTAgedScoreQuery>(agedScoreQuery.PK);
				AssertEquals("ACB123", loadedQuery.MAQ_Code);
			}
		}

		public void TestDontKeepUnnecessaryValidationErrorsAround()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(component, 0, 0, 0, 0, 0, 0, ".");

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(1);
				var activeCheckbox = form.FindAll<ZCheckBox>().First(f => f.Text == "MENT Enabled");
				AssertEquals(false, activeCheckbox.Checked);
				activeCheckbox.Checked = true;

				var codeTextBox = form.FindAll<ZTextBox>().Single(t => t.Name == "codeTextEdit");
				var descriptionBox = form.FindAll<ZTextBox>().Single(t => t.Name == "queryDescriptionTextEdit");
				codeTextBox.Focus();
				Application.DoEvents();
				descriptionBox.Focus();
				Application.DoEvents();

				var agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(new ZQuery());
				AssertHasErrors(agedScoreQuery.MAQ_CodeInfo);

				activeCheckbox.Checked = false;
				Application.DoEvents();

				AssertNoErrors(agedScoreQuery);

				form.FireSaveButton();

				Application.DoEvents();

				var loadedQuery = Factory.CreateNewFactory().LoadTop1<MENTAgedScoreQuery>(new ZQuery());
				AssertNull(loadedQuery);
			}
		}

		public void TestFormTickActiveCorrectlyBinds()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(component, 0, 0, 0, 0, 0, 0, "ab");

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(1);
				var activeCheckbox = form.FindAll<ZCheckBox>().First(f => f.Text == "MENT Enabled");
				AssertEquals(false, activeCheckbox.Checked);
				form.FireSaveButton();

				var factoryStatistic = new BusinessObjectFactoryStatistic(Factory);
				AssertEquals(1, factoryStatistic.FactoryInternals.AllBusinessObjects.OfType<MENTAgedScoreQuery>().Count());

				var newFactory = Factory.CreateNewFactory();

				var query = new ZQuery(MENTAgedScoreQuerySchema.MAQ_BAB_RelatedAcceptabilityBand, SQLComparisonOperator.Equal, band.PK);
				var agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);
				AssertNotNull(agedScoreQuery);
				AssertEquals(0, newFactory.Load<MENTAgedScoreQuery>(new ZQuery()).Length);

				activeCheckbox.Checked = true;
				Assert(activeCheckbox.Checked);
				agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);
				AssertEquals("ab", agedScoreQuery.MAQ_Code);

				Application.DoEvents();

				var codeTextBox = form.FindAll<ZTextBox>().Single(t => t.Name == "codeTextEdit");

				AssertEquals("AB", codeTextBox.Text);

				activeCheckbox.Checked = false;

				Application.DoEvents();

				codeTextBox = form.FindAll<ZTextBox>().Single(t => t.Name == "codeTextEdit");

				AssertEquals("Unticking should keep around unsaved backing query", "AB", codeTextBox.Text);
				AssertEquals("Query should be readonly when unsaved and unticked", true, agedScoreQuery.ReadOnly);
			}
		}

		public void TestFormCloseAndOpenKeepsQueryAround()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(component, 0, 0, 0, 0, 0, 0, "ab");

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(1);
				var activeCheckbox = form.FindAll<ZCheckBox>().First(f => f.Text == "MENT Enabled");
				Assert(!activeCheckbox.Checked);

				activeCheckbox.Checked = true;
				AssertEquals(true, activeCheckbox.Checked);
				var query = new ZQuery(MENTAgedScoreQuerySchema.MAQ_BAB_RelatedAcceptabilityBand, SQLComparisonOperator.Equal, band.PK);
				var agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);
				AssertNotNull(agedScoreQuery);

				form.FireSaveButton();
			}

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(1);
				var activeCheckbox = form.FindAll<ZCheckBox>().First(f => f.Text == "MENT Enabled");
				Assert(activeCheckbox.Checked);

				activeCheckbox.Checked = false;
				AssertEquals(false, activeCheckbox.Checked);
				var query = new ZQuery(MENTAgedScoreQuerySchema.MAQ_BAB_RelatedAcceptabilityBand, SQLComparisonOperator.Equal, band.PK);
				var agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);
				AssertNotNull(agedScoreQuery);

				form.FireSaveButton();
			}
		}

		public void TestTickActiveAndThenUntickBeforeSaveMeansQueryAndScheduleIsNotSaved()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(component, 0, 0, 0, 0, 0, 0, "ab");

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(1);
				var activeCheckbox = form.FindAll<ZCheckBox>().First(f => f.Text == "MENT Enabled");
				Assert(!activeCheckbox.Checked);

				activeCheckbox.Checked = true;
				AssertEquals(true, activeCheckbox.Checked);
				var query = new ZQuery(MENTAgedScoreQuerySchema.MAQ_BAB_RelatedAcceptabilityBand, SQLComparisonOperator.Equal, band.PK);
				var agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);
				AssertNotNull(agedScoreQuery);

				activeCheckbox.Checked = false;
				AssertEquals(false, activeCheckbox.Checked);

				AssertEquals(false, agedScoreQuery.IsDeleted);
				AssertEquals(false, agedScoreQuery.QuerySchedule.IsDeleted);
				form.FireSaveButton();

				AssertEquals(false, agedScoreQuery.IsInDatabase);
				AssertEquals(false, agedScoreQuery.QuerySchedule.IsInDatabase);
			}

			var newFactory = new BusinessObjectFactory();
			AssertEquals(0, newFactory.Load<MENTAgedScoreQuery>(new ZQuery()).Length);
		}

		public void TestFormSetsQueryToBandName()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(component, 0, 0, 0, 0, 0, 0, "default");

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(1);
				var activeCheckbox = form.FindAll<ZCheckBox>().First(f => f.Text == "MENT Enabled");
				band.BAB_Name = "MENTCODE";
				activeCheckbox.Checked = true;
				Assert(activeCheckbox.Checked);

				var query = new ZQuery(MENTAgedScoreQuerySchema.MAQ_BAB_RelatedAcceptabilityBand, SQLComparisonOperator.Equal, band.PK);
				var agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);
				AssertEquals("name is what was originally set", "MENTCODE", agedScoreQuery.MAQ_Code);
			}
		}

		public void TestFormStartsWithNoQueryValidation_OnSave()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(component, 0, 0, 0, 0, 0, 0, "default");
			band.BAB_FiltersBySection = false;

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(2);

				AssertNoErrors(band);
				var postingControls = form.FindAll<ZPostingButtonsUserControl>().First();
				postingControls.SaveButton.PerformClick();
			}
		}

		public void TestMENTEnabled_FormSaveValidatesQueryCode()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var band = BMSTestHelper.CreateAcceptabilityBand(component, 0, 0, 0, 0, 0, 0, "$$$");

			using (var form = new AcceptabilityBandForm(band))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTemplateTabControl>().First();
				tabs.SelectTab(1);
				var activeCheckbox = form.FindAll<ZCheckBox>().First(f => f.Text == "MENT Enabled");

				activeCheckbox.Checked = true;
				var query = new ZQuery(MENTAgedScoreQuerySchema.MAQ_BAB_RelatedAcceptabilityBand, SQLComparisonOperator.Equal, band.PK);
				var agedScoreQuery = Factory.LoadTop1<MENTAgedScoreQuery>(query);

				AssertEquals(agedScoreQuery.MAQ_Code, "");

				var formSave = form.FireSaveButton();

				AssertEquals(ContinueWithSave.No, formSave);
			}
		}
	}
}
