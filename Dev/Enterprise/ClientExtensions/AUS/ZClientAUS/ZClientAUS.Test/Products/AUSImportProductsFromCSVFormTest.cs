using System.Data;
using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Products
{
	[TestedType(typeof(AUSImportProductsFromCSVForm))]
	public class AUSImportProductsFromCSVFormTest : MasterFiles.GUI.Testing.DataLoaderFormTestCase
	{
		protected override bool ShouldIgnoreMissingBindingMember(System.Windows.Forms.Control control)
		{
			if (control.Name == "OutputListBox")
			{
				return true;
			}

			return base.ShouldIgnoreMissingBindingMember(control);
		}

		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			AUSImportProductBusinessObject importObj = new AUSImportProductBusinessObject(Factory);
			return new AUSImportProductsFromCSVForm(importObj);
		}

		protected override string CountryCode
		{
			get
			{
				return "ER";
			}
		}

		public void TestFormHeading()
		{
			AUSImportProductBusinessObject importObj = new AUSImportProductBusinessObject(Factory);
			using (AUSImportProductsFromCSVForm testForm = new AUSImportProductsFromCSVForm(importObj))
			{
				testForm.Show();
				AssertEquals("Form text", "Product Import - Austin csv-file", testForm.Text);
			}
		}

		public void TestConfirmOKLoadData() // End to End testing
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			RegisterImporterSupplier(testImporter.PK, testSupplier.PK);
			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);
				AUSImportProductBusinessObject importObj = new AUSImportProductBusinessObject(Factory);
				using (AUSImportProductsFromCSVForm testForm = new AUSImportProductsFromCSVForm(importObj))
				{
					testForm.Show();
					testForm.InternalFileNameTextBox.Text = tempFile.Filename;
					testForm.InternalImporterFindBox.CodeBox.Text = testImporter.OH_Code;
					testForm.InternalSupplierFindBox.CodeBox.Text = testSupplier.OH_Code;
					testForm.InternalStartButton.PerformClick();
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.InternalCopyLogToClipboardButton.Enabled);
					Assert(testForm.InternalCopyLogToClipboardButton.Visible);
					Assert(testForm.InternalCloseButton.Enabled);
					Assert(testForm.InternalCloseButton.Visible);
					Assert(testForm.InternalOutputListBox.Items.Count > 0);
					AssertEquals("Progress Bar total has not been set", 4, testForm.InternalProgressBar.Maximum);
					AssertEquals("Progress Bar has not been updated", 4, testForm.InternalProgressBar.Value);
					string logData = testForm.InternalGetLog();
					Assert("Log Data not as expected", logData.StartsWith("Products to Import = 3"));
					Assert("Parts not created as expected", logData.LastIndexOf("Products created = 3") > 0);
					string logDataOutputFileName = null;
					try
					{
						logDataOutputFileName = testForm.InternalCreateLogInDataDirectory(logData);
						Assert(logDataOutputFileName.Length > 0);
						Assert(logDataOutputFileName != "Not Created");
					}
					finally
					{
						File.Delete(logDataOutputFileName);
					}
				}
			}
		}

		public void TestValidateOrganisations()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);
				AUSImportProductBusinessObject importObj = new AUSImportProductBusinessObject(Factory);
				using (AUSImportProductsFromCSVForm testForm = new AUSImportProductsFromCSVForm(importObj))
				{
					testForm.Show();
					testForm.InternalFileNameTextBox.Text = tempFile.Filename;
					testForm.InternalImporterFindBox.CodeBox.Text = testImporter.OH_Code;
					testForm.InternalSupplierFindBox.CodeBox.Text = testSupplier.OH_Code;
					testForm.InternalStartButton.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Importer/Supplier not registered error expected", "This Importer and Supplier combination has not been established yet to enable product import by this option." + System.Environment.NewLine + "Set up these details using the Setup Product Import and Export option.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation
		void PopulateTestFile(TempFile tempFile)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("Part Number,Description,Import Classification");
				sw.WriteLine("A1870GE,OWNER MANUAL IMPREZA  MY04,17C,SUSF");
				sw.WriteLine("B1870AE,SUPPLEMENT IMPREZA    MY04,17C,SUSF");
				sw.WriteLine("E2417AG010VW,FRONT UNDER SPOILER        21Z,209E,SUSF");
				sw.Flush();
			}
		}

		void RegisterImporterSupplier(ZGuid importerPK, ZGuid supplierPK)
		{
			string sqlText = RegisterImporterSupplierSql;
			DbCommand command = Db.Connection.Command(sqlText);
			command.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, importerPK.ToGuid());
			command.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, supplierPK.ToGuid());
			command.ExecuteNonQuery();
		}

		const string RegisterImporterSupplierSql = @"INSERT INTO ClientAUSProductImportRegistry(T6_OH_Importer, T6_OH_Supplier)
									VALUES(@ImporterPK, @SupplierPK)";
		#endregion
	}
}
