
namespace Enterprise.BusinessObjectGenerator
{
	class NamingProvider
	{
		public static string PluralizePropertyName(string propertyName)
		{
			string result;

			string lowerCasePropertyName = propertyName.ToLower();

			// weird exceptions (ie. should probably rename columns in the DB)
			if (propertyName.StartsWith("FK"))
			{
				result = propertyName.Insert(2, "s");
			}
			else if (propertyName.EndsWith("Curr"))
			{
				result = propertyName + "encies";
			}
			// general exceptions
			else if (lowerCasePropertyName.EndsWith("ch") || lowerCasePropertyName.EndsWith("ss"))
			{
				result = propertyName + "es";
			}
			else if (lowerCasePropertyName.EndsWith("y") && !propertyName.EndsWith("By"))
			{
				result = propertyName.Remove(propertyName.Length - 1, 1) + "ies";
			}
			else if (lowerCasePropertyName.EndsWith("child"))
			{
				result = propertyName + "ren";
			}
			else if (lowerCasePropertyName.EndsWith("s") || lowerCasePropertyName.EndsWith("staff"))
			{
				result = propertyName;
			}
			// standard case
			else
			{
				result = propertyName + "s";
			}

			return result;
		}
	}
}
