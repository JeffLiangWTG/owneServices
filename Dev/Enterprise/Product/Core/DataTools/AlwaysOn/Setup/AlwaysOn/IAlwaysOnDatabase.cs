using System;

namespace Enterprise.AlwaysOn.Setup
{
	public interface IAlwaysOnDatabase
	{
		string Name { get; }
		string GroupName { get; }
		Guid GroupId { get; }
		void SetGroupInfo(string groupName, Guid groupId);
	}
}
