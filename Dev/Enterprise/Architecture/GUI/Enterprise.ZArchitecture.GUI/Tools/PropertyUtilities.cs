namespace Enterprise.ZArchitecture.GUI
{
	public static class PropertyUtilities
	{
		public static string GetPropertyName(string propertyPath)
		{
			var index = propertyPath.LastIndexOfAny(new[] { '.', '+' });
			return index != -1 ? propertyPath.Substring(index + 1) : propertyPath;
		}
	}
}
