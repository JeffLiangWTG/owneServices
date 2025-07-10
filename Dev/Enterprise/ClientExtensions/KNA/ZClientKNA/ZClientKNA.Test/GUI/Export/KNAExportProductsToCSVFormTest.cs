using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.KNA.GUI
{
	[TestedType(typeof(KNAExportProductsToCSVForm))]
	public class KNAExportProductsToCSVFormTest : ZFormBasherTest, IDisplayResultsQuery
	{
		protected override Form GetFormToBashCore()
		{
			return new KNAExportProductsToCSVForm();
		}

		protected override string CountryCode
		{
			get
			{
				return "AU";
			}
		}

		[ExpectNoExceptions]
		public virtual void TestLoadForm()
		{
			using (KNAExportProductsToCSVFormForTesting testForm = new KNAExportProductsToCSVFormForTesting())
			{
				testForm.Show();
				Application.DoEvents();
			}
		}

		public virtual void TestFormHeading()
		{
			using (KNAExportProductsToCSVFormForTesting testForm = new KNAExportProductsToCSVFormForTesting())
			{
				testForm.Show();
				AssertEquals("Form text", "Test Form - csv export", testForm.Text);
			}
		}

		public void TestFileLocationIsEntered()
		{
			using (KNAExportProductsToCSVFormForTesting testForm = new KNAExportProductsToCSVFormForTesting())
			{
				testForm.Show();
				testForm.StartButton.PerformClick();
				AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please enter the location of the file you wish to export data to.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFormIsClosed()
		{
			using (KNAExportProductsToCSVFormForTesting testForm = new KNAExportProductsToCSVFormForTesting())
			{
				testForm.Show();
				testForm.CloseButton.Enabled = true;
				testForm.CloseButton.Visible = true;
				testForm.CloseButton.PerformClick();
				Assert(!testForm.Visible);
			}
		}

		public void TestSaveSpecificDataType()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			List<OrgSupplierPart> parts = new List<OrgSupplierPart>();
			for (int i = 0; i < 4; i++)
			{
				parts.Add(factory.NewWithValidTestData<OrgSupplierPart>());
			}

			parts[0].OP_Desc = "Good Part";
			parts[1].OP_Desc = "Good Part";
			factory.Save();
			testDisplayQuery = new ZQuery(OrgSupplierPartSchema.OP_Desc, SQLComparisonOperator.StartsWith, "Good");
			using (KNAExportProductsToCSVFormForTesting testForm = new KNAExportProductsToCSVFormForTesting(this))
			{
				testForm.Show();
				string tempLocation = TempForTest.GetTempFileName();
				try
				{
					testForm.SaveSpecificDataType(tempLocation);
					List<string> actual = new List<string>();
					using (StreamReader reader = new StreamReader(tempLocation))
					{
						new OCsvLine(reader.ReadLine());
						string currentLine;
						while ((currentLine = reader.ReadLine()) != null)
						{
							var line = new OCsvLine(currentLine);
							actual.Add(line.ToString());
						}
					}

					for (int i = 0; i < 2; i++)
					{
						AssertContains(parts[i].OP_Desc, actual[i]);
					}
				}
				finally
				{
					File.Delete(tempLocation);
				}
			}
		}

		public ZQuery GetDisplayQuery()
		{
			return testDisplayQuery;
		}

		ZQuery testDisplayQuery;
#region Test Form
		class KNAExportProductsToCSVFormForTesting : KNAExportProductsToCSVForm
		{
			public KNAExportProductsToCSVFormForTesting()
			{
			}

			public KNAExportProductsToCSVFormForTesting(IDisplayResultsQuery queryProvider) : base(queryProvider)
			{
			}

			public override string FormHeading
			{
				get
				{
					return "Test Form - csv export";
				}
			}

			internal new ZButton StartButton
			{
				get
				{
					return base.StartButton;
				}
			}

			internal new ZButton CloseButton
			{
				get
				{
					return base.CloseButton;
				}
			}
		}
#endregion
	}
}
