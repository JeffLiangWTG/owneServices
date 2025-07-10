using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceDetailGroupByRegistryDataType))]
	sealed class InvoiceDetailGroupByRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InvoiceDetailGroupByRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			InvoiceDetailGroupBy item = new InvoiceDetailGroupBy();

			item.Group2 = "CCO";
			item.Group3 = "PRD";

			string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><InvoiceDetailGroupBy><Group1>JTY</Group1><Group2>CCO</Group2><Group3>PRD</Group3></InvoiceDetailGroupBy>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(item, System.Text.Encoding.Unicode.GetBytes(xml))
			};
		}

		protected override InvoiceDetailGroupByRegistryDataType GetNewDataType()
		{
			return new InvoiceDetailGroupByRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "InvoiceDetailGroupByRegistryItemEditor"; }
		}
	}
}
