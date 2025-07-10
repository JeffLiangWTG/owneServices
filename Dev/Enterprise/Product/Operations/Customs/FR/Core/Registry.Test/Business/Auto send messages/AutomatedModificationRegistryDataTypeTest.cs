using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(AutomatedModificationRegistryDataType))]
	class AutomatedModificationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutomatedModificationRegistryDataType>
	{
		protected override string ExpectedEditorName => "AutomatedModificationRegistryItemEditor";

		protected override AutomatedModificationRegistryDataType GetNewDataType()
		{
			return new AutomatedModificationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var automaticAdvanceDate1 = new AutomatedModification();
			automaticAdvanceDate1.TimeByDefault = ZDateTime.Empty;

			var automaticAdvanceDate2 = new AutomatedModification();
			automaticAdvanceDate2.TimeByDefault = ZDateTime.Today;

			var serializer = ZXmlSerializer.New(automaticAdvanceDate1.GetType());

			var xml1 = new StringBuilder();
			using (var stream = new StringWriter(xml1))
			{
				serializer.Serialize(stream, automaticAdvanceDate1);
			}

			var xml2 = new StringBuilder();
			using (var stream = new StringWriter(xml2))
			{
				serializer.Serialize(stream, automaticAdvanceDate2);
			}

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(automaticAdvanceDate1, Encoding.Unicode.GetBytes(xml1.ToString())),
				new ValidSampleAndBinaryValueInDB(automaticAdvanceDate2, Encoding.Unicode.GetBytes(xml2.ToString()))
			};
		}
	}
}
