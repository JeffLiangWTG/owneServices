using System;

namespace Enterprise.AlwaysOn.Setup
{
	public struct DatabaseAlwaysOnStatus
	{
		public DatabaseAlwaysOnStatus(string name, Guid groupId, bool hasFullBackup, bool isDataMovementSuspended)
		{
			Name = name;
			GroupId = groupId;
			HasFullBackup = hasFullBackup;
			RecoveryModelChanged = false;
			IsDataMovementSuspended = isDataMovementSuspended;
		}

		public override string ToString()
		{
			return Name;
		}

		public string Name { get; }

		public Guid GroupId { get; }

		public bool HasFullBackup { get; }

		public bool RecoveryModelChanged { get; set; }

		public bool IsDataMovementSuspended { get; }
	}
}
