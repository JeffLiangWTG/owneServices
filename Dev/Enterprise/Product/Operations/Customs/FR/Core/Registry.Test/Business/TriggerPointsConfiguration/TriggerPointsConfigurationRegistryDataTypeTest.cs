using System.IO;
using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(TriggerPointsConfigurationRegistryDataType))]
	class TriggerPointsConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TriggerPointsConfigurationRegistryDataType>
	{
		protected override string ExpectedEditorName => "TriggerPointsConfigurationRegistryItemEditor";

		protected override TriggerPointsConfigurationRegistryDataType GetNewDataType()
		{
			return new TriggerPointsConfigurationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var trigger1 = new TriggerPointsConfiguration();
			trigger1.ImportTriggerPoint = TriggerPointsCodeList.Codes.PAB;
			trigger1.ExportTriggerPoint = TriggerPointsCodeList.Codes.NUL;

			var trigger2 = new TriggerPointsConfiguration();
			trigger2.ImportTriggerPoint = TriggerPointsCodeList.Codes.NUL;
			trigger2.ExportTriggerPoint = TriggerPointsCodeList.Codes.REC;

			var serializer = ZXmlSerializer.New(trigger1.GetType());

			StringBuilder xml1 = new StringBuilder();
			using (StringWriter stream = new StringWriter(xml1))
			{
				serializer.Serialize(stream, trigger1);
			}

			StringBuilder xml2 = new StringBuilder();
			using (StringWriter stream = new StringWriter(xml2))
			{
				serializer.Serialize(stream, trigger2);
			}

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(trigger1, Encoding.Unicode.GetBytes(xml1.ToString())),
				new ValidSampleAndBinaryValueInDB(trigger2, Encoding.Unicode.GetBytes(xml2.ToString()))
			};
		}
	}
}
