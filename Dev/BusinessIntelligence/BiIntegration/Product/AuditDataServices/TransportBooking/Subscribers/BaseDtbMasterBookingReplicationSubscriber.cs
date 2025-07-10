using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;

namespace Enterprise.AuditDataServices.TransportBooking.Subscribers
{
	public abstract class BaseDtbMasterBookingReplicationSubscriber : ActualDataChangesAuditSubscriber
	{
		public BaseDtbMasterBookingReplicationSubscriber()
		{
			Factory = new BusinessObjectFactory();
		}

		public BaseDtbMasterBookingReplicationSubscriber(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => Table.TableName + " Master Booking Replication Change Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get
			{
				if (specificColumns == null)
				{
					specificColumns = OtherIncludedColumns.Concat(ReplicationColumns).Append(PKColumn).Append(IsMasterColumn).Append(MasterBookingVersionColumn).Append(LastEditAuditTimeUtcColumn);
				}
				return specificColumns;
			}
		}
		IEnumerable<SchemaColumn> specificColumns;

		public override bool IsRequired() => TransportRegistry.Instance.MasterBookingsEnabled.Value;

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[IsMasterColumn.Name] == DBNull.Value || Convert.ToInt32(row[IsMasterColumn.Name]) != 1)
			{
				row.Delete();
			}
		};

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var currentRowIndex = 0;
			while (currentRowIndex < changeTable.Rows.Count)
			{
				var currentRow = changeTable.Rows[currentRowIndex];

				if (IsChangeRowUpdate(currentRow))
				{
					PerformReplication(RowChangeType.Update, logger, currentRow);
				}
				else if (IsChangeRowInsert(currentRow))
				{
					PerformReplication(RowChangeType.Insert, logger, currentRow);
				}

				currentRowIndex++;
			}
		}

		protected abstract void PerformReplication(RowChangeType rowChangeType, ILogger logger, DataRow currentRow);

		protected void UpdateSubEntityFromRow(BusinessObject subEntity, DataRow changeRow, ZShort newMasterBookingVersion, bool checkSubEntityMasterBookingVersion)
		{
			if (!checkSubEntityMasterBookingVersion || ((ZShort)subEntity[MasterBookingVersionColumn] != newMasterBookingVersion))
			{
				foreach (var column in ReplicationColumns)
				{
					if (!subEntity[column].Equals(changeRow[column.Name]))
					{
						subEntity[column] = changeRow[column.Name];
					}
				}
				subEntity[MasterBookingVersionColumn] = newMasterBookingVersion;
			}
		}

		bool IsChangeRowUpdate(DataRow currentRow) =>
			currentRow != null &&
			currentRow[AuditFieldNames.OperationFieldName].GetDataRowValue<int>() == CdcOperationCodes.UpdateAfterWrapperFiltering;

		bool IsChangeRowInsert(DataRow currentRow) =>
			currentRow != null &&
			currentRow[AuditFieldNames.OperationFieldName].GetDataRowValue<int>() == CdcOperationCodes.Insert;

		protected void SaveToFactory()
		{
#if DEBUG
			if (ThrowTestErrorOnSaveToFactory)
			{
				if (OnlyThrowTestErrorOnSaveToFactoryOnce)
				{
					ThrowTestErrorOnSaveToFactory = false;
				}

				throw new Exception("Test Replication Error");
			}
#endif

			Factory.Save();
		}

#if DEBUG
		public bool ThrowTestErrorOnSaveToFactory { get; set; }
		public bool OnlyThrowTestErrorOnSaveToFactoryOnce { get; set; }
#endif

		protected abstract Type BizOType { get; }

		protected IEnumerable<SchemaColumn> ReplicationColumns => DtbMasterBookingReplication.GetReplicatedColumnsForTable(Table.TableName);

		protected abstract IEnumerable<SchemaColumn> OtherIncludedColumns { get; }

		protected SchemaColumn PKColumn => Table.PK;

		protected SchemaColumn LastEditAuditTimeUtcColumn
		{
			get
			{
				if (lastEditAuditColumn == null)
				{
					lastEditAuditColumn = Table.GetSchemaColumn(Table.PK.ColumnPrefix + "_SystemLastEditTimeUtc");
				}
				return lastEditAuditColumn;
			}
		}
		SchemaColumn lastEditAuditColumn;

		protected SchemaColumn IsMasterColumn
		{
			get
			{
				if (isMasterColumn == null)
				{
					isMasterColumn = Table.GetSchemaColumn(Table.PK.ColumnPrefix + "_IsMaster");
				}

				return isMasterColumn;
			}
		}
		SchemaColumn isMasterColumn;

		protected SchemaColumn MasterBookingVersionColumn
		{
			get
			{
				if (masterBookingVersionColumn == null)
				{
					masterBookingVersionColumn = Table.GetSchemaColumn(Table.PK.ColumnPrefix + "_MasterBookingVersion");
				}

				return masterBookingVersionColumn;
			}
		}
		SchemaColumn masterBookingVersionColumn;

		protected SchemaColumn SubToMasterLinkColumn
		{
			get
			{
				if (subToMasterLinkColumn == null)
				{
					var baseTablePrefix = Table.PK.ColumnPrefix;
					subToMasterLinkColumn = Table.GetSchemaColumn(baseTablePrefix + "_" + baseTablePrefix + "_Master" + EntityShortNameWithoutSpaces);
				}

				return subToMasterLinkColumn;
			}
		}
		SchemaColumn subToMasterLinkColumn;

		protected BusinessObject GetMasterEntity(ZGuid pk)
		{
			return Factory.Load(BizOType, pk);
		}

		protected abstract string GetChangeRowEntityDescription(DataRow changeRow, BusinessObject parentEntity);

		protected abstract string GetChangeRowEntityMultilingualDescription(DataRow changeRow, BusinessObject parentEntity);

		protected abstract string GetEntityDescription(BusinessObject entity);

		protected abstract string GetEntityMultilingualDescription(BusinessObject entity);

		protected string GetRowChangeTypeMultilingualString(RowChangeType rowChangeType)
		{
			switch (rowChangeType)
			{
				case RowChangeType.Update:
					return Res.GetString("7ac4ff87-8e23-4550-a21f-8eba2e787b4b", "Update");

				case RowChangeType.Insert:
					return Res.GetString("4e992778-912f-472a-8798-51cb43a12139", "Insert");

				default:
					return string.Empty;
			}
		}

		protected string EntityShortNameWithoutSpaces
		{
			get
			{
				if (entityShortNameWithoutSpaces == null)
				{
					entityShortNameWithoutSpaces = Table.TableName.Replace("Dtb", string.Empty);
				}
				return entityShortNameWithoutSpaces;
			}
		}
		string entityShortNameWithoutSpaces;

		protected string EntityShortName
		{
			get
			{
				if (entityShortName == null)
				{
					entityShortName = EntityShortNameWithoutSpaces.Replace("Booking", "Booking ").Trim();
				}
				return entityShortName;
			}
		}
		string entityShortName;

		protected string ErrorNoteDescription => Res.GetString("4eea635f-2813-4455-88a4-2e95c6aa216a", "Master Booking Replication Error");

		protected string GetErrorNoteMessageCore(string subAndMasterMessagePart, ZShort newMasterBookingVersion, Exception ex)
		{
			return Res.GetString("fbec51eb-990f-42e5-96c3-9c2aeb8361bd", "{0} on Master Booking Version {1} failed with exception {2}", subAndMasterMessagePart, newMasterBookingVersion, ex.Message);
		}

		protected string GetErrorNoteLogCore(string subAndMasterMessagePart, ZShort newMasterBookingVersion, Exception ex) =>
			FormattableString.Invariant($"{subAndMasterMessagePart} on MasterBookingVersion {newMasterBookingVersion} failed with exception {ex.Message}");

#if DEBUG
		internal
#endif
		protected BusinessObjectFactory Factory
		{ get; set; }

#if DEBUG
		internal BusinessObject ReloadEntityFromCurrentFactory(BusinessObject entity)
		{
			return Factory.Load(BizOType, entity.PK);
		}
#endif
		protected enum RowChangeType
		{
			Insert,
			Update,
		}
	}
}
