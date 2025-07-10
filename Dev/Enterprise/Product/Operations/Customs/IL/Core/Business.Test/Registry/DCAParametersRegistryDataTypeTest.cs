using System.IO;
using System.Text;
using System.Xml;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(DCAParametersRegistryDataType))]
	sealed class DCAParametersRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DCAParametersRegistryDataType>
	{
		protected override DCAParametersRegistryDataType GetNewDataType() => new DCAParametersRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new DCAParameters();
			byte[] binary1 = GetBinary(sample1);

			var sample2 = new DCAParameters
			{
				AllServices = false,
			};

			sample2.Services.Add(new DCAService { Name = "Service1" });

			var binary2 = GetBinary(sample2);

			return [new ValidSampleAndBinaryValueInDB(sample1, binary1), new ValidSampleAndBinaryValueInDB(sample2, binary2)];

			static byte[] GetBinary(DCAParameters value)
			{
				byte[] result = null;
				var zXmlSerializer = ZXmlSerializer.New(typeof(DCAParameters));
				using (var memoryStream = new MemoryStream())
				{
					using var xmlTextWriter = new XmlTextWriter(memoryStream, new UnicodeEncoding(bigEndian: false, byteOrderMark: false));
					zXmlSerializer.Serialize(xmlTextWriter, value);
					xmlTextWriter.Flush();
					result = memoryStream.ToArray();
				}

				return result;
			}
		}

		protected override string ExpectedEditorName => "DCAParametersRegistryEditor";
	}
}
