using System;

namespace Enterprise.AlwaysOn.Setup
{
	public static class AlwaysOnDatabaseFactory
	{
		public static IAlwaysOnDatabase New(string name, string groupName, Guid groupId)
		{
			var result = new AlwaysOnDatabase(name, groupName, groupId);
			return result;
		}
	}
}
