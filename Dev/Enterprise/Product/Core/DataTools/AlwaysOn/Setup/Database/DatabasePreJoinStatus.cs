using System;

namespace Enterprise.AlwaysOn.Setup
{
	public class DatabasePreJoinStatus : IDatabasePreJoinStatus
	{
		public DatabasePreJoinStatus(string name, DbFileAndTransactionLogInfo secondaryRedoInfo, DbFileAndTransactionLogInfo primaryLastBackupInfo, bool isDataMovementSuspended, sbyte secondaryState, string secondaryStateDescription, Guid groupId, Guid currentGroupId)
		{
			this.name = name;
			this.secondaryRedoInfo = secondaryRedoInfo;
			this.primaryLastBackupInfo = primaryLastBackupInfo;
			this.isDataMovementSuspended = isDataMovementSuspended;
			this.secondaryState = secondaryState;
			this.secondaryStateDescription = secondaryStateDescription;
			isPartOfCurrentGroup = (groupId != Guid.Empty && groupId == currentGroupId);
			isPartOfAnotherGroup = (groupId != Guid.Empty && groupId != currentGroupId);
		}

		string IDatabasePreJoinStatus.Name
		{
			get { return name; }
		}
		readonly string name;

		decimal IDatabasePreJoinStatus.LastPrimaryBackupLsn
		{
			get { return primaryLastBackupInfo?.LogSequenceNumber ?? -1m; }
		}

		bool IDatabasePreJoinStatus.SecondaryExists
		{
			get { return (secondaryRedoInfo != null); }
		}

		sbyte IDatabasePreJoinStatus.SecondaryState
		{
			get { return secondaryState; }
		}
		readonly sbyte secondaryState;

		string IDatabasePreJoinStatus.SecondaryStateDescription
		{
			get { return secondaryStateDescription; }
		}
		readonly string secondaryStateDescription;

		const int db_state_restoring = 1;

		bool IDatabasePreJoinStatus.IsSecondaryRestoring
		{
			get { return secondaryState == db_state_restoring; }
		}

		decimal IDatabasePreJoinStatus.SecondaryRedoLsn
		{
			get { return secondaryRedoInfo?.LogSequenceNumber ?? -1m; }
		}

		bool IDatabasePreJoinStatus.DbFilesMatch
		{
			get
			{
				if (dbFilesMatch == null)
				{
					dbFilesMatch = (primaryLastBackupInfo != null && secondaryRedoInfo != null && primaryLastBackupInfo.Files != null && primaryLastBackupInfo.Files.CompareTo(secondaryRedoInfo.Files) == 0);
				}

				return dbFilesMatch.Value;
			}
		}
		bool? dbFilesMatch;

		bool IDatabasePreJoinStatus.IsDataMovementSuspended { get { return isDataMovementSuspended; } }
		readonly bool isDataMovementSuspended;

		PreJoinLevel IDatabasePreJoinStatus.JoinLevel
		{
			get
			{
				if (joinLevel == null)
				{
					if (isPartOfCurrentGroup)
					{
						joinLevel = PreJoinLevel.Already_joined;
					}
					else if (primaryLastBackupInfo == null || isPartOfAnotherGroup)
					{
						joinLevel = PreJoinLevel.Cannot_be_joined;
					}
					else if (secondaryRedoInfo == null || !((IDatabasePreJoinStatus)this).IsSecondaryRestoring || !((IDatabasePreJoinStatus)this).DbFilesMatch)
					{
						joinLevel = PreJoinLevel.Full_restore_required;
					}
					else if (primaryLastBackupInfo.LogSequenceNumber != secondaryRedoInfo.LogSequenceNumber)
					{
						joinLevel = PreJoinLevel.Partial_restore_required;
					}
					else
					{
						joinLevel = PreJoinLevel.Ready_to_be_joined;
					}
				}

				return joinLevel.Value;
			}
		}
		PreJoinLevel? joinLevel;

		readonly DbFileAndTransactionLogInfo primaryLastBackupInfo;
		readonly DbFileAndTransactionLogInfo secondaryRedoInfo;
		readonly bool isPartOfCurrentGroup;
		readonly bool isPartOfAnotherGroup;
	}
}
