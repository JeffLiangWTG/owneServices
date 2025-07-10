using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DefaultPremiseIDsRegistryDataType))]
	sealed class DefaultPremiseIDsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultPremiseIDsRegistryDataType>
	{
		protected override DefaultPremiseIDsRegistryDataType GetNewDataType()
		{
			return new DefaultPremiseIDsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DefaultPremiseIDsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new DefaultPremiseIDCollection();
			var item1 = sample1.AddNew();
			item1.AirlineCode = "QF";
			item1.PortOfDischarge = "AUSYD";
			item1.PremiseID = "9914N";

			var sample2 = new DefaultPremiseIDCollection();
			var item2 = sample1.AddNew();
			item2.AirlineCode = "SG";
			item2.PortOfDischarge = "SGSIN";
			item2.PremiseID = "99SDT";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, new DefaultPremiseIDsRegistryDataType().Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, new DefaultPremiseIDsRegistryDataType().Serialise(sample2))
			};
		}
	}
}
