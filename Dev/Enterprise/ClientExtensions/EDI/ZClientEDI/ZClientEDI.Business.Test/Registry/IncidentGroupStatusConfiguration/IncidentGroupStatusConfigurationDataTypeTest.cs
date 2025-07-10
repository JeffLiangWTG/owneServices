using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IncidentGroupStatusConfigurationDataType))]
	public class IncidentGroupStatusConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IncidentGroupStatusConfigurationDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "IncidentGroupStatusConfigurationRegistryEditor"; }
		}

		protected override IncidentGroupStatusConfigurationDataType GetNewDataType()
		{
			return new IncidentGroupStatusConfigurationDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new IncidentGroupTypeCollection();
			collection.RemoveAll();
			var groupType1 = collection.AddNew("GT1", "Test group type 1");
			var groupType2 = collection.AddNew("GT2", "Test group type 2");

			var setupCollection1 = groupType1.IncidentGroupStatusConfigurations;
			setupCollection1.RemoveAll();
			setupCollection1.AddNew("S11", "S11", "S11");
			setupCollection1.AddNew("S12", "S12", "S12");

			var setupCollection2 = groupType2.IncidentGroupStatusConfigurations;
			setupCollection2.RemoveAll();
			setupCollection2.AddNew("S21", "S21", "S21");
			setupCollection2.AddNew("S22", "S22", "S22");

			var collection2 = new IncidentGroupTypeCollection();
			var groupType21 = collection.AddNew("GU1", "Test group type 21");
			var groupType22 = collection.AddNew("GU2", "Test group type 22");

			var setupCollection21 = groupType21.IncidentGroupStatusConfigurations;
			setupCollection21.RemoveAll();
			setupCollection21.AddNew("SS1", "SS1", "SS1");
			setupCollection21.AddNew("SS2", "SS2", "SS2");

			var setupCollection22 = groupType22.IncidentGroupStatusConfigurations;
			setupCollection22.RemoveAll();
			setupCollection22.AddNew("SU1", "SU1", "SU1");
			setupCollection22.AddNew("SU2", "SU2", "SU2");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new IncidentGroupStatusConfigurationDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new IncidentGroupStatusConfigurationDataType().Serialise(collection2))
			};
		}
	}
}
