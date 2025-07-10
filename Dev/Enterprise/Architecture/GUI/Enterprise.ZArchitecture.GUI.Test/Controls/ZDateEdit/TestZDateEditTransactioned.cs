using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestZDateEditTransactioned : TransactionedTestCase
	{
		public void TestIncrementTextAsDateTime()
		{
			using (var testControl = new DateEditTestClass())
			{
				testControl.Text = "15-NOV-10";
				testControl.IncrementTextAsDateTime_Exposed(1, 0, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("15-NOV-11", testControl.Text.ToUpperInvariant());
				testControl.IncrementTextAsDateTime_Exposed(0, 1, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("15-DEC-11", testControl.Text.ToUpperInvariant());
				testControl.IncrementTextAsDateTime_Exposed(0, 0, 1, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("16-DEC-11", testControl.Text.ToUpperInvariant());
			}

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			using (var testControl = new DateEditTestClass())
			{
				testControl.Text = "2015-11-26";
				testControl.IncrementTextAsDateTime_Exposed(1, 0, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("2016-11-26", testControl.Text);
			}

			var factory = new BusinessObjectFactory();

			var japaneseCompany = factory.New<IGlbCompany>();
			japaneseCompany.SetCountry("JP");
			(japaneseCompany as BusinessObject)[GlbCompanySchema.GC_Code] = "XXX";

			var branch = factory.New<IGlbBranch>();
			branch.GB_GC = japaneseCompany.PK;
			(branch as BusinessObject)[GlbBranchSchema.GB_Code] = "JAP";
			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			using (var testControl = new DateEditTestClass())
			{
				testControl.Text = "15-NOV-10";
				testControl.IncrementTextAsDateTime_Exposed(1, 0, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("15-NOV-11", testControl.Text.ToUpperInvariant());
				testControl.IncrementTextAsDateTime_Exposed(0, 1, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("15-DEC-11", testControl.Text.ToUpperInvariant());
				testControl.IncrementTextAsDateTime_Exposed(0, 0, 1, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("16-DEC-11", testControl.Text.ToUpperInvariant());
			}
		}

		public void TestIncrementTextAsDateTime_DateTimeOffsetVersion()
		{
			using (var testControl = new DateTimeOffsetEditTestClass())
			{
				testControl.Text = "15-NOV-10";
				testControl.IncrementTextAsDateTime_Exposed(1, 0, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("15-NOV-11 00:00", testControl.Text.ToUpperInvariant());
				testControl.IncrementTextAsDateTime_Exposed(0, 1, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("15-DEC-11 00:00", testControl.Text.ToUpperInvariant());
				testControl.IncrementTextAsDateTime_Exposed(0, 0, 1, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("16-DEC-11 00:00", testControl.Text.ToUpperInvariant());
			}

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			using (var testControl = new DateEditTestClass())
			{
				testControl.Text = "2015-11-26";
				testControl.IncrementTextAsDateTime_Exposed(1, 0, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("2016-11-26", testControl.Text);
			}

			var factory = new BusinessObjectFactory();

			var japaneseCompany = factory.New<IGlbCompany>();
			japaneseCompany.SetCountry("JP");
			(japaneseCompany as BusinessObject)[GlbCompanySchema.GC_Code] = "XXX";

			var branch = factory.New<IGlbBranch>();
			branch.GB_GC = japaneseCompany.PK;
			(branch as BusinessObject)[GlbBranchSchema.GB_Code] = "JAP";
			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			using (var testControl = new DateEditTestClass())
			{
				testControl.Text = "15-NOV-10";
				testControl.IncrementTextAsDateTime_Exposed(1, 0, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("15-NOV-11", testControl.Text.ToUpperInvariant());
				testControl.IncrementTextAsDateTime_Exposed(0, 1, 0, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("15-DEC-11", testControl.Text.ToUpperInvariant());
				testControl.IncrementTextAsDateTime_Exposed(0, 0, 1, 0, new KeyEventArgs(Keys.Control));
				AssertEquals("16-DEC-11", testControl.Text.ToUpperInvariant());
			}
		}
	}
}
