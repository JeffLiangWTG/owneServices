using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityStageRegistryDataType))]
	public class GlowOpportunityStageRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeWithEnabledItemTest<GlowOpportunityStageRegistryDataType>
	{
		protected override GlowOpportunityStageRegistryDataType GetNewDataType()
		{
			var defaultValues = new GlowOpportunityStageCollection();
			defaultValues.Add("AAA", (NoResString)"AAA Description", true);
			defaultValues.Add("ZZZ", (NoResString)"ZZZ Description", false);
			return new GlowOpportunityStageRegistryDataType(defaultValues);
		}

		protected override string ExpectedEditorName
		{
			get { return "GlowOpportunityStageRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var defaultValues = new GlowOpportunityStageCollection();
			var value1 = defaultValues.AddNew();
			value1.Code = "NEW";
			value1.EnglishDescription = "New";
			value1.Bool = true;
			value1.WinProbability = 0;

			var value2 = defaultValues.AddNew();
			value2.Code = "UNF";
			value2.EnglishDescription = "Undefined";
			value2.Bool = true;

			var xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowOpportunityStage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>NEW</Code><Description>New</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>UNF</Code><Description>Undefined</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage></ArrayOfGlowOpportunityStage>";

			var defaultValues2 = new GlowOpportunityStageCollection();
			var value3 = defaultValues2.AddNew();
			value3.Code = "NEW";
			value3.EnglishDescription = "New";
			value3.Bool = false;

			var value4 = defaultValues2.AddNew();
			value4.Code = "SUC";
			value4.EnglishDescription = "Success";
			value4.Bool = true;
			value4.WinProbability = 100;

			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowOpportunityStage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>NEW</Code><Description>New</Description><Bool>N</Bool><SystemDefined>False</SystemDefined><WinProbability>0</WinProbability></GlowOpportunityStage><GlowOpportunityStage><CodeMaxLength>3</CodeMaxLength><Code>SUC</Code><Description>Success</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined><WinProbability>100</WinProbability></GlowOpportunityStage></ArrayOfGlowOpportunityStage>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(defaultValues, xml),
				new ValidSampleAndBinaryValueInDB(defaultValues2, xml2)
			};
		}

		protected override IRegistryItem GetRegistryItem() => OrganisationsDataRegistry.Instance.GlowOpportunityStages;

		protected override RegistryBusinessObjectCollection[] RegistriesWithEnabledItem()
		{
			var defaultValues = new GlowOpportunityStageCollection();
			defaultValues.Add("NEW", (NoResString)"New", true, 0);

			return [defaultValues];
		}

		protected override RegistryBusinessObjectCollection[] RegistriesWithoutEnabledItem()
		{
			var defaultValues1 = new GlowOpportunityStageCollection();

			var defaultValues2 = new GlowOpportunityStageCollection();
			defaultValues2.Add("NEW", (NoResString)"New", false, 0);

			return [defaultValues1, defaultValues2];
		}
	}
}
