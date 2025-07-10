namespace Enterprise.AlwaysOn.Setup
{
	using System;
	using static System.FormattableString;

	class AlwaysOnDatabase : IAlwaysOnDatabase
	{
		public AlwaysOnDatabase(string name, string groupName, Guid groupId)
		{
			this.name = name;
			this.groupName = groupName;
			this.groupId = groupId;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Groupname string")]
		public override string ToString()
		{
			return name + (string.IsNullOrWhiteSpace(groupName) ? "" : " (group: " + groupName + ")");
		}

		string IAlwaysOnDatabase.Name
		{
			get { return name; }
		}
		readonly string name;

		string IAlwaysOnDatabase.GroupName
		{
			get { return groupName; }
		}
		string groupName;

		Guid IAlwaysOnDatabase.GroupId
		{
			get { return groupId; }
		}
		Guid groupId;

		void IAlwaysOnDatabase.SetGroupInfo(string groupName, Guid groupId)
		{
			if (this.groupId != Guid.Empty || !string.IsNullOrWhiteSpace(this.groupName))
			{
				throw new AlwaysOnException(Invariant($"Database [{name}] already has an availability group defined: ID [{this.groupName}], Name [{this.groupId}]."));
			}

			this.groupName = groupName;
			this.groupId = groupId;
		}
	}
}
