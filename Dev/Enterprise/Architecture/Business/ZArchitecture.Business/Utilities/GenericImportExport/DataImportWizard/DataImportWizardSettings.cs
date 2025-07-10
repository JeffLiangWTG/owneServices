using System.Collections.Generic;
using System.Xml.Serialization;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class DataImportWizardSettings : ImportExportWizardSettings
	{
		public DataImportWizardSettings()
		{
			StartingRow = 1;
		}

		public int StartingRow { get; set; }
		[XmlElement("Mapping")]
		public IList<DataImportWizardMappingSetting> Mappings { get; set; }
	}

	public sealed class DataImportWizardMappingSetting : XmlSerializableSetting
	{
		public DataImportWizardMappingSetting()
		{
			FileColumnIndex = -1;
			MappingName = "";
			DefaultValue = "";
			Expression = "";
			MapAs = "";
			CustomMapList = "";
			ProperCase = false;
			UpdateExisting = false;
			WesternCharactersOnly = false;
		}

		[XmlElement("ColumnIndex")]
		public int FileColumnIndex { get; set; }
		[XmlElement("Name")]
		public string MappingName { get; set; }
		public string DefaultValue { get; set; }
		public string Expression { get; set; }
		public string MapAs { get; set; }
		public bool ProperCase { get; set; }
		public bool UpdateExisting { get; set; }
		public string CustomMapList { get; set; }
		public string Delimiter { get; set; }
		[XmlElement("ColumnIndexOrder")]
		public IList<int> FileColumnIndexOrder { get; set; }
		public bool WesternCharactersOnly { get; set; }
	}
}
