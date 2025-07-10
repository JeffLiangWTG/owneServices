using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TestCustomLabel))]
	sealed class CustomLabelTest : ValueProviderTest
	{
		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>()
				{
					typeof(TestCustomLabel).GetField("fConfigOrg", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(TestCustomLabel).GetField("ShouldReturnConfigOrg", BindingFlags.Instance | BindingFlags.Public)
				};
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <CustomLabel>", !ValueProviderToTest.IsResponsibleForReplacing("<CustomLabel>", Passes.FirstPass));
			Assert("should match < CustomLabel(testfield,defcaption)       >", ValueProviderToTest.IsResponsibleForReplacing("< CustomLabel(testfield,defcaption)       >", Passes.FirstPass));
			Assert("should match <CustomLabel(testfield,defcaption)>", ValueProviderToTest.IsResponsibleForReplacing("<CustomLabel(testfield,defcaption)>", Passes.FirstPass));
			Assert("should match <CustomLabel(testfield,defcaption with spaces)>", ValueProviderToTest.IsResponsibleForReplacing("<CustomLabel(testfield,defcaption with spaces)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			CustomLabelProviderToTest.ShouldReturnConfigOrg = false;
			AssertEquals("defcaption", ValueProviderToTest.GetReplacement("<CustomLabel(testfield,defcaption)>", Report));

			CustomLabelProviderToTest.ShouldReturnConfigOrg = true;
			AssertEquals("defcaption", ValueProviderToTest.GetReplacement("<CustomLabel(testfield,defcaption)>", Report));

			var customLabel = CustomLabelProviderToTest.ConfigOrg.CustomLabels.AddNew();
			AssertEquals("defcaption", ValueProviderToTest.GetReplacement("<CustomLabel(testfield,defcaption)>", Report));

			customLabel.OT_FieldName = "testfield";
			customLabel.OT_Caption = "mycaption";
			AssertEquals("defcaption", ValueProviderToTest.GetReplacement("<CustomLabel(testfield,defcaption)>", Report));

			customLabel.OT_Type = OrgConstants.CustomLabelType.Document;
			AssertEquals("mycaption", ValueProviderToTest.GetReplacement("<CustomLabel(testfield,defcaption)>", Report));
		}

		public void TestGetCustomLabel()
		{
			var factory = new BusinessObjectFactory();

			var companyOrgProxy = factory.New<OrgHeader>();
			companyOrgProxy.OH_Code = "DUNNO";
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;
			var companyLabel = companyOrgProxy.CustomLabels.AddNew();
			companyLabel.OT_FieldName = "company_field";
			companyLabel.OT_Type = OrgConstants.CustomLabelType.Document;
			companyLabel.OT_Caption = "company_caption";
			factory.Save(); // Needed to push proxy across to static factory

			var provider = new TestCustomLabel();
			provider.ShouldReturnConfigOrg = true;
			var retrievedCompanyLabel = provider.GetCustomLabel("company_field", provider.ConfigOrg);
			AssertEquals("Should find company label", "company_caption", retrievedCompanyLabel.OT_Caption);

			var organisationLabel = provider.ConfigOrg.CustomLabels.AddNew();
			organisationLabel.OT_FieldName = "organisation_field";
			organisationLabel.OT_Type = OrgConstants.CustomLabelType.Document;
			organisationLabel.OT_Caption = "organisation_caption";

			var retrievedOrganisationLabel = provider.GetCustomLabel("organisation_field", provider.ConfigOrg);
			AssertEquals("Should find organisation label", "organisation_caption", retrievedOrganisationLabel.OT_Caption);

			AssertNull(provider.GetCustomLabel("company_field", provider.ConfigOrg));
		}

		protected override System.Type ValueProviderType
		{
			get { return typeof(CustomLabel); }
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new TestCustomLabel();
		}

		#region Implementation

		TestCustomLabel CustomLabelProviderToTest
		{
			get { return (TestCustomLabel)base.ValueProviderToTest; }
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			CustomLabelProviderToTest.ShouldReturnConfigOrg = true;
			var customLabel = CustomLabelProviderToTest.ConfigOrg.CustomLabels.AddNew();
			customLabel.OT_FieldName = "CustomText1";
			customLabel.OT_Caption = "PalletValue";
			customLabel.OT_Type = OrgConstants.CustomLabelType.Document;
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			AssertEquals(expectedResult, ValueProviderToTest.GetReplacement(example, Report));
		}

		class TestCustomLabel : CustomLabel
		{
			protected override ValueProviderDocumenter GetDocumentation()
			{
				return new ValueProviderDocumenter("<CustomLabel({customfieldname},{defaultlabeltext})>",
					(NoResString)@"This functionality is used when Custom Labels are defined on the Organisation the report or document is being produced for. Falls back to the Company Organisation Proxy to find the Custom Label if not found. If no Customs Label is found for the specified custom field name, the default label text is used.",
					new List<(string example, object expectedResult)> { ("<CustomLabel(CustomText1, Pallet Type)>", "PalletValue") });
			}

			public bool ShouldReturnConfigOrg;

			OrgHeader fConfigOrg;
			public OrgHeader ConfigOrg
			{
				get
				{
					if (fConfigOrg == null)
					{
						fConfigOrg = new BusinessObjectFactory().New<OrgHeader>();
					}
					return ShouldReturnConfigOrg ? fConfigOrg : null;
				}
			}

			public new OrgCustomLabels GetCustomLabel(string customLabelFieldName, OrgHeader configOrg)
			{
				return base.GetCustomLabel(customLabelFieldName, configOrg);
			}

			protected override string GetCustomLabelsType()
			{
				return OrgConstants.CustomLabelType.Document;
			}

			protected override OrgHeader GetConfigOrganisation(Report report)
			{
				return ConfigOrg;
			}

			static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)CustomLabel(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
			public override Regex Regex
			{
				get
				{
					return fRegex;
				}
			}
		}

		#endregion
	}
}
