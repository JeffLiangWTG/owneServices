using CargoWise.Schema;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// Record type to be archived
	/// </summary>
	public class ArchiveableType
	{
		public ArchiveableType(SchemaColumn pkColumn, SchemaColumn nkColumn)
		{
			PKColumn = pkColumn;
			NKColumn = nkColumn;
		}

		public SchemaColumn PKColumn { get; private set; }

		public SchemaColumn NKColumn { get; private set; }
	}
}
