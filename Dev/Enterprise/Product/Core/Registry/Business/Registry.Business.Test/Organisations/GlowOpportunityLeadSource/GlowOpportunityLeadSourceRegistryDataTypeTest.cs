using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityLeadSourceRegistryDataType))]
	sealed class GlowOpportunityLeadSourceRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeWithEnabledItemTest<GlowOpportunityLeadSourceRegistryDataType>
	{
		protected override GlowOpportunityLeadSourceRegistryDataType GetNewDataType()
		{
			var defaultValues = new CodeDescriptionBoolCollection();
			defaultValues.Add("AAA", (NoResString)"AAA Description", true);
			defaultValues.Add("ZZZ", (NoResString)"ZZZ Description", true);
			return new GlowOpportunityLeadSourceRegistryDataType(defaultValues);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var defaultValues = new CodeDescriptionBoolCollection();
			var value1 = defaultValues.AddNew();
			value1.Code = "AAA";
			value1.EnglishDescription = "AAA Description";
			value1.Bool = true;

			var xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfCodeDescriptionBool xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
				"<CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>AAA</Code><Description>AAA Description</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool>" +
				"</ArrayOfCodeDescriptionBool>";

			var defaultValues2 = new CodeDescriptionBoolCollection();

			var value2 = defaultValues2.AddNew();
			value2.Code = "ZZZ";
			value2.EnglishDescription = "ZZZ Description";
			value2.Bool = true;

			var value3 = defaultValues2.AddNew();
			value3.Code = "BBB";
			value3.EnglishDescription = "BBB Description";
			value3.Bool = false;

			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfCodeDescriptionBool xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
				"<CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>ZZZ</Code><Description>ZZZ Description</Description><Bool>Y</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool>" +
				"<CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>BBB</Code><Description>BBB Description</Description><Bool>N</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool>" +
				"</ArrayOfCodeDescriptionBool>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(defaultValues, Encoding.Unicode.GetBytes(xml)),
				new ValidSampleAndBinaryValueInDB(defaultValues2, Encoding.Unicode.GetBytes(xml2))
			};
		}

		protected override IRegistryItem GetRegistryItem() => OrganisationsDataRegistry.Instance.GlowOpportunityLeadSource;

		protected override RegistryBusinessObjectCollection[] RegistriesWithEnabledItem()
		{
			var defaultValues = new CodeDescriptionBoolCollection();
			defaultValues.Add("NEW", (NoResString)"New", true);

			return [defaultValues];
		}

		protected override RegistryBusinessObjectCollection[] RegistriesWithoutEnabledItem()
		{
			var defaultValues1 = new CodeDescriptionBoolCollection();

			var defaultValues2 = new CodeDescriptionBoolCollection();
			defaultValues2.Add("NEW", (NoResString)"New", false);

			return [defaultValues1, defaultValues2];
		}

		protected override bool HasEditor
		{
			get { return false; }
		}
	}
}
