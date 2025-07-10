using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.AutomaticContainerCreation;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutomaticContainerCreationRegistryDataType))]
	sealed class AutomaticContainerCreationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutomaticContainerCreationRegistryDataType>
	{
		protected override AutomaticContainerCreationRegistryDataType GetNewDataType()
		{
			return new AutomaticContainerCreationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var item1 = new AutomaticContainerCreation(CreateRules.AlwaysCreate);
			var item2 = new AutomaticContainerCreation(CreateRules.NeverCreate);
			var item3 = new AutomaticContainerCreation(CreateRules.CreateUpToATDOrShippingInstruction);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(item1, DataType.Serialise(item1)),
				new ValidSampleAndBinaryValueInDB(item2, DataType.Serialise(item2)),
				new ValidSampleAndBinaryValueInDB(item3, DataType.Serialise(item3))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "AutomaticContainerCreationRegistryItemEditor"; }
		}

		protected override string ExpectedCode
		{
			get { return RegistryDataTypes.Codes.String; }
		}
	}
}
