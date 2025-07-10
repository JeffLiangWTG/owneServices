namespace Enterprise.AlwaysOn.Setup
{
	public enum PreJoinLevel
	{
		Cannot_be_joined = 0,
		Full_restore_required = 1,
		Partial_restore_required = 2,
		Ready_to_be_joined = 3,
		Already_joined = 4,
	}

	public interface IDatabasePreJoinStatus
	{
		string Name { get; }
		decimal LastPrimaryBackupLsn { get; }
		bool SecondaryExists { get; }
		bool IsSecondaryRestoring { get; }
		sbyte SecondaryState { get; }
		string SecondaryStateDescription { get; }
		decimal SecondaryRedoLsn { get; }
		bool DbFilesMatch { get; }
		bool IsDataMovementSuspended { get; }

		PreJoinLevel JoinLevel { get; }
	}
}
