using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class PGATabCollectionTests : TestCaseWithFactory
	{
		public void TestNoNullReferenceExceptionThrown_WhenPGARequirementCollectionChangedToNull()
		{
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				var pgaRequirements = invoiceLine.PGARequirements;
				using (var tabControl = new ZTabControl())
				{
					var pgaTabCollection = new PGATabCollectionTest(tabControl, null, true);
					pgaTabCollection.Update(null);
					pgaTabCollection.Update(pgaRequirements);
					pgaTabCollection.Update(null);
				}
			});
		}

		public void TestVisibilityTriggerForEachTabForInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var pgaRequirements = invoiceLine.PGARequirements;

			TestVisibilityTriggerForEachTab(pgaRequirements);
		}

		public void TestPGARequirementsAreDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var pgaRequirements = invoiceLine.PGARequirements;
			using (var tabControl = new ZTabControl())
			{
				var pgaTabCollection = new PGATabCollectionTest(tabControl, null, true);

				pgaTabCollection.Update(pgaRequirements);

				AssertEquals(0, tabControl.TabPages.Count);

				var pgaCodes = new PGACodes();
				pgaCodes.RemoveCode(PGACodes.Codes.CBSA);
				foreach (string pgaCode in pgaCodes.GetAllCodes())
				{
					var pgaRequirement = pgaRequirements.Cast<PGARequirement>().Single(r => r.AgencyCode == pgaCode);

					pgaRequirement.ProgramCodeRequirements[0].Indicator = "Y";
				}
				AssertNotNull("Should have PGA requirements.", pgaTabCollection.PGARequirementsExposed);
				pgaTabCollection.Dispose();
				AssertNull("PGA requirements on PGATabCollection disposed.", pgaTabCollection.PGARequirementsExposed);
			}
		}

		public void TestVisibilityTriggerForEachTabForCusClassPartPivot()
		{
			CusClassPartPivot pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var pgaRequirements = pivot.PGARequirements;

			TestVisibilityTriggerForEachTab(pgaRequirements);
		}

		void TestVisibilityTriggerForEachTab(PGARequirementCollection pgaRequirements)
		{
			using (var tabControl = new ZTabControl())
			{
				var pgaTabCollection = new PGATabCollection(tabControl, null, true);

				pgaTabCollection.Update(pgaRequirements);

				AssertEquals(0, tabControl.TabPages.Count);

				var pgaCodes = new PGACodes();
				pgaCodes.RemoveCode(PGACodes.Codes.CBSA);
				foreach (string pgaCode in pgaCodes.GetAllCodes())
				{
					var pgaRequirement = pgaRequirements.Cast<PGARequirement>().Single(r => r.AgencyCode == pgaCode);

					pgaRequirement.ProgramCodeRequirements[0].Indicator = "Y";
					AssertEquals(pgaCode, 1, tabControl.TabPages.Count);
					AssertEquals(pgaCode, pgaCode, tabControl.TabPages[0].Text);

					pgaRequirement.ProgramCodeRequirements[0].Indicator = "N";
					AssertEquals(pgaCode, 0, tabControl.TabPages.Count);
				}
			}
		}

		sealed class PGATabCollectionTest : PGATabCollection
		{
			public PGATabCollectionTest(ZTabControl tabControl, ZGrid relatedGrid, bool isOnInvoiceLine) : base(tabControl, relatedGrid, isOnInvoiceLine)
			{
			}

			internal PGARequirementCollection PGARequirementsExposed => pgaRequirements;
		}
	}
}
