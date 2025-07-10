using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test
{
	[TestedType(typeof(AmountOrPercentageAuthorisationSettingsControl))]
	sealed class AmountOrPercentageAuthorisationSettingsControlTest : PaymentAuthorisationSettingsControlWithPaymentAuthorisationTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AmountOrPercentageAuthorisationSettingsControl)control).PaymentAuthorisationSettingsGrid_ForTest.ReadOnly;
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
				var grid = ((AmountOrPercentageAuthorisationSettingsControl)testControl).PaymentAuthorisationSettingsGrid_ForTest;
				AssertEquals("RangeLocalized", ((ZGridColumnInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("Amount", ((ZGridColumnInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals("Percentage", ((ZGridColumnInfo)grid.ColumnStyles[2]).ColumnName);
				AssertEquals("AuthorisationRequirementLocalized", ((ZGridColumnInfo)grid.ColumnStyles[3]).ColumnName);
			}
		}
	}
}
