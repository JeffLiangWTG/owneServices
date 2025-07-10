using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.KNA.GUI
{
	[TestedType(typeof(KNAImportProductsFromCSVForm))]
	internal class KNAImportProductsFromCSVFormTest : MasterFiles.GUI.Testing.DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new KNAImportProductsFromCSVForm();
		}

		protected override string CountryCode
		{
			get
			{
				return "AU";
			}
		}

		public void TestFormHeading()
		{
			using (KNAImportProductsFromCSVForm testForm = new KNAImportProductsFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form text", "Import Product Data", testForm.Text);
			}
		}

		public void TestConfirmOKLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);
				using (KNAImportProductsFromCSVForm testForm = new KNAImportProductsFromCSVForm())
				{
					testForm.Show();
					testForm.InternalFileNameTextBoxTest.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.InternalStartButtonTest.PerformClick();
					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Please Note: Only Products with valid Organization and Classification Lookup links will be loaded.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.InternalCopyLogToClipboardButtonTest.Enabled);
					Assert(testForm.InternalCopyLogToClipboardButtonTest.Visible);
					Assert(testForm.InternalCloseButtonTest.Enabled);
					Assert(testForm.InternalCloseButtonTest.Visible);
					Assert(testForm.InternalOutputListBoxTest.Items.Count > 0);
					AssertEquals("Progress Bar total has not been set", 4, testForm.InternalProgressBarTest.Maximum);
					AssertEquals("Progress Bar has not been updated", 4, testForm.InternalProgressBarTest.Value);
					string logData = testForm.InternalGetLogTest();
					Assert("Log Data not as expected", logData.StartsWith("Products to Import = 3"));
					string logDataOutputFileName = null;
					try
					{
						logDataOutputFileName = testForm.InternalCreateLogInDataDirectoryTest(logData);
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

		void PopulateTestFile(TempFile tempFile)
		{
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Division,QtyinStock");
				sw.WriteLine("1,RUBBER GASKET,KG,XXX,Class-Lookup,," + testOrganisation.OH_Code + ",,0");
				sw.WriteLine("P1234-4848X,Test Part,,Exp Lookup,," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code + ",Major Accounts,175850");
				sw.WriteLine("P1234-4848I,Test Part,,,Imp Lookup," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code + ",Major Accounts,175850");
				sw.Flush();
			}
		}
	}
}
