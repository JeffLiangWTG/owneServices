namespace Enterprise.AuditDataServices.Subscription.Common
{
	public static class ObjectExtensions
	{
		public static string ToStringSafe(this object source)
		{
			return source == null ? string.Empty : source.ToString();
		}

		public static T GetDataRowValue<T>(this object data)
		{
			var result = default(T);

			if (data is T tData)
			{
				result = tData;
			}

			return result;
		}
	}
}
