using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.ODPL.Test
{
	[TestedType(typeof(OdplRawUsage))]
	internal class OdplRawUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			OdplRawUsage odplRawUsage = CreateRawUsage();
			AssertEquals(BillingConstants.BillingSystem.ODM, odplRawUsage.SystemCode);
		}

		public void TestModuleUsages()
		{
			OdplRawUsage odplRawUsage = CreateRawUsage();
			AssertEquals("Precondition", 0, odplRawUsage.ModuleCount);

			odplRawUsage.AddModuleUsage("", "AAA", "", "", new string[] { "frodo", "bilbo" });
			AssertEquals("One module", 1, odplRawUsage.ModuleCount);

			odplRawUsage.AddModuleUsage("", "BBB", "", "", new string[] { "frodo", "bilbo" });
			odplRawUsage.AddModuleUsage("", "BBB", "", "", new string[] { "gandalf" });

			AssertEquals("modules", 3, odplRawUsage.ModuleCount);
		}

		public void TestGetRawUsageSummarySections_NoPriceList()
		{
			OdplRawUsage odplRawUsage = CreateRawUsage();
			odplRawUsage.AddModuleUsage("", BillingConstants.CoreModuleCode, "", "", new string[] { "frodo", "bilbo", "sam" });
			odplRawUsage.AddModuleUsage("", "BBB", "", "", new string[] { "frodo", "bilbo" });
			odplRawUsage.AddModuleUsage("SC2", BillingConstants.CoreModuleCode, "", "", new string[] { "sam" });
			odplRawUsage.ApplyFeeTypes();

			string indention = "                ";

			SummarySection[] summarySections = odplRawUsage.GetRawUsageSummarySections();
			AssertEquals("summary sections", 2, summarySections.Length);

			SummarySection productionSection = summarySections[0];
			AssertEquals("Production License Usage", productionSection.Header.TopLevelDescription);
			AssertEquals("Module / Staff Name / Country", productionSection.Header.MainDescription);
			AssertEquals("Count", productionSection.Header.AdditionalDescription);
			AssertEquals("Summary lines count", 7, productionSection.Lines.Count);

			AssertEquals(Env.Licence.Core.DisplayName + " (COR)", productionSection.Lines[0].MainDescription);
			AssertEquals("3", productionSection.Lines[0].AdditionalDescription);
			AssertEquals(indention + "frodo", productionSection.Lines[1].MainDescription);
			AssertEquals(indention + "bilbo", productionSection.Lines[2].MainDescription);
			AssertEquals(indention + "sam", productionSection.Lines[3].MainDescription);

			AssertEquals("BBB", productionSection.Lines[4].MainDescription);
			AssertEquals("2", productionSection.Lines[4].AdditionalDescription);
			AssertEquals(indention + "frodo", productionSection.Lines[5].MainDescription);
			AssertEquals(indention + "bilbo", productionSection.Lines[6].MainDescription);

			SummarySection productionSection2 = summarySections[1];
			AssertEquals("Production License Usage (SC2)", productionSection2.Header.TopLevelDescription);
			AssertEquals(Env.Licence.Core.DisplayName + " (COR)", productionSection2.Lines[0].MainDescription);
			AssertEquals("1", productionSection2.Lines[0].AdditionalDescription);
			AssertEquals(indention + "sam", productionSection2.Lines[1].MainDescription);
		}

		public void TestGetRawUsageSummarySections_WithPriceList()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			EDIOrgHeader childOrganisation = BillingTestHelper.CreateDependentOrganisation(organisation, "BBB");

			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, childOrganisation.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

			OdplRawUsage odplRawUsage = new OdplRawUsage(context);
			odplRawUsage.AddModuleUsage("", "ACC", "Hell on Earth!", "", new string[] { "Mickey" });
			odplRawUsage.AddModuleUsage("", "111", "", "", new string[] { "Mouse" });
			odplRawUsage.ApplyFeeTypes();

			SummarySection[] summarySections = odplRawUsage.GetRawUsageSummarySections();
			AssertEquals("line count", 4, summarySections[0].Lines.Count);
			AssertEquals("Hell on Earth! (ACC)", summarySections[0].Lines[0].MainDescription);

			odplRawUsage = new OdplRawUsage(context);
			odplRawUsage.AddModuleUsage("", "ACC", "", "", new string[] { "Mickey" });
			odplRawUsage.AddModuleUsage("", "111", "", "", new string[] { "Mouse" });
			odplRawUsage.ApplyFeeTypes();
			summarySections = odplRawUsage.GetRawUsageSummarySections();
			AssertEquals("line count", 4, summarySections[0].Lines.Count);
			AssertEquals("default name is internal module name", Env.Licence.Accountant.DisplayName + " (ACC)", summarySections[0].Lines[0].MainDescription);

			odplRawUsage = new OdplRawUsage(context);
			odplRawUsage.AddModuleUsage("", "HEL", "Hello", "", new string[] { "Mickey" });
			odplRawUsage.AddModuleUsage("", "WOR", "World", "", new string[] { "Mickey" });
			odplRawUsage.AddModuleUsage("", "WEB", "WebTracker", "", new string[] { "WebUser" });
			odplRawUsage.AddModuleUsage("", "FAX", "Faxing", "", new string[] { "Joe" });
			odplRawUsage.ApplyFeeTypes();

			summarySections = odplRawUsage.GetRawUsageSummarySections();
			AssertEquals("line count", 8, summarySections[0].Lines.Count);
			AssertEquals("Hello (HEL)", summarySections[0].Lines[0].MainDescription);
			AssertEquals("                Mickey", summarySections[0].Lines[1].MainDescription);
			AssertEquals("World (WOR)", summarySections[0].Lines[2].MainDescription);
			AssertEquals("                Mickey", summarySections[0].Lines[3].MainDescription);
			AssertEquals("WebTracker (WEB)", summarySections[0].Lines[4].MainDescription);
			AssertEquals("                WebUser", summarySections[0].Lines[5].MainDescription);
			AssertEquals("Faxing (FAX)", summarySections[0].Lines[6].MainDescription);
			AssertEquals("                Joe", summarySections[0].Lines[7].MainDescription);

			AssertEquals("1", summarySections[0].Lines[0].AdditionalDescription);
			AssertEquals("", summarySections[0].Lines[1].AdditionalDescription);
			AssertEquals("1", summarySections[0].Lines[2].AdditionalDescription);
			AssertEquals("", summarySections[0].Lines[3].AdditionalDescription);
			AssertEquals("1", summarySections[0].Lines[4].AdditionalDescription);
			AssertEquals("", summarySections[0].Lines[5].AdditionalDescription);
			AssertEquals("1", summarySections[0].Lines[6].AdditionalDescription);
			AssertEquals("", summarySections[0].Lines[7].AdditionalDescription);
		}

		public void TestGetRawUsageSummarySections_CoreUsers()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, organisation.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			OdplRawUsage odplRawUsage = new OdplRawUsage(context);
			odplRawUsage.AddModuleUsage("", "COR", "", "", new string[] { "Albert", "Betty" });
			odplRawUsage.AddModuleUsage("", "#C1", "ediCore Pack 1", BillingConstants.FeeType.CoreUsers, new string[] { "Betty" });
			odplRawUsage.ApplyFeeTypes();

			SummarySection[] summarySections = odplRawUsage.GetRawUsageSummarySections();
			var lines = summarySections[0].Lines;
			AssertEquals("line count - core users fee basis doesn't show staff", 4, lines.Count);
			AssertEquals("Core (COR)", lines[0].MainDescription);
			AssertEquals("                Albert", lines[1].MainDescription);
			AssertEquals("                Betty", lines[2].MainDescription);
			AssertEquals("ediCore Pack 1 (#C1)", lines[3].MainDescription);

			AssertEquals("2", lines[0].AdditionalDescription);
			AssertEquals("2", lines[3].AdditionalDescription);
		}

		public void TestGetRawUsageSummarySections_DatabaseUsers()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var periodStart = new ZDateTime(2013, 9, 1);
			var context = new BillingLoadRawUsageContext(Factory, periodStart, lic.Company.LC_OH, ZGuid.Empty, lic.Company.PK, lic.Database.PK);
			OdplRawUsage odplRawUsage = new OdplRawUsage(context);
			odplRawUsage.AddModuleUsage("", "COR", "", "", new string[] { "Albert" });
			var dbUsers = new string[] { "Al", "Albert", "Betty" };
			odplRawUsage.AddModuleUsage("", "BBB", "Cloud Licence", BillingConstants.FeeType.DatabaseUsers, dbUsers);
			odplRawUsage.AddModuleUsage("", "CCC", "Remote Access Licence", BillingConstants.FeeType.DatabaseUsers, dbUsers);
			odplRawUsage.ApplyFeeTypes();

			SummarySection[] summarySections = odplRawUsage.GetRawUsageSummarySections();
			var lines = summarySections[0].Lines;
			AssertEquals("line count", 7, lines.Count);
			AssertEquals("Core (COR)", lines[0].MainDescription);
			AssertEquals("                Albert", lines[1].MainDescription);
			AssertEquals("Cloud Licence (BBB)", lines[2].MainDescription);
			AssertEquals("Remote Access Licence (CCC)", lines[3].MainDescription);
			AssertEquals("                Al", lines[4].MainDescription);
			AssertEquals("                Albert", lines[5].MainDescription);
			AssertEquals("                Betty", lines[6].MainDescription);

			AssertEquals("1", lines[0].AdditionalDescription);
			AssertEquals("", lines[1].AdditionalDescription);
			AssertEquals("3", lines[2].AdditionalDescription);
			AssertEquals("3", lines[3].AdditionalDescription);
			AssertEquals("", lines[4].AdditionalDescription);
			AssertEquals("", lines[5].AdditionalDescription);
			AssertEquals("", lines[6].AdditionalDescription);
		}

		#region Implementation

		OdplRawUsage CreateRawUsage()
		{
			return GetNewBusinessObject() as OdplRawUsage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			return new OdplRawUsage(context);
		}

		#endregion
	}
}
