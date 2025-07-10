using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRDataExporterCSVPrivateTest : TestCaseWithFactory
	{
		[TestDate(2007, 3, 5, 7, 30, 59, 73)]
		public void TestFileName()
		{
			DummyExporter testExporter = new DummyExporter(Factory.New<DummyBusinessObject>());
			var generatedData = testExporter.Generate();
			AssertEquals("file name", "ZUBIN073059DummySuffix.csv", generatedData.FileName);
		}

		public void TestCMRDateString()
		{
			AssertEquals("20040301", CMRDataExporterCSV.CMRDateString(new ZDateTime(2004, 3, 1, 10, 15, 00)));
		}

		public void TestCMRTimeString()
		{
			AssertEquals("2315", CMRDataExporterCSV.CMRTimeString(new ZDateTime(2004, 3, 1, 23, 15, 00)));
		}

		public void TestAddressAsASingleLine()
		{
			AssertEquals("ADD1 ADD2 CITY STATE PC", CMRDataExporterCSV.AddressAsASingleLine("ADD1", "ADD2", "CITY", "STATE", "PC"));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			currentUserEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			currentyCompanyABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			GlbStaff.CurrentUser.GS_EmailAddress = "test@example.com";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "89 123 441 321";
		}

		protected override void TearDown()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = currentUserEmailAddress;
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = currentyCompanyABN;
			base.TearDown();
		}

		string currentUserEmailAddress;
		string currentyCompanyABN;

		#endregion

	}
}
