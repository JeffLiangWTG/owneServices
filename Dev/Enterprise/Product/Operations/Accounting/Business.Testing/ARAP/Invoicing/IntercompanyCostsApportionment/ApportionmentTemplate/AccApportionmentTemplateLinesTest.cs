using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(AccApportionmentTemplateLines))]
	public class AccApportionmentTemplateLinesTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testTemplate = Factory.New<AccApportionmentTemplate>();
			testTemplateLines = Factory.New<AccApportionmentTemplateLines>();
			testTemplate.Lines.Add(testTemplateLines);
			testTemplateLines.Y0_A0 = testTemplate.PK;
			testTemplateLines.Y0_GB = GlbBranch.CurrentBranch.PK;
			testTemplateLines.Y0_GE = GlbDepartment.CurrentDepartment.PK;
		}

		protected AccApportionmentTemplate testTemplate;
		protected AccApportionmentTemplateLines testTemplateLines;

		#endregion

		#region Tests

		public void TestZDecimalsHaveCorrectDecimalPlacesAccApportionmentTemplate()
		{
			var template = Factory.New<AccApportionmentTemplateLines>();
			var decimals = ((DecimalPlacesAttribute[])template.GetType().GetProperty(nameof(template.Y0_Percentage))?.GetCustomAttributes(typeof(DecimalPlacesAttribute), true))?.FirstOrDefault()?.DecimalPlaces ?? -1;
			int percentageDecimalPlaces = BitConverter.ToInt32(new byte[] { AccApportionmentTemplateLinesSchema.Y0_Percentage.Scale, 0, 0, 0 }, 0);
			AssertEquals("Y0_Percentage should always have decimals equal to Y0_Percentage.Scale", percentageDecimalPlaces, decimals);
		}

		public void TestCompany()
		{
			ZGuid branchPK = GlbBranch.CurrentBranch.PK;
			testTemplateLines.Y0_GB = branchPK;
			GlbBranch branch = Factory.Load<GlbBranch>(branchPK);
			AssertEquals("Company", branch.GB_GC, testTemplateLines.Company);
			testTemplateLines.Y0_GB = ZGuid.Empty;
			AssertEquals("Company", ZGuid.Empty, testTemplateLines.Company);
		}

		public void TestValidatePercentageTotal()
		{
			testTemplateLines.Y0_Percentage = 50.000m;
			Assert("PercentageTotalInfo", testTemplate.PercentageTotalInfo.HasError("Must equal 100."));
			testTemplateLines.Y0_Percentage = 100.000m;
			Assert("PercentageTotalInfo", !testTemplate.PercentageTotalInfo.HasErrors());
		}

		#endregion

	}
}
