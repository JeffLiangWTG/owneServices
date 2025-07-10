using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.DocBuilderThemeRegistryItem;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocBuilderThemeRegistryDataType))]
	class DocBuilderThemeRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DocBuilderThemeRegistryDataType>
	{
		protected override string ExpectedEditorName => "DocBuilderThemeRegistryItemEditor";

		protected override DocBuilderThemeRegistryDataType GetNewDataType()
		{
			return new DocBuilderThemeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = new DocBuilderThemeRegistry();
			var theme = new DocBuilderTheme("TEST NEW", ResString.GetMultilingualString("CD35074F-855B-481B-B940-41F23A50ED43", "TEST NEW"), true);
			result.Themes.Add(theme);

			var result2 = new DocBuilderThemeRegistry();
			var theme2 = new DocBuilderTheme("TEST NEW 2", ResString.GetMultilingualString("FB504F18-EF9D-4A0C-9E32-CF4003691E14", "TEST NEW 2"), true);
			result2.Themes.Add(theme2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result, new DocBuilderThemeRegistryDataType().Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new DocBuilderThemeRegistryDataType().Serialise(result2))
			};
		}
	}
}
