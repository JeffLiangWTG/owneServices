using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StatusSentimentRegistryDataType))]
	sealed class StatusSentimentRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StatusSentimentRegistryDataType>
	{
		protected override string ExpectedEditorName => "StatusSentimentRegistryItemEditor";

		protected override StatusSentimentRegistryDataType GetNewDataType()
		{
			return new StatusSentimentRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfStatusSentiment xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><StatusSentiment><Code>BKD</Code><Description>Booked</Description><Sentiment>Success</Sentiment><Variant>Outline</Variant></StatusSentiment></ArrayOfStatusSentiment>";
			var collection1 = new StatusSentimentCollection();
			var statusSentiment1InCollection1 = collection1.AddNew();
			statusSentiment1InCollection1.Code = "BKD";
			statusSentiment1InCollection1.Description = "Booked";
			statusSentiment1InCollection1.Sentiment = StatusSentiment.SentimentTypes.Success;
			statusSentiment1InCollection1.Variant = StatusSentiment.VariantTypes.Outline;
			var byteArray1 = Encoding.Unicode.GetBytes(xml);

			string xml2 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfStatusSentiment xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><StatusSentiment><Code>INC</Code><Description>Incomplete</Description><Sentiment /><Variant /></StatusSentiment></ArrayOfStatusSentiment>";
			var collection2 = new StatusSentimentCollection();
			var statusSentiment2InCollection2 = collection2.AddNew();
			statusSentiment2InCollection2.Code = "INC";
			statusSentiment2InCollection2.Description = "Incomplete";
			var byteArray2 = Encoding.Unicode.GetBytes(xml2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, byteArray1),
				new ValidSampleAndBinaryValueInDB(collection2, byteArray2)
			};
		}
	}
}
