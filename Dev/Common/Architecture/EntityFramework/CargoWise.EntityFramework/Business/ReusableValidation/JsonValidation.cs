using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public static class JsonValidation
	{
		/**
		 * <summary>
		 * Checks whether the ZPropertyInfo holds a valid JSON string. If not, an error is added to the ZPropertyInfo.
		 * </summary>
		 * <param name="property">The property to validate</param>
		 */
		public static void ValidateJson(ZPropertyInfo property)
		{
			ZString jsonZString = (ZString)property.Value;
			string jsonString = jsonZString.ToString();
			try
			{
				JToken.Parse(jsonString);
			}
			catch (JsonReaderException)
			{
				property.AddError(Res.GetString("100e3e40-5134-4591-b553-1f70fa792ef8", "A valid JSON is required"));
			}
		}
	}
}
