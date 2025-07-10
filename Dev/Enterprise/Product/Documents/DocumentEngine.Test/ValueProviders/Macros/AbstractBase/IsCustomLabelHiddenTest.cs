using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TestIsCustomLabelHidden))]
	sealed class IsCustomLabelHiddenTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <IsCustomLabelHidden(x,x)>", ValueProviderToTest.IsResponsibleForReplacing("<IsCustomLabelHidden(x,x)>", Passes.FirstPass));
			Assert("should match <  IsCustomLabelHidden(  x  ,  x  ) >", ValueProviderToTest.IsResponsibleForReplacing("<  IsCustomLabelHidden(  x  ,  x  ) >", Passes.FirstPass));
		}

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>()
				{
					typeof(TestIsCustomLabelHidden).GetField("fConfigOrg", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(TestIsCustomLabelHidden).GetField("ConfigOrgReturnNull", BindingFlags.Instance | BindingFlags.Public)
				};
			}
		}

		public void TestCustomLabelHidden()
		{
			var label = IsCustomLabelHiddenProviderToTest.ConfigOrg.CustomLabels.AddNew();
			label.OT_Type = OrgConstants.CustomLabelType.Form;
			label.OT_FieldName = "prefix.splaty";
			AssertEquals(
				"Prefix exists, so don't show this by default. Field not configred, so hide the field.",
				ZBool.True, ValueProviderToTest.GetReplacement("<IsCustomLabelHidden(prefix.fieldname, Y)>", Report));
			AssertEquals(
				"Prefix with lots of xx's doesn't exist, don't hide by default",
				ZBool.False, ValueProviderToTest.GetReplacement("<IsCustomLabelHidden(prefixxxxxx.fieldname, Y)>", Report));

			label.OT_Type = OrgConstants.CustomLabelType.Form;
			label.OT_FieldName = "prefix.fieldname";
			AssertEquals(
				"Don't hide the field that is configured",
				ZBool.False, ValueProviderToTest.GetReplacement("<IsCustomLabelHidden(prefix.fieldname, Y)>", Report));
		}

		public void TestCustomLabelHidden_WhenNoOtherLabels()
		{
			IsCustomLabelHiddenProviderToTest.ConfigOrg.CustomLabels.RemoveAndDeleteAll();
			AssertEquals(
				"There are no custom labels with field name like 'prefix.xxx' so default is not hidden",
				ZBool.False, ValueProviderToTest.GetReplacement("<IsCustomLabelHidden(prefix.fieldname, Y)>", Report));
			AssertEquals(
				"'No' specified for default, so it should be hidden as 'prefix.fieldname' doesn't exist by the prefix does",
				ZBool.True, ValueProviderToTest.GetReplacement("<IsCustomLabelHidden(prefix.fieldname, N)>", Report));
		}

		public void TestCustomLabelsHidden_NoConfigOrg()
		{
			IsCustomLabelHiddenProviderToTest.ConfigOrgReturnNull = true;
			AssertEquals(
				"There are no custom labels with field name like 'prefix.xxx' so default is not hidden",
				ZBool.False, ValueProviderToTest.GetReplacement("<IsCustomLabelHidden(prefix.fieldname, Y)>", Report));
			AssertEquals(
				"'No' specified for default, so it should be hidden as 'prefix.fieldname' doesn't exist by the prefix does",
				ZBool.True, ValueProviderToTest.GetReplacement("<IsCustomLabelHidden(prefix.fieldname, N)>", Report));
		}

		protected override System.Type ValueProviderType
		{
			get { return typeof(IsCustomLabelHidden); }
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new TestIsCustomLabelHidden();
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			ValueProviderToTest.GetReplacement(example, Report);
		}

		#region Test Classes

		sealed class TestIsCustomLabelHidden : IsCustomLabelHidden
		{
			protected override ValueProviderDocumenter GetDocumentation()
			{
				return new ValueProviderDocumenter("<IsCustomLabelHidden({customlabelname},{defaultreturnvalue})>",
					(NoResString)@"Returns a boolean value which is the opposite of the default return value if the Custom Label is not defined on the Organisation associated with the Document or Report. The default return value is always either 'Y' or 'N'.",
					new List<(string example, object expectedResult)> { ("<IsCustomLabelHidden(OrderHeader.CustomDate2, Y)>", false) });
			}

			public bool ConfigOrgReturnNull;

			OrgHeader fConfigOrg;
			public OrgHeader ConfigOrg
			{
				get
				{
					if (fConfigOrg == null)
					{
						fConfigOrg = new BusinessObjectFactory().New<OrgHeader>();
					}
					return ConfigOrgReturnNull ? null : fConfigOrg;
				}
			}

			protected override string GetCustomLabelsType()
			{
				return OrgConstants.CustomLabelType.Form;
			}

			protected override Enterprise.MasterFiles.Business.OrgHeader GetConfigOrganisation(Report report)
			{
				return fConfigOrg;
			}

			static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)IsCustomLabelHidden(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
			public override Regex Regex
			{
				get { return fRegex; }
			}
		}

		#endregion

		#region Implementation

		TestIsCustomLabelHidden IsCustomLabelHiddenProviderToTest
		{
			get { return (TestIsCustomLabelHidden)base.ValueProviderToTest; }
		}

		#endregion
	}
}
