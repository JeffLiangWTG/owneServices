using Enterprise.DataTransfer.Native.DB;

namespace Enterprise.DataTransfer.Native.Common
{
	public class PropertyDefinition : IPropertyDef
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "special code string")]
		public PropertyDefinition(ColumnDef columnDef, bool isForExport = true, bool stripCRLF = false, bool excludeFromStrip = false)
		{
			this.ColumnDef = columnDef;
			IsForExport = isForExport;
			StripCRLF = (!excludeFromStrip) && (ColumnDef.DataType.Equals("varchar") || ColumnDef.DataType.Equals("nvarchar")) && ((ColumnDef.Length >= 0 && ColumnDef.Length < 200) || stripCRLF);
		}

		public string PropertyName
		{
			get { return ColumnDef.HumanName; }
		}

		public IColumnDef ColumnDef { get; private set; }

		public bool IsForExport { get; private set; }

		public bool StripCRLF { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override string ToString()
		{
			return string.Format("Name: {0}", PropertyName);
		}
	}
}
