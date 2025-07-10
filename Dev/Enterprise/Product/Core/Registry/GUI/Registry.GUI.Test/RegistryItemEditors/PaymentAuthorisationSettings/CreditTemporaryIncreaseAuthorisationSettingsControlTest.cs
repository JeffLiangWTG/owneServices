using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test
{
	[TestedType(typeof(CreditTemporaryIncreaseAuthorisationSettingsControl))]
	sealed class CreditTemporaryIncreaseAuthorisationSettingsControlTest : PaymentAuthorisationSettingsControlWithPaymentAuthorisationTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CreditTemporaryIncreaseAuthorisationSettingsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CreditTemporaryIncreaseAuthorisationSettingsControl)control).PaymentAuthorisationSettingsGrid_ForTest.ReadOnly;
		}

		[RequiresSTA]
		public void TestColumnsInExpectedOrder()
		{
			using (ZForm testForm = new ZForm())
			{
				testForm.MinimumSize = new System.Drawing.Size(1024, 600);
				testForm.Size = new System.Drawing.Size(1024, 600);
				testForm.CaptionRenderingEnabled = true;

				var testControl = GetNewControl();
				testForm.Controls.Add(testControl);
				var grid = ((CreditTemporaryIncreaseAuthorisationSettingsControl)testControl).PaymentAuthorisationSettingsGrid_ForTest;
				AssertEquals("RangeLocalized", ((ZGridColumnInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("Amount", ((ZGridColumnInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals("Percentage", ((ZGridColumnInfo)grid.ColumnStyles[2]).ColumnName);
				AssertEquals("AuthorisationRequirementLocalized", ((ZGridColumnInfo)grid.ColumnStyles[3]).ColumnName);
				AssertEquals("DaysToExpiry", ((ZGridColumnInfo)grid.ColumnStyles[4]).ColumnName);
			}
		}
	}
}
