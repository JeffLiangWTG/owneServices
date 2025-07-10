using System.Collections.Generic;
using System.Xml.Serialization;

namespace Enterprise.ZArchitecture.DataMapping
{
	public sealed class DataExportWizardSettings : ImportExportWizardSettings
	{
		public DataExportWizardSettings()
		{
			AppendExtraNewLineToEndOfFile = true;
			IncludeHeaders = true;
			EncodingType = ExportWizard.Constants.EncodingTypes.ASCII;
		}

		public string EncodingType { get; set; }
		public bool AppendExtraNewLineToEndOfFile { get; set; }
		public bool IncludeHeaders { get; set; }
		public bool AlwaysUseTextQualifier { get; set; }
		public string UseTextQualifier { get; set; }
		public bool FixedWidth { get; set; }
		public string FileNameExpression { get; set; }
		[XmlElement("Mapping")]
		public IList<DataExportWizardMappingSetting> Mappings { get; set; }
	}

	public sealed class DataExportWizardMappingSetting : XmlSerializableSetting
	{
		public DataExportWizardMappingSetting()
		{
			MappingName = "";
			MapAs = "";
			CustomMapList = "";
			Header = "";
			RowTypeName = "";
			Expression = "";
			ConditionExpr = "";
			RowTypeIdentifier = "";
		}

		[XmlElement("Name")]
		public string MappingName { get; set; }
		public string MapAs { get; set; }
		public string CustomMapList { get; set; }
		public string Header { get; set; }
		public int Width { get; set; }
		public ExportWizardMapping.FieldAlignment Alignment { get; set; }
		public string RowTypeName { get; set; }
		public string Expression { get; set; }
		public string ConditionExpr { get; set; }
		public string RowTypeIdentifier { get; set; }
	}
}
