using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SummarizeULDSLACConfigDataType))]
	sealed class SummarizeULDSLACConfigDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SummarizeULDSLACConfigDataType>
	{
		protected override SummarizeULDSLACConfigDataType GetNewDataType()
		{
			return new SummarizeULDSLACConfigDataType();
		}

		protected override string ExpectedEditorName => "SummarizeULDSLACConfigItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new SummarizeULDSLACConfigCollection();
			var config1 = collection1.AddNew();
			config1.DestinationCountry = "AU";
			config1.TotalSLAC = false;
			config1.UseShipmentInners = false;
			string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfSummarizeULDSLACConfig xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><SummarizeULDSLACConfig><DestinationCountry>AU</DestinationCountry><TotalSLAC>N</TotalSLAC><UseShipmentInners>N</UseShipmentInners></SummarizeULDSLACConfig></ArrayOfSummarizeULDSLACConfig>";

			var collection2 = new SummarizeULDSLACConfigCollection();
			var config2 = collection2.AddNew();
			config2.DestinationCountry = "US";
			config2.TotalSLAC = true;
			config2.UseShipmentInners = true;
			string xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfSummarizeULDSLACConfig xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><SummarizeULDSLACConfig><DestinationCountry>US</DestinationCountry><TotalSLAC>Y</TotalSLAC><UseShipmentInners>Y</UseShipmentInners></SummarizeULDSLACConfig></ArrayOfSummarizeULDSLACConfig>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, xml1),
				new ValidSampleAndBinaryValueInDB(collection2, xml2)
			};
		}

		public static SummarizeULDSLACConfigCollection GetValidSample()
		{
			var collection = new SummarizeULDSLACConfigCollection();
			var config = new SummarizeULDSLACConfig();
			collection.Add(config);
			config.DestinationCountry = "AU";
			return collection;
		}
	}
}
