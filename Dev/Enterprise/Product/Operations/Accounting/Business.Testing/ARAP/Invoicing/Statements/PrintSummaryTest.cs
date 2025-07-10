using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PrintSummary))]
	public class PrintSummaryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFirstPrintStatement()
		{
			AssertEquals(statement, printSummary.FirstPrintStatement);
		}

		public void TestOrganisation()
		{
			AssertEquals(organisation, printSummary.Organisation);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			Dictionary<string, PrintStatement> table = new Dictionary<string, PrintStatement>();
			return new PrintSummary(Factory, table);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			statement.OrganisationPK = organisation.PK;
			Dictionary<string, PrintStatement> table = new Dictionary<string, PrintStatement>();
			table.Add(organisation.PK.ToString(), statement);
			printSummary = new PrintSummary(Factory, table);
		}
		PrintSummary printSummary;
		OrgHeader organisation;
		PrintStatement statement;

		#endregion
	}
}
