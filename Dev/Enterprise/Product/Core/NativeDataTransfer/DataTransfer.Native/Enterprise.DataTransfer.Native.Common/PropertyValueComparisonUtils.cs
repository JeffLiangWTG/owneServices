using CargoWise.Types;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses
{
	public static class PropertyValueComparisonUtils
	{
		public static bool StringValueHasChanges(IEntity entity, string propertyName, string comparisonValue)
		{
			var result = false;
			if (entity != null && entity.HasProperty(propertyName))
			{
				var propertyValue = entity.GetPropertyOrBlankString(propertyName);
				result = propertyValue != comparisonValue;
			}

			return result;
		}

		public static bool BoolValueHasChanges(IEntity entity, string propertyName, bool comparisonValue)
		{
			var result = false;
			if (entity != null && entity.HasProperty(propertyName))
			{
				var propertyValue = entity.GetPropertyOrBlankString(propertyName);
				if (bool.TryParse(propertyValue, out var boolValue))
				{
					result = !boolValue && comparisonValue;
				}
			}

			return result;
		}

		public static bool DateTimeValueHasChanges(IEntity entity, string propertyName, ZDateTime comparisonValue)
		{
			var result = false;
			if (entity != null && entity.HasProperty(propertyName))
			{
				var propertyValue = entity.GetPropertyOrBlankString(propertyName);
				if (ZDateTime.TryParseISO8601Date(propertyValue, out ZDateTime dateValue))
				{
					result = dateValue != comparisonValue;
				}
			}

			return result;
		}

		public static bool IsTrue(IEntity entity, string propertyName)
		{
			var result = false;
			if (entity != null && entity.HasProperty(propertyName))
			{
				var propertyValue = entity.GetPropertyOrBlankString(propertyName);
				if (bool.TryParse(propertyValue, out var boolValue))
				{
					result = boolValue;
				}
			}

			return result;
		}
	}
}
