using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingVAT))]
	sealed class GuidedDecisionMakingVATTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDisplayText()
		{
			CombineAssertions(() =>
			{
				var gdmVAT = (GuidedDecisionMakingVAT)GetNewBusinessObject();
				gdmVAT.VATCode = "RED";
				gdmVAT.Description = "Reduced Tax Desc";
				gdmVAT.VATRateValue = 0.245;
				gdmVAT.AdditionalCode = "V001";
				gdmVAT.AdditionalCodeDescription = "V001 Description.";
				gdmVAT.Category = "A001";
				AssertEquals("DisplayText of gdmVAT.", "RED - Reduced Tax Desc - 24.50%\r\nV001 - V001 Description.\r\nA001", gdmVAT.DisplayText);

				var gdmVAT_WithoutAdditionalCode = (GuidedDecisionMakingVAT)GetNewBusinessObject();
				gdmVAT_WithoutAdditionalCode.VATCode = "SRR";
				gdmVAT_WithoutAdditionalCode.Description = "Super-Reduced Tax Desc";
				gdmVAT_WithoutAdditionalCode.VATRateValue = 0.00456;
				gdmVAT_WithoutAdditionalCode.AdditionalCode = "";
				gdmVAT_WithoutAdditionalCode.AdditionalCodeDescription = "Description for Empty code.";
				gdmVAT_WithoutAdditionalCode.Category = "A001";
				AssertEquals("DisplayText of gdmVAT_WithoutAdditionalCode.", "SRR - Super-Reduced Tax Desc - 0.46%\r\nDescription for Empty code.\r\nA001", gdmVAT_WithoutAdditionalCode.DisplayText);

				var gdmVAT_WithoutDescription = (GuidedDecisionMakingVAT)GetNewBusinessObject();
				gdmVAT_WithoutDescription.VATCode = "ORD";
				gdmVAT_WithoutDescription.Description = "Ordinary";
				gdmVAT_WithoutDescription.VATRateValue = 0.1;
				gdmVAT_WithoutDescription.AdditionalCode = "V999";
				gdmVAT_WithoutDescription.AdditionalCodeDescription = "";
				gdmVAT_WithoutDescription.Category = "A001";
				AssertEquals("DisplayText of gdmVAT_WithoutDescription.", "ORD - Ordinary - 10.00%\r\nV999\r\nA001", gdmVAT_WithoutDescription.DisplayText);

				var gdmVAT_WithoutAdditionalCodeOrDescription = (GuidedDecisionMakingVAT)GetNewBusinessObject();
				gdmVAT_WithoutAdditionalCodeOrDescription.VATCode = "STD";
				gdmVAT_WithoutAdditionalCodeOrDescription.Description = "Standard Tax Desc";
				gdmVAT_WithoutAdditionalCodeOrDescription.VATRateValue = 0.00451;
				gdmVAT_WithoutAdditionalCodeOrDescription.AdditionalCode = "";
				gdmVAT_WithoutAdditionalCodeOrDescription.AdditionalCodeDescription = "";
				gdmVAT_WithoutAdditionalCodeOrDescription.Category = "A001";
				AssertEquals("DisplayText of gdmVAT_WithoutAdditionalCodeOrDescription.", "STD - Standard Tax Desc - 0.45%\r\nA001", gdmVAT_WithoutAdditionalCodeOrDescription.DisplayText);
			});
		}

		public void TestParent()
		{
			var gdmVAT = (GuidedDecisionMakingVAT)GetNewBusinessObject();

			AssertType<GuidedDecisionMakingBasic>("Parent of GuidedDecisionMakingVAT should be of type GuidedDecisionMakingBasic.", gdmVAT.Parent);
		}

		public void TestFieldsMaxLength()
		{
			var vat = (GuidedDecisionMakingVAT)GetNewBusinessObject();
			AssertEquals("VATCodeMaxLength: ", 4, vat.VATCodeInfo.MaxLength);
			AssertEquals("DescriptionMaxLength: ", 500, vat.DescriptionInfo.MaxLength);
			AssertEquals("AdditionalCodeMaxLength: ", 15, vat.AdditionalCodeInfo.MaxLength);
			AssertEquals("AdditionalCodeDescriptionMaxLength: ", 500, vat.AdditionalCodeDescriptionInfo.MaxLength);
			AssertEquals("CategoryMaxLength: ", 4, vat.CategoryInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var gdmBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			return new GuidedDecisionMakingVAT(gdmBasic);
		}
	}
}
