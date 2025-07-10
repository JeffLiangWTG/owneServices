using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	abstract class FilterBuilderTest : TestCaseWithFactory
	{
		public void TestAllExpectedPropertiesDefinedInSchemaXML()
		{
			var list = new FilterBuilderPropertyCodeDescriptionList();
			foreach (var property in FilterBuilder.ExpectedProperties)
			{
				Assert(string.Format(@"Property:{0} should be defined in FilterBuilderPropertyCodeDescriptionList.xml for documentation", property), list.ContainsCode(property));
			}
		}

		public void TestDocumentationProperties()
		{
			var documentation = FilterBuilder.Documentation;
			AssertNotNull("Each FilterBuilder should have the documentation.", documentation);

			var expectedProperties = FilterBuilder.ExpectedProperties.ToList();
			expectedProperties.Sort();
			documentation.SupportedProperties.Sort();
			AssertArrayEqualsByElements(expectedProperties.ToArray(), documentation.SupportedProperties.ToArray());

			CombineAssertions(string.Format("The document useage:{0} can be built for the {1}.", documentation.Useage, FilterBuilder.GetType().Name)
				, () => { FilterBuilder.CanBuild(documentation.Useage.Replace("[", "").Replace("]", "")); });

			if (FilterBuilder is LookupBuilderBase)
			{
				Assert(documentation.SupportLookup);
			}
		}

		public void TestBuilderInFilterBuildersCollection()
		{
			bool filterExist = false;
			foreach (FilterBuilder filterBuilder in new FilterCollectionBuilderForTest(new StringCollection(), new ValidatorPack(), new StringTreeNode(), DummyEvaluator).Filterbuilders)
			{
				if (filterBuilder.GetType() == FilterBuilder.GetType())
				{
					filterExist = true;
					break;
				}
			}
			AssertEquals("FilterBuilderCollection should contain this filter", true, filterExist);
		}

		public void TestFactoryInstance()
		{
			AssertEquals("factory instance should be the same", FilterBuilder.GetFactoryForTest(), Factory);
		}

		public virtual string DummyEvaluator(Match match)
		{
			return match.Value;
		}

		FilterBuilder FilterBuilder
		{
			get
			{
				if (fFilterBuilder == null)
				{
					fFilterBuilder = GetFilterBuilderToTest();
				}
				return fFilterBuilder;
			}
		}
		FilterBuilder fFilterBuilder;
		protected abstract FilterBuilder GetFilterBuilderToTest();

		sealed class FilterCollectionBuilderForTest : FilterCollectionBuilder
		{
			internal FilterCollectionBuilderForTest(StringCollection parameters, ValidatorPack validatorPack, StringTreeNode root, MatchEvaluator evaluatorForDefaultValues)
				: base(parameters, validatorPack, root, evaluatorForDefaultValues)
			{ }

			internal ArrayList Filterbuilders => FilterBuilders;
		}
	}
}
