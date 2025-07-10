namespace Enterprise.DataTransfer.Native.Common
{
	public enum EntityAction
	{
		EMPTY, INSERT, UPDATE, MERGE, DELETE, IGNORE
	}

	public static class EventExtensions
	{
		public static string Code(this EntityAction action)
		{
			switch (action)
			{
				case EntityAction.INSERT:
					return "ADD";
				case EntityAction.UPDATE:
					return "EDT";
				case EntityAction.DELETE:
					return "DEL";
				default:
					return string.Empty;
			}
		}

		public static EntityAction ToEvent(this string action)
		{
			action = action.ToUpper();

			if (action == "INSERT" || action == "CREATE")
			{
				return EntityAction.INSERT;
			}
			if (action == "UPDATE")
			{
				return EntityAction.UPDATE;
			}
			if (action == "MERGE")
			{
				return EntityAction.MERGE;
			}
			if (action == "DELETE" || action == "REMOVE")
			{
				return EntityAction.DELETE;
			}
			if (action == "IGNORE")
			{
				return EntityAction.IGNORE;
			}
			if (string.IsNullOrEmpty(action))
			{
				return EntityAction.EMPTY;
			}
			throw new NativeXMLUserVisibleException("Invalid action specified: " + action);
		}
	}
}
