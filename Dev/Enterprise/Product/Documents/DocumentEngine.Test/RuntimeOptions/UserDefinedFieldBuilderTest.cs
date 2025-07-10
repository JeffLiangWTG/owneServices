using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestsSubclassesOf(typeof(UserDefinedFieldBuilder), ExcludePrivate = true)]
	abstract class UserDefinedFieldBuilderTest : TestCaseWithFactory
	{
		public void TestAllExpectedPropertiesDefinedInSchemaXML()
		{
			ZStringBuilder errorBuilder = new ZStringBuilder();
			var fieldBuilderList = new FieldBuilderPropertyCodeDescriptionList();
			var filterBuilderList = new FilterBuilderPropertyCodeDescriptionList();

			Assert(true);

			foreach (var property in FieldBuilder.ExpectedProperties)
			{
				if (!(fieldBuilderList.ContainsCode(property) || filterBuilderList.ContainsCode(property)))
				{
					errorBuilder.AppendLine(string.Format(
						@"Property:{0} should be defined in FieldBuilderPropertyCodeDescriptionList.xml or FilterBuilderPropertyCodeDescriptionList.xml for documentation.",
						property));
				}
			}

			if (errorBuilder.Length > 0)
			{
				Fail(errorBuilder.ToString());
			}
		}

		public void TestDocumentationProperties()
		{
			var documentation = FieldBuilder.Documentation;
			AssertNotNull("Each FieldBuilder should have the documentation.", documentation);

			var expectedProperties = FieldBuilder.ExpectedProperties.ToList();
			expectedProperties.Sort();
			documentation.SupportedProperties.Sort();
			AssertArrayEqualsByElements(expectedProperties.ToArray(), documentation.SupportedProperties.ToArray());

			CombineAssertions(string.Format("The document useage:{0} can be built for the {1}.", documentation.Useage, FieldBuilder.GetType().Name)
				, () => { FieldBuilder.CanBuild(documentation.Useage.Replace("[", "").Replace("]", "")); });

			if (FieldBuilder is LookupFieldBuilder)
			{
				Assert(documentation.SupportLookup);
			}
		}

		public void TestBuilderInFilterBuildersCollection()
		{
			bool filterExist = false;
			foreach (UserDefinedFieldBuilder fieldBuilder in new UserDefinedFieldCollectionBuilderForTest(new StringTreeNode(), string.Empty, new ValidatorPack(), DummyEvaluator).UserDefinedFieldbuilders)
			{
				if (fieldBuilder.GetType() == FieldBuilder.GetType())
				{
					filterExist = true;
					break;
				}
			}
			AssertEquals("UserDefinedFieldCollectionBuilder should contain this filter", true, filterExist);
		}

		public void TestFactoryInstance()
		{
			AssertEquals("factory instance should be the same", FieldBuilder.GetFactoryForTest(), Factory);
		}

		public virtual string DummyEvaluator(Match match)
		{
			return match.Value;
		}

		protected UserDefinedFieldBuilder FieldBuilder
		{
			get
			{
				if (fieldBuilder == null)
				{
					fieldBuilder = GetFilterBuilderToTest();
				}
				return fieldBuilder;
			}
		}
		UserDefinedFieldBuilder fieldBuilder;
		protected abstract UserDefinedFieldBuilder GetFilterBuilderToTest();
	}
}
