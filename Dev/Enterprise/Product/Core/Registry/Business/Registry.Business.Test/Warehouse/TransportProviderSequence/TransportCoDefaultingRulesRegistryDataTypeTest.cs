using System.Text;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransportCoDefaultingRulesRegistryDataType))]
	sealed class TransportCoDefaultingRulesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TransportCoDefaultingRulesRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "TransportCoDefaultingRulesRegistryItemEditor"; }
		}

		protected override TransportCoDefaultingRulesRegistryDataType GetNewDataType()
		{
			return new TransportCoDefaultingRulesRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sequence = new TransportCoDefaultingRules();

			sequence.Consignee = 1;
			sequence.Client = 2;
			sequence.Warehouse = 3;

			string xml =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				@"<TransportCoDefaultingRules>
					<Consignee>1</Consignee>
					<Client>2</Client>
					<Warehouse>3</Warehouse>
				</TransportCoDefaultingRules>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sequence, Encoding.Unicode.GetBytes(xml))
			};
		}
	}
}
