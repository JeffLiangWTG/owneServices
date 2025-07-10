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
	[TestedType(typeof(AccApportionmentTemplate))]
	public class AccApportionmentTemplateTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testTemplate = Factory.New<AccApportionmentTemplate>();
		}

		protected AccApportionmentTemplate testTemplate;

		#endregion

		#region Tests

		public void TestZDecimalsHaveCorrectDecimalPlacesAccApportionmentTemplate()
		{
			var template = Factory.New<AccApportionmentTemplate>();
			var decimals = ((DecimalPlacesAttribute[])template.GetType().GetProperty(nameof(template.PercentageTotal))?.GetCustomAttributes(typeof(DecimalPlacesAttribute), true))?.FirstOrDefault()?.DecimalPlaces ?? -1;
			int percentageDecimalPlaces = BitConverter.ToInt32(new byte[] { AccApportionmentTemplateLinesSchema.Y0_Percentage.Scale, 0, 0, 0 }, 0);
			AssertEquals("PercentageTotal should always have decimals equal to Y0_Percentage.Scale", percentageDecimalPlaces, decimals);
		}

		public void TestPercentageTotal()
		{
			AccApportionmentTemplateLines line1 = testTemplate.Lines.AddNew();
			AccApportionmentTemplateLines line2 = testTemplate.Lines.AddNew();
			AccApportionmentTemplateLines line3 = testTemplate.Lines.AddNew();
			line1.Y0_Percentage = 33.333m;
			line2.Y0_Percentage = 33.333m;
			line3.Y0_Percentage = 33.334m;
			ZDecimal total = line1.Y0_Percentage + line2.Y0_Percentage + line3.Y0_Percentage;
			AssertEquals("PercentageTotal", total, testTemplate.PercentageTotal);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("SetDefaultValues", GlbCompany.CurrentCompany.PK, testTemplate.A0_GC);
		}

		public void TestDefaultDescription()
		{
			AssertEquals("DefaultDescription", ZString.Empty, testTemplate.DefaultDescription);
			ZString description = "Line Description";
			AccApportionmentTemplateLines line1 = testTemplate.Lines.AddNew();
			line1.Y0_Description = description;
			AssertEquals("DefaultDescription", description, testTemplate.DefaultDescription);
			ZString description2 = "Another Line Description";
			AccApportionmentTemplateLines line2 = testTemplate.Lines.AddNew();
			line2.Y0_Description = description2;
			AssertEquals("DefaultDescription", description2, testTemplate.DefaultDescription);
		}

		#endregion

	}
}
