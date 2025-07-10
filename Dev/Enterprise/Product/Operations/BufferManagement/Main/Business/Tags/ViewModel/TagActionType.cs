using System;

namespace Enterprise.BufferManagement.Business
{
	public enum TagActionType
	{
		Unknown = 0,
		AddTag,
		RemoveTag,
		ModifyTag,
	}

	public static class TagActionTypeExtensions
	{
		public static string ToCode(this TagActionType actionType)
		{
			switch (actionType)
			{
				case TagActionType.AddTag:
					return "ADD";

				case TagActionType.RemoveTag:
					return "DEL";

				case TagActionType.ModifyTag:
					return "MOD";

				default:
					throw new ArgumentException("Invalid TagActionType: " + actionType);
			}
		}
	}
}
