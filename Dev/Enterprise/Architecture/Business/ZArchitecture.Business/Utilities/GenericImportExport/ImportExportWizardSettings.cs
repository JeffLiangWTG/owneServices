using System.Collections.Generic;
using System.Xml.Serialization;

namespace Enterprise.ZArchitecture.DataMapping
{
	public abstract class ImportExportWizardSettings : XmlSerializableSetting
	{
		public ImportExportWizardSettings()
		{
			Delimiter = ",";
			TextQualifier = "\"";
		}

		public string Delimiter { get; set; }
		public string TextQualifier { get; set; }
		[XmlElement("CustomMapList")]
		public IList<CustomMapPairListSetting> CustomMapLists { get; set; }
		public int FilterIndex { get; set; }
	}

	public sealed class CustomMapPairListSetting : XmlSerializableSetting
	{
		public CustomMapPairListSetting()
		{
			Name = "";
		}

		public string Name { get; set; }
		[XmlElement("Pair")]
		public IList<CustomMapPairSetting> Pairs { get; set; }
	}

	public sealed class CustomMapPairSetting : XmlSerializableSetting
	{
		public CustomMapPairSetting()
		{
			Input = "";
			Output = "";
		}

		public string Input { get; set; }
		public string Output { get; set; }
	}
}
