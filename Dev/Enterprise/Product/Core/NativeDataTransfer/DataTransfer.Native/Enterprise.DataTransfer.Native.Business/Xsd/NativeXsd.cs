using System.Xml.Linq;

namespace Enterprise.DataTransfer.Native.Business.Xsd
{
	public abstract class NativeXsd
	{
		public static XNamespace xs
		{
			get { return "http://www.w3.org/2001/XMLSchema"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		public static class Tag
		{
			public const string Schema = "schema";
			public const string TargetNamespace = "targetNamespace";
			public const string Version = "version";
			public const string ElementFormDefault = "elementFormDefault";
			public const string Xmlns = "xmlns";

			public const string CargoWisePrefix = "cw";
			public const string Include = "include";
			public const string SchemaLocation = "schemaLocation";

			public const string All = "all";
			public const string Sequence = "sequence";
			public const string Element = "element";
			public const string ElementName = "name";
			public const string ElementType = "type";
			public const string ComplexType = "complexType";
			public const string TableKey = "TableKey";
			public const string CustomValues = "CustomValues";

			public const string Attribute = "attribute";
			public const string AttributeName = "name";
			public const string AttributeType = "type";

			// Column
			public const string MinOccurs = "minOccurs";
			public const string MaxOccurs = "maxOccurs";

			public const string SimpleType = "simpleType";
			public const string SimpleContent = "simpleContent";
			public const string Restriction = "restriction";
			public const string Extension = "extension";
			public const string Base = "base";
			public const string MaxLength = "maxLength";

			public const string ColumnValue = "value";
			public const string ColumnType = "type";
		}
	}
}
