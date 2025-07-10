using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Products
{
	[TestedType(typeof(ExportProductsToCSVForm))]
	public class ExportProductsToCSVFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ExportProductsToCSVForm();
		}

		protected override string CountryCode
		{
			get
			{
				return "ER";
			}
		}

		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			using (ExportProductsToCSVForm testForm = new ExportProductsToCSVForm())
			{
				testForm.Show();
				Application.DoEvents();
			}
		}

		public void TestFormHeading()
		{
			using (ExportProductsToCSVForm testForm = new ExportProductsToCSVForm())
			{
				testForm.Show();
				AssertEquals("Form text", "Export Products - Austin csv-file", testForm.Text);
			}
		}

		public void TestValidateOrganisations()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			using (ExportProductsToCSVForm testForm = new ExportProductsToCSVForm())
			{
				testForm.Show();
				testForm.ImporterFindBox.CodeBox.Text = testImporter.OH_Code;
				testForm.SupplierFindBox.CodeBox.Text = testSupplier.OH_Code;
				testForm.OKBoundButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Importer/Supplier not registered error expected", "This Importer and Supplier combination has not been established yet to enable use via Austin csv options." + System.Environment.NewLine + "Set up these details using the Setup Product Import and Export option.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
