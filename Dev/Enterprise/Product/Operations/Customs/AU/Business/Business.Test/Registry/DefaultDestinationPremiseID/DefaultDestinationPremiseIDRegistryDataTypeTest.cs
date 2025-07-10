using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DefaultDestinationPremiseIDRegistryDataType))]
	sealed class DefaultDestinationPremiseIDRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultDestinationPremiseIDRegistryDataType>
	{
		protected override DefaultDestinationPremiseIDRegistryDataType GetNewDataType()
		{
			return new DefaultDestinationPremiseIDRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DefaultDestinationPremiseIDRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new DefaultDestinationPremiseIDCollection();
			var item1 = sample1.AddNew();
			item1.AirlineCode = "QF";
			item1.PortOfDischarge = "AUSYD";
			item1.PremiseID = "9914N";
			item1.UseDischargePort = true;

			var sample2 = new DefaultDestinationPremiseIDCollection();
			var item2 = sample1.AddNew();
			item2.AirlineCode = "SG";
			item2.PortOfDischarge = "SGSIN";
			item2.PremiseID = "99SDT";
			item2.UseDischargePort = false;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, new DefaultDestinationPremiseIDRegistryDataType().Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, new DefaultDestinationPremiseIDRegistryDataType().Serialise(sample2))
			};
		}
	}
}
