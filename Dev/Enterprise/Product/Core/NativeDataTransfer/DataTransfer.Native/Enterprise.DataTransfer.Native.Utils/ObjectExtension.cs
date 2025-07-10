using System;

namespace Enterprise.DataTransfer.Native.Utils
{
	public static class ObjectExtension
	{
		public static bool IsEmpty(this Object o)
		{
			if (o == null)
			{
				return true;
			}

			if (o is Guid)
			{
				return (Guid)o == Guid.Empty;
			}

			if (o is String)
			{
				return String.IsNullOrEmpty(o.ToString().Trim());
			}

			if (o is DBNull)
			{
				return true;
			}

			return o.ToString().Trim().IsEmpty();
		}
	}
}