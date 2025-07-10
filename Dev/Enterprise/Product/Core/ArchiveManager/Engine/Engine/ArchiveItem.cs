using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// A light-weight in-memory representation of a database record to be archived
	/// </summary>
	public class ArchiveItem : ArchiveableType, IArchiveItem
	{
		public ArchiveItem(SchemaColumn pKColumn, Guid pK)
			: this(pKColumn, pK, null, Guid.Empty, false, null)
		{ }

		public ArchiveItem(SchemaColumn pKColumn, Guid pK, SchemaColumn parentPKColumn, Guid parentPK, bool isReversed, string tableCode)
			: base(pKColumn, null)
		{
			PK = pK;
			Purgeable = true;
			ParentPK = parentPK;
			ParentPKColumn = parentPKColumn;
			IsReversed = isReversed;
			TableCode = tableCode;
		}

		#region IArchiveType Members

		public SchemaColumn ParentPKColumn { get; private set; }

		#endregion

		#region IArchiveItem Members

		public Guid PK { get; private set; }

		public bool Purgeable { get; set; }

		public Guid ParentPK { get; private set; }

		public bool IsReversed { get; private set; }

		public string humanReadableName;
		public string HumanReadableName
		{
			get
			{
				if (humanReadableName == null)
				{
					if (ArchiveItemHelper.ValidTableCodes.Contains(TableCode))
					{
						var factory = new BusinessObjectFactory();
						var bO = ArchiveItemHelper.GetBusinessObjectFromPkAndTableCode(factory, PK, TableCode);

						if (bO == null)
						{
							return string.Empty;
						}

						return TableCode == DummyBizoSchema.Constants.Prefix
							? (humanReadableName = bO.HumanReadableShortcutName)
							: (humanReadableName = bO.HumanReadableName);
					}

					return string.Empty;
				}

				return humanReadableName;
			}
		}

		public string TableCode { get; set; }

		public int TotalDocumentsDeleted { get; set; }

		#endregion

		public override string ToString()
			=> $"ArchiveItem(tableName={PKColumn.TableName}, pk={PK}, parentTableName={ParentPKColumn?.TableName}, parentPK={ParentPK}, isReversed={IsReversed})";
	}
}
