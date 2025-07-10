using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CreditControlledDocumentsCheckConfigurationControl))]
	class CreditControlledDocumentsCheckConfigurationControlTest : PaymentAuthorisationSettingsControlWithPaymentAuthorisationTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CreditControlledDocumentsCheckConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CreditControlledDocumentsCheckConfigurationControl)control).PaymentAuthorisationSettingsGrid_ForTest.ReadOnly;
		}

		public void TestColumnsInExpectedOrder()
		{
			using (ZForm testForm = new ZForm())
			{
				testForm.MinimumSize = new System.Drawing.Size(1024, 600);
				testForm.Size = new System.Drawing.Size(1024, 600);
				testForm.CaptionRenderingEnabled = true;

				var testControl = GetNewControl();
				testForm.Controls.Add(testControl);
				var grid = ((CreditControlledDocumentsCheckConfigurationControl)testControl).PaymentAuthorisationSettingsGrid_ForTest;
				//InvoiceType
				AssertEquals("InvoiceType", ((ZGridColumnInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("RangeLocalized", ((ZGridColumnInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals("Amount", ((ZGridColumnInfo)grid.ColumnStyles[2]).ColumnName);
				AssertEquals("NumberOfDaysOverdue", ((ZGridColumnInfo)grid.ColumnStyles[3]).ColumnName);
				AssertEquals("AuthorisationRequirementLocalized", ((ZGridColumnInfo)grid.ColumnStyles[4]).ColumnName);
			}
		}
	}
}
